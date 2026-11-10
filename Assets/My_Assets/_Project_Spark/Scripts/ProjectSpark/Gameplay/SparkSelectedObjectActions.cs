using ProjectSpark.Gameplay.Shelf;
using UnityEngine;

namespace ProjectSpark.Gameplay
{
    // Authorization for player shelf drag/throw/recycle only. No scene component required.
    public static class SparkSelectedObjectActions
    {
        private static SparkToolController tools;
        private static SparkSelectionController selection;
        private static float nextSearch;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void Reset() { tools = null; selection = null; nextSearch = 0; }
        private static void Resolve()
        {
            if (Time.unscaledTime < nextSearch && tools != null && selection != null) return;
            nextSearch = Time.unscaledTime + .25f;
            tools = Object.FindFirstObjectByType<SparkToolController>();
            selection = Object.FindFirstObjectByType<SparkSelectionController>();
        }
        public static bool SelectToolEnabled
        {
            get
            {
                Resolve();
                return tools != null && tools.isActiveAndEnabled && tools.ActiveTool != null &&
                    tools.ActiveTool.isActiveAndEnabled && tools.ActiveToolType == SparkToolType.Select &&
                    !AAAUI.VFX.SignalWireBuilder.AnyWireDrawing;
            }
        }
        public static bool IsSelected(SparkShelfItem item)
        {
            Resolve();
            if (item == null || selection == null || !selection.isActiveAndEnabled) return false;
            var selected = selection.SelectedObject;
            return selected != null && selected.isActiveAndEnabled &&
                (selected.transform == item.transform || selected.GetComponentInParent<SparkShelfItem>() == item);
        }
        public static bool CanAct(SparkShelfItem item) => SelectToolEnabled && IsSelected(item) &&
            item.isActiveAndEnabled && item.AllowTouch && !item.IsMoving;

        public static bool TrySelectItem(SparkShelfItem item)
        {
            if (!SelectToolEnabled || item == null || !item.isActiveAndEnabled || !item.AllowTouch || item.IsMoving) return false;
            if (IsSelected(item)) return true;
            if (selection == null || !selection.isActiveAndEnabled) return false;
            var selectable = item.GetComponent<SparkSelectable>();
            if (selectable == null) selectable = item.GetComponentInChildren<SparkSelectable>();
            if (selectable == null || !selectable.isActiveAndEnabled) return false;
            selection.Select(selectable);
            return IsSelected(item);
        }
    }
}
