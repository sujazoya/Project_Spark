
#if UNITY_EDITOR

using System;
using UnityEditor;
using UnityEngine;

namespace ProjectSpark.AI.LipSync
{
    public sealed class SparkFacialPoseBaker : EditorWindow
    {
        [Serializable]
        private sealed class ClipEntry
        {
            public SparkViseme viseme;
            public AnimationClip clip;
            public float sampleTime;
        }

        private GameObject modelPrefab;
        private Transform modelRoot;
        private SparkFacialPoseLibrary outputLibrary;

        private Transform[] facialBones =
            new Transform[2];

        private ClipEntry[] entries;

        [MenuItem("Project Spark/AI/Facial Pose Baker")]
        private static void Open()
        {
            GetWindow<SparkFacialPoseBaker>(
                "Spark Facial Pose Baker");
        }

        private void OnEnable()
        {
            Array values = Enum.GetValues(
                typeof(SparkViseme));

            entries = new ClipEntry[values.Length];

            for (int i = 0; i < values.Length; i++)
            {
                entries[i] = new ClipEntry
                {
                    viseme = (SparkViseme)values.GetValue(i)
                };
            }
        }

        private void OnGUI()
        {
            EditorGUILayout.LabelField(
                "Project Spark Facial Pose Baker",
                EditorStyles.boldLabel);

            EditorGUILayout.Space();

            modelPrefab = (GameObject)
                EditorGUILayout.ObjectField(
                    "Imported Model FBX",
                    modelPrefab,
                    typeof(GameObject),
                    false);

            modelRoot = (Transform)
                EditorGUILayout.ObjectField(
                    "Scene Model Root",
                    modelRoot,
                    typeof(Transform),
                    true);

            outputLibrary = (SparkFacialPoseLibrary)
                EditorGUILayout.ObjectField(
                    "Output Library",
                    outputLibrary,
                    typeof(SparkFacialPoseLibrary),
                    false);

            EditorGUILayout.Space();

            EditorGUILayout.LabelField(
                "Facial Bones",
                EditorStyles.boldLabel);

            for (int i = 0; i < facialBones.Length; i++)
            {
                facialBones[i] = (Transform)
                    EditorGUILayout.ObjectField(
                        i == 0
                            ? "Upper Lip"
                            : "Lower Lip",
                        facialBones[i],
                        typeof(Transform),
                        true);
            }

            EditorGUILayout.Space();

            EditorGUILayout.LabelField(
                "Imported Blender Actions",
                EditorStyles.boldLabel);

            for (int i = 0; i < entries.Length; i++)
            {
                ClipEntry entry = entries[i];

                EditorGUILayout.BeginVertical("box");

                EditorGUILayout.LabelField(
                    entry.viseme.ToString(),
                    EditorStyles.boldLabel);

                entry.clip = (AnimationClip)
                    EditorGUILayout.ObjectField(
                        "Animation Clip",
                        entry.clip,
                        typeof(AnimationClip),
                        false);

                entry.sampleTime =
                    EditorGUILayout.FloatField(
                        "Sample Time (seconds)",
                        entry.sampleTime);

                EditorGUILayout.EndVertical();
            }

            EditorGUILayout.Space();

            GUI.enabled =
                modelRoot != null &&
                outputLibrary != null;

            if (GUILayout.Button(
                "Bake Facial Pose Library",
                GUILayout.Height(35)))
            {
                Bake();
            }

            GUI.enabled = true;
        }

        private void Bake()
        {
            if (modelRoot == null ||
                outputLibrary == null)
                return;

            string[] paths =
                new string[facialBones.Length];

            Quaternion[] originalRotations =
                new Quaternion[facialBones.Length];

            for (int i = 0; i < facialBones.Length; i++)
            {
                Transform bone = facialBones[i];

                if (bone == null ||
                    (bone != modelRoot &&
                     !bone.IsChildOf(modelRoot)))
                {
                    Debug.LogError(
                        "All facial bones must belong " +
                        "to the assigned scene model root.");

                    return;
                }

                paths[i] =
                    AnimationUtility.CalculateTransformPath(
                        bone,
                        modelRoot);

                originalRotations[i] = bone.localRotation;
            }

            SparkFacialPoseLibrary.Pose[] baked =
                new SparkFacialPoseLibrary.Pose[
                    entries.Length];

            // SampleAnimation changes the scene instance.
            // The sampled lip rotations are restored afterward.
            try
            {
                for (int i = 0; i < entries.Length; i++)
                {
                    ClipEntry entry = entries[i];

                    if (entry.clip == null)
                    {
                        Debug.LogError(
                            "Missing clip for pose: " +
                            entry.viseme);
                        return;
                    }

                    // Restore neutral before each sample.
                    for (int b = 0; b < facialBones.Length; b++)
                    {
                        facialBones[b].localRotation =
                            originalRotations[b];
                    }

                    float time = Mathf.Clamp(
                        entry.sampleTime,
                        0f,
                        entry.clip.length);

                    entry.clip.SampleAnimation(
                        modelRoot.gameObject,
                        time);

                    Quaternion[] rotations =
                        new Quaternion[facialBones.Length];

                    for (int b = 0; b < facialBones.Length; b++)
                    {
                        rotations[b] =
                            facialBones[b].localRotation;
                    }

                    baked[i] =
                        new SparkFacialPoseLibrary.Pose
                        {
                            viseme = entry.viseme,
                            localRotations = rotations
                        };
                }

                Undo.RecordObject(
                    outputLibrary,
                    "Bake Spark Facial Poses");

                outputLibrary.SetBakedData(
                    paths,
                    baked);

                EditorUtility.SetDirty(outputLibrary);
                AssetDatabase.SaveAssets();

                Debug.Log(
                    "Project Spark facial pose library baked.");
            }
            finally
            {
                for (int i = 0; i < facialBones.Length; i++)
                {
                    facialBones[i].localRotation =
                        originalRotations[i];
                }
            }
        }
    }
}

#endif
