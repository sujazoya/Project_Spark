using UnityEngine;
using ProjectSpark.Electrical;
using ProjectSpark.Gameplay.Shelf;

namespace ProjectSpark.Gameplay
{
    // Dedicated multi-component binding: the generic two-pin bridge skips this slot.
    [DisallowMultipleComponent]
    [RequireComponent(typeof(SparkShelfSlot))]
    public sealed class SparkSpdtSlotBinding : MonoBehaviour
    {
        [SerializeField] private string commonToAId = "SPDT_COM_A";
        [SerializeField] private string commonToBId = "SPDT_COM_B";
        [SerializeField] private string commonId = "SPDT_COM";
        [SerializeField] private string aId = "SPDT_A";
        [SerializeField] private string bId = "SPDT_B";
        [SerializeField] private SparkElectricalSolver solver;
        [Tooltip("For the supplied Slot_06, which has no SparkShelfItem. Leave empty for shelf-managed removable items.")]
        [SerializeField] private SparkSpdtDualSwitchRouter fixedInstalledRouter;
        private SparkShelfSlot slot;
        private SparkLevelLayout layout;
        private SparkLevelLayoutLoader loader;
        private SparkLevelSceneBindings applied;
        private SparkSpdtDualSwitchRouter bound;
        private float nextSync;
        private bool warned;

        private void OnEnable()
        {
            slot = GetComponent<SparkShelfSlot>();
            slot.BindingRefreshRequested += Refresh;
            nextSync = 0;
        }
        private void OnDisable()
        {
            if (slot != null) slot.BindingRefreshRequested -= Refresh;
            Clear();
        }
        private void Refresh(SparkShelfSlot _) => Synchronize();
        private void LateUpdate()
        {
            if (Time.unscaledTime < nextSync) return;
            nextSync = Time.unscaledTime + .1f;
            Synchronize();
        }
        private bool Declared(SparkLevelSceneBindings b)
        {
            return !string.IsNullOrWhiteSpace(commonToAId) &&
                !string.IsNullOrWhiteSpace(commonToBId) && commonToAId != commonToBId &&
                !string.IsNullOrWhiteSpace(commonId) && !string.IsNullOrWhiteSpace(aId) &&
                !string.IsNullOrWhiteSpace(bId) && commonId != aId && commonId != bId && aId != bId &&
                b.IsComponentIdDeclared(commonToAId) && b.IsComponentIdDeclared(commonToBId) &&
                b.IsTerminalIdDeclared(commonId) && b.IsTerminalIdDeclared(aId) && b.IsTerminalIdDeclared(bId);
        }
        [ContextMenu("SPDT / Synchronize")]
        public void Synchronize()
        {
            if (!Application.isPlaying || !isActiveAndEnabled) return;
            if (slot == null) slot = GetComponent<SparkShelfSlot>();
            if (layout == null) layout = GetComponentInParent<SparkLevelLayout>();
            if (loader == null) loader = FindFirstObjectByType<SparkLevelLayoutLoader>();
            var bindings = loader != null && layout != null && loader.ActiveLayout == layout
                ? loader.ActiveBindings : null;
            if (bindings == null) { Clear(); return; }
            if (!Declared(bindings))
            {
                Clear();
                if (!warned) Debug.LogError("[SPDT] Declare two unique component IDs and three unique terminal IDs in active Scene Bindings.", this);
                warned = true;
                return;
            }
            warned = false;
            var item = slot.isActiveAndEnabled ? slot.InstalledItem : null;
            SparkSpdtDualSwitchRouter router = null;
            if (item != null && item.isActiveAndEnabled && item.IsAtWorkbench)
            {
                var routers = item.GetComponentsInChildren<SparkSpdtDualSwitchRouter>(true);
                if (routers.Length == 1 && routers[0].isActiveAndEnabled && routers[0].IsReady)
                    router = routers[0];
            }
            else if (slot.isActiveAndEnabled && slot.Occupant == null &&
                fixedInstalledRouter != null && fixedInstalledRouter.isActiveAndEnabled &&
                fixedInstalledRouter.transform.IsChildOf(slot.transform) && fixedInstalledRouter.IsReady)
                router = fixedInstalledRouter;
            if (applied == bindings && bound == router) return;
            Clear();
            applied = bindings;
            bound = router;
            Write(bindings, router);
            Dirty();
        }
        private void Write(SparkLevelSceneBindings b, SparkSpdtDualSwitchRouter r)
        {
            b.SetRuntimeComponentBinding(commonToAId, r != null ? r.CommonToA : null);
            b.SetRuntimeComponentBinding(commonToBId, r != null ? r.CommonToB : null);
            b.SetRuntimeTerminal(commonId, r != null ? r.Common : null);
            b.SetRuntimeTerminal(aId, r != null ? r.A : null);
            b.SetRuntimeTerminal(bId, r != null ? r.B : null);
        }
        private void Clear()
        {
            if (applied == null) { bound = null; return; }
            Write(applied, null);
            applied = null;
            bound = null;
            Dirty();
        }
        private void Dirty()
        {
            if (solver == null) solver = FindFirstObjectByType<SparkElectricalSolver>();
            if (solver != null) solver.MarkTopologyDirty();
            var gameplay = FindFirstObjectByType<LevelGamePlayManager>();
            if (gameplay != null) gameplay.QueueEvaluation();
        }
        [ContextMenu("SPDT / Audit Live Graph")]
        public void AuditLiveGraph()
        {
            Synchronize();
            var g = solver != null ? solver.CurrentGraph : null;
            bool devices = false;
            if (solver != null && bound != null)
            {
                bool a = false, b = false;
                foreach (var c in solver.ElectricalComponents)
                { a |= c == bound.CommonToA; b |= c == bound.CommonToB; }
                devices = a && b;
            }
            bool valid = bound != null && g != null && devices &&
                g.ComponentTerminals.ContainsKey(bound.CommonToA) &&
                g.ComponentTerminals.ContainsKey(bound.CommonToB) &&
                g.ContainsTerminal(bound.Common) && g.ContainsTerminal(bound.A) && g.ContainsTerminal(bound.B);
            if (valid) Debug.Log("[SPDT] PASS: both devices and all three terminals are in the live graph. Check branch states and measured current separately.", this);
            else Debug.LogError("[SPDT] Graph audit failed or graph not built yet. Check installed item, bindings, terminal parenting, solver search root and solve once before retrying.", this);
        }
    }
}
