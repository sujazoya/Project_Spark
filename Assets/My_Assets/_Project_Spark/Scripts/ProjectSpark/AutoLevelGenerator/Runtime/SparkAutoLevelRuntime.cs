using System;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectSpark.AutoLevelGenerator
{
    /// <summary>
    /// Adapter to the actual Project Spark electrical graph, placement/shelf, repair and LevelGamePlayManager APIs.
    /// Deliberately abstract: the generator NEVER invents approvals or completion. An existing-project adapter
    /// must check real contact, current topology, telemetry, safety and objective authority.
    /// </summary>
    public abstract class SparkAutoLevelAuthorityBridge : MonoBehaviour
    {
        public abstract bool TryCommitPart(SparkAutoPart definition, SparkAutoPlacementTarget target, out string rejection);
        public abstract bool TryCommitRoute(SparkAutoRoute definition, out string rejection);
        public abstract bool TryCommitVia(SparkAutoVia definition, out string rejection);
        public abstract bool TryConfirmObjective(SparkAutoObjective objective, out string rejection);
        public abstract bool TryConfirmSafetyReset(SparkAutoSafety safety, out string rejection);
    }

    public sealed class SparkAutoPlacementTarget : MonoBehaviour
    {
        [SerializeField] private string partId;
        [SerializeField] private string partType;
        [SerializeField] private SparkAutoLevelRuntime levelRuntime;
        [SerializeField] private bool isReferenceArt;
        [SerializeField] private GameObject installedVisualPrefab;
        [SerializeField] private bool spawnVisualAfterAuthorizedPlacement = true;
        private GameObject installedVisual;

        public string PartId => partId;
        public string PartType => partType;
        public bool IsReferenceArt => isReferenceArt;

        public void Configure(string id, string type, SparkAutoLevelRuntime owner, bool reference, GameObject visualPrefab = null)
        {
            partId = id;
            partType = type;
            levelRuntime = owner;
            isReferenceArt = reference;
            installedVisualPrefab = visualPrefab;
        }

        internal void ShowApprovedVisual()
        {
            if (!spawnVisualAfterAuthorizedPlacement || installedVisualPrefab == null || installedVisual != null) return;
            installedVisual = Instantiate(installedVisualPrefab, transform);
            installedVisual.name = partId + "_INSTALLED_APPROVED_VISUAL";
            installedVisual.transform.localPosition = Vector3.zero;
            installedVisual.transform.localRotation = Quaternion.identity;
        }

        /// <summary>Call from existing mouse/touch selection or component placement tool after real placement.</summary>
        public bool CommitFromExistingPlacementTool(out string reason)
        {
            if (isReferenceArt || levelRuntime == null) { reason = "Reference-only or unbound target"; return false; }
            return levelRuntime.TryCommitPart(partId, this, out reason);
        }
    }

    public sealed class SparkAutoLevelRuntime : MonoBehaviour
    {
        [SerializeField] private SparkAutoLevelBlueprint blueprint;
        [SerializeField] private SparkAutoLevelAuthorityBridge authorityBridge;
        [SerializeField] private bool isReferenceOnly;
        private readonly HashSet<string> installed = new HashSet<string>(StringComparer.Ordinal);
        private readonly HashSet<string> completedRoutes = new HashSet<string>(StringComparer.Ordinal);
        private readonly HashSet<string> completedVias = new HashSet<string>(StringComparer.Ordinal);
        private readonly HashSet<string> passedObjectives = new HashSet<string>(StringComparer.Ordinal);
        private bool safetyResetConfirmed;

        public SparkAutoLevelBlueprint Blueprint => blueprint;
        public bool IsReferenceOnly => isReferenceOnly;
        public bool CanCertify => !isReferenceOnly && authorityBridge != null;
        public int InstalledCount => installed.Count;
        public int CompletedRouteCount => completedRoutes.Count;
        public int CompletedViaCount => completedVias.Count;
        public int PassedObjectiveCount => passedObjectives.Count;
        public bool SafetyResetConfirmed => safetyResetConfirmed;
        public event Action ProgressChanged;

        public void Configure(SparkAutoLevelBlueprint data, bool referenceOnly)
        {
            blueprint = data;
            isReferenceOnly = referenceOnly;
            installed.Clear(); completedRoutes.Clear(); completedVias.Clear(); passedObjectives.Clear();
            safetyResetConfirmed = false;
        }

        /// <summary>Explicit integration only. Assign an adapter that delegates to real Project Spark systems.</summary>
        public void BindAuthority(SparkAutoLevelAuthorityBridge bridge) { authorityBridge = bridge; }

        public bool TryCommitPart(string id, SparkAutoPlacementTarget target, out string reason)
        {
            if (!CheckBridge(out reason)) return false;
            SparkAutoPart part;
            if (!blueprint.TryGetPart(id, out part) || !part.Installable || target == null ||
                !string.Equals(target.PartId, id, StringComparison.Ordinal) || target.IsReferenceArt)
            { reason = "Invalid installable part or placement target"; return false; }
            if (installed.Contains(id)) { reason = "Already installed"; return false; }
            if (!authorityBridge.TryCommitPart(part, target, out reason)) return false;
            installed.Add(id); target.ShowApprovedVisual(); ProgressChanged?.Invoke(); return true;
        }

        public bool TryCommitRoute(string id, out string reason)
        {
            if (!CheckBridge(out reason)) return false;
            SparkAutoRoute route;
            if (!blueprint.TryGetRoute(id, out route)) { reason = "Unknown route"; return false; }
            if (!route.HasSchematicFields(blueprint.CopperLayers)) { reason = "SCHEMATIC_REQUIRED: unapproved route pins/net/layer"; return false; }
            if (completedRoutes.Contains(id)) { reason = "Already completed"; return false; }
            if (!authorityBridge.TryCommitRoute(route, out reason)) return false;
            completedRoutes.Add(id); ProgressChanged?.Invoke(); return true;
        }

        public bool TryCommitVia(string id, out string reason)
        {
            if (!CheckBridge(out reason)) return false;
            SparkAutoVia via;
            if (!blueprint.TryGetVia(id, out via)) { reason = "Unknown via"; return false; }
            if (!via.HasSchematicFields(blueprint.CopperLayers)) { reason = "SCHEMATIC_REQUIRED: unapproved via net/layer span"; return false; }
            if (completedVias.Contains(id)) { reason = "Already completed"; return false; }
            if (!authorityBridge.TryCommitVia(via, out reason)) return false;
            completedVias.Add(id); ProgressChanged?.Invoke(); return true;
        }

        public bool TryConfirmSafetyReset(out string reason)
        {
            if (!CheckBridge(out reason)) return false;
            if (blueprint.Safety == null || !blueprint.Safety.HasApprovedPhysicalPins)
            { reason = "SCHEMATIC_REQUIRED: safety contact pins unverified"; return false; }
            if (!authorityBridge.TryConfirmSafetyReset(blueprint.Safety, out reason)) return false;
            safetyResetConfirmed = true; ProgressChanged?.Invoke(); return true;
        }

        public bool TryAwardObjective(string id, out string reason)
        {
            if (!CheckBridge(out reason)) return false;
            SparkAutoObjective found = null;
            var objectives = blueprint.Objectives;
            for (int i = 0; i < objectives.Count; i++)
                if (string.Equals(objectives[i].Id, id, StringComparison.Ordinal)) { found = objectives[i]; break; }
            if (found == null) { reason = "Unknown objective"; return false; }
            if (passedObjectives.Contains(id)) { reason = "Already awarded"; return false; }
            // Structural guards in addition to the real gameplay manager's full objective criteria.
            if (id == "01" && installed.Count != blueprint.InstallableCount) { reason = "Assembly is incomplete"; return false; }
            if (id == "02" && (blueprint.Routes.Count == 0 || blueprint.Vias.Count == 0 ||
                completedRoutes.Count != blueprint.Routes.Count || completedVias.Count != blueprint.Vias.Count))
            { reason = "Electrical construction incomplete"; return false; }
            if (id == "06" && (!safetyResetConfirmed || blueprint.Routes.Count == 0 || blueprint.Vias.Count == 0 ||
                completedRoutes.Count != blueprint.Routes.Count || completedVias.Count != blueprint.Vias.Count))
            { reason = "Independent safety/electrical verification incomplete"; return false; }
            if (!authorityBridge.TryConfirmObjective(found, out reason)) return false;
            passedObjectives.Add(id); ProgressChanged?.Invoke(); return true;
        }

        private bool CheckBridge(out string reason)
        {
            if (blueprint == null) { reason = "Missing level blueprint"; return false; }
            if (isReferenceOnly) { reason = "Completed reference art has no gameplay authority"; return false; }
            if (authorityBridge == null) { reason = "AUTHORITY_BRIDGE_REQUIRED: bind actual Project Spark systems"; return false; }
            reason = null; return true;
        }
    }

    public sealed class SparkAutoGeneratedMarker : MonoBehaviour
    {
        [SerializeField] private int levelId;
        [SerializeField] private bool referenceOnly;
        [SerializeField] private string generatorVersion = "2.0.0";
        public int LevelId => levelId;
        public bool ReferenceOnly => referenceOnly;
        public void Configure(int id, bool reference) { levelId = id; referenceOnly = reference; }
    }
}
