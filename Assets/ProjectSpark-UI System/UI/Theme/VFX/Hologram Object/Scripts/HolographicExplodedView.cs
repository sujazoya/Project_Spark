using UnityEngine;

namespace ProjectSpark.HolographicViewer
{
    public sealed class HolographicExplodedView : MonoBehaviour
    {
        [System.Serializable]
        private class Part
        {
            public Transform transform;
            public Vector3 explodedOffset;

            [HideInInspector]
            public Vector3 originalPosition;
        }

        [SerializeField] private Part[] parts;

        [SerializeField]
        private float explosionAmount = 0f;

        [SerializeField]
        private float smoothSpeed = 8f;

        private void Awake()
        {
            for (int i = 0; i < parts.Length; i++)
            {
                if (parts[i].transform != null)
                {
                    parts[i].originalPosition =
                        parts[i].transform.localPosition;
                }
            }
        }

        private void LateUpdate()
        {
            for (int i = 0; i < parts.Length; i++)
            {
                Part part = parts[i];

                if (part.transform == null)
                    continue;

                Vector3 target =
                    part.originalPosition +
                    part.explodedOffset *
                    explosionAmount;

                part.transform.localPosition =
                    Vector3.Lerp(
                        part.transform.localPosition,
                        target,
                        Time.deltaTime *
                        smoothSpeed
                    );
            }
        }

        public void SetExploded(bool value)
        {
            explosionAmount =
                value ? 1f : 0f;
        }

        public void SetAmount(float value)
        {
            explosionAmount =
                Mathf.Clamp01(value);
        }
    }
}