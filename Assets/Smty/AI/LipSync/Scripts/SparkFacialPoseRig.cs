using System;
using UnityEngine;

namespace ProjectSpark.AI.LipSync
{
    [DefaultExecutionOrder(100)]
    [DisallowMultipleComponent]
    public sealed class SparkFacialPoseRig : MonoBehaviour
    {
        [SerializeField] private SparkFacialPoseLibrary library;
        [Tooltip("Optional explicit bone list in the same order as the library. Leave empty to resolve paths from this transform.")]
        [SerializeField] private Transform[] facialBones = Array.Empty<Transform>();
        [SerializeField, Min(0.005f)] private float blendTime = 0.055f;

        private Transform[] resolvedBones;
        private Quaternion[] targetRotations;
        private bool ready;
        private SparkViseme currentViseme;

        public bool IsReady => ready;
        public SparkViseme CurrentViseme => currentViseme;

        private void Awake() => Initialize();

        public bool Initialize()
        {
            ready = false;

            if (library == null || library.BoneCount == 0)
            {
                Debug.LogError("SparkFacialPoseRig: assign a populated pose library.", this);
                return false;
            }

            resolvedBones = new Transform[library.BoneCount];
            targetRotations = new Quaternion[library.BoneCount];

            if (facialBones != null && facialBones.Length > 0)
            {
                if (facialBones.Length != library.BoneCount)
                {
                    Debug.LogError("SparkFacialPoseRig: explicit bone count does not match the pose library.", this);
                    return false;
                }

                Array.Copy(facialBones, resolvedBones, facialBones.Length);
            }
            else
            {
                for (int i = 0; i < library.BoneCount; i++)
                {
                    string path = library.GetBonePath(i);
                    Transform found = string.IsNullOrEmpty(path) ? transform : transform.Find(path);

                    if (found == null)
                    {
                        Debug.LogError("SparkFacialPoseRig: cannot resolve facial bone path: " + path +
                                       ". Assign the bones explicitly or use the correct rig root.", this);
                        return false;
                    }

                    resolvedBones[i] = found;
                }
            }

            for (int i = 0; i < resolvedBones.Length; i++)
            {
                if (resolvedBones[i] == null)
                {
                    Debug.LogError("SparkFacialPoseRig: a facial bone reference is missing.", this);
                    return false;
                }
            }

            foreach (SparkViseme viseme in Enum.GetValues(typeof(SparkViseme)))
            {
                if (library.TryGetPose(viseme, out Quaternion[] poseRotations) &&
                    poseRotations.Length != library.BoneCount)
                {
                    Debug.LogError(
                        "SparkFacialPoseRig: pose " + viseme +
                        " has " + poseRotations.Length + " rotations; expected " +
                        library.BoneCount + ".", this);
                    return false;
                }
            }

            ready = true;
            currentViseme = (SparkViseme)(-1);

            if (!SetViseme(SparkViseme.Default))
                Debug.LogWarning("SparkFacialPoseRig: Default pose is missing from the library.", this);

            return true;
        }

        public bool SetViseme(SparkViseme viseme)
        {
            if (!ready || !library.TryGetPose(viseme, out Quaternion[] rotations) ||
                rotations == null)
                return false;

            if (rotations.Length != targetRotations.Length)
                return false;

            Array.Copy(rotations, targetRotations, rotations.Length);
            currentViseme = viseme;
            return true;
        }

        public void ResetFace() => SetViseme(SparkViseme.Default);

        private void LateUpdate()
        {
            if (!ready) return;

            float deltaTime = Mathf.Max(0f, Time.deltaTime);
            float blend = 1f - Mathf.Exp(-deltaTime / Mathf.Max(0.005f, blendTime));

            for (int i = 0; i < resolvedBones.Length; i++)
            {
                if (resolvedBones[i] != null)
                {
                    resolvedBones[i].localRotation = Quaternion.Slerp(
                        resolvedBones[i].localRotation, targetRotations[i], blend);
                }
            }
        }
    }
}
