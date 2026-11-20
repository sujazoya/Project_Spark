using System;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectSpark.AutoLevelGenerator
{
    /// <summary>Serializable, source-backed level design. This is not an electrical solver or a certificate.</summary>
    [CreateAssetMenu(fileName = "AutoLevelBlueprint", menuName = "Project Spark/Automatic Levels/Blueprint")]
    public sealed class SparkAutoLevelBlueprint : ScriptableObject
    {
        [SerializeField] private int schemaVersion = 1;
        [SerializeField] private int levelId;
        [SerializeField] private string boardId;
        [SerializeField] private string displayName;
        [SerializeField] private float boardWidthMm;
        [SerializeField] private float boardHeightMm;
        [SerializeField] private int copperLayers;
        [SerializeField] private float millimetersToUnityUnits = 0.001f;
        [SerializeField] private List<SparkAutoPart> parts = new List<SparkAutoPart>();
        [SerializeField] private List<SparkAutoRoute> routes = new List<SparkAutoRoute>();
        [SerializeField] private List<SparkAutoVia> vias = new List<SparkAutoVia>();
        [SerializeField] private List<SparkAutoObjective> objectives = new List<SparkAutoObjective>();
        [SerializeField] private List<SparkAutoFault> faults = new List<SparkAutoFault>();
        [SerializeField] private SparkAutoSafety safety = new SparkAutoSafety();

        public int SchemaVersion => schemaVersion;
        public int LevelId => levelId;
        public string BoardId => boardId;
        public string DisplayName => displayName;
        public float BoardWidthMm => boardWidthMm;
        public float BoardHeightMm => boardHeightMm;
        public int CopperLayers => copperLayers;
        public float MillimetersToUnityUnits => millimetersToUnityUnits;
        public IReadOnlyList<SparkAutoPart> Parts => parts;
        public IReadOnlyList<SparkAutoRoute> Routes => routes;
        public IReadOnlyList<SparkAutoVia> Vias => vias;
        public IReadOnlyList<SparkAutoObjective> Objectives => objectives;
        public IReadOnlyList<SparkAutoFault> Faults => faults;
        public SparkAutoSafety Safety => safety;

        public int InstallableCount
        {
            get { int n = 0; for (int i = 0; i < parts.Count; i++) if (parts[i].Installable) n++; return n; }
        }

        public void Populate(int id, string board, string title, float widthMm, float heightMm, int layers,
            float unitsPerMillimeter, List<SparkAutoPart> partData, List<SparkAutoRoute> routeData,
            List<SparkAutoVia> viaData, List<SparkAutoObjective> objectiveData,
            List<SparkAutoFault> faultData, SparkAutoSafety safetyData)
        {
            levelId = id;
            boardId = board ?? string.Empty;
            displayName = title ?? string.Empty;
            boardWidthMm = widthMm;
            boardHeightMm = heightMm;
            copperLayers = layers;
            millimetersToUnityUnits = unitsPerMillimeter;
            parts = partData ?? new List<SparkAutoPart>();
            routes = routeData ?? new List<SparkAutoRoute>();
            vias = viaData ?? new List<SparkAutoVia>();
            objectives = objectiveData ?? new List<SparkAutoObjective>();
            faults = faultData ?? new List<SparkAutoFault>();
            safety = safetyData ?? new SparkAutoSafety();
        }

        public bool TryGetPart(string id, out SparkAutoPart part)
        {
            for (int i = 0; i < parts.Count; i++)
                if (string.Equals(parts[i].Id, id, StringComparison.Ordinal)) { part = parts[i]; return true; }
            part = null; return false;
        }
        public bool TryGetRoute(string id, out SparkAutoRoute route)
        {
            for (int i = 0; i < routes.Count; i++)
                if (string.Equals(routes[i].Id, id, StringComparison.Ordinal)) { route = routes[i]; return true; }
            route = null; return false;
        }
        public bool TryGetVia(string id, out SparkAutoVia via)
        {
            for (int i = 0; i < vias.Count; i++)
                if (string.Equals(vias[i].Id, id, StringComparison.Ordinal)) { via = vias[i]; return true; }
            via = null; return false;
        }
    }

    [Serializable]
    public sealed class SparkAutoPart
    {
        public string Id;
        public string Type;
        public string AssetPath;
        public float Xmm;
        public float Ymm;
        public float Zmm;
        public float OrientationDegrees;
        public bool Installable;
        public string Notes;
    }

    [Serializable]
    public sealed class SparkAutoRoute
    {
        public string Id;
        public string Subsystem;
        public string NetId;
        public string SourceComponent;
        public string SourcePad;
        public string DestinationComponent;
        public string DestinationPad;
        public string Layer;
        public string SourceStatus;

        public bool HasSchematicFields(int layerCount)
        {
            if (SparkAutoDesignChecks.IsUnknown(NetId) || SparkAutoDesignChecks.IsUnknown(SourceComponent) ||
                SparkAutoDesignChecks.IsUnknown(SourcePad) || SparkAutoDesignChecks.IsUnknown(DestinationComponent) ||
                SparkAutoDesignChecks.IsUnknown(DestinationPad) || SparkAutoDesignChecks.IsUnknown(Layer)) return false;
            int n;
            return int.TryParse(Layer, out n) && n >= 1 && n <= layerCount &&
                   string.Equals(SourceStatus, "APPROVED", StringComparison.OrdinalIgnoreCase);
        }
    }

    [Serializable]
    public sealed class SparkAutoVia
    {
        public string Id;
        public string NetId;
        public string FromLayer;
        public string ToLayer;
        public string Xmm;
        public string Ymm;
        public string ViaType;
        public string SourceStatus;

        public bool HasSchematicFields(int layerCount)
        {
            if (SparkAutoDesignChecks.IsUnknown(NetId) || SparkAutoDesignChecks.IsUnknown(ViaType) ||
                SparkAutoDesignChecks.IsUnknown(Xmm) || SparkAutoDesignChecks.IsUnknown(Ymm)) return false;
            int from, to;
            float x, y;
            return int.TryParse(FromLayer, out from) && int.TryParse(ToLayer, out to) &&
                   from >= 1 && to >= 1 && from <= layerCount && to <= layerCount && from != to &&
                   float.TryParse(Xmm, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out x) &&
                   float.TryParse(Ymm, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out y) &&
                   !float.IsNaN(x) && !float.IsNaN(y) && !float.IsInfinity(x) && !float.IsInfinity(y) &&
                   string.Equals(SourceStatus, "APPROVED", StringComparison.OrdinalIgnoreCase);
        }
    }

    [Serializable] public sealed class SparkAutoObjective { public string Id; public string Title; public string Criterion; }
    [Serializable] public sealed class SparkAutoFault { public string Id; public string Title; public string Mechanism; public string Evidence; public string Repair; }
    [Serializable]
    public sealed class SparkAutoSafety
    {
        public string JumperId;
        public string LogicalSource;
        public string LogicalDestination;
        public string PhysicalPinStatus;
        public string NetlistStatus;
        public bool StartsDisconnected = true;
        public bool HasApprovedPhysicalPins => !SparkAutoDesignChecks.IsUnknown(PhysicalPinStatus) &&
            string.Equals(NetlistStatus, "APPROVED", StringComparison.OrdinalIgnoreCase);
    }

    public static class SparkAutoDesignChecks
    {
        public static bool IsUnknown(string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return true;
            string value = text.Trim();
            return value.Equals("UNASSIGNED", StringComparison.OrdinalIgnoreCase) ||
                   value.Equals("UNKNOWN", StringComparison.OrdinalIgnoreCase) ||
                   value.Equals("NOT_CERTIFIABLE", StringComparison.OrdinalIgnoreCase) ||
                   value.Equals("SCHEMATIC_REQUIRED", StringComparison.OrdinalIgnoreCase) ||
                   value.Equals("ART_PLACEHOLDER", StringComparison.OrdinalIgnoreCase);
        }
    }
}
