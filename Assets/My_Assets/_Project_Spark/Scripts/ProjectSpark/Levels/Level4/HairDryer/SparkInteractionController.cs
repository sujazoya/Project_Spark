using UnityEngine;

namespace ProjectSpark.Gameplay
{
    /// <summary>
    /// Main Project Spark player interaction controller.
    ///
    /// Responsibilities:
    /// - Raycast from the player camera.
    /// - Detect SparkInteractable objects.
    /// - Highlight the object under the cursor.
    /// - Interact with the object on mouse click.
    ///
    /// This system is intentionally small.
    ///
    /// It does NOT control:
    /// - Movement
    /// - Rotation
    /// - Electrical simulation
    /// - Circuit topology
    /// - Hair-dryer logic
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class SparkInteractionController : MonoBehaviour
    {
        [Header("Camera")]
        [SerializeField]
        private Camera targetCamera;

        [Header("Raycast")]
        [SerializeField]
        [Min(0.1f)]
        private float interactionDistance = 5f;

        [SerializeField]
        private LayerMask interactionLayers =
            ~0;

        [SerializeField]
        private QueryTriggerInteraction
            triggerInteraction =
                QueryTriggerInteraction.Ignore;

        [Header("Input")]
        [SerializeField]
        private KeyCode interactKey =
            KeyCode.Mouse0;

        [Header("Settings")]
        [SerializeField]
        private bool interactionEnabled = true;

        [SerializeField]
        private bool highlightTarget = true;

        [Header("Runtime")]
        [SerializeField]
        private SparkInteractable currentTarget;

        [SerializeField]
        private SparkInteractable previousTarget;

        public SparkInteractable CurrentTarget =>
            currentTarget;

        public bool HasTarget =>
            currentTarget != null;

        public bool InteractionEnabled =>
            interactionEnabled;

        private void Awake()
        {
            if (targetCamera == null)
                targetCamera = Camera.main;
        }

        private void Update()
        {
            if (!interactionEnabled)
            {
                ClearTarget();
                return;
            }

            UpdateTarget();

            if (Input.GetKeyDown(interactKey))
            {
                TryInteract();
            }
        }

        private void UpdateTarget()
        {
            SparkInteractable newTarget =
                RaycastForInteractable();

            if (newTarget == currentTarget)
                return;

            previousTarget =
                currentTarget;

            currentTarget =
                newTarget;

            UpdateHighlightState();
        }

        private SparkInteractable
            RaycastForInteractable()
        {
            if (targetCamera == null)
                return null;

            Ray ray =
                targetCamera.ScreenPointToRay(
                    Input.mousePosition);

            if (!Physics.Raycast(
                    ray,
                    out RaycastHit hit,
                    interactionDistance,
                    interactionLayers,
                    triggerInteraction))
            {
                return null;
            }

            SparkInteractable interactable =
                hit.collider.GetComponent<
                    SparkInteractable>();

            if (interactable == null)
            {
                interactable =
                    hit.collider.GetComponentInParent<
                        SparkInteractable>();
            }

            if (interactable == null)
                return null;

            if (!interactable.CanInteract())
                return null;

            return interactable;
        }

        private void TryInteract()
        {
            if (currentTarget == null)
                return;

            if (!currentTarget.CanInteract())
                return;

            currentTarget.Interact();
        }

        private void UpdateHighlightState()
        {
            if (!highlightTarget)
                return;

            if (previousTarget != null)
            {
                previousTarget.SetHighlighted(false);
            }

            if (currentTarget != null)
            {
                currentTarget.SetHighlighted(true);
            }
        }

        private void ClearTarget()
        {
            if (currentTarget != null)
            {
                currentTarget.SetHighlighted(false);
            }

            currentTarget = null;
            previousTarget = null;
        }

        public void SetInteractionEnabled(
            bool enabled)
        {
            interactionEnabled = enabled;

            if (!enabled)
                ClearTarget();
        }

        public void SetInteractionDistance(
            float distance)
        {
            interactionDistance =
                Mathf.Max(
                    0.1f,
                    distance);
        }

        [ContextMenu("Test / Clear Target")]
        private void TestClearTarget()
        {
            ClearTarget();
        }
    }
}