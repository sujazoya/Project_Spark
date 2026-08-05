using UnityEditor;
using UnityEngine;

public static class SystemsHierarchyCreator
{
    [MenuItem("Project Spark/Create Systems Hierarchy")]
    public static void CreateSystemsHierarchy()
    {
        // Find or create Systems root
        GameObject systems = GameObject.Find("Systems");

        if (systems == null)
        {
            systems = new GameObject("Systems");
        }

        // List of managers/systems
        string[] systemNames =
        {
            "Bootstrap",
            "GameManager",
            "LevelManager",
            "ScenarioManager",
            "ObjectiveManager",
            "SimulationManager",
            "WorkshopManager",
            "DiagnosticsManager",
            "ToolManager",
            "InputManager",
            "CameraManager",
            "SaveManager",
            "AudioManager",
            "UIManager",
            "CursorManager",
            "HintManager"
        };

        foreach (string systemName in systemNames)
        {
            // Don't create duplicates
            Transform existing = systems.transform.Find(systemName);

            if (existing == null)
            {
                GameObject systemObject = new GameObject(systemName);
                systemObject.transform.SetParent(
                    systems.transform,
                    false
                );
            }
        }

        // Select Systems in Hierarchy
        Selection.activeGameObject = systems;

        Debug.Log(
            "Project Spark Systems hierarchy created successfully."
        );
    }
}