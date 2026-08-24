using System.Collections.Generic;
using UnityEngine;

namespace ProjectSpark.Scanner
{
    [DisallowMultipleComponent]
    public sealed class Scanner3DHoverController
        : MonoBehaviour
    {
        [Header("Raycast")]
        [SerializeField]
        private Camera targetCamera;

        [SerializeField]
        private LayerMask hoverMask = ~0;

        [SerializeField, Min(0.1f)]
        private float maxDistance = 100f;

        [SerializeField]
        private QueryTriggerInteraction triggerInteraction =
            QueryTriggerInteraction.Ignore;

        [Header("Hover")]
        [SerializeField]
        private bool allowHover = true;

        private readonly List<Scanner3DHoverTarget>
            activeTargets =
                new List<Scanner3DHoverTarget>();

        private Scanner3DHoverTarget currentTarget;

        public Scanner3DHoverTarget CurrentTarget =>
            currentTarget;

        private void Awake()
        {
            if (targetCamera == null)
                targetCamera = Camera.main;
        }

        private void Update()
        {
            if (targetCamera == null)
            {
                targetCamera = Camera.main;

                if (targetCamera == null)
                    return;
            }

            if (allowHover)
            {
                Scanner3DHoverTarget hitTarget =
                    RaycastHoverTarget();

                UpdateCurrentTarget(
                    hitTarget);
            }
            else
            {
                ClearHover();
            }

            TickActiveTargets();
        }

        // =============================================================
        // RAYCAST
        // =============================================================

        private Scanner3DHoverTarget RaycastHoverTarget()
        {
            Ray ray =
                targetCamera.ScreenPointToRay(
                    Input.mousePosition);

            if (!Physics.Raycast(
                    ray,
                    out RaycastHit hit,
                    maxDistance,
                    hoverMask,
                    triggerInteraction))
            {
                return null;
            }

            return hit.collider
                .GetComponentInParent<
                    Scanner3DHoverTarget>();
        }

        // =============================================================
        // CURRENT TARGET
        // =============================================================

        private void UpdateCurrentTarget(
            Scanner3DHoverTarget target)
        {
            if (currentTarget == target)
                return;

            if (currentTarget != null)
            {
                currentTarget.SetHovered(false);
                AddActiveTarget(currentTarget);
            }

            currentTarget =
                target;

            if (currentTarget != null)
            {
                currentTarget.SetHovered(true);
                AddActiveTarget(currentTarget);
            }
        }

        // =============================================================
        // ACTIVE TARGETS
        // =============================================================

        private void AddActiveTarget(
            Scanner3DHoverTarget target)
        {
            if (target == null)
                return;

            for (int i = 0;
                 i < activeTargets.Count;
                 i++)
            {
                if (activeTargets[i] == target)
                    return;
            }

            activeTargets.Add(target);
        }

        private void TickActiveTargets()
        {
            for (int i = activeTargets.Count - 1;
                 i >= 0;
                 i--)
            {
                Scanner3DHoverTarget target =
                    activeTargets[i];

                if (target == null)
                {
                    activeTargets.RemoveAt(i);
                    continue;
                }

                bool stillActive =
                    target.TickHover(
                        Time.deltaTime);

                if (!stillActive)
                {
                    activeTargets.RemoveAt(i);
                }
            }
        }

        // =============================================================
        // CLEAR
        // =============================================================

        public void ClearHover()
        {
            if (currentTarget != null)
            {
                currentTarget.SetHovered(false);
                AddActiveTarget(currentTarget);

                currentTarget = null;
            }
        }

        // =============================================================
        // ENABLE / DISABLE
        // =============================================================

        public void SetHoverEnabled(
            bool enabled)
        {
            allowHover = enabled;

            if (!enabled)
                ClearHover();
        }

        // =============================================================
        // HARD RESET
        // =============================================================

        public void ResetHoverImmediate()
        {
            if (currentTarget != null)
            {
                currentTarget.SetHovered(false);
                AddActiveTarget(currentTarget);
                currentTarget = null;
            }

            for (int i = 0;
                 i < activeTargets.Count;
                 i++)
            {
                Scanner3DHoverTarget target =
                    activeTargets[i];

                if (target == null)
                    continue;

                target.SetHovered(false);
                target.SetImmediate(0f);
            }

            activeTargets.Clear();
        }
    }
}