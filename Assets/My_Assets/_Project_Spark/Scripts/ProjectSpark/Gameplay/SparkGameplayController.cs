using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace ProjectSpark.Gameplay
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(SparkToolController))]
    public sealed class SparkGameplayController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Camera interactionCamera;
        [SerializeField] private SparkToolController toolController;
        [SerializeField] private EventSystem eventSystem;

        [Header("Targeting")]
        [SerializeField] private LayerMask interactionLayers = ~0;

        [SerializeField, Min(0.1f)]
        private float interactionDistance = 8f;

        [SerializeField]
        private QueryTriggerInteraction triggerInteraction =
            QueryTriggerInteraction.Ignore;

        [Header("Input")]
        [SerializeField]
        private InputActionReference primaryAction;

        [SerializeField]
        private InputActionReference cancelAction;

        [SerializeField]
        private InputActionReference pointerAction;

        [Header("UI")]
        [SerializeField]
        private bool blockWorldInputOverUI = true;

        public SparkTargetHit? CurrentTarget { get; private set; }

        public SparkInteractionSession ActiveSession
        {
            get;
            private set;
        }

        public event Action<SparkTargetHit?> TargetChanged;
        public event Action<SparkResult> ResultProduced;

        // ========================================================
        // UNITY
        // ========================================================

        private void Reset()
        {
            toolController =
                GetComponent<SparkToolController>();

            interactionCamera =
                Camera.main;

            eventSystem =
                EventSystem.current;
        }

        private void Awake()
        {
            if (toolController == null)
                toolController =
                    GetComponent<SparkToolController>();

            if (interactionCamera == null)
                interactionCamera =
                    Camera.main;

            if (eventSystem == null)
                eventSystem =
                    EventSystem.current;
        }

        private void OnEnable()
        {
            if (primaryAction != null)
            {
                primaryAction.action.started +=
                    OnPrimaryStarted;

                primaryAction.action.canceled +=
                    OnPrimaryCanceled;

                primaryAction.action.Enable();
            }

            if (cancelAction != null)
            {
                cancelAction.action.performed +=
                    OnCancelPerformed;

                cancelAction.action.Enable();
            }

            if (pointerAction != null)
                pointerAction.action.Enable();
        }

        private void OnDisable()
        {
            if (primaryAction != null)
            {
                primaryAction.action.started -=
                    OnPrimaryStarted;

                primaryAction.action.canceled -=
                    OnPrimaryCanceled;

                primaryAction.action.Disable();
            }

            if (cancelAction != null)
            {
                cancelAction.action.performed -=
                    OnCancelPerformed;

                cancelAction.action.Disable();
            }

            if (pointerAction != null)
                pointerAction.action.Disable();

            CancelActiveInteraction();

            SetCurrentTarget(null);
        }

        private void Update()
        {
            RefreshTarget();

            if (ActiveSession == null)
                return;

            if (toolController == null)
                return;

            if (toolController.ActiveTool == null)
                return;

            if (CurrentTarget.HasValue)
            {
                ActiveSession.UpdateHit(
                    CurrentTarget.Value);
            }

            SparkTargetHit hit =
                ActiveSession.LastHit;

            SparkToolContext context =
                new SparkToolContext(
                    this,
                    toolController,
                    ActiveSession,
                    hit);

            SparkResult result =
                toolController.ActiveTool.Tick(
                    context);

            if (!result.Succeeded &&
                result.Code != SparkResultCode.Unavailable)
            {
                Publish(result);
            }
        }

        // ========================================================
        // TARGETING
        // ========================================================

        private void RefreshTarget()
        {
            if (interactionCamera == null)
                return;

            if (blockWorldInputOverUI &&
                IsPointerOverUI())
            {
                SetCurrentTarget(null);
                return;
            }

            Vector2 pointer =
                ReadPointerPosition();

            Ray ray =
                interactionCamera.ScreenPointToRay(
                    pointer);

            if (!Physics.Raycast(
                    ray,
                    out RaycastHit hit,
                    interactionDistance,
                    interactionLayers,
                    triggerInteraction))
            {
                SetCurrentTarget(null);
                return;
            }

            SparkElectronicObject target =
                hit.collider
                    .GetComponentInParent<
                        SparkElectronicObject>();

            if (target == null ||
                !target.isActiveAndEnabled ||
                !target.InteractionsEnabled)
            {
                SetCurrentTarget(null);
                return;
            }

            SetCurrentTarget(
                new SparkTargetHit(
                    target,
                    hit.collider,
                    hit.point,
                    hit.normal,
                    hit.distance));
        }

        // ========================================================
        // PRIMARY
        // ========================================================

        private void OnPrimaryStarted(
            InputAction.CallbackContext _)
        {
            if (blockWorldInputOverUI &&
                IsPointerOverUI())
            {
                return;
            }

            if (ActiveSession != null)
                return;

            if (!CurrentTarget.HasValue)
                return;

            SparkTool tool =
                toolController.ActiveTool;

            if (tool == null)
            {
                Publish(
                    SparkResult.Unavailable(
                        "No active tool."));
                return;
            }

            SparkTargetHit hit =
                CurrentTarget.Value;

            SparkInteractionType type =
                MapInteraction(
                    tool.ToolType);

            SparkInteractionContext interactionContext =
                CreateInteractionContext(
                    hit,
                    type);

            if (!hit.Target.CanInteract(
                    interactionContext,
                    out string reason))
            {
                Publish(
                    SparkResult.Rejected(reason));
                return;
            }

            SparkInteractionSession session =
                new SparkInteractionSession(
                    type,
                    tool.ToolType,
                    hit.Target,
                    hit);

            SparkToolContext toolContext =
                new SparkToolContext(
                    this,
                    toolController,
                    session,
                    hit);

            SparkResult result =
                tool.Begin(toolContext);

            Publish(result);

            if (result.Succeeded)
                ActiveSession = session;
        }

        // ========================================================
        // PRIMARY RELEASE
        // ========================================================

        private void OnPrimaryCanceled(
            InputAction.CallbackContext _)
        {
            if (ActiveSession == null)
                return;

            SparkTool tool =
                toolController.ActiveTool;

            if (tool == null)
            {
                ActiveSession = null;
                return;
            }

            SparkInteractionSession session =
                ActiveSession;

            SparkToolContext context =
                new SparkToolContext(
                    this,
                    toolController,
                    session,
                    session.LastHit);

            Publish(
                tool.End(context));

            ActiveSession = null;
        }

        // ========================================================
        // CANCEL
        // ========================================================

        private void OnCancelPerformed(
            InputAction.CallbackContext _)
        {
            CancelActiveInteraction();
        }

        private void CancelActiveInteraction()
        {
            if (ActiveSession == null)
                return;

            SparkTool tool =
                toolController.ActiveTool;

            if (tool == null)
            {
                ActiveSession = null;
                return;
            }

            SparkInteractionSession session =
                ActiveSession;

            SparkToolContext context =
                new SparkToolContext(
                    this,
                    toolController,
                    session,
                    session.LastHit);

            Publish(
                tool.Cancel(context));

            ActiveSession = null;
        }

        // ========================================================
        // CONTEXT
        // ========================================================

        public SparkInteractionContext
            CreateInteractionContext(
                in SparkTargetHit hit,
                SparkInteractionType type)
        {
            Vector2 pointer =
                ReadPointerPosition();

            return new SparkInteractionContext(
                this,
                hit.Target,
                type,
                toolController != null
                    ? toolController.ActiveToolType
                    : SparkToolType.Select,
                hit.Point,
                hit.Normal,
                pointer,
                hit.Collider != null
                    ? hit.Collider.gameObject
                    : null);
        }

        // ========================================================
        // TARGET STATE
        // ========================================================

        private void SetCurrentTarget(
            SparkTargetHit? next)
        {
            if (CurrentTarget.HasValue ==
                next.HasValue &&
                (!CurrentTarget.HasValue ||
                 CurrentTarget.Value.Target ==
                 next.Value.Target))
            {
                return;
            }

            if (CurrentTarget.HasValue)
            {
                CurrentTarget.Value.Target
                    .SetHovered(false);
            }

            CurrentTarget = next;

            if (CurrentTarget.HasValue)
            {
                CurrentTarget.Value.Target
                    .SetHovered(true);
            }

            TargetChanged?.Invoke(
                CurrentTarget);
        }

        // ========================================================
        // POINTER
        // ========================================================

        private Vector2 ReadPointerPosition()
        {
            if (pointerAction != null)
            {
                return pointerAction.action
                    .ReadValue<Vector2>();
            }

            if (Pointer.current != null)
            {
                return Pointer.current.position
                    .ReadValue();
            }

            return new Vector2(
                Screen.width * 0.5f,
                Screen.height * 0.5f);
        }

        // ========================================================
        // UI
        // ========================================================

        private bool IsPointerOverUI()
        {
            return eventSystem != null &&
                   eventSystem.IsPointerOverGameObject();
        }

        private void Publish(
            SparkResult result)
        {
            ResultProduced?.Invoke(result);
        }

        // ========================================================
        // TOOL → INTERACTION
        // ========================================================

        private static SparkInteractionType
            MapInteraction(SparkToolType type)
        {
            switch (type)
            {
                case SparkToolType.Select:
                    return SparkInteractionType.Select;

                case SparkToolType.Inspect:
                    return SparkInteractionType.Inspect;

                case SparkToolType.Move:
                    return SparkInteractionType.Move;

                case SparkToolType.Rotate:
                    return SparkInteractionType.Rotate;

                case SparkToolType.Wire:
                    return SparkInteractionType.Connect;

                case SparkToolType.Measure:
                    return SparkInteractionType.Measure;

                case SparkToolType.Probe:
                    return SparkInteractionType.Probe;

                case SparkToolType.Scan:
                    return SparkInteractionType.Scan;

                default:
                    return SparkInteractionType.None;
            }
        }
    }
}