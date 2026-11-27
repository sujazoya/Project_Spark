using System;
using System.Collections.Generic;
using System.Threading;
using System.Text.RegularExpressions;
using UnityEngine;
using ProjectSpark.AI;
using ProjectSpark.AI.Tutorial;
using ProjectSpark.Gameplay;
using ProjectSpark.Circuit;
using ProjectSpark.UI.VisualTeaching;
namespace ProjectSpark.SparkV5.Pro
{
    // Adapter to the actual AI.zip APIs. All gameplay/electrical authority remains in existing systems.
    [DisallowMultipleComponent]
    public sealed class SparkProExecutive : MonoBehaviour
    {
        [Header("Existing systems (assign explicitly)")]
        [SerializeField] SparkAIUnifiedAssistant assistant;
        [SerializeField] SparkAIBrain world;
        [SerializeField] SparkAIVoiceController voice;
        [SerializeField] SparkAITTSBridge tts;
        [SerializeField] AudioSource playbackSource;
        [SerializeField] SparkSelectionController selection;
        [SerializeField] SparkAIKnowledge knowledge;
        [Header("New systems")]
        [SerializeField] SparkProCharacter character;
        [SerializeField] SparkProVoiceInput microphone;
        [SerializeField] SparkProAcademy academy;
        [SerializeField] SparkProHologram hologram;
        [SerializeField] SparkProCloudExplainer cloud;
        [SerializeField] SparkProGameCatalog catalog;
        [SerializeField] Transform playerFocus;
        [SerializeField] bool speakAnswers=true;
        [SerializeField] bool speakObservationWarnings;
        [SerializeField] bool examinationMode;
        [SerializeField,Range(3,45)] float speechStartupTimeout=20;
        public readonly struct Turn
        {
            public readonly string Speaker;
            public readonly string Text;
            public Turn(string speaker, string text) { Speaker = speaker; Text = text; }
        }

        const int HistoryLimit = 24;
        readonly List<Turn> history = new List<Turn>(HistoryLimit);
        private readonly SparkCortex cortex = new();
        internal SparkCortex Cortex => cortex;
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        public string CortexStatus => $"{ExplanationProvider} | {cortex.Intent} | {cortex.LatencyMs:F1} ms | request {cortex.Revision} | {CharacterState}\n" +
            $"Lesson: {cortex.ActiveLesson}\n" + string.Join("\n", System.Linq.Enumerable.Select(cortex.Trace,
                evidence => $"{evidence.Tool} · {evidence.Claim} · {evidence.Source} · topology {evidence.Topology} · solve {evidence.Solve} · {evidence.Limit}"));
#endif
        internal string ExplanationProvider => cloud && cloud.Available ? cloud.ProviderName : "Offline";
        private SparkLevelLayout priorLayout;
        private CancellationTokenSource semanticTurn;
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        public bool SemanticEnabled { get; set; }
        public bool SemanticCloudConsent { get; set; }
        public string SemanticStatus { get; private set; } = "Off";
#endif
        private bool SemanticBusy => semanticTurn != null;
        private SparkGesture segmentGesture;
        private int lastGestureSegment;
        private float nextProactiveAt;
        private bool proactive;
        private const string ProactiveKey = "ProjectSpark.Cortex.Proactive";
        LevelGamePlayManager levels;
        SparkAILearningPathAssessment assessment;
        public IReadOnlyList<Turn> History => history;
        public SparkProVoiceInput Microphone => microphone;
        public SparkProHologram Hologram => hologram;
        public SparkState CharacterState => character ? character.State : SparkState.Idle;
        public bool PlaybackActive => playbackSource && playbackSource.isPlaying || tts && tts.NativeSpeaking;
        public bool IsProcessing => submitting || awaitingAudio || SemanticBusy || (cloud && cloud.Busy);
        public bool SpeechOutputAvailable => voice && tts && tts.IsProviderAvailable;
        public bool SpeakAnswers => speakAnswers;
        public string LastTranscript { get; private set; } = string.Empty;
        public event Action ConversationChanged;
        internal event Action NexusTick;
        public event Action VisualOpened;
        public event Action<string> AnswerReady;
        public event Action<string> TranscriptReady;
        public string LastAnswer {get;private set;}="";
        public LevelGamePlayManager ActiveLevelManager => levels;
        public SparkSelectionController ActiveSelectionController => selection;
        public bool ExamMode => examinationMode || (assessment && assessment.RequiresIndependentWork);
        public void BindSession(LevelGamePlayManager manager, SparkAILearningPathAssessment examiner)
        {
            levels = manager;
            assessment = examiner;
            if (assistant)
                assistant.SetIndependentWorkGuard(() => ExamMode);
        }

        public void MuteMicrophone()
        {
            CancelSemantic();
            if (microphone) microphone.Mute();
        }

        public void SetSpeakAnswers(bool enabled)
        {
            speakAnswers = enabled;
            if (!enabled)
                StopSpeech();
        }

        public float SampleVoice(float[] samples)
        {
            if (!playbackSource || !playbackSource.isPlaying || samples == null || samples.Length == 0)
                return 0;
            playbackSource.GetOutputData(samples, 0);
            float sum = 0;
            foreach (float sample in samples) sum += sample * sample;
            return Mathf.Sqrt(sum / samples.Length);
        }

        private void Append(string speaker, string value)
        {
            if (history.Count >= HistoryLimit)
                history.RemoveAt(0);
            history.Add(new Turn(speaker, value.Length > 5500 ? value.Substring(0, 5500) : value));
            ConversationChanged?.Invoke();
        }

        public void ClearConversation()
        {
            EndLiveSession();
            if (cloud) cloud.ClearConversation();
            cortex.Reset(true);
            LastAnswer = LastTranscript = string.Empty;
            history.Clear();
            ConversationChanged?.Invoke();
        }

        public void EndLiveSession()
        {
            CancelSemantic();
            cortex.Cancel();
            if (microphone) microphone.Stop();
            if (cloud) cloud.Cancel();
            if (assistant) assistant.CancelPendingQuestionPro();
            StopSpeech();
            if (character)
            {
                character.ClearGesture();
                character.SetState(SparkState.Idle);
            }
        }
        bool warning,awaitingAudio,wasPlaying,submitting,previousCompleted;string previousLevel;int previousSelection=int.MinValue;float audioUntil;string utterance="";AudioClip previousClip;
        List<SparkVisemeScheduler.Cue> cues;SparkState returnState=SparkState.Idle;
        void OnEnable(){
            proactive = PlayerPrefs.GetInt(ProactiveKey, 0) == 1;
            if(!assistant)Debug.LogError("[Spark Executive] Assign SparkAIUnifiedAssistant.",this);
            if(character){character.BindVoice(playbackSource);character.InterruptRequested+=Interrupt;}
            if(microphone){microphone.Transcript+=OnTranscript;microphone.InterruptRequested+=InterruptForPushToTalk;microphone.StatusChanged+=OnMicrophoneState;microphone.CanListen=CanListen;}
            if(voice){voice.VoiceRequestCreated+=OnVoiceRequest;voice.VoiceStopped+=OnVoiceStopped;}
            if(world)world.WorldObserved+=OnWorld;
            // One presentation path for voice requests. Existing UI may still speak through its own settings.
            if(assistant){assistant.SetIndependentWorkGuard(() => ExamMode);assistant.SetExecutiveQuestionRouter(Ask);}
        }
        void OnDisable(){CancelSemantic();if(cloud)cloud.ClearConversation();if(character)character.InterruptRequested-=Interrupt;if(microphone){microphone.Transcript-=OnTranscript;microphone.InterruptRequested-=InterruptForPushToTalk;microphone.StatusChanged-=OnMicrophoneState;microphone.CanListen=null;microphone.Stop();}if(world)world.WorldObserved-=OnWorld;if(assistant){assistant.SetExecutiveQuestionRouter(null);assistant.SetIndependentWorkGuard(null);}if(voice){voice.VoiceRequestCreated-=OnVoiceRequest;voice.VoiceStopped-=OnVoiceStopped;}StopSpeech();}
        bool CanListen()=>!SemanticBusy&&!warning&&!awaitingAudio&&!PlaybackActive&&!submitting&&!(cloud&&cloud.Busy);
        public void SetExaminationMode(bool value){examinationMode=value;if(value)CancelSemantic();if(value&&cloud)cloud.Cancel();if(value&&academy&&academy.IsPracticing)academy.Cancel();}
        void OnTranscript(string text)
        {
            LastTranscript = text;
            TranscriptReady?.Invoke(text);
            Ask(text);
        }

        private void OnMicrophoneState(string _)
        {
            if (!character || warning || PlaybackActive || IsProcessing)
                return;
            if (microphone.State == SparkVoiceInputState.Listening)
                character.SetState(SparkState.Listening);
            else if (microphone.State == SparkVoiceInputState.Recognizing)
                character.SetState(SparkState.Thinking);
            else if (microphone.State != SparkVoiceInputState.Starting)
                character.SetState(microphone.ContinuousRequested ? SparkState.Waiting : SparkState.Idle);
        }

        private void InterruptForPushToTalk() => InterruptCore(false);
#if UNITY_ANDROID && !UNITY_EDITOR
        private void OnApplicationPause(bool paused)
        {
            if (paused) InterruptCore(false);
        }
#endif
        public void Interrupt() => InterruptCore(true);

        private void InterruptCore(bool cancelCapture)
        {
            CancelSemantic();
            cortex.Cancel();
            if (cancelCapture && microphone) microphone.YieldForResponse();
            if (cloud) cloud.Cancel();
            if (assistant) assistant.CancelPendingQuestionPro();
            StopSpeech();
            if (character)
            {
                character.ClearGesture();
                character.SetState(SparkState.Waiting);
            }
        }
        void StopSpeech(){if(voice)voice.StopVoice();if(tts)tts.Stop();if(playbackSource)playbackSource.Stop();awaitingAudio=false;wasPlaying=false;cues=null;if(character)character.ClearViseme();}
        void OnVoiceStopped(){awaitingAudio=false;wasPlaying=false;cues=null;if(character)character.ClearViseme();}
        void OnVoiceRequest(SparkAIVoiceRequest request){if(!request.IsValid || !SpeechOutputAvailable)return;lastGestureSegment=0;utterance=request.Text;awaitingAudio=true;audioUntil=Time.unscaledTime+speechStartupTimeout;previousClip=null;cues=null;wasPlaying=false;if(character)character.ClearViseme();}
        void Update(){
            NexusTick?.Invoke();
            bool native = tts && tts.NativeSpeaking;
            if (native)
            {
                awaitingAudio = false;
                wasPlaying = true;
                if (character) character.SetState(SparkState.Speaking);
                return;
            }
            bool playing=playbackSource&&playbackSource.isPlaying;
            if(playing){awaitingAudio=false;if(!wasPlaying||previousClip!=playbackSource.clip){previousClip=playbackSource.clip;cues=previousClip?SparkVisemeScheduler.Build(utterance,previousClip.length):null;}if(character&&cues!=null&&cues.Count>0)character.SetViseme(Map(SparkVisemeScheduler.At(cues,playbackSource.time)));wasPlaying=true;
                if (character && segmentGesture != SparkGesture.None && previousClip && previousClip.length > 3f)
                {
                    int segment = Mathf.FloorToInt(3 * playbackSource.time / previousClip.length);
                    if (segment > lastGestureSegment)
                    {
                        lastGestureSegment = segment;
                        character.PlayGesture(segment == 1 ? segmentGesture : SparkGesture.Nod, 1.3f);
                    }
                }}
            else if(wasPlaying){wasPlaying=false;cues=null;if(character){character.ClearViseme();character.SetState(returnState);}}
            else if(awaitingAudio&&Time.unscaledTime>=audioUntil){StopSpeech();if(character)character.SetState(returnState);}
        }
        static SparkMouth Map(SparkVisemeScheduler.Pose p){switch(p){case SparkVisemeScheduler.Pose.R_Sn:return SparkMouth.RSn;case SparkVisemeScheduler.Pose.Default:return SparkMouth.Rest;case SparkVisemeScheduler.Pose.Ah:return SparkMouth.Ah;case SparkVisemeScheduler.Pose.Ai:return SparkMouth.Ai;case SparkVisemeScheduler.Pose.Ee:return SparkMouth.Ee;case SparkVisemeScheduler.Pose.Eh:return SparkMouth.Eh;case SparkVisemeScheduler.Pose.FV:return SparkMouth.FV;case SparkVisemeScheduler.Pose.MBP:return SparkMouth.MBP;case SparkVisemeScheduler.Pose.Oo:return SparkMouth.Oo;case SparkVisemeScheduler.Pose.Ooh:return SparkMouth.Ooh;case SparkVisemeScheduler.Pose.S:return SparkMouth.S;default:return SparkMouth.Rest;}}
        void OnWorld(object sender,SparkAIWorldChangedEventArgs args){
            var s=world.CurrentSnapshot;if(s==null)return;bool verified=!levels||SparkAILevelContext.Capture(levels).HasEvaluation;bool danger=verified&&s.Level.HasLevel&&(s.Level.SourceShorted||s.Level.TargetShorted||s.Level.Overloaded);
            if(danger!=warning){warning=danger;if(character)character.SetWarning(danger);if(danger){CancelSemantic();if(cloud)cloud.Cancel();if(microphone)microphone.Pause();if(assistant)assistant.CancelPendingQuestionPro();StopSpeech();string cause=s.Level.SourceShorted?"The current level reports a source short.":s.Level.TargetShorted?"The current level reports a target short.":"The current level reports an overload.";Publish(cause+" Follow the game's safety instructions before continuing.",speakObservationWarnings);}}
            string level=s.Level.HasLevel?s.Level.LevelId:"";
            var currentLayout = SparkAILevelContext.Capture(levels).Layout;
            if (!string.Equals(previousLevel,level,StringComparison.Ordinal) || priorLayout != currentLayout){priorLayout = currentLayout;CancelSemantic();cortex.Reset(false);bool changedFromLevel=previousLevel!=null;previousLevel=level;previousCompleted=false;if(cloud)cloud.ClearConversation();if(assistant)assistant.CancelPendingQuestionPro();if(changedFromLevel&&academy&&academy.IsPracticing)academy.Cancel();if(!warning)StopSpeech();returnState=SparkState.Idle;if(character){character.ClearGesture();character.FocusOn(playerFocus);character.SetState(returnState);}}
            int selectedId=s.Player.HasSelection?s.Player.SelectedObjectInstanceId:0;
            if(selectedId!=previousSelection){previousSelection=selectedId;if(character&&selection){character.FocusOn(selection.SelectedObject?selection.SelectedObject.transform:playerFocus);if (!warning && !PlaybackActive && !IsProcessing && (!microphone || microphone.State != SparkVoiceInputState.Listening)) character.SetState(selection.SelectedObject ? SparkState.Observing : SparkState.Idle);}}
            if(character&&verified&&!warning&&s.Level.Completed&&!previousCompleted){character.SetState(SparkState.Success);character.PlayGesture(SparkGesture.Celebrate,3);}
            if (verified && proactive && !warning && !ExamMode && Time.unscaledTime >= nextProactiveAt &&
                !PlaybackActive && !IsProcessing && (!microphone || microphone.State != SparkVoiceInputState.Listening) &&
                s.Level.Completed && !previousCompleted)
            {
                nextProactiveAt = Time.unscaledTime + 40f;
                Publish($"The evaluator confirmed {s.Level.SatisfiedTargets}/{s.Level.TotalTargets} approved objectives.", false);
            }
            previousCompleted=s.Level.Completed;
        }
        internal bool TrySubmitNexus(string question)
        {
            if (!isActiveAndEnabled || submitting || string.IsNullOrWhiteSpace(question)) return false;
            Ask(question);
            return true;
        }

        public void Ask(string question){
            if(string.IsNullOrWhiteSpace(question)||submitting)return;question=question.Trim();if(question.Length>1000)question=question.Substring(0,1000);string q=question.ToLowerInvariant().TrimEnd('?','.','!');
            question = Regex.Replace(question, @"^hey spark(?:[,.:!]?\s+|[,.:!])", string.Empty, RegexOptions.IgnoreCase).Trim();
            if (question.Length == 0) return;
            q = Regex.Replace(question, @"[,;]\s*", " ").ToLowerInvariant().TrimEnd('?','.','!');
            if(q=="stop"||q=="stop speaking"||q=="wait"||q=="wait a second"||q=="wait stop speaking"){Interrupt();return;}
            if(q=="mute microphone"){MuteMicrophone();return;}
            CancelSemantic();
            Append("You", question);
            if (q is "turn proactive coaching on" or "turn proactive coaching off")
            {
                proactive = q.EndsWith("on", StringComparison.Ordinal);
                PlayerPrefs.SetInt(ProactiveKey, proactive ? 1 : 0);
                Publish(proactive ? "Optional coaching is enabled." : "Optional coaching is disabled.", false);
                return;
            }
            if (q == "reset optional personalization" && !ExamMode)
            {
                if (academy) academy.ResetOptionalLearningObservations();
                cortex.Reset(true);
                Publish("Optional lesson observations and conversation references cleared. Academy practice and campaign progress are preserved.", false);
                return;
            }
            if(warning){Publish("A current level safety warning is active. Follow official safety instructions first.",false);return;}
            submitting=true;returnState=SparkState.Idle;
            if (microphone) microphone.YieldForResponse();
            try{
                if (SparkEngineeringDeviceOperator.TryHandleFrom(this, Regex.Replace(question, @"[,;]\s*", " ").TrimEnd('?', '.', '!'), out string controlResponse) ||
                    SparkEngineeringControlHub.TryHandleFrom(this, question, out controlResponse))
                {
                    Publish(controlResponse, speakAnswers);
                    return;
                }
if(cloud)cloud.Cancel();if(assistant)assistant.CancelPendingQuestionPro();StopSpeech();if(world&&world.IsInitialized)world.ObserveNow();if(warning)return;if(character)character.SetState(SparkState.Thinking);
                var context = CaptureContext(question);
                int requestRevision = cortex.Begin(context);
                segmentGesture = SparkGesture.None;
                string answer;
                if(ExamMode){returnState=SparkState.Examining;answer="Independent examination mode is active. Spark will not provide solutions or hints. Use the official examiner to submit work.";}
                else if (q is "spark inspect mission" or "spark mission status" or "spark export mission report")
                {
                    var advisor = GetComponent<SparkEngineeringMissionAdvisor>();
                    answer = !advisor ? "Mission Advisor is unavailable." :
                        q == "spark export mission report" ? advisor.ExportLastReport() : advisor.Inspect();
                    returnState = SparkState.Observing;
                }
                else if (SparkV5FlightController.TryHandleFrom(this, question, out answer))
                {
                    returnState = SparkState.Teaching;
                }
                else if (SparkTelescopicArmCommands.TryHandleFrom(this, question, out answer)) { } // SPARK_V18_TELESCOPIC
                else if (SparkNativeWireAutoEngineer.TryHandleFrom(this, question, out answer))
                {
                    // Reviewed practice wiring and player demonstration recorder; ahead of generic Cortex.
                    returnState = SparkState.Teaching;
                }                else if (SparkNativeWireDirector.TryHandleFrom(this, question, out answer))
                {
                    // Native player's original wire tool, before legacy graph-only v8.
                    returnState = SparkState.Teaching;
                }
                else if (SparkLabAutonomyAgent.TryHandleFrom(this, question, out answer))
                {
                    // Direct, typed in-game actions before generic Cortex information answers.
                    returnState = SparkState.Teaching;
                }
                else if (SparkLabDemonstrationDirector.TryHandleFrom(this, question, out answer))
                {
                    // Explicit demonstration commands take priority over generic Cortex answers.
                    returnState = SparkState.Teaching;
                }
                else if (SparkLabFocusIntent.Classify(question) != SparkLabFocusIntent.Kind.None)
                {
                    // Explicit world-visual commands take precedence over general LED answers.
                    returnState = SparkState.Observing;
                    var labDirector = GetComponent<SparkLabTeachingDirector>();
                    answer = labDirector ? labDirector.HandleVisualCommand(question) :
                        "AAA Laboratory director is not attached. Run AAA Laboratory > AUTO Bind Prefab and Open Scenes.";
                }
                else if (cortex.TryAnswer(context, out var planned))
                {
                    answer = planned.Text;
                    returnState = planned.State;
                    segmentGesture = planned.Gesture;
                    if (character && planned.Gesture != SparkGesture.None) character.PlayGesture(planned.Gesture, 2.5f);
                }
                else if(q.StartsWith("online explain ") || q.StartsWith("model explain ")){
                    string topic=question.Substring(q.StartsWith("online", StringComparison.Ordinal) ? 15 : 14).Trim();string reference="";if(knowledge&&knowledge.TryFindEntryForQuestion(topic,out var found)&&found!=null)reference=found.Definition+" "+found.Explanation;
                    returnState=SparkState.Teaching;string fallback=string.IsNullOrWhiteSpace(reference)?"No curated definition was found. Use local game questions or select an authored lesson.":reference;
                    if(cloud&&cloud.Explain(topic,reference,reply=>{if(!isActiveAndEnabled||warning||ExamMode||requestRevision!=cortex.Revision)return;if(character)character.SetState(SparkState.Teaching);Publish(string.IsNullOrWhiteSpace(reply)?"Online service unavailable. Authored reference: "+fallback:"Model explanation · general electronics only: "+reply,speakAnswers);}))return;
                    answer="Online explanations disabled or unavailable. Authored reference: "+fallback;
                }
                else if(academy&&(q=="practice"||q=="give me a practice question"||q.StartsWith("practice ")&&q!="practice hint")){returnState=SparkState.Teaching;var current = SparkAILevelContext.Capture(levels).Definition;
                    bool entryLesson = current && current.LevelNumber <= 20;
                    answer = academy.StartPractice(entryLesson && q.Contains("practice question") ? "ohm" : academy.SelectTopic(q));}
                else if(academy&&(q=="practice hint"||(academy.IsPracticing&&q=="hint")))answer=academy.Hint();
                else if(academy&&q.StartsWith("answer "))answer=academy.Submit(q.Substring(7));
                else if(academy&&q=="stop practice")answer=academy.Cancel();
                else if(academy&&(q=="my learning report"||q=="my practice report"||q=="what did i learn previously"))answer=academy.Report();
                else if(academy&&q=="continue our lesson")answer=academy.ResumePractice();
                else if(q=="which chapter"||q=="current chapter"||q=="current campaign"||q=="explain the current mission"||q=="explain current mission")
                {
                    var missionContext = SparkAILevelContext.Capture(levels);
                    var chapter = levels && levels.Campaign ? levels.Campaign.FindChapter(missionContext.Definition) : null;
                    answer = chapter ? $"{levels.Campaign.DisplayName}, chapter {chapter.ChapterNumber}: {chapter.DisplayName}.\n{missionContext.Describe()}" : missionContext.Describe();
                }
                else if (levels && (q=="what tool am i using" || q=="which tool is active"))
                {
                    string activeTool = world ? world.CurrentSnapshot?.Player.ActiveTool : null;
                    answer = string.IsNullOrWhiteSpace(activeTool) ? "No active tool was reported." : "Active tool: " + activeTool;
                }
                else if(q=="show me the positive terminal" || q=="show me the negative terminal")
                {
                    returnState = SparkState.Observing;
                    answer = PointAtTerminal(q.Contains("positive") ? SparkTerminalPolarity.Positive : SparkTerminalPolarity.Negative);
                    if (character && character.FocusTarget && answer.StartsWith("Pointing"))
                        VisualOpened?.Invoke();
                }
                else if(q=="which terminals are connected" || q=="show circuit connections")
                    answer = DescribeConnections();
                else if(q=="what is in my inventory" || q=="show my inventory" || q=="inventory")
                    answer = DescribeInventory();
                else if(q=="power supply settings" || q=="what is this power supply set to")
                    answer = DescribePowerSupply();
                else if(q=="point at this"||q=="point at selected component"||q=="show me this component"){
                    var selected=selection?selection.SelectedObject:null;if(!selected||!character)answer="Select a component first. No verified target is currently selected.";else{character.PointAt(selected.transform);returnState=SparkState.Teaching;answer="I'm pointing toward the selected "+selected.name+". Move closer if it is out of reach.";}}
                else if(q=="teaching models")answer=hologram?hologram.DescribeLibrary():"No teaching model display is assigned.";
                else if(q.StartsWith("show teaching model ") || (q.StartsWith("show ") && q.EndsWith(" in 3d") && !q.Contains("objective") && !q.Contains("level") && !q.Contains("circuit")))
                {
                    returnState=SparkState.Teaching;
                    string model = q.StartsWith("show teaching model ") ? q.Substring(20) : q.Substring(5, q.Length - 11);
                    if (model.StartsWith("me ")) model = model.Substring(3);
                    if (model.StartsWith("a ")) model = model.Substring(2);
                    if (model.StartsWith("an ")) model = model.Substring(3);
                    if (hologram && hologram.Show(model, out string description))
                    {
                        answer = description;
                        VisualOpened?.Invoke();
                    }
                    else answer = "No registered teaching model matches that name.";
                }
                else if(q=="close teaching model"){if(hologram)hologram.Clear();answer="Teaching model closed.";}
                else if(q=="catalog coverage")answer=catalog?$"The compiled catalog contains {catalog.Count} authored levels and {catalog.gaps.Length} audit findings. It is reference data, not live progress.":"Compile and assign the game catalog first.";
                else if(q.StartsWith("catalog level ")){var match=Regex.Match(q,@"\d+");if(match.Success&&int.TryParse(match.Value,out int number)&&catalog&&catalog.TryLevel(number,out var r))answer=$"Reference: Level {r.levelNumber}, {r.title}. {r.description}\n{r.conditions}\n{string.Join("\n",r.objectives)}";else answer="No matching authored level was found in the catalog.";}
                else if(q.StartsWith("define ")&&knowledge&&knowledge.TryFindEntryForQuestion(question,out var entry)&&entry!=null){returnState=SparkState.Teaching;answer=entry.Title+": "+entry.Definition+" "+entry.Explanation;}
                else if(assistant)
                {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                    if (SemanticEnabled && cortex.Intent == SparkIntent.Legacy && BeginSemantic(context)) return;
#endif
                    returnState=q.StartsWith("teach")||q.StartsWith("draw")?SparkState.Teaching:
                        q.Contains("diagnos")||q.Contains("not working")?SparkState.Diagnosing:SparkState.Observing;
                    answer=assistant.AskVerifiedLocalPro(question, out bool opened);
                    if (opened) VisualOpened?.Invoke();
                }
                else answer="Game assistant not assigned. I cannot verify current game or electrical information.";
                if(character){character.SetState(returnState);if(segmentGesture==SparkGesture.None&&returnState==SparkState.Teaching&&!q.StartsWith("point")&&q!="show me this component")character.PlayGesture(SparkGesture.Explain,3);}
                Publish(string.IsNullOrWhiteSpace(answer)?"No verified answer is available.":answer,speakAnswers);
            }catch(Exception ex){Debug.LogException(ex,this);Publish("The request could not be completed reliably. Check the game integration references.",false);if(character)character.SetState(SparkState.Idle);}
            finally{submitting=false;}
        }
        private void CancelSemantic()
        {
            var active = semanticTurn;
            semanticTurn = null;
            if (active != null)
            {
                active.Cancel();
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                SemanticStatus = "Cancelled";
#endif
            }
        }

#if UNITY_EDITOR || DEVELOPMENT_BUILD
        private bool BeginSemantic(SparkCortex.Context context)
        {
            if (ExamMode || warning) return false;
            _ = ResolveSemantic(context);
            return true;
        }

        internal static bool SemanticContextMatches(SparkCortex.Context planned, SparkCortex.Context current,
            int request, string session)
        {
            if (planned.Request != request || planned.Session != session || planned.Level.Layout != current.Level.Layout ||
                planned.Level.Manager != current.Level.Manager || planned.Restricted || current.Restricted ||
                planned.Object != current.Object || planned.Pin != current.Pin) return false;
            var manager = current.Level.Manager;
            return !manager || (manager.CircuitSystem ? manager.CircuitSystem.TopologyVersion : -1) == planned.Topology &&
                (!manager.ElectricalSolver || !manager.ElectricalSolver.IsDirty && !manager.ElectricalSolver.IsSolving) &&
                (manager.ElectricalSolver ? manager.ElectricalSolver.CompletedSolveRevision : -1) == planned.Solve;
        }

        private async System.Threading.Tasks.Task ResolveSemantic(SparkCortex.Context planned)
        {
            var turn = new CancellationTokenSource(TimeSpan.FromSeconds(18));
            semanticTurn = turn;
            bool cloudAllowed = SemanticCloudConsent;
            SemanticStatus = "Selecting";
            string id = $"{planned.Session}:{planned.Request}";
            try
            {
                string json = await SparkPhase4SemanticRouter.Select(id, planned.Question, cloudAllowed, turn.Token);
                if (semanticTurn != turn || !this || !isActiveAndEnabled) return;
                semanticTurn = null;
                if (ExamMode || warning || !SemanticEnabled)
                {
                    SemanticStatus = "Unavailable in current context";
                    return;
                }
                var current = CaptureContext(planned.Question);
                if (!SemanticContextMatches(planned, current, cortex.Revision, cortex.Session))
                {
                    SemanticStatus = "Context changed";
                    if (character) character.SetState(SparkState.Observing);
                    Publish("The workbench changed while I was checking your question. Ask again to use the current evidence.", false);
                    return;
                }
                if ((!cloudAllowed || SemanticCloudConsent) && SparkPhase4SemanticRouter.TrySelect(json, id, cloudAllowed, out var intent) &&
                    cortex.TryAnswer(planned, out var answer, intent))
                {
                    SemanticStatus = "Validated candidate";
                    returnState = answer.State;
                    segmentGesture = answer.Gesture;
                    if (character)
                    {
                        character.SetState(answer.State);
                        if (answer.Gesture != SparkGesture.None) character.PlayGesture(answer.Gesture, 2.5f);
                    }
                    Publish(answer.Text, speakAnswers);
                }
                else
                {
                    SemanticStatus = "Offline fallback";
                    if (character) character.SetState(SparkState.Observing);
                    string fallback = "Try naming the item you want to inspect.";
                    if (assistant)
                    {
                        fallback = assistant.AskVerifiedLocalPro(planned.Question, out bool opened);
                        if (opened) VisualOpened?.Invoke();
                    }
                    Publish(fallback, speakAnswers);
                }
            }
            finally
            {
                if (semanticTurn == turn) semanticTurn = null;
                turn.Dispose();
            }
        }
#endif

        private SparkCortex.Context CaptureContext(string question)
        {
            var target = selection ? selection.SelectedObject : null;
            var pin = target ? target.GetComponentInParent<SparkTerminal>() : null;
            return new SparkCortex.Context
            {
                Level = SparkAILevelContext.Capture(levels),
                World = world ? world.CurrentSnapshot : null,
                Catalog = catalog, Academy = academy, Question = question, Restricted = ExamMode,
                Object = pin && pin.Owner ? pin.Owner : target ? target.GetComponentInParent<SparkElectronicObject>() : null,
                Pin = pin,
                Point = position =>
                {
                    if (!character || warning || ExamMode) return false;
                    character.PointAt(position);
                    return true;
                },
                Draw = command =>
                {
                    if (!assistant || ExamMode) return ("Drawing is unavailable.", false);
                    string result = assistant.AskVerifiedLocalPro(command, out bool opened);
                    if (opened) VisualOpened?.Invoke();
                    return (result, opened);
                },
                TeachingModel = model =>
                {
                    if (!hologram || ExamMode || !hologram.Show(model, out string explanation))
                        return ("No registered model is available for this lesson.", false);
                    VisualOpened?.Invoke();
                    return (explanation, true);
                },
                Demonstrate = request => warning || ExamMode
                    ? "Demonstration is unavailable during a safety warning or independent examination."
                    : SparkProLabCircuit.Demonstrate(request)
            };
        }

        private string PointAtTerminal(SparkTerminalPolarity polarity)
        {
            var selected = selection ? selection.SelectedObject : null;
            if (!selected || !character)
                return "Select a component or terminal first.";
            var terminal = selected.GetComponentInParent<SparkTerminal>();
            if (terminal && terminal.Polarity != polarity)
                return "The selected terminal has a different authored polarity. Inspect the component to locate the correct terminal.";
            if (!terminal)
            {
                var component = selected.GetComponentInParent<SparkElectronicObject>();
                var root = component ? component.transform : selected.transform;
                foreach (var candidate in root.GetComponentsInChildren<SparkTerminal>())
                {
                    if (!candidate || candidate.Polarity != polarity)
                        continue;
                    if (terminal)
                        return $"This component has more than one authored {polarity.ToString().ToLowerInvariant()} terminal. Select the specific terminal.";
                    terminal = candidate;
                }
            }
            if (!terminal)
                return "This selection has no explicitly marked terminal of that polarity. Use the authored pin labels or take a measurement.";
            character.PointAt(terminal.transform);
            return $"Pointing at {terminal.name}, marked {polarity.ToString().ToLowerInvariant()} in the component definition.";
        }

        private static string DescribeInventory()
        {
            var economy = SparkEconomyService.Instance;
            if (!economy || !economy.Catalog || economy.Catalog.Items == null)
                return "The purchased-component inventory is unavailable.";
            var items = new System.Text.StringBuilder();
            int kinds = 0;
            foreach (var item in economy.Catalog.Items)
            {
                if (item == null) continue;
                int count = economy.GetItemCount(item.id);
                if (count > 0 && ++kinds <= 12)
                    items.Append(count).Append(" × ").Append(item.displayName).AppendLine();
            }
            return kinds == 0 ? "The inventory reports no owned shop components." :
                $"Purchased-component inventory has {kinds} item types.\n{items}".TrimEnd();
        }

        private string DescribePowerSupply()
        {
            var selected = selection ? selection.SelectedObject : null;
            var supply = selected ? selected.GetComponentInParent<SparkPowerSupply>() : null;
            if (!supply)
                return "Select a workbench power supply to read its settings.";
            return $"{supply.name} is set to {supply.OutputVoltage:G4} V with a {supply.CurrentLimit:G4} A current limit. " +
                $"The output is {(supply.IsOutputActive ? "on" : "off")}. Measure the circuit to verify its actual voltage.";
        }

        private string DescribeConnections()
        {
            var snapshot = world ? world.CurrentSnapshot : null;
            if (snapshot == null || !snapshot.CircuitAvailable || snapshot.Connections == null)
                return "The live circuit topology is unavailable.";
            var result = new System.Text.StringBuilder();
            int count = 0;
            foreach (var connection in snapshot.Connections)
            {
                if (!connection.Valid) continue;
                if (++count <= 12)
                    result.Append(connection.SourceTerminalName).Append(" to ").Append(connection.TargetTerminalName).AppendLine();
            }
            return count == 0 ? "The circuit reports no valid connections." :
                $"The circuit reports {count} valid connections.\n{result}".TrimEnd();
        }

        /// <summary>
        /// Reports physical wiring stages in the existing Spark Live conversation;
        /// uses current speech output only when available and enabled.
        /// Does not modify electrical state, assessment or game progression.
        /// </summary>
        public void PublishWirePerformanceNarration(string message)
        {
            PublishScheduledWireNarration(message, true);
        }

        internal bool PublishScheduledWireNarration(string message, bool allowSpeech)
        {
            if (ExamMode || string.IsNullOrWhiteSpace(message)) return false;
            if (message.Length > 260) message = message.Substring(0, 260);
            Publish(message, false);
            return allowSpeech && !warning && speakAnswers && SpeechOutputAvailable && voice.SpeakText(message);
        }

        void Publish(string answer,bool speak)
        {
            LastAnswer=answer;
            Append(warning ? "Safety" : "Spark", answer);
            AnswerReady?.Invoke(answer);
            if(speak&&SpeechOutputAvailable)voice.SpeakText(answer);
        }
    }
}
