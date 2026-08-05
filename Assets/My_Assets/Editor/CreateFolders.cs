using UnityEditor;
using System.IO;

public static class CreateProjectSparkFolders
{
    [MenuItem("Tools/Project Spark/Create Folder Structure")]
    public static void CreateFolders()
    {
        string[] folders =
        {
            "Assets/ProjectSpark",

            "Assets/ProjectSpark/Core",
            "Assets/ProjectSpark/Core/Bootstrap",
            "Assets/ProjectSpark/Core/Logging",
            "Assets/ProjectSpark/Core/Events",
            "Assets/ProjectSpark/Core/Services",
            "Assets/ProjectSpark/Core/Update",
            "Assets/ProjectSpark/Core/Configuration",
            "Assets/ProjectSpark/Core/Lifetime",
            "Assets/ProjectSpark/Core/Utilities",
            "Assets/ProjectSpark/Core/Editor",

            "Assets/ProjectSpark/Input",
            "Assets/ProjectSpark/Camera",
            "Assets/ProjectSpark/Interaction",
            "Assets/ProjectSpark/Electronics",
            "Assets/ProjectSpark/Gameplay",
            "Assets/ProjectSpark/Rendering",
            "Assets/ProjectSpark/UI",
            "Assets/ProjectSpark/Audio",
            "Assets/ProjectSpark/Save",
            "Assets/ProjectSpark/Tools",
            "Assets/ProjectSpark/Tests",
            "Assets/ProjectSpark/Documentation"
        };

        foreach (string folder in folders)
        {
            if (!AssetDatabase.IsValidFolder(folder))
            {
                Directory.CreateDirectory(folder);
            }
        }

        AssetDatabase.Refresh();

        UnityEngine.Debug.Log("✅ Project Spark folder structure created successfully!");
    }
}