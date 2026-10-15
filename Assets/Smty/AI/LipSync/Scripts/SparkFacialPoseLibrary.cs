using System;
using UnityEngine;

namespace ProjectSpark.AI.LipSync
{
    public enum SparkViseme
    {
        Default,
        Ah,
        Ai,
        Ee,
        Eh,
        FV,
        MBP,
        Oo,
        Ooh,
        R_Sh,
        S
    }

    [CreateAssetMenu(
        fileName = "SparkFacialPoseLibrary",
        menuName = "Project Spark/AI/Facial Pose Library")]
    public sealed class SparkFacialPoseLibrary : ScriptableObject
    {
        [Serializable]
        public sealed class Pose
        {
            public SparkViseme viseme;
            public Quaternion[] localRotations;
        }

        [SerializeField]
        private string[] bonePaths = Array.Empty<string>();

        [SerializeField]
        private Pose[] poses = Array.Empty<Pose>();

        public string[] BonePaths => bonePaths;

        public int BoneCount =>
            bonePaths == null ? 0 : bonePaths.Length;

        public int PoseCount =>
            poses == null ? 0 : poses.Length;

        public string GetBonePath(int index)
        {
            if (bonePaths == null ||
                index < 0 ||
                index >= bonePaths.Length)
            {
                return null;
            }

            return bonePaths[index];
        }

        public bool TryGetPose(
            SparkViseme viseme,
            out Quaternion[] rotations)
        {
            if (poses != null)
            {
                for (int i = 0; i < poses.Length; i++)
                {
                    Pose pose = poses[i];

                    if (pose == null || pose.viseme != viseme)
                    {
                        continue;
                    }

                    if (pose.localRotations == null ||
                        pose.localRotations.Length != BoneCount)
                    {
                        break;
                    }

                    rotations = pose.localRotations;
                    return true;
                }
            }

            rotations = null;
            return false;
        }

#if UNITY_EDITOR
        public void SetBakedData(
            string[] paths,
            Pose[] bakedPoses)
        {
            if (paths == null)
            {
                throw new ArgumentNullException(nameof(paths));
            }

            if (bakedPoses == null)
            {
                throw new ArgumentNullException(nameof(bakedPoses));
            }

            for (int i = 0; i < bakedPoses.Length; i++)
            {
                if (bakedPoses[i] == null)
                {
                    throw new ArgumentException(
                        "A baked pose cannot be null.",
                        nameof(bakedPoses));
                }

                if (bakedPoses[i].localRotations == null ||
                    bakedPoses[i].localRotations.Length != paths.Length)
                {
                    throw new ArgumentException(
                        "Every pose must contain one rotation per bone.",
                        nameof(bakedPoses));
                }
            }

            bonePaths = (string[])paths.Clone();
            poses = (Pose[])bakedPoses.Clone();
        }
#endif
    }
}
