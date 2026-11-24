using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
#if UNITY_ANDROID && !UNITY_EDITOR
using UnityEngine.Android;
#endif

namespace ProjectSpark.SparkV5.Pro
{
    public enum SparkVoiceInputState { Off, Starting, Listening, Recognizing, Paused, Error }
    // Explicit opt-in, bounded audio, no file writes. VAD is energy based; no acoustic echo cancellation.
    [DisallowMultipleComponent]
    public sealed class SparkProVoiceInput : MonoBehaviour
    {
        [SerializeField] string endpoint="http://127.0.0.1:8766/transcribe";
        [SerializeField] bool allowInsecureLanDevelopment;
        [SerializeField] string microphoneDevice="";
        [SerializeField,Range(.005f,.15f)] float threshold=.025f;
        [SerializeField,Range(.3f,1.5f)] float endSilence=.65f;
        [SerializeField,Range(2,20)] int maxSeconds=12;
        [SerializeField] bool requireWakePhrase;
        [SerializeField] string wakePhrase="hey spark";
        [SerializeField] SparkProCharacter character;
        public SparkVoiceInputState State {get;private set;}
        public float InputLevel {get;private set;}
        public string Status {get;private set;}="Microphone off";
        public bool ContinuousRequested {get;private set;}
        public string ProviderStatus { get; private set; } = "Speech connection unchecked";
        public string Device => microphoneDevice;
        public float Sensitivity => threshold;
        public float EndSilence => endSilence;
        public bool WakePhraseRequired => requireWakePhrase;
        public bool UsesLoopback => Uri.TryCreate(endpoint, UriKind.Absolute, out var url) && url.IsLoopback;
        public event Action<string> Transcript;
        public event Action<string> StatusChanged;
        public event Action<string> ProviderStatusChanged;
        public event Action InterruptRequested;
        public Func<bool> CanListen;
        const int RequestedRate=16000;
        readonly List<short> samples=new List<short>(RequestedRate*20);
        readonly Queue<short> preRoll=new Queue<short>(RequestedRate/4);
        AudioClip mic; string activeDevice;float[] frame;int lastPosition,frequency,channels,frameSamples,generation;
        float lastRead,startedAt,silence,voiced;bool detected,continuous,starting;Coroutine send,start;UnityWebRequest request;
        [SerializeField] private bool startLocalSpeechService = true;
        private readonly SparkProSpeechProcess speechProcess = new();
        public float? LastRecognitionConfidence { get; private set; }
#if UNITY_ANDROID && !UNITY_EDITOR
        private readonly ProjectSpark.AI.SparkAndroidRecognition android = new();
        private bool nativeCapture;
        private float nativeDeadline;
#endif
        Coroutine healthMonitor;
        UnityWebRequest healthRequest;
        CancellationTokenSource healthCancellation, recognitionCancellation;
        private static readonly Lazy<HttpClient> LoopbackClient = new Lazy<HttpClient>(() =>
            new HttpClient(new HttpClientHandler { AllowAutoRedirect = false, UseProxy = false })
            {
                MaxResponseContentBufferSize = 32768
            });

        private static async Task<(string Body, long Code)> SendLoopback(Uri uri, byte[] wav, CancellationToken token)
        {
            if (!uri.IsLoopback || uri.Scheme != Uri.UriSchemeHttp)
                throw new InvalidOperationException("The local speech transport requires an HTTP loopback endpoint.");
            try
            {
                using var message = new HttpRequestMessage(wav == null ? HttpMethod.Get : HttpMethod.Post, uri);
                if (wav != null)
                {
                    message.Content = new ByteArrayContent(wav);
                    message.Content.Headers.ContentType = new MediaTypeHeaderValue("audio/wav");
                }
                using var response = await LoopbackClient.Value.SendAsync(message, token).ConfigureAwait(false);
                return (await response.Content.ReadAsStringAsync().ConfigureAwait(false), (long)response.StatusCode);
            }
            catch (HttpRequestException) { return (null, 0); }
            catch (OperationCanceledException) { return (null, 0); }
        }

        private static void Cancel(ref CancellationTokenSource source)
        {
            var active = source;
            source = null;
            if (active != null)
            {
                active.Cancel();
                active.Dispose();
            }
        }

        private const string SettingsKey = "ProjectSpark.V5.Voice.v1.";

        private void Awake()
        {
            microphoneDevice = PlayerPrefs.GetString(SettingsKey + "Device", microphoneDevice);
            threshold = Mathf.Clamp(PlayerPrefs.GetFloat(SettingsKey + "Threshold", threshold), .005f, .15f);
            endSilence = Mathf.Clamp(PlayerPrefs.GetFloat(SettingsKey + "Silence", endSilence), .3f, 1.5f);
            requireWakePhrase = PlayerPrefs.GetInt(SettingsKey + "Wake", requireWakePhrase ? 1 : 0) == 1;
        }

        public void Configure(SparkProCharacter target){character=target;}

        public void SetDevice(string value)
        {
            if (microphoneDevice == value)
                return;
            Stop();
            microphoneDevice = value ?? string.Empty;
            PlayerPrefs.SetString(SettingsKey + "Device", microphoneDevice);
        }

        public void SetSensitivity(float value)
        {
            threshold = Mathf.Clamp(value, .005f, .15f);
            PlayerPrefs.SetFloat(SettingsKey + "Threshold", threshold);
        }

        public void SetEndSilence(float value)
        {
            endSilence = Mathf.Clamp(value, .3f, 1.5f);
            PlayerPrefs.SetFloat(SettingsKey + "Silence", endSilence);
        }

        public void SetWakePhrase(bool value)
        {
            requireWakePhrase = value;
            PlayerPrefs.SetInt(SettingsKey + "Wake", value ? 1 : 0);
        }

        public void SetHealthMonitoring(bool enabled)
        {
            if (enabled && isActiveAndEnabled)
            {
                if (healthMonitor == null)
                    healthMonitor = StartCoroutine(MonitorHealth());
            }
            else
            {
                Cancel(ref healthCancellation);
                if (healthRequest != null)
                {
                    healthRequest.Abort();
                    healthRequest.Dispose();
                    healthRequest = null;
                }
                if (healthMonitor != null)
                    StopCoroutine(healthMonitor);
                healthMonitor = null;
            }
        }

        private void ReportProviderStatus(string status)
        {
            if (ProviderStatus == status) return;
            ProviderStatus = status;
            ProviderStatusChanged?.Invoke(status);
        }

        [Serializable] sealed class HealthReply { public bool ready; }

        IEnumerator MonitorHealth()
        {
            if (!ValidEndpoint() || !Uri.TryCreate(endpoint, UriKind.Absolute, out var uri))
            {
                ReportProviderStatus("Invalid speech endpoint");
                yield return null;
                healthMonitor = null;
                yield break;
            }

#if UNITY_ANDROID && !UNITY_EDITOR
            if (uri.IsLoopback)
            {
                ReportProviderStatus(ProjectSpark.AI.SparkAndroidRecognition.Available() ?
                    "Android on-device speech available" : "On-device speech unavailable. Configure an HTTPS provider or type.");
                yield return null;
                healthMonitor = null;
                yield break;
            }
#endif
            var healthUri = new UriBuilder(uri) { Path = "/health", Query = "", Fragment = "" }.Uri;
            while (true)
            {
                string body = null;
                if (healthUri.IsLoopback && healthUri.Scheme == Uri.UriSchemeHttp)
                {
                    healthCancellation = new CancellationTokenSource(TimeSpan.FromSeconds(3));
                    var check = SendLoopback(healthUri, null, healthCancellation.Token);
                    while (!check.IsCompleted) yield return null;
                    if (check.Status == TaskStatus.RanToCompletion && check.Result.Code == 200)
                        body = check.Result.Body;
                    healthCancellation.Dispose();
                    healthCancellation = null;
                }
                else
                {
                    using var check = UnityWebRequest.Get(healthUri);
                    healthRequest = check;
                    check.timeout = 3;
                    check.redirectLimit = 0;
                    yield return check.SendWebRequest();
                    if (check.result == UnityWebRequest.Result.Success)
                        body = check.downloadHandler.text;
                    healthRequest = null;
                }
                bool ready = false;
                if (!string.IsNullOrWhiteSpace(body))
                {
                    try { ready = JsonUtility.FromJson<HealthReply>(body)?.ready == true; }
                    catch (ArgumentException) { }
                }
                if (startLocalSpeechService) speechProcess.OnHealth(healthUri, ready, Time.realtimeSinceStartup);
                string diagnostic = healthUri.Port == 8766 ? speechProcess.Diagnostic : "";
                ReportProviderStatus(ready ? "Speech server ready" : string.IsNullOrEmpty(diagnostic)
                    ? "Speech server unavailable. You can still type." : diagnostic + ". You can still type.");
                yield return new WaitForSecondsRealtime(5);
            }
        }

        public void YieldForResponse()
        {
            if (ContinuousRequested) Pause();
            else if (State != SparkVoiceInputState.Off) Stop();
        }
        public void BeginPushToTalk(){ContinuousRequested=false;InterruptRequested?.Invoke();if(CanListen!=null&&!CanListen()){Set(SparkVoiceInputState.Off,"Voice input is blocked by the current state.");return;}Begin(false);}
        public void EndPushToTalk()
        {
            if (continuous) return;
            if (starting) { Stop(); return; }
#if UNITY_ANDROID && !UNITY_EDITOR
            if (nativeCapture)
            {
                android.End();
                Set(SparkVoiceInputState.Recognizing, "Recognizing speech on device");
                return;
            }
#endif
            if (mic) Finish();
        }
        public void StartHandsFree(){ContinuousRequested=true;if(CanListen==null||CanListen())Begin(true);else Set(SparkVoiceInputState.Paused,"Waiting for Spark to finish speaking");}
        public void StopHandsFree(){Stop();}
        public void Mute(){Stop();}
        public void Stop(){ContinuousRequested=false;CancelCaptureAndRequest();Set(SparkVoiceInputState.Off,"Microphone off");}
        public void Pause(){CancelCaptureAndRequest();Set(SparkVoiceInputState.Paused,"Microphone paused");}
        void OnDisable(){SetHealthMonitoring(false);Stop();speechProcess.Dispose();}
        void OnApplicationPause(bool paused)
        {
            if (paused && State != SparkVoiceInputState.Starting) Stop();
        }
        void OnApplicationFocus(bool hasFocus){if(!hasFocus&&State!=SparkVoiceInputState.Starting)Stop();}
        void Set(SparkVoiceInputState state,string message){State=state;Status=message;StatusChanged?.Invoke(message);}
        bool ValidEndpoint(){if(!Uri.TryCreate(endpoint,UriKind.Absolute,out var u))return false;return u.Scheme=="https"||(u.Scheme=="http"&&(u.IsLoopback||(Debug.isDebugBuild&&allowInsecureLanDevelopment)));}
        void Begin(bool auto)
        {
            CancelCaptureAndRequest();continuous=auto;if(!ValidEndpoint()){ContinuousRequested=false;Set(SparkVoiceInputState.Error,"Recognition endpoint must use HTTPS or permitted development HTTP.");return;}
            int id=generation;starting=true;start=StartCoroutine(Open(id));
        }
        IEnumerator Open(int id)
        {
            Set(SparkVoiceInputState.Starting,"Requesting microphone access");
#if UNITY_ANDROID && !UNITY_EDITOR
            if(!Permission.HasUserAuthorizedPermission(Permission.Microphone)){
                Permission.RequestUserPermission(Permission.Microphone);float deadline=Time.realtimeSinceStartup+15;
                while(!Permission.HasUserAuthorizedPermission(Permission.Microphone)&&Time.realtimeSinceStartup<deadline&&id==generation)yield return null;
                if(!Permission.HasUserAuthorizedPermission(Permission.Microphone)){Fail("Microphone permission denied.");yield break;}
            }
#else
            if(!Application.HasUserAuthorization(UserAuthorization.Microphone))yield return Application.RequestUserAuthorization(UserAuthorization.Microphone);
            if(!Application.HasUserAuthorization(UserAuthorization.Microphone)){Fail("Microphone permission denied.");yield break;}
#endif
            while (id == generation && !Application.isFocused)
                yield return null;
            if(id!=generation)yield break;
#if UNITY_ANDROID && !UNITY_EDITOR
            if (UsesLoopback)
            {
                if (!ProjectSpark.AI.SparkAndroidRecognition.Available()) { Fail("On-device speech unavailable. Configure an HTTPS provider or type."); yield break; }
                nativeCapture = true;
                nativeDeadline = Time.realtimeSinceStartup + maxSeconds + 4f;
                starting = false;
                start = null;
                Set(SparkVoiceInputState.Listening, "Listening on device");
                android.Start((text, confidence) =>
                {
                    if (id != generation) return;
                    nativeCapture = false;
                    android.Stop();
                    LastRecognitionConfidence = confidence >= 0f ? confidence : null;
                    Deliver(LastRecognitionConfidence.HasValue && confidence < .57f ? "" : text);
                }, () =>
                {
                    if (id == generation) { nativeCapture = false; android.Stop(); Fail("On-device recognition failed."); }
                });
                yield break;
            }
#endif
            if(Microphone.devices.Length==0){Fail("No microphone found.");yield break;}
            activeDevice = string.IsNullOrWhiteSpace(microphoneDevice) || Array.IndexOf(Microphone.devices, microphoneDevice) < 0
                ? null : microphoneDevice;
            string openError=null;try{mic=Microphone.Start(activeDevice,true,2,RequestedRate);}catch(Exception ex){openError=ex.Message;}
            if(openError!=null){Fail("Microphone error: "+openError);yield break;}
            if(!mic){Fail("Could not start microphone.");yield break;}
            float until=Time.realtimeSinceStartup+3;
            while(Microphone.GetPosition(activeDevice)<=0&&Time.realtimeSinceStartup<until&&id==generation)yield return null;
            if(id!=generation)yield break;
            if(Microphone.GetPosition(activeDevice)<=0){Fail("Microphone startup timeout.");yield break;}
            frequency=mic.frequency;channels=mic.channels;if(frequency<8000||frequency>48000||channels<=0){Fail("Unsupported microphone format.");yield break;}frameSamples=Mathf.Max(1,frequency/50);frame=new float[frameSamples*channels];lastPosition=0;lastRead=startedAt=Time.realtimeSinceStartup;
            samples.Clear();preRoll.Clear();silence=voiced=0;detected=false;starting=false;start=null;Set(SparkVoiceInputState.Listening,continuous?"Listening for speech":"Recording — release to send");
            if(character)character.SetState(SparkState.Listening);
        }
        void Update()
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            android.Update();
            if (nativeCapture && Time.realtimeSinceStartup > nativeDeadline)
            {
                CancelCaptureAndRequest();
                Fail("On-device speech timed out.");
            }
#endif
            if(ContinuousRequested&&State==SparkVoiceInputState.Paused&&(CanListen==null||CanListen())){Begin(true);return;}
            if(State!=SparkVoiceInputState.Listening||!mic)return;
            if(continuous&&CanListen!=null&&!CanListen()){Pause();return;}
            float now=Time.realtimeSinceStartup;if(now-lastRead>1.8f){Fail("Microphone buffer overrun.");return;}lastRead=now;
            int write=Microphone.GetPosition(activeDevice);if(write<0){Fail("Microphone disconnected.");return;}
            int available=(write-lastPosition+mic.samples)%mic.samples;
            while(available>=frameSamples){if(!mic.GetData(frame,lastPosition)){Fail("Microphone read failed.");return;}lastPosition=(lastPosition+frameSamples)%mic.samples;available-=frameSamples;
                float energy=0;for(int i=0;i<frameSamples;i++){float sum=0;for(int c=0;c<channels;c++)sum+=frame[i*channels+c];float v=sum/channels;energy+=v*v;frame[i]=v;}
                InputLevel=Mathf.Sqrt(energy/frameSamples);bool loud=InputLevel>=threshold;float span=(float)frameSamples/frequency;
                if(continuous&&!detected){
                    for(int i=0;i<frameSamples;i++){preRoll.Enqueue(ToPcm(frame[i]));while(preRoll.Count>frequency/4)preRoll.Dequeue();}
                    if(loud)voiced+=span;else voiced=Mathf.Max(0,voiced-span);
                    if(voiced>=.10f){detected=true;while(preRoll.Count>0)samples.Add(preRoll.Dequeue());silence=0;}
                }else{
                    for(int i=0;i<frameSamples;i++)samples.Add(ToPcm(frame[i]));if(loud){voiced+=span;silence=0;}else silence+=span;
                }
                if(samples.Count>=frequency*maxSeconds||(continuous&&detected&&silence>=endSilence)){Finish();return;}
            }
            if(!continuous&&now-startedAt>=maxSeconds+.5f)Finish();
        }
        static short ToPcm(float value)=>(short)Mathf.RoundToInt(Mathf.Clamp(value,-1,1)*32767);
        void Finish(){StopMic();if(samples.Count<frequency/5||voiced<.08f){samples.Clear();if(continuous&&ContinuousRequested)Set(SparkVoiceInputState.Paused,"No speech detected");else Set(SparkVoiceInputState.Off,"No speech detected");return;}
            byte[] wav=MakeWave(samples,frequency);samples.Clear();int id=generation;Set(SparkVoiceInputState.Recognizing,"Recognizing speech");if(character)character.SetState(SparkState.Thinking);send=StartCoroutine(Recognize(wav,id));}
        [Serializable] sealed class Reply
        {
            public string text, error;
            public bool confidence_available;
            public float confidence;
        }
        IEnumerator Recognize(byte[] wav,int id)
        {
            LastRecognitionConfidence = null;
            bool uncertain = false;
            string value=null,problem=null;
            string response = null;
            long code = 0;
            var uri = new Uri(endpoint);
            if (uri.IsLoopback && uri.Scheme == Uri.UriSchemeHttp)
            {
                recognitionCancellation = new CancellationTokenSource(TimeSpan.FromSeconds(15));
                var post = SendLoopback(uri, wav, recognitionCancellation.Token);
                while (!post.IsCompleted) yield return null;
                if (id != generation) yield break;
                if (post.Status == TaskStatus.RanToCompletion)
                {
                    response = post.Result.Body;
                    code = post.Result.Code;
                }
                recognitionCancellation.Dispose();
                recognitionCancellation = null;
            }
            else
            {
                using var req = new UnityWebRequest(endpoint, "POST");
                request = req;
                req.uploadHandler = new UploadHandlerRaw(wav);
                req.downloadHandler = new DownloadHandlerBuffer();
                req.SetRequestHeader("Content-Type", "audio/wav");
                req.timeout = 15;
                req.redirectLimit = 0;
                yield return req.SendWebRequest();
                if (id != generation) yield break;
                code = req.responseCode;
                response = req.downloadHandler.text;
                request = null;
            }
            if (code == 429)
                problem = "The speech recognizer is busy. Try again or type a question.";
            else if (code != 200 || string.IsNullOrWhiteSpace(response))
                problem = "Recognition unavailable. Check the speech server or type a question.";
            else
            {
                try
                {
                    var reply = JsonUtility.FromJson<Reply>(response);
                    value = reply?.text;
                    problem = reply?.error;
                    if (reply?.confidence_available == true && reply.confidence >= 0f && reply.confidence <= 1f)
                    {
                        LastRecognitionConfidence = reply.confidence;
                        uncertain = reply.confidence < .57f && !string.IsNullOrWhiteSpace(value);
                    }
                }
                catch (ArgumentException)
                {
                    problem = "The speech provider returned an invalid reply.";
                }
            }
            send=null;if(!string.IsNullOrEmpty(problem)){Fail(problem);yield break;}
            if (uncertain)
            {
                Set(ContinuousRequested ? SparkVoiceInputState.Paused : SparkVoiceInputState.Off,
                    "Speech was unclear. Repeat your question or type it.");
                yield break;
            }
            Deliver(value);
        }
        private void Deliver(string value)
        {
            value=(value??"").Trim();if(continuous&&requireWakePhrase){string prefix=(wakePhrase??"").Trim();if(prefix.Length>0){if(value.StartsWith(prefix,StringComparison.OrdinalIgnoreCase)&&(value.Length==prefix.Length||char.IsWhiteSpace(value[prefix.Length])||char.IsPunctuation(value[prefix.Length])))value=value.Substring(prefix.Length).Trim(' ', ',', '.', ':');else value="";}}
            Set(ContinuousRequested?SparkVoiceInputState.Paused:SparkVoiceInputState.Off,"Microphone off — "+(value.Length>0?"turn recognized":"no recognized words"));
            if(value.Length>0){if(value.Length>1000)value=value.Substring(0,1000);Transcript?.Invoke(value);}else if(character)character.SetState(SparkState.Idle);
        }
        void StopMic(){if(mic){Microphone.End(activeDevice);Destroy(mic);mic=null;}InputLevel=0;}
        void CancelCaptureAndRequest(){
#if UNITY_ANDROID && !UNITY_EDITOR
            if (nativeCapture) { nativeCapture = false; android.Stop(); }
#endif
            generation++;starting=false;Cancel(ref recognitionCancellation);if(start!=null){StopCoroutine(start);start=null;}if(request!=null)request.Abort();if(send!=null){StopCoroutine(send);send=null;}request?.Dispose();request=null;StopMic();samples.Clear();preRoll.Clear();}
        void Fail(string why){ContinuousRequested=false;starting=false;start=null;StopMic();samples.Clear();Set(SparkVoiceInputState.Error,why);if(character)character.SetState(SparkState.Idle);}
        public static byte[] MakeWave(IList<short> pcm,int sampleRate){using(var ms=new MemoryStream(44+pcm.Count*2))using(var w=new BinaryWriter(ms)){w.Write(new[]{'R','I','F','F'});w.Write(36+pcm.Count*2);w.Write(new[]{'W','A','V','E','f','m','t',' '});w.Write(16);w.Write((short)1);w.Write((short)1);w.Write(sampleRate);w.Write(sampleRate*2);w.Write((short)2);w.Write((short)16);w.Write(new[]{'d','a','t','a'});w.Write(pcm.Count*2);foreach(short s in pcm)w.Write(s);return ms.ToArray();}}
    }
}
