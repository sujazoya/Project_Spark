#!/usr/bin/env python3
"""Write OPTIONAL Unity art-only helpers, not circuit or gameplay systems."""
from pathlib import Path
from build_level87 import NAMES,PIVOTS
ROOT=Path(__file__).resolve().parent.parent
DST=ROOT/'Unity_Optional'
labels='\n'.join('            "'+n+'",' for n in NAMES)
pivots='\n'.join('            "'+r['Pivot']+'",' for r in PIVOTS)
placement=r'''#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

namespace ProjectSpark.EditorTools
{
    /// <summary>Level 87 visual-art importer ONLY. Does not generate playable circuits,
    /// actual SparkTerminals, bearing physics, ring dynamics or gameplay objectives.</summary>
    public static class SparkLevel87ArtPlacementEditor
    {
        public const string Folder = "Assets/ProjectSpark/Level87/Models/";
        public const string VisualRoot = "L87_HD_VISUAL_ART";
        public static readonly string[] ModelNames = new string[]
        {
__MODEL_NAMES__
        };

        [MenuItem("Tools/Project Spark/Levels/87/Place HD Art Under Selected Layout")]
        private static void PlaceArt()
        {
            GameObject parent = Selection.activeGameObject;
            if (parent == null)
            {
                EditorUtility.DisplayDialog("Level 87", "Select the existing Level 87 layout root GameObject in the Hierarchy first.", "OK");
                return;
            }
            var source = new GameObject[ModelNames.Length];
            var missing = new List<string>();
            for (int i = 0; i < source.Length; i++)
            {
                string path = Folder + i.ToString("00") + "_" + ModelNames[i] + ".fbx";
                source[i] = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (source[i] == null) missing.Add(path);
            }
            if (missing.Count > 0)
            {
                Debug.LogError("[Level87Art] Missing FBX: " + string.Join("; ", missing));
                EditorUtility.DisplayDialog("Level 87", "Missing " + missing.Count + " FBX files. Import all 86 numbered FBXs first.", "OK");
                return;
            }
            if (!EditorUtility.DisplayDialog("Level 87 Modular Art",
                "Replace ONLY the art root '" + VisualRoot + "' under selected layout? Actual gameplay children outside it remain intact.",
                "Place Art", "Cancel")) return;
            var oldRoot = parent.transform.Find(VisualRoot);
            if (oldRoot != null) Undo.DestroyObjectImmediate(oldRoot.gameObject);
            var visual = new GameObject(VisualRoot);
            Undo.RegisterCreatedObjectUndo(visual, "Create Level 87 art root");
            Undo.SetTransformParent(visual.transform, parent.transform, "Parent Level 87 art root");
            visual.transform.localPosition = Vector3.zero;
            visual.transform.localRotation = Quaternion.identity;
            visual.transform.localScale = Vector3.one;
            int placed = 0;
            for (int i = 0; i < source.Length; i++)
            {
                GameObject inst = PrefabUtility.InstantiatePrefab(source[i], visual.transform) as GameObject;
                if (inst == null)
                {
                    Debug.LogError("[Level87Art] Could not instantiate " + ModelNames[i]);
                    continue;
                }
                Undo.RegisterCreatedObjectUndo(inst, "Place Level 87 modular model");
                inst.name = ModelNames[i];
                inst.transform.localPosition = Vector3.zero;
                inst.transform.localRotation = Quaternion.identity;
                inst.transform.localScale = Vector3.one;
                placed++;
            }
            EditorUtility.SetDirty(parent);
            if (parent.scene.IsValid()) EditorSceneManager.MarkSceneDirty(parent.scene);
            Debug.Log("[Level87Art] Placed " + placed + "/86 common-origin modular FBXs. No gameplay was generated. Check metres-scale bounds before playing.");
            Selection.activeGameObject = visual;
        }
    }
}
#endif
'''.replace('__MODEL_NAMES__',labels)
(DST/'SparkLevel87ArtPlacementEditor.cs').write_text(placement)
validator=r'''#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace ProjectSpark.EditorTools
{
    /// <summary>Art structure validator. NOT proof of runtime, wiring, or gameplay correctness.</summary>
    public static class SparkLevel87AssetValidatorEditor
    {
        public static readonly string[] Pivots = new string[]
        {
__PIVOTS__
        };
        [MenuItem("Tools/Project Spark/Levels/87/Validate Imported HD Art")]
        public static void Validate()
        {
            int found = 0;
            for (int i = 0; i < SparkLevel87ArtPlacementEditor.ModelNames.Length; i++)
            {
                string path = SparkLevel87ArtPlacementEditor.Folder + i.ToString("00") + "_" +
                    SparkLevel87ArtPlacementEditor.ModelNames[i] + ".fbx";
                if (AssetDatabase.LoadAssetAtPath<GameObject>(path) != null) found++;
                else Debug.LogError("[Level87Validator] MISSING " + path);
            }
            Transform root = null;
            if (Selection.activeGameObject != null)
            {
                root = Selection.activeGameObject.transform;
                if (root.name != SparkLevel87ArtPlacementEditor.VisualRoot)
                    root = root.Find(SparkLevel87ArtPlacementEditor.VisualRoot);
            }
            if (root == null)
            {
                Debug.LogWarning("[Level87Validator] 3D art root not selected. Checked FBX imports only: " + found + "/86.");
                return;
            }
            var pivotCounts = new Dictionary<string,int>();
            var socketSet = new HashSet<string>();
            int rendererCount = 0;
            foreach (Transform t in root.GetComponentsInChildren<Transform>(true))
            {
                if (t.name.StartsWith("ANIM_"))
                {
                    int old;
                    pivotCounts.TryGetValue(t.name, out old);
                    pivotCounts[t.name] = old + 1;
                }
                if (t.name.StartsWith("SOCKET_P")) socketSet.Add(t.name);
            }
            rendererCount = root.GetComponentsInChildren<MeshRenderer>(true).Length;
            int properPivots = 0;
            foreach (string p in Pivots)
            {
                int count;
                if (pivotCounts.TryGetValue(p, out count) && count == 1) properPivots++;
                else Debug.LogError("[Level87Validator] Expected exactly one " + p + " but got " + (pivotCounts.ContainsKey(p) ? pivotCounts[p] : 0));
            }
            if (socketSet.Count != 236) Debug.LogWarning("[Level87Validator] Found " + socketSet.Count + "/236 DECORATIVE sockets. None are functional SparkTerminals.");
            Debug.Log("[Level87Validator] FBX: " + found + "/86, pivots: " + properPivots + "/68, decorative socket ids: " + socketSet.Count + "/236, mesh renderers: " + rendererCount + ". Unity gameplay/Play Mode: UNTESTED.");
        }
    }
}
#endif
'''.replace('__PIVOTS__',pivots)
(DST/'SparkLevel87AssetValidatorEditor.cs').write_text(validator)
materials=r'''#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace ProjectSpark.EditorTools
{
    /// <summary>Optional stylized PBR material-color assignment only.
    /// Procedural PNG reference maps need UVs and explicit shader-channel mapping.</summary>
    public static class SparkLevel87MaterialUpgradeEditor
    {
        [MenuItem("Tools/Project Spark/Levels/87/Create & Apply HD Materials To Selected Art")]
        public static void Upgrade()
        {
            var target = Selection.activeGameObject;
            if (target == null)
            {
                EditorUtility.DisplayDialog("Level 87", "Select the L87_HD_VISUAL_ART GameObject.", "OK");
                return;
            }
            const string folder = "Assets/ProjectSpark/Level87/Materials";
            if (!AssetDatabase.IsValidFolder("Assets/ProjectSpark")) AssetDatabase.CreateFolder("Assets", "ProjectSpark");
            if (!AssetDatabase.IsValidFolder("Assets/ProjectSpark/Level87")) AssetDatabase.CreateFolder("Assets/ProjectSpark", "Level87");
            if (!AssetDatabase.IsValidFolder(folder)) AssetDatabase.CreateFolder("Assets/ProjectSpark/Level87", "Materials");
            var colors = new Dictionary<string,Color>()
            {
                {"structural", new Color(.35f,.46f,.57f)}, {"titanium", new Color(.63f,.70f,.76f)},
                {"dark",new Color(.060f,.078f,.12f)}, {"graphite",new Color(.12f,.15f,.20f)},
                {"copper",new Color(.83f,.39f,.15f)}, {"gold",new Color(.95f,.69f,.22f)},
                {"ceramic",new Color(.85f,.88f,.88f)}, {"glass",new Color(.18f,.61f,.86f,.48f)},
                {"panel",new Color(.025f,.115f,.21f)}, {"pcb",new Color(.04f,.22f,.17f)},
                {"cyan",new Color(.16f,.83f,.96f)}, {"blue",new Color(.18f,.39f,.85f)},
                {"green",new Color(.10f,.91f,.56f)}, {"amber",new Color(1f,.59f,.13f)},
                {"violet",new Color(.64f,.43f,.96f)}, {"red",new Color(1f,.16f,.14f)},
                {"orange",new Color(1f,.38f,.07f)}, {"white",new Color(.95f,.96f,.94f)},
                {"magnet",new Color(.21f,.26f,.34f)}, {"stars",new Color(.77f,.87f,1f)},
                {"black",new Color(.025f,.03f,.04f)}
            };
            Shader shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null) shader = Shader.Find("HDRP/Lit");
            if (shader == null) shader = Shader.Find("Standard");
            if (shader == null) { Debug.LogError("[Level87Materials] No supported Lit shader found."); return; }
            var map = new Dictionary<string,Material>();
            foreach (var entry in colors)
            {
                string path = folder + "/PS_" + entry.Key + ".mat";
                Material mat = AssetDatabase.LoadAssetAtPath<Material>(path);
                if (mat == null) { mat = new Material(shader); AssetDatabase.CreateAsset(mat, path); }
                mat.color = entry.Value;
                if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", entry.Value);
                if (mat.HasProperty("_Color")) mat.SetColor("_Color", entry.Value);
                if (mat.HasProperty("_Metallic")) mat.SetFloat("_Metallic", entry.Key=="gold"||entry.Key=="titanium"||entry.Key=="copper"? .85f : .18f);
                if (mat.HasProperty("_Smoothness")) mat.SetFloat("_Smoothness", entry.Key=="gold"||entry.Key=="titanium" ? .68f : .38f);
                EditorUtility.SetDirty(mat);
                map[entry.Key] = mat;
            }
            int replaced = 0;
            foreach (Renderer r in target.GetComponentsInChildren<Renderer>(true))
            {
                var mats = r.sharedMaterials;
                for (int j=0; j<mats.Length; j++)
                {
                    string name = mats[j] != null ? mats[j].name.Replace(" (Instance)", "").Replace("PS_", "") : "";
                    Material match;
                    if (map.TryGetValue(name, out match)) { mats[j] = match; replaced++; }
                }
                Undo.RecordObject(r, "Apply Level 87 reference materials");
                r.sharedMaterials = mats;
                EditorUtility.SetDirty(r);
            }
            AssetDatabase.SaveAssets();
            Debug.Log("[Level87Materials] Assigned " + replaced + " material slots. This is COLOR/PBR preview; authored UV and final AAA bakes are not included.");
        }
    }
}
#endif
'''
(DST/'SparkLevel87MaterialUpgradeEditor.cs').write_text(materials)
# Visual rig: reads external measured state only. Reparents robot joints so visual kinematics are a real serial chain.
rig=r'''using System.Collections.Generic;
using UnityEngine;

namespace ProjectSpark.Visuals
{
    /// <summary>
    /// VISUAL ONLY. The actual seven-axis gravity ring, magnetic bearing, motor and independent safety logic must
    /// exist in Project Spark's gameplay. Never use pivot transforms as certification evidence.
    /// Optional script: move seven modular pivot transforms into serial hierarchy at runtime.
    /// </summary>
    public sealed class SparkLevel87VisualRig : MonoBehaviour
    {
        [SerializeField] private Transform artRoot;
        [SerializeField] private bool bindAutomatically = true;
        [SerializeField] private Light contactFeedbackLamp;
        [SerializeField] private Light thermalHealthyLamp;
        [SerializeField] private Light criticalFaultLamp;
        private readonly Dictionary<string,Transform> pivots = new Dictionary<string,Transform>();
        private readonly Dictionary<string,Quaternion> homeRotations = new Dictionary<string,Quaternion>();
        private readonly Dictionary<string,Vector3> homePositions = new Dictionary<string,Vector3>();
        private static readonly string[] JointNames = {
            "ANIM_ROBOT_BASE_ROTATE", "ANIM_ROBOT_SHOULDER_PITCH", "ANIM_ROBOT_UPPERARM_PITCH",
            "ANIM_ROBOT_ELBOW_PITCH", "ANIM_ROBOT_FOREARM_ROLL", "ANIM_ROBOT_WRIST_PITCH",
            "ANIM_ROBOT_WRIST_ROLL"};
        private static readonly Vector3[] JointAxes = {
            Vector3.up, Vector3.forward, Vector3.forward, Vector3.forward,
            Vector3.right, Vector3.forward, Vector3.right};
        private bool chainBound;
        private float[] angles = new float[7];
        private float railX;
        private float gripperOpen;
        private float coolantValveDegrees;
        private float failedRelease;
        private float replacementLift;
        private bool w129Trusted;
        private bool criticalFault;
        private bool coolingQualified;

        private void Awake()
        {
            if (artRoot == null) artRoot = transform.Find("L87_HD_VISUAL_ART");
            if (artRoot == null) artRoot = transform;
            if (bindAutomatically) BindVisuals();
        }

        public void BindVisuals()
        {
            pivots.Clear();homeRotations.Clear();homePositions.Clear();
            foreach (Transform t in artRoot.GetComponentsInChildren<Transform>(true))
            {
                if (t.name.StartsWith("ANIM_") && !pivots.ContainsKey(t.name)) pivots[t.name]=t;
            }
            if (!chainBound)
            {
                Transform previous = Find("ANIM_LINEAR_RAIL_CARRIAGE");
                Transform robotBase = artRoot.Find("L87_SevenAxisRobotBase");
                if (previous != null && robotBase != null) robotBase.SetParent(previous, true);
                for (int i=0; i<JointNames.Length; i++)
                {
                    Transform node = Find(JointNames[i]);
                    if (node == null) continue;
                    if (previous != null && !node.IsChildOf(previous)) node.SetParent(previous, true);
                    previous=node;
                }
                chainBound=true;
            }
            foreach (var pair in pivots)
            {
                homeRotations[pair.Key]=pair.Value.localRotation;
                homePositions[pair.Key]=pair.Value.localPosition;
            }
        }
        private Transform Find(string n) { Transform t; return pivots.TryGetValue(n,out t)?t:null; }
        private void Rotate(string n,Vector3 axis,float deg)
        {
            Transform t=Find(n);Quaternion q;
            if (t!=null && homeRotations.TryGetValue(n,out q)) t.localRotation=q*Quaternion.AngleAxis(deg,axis);
        }
        private void Translate(string n,Vector3 offset)
        {
            Transform t=Find(n);Vector3 pos;
            if (t!=null && homePositions.TryGetValue(n,out pos)) t.localPosition=pos+offset;
        }
        /// <summary>Set measured ACTUAL joint positions in DEGREES and rail displacement in METRES.</summary>
        public void SetActualRobotJointAngles(float j1,float j2,float j3,float j4,float j5,float j6,float j7,float railMeters)
        {
            angles[0]=j1;angles[1]=j2;angles[2]=j3;angles[3]=j4;angles[4]=j5;angles[5]=j6;angles[6]=j7;
            railX=Mathf.Clamp(railMeters,-2.5f,2.5f);
        }
        /// <summary>Normalized actual jaw opening, panel release, lift and actual coolant valve angle.</summary>
        public void SetActualServicingMechanisms(float jawOpen01,float releasedPanel01,float liftPanel01,float actualCoolantValveDegrees)
        {
            gripperOpen=Mathf.Clamp01(jawOpen01);
            failedRelease=Mathf.Clamp01(releasedPanel01);
            replacementLift=Mathf.Clamp01(liftPanel01);
            coolantValveDegrees=Mathf.Clamp(actualCoolantValveDegrees,0,90);
        }
        public void SetActualSafetyState(bool trustedW129Fresh,bool criticalFaultLatched,bool radiatorCoolingQualified)
        {
            w129Trusted=trustedW129Fresh;criticalFault=criticalFaultLatched;coolingQualified=radiatorCoolingQualified;
        }
        private void LateUpdate()
        {
            if (pivots.Count==0 || !chainBound) return;
            Translate("ANIM_LINEAR_RAIL_CARRIAGE",Vector3.right*railX);
            for(int i=0;i<7;i++) Rotate(JointNames[i],JointAxes[i],angles[i]);
            Translate("ANIM_GRIPPER_JAW_LEFT",Vector3.forward *(.08f*gripperOpen));
            Translate("ANIM_GRIPPER_JAW_RIGHT",Vector3.back *(.08f*gripperOpen));
            Translate("ANIM_FAILED_RADIATOR_RELEASE",Vector3.right*(.25f*failedRelease));
            Translate("ANIM_REPLACEMENT_RADIATOR_LIFT",Vector3.up*(.35f*replacementLift));
            Rotate("ANIM_COOLANT_VALVE",Vector3.right,coolantValveDegrees);
            if (contactFeedbackLamp!=null) contactFeedbackLamp.enabled=w129Trusted && !criticalFault;
            if (thermalHealthyLamp!=null) thermalHealthyLamp.enabled=coolingQualified && !criticalFault;
            if (criticalFaultLamp!=null) criticalFaultLamp.enabled=criticalFault;
        }
    }
}
'''
(DST/'SparkLevel87VisualRig.cs').write_text(rig)
print('CREATED',len(list(DST.glob('*.cs'))),'Unity helper .cs files')
