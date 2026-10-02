using UnityEngine;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace ProjectSpark.Gameplay
{
    /// <summary>
    /// Two-stage interaction for SparkSingleWire.
    ///
    /// Stage 1:
    ///     Click this wire itself.
    ///
    /// Stage 2:
    ///     Wire is armed and waits for a target click.
    ///
    /// A click anywhere else while idle does nothing.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class SparkWireContact : MonoBehaviour
    {
        // ============================================================
        // REFERENCES
        // ============================================================

        [Header("Wire")]
        [SerializeField] private SparkSingleWire wire;

        [SerializeField] private Collider contactCollider;

        [Header("Input")]
        [SerializeField] private Camera inputCamera;

        [SerializeField] private LayerMask detectionLayers = ~0;

        [SerializeField, Min(0.1f)]
        private float raycastDistance = 1000f;

        [SerializeField]
        private bool ignoreUI = true;

        [SerializeField, Min(0f)]
        private float clickThreshold = 12f;

        [Header("Interaction")]
        [SerializeField]
        private bool armed;

        private Vector2 pointerDownPosition;

        // ============================================================
        // PUBLIC STATE
        // ============================================================

        public SparkSingleWire Wire => wire;

        public Collider ContactCollider => contactCollider;

        public bool IsArmed => armed;

        // ============================================================
        // UNITY
        // ============================================================

        private void Awake()
        {
            if (wire == null)
            {
                wire = GetComponentInParent<SparkSingleWire>();
            }

            if (contactCollider == null)
            {
                contactCollider = GetComponent<Collider>();
            }

            if (inputCamera == null)
            {
                inputCamera = Camera.main;
            }
        }

        private void Update()
        {
#if ENABLE_INPUT_SYSTEM

            if (Mouse.current == null)
            {
                return;
            }

            if (Mouse.current.leftButton.wasPressedThisFrame)
            {
                pointerDownPosition =
                    Mouse.current.position.ReadValue();

                return;
            }

            if (Mouse.current.leftButton.wasReleasedThisFrame)
            {
                Vector2 pointer =
                    Mouse.current.position.ReadValue();

                if (Vector2.Distance(
                        pointerDownPosition,
                        pointer) > clickThreshold)
                {
                    return;
                }

                ProcessClick(pointer);
            }

#endif
        }

        // ============================================================
        // CLICK PROCESS
        // ============================================================

        private void ProcessClick(Vector2 screenPosition)
        {
            if (inputCamera == null)
            {
                inputCamera = Camera.main;
            }

            if (inputCamera == null)
            {
                return;
            }

            if (ignoreUI && IsPointerOverUI())
            {
                return;
            }

            Ray ray =
                inputCamera.ScreenPointToRay(screenPosition);

            if (!Physics.Raycast(
                    ray,
                    out RaycastHit hit,
                    raycastDistance,
                    detectionLayers,
                    QueryTriggerInteraction.Collide))
            {
                return;
            }

            // ========================================================
            // IDLE
            // ========================================================
            //
            // ONLY OUR OWN WIRE COLLIDER CAN ARM THE WIRE.
            //
            if (!armed)
            {
                if (!IsOurCollider(hit.collider))
                {
                    return;
                }

                ArmWire(hit);
                return;
            }

            // ========================================================
            // ARMED
            // ========================================================
            //
            // We are now looking for the target.
            //
            // Clicking the wire again simply keeps it armed.
            //
            if (IsOurCollider(hit.collider))
            {
                return;
            }

            TryTakeTarget(hit);
        }

        // ============================================================
        // ARM
        // ============================================================

        private void ArmWire(RaycastHit hit)
        {
            if (wire == null)
            {
                return;
            }

            armed = true;

            wire.OnWireClicked(
                hit.point,
                hit.normal);

#if UNITY_EDITOR
            Debug.Log(
                $"[SparkWireContact] Wire armed: {wire.name}",
                this);
#endif
        }

        // ============================================================
        // TARGET
        // ============================================================

       private void TryTakeTarget(RaycastHit hit)
{
    if (wire == null)
    {
        armed = false;
        return;
    }

    SparkTerminal target =
        hit.collider.GetComponentInParent<SparkTerminal>();

    if (target == null)
    {
        return;
    }

    bool connected =
        wire.TryConnectToTarget(
            target,
            this);

    if (connected)
    {
        armed = false;

#if UNITY_EDITOR
        Debug.Log(
            $"[SparkWireContact] Wire connected to {target.name}",
            this);
#endif
    }
}

        // ============================================================
        // COLLIDER OWNERSHIP
        // ============================================================

        private bool IsOurCollider(Collider collider)
        {
            if (collider == null)
            {
                return false;
            }

            if (contactCollider != null &&
                collider == contactCollider)
            {
                return true;
            }

            SparkWireContact other =
                collider.GetComponentInParent<SparkWireContact>();

            return other == this;
        }

        // ============================================================
        // UI
        // ============================================================

        private bool IsPointerOverUI()
        {
#if ENABLE_INPUT_SYSTEM && ENABLE_INPUT_SYSTEM_PACKAGE

            return UnityEngine.EventSystems.EventSystem.current != null &&
                   UnityEngine.EventSystems.EventSystem.current
                       .IsPointerOverGameObject();

#else
            return false;
#endif
        }

        // ============================================================
        // CONTROL
        // ============================================================

        public void Disarm()
        {
            armed = false;
        }

        public void SetWire(SparkSingleWire value)
        {
            wire = value;
        }

        public void SetCollider(Collider value)
        {
            contactCollider = value;
        }

        public void SetCamera(Camera value)
        {
            inputCamera = value;
        }
    }
}