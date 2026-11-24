using System;
using System.IO;
using System.Diagnostics;
using UnityEngine;

namespace ProjectSpark.SparkV5.Pro
{
    internal sealed class SparkProSpeechProcess : IDisposable
    {
        private Process owned;
        private float nextAttempt;
        private float started;
        public string Diagnostic { get; private set; } = "";

        public void OnHealth(Uri uri, bool ready, float now)
        {
#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
            if (ready || uri == null || uri.Scheme != "http" || !uri.IsLoopback || uri.Port != 8766) return;
            if (owned != null && !Exited())
            {
                if (now - started < 35f) return;
                Dispose();
                nextAttempt = now + 8f;
                Diagnostic = "Restarting the local speech service";
                return;
            }
            if (now < nextAttempt) return;
            Dispose();
            nextAttempt = now + 8f;

            string directory = Path.Combine(Application.streamingAssetsPath, "SparkSpeech");
            string executable = Path.Combine(directory, "spark-stt.exe");
            string model = Path.Combine(directory, "model");
            string scriptArg = "";
#if UNITY_EDITOR_WIN
            if (!File.Exists(executable))
            {
                directory = Path.GetFullPath(Path.Combine(Application.dataPath, "..", "..", "AI", "SpeechServer"));
                executable = Path.Combine(directory, ".venv", "Scripts", "python.exe");
                string packagedScript = Path.GetFullPath(Path.Combine(Application.dataPath, "..", "Python", "SpeechServer", "spark_stt.py"));
                if (!File.Exists(packagedScript)) return;
                scriptArg = Quote(packagedScript) + " ";
                model = Path.Combine(directory, "model");
            }
#endif
            if (!File.Exists(executable) || !Directory.Exists(model))
            {
                Diagnostic = "Local speech runtime is not installed";
                return;
            }
            try
            {
                owned = Process.Start(new ProcessStartInfo
                {
                    FileName = executable,
                    Arguments = scriptArg + "--host 127.0.0.1 --port 8766 --model " + Quote(model),
                    WorkingDirectory = directory,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    WindowStyle = ProcessWindowStyle.Hidden
                });
                started = now;
                Diagnostic = owned != null ? "Starting the local speech service" : "Local speech service could not start";
            }
            catch (Exception error) when (error is InvalidOperationException or System.ComponentModel.Win32Exception or IOException)
            { Diagnostic = "Local speech service could not start"; }
#endif
        }

        private bool Exited()
        {
            try { return owned?.HasExited != false; }
            catch (InvalidOperationException) { return true; }
        }

        public void Dispose()
        {
            if (owned == null) return;
            try { if (!Exited()) owned.Kill(); }
            catch (Exception error) when (error is InvalidOperationException or System.ComponentModel.Win32Exception) { }
            finally { owned.Dispose(); owned = null; }
        }

        private static string Quote(string path) => "\"" + path + "\"";
    }
}
