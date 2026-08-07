using System;
using UnityEngine;

namespace ProjectSpark.Hologram
{
    public sealed class HologramScanner : MonoBehaviour
    {
        [Header("Scan")]
        [SerializeField]
        private Camera scanCamera;

        [SerializeField, Min(0.1f)]
        private float scanDistance = 10f;

        [SerializeField]
        private LayerMask targetLayers = ~0;

        public event Action<HologramTarget> TargetFound;

        public HologramTarget CurrentTarget { get; private set; }

        public bool Scan()
        {
            if (scanCamera == null)
                return false;

            Ray ray = scanCamera.ViewportPointToRay(
                new Vector3(0.5f, 0.5f, 0f));

            if (!Physics.Raycast(
                    ray,
                    out RaycastHit hit,
                    scanDistance,
                    targetLayers,
                    QueryTriggerInteraction.Ignore))
            {
                ClearTarget();
                return false;
            }

            HologramTarget target =
                hit.collider.GetComponentInParent<HologramTarget>();

            if (target == null)
            {
                ClearTarget();
                return false;
            }

            if (CurrentTarget != target)
            {
                CurrentTarget = target;
                TargetFound?.Invoke(target);
            }

            return true;
        }

        public void ClearTarget()
        {
            CurrentTarget = null;
        }
    }
}