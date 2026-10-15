
using UnityEngine;

namespace ProjectSpark.AI.LipSync
{
    [DisallowMultipleComponent]
    public sealed class SparkFacialPoseController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField]
        private SparkFacialPoseLibrary library;

        [Tooltip("The exact model root used during pose baking.")]
        [SerializeField]
        private Transform modelRoot;

        [Header("Animation")]
        [SerializeField, Min(0.001f)]
        private float blendTime = 0.055f;

        private Transform[] bones;
        private Quaternion[] currentRotations;
        private Quaternion[] targetRotations;

        private SparkViseme currentViseme;
        private bool initialized;

        public SparkViseme CurrentViseme => currentViseme;

        private void Awake()
        {
            Initialize();
        }

        public void Initialize()
        {
            initialized = false;

            if (library == null || modelRoot == null)
            {
                Debug.LogError(
                    "Spark facial library or model root is missing.",
                    this);
                return;
            }

            string[] paths = library.BonePaths;

            if (paths == null || paths.Length == 0)
            {
                Debug.LogError(
                    "Spark facial library has no baked bones.",
                    this);
                return;
            }

            bones = new Transform[paths.Length];
            currentRotations = new Quaternion[paths.Length];
            targetRotations = new Quaternion[paths.Length];

            for (int i = 0; i < paths.Length; i++)
            {
                Transform bone = string.IsNullOrEmpty(paths[i])
                    ? modelRoot
                    : modelRoot.Find(paths[i]);

                if (bone == null)
                {
                    Debug.LogError(
                        "Missing facial bone: " + paths[i],
                        this);
                    return;
                }

                bones[i] = bone;
                currentRotations[i] = bone.localRotation;
                targetRotations[i] = bone.localRotation;
            }

            initialized = true;
            SetViseme(SparkViseme.Default);
        }

        public void SetViseme(SparkViseme viseme)
        {
            if (!initialized)
                return;

            if (!library.TryGetPose(
                viseme,
                out Quaternion[] rotations))
            {
                return;
            }

            if (rotations.Length != bones.Length)
                return;

            currentViseme = viseme;

            for (int i = 0; i < rotations.Length; i++)
            {
                targetRotations[i] = rotations[i];
            }
        }

        public void ResetMouth()
        {
            SetViseme(SparkViseme.Default);
        }

        private void LateUpdate()
        {
            if (!initialized)
                return;

            float factor = 1f -
                Mathf.Exp(-Time.deltaTime / blendTime);

            for (int i = 0; i < bones.Length; i++)
            {
                currentRotations[i] = Quaternion.Slerp(
                    currentRotations[i],
                    targetRotations[i],
                    factor);

                bones[i].localRotation =
                    currentRotations[i];
            }
        }
    }
}
