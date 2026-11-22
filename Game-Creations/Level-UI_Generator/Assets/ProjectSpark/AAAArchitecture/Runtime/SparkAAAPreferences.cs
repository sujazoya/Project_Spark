using UnityEngine;
namespace ProjectSpark.AAAArchitecture
{
    // Only UI preferences, never gameplay progress or rewards.
    public static class SparkAAAPreferences
    {
        private const string Prefix = "ProjectSpark.AAA.UI.v1.";
        public static int Theme { get => Mathf.Clamp(PlayerPrefs.GetInt(Prefix + "theme", 2), 0, 5); set { PlayerPrefs.SetInt(Prefix + "theme", Mathf.Clamp(value,0,5)); PlayerPrefs.Save(); } }
        public static string LastKey { get => PlayerPrefs.GetString(Prefix + "selection", ""); set { PlayerPrefs.SetString(Prefix + "selection", value ?? ""); PlayerPrefs.Save(); } }
    }
}
