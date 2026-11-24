#if UNITY_ANDROID
using System.IO;
using System.Linq;
using System.Xml.Linq;
using UnityEditor.Android;

namespace ProjectSpark.SparkV5.Pro.Editor
{
    internal sealed class SparkAndroidVoiceManifest : IPostGenerateGradleAndroidProject
    {
        public int callbackOrder => 30;

        public void OnPostGenerateGradleAndroidProject(string path)
        {
            var manifestPath = Path.Combine(path, "src", "main", "AndroidManifest.xml");
            var document = XDocument.Load(manifestPath);
            var manifest = document.Root;
            XNamespace android = "http://schemas.android.com/apk/res/android";
            const string permission = "android.permission.RECORD_AUDIO";
            if (!manifest.Elements("uses-permission").Any(entry => (string)entry.Attribute(android + "name") == permission))
                manifest.AddFirst(new XElement("uses-permission", new XAttribute(android + "name", permission)));
            var queries = manifest.Element("queries");
            if (queries == null) manifest.Add(queries = new XElement("queries"));
            foreach (string service in new[] { "android.speech.RecognitionService", "android.intent.action.TTS_SERVICE" })
                if (!queries.Descendants("action").Any(action => (string)action.Attribute(android + "name") == service))
                    queries.Add(new XElement("intent", new XElement("action", new XAttribute(android + "name", service))));
            document.Save(manifestPath);
        }
    }
}
#endif
