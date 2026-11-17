#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace ProjectSpark.PCBVirtualStudio.Editor
{
    public static class SparkPCBStudioShaderInstaller
    {
        private const string Root = "Assets/ProjectSpark/PCBVirtualStudio";
        private const string Generated = Root + "/Generated";
        private const string MaterialPath = Generated + "/Spark_AAA_Holographic_UI.mat";

        [MenuItem("Tools/Project Spark/PCB Virtual Studio/4. Build + Apply AAA Hologram Material")]
        public static void InstallForSelected()
        {
            var root = Selection.activeGameObject;
            if (root == null || root.GetComponent<SparkVirtualPCBWorkstation>() == null)
            {
                Debug.LogWarning("[Spark PCB UI] Select the generated studio root first.");
                return;
            }
            Shader shader = Shader.Find("ProjectSpark/PCB/AAA Hologram UI");
            if (shader == null)
            {
                Debug.LogWarning("[Spark PCB UI] Optional hologram shader missing; UGUI default materials remain fully usable.");
                return;
            }
            if (!AssetDatabase.IsValidFolder(Generated))
            {
                if (!AssetDatabase.IsValidFolder(Root))
                {
                    Debug.LogError("[Spark PCB UI] Install scripts under " + Root + " and re-run material generation.");
                    return;
                }
                AssetDatabase.CreateFolder(Root, "Generated");
            }
            Material mat = AssetDatabase.LoadAssetAtPath<Material>(MaterialPath);
            if (mat == null)
            {
                mat = new Material(shader);
                mat.name = "Spark_AAA_Holographic_UI";
                mat.SetColor("_ScanColor", new Color(.08f,.77f,.94f,1f));
                mat.SetFloat("_ScanlineStrength", .085f);
                mat.SetFloat("_ScanlineDensity", 65f);
                mat.SetFloat("_ScanlineSpeed", .15f);
                AssetDatabase.CreateAsset(mat, MaterialPath);
            }
            string[] selectedNames = {
                "HeaderPanel", "VirtualToolDeck", "PCB_DigitalTwinPanel", "VirtualInstrumentDeck",
                "FooterStatusRail", "BOARD_UI_ONLY", "ScopeFrame"
            };
            int applied = 0;
            var graphics = root.GetComponentsInChildren<SparkPCBRoundedPanelGraphic>(true);
            foreach (var g in graphics)
            {
                foreach (string n in selectedNames)
                {
                    if (g.name != n) continue;
                    Undo.RecordObject(g, "Apply Project Spark hologram UI material");
                    g.material = mat;
                    EditorUtility.SetDirty(g);
                    applied++;
                    break;
                }
            }
            AssetDatabase.SaveAssets();
            Debug.Log("[Spark PCB UI] Applied optional holo material to " + applied +
                " surfaces. This is visual-only; no electrical state is changed.");
        }
    }
}
#endif
