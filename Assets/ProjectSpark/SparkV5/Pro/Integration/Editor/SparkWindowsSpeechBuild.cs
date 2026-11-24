using System;
using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;

namespace ProjectSpark.SparkV5.Pro.Editor
{
    internal sealed class SparkWindowsSpeechBuild : IPostprocessBuildWithReport
    {
        public int callbackOrder => 100;

        public void OnPostprocessBuild(BuildReport report)
        {
            if (report.summary.platform != BuildTarget.StandaloneWindows64 &&
                report.summary.platform != BuildTarget.StandaloneWindows) return;
            string source = Environment.GetEnvironmentVariable("SPARK_WINDOWS_SPEECH_PACKAGE");
            if (string.IsNullOrWhiteSpace(source)) return;
            string executable = report.summary.outputPath;
            string target = Path.Combine(Path.GetDirectoryName(executable),
                Path.GetFileNameWithoutExtension(executable) + "_Data", "StreamingAssets", "SparkSpeech");
            Stage(source, target);
        }

        internal static void Stage(string source, string target)
        {
            if (!File.Exists(Path.Combine(source, "spark-stt.exe")) ||
                !File.Exists(Path.Combine(source, "model", "am", "final.mdl")))
                throw new BuildFailedException("The configured Spark Windows speech package is incomplete.");
            Copy(source, target);
        }

        private static void Copy(string source, string target)
        {
            Directory.CreateDirectory(target);
            foreach (string file in Directory.EnumerateFiles(source))
                File.Copy(file, Path.Combine(target, Path.GetFileName(file)), true);
            foreach (string directory in Directory.EnumerateDirectories(source))
                Copy(directory, Path.Combine(target, Path.GetFileName(directory)));
        }
    }
}
