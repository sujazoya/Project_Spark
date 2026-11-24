#if UNITY_ANDROID && !UNITY_EDITOR
using System;
using System.Collections.Concurrent;
using System.Threading;
using UnityEngine;

namespace ProjectSpark.AI
{
    // Reuses the Executive's turn state and the platform's installed offline speech services.
    internal sealed class SparkAndroidRecognition
    {
        private readonly ConcurrentQueue<Action> callbacks = new();
        private AndroidJavaObject recognizer;
        private Listener listener;
        private int generation;

        internal static AndroidJavaObject Activity()
        {
            using var player = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            return player.GetStatic<AndroidJavaObject>("currentActivity");
        }

        internal static bool Available()
        {
            try
            {
                using var version = new AndroidJavaClass("android.os.Build$VERSION");
                if (version.GetStatic<int>("SDK_INT") < 31) return false;
                using var activity = Activity();
                using var service = new AndroidJavaClass("android.speech.SpeechRecognizer");
                return activity != null && service.CallStatic<bool>("isOnDeviceRecognitionAvailable", activity);
            }
            catch (AndroidJavaException) { return false; }
        }

        internal void Start(Action<string, float> complete, Action fail)
        {
            int id = Interlocked.Increment(ref generation);
            void Result(string transcript, float confidence) =>
                callbacks.Enqueue(() => { if (generation == id) complete(transcript, confidence); });
            void Failed() => callbacks.Enqueue(() => { if (generation == id) fail(); });
            using var activity = Activity();
            activity.Call("runOnUiThread", new AndroidJavaRunnable(() =>
            {
                if (generation != id) return;
                try
                {
                    Release();
                    using var owner = Activity();
                    using var service = new AndroidJavaClass("android.speech.SpeechRecognizer");
                    recognizer = service.CallStatic<AndroidJavaObject>("createOnDeviceSpeechRecognizer", owner);
                    listener = new Listener(Result, Failed);
                    recognizer.Call("setRecognitionListener", listener);
                    using var intent = new AndroidJavaObject("android.content.Intent", "android.speech.action.RECOGNIZE_SPEECH");
                    using var setting = intent.Call<AndroidJavaObject>("putExtra", "android.speech.extra.LANGUAGE_MODEL", "free_form");
                    recognizer.Call("startListening", intent);
                }
                catch (AndroidJavaException) { Failed(); }
            }));
        }

        internal void Update() { while (callbacks.TryDequeue(out var callback)) callback(); }

        internal void End()
        {
            int id = generation;
            using var activity = Activity();
            activity.Call("runOnUiThread", new AndroidJavaRunnable(() =>
            {
                if (generation != id || recognizer == null) return;
                try { recognizer.Call("stopListening"); } catch (AndroidJavaException) { }
            }));
        }

        internal void Stop()
        {
            int id = Interlocked.Increment(ref generation);
            using var activity = Activity();
            activity?.Call("runOnUiThread", new AndroidJavaRunnable(() =>
            {
                if (generation == id) Release();
            }));
        }

        private void Release()
        {
            if (recognizer == null) return;
            try { recognizer.Call("cancel"); recognizer.Call("destroy"); }
            catch (AndroidJavaException) { }
            finally { recognizer.Dispose(); recognizer = null; listener = null; }
        }

        [UnityEngine.Scripting.Preserve]
        private sealed class Listener : AndroidJavaProxy
        {
            private readonly Action<string, float> result;
            private readonly Action failure;
            internal Listener(Action<string, float> result, Action failure) : base("android.speech.RecognitionListener")
            { this.result = result; this.failure = failure; }
            public void onResults(AndroidJavaObject bundle)
            {
                try
                {
                    using var texts = bundle.Call<AndroidJavaObject>("getStringArrayList", "results_recognition");
                    using var first = texts != null && texts.Call<int>("size") > 0 ? texts.Call<AndroidJavaObject>("get", 0) : null;
                    string text = first?.Call<string>("toString") ?? "";
                    float[] confidence = bundle.Call<float[]>("getFloatArray", "confidence_scores");
                    result(text, confidence?.Length > 0 ? confidence[0] : -1f);
                }
                catch (AndroidJavaException) { failure(); }
            }

            public void onError(int code) { if (code == 6 || code == 7) result("", -1f); else failure(); }
            public void onReadyForSpeech(AndroidJavaObject unused) { }
            public void onBeginningOfSpeech() { }
            public void onRmsChanged(float unused) { }
            public void onBufferReceived(byte[] unused) { }
            public void onEndOfSpeech() { }
            public void onPartialResults(AndroidJavaObject unused) { }
            public void onEvent(int code, AndroidJavaObject unused) { }
        }
    }

    internal sealed class SparkAndroidTts : ISparkAITTSProvider
    {
        private AndroidJavaObject engine;
        private readonly Init listener;
        private bool checkedVoice, available;
        private int state = -2;

        internal SparkAndroidTts()
        {
            listener = new Init(status => Volatile.Write(ref state, status));
            try
            {
                using var activity = SparkAndroidRecognition.Activity();
                if (activity != null) engine = new AndroidJavaObject("android.speech.tts.TextToSpeech", activity, listener);
            }
            catch (AndroidJavaException) { }
        }

        public bool IsAvailable
        {
            get
            {
                if (engine == null || Volatile.Read(ref state) != 0) return false;
                if (!checkedVoice)
                {
                    checkedVoice = true;
                    try
                    {
                        using var voice = engine.Call<AndroidJavaObject>("getVoice");
                        available = voice != null && !voice.Call<bool>("isNetworkConnectionRequired");
                    }
                    catch (AndroidJavaException) { available = false; }
                }
                return available;
            }
        }

        internal bool IsSpeaking
        {
            get
            {
                try { return engine != null && engine.Call<bool>("isSpeaking"); }
                catch (AndroidJavaException) { return false; }
            }
        }

        public bool Speak(SparkAIVoiceRequest request)
        {
            if (!IsAvailable || !request.IsValid) return false;
            try
            {
                using var payload = new AndroidJavaObject("java.lang.String", request.Text);
                using var options = new AndroidJavaObject("android.os.Bundle");
                return engine.Call<int>("speak", payload, 0, options, Guid.NewGuid().ToString("N")) == 0;
            }
            catch (AndroidJavaException) { return false; }
        }
        public void Stop() { try { engine?.Call<int>("stop"); } catch (AndroidJavaException) { } }
        public void Shutdown()
        {
            try { engine?.Call("shutdown"); } catch (AndroidJavaException) { }
            finally { engine?.Dispose(); engine = null; }
        }
        [UnityEngine.Scripting.Preserve]
        private sealed class Init : AndroidJavaProxy
        {
            private readonly Action<int> ready;
            internal Init(Action<int> ready) : base("android.speech.tts.TextToSpeech$OnInitListener") { this.ready = ready; }
            public void onInit(int status) => ready(status);
        }
    }
}
#endif
