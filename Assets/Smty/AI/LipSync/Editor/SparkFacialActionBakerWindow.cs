
#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using ProjectSpark.AI.LipSync;
using UnityEditor;
using UnityEngine;

namespace ProjectSpark.AI.LipSync.Editor
{
    /// <summary>
    /// Samples imported facial animation clips and stores local bone
    /// rotations in a SparkFacialPoseLibrary.
    ///
    /// Responsibilities:
    /// - Validate the model asset and scene bone mappings.
    /// - Create an isolated temporary model for animation sampling.
    /// - Sample each assigned facial animation clip.
    /// - Capture local rotations in a deterministic bone order.
    /// - Validate duplicate visemes and missing clips.
    /// - Update the pose library and save the asset.
    ///
    /// This editor utility does not modify the original scene model.
    /// </summary>
    public sealed class SparkFacialActionBakerWindow : EditorWindow
    {
        [Serializable]
        private sealed class Mapping
        {
            public SparkViseme viseme;
            public AnimationClip clip;

            [Min(0f)]
            public float sampleTime;
        }

        [SerializeField]
        private GameObject modelPrefab;

        [SerializeField]
        private Transform sourceModelRoot;

        [SerializeField]
        private Transform[] facialBones = Array.Empty<Transform>();

        [SerializeField]
        private SparkFacialPoseLibrary outputLibrary;

        [SerializeField]
        private Mapping[] mappings = CreateMappings();

        [SerializeField]
        private bool skipUnassignedClips = true;

        [SerializeField]
        private bool showAdvancedOptions;

        private Vector2 scrollPosition;

        [MenuItem("Project Spark/AI/Import Facial Pose Actions")]
        private static void Open()
        {
            SparkFacialActionBakerWindow window =
                GetWindow<SparkFacialActionBakerWindow>(
                    "Spark Facial Action Baker");

            window.minSize = new Vector2(460f, 560f);
            window.Show();
        }

        private void OnGUI()
        {
            scrollPosition = EditorGUILayout.BeginScrollView(
                scrollPosition);

            DrawIntroduction();
            DrawModelSettings();
            DrawBoneSettings();
            DrawOutputSettings();
            DrawMappingSettings();
            DrawAdvancedSettings();
            DrawBakeButton();

            EditorGUILayout.EndScrollView();
        }

        private static void DrawIntroduction()
        {
            EditorGUILayout.Space(4f);

            EditorGUILayout.HelpBox(
                "Bake facial animation clips into a reusable " +
                "SparkFacialPoseLibrary.\n\n" +
                "1. Assign the imported model asset.\n" +
                "2. Assign a matching instance in the open scene.\n" +
                "3. Select facial bones in a consistent order.\n" +
                "4. Assign the animation clip for each viseme.\n" +
                "5. Choose a sample time where the pose is fully formed.\n" +
                "6. Bake and test the result in Play Mode.\n\n" +
                "The baker records local rotations, not vertex blend " +
                "shapes or facial materials.",
                MessageType.Info);
        }

        private void DrawModelSettings()
        {
            EditorGUILayout.Space(8f);
            EditorGUILayout.LabelField(
                "Model",
                EditorStyles.boldLabel);

            modelPrefab = (GameObject)EditorGUILayout.ObjectField(
                "Model Prefab / FBX",
                modelPrefab,
                typeof(GameObject),
                false);

            sourceModelRoot = (Transform)EditorGUILayout.ObjectField(
                "Scene Model Root",
                sourceModelRoot,
                typeof(Transform),
                true);

            if (modelPrefab != null && sourceModelRoot != null)
            {
                EditorGUILayout.HelpBox(
                    "The scene model is used to define bone paths. " +
                    "Animation clips are sampled on a temporary " +
                    "instance of the model asset.",
                    MessageType.None);
            }
        }

        private void DrawBoneSettings()
        {
            EditorGUILayout.Space(8f);
            EditorGUILayout.LabelField(
                "Facial Bones",
                EditorStyles.boldLabel);

            EditorGUILayout.HelpBox(
                "Bone order is significant. Use exactly the same " +
                "order for every pose. For example: jaw, upper lip, " +
                "lower lip, left mouth corner, right mouth corner.",
                MessageType.None);

            if (facialBones == null)
            {
                facialBones = Array.Empty<Transform>();
            }

            int requestedCount = Mathf.Max(
                0,
                EditorGUILayout.IntField(
                    "Bone Count",
                    facialBones.Length));

            if (requestedCount != facialBones.Length)
            {
                Array.Resize(
                    ref facialBones,
                    requestedCount);
            }

            for (int i = 0; i < facialBones.Length; i++)
            {
                facialBones[i] = (Transform)EditorGUILayout.ObjectField(
                    "Bone " + i,
                    facialBones[i],
                    typeof(Transform),
                    true);
            }

            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("Add Bone"))
            {
                Array.Resize(
                    ref facialBones,
                    facialBones.Length + 1);
            }

            GUI.enabled = facialBones.Length > 0;

            if (GUILayout.Button("Remove Last Bone"))
            {
                Array.Resize(
                    ref facialBones,
                    facialBones.Length - 1);
            }

            GUI.enabled = true;
            EditorGUILayout.EndHorizontal();
        }

        private void DrawOutputSettings()
        {
            EditorGUILayout.Space(8f);
            EditorGUILayout.LabelField(
                "Output",
                EditorStyles.boldLabel);

            outputLibrary = (SparkFacialPoseLibrary)
                EditorGUILayout.ObjectField(
                    "Pose Library",
                    outputLibrary,
                    typeof(SparkFacialPoseLibrary),
                    false);

            if (outputLibrary == null)
            {
                EditorGUILayout.HelpBox(
                    "Create a SparkFacialPoseLibrary asset and assign " +
                    "it here before baking.",
                    MessageType.Warning);
            }
        }

        private void DrawMappingSettings()
        {
            EditorGUILayout.Space(8f);
            EditorGUILayout.LabelField(
                "Animation Clip Mappings",
                EditorStyles.boldLabel);

            if (mappings == null)
            {
                mappings = CreateMappings();
            }

            for (int i = 0; i < mappings.Length; i++)
            {
                if (mappings[i] == null)
                {
                    mappings[i] = new Mapping();
                }

                Mapping mapping = mappings[i];

                EditorGUILayout.BeginVertical(
                    EditorStyles.helpBox);

                mapping.viseme = (SparkViseme)EditorGUILayout.EnumPopup(
                    "Viseme",
                    mapping.viseme);

                mapping.clip = (AnimationClip)EditorGUILayout.ObjectField(
                    "Animation Clip",
                    mapping.clip,
                    typeof(AnimationClip),
                    false);

                using (new EditorGUI.DisabledScope(
                           mapping.clip == null))
                {
                    float maxTime = mapping.clip != null
                        ? Mathf.Max(0f, mapping.clip.length)
                        : 0f;

                    mapping.sampleTime = EditorGUILayout.Slider(
                        "Sample Time (seconds)",
                        Mathf.Clamp(
                            mapping.sampleTime,
                            0f,
                            maxTime),
                        0f,
                        maxTime);
                }

                if (mapping.clip != null)
                {
                    EditorGUILayout.LabelField(
                        "Clip Length",
                        mapping.clip.length.ToString("F3") +
                        " seconds",
                        EditorStyles.miniLabel);
                }

                EditorGUILayout.EndVertical();
            }

            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("Reset Default Mappings"))
            {
                bool confirmed = EditorUtility.DisplayDialog(
                    "Reset Clip Mappings",
                    "Reset all mappings to the default viseme list? " +
                    "Existing clip assignments will be discarded.",
                    "Reset",
                    "Cancel");

                if (confirmed)
                {
                    mappings = CreateMappings();
                }
            }

            if (GUILayout.Button("Add Mapping"))
            {
                Array.Resize(
                    ref mappings,
                    mappings.Length + 1);

                mappings[mappings.Length - 1] = new Mapping();
            }

            EditorGUILayout.EndHorizontal();
        }

        private void DrawAdvancedSettings()
        {
            EditorGUILayout.Space(8f);

            showAdvancedOptions = EditorGUILayout.Foldout(
                showAdvancedOptions,
                "Advanced Options",
                true);

            if (!showAdvancedOptions)
            {
                return;
            }

            EditorGUI.indentLevel++;

            skipUnassignedClips = EditorGUILayout.Toggle(
                "Skip Unassigned Clips",
                skipUnassignedClips);

            EditorGUI.indentLevel--;
        }

        private void DrawBakeButton()
        {
            EditorGUILayout.Space(12f);

            bool canBake =
                modelPrefab != null &&
                sourceModelRoot != null &&
                outputLibrary != null &&
                facialBones != null &&
                facialBones.Length > 0 &&
                mappings != null &&
                mappings.Length > 0;

            using (new EditorGUI.DisabledScope(!canBake))
            {
                if (GUILayout.Button(
                        "Bake Facial Pose Library",
                        GUILayout.Height(36f)))
                {
                    Bake();
                }
            }

            EditorGUILayout.Space(8f);
        }

        private void Bake()
        {
            if (!ValidateInputs(out string validationError))
            {
                EditorUtility.DisplayDialog(
                    "Spark Facial Action Baker",
                    validationError,
                    "OK");

                return;
            }

            if (!TryBuildBonePaths(
                    out string[] bonePaths,
                    out validationError))
            {
                EditorUtility.DisplayDialog(
                    "Spark Facial Action Baker",
                    validationError,
                    "OK");

                return;
            }

            if (!TryValidateMappings(out validationError))
            {
                EditorUtility.DisplayDialog(
                    "Spark Facial Action Baker",
                    validationError,
                    "OK");

                return;
            }

            GameObject temporaryInstance = null;

            try
            {
                temporaryInstance = CreateTemporaryInstance(
                    modelPrefab);

                if (temporaryInstance == null)
                {
                    throw new InvalidOperationException(
                        "Could not create a temporary instance of " +
                        "the model asset.");
                }

                temporaryInstance.name =
                    modelPrefab.name + "_SparkFacialBake_TEMP";

                temporaryInstance.hideFlags =
                    HideFlags.HideAndDontSave;

                Transform[] sampledBones = ResolveSampledBones(
                    temporaryInstance.transform,
                    bonePaths);

               var bakedPoses =   new List<SparkFacialPoseLibrary.Pose>();

                int clipCount = CountAssignedMappings();

                if (clipCount == 0)
                {
                    throw new InvalidOperationException(
                        "No animation clips are assigned.");
                }

                int processed = 0;

                foreach (Mapping mapping in mappings)
                {
                    if (mapping == null || mapping.clip == null)
                    {
                        if (skipUnassignedClips)
                        {
                            continue;
                        }

                        throw new InvalidOperationException(
                            "A mapping has no assigned animation clip.");
                    }

                    processed++;

                    bool cancelled = EditorUtility.DisplayCancelableProgressBar(
                        "Spark Facial Action Baker",
                        "Sampling " + mapping.viseme +
                        " (" + processed + "/" + clipCount + ")",
                        (float)processed / clipCount);

                    if (cancelled)
                    {
                        throw new OperationCanceledException(
                            "Facial pose baking was cancelled.");
                    }

                    SamplePose(
                        temporaryInstance,
                        mapping,
                        sampledBones,
                        bakedPoses);
                }

                if (bakedPoses.Count == 0)
                {
                    throw new InvalidOperationException(
                        "No facial poses were generated. " +
                        "Assign valid animation clips.");
                }

                Undo.RecordObject(
                    outputLibrary,
                    "Bake Spark Facial Pose Library");

               outputLibrary.SetBakedData(
    bonePaths,
    bakedPoses.ToArray());

                EditorUtility.SetDirty(outputLibrary);
                AssetDatabase.SaveAssets();

                Debug.Log(
                    "[Spark Facial Action Baker] Successfully baked " +
                    bakedPoses.Count + " facial poses into: " +
                    AssetDatabase.GetAssetPath(outputLibrary),
                    outputLibrary);

                EditorUtility.DisplayDialog(
                    "Spark Facial Action Baker",
                    "Successfully baked " + bakedPoses.Count +
                    " facial poses.\n\n" +
                    "Bone count: " + bonePaths.Length + "\n" +
                    "Output: " +
                    AssetDatabase.GetAssetPath(outputLibrary) +
                    "\n\nTest the poses in Play Mode.",
                    "OK");
            }
            catch (OperationCanceledException)
            {
                Debug.LogWarning(
                    "[Spark Facial Action Baker] Baking cancelled.");
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);

                EditorUtility.DisplayDialog(
                    "Spark Facial Action Baker",
                    "Baking failed.\n\n" +
                    exception.Message +
                    "\n\nCheck the Unity Console for details.",
                    "OK");
            }
            finally
            {
                EditorUtility.ClearProgressBar();

                if (temporaryInstance != null)
                {
                    DestroyImmediate(temporaryInstance);
                }
            }
        }

        private bool ValidateInputs(out string error)
        {
            if (modelPrefab == null)
            {
                error = "Assign the model prefab or imported FBX asset.";
                return false;
            }

            if (sourceModelRoot == null)
            {
                error = "Assign the matching scene model root.";
                return false;
            }

            if (outputLibrary == null)
            {
                error = "Assign the output facial pose library.";
                return false;
            }

            if (facialBones == null || facialBones.Length == 0)
            {
                error = "Assign at least one facial bone.";
                return false;
            }

            if (mappings == null || mappings.Length == 0)
            {
                error = "Add at least one viseme mapping.";
                return false;
            }

            if (EditorUtility.IsPersistent(sourceModelRoot))
            {
                error =
                    "Scene Model Root must reference an instance " +
                    "in the open scene, not a Project asset.";
                return false;
            }

            if (!EditorUtility.IsPersistent(outputLibrary))
            {
                error =
                    "The output library must be a saved asset in " +
                    "the Unity project.";
                return false;
            }

            error = null;
            return true;
        }

        private bool TryBuildBonePaths(
            out string[] paths,
            out string error)
        {
            paths = new string[facialBones.Length];

            var uniquePaths = new HashSet<string>(
                StringComparer.Ordinal);

            for (int i = 0; i < facialBones.Length; i++)
            {
                Transform bone = facialBones[i];

                if (bone == null)
                {
                    error = "Facial Bone " + i + " is not assigned.";
                    return false;
                }

                string path = GetRelativePath(
                    sourceModelRoot,
                    bone);

                if (path == null)
                {
                    error =
                        "Facial Bone " + i +
                        " is not a child of Scene Model Root.";
                    return false;
                }

                if (!uniquePaths.Add(path))
                {
                    error =
                        "Duplicate facial bone mapping detected at " +
                        "index " + i + ": " + path;
                    return false;
                }

                paths[i] = path;
            }

            error = null;
            return true;
        }

        private bool TryValidateMappings(out string error)
        {
            var usedVisemes = new HashSet<SparkViseme>();

            for (int i = 0; i < mappings.Length; i++)
            {
                Mapping mapping = mappings[i];

                if (mapping == null)
                {
                    error = "Mapping " + i + " is null.";
                    return false;
                }

                if (!usedVisemes.Add(mapping.viseme))
                {
                    error =
                        "Duplicate viseme mapping detected: " +
                        mapping.viseme +
                        ". Each viseme must have one mapping.";
                    return false;
                }

                if (mapping.clip == null)
                {
                    if (skipUnassignedClips)
                    {
                        continue;
                    }

                    error =
                        "No animation clip is assigned to viseme " +
                        mapping.viseme + ".";
                    return false;
                }

                if (mapping.clip.length < 0f)
                {
                    error =
                        "Invalid animation clip length for " +
                        mapping.viseme + ".";
                    return false;
                }

                if (mapping.clip.legacy)
                {
                    error =
                        "Legacy animation clip detected for " +
                        mapping.viseme +
                        ". Convert the clip to a compatible animation " +
                        "format before baking.";
                    return false;
                }

                if (float.IsNaN(mapping.sampleTime) ||
                    float.IsInfinity(mapping.sampleTime))
                {
                    error =
                        "Invalid sample time for viseme " +
                        mapping.viseme + ".";
                    return false;
                }
            }

            error = null;
            return true;
        }

        private int CountAssignedMappings()
        {
            int count = 0;

            foreach (Mapping mapping in mappings)
            {
                if (mapping != null && mapping.clip != null)
                {
                    count++;
                }
            }

            return count;
        }

        private static GameObject CreateTemporaryInstance(
            GameObject modelAsset)
        {
            if (PrefabUtility.IsPartOfPrefabAsset(modelAsset))
            {
                return (GameObject)PrefabUtility.InstantiatePrefab(
                    modelAsset);
            }

            return Instantiate(modelAsset);
        }

        private static Transform[] ResolveSampledBones(
            Transform instanceRoot,
            string[] bonePaths)
        {
            var result = new Transform[bonePaths.Length];

            for (int i = 0; i < bonePaths.Length; i++)
            {
                string path = bonePaths[i];

                result[i] = string.IsNullOrEmpty(path)
                    ? instanceRoot
                    : instanceRoot.Find(path);

                if (result[i] == null)
                {
                    throw new InvalidOperationException(
                        "Could not find bone path '" + path +
                        "' on the temporary model instance.\n\n" +
                        "Check that the scene instance and model asset " +
                        "have matching hierarchies and bone names.");
                }
            }

            return result;
        }

        private static void SamplePose(
            GameObject instance,
            Mapping mapping,
            Transform[] sampledBones,
            List<SparkFacialPoseLibrary.Pose> output)
        {
            float sampleTime = Mathf.Clamp(
                mapping.sampleTime,
                0f,
                Mathf.Max(0f, mapping.clip.length));

            mapping.clip.SampleAnimation(
                instance,
                sampleTime);

            var rotations = new Quaternion[sampledBones.Length];

            for (int i = 0; i < sampledBones.Length; i++)
            {
                Transform bone = sampledBones[i];

                if (bone == null)
                {
                    throw new InvalidOperationException(
                        "A sampled facial bone was destroyed or " +
                        "is no longer available.");
                }

                rotations[i] = bone.localRotation;
            }

            output.Add(new SparkFacialPoseLibrary.Pose
{
    viseme = mapping.viseme,
    localRotations = rotations
});
        }

        private static string GetRelativePath(
            Transform root,
            Transform child)
        {
            if (root == null || child == null)
            {
                return null;
            }

            if (root == child)
            {
                return string.Empty;
            }

            var segments = new Stack<string>();

            Transform current = child;

            while (current != null && current != root)
            {
                segments.Push(current.name);
                current = current.parent;
            }

            if (current != root)
            {
                return null;
            }

            return string.Join("/", segments);
        }

        private static Mapping[] CreateMappings()
        {
            SparkViseme[] visemes =
                (SparkViseme[])Enum.GetValues(
                    typeof(SparkViseme));

            var result = new Mapping[visemes.Length];

            for (int i = 0; i < visemes.Length; i++)
            {
                result[i] = new Mapping
                {
                    viseme = visemes[i],
                    clip = null,
                    sampleTime = 0f
                };
            }

            return result;
        }
    }
}
#endif
