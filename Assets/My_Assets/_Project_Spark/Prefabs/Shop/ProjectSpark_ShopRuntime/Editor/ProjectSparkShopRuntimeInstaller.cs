#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using ProjectSpark.UI;

namespace ProjectSpark.EditorTools
{
    public static class ProjectSparkShopRuntimeInstaller
    {
        [MenuItem("Tools/ProjectSpark/UI/Attach Runtime To Selected Shop Panel")]
        private static void Attach()
        {
            GameObject selected = Selection.activeGameObject;
            if (selected == null || selected.transform.Find("Backdrop/ShopWindow") == null)
            {
                EditorUtility.DisplayDialog("ProjectSpark Shop", "Select the generated ProjectSparkShopPanel root in the Hierarchy first.", "OK");
                return;
            }
            ProjectSparkShopRuntime existing = selected.GetComponent<ProjectSparkShopRuntime>();
            if (existing == null) Undo.AddComponent<ProjectSparkShopRuntime>(selected);
            EditorUtility.SetDirty(selected);
            Debug.Log("[ProjectSpark] Runtime shop binder attached to " + selected.name + ". Assign SparkEconomyCatalog on the EconomyRoot and enter Play Mode.", selected);
        }
        [MenuItem("Tools/ProjectSpark/UI/Attach Runtime To Selected Shop Panel", true)]
        private static bool ValidateAttach() => Selection.activeGameObject != null;
    }
}
#endif
