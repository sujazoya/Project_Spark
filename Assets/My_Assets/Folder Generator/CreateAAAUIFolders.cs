using System.IO;
using UnityEditor;
using UnityEngine;

public static class CreateAAAUIFolders
{
    [MenuItem("Tools/AAAUI/Create Folder Structure")]
    public static void CreateFolders()
    {
        string[] folders =
        {
            // Root
    "Assets/AAAUI",

    // Runtime
    "Assets/AAAUI/Runtime",

    // Core
    "Assets/AAAUI/Runtime/Core",
    "Assets/AAAUI/Runtime/Core/Internal",

    // Animation
    "Assets/AAAUI/Runtime/Animation",
    "Assets/AAAUI/Runtime/Animation/Runtime",
    "Assets/AAAUI/Runtime/Animation/Handles",
    "Assets/AAAUI/Runtime/Animation/Tracks",
    "Assets/AAAUI/Runtime/Animation/Evaluation",

    // Property System
    "Assets/AAAUI/Runtime/PropertySystem",
    "Assets/AAAUI/Runtime/PropertySystem/Custom",

    // Binding
    "Assets/AAAUI/Runtime/Binding",

    // Interaction
    "Assets/AAAUI/Runtime/Interaction",
    "Assets/AAAUI/Runtime/Interaction/Input",
    "Assets/AAAUI/Runtime/Interaction/Focus",
    "Assets/AAAUI/Runtime/Interaction/States",

    // UI Components
    "Assets/AAAUI/Runtime/UIComponents",

    // Navigation
    "Assets/AAAUI/Runtime/Navigation",

    // Transitions
    "Assets/AAAUI/Runtime/Transitions",
    "Assets/AAAUI/Runtime/Transitions/Custom",

    // Feedback
    "Assets/AAAUI/Runtime/Feedback",
    "Assets/AAAUI/Runtime/Feedback/Audio",
    "Assets/AAAUI/Runtime/Feedback/Haptics",
    "Assets/AAAUI/Runtime/Feedback/VFX",
    "Assets/AAAUI/Runtime/Feedback/Effects",

    // Rendering
    "Assets/AAAUI/Runtime/Rendering",
    "Assets/AAAUI/Runtime/Rendering/URP",

    // Performance
    "Assets/AAAUI/Runtime/Performance",

    // Async
    "Assets/AAAUI/Runtime/Async",

    // Accessibility
    "Assets/AAAUI/Runtime/Accessibility",

    // SaveState
    "Assets/AAAUI/Runtime/SaveState",

    // Integration
    "Assets/AAAUI/Runtime/Integration",
    "Assets/AAAUI/Runtime/Integration/InputSystem",
    "Assets/AAAUI/Runtime/Integration/TextMeshPro",
    "Assets/AAAUI/Runtime/Integration/Addressables",
    "Assets/AAAUI/Runtime/Integration/URP",

    // Data
    "Assets/AAAUI/Runtime/Data",
    "Assets/AAAUI/Runtime/Data/Animation",
    "Assets/AAAUI/Runtime/Data/Feedback",
    "Assets/AAAUI/Runtime/Data/Navigation",
    "Assets/AAAUI/Runtime/Data/Accessibility",
    "Assets/AAAUI/Runtime/Data/Settings",

    // Assemblies
    "Assets/AAAUI/Runtime/Assemblies",

    // Editor
    "Assets/AAAUI/Editor",
    "Assets/AAAUI/Editor/Core",
    "Assets/AAAUI/Editor/Animation",
    "Assets/AAAUI/Editor/Binding",
    "Assets/AAAUI/Editor/Navigation",
    "Assets/AAAUI/Editor/Feedback",
    "Assets/AAAUI/Editor/Components",
    "Assets/AAAUI/Editor/Debug",
    "Assets/AAAUI/Editor/Windows",

    // Samples
    "Assets/AAAUI/Samples",
    "Assets/AAAUI/Samples/Demo",
    "Assets/AAAUI/Samples/Animation",
    "Assets/AAAUI/Samples/Navigation",
    "Assets/AAAUI/Samples/Feedback",
    "Assets/AAAUI/Samples/CompleteUI",

    // Resources
    "Assets/AAAUI/Resources",
    "Assets/AAAUI/Resources/AAAUI",

    // Settings
    "Assets/AAAUI/Settings",

    // Documentation
    "Assets/AAAUI/Documentation"
        };

        foreach (var folder in folders)
        {
            if (!Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
                Debug.Log("Created: " + folder);
            }
        }
        

        AssetDatabase.Refresh();

        Debug.Log("<color=green>AAAUI folder structure created successfully.</color>");
    }
}