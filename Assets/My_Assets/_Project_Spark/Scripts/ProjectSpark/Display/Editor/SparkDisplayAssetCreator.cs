#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace ProjectSpark.Display.Editor
{
    public static class SparkDisplayAssetCreator
    {
        [MenuItem("Project Spark/Display/Create Default Profiles")]
        private static void CreateProfiles()
        {
            const string root = "Assets/My_Assets/_Project_Spark/Data/Display";
            const string profilePath = root + "/Profiles";
            const string animationPath = root + "/Animation";

            EnsureFolder("Assets/My_Assets");
            EnsureFolder("Assets/My_Assets/_Project_Spark");
            EnsureFolder(root);
            EnsureFolder(profilePath);
            EnsureFolder(animationPath);

            CreateIfMissing<SparkDisplayProfile>(
                profilePath + "/Generic_Display_Profile.asset",
                "Generic Display Profile");

            CreateIfMissing<SparkDisplayAnimationProfile>(
                animationPath + "/Industrial_Display_Animation.asset",
                "Industrial Display Animation");

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("Project Spark display profiles created.");
        }

        private static void CreateIfMissing<T>(string path, string name) where T : ScriptableObject
        {
            if (AssetDatabase.LoadAssetAtPath<T>(path) != null)
                return;

            T asset = ScriptableObject.CreateInstance<T>();
            asset.name = name;
            AssetDatabase.CreateAsset(asset, path);
        }

        private static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path))
                return;

            string parent = System.IO.Path.GetDirectoryName(path)?.Replace("\\", "/");
            string folder = System.IO.Path.GetFileName(path);

            if (!string.IsNullOrEmpty(parent) && !AssetDatabase.IsValidFolder(parent))
                EnsureFolder(parent);

            AssetDatabase.CreateFolder(parent, folder);
        }
    }
}
#endif
