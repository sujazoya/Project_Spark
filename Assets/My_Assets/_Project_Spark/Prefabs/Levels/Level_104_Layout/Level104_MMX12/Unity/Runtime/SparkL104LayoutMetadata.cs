using System;
using UnityEngine;

namespace ProjectSpark.Level104.AutoLayout
{
    /// <summary>
    /// Art-only scene metadata. None of these types may connect a SparkTerminal,
    /// modify the electrical solver, or award LevelGamePlayManager objectives.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class SparkL104LayoutState : MonoBehaviour
    {
        public const int RequiredComponents = 94;
        public const int RequiredRoutes = 124;
        public const int RequiredVias = 34;
        public const int ObjectiveCount = 6;

        [Header("Source design / not authoritative gameplay")]
        public int levelNumber = 104;
        public string boardId = "MMX-12";
        public Vector2 physicalBoardMeters = new Vector2(0.260f, 0.185f);
        public int copperLayers = 8;
        [Header("Initial, blocked state")]
        public bool schematicApproved = false;
        public bool electricalIntegrationVerified = false;
        public bool w125Connected = false;
        public int installedComponents = 0;
        public int certifiedRoutes = 0;
        public int certifiedVias = 0;
        public int awardedObjectives = 0;
        [TextArea(3, 5)]
        public string blocker =
            "ART ONLY: 124 route endpoints, 34 via spans and W125 pins are unverified. " +
            "W125 names U11/U12, absent from the 94-part BOM. " +
            "Do not certify gameplay without inspected project APIs and an approved schematic.";
    }

    [Serializable]
    public sealed class SparkL104ModelRecord
    {
        public string assetId;
        public string kind;
        public string sourceFbx;
        public string sourceGlb;
        public Vector3 physicalLocalPosition;
        public bool installable;
    }

    [Serializable]
    public sealed class SparkL104TaskRecord
    {
        public string id;
        public string category;
        public string sourcePad;
        public string destinationPad;
        public string netId;
        public string layerSpan;
        public bool certifiable = false;
    }

    public sealed class SparkL104SourceBlueprint : ScriptableObject
    {
        public string boardId = "MMX-12";
        public Vector2 physicalBoardMeters = new Vector2(0.260f, 0.185f);
        public int copperLayers = 8;
        public SparkL104ModelRecord[] models = Array.Empty<SparkL104ModelRecord>();
        public SparkL104TaskRecord[] copperRoutes = Array.Empty<SparkL104TaskRecord>();
        public SparkL104TaskRecord[] vias = Array.Empty<SparkL104TaskRecord>();
        public bool approvedPinLevelNetlist = false;
        public bool w125EndpointMappingApproved = false;
        [TextArea(2, 5)]
        public string validationNote =
            "Source-art blueprint only. Route/via records are NOT_CERTIFIABLE, and " +
            "W125 U11/U12 logical owners are not present in the BOM.";
    }

    [DisallowMultipleComponent]
    public sealed class SparkL104ComponentVisualSlot : MonoBehaviour
    {
        public string componentReference;
        public string sourceAssetId;
        public string visualPrefabPath;
        public Vector3 intendedPhysicalPosition;
        public bool installed = false;
        public bool footprintApproved = false;
        public bool solderValidated = false;
        public bool conductive = false;
    }

    [DisallowMultipleComponent]
    public sealed class SparkL104BlockedTaskMarker : MonoBehaviour
    {
        public string taskId;
        public string taskGroup;
        public string sourcePad;
        public string destinationPad;
        public string netId;
        public string layerSpan;
        public bool certifiable = false;
        public bool completed = false;
    }

    [DisallowMultipleComponent]
    public sealed class SparkL104VisualReference : MonoBehaviour
    {
        public bool visualOnly = true;
        public bool participatesInGameplay = false;
    }
}
