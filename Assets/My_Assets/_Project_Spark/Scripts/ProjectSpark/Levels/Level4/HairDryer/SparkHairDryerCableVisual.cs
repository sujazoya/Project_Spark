using UnityEngine;

namespace ProjectSpark.Gameplay
{
    /// <summary>
    /// Visual cable controller for the hair dryer.
    ///
    /// Responsibilities:
    /// - Smooth cable movement between two anchors.
    /// - Keeps the cable visually connected.
    /// - Supports a separate strain-relief section.
    ///
    /// Visual only.
    /// Does not handle electrical connections.
    /// Does not move the hair-dryer plug.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class SparkHairDryerCableVisual : MonoBehaviour
    {
        [Header("Cable")]
        [SerializeField]
        private Transform cableRoot;

        [SerializeField]
        private Transform startPoint;

        [SerializeField]
        private Transform endPoint;

        [Header("Cable Direction")]
        [SerializeField]
        private Vector3 localForward =
            Vector3.forward;

        [Header("Length")]
        [SerializeField]
        [Min(0.001f)]
        private float minimumLength = 0.02f;

        [Header("Strain Relief")]
        [SerializeField]
        private Transform strainRelief;

        [SerializeField]
        [Min(0f)]
        private float strainReliefLength = 0.08f;

        [SerializeField]
        private bool stretchCable = true;

        [Header("Runtime")]
        [SerializeField]
        private float currentLength;

        public float CurrentLength =>
            currentLength;

        private void LateUpdate()
        {
            UpdateCable();
        }

        private void UpdateCable()
        {
            if (cableRoot == null ||
                startPoint == null ||
                endPoint == null)
            {
                return;
            }

            Vector3 start =
                startPoint.position;

            Vector3 end =
                endPoint.position;

            Vector3 direction =
                end - start;

            float length =
                direction.magnitude;

            if (length < minimumLength)
                length = minimumLength;

            currentLength = length;

            Vector3 normalizedDirection =
                direction.sqrMagnitude > 0.000001f
                    ? direction.normalized
                    : cableRoot.forward;

            cableRoot.position =
                start;

            cableRoot.rotation =
                Quaternion.LookRotation(
                    normalizedDirection,
                    Vector3.up);

            if (stretchCable)
            {
                Vector3 scale =
                    cableRoot.localScale;

                scale.z =
                    length;

                cableRoot.localScale =
                    scale;
            }

            UpdateStrainRelief(
                start,
                normalizedDirection);
        }

        private void UpdateStrainRelief(
            Vector3 start,
            Vector3 direction)
        {
            if (strainRelief == null)
                return;

            strainRelief.position =
                start;

            if (direction.sqrMagnitude >
                0.000001f)
            {
                strainRelief.rotation =
                    Quaternion.LookRotation(
                        direction,
                        Vector3.up);
            }
        }

        public void SetEndpoints(
            Transform start,
            Transform end)
        {
            startPoint = start;
            endPoint = end;
        }

        public void SetStretching(bool enabled)
        {
            stretchCable = enabled;
        }

        [ContextMenu("Test / Refresh Cable")]
        private void TestRefresh()
        {
            UpdateCable();
        }
    }
}