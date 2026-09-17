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
        [SerializeField]
        private Camera interactionCamera;

        [SerializeField]
        private SparkToolController toolController;

        [SerializeField]
        private EventSystem eventSystem;

        [Header("Targeting")]
        [SerializeField]
        private LayerMask interactionLayers = ~0;

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

        public SparkTargetHit? CurrentTarget
        {
            get;
            private set;
        }

        public SparkInteractionSession ActiveSession
        {
            get;
            private set;
        }

        public bool HasActiveInteraction
        {
            get
            {
                return ActiveSession != null &&
                       ActiveSession.IsActive;
            }
        }

        public Camera InteractionCamera
        {
            get
            {
                return interactionCamera;
            }
        }

        public SparkToolController ToolController
        {
            get
            {
                return toolController;
            }
        }

        public Vector2 PointerPosition
        {
            get
            {
                return ReadPointerPosition();
            }
        }
        private bool pointerOverUI;
        

        public event Action<SparkTargetHit?> TargetChanged;

        public event Action<SparkResult> ResultProduced;

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
            {
                toolController =
                    GetComponent<SparkToolController>();
            }

            if (interactionCamera == null)
            {
                interactionCamera =
                    Camera.main;
            }

            if (eventSystem == null)
            {
                eventSystem =
                    EventSystem.current;
            }
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
            {
                pointerAction.action.Enable();
            }
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
            {
                pointerAction.action.Disable();
            }

            CancelActiveInteraction();

            SetCurrentTarget(null);
        }
private void Update()
{
    pointerOverUI = IsPointerOverUI();

    RefreshTarget();

    if (ActiveSession == null)
    {
        return;
    }

    if (toolController == null)
    {
        return;
    }

    SparkTool activeTool =
        toolController.ActiveTool;

    if (activeTool == null)
    {
        return;
    }

    if (CurrentTarget.HasValue)
    {
        ActiveSession.UpdateHit(
            CurrentTarget.Value);
    }

    SparkTargetHit hit =
        ActiveSession.LastHit;

    Vector2 pointer =
        ReadPointerPosition();

    SparkInteractionContext interactionContext =
        CreateInteractionContext(
            in hit,
            ActiveSession.InteractionType);

    SparkToolContext context =
        new SparkToolContext(
            this,
            toolController,
            ActiveSession,
            in hit,
            pointer,
            interactionCamera,
            interactionContext);

    SparkResult result =
        activeTool.Tick(context);

    if (!result.Succeeded &&
        result.Code != SparkResultCode.Unavailable)
    {
        Publish(result);
    }
}
       private void RefreshTarget()
{
    if (interactionCamera == null)
    {
        return;
    }

    if (blockWorldInputOverUI &&
        pointerOverUI)
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
        hit.collider.GetComponentInParent<
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

        private void OnPrimaryStarted(
            InputAction.CallbackContext _)
        {
            if (blockWorldInputOverUI &&
                pointerOverUI)
            {
                return;
            }

            if (ActiveSession != null)
            {
                return;
            }

            if (toolController == null)
            {
                Publish(
                    SparkResult.Unavailable(
                        "SparkToolController is not configured."));

                return;
            }

            SparkTool tool =
                toolController.ActiveTool;

            if (tool == null)
            {
                Publish(
                    SparkResult.Unavailable(
                        "No active tool."));

                return;
            }

            if (!CurrentTarget.HasValue)
            {
                Publish(
                    SparkResult.Rejected(
                        "No interactive target under pointer."));

                return;
            }

            SparkTargetHit hit =
                CurrentTarget.Value;

            SparkInteractionType type =
                MapInteraction(
                    tool.ToolType);

            if (type == SparkInteractionType.None)
            {
                Publish(
                    SparkResult.Rejected(
                        "Active tool has no interaction type."));

                return;
            }

            SparkInteractionContext interactionContext =
                CreateInteractionContext(
                    in hit,
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
                    in hit);

            Vector2 pointer =
                ReadPointerPosition();

            SparkToolContext toolContext =
                new SparkToolContext(
                    this,
                    toolController,
                    session,
                    in hit,
                    pointer,
                    interactionCamera,
                    interactionContext);

            SparkResult result =
                tool.Begin(toolContext);

            Publish(result);

            if (result.Succeeded)
            {
                ActiveSession = session;
            }
        }

        private void OnPrimaryCanceled(
            InputAction.CallbackContext _)
        {
            if (ActiveSession == null)
            {
                return;
            }

            if (toolController == null)
            {
                ActiveSession = null;
                return;
            }

            SparkTool tool =
                toolController.ActiveTool;

            if (tool == null)
            {
                ActiveSession = null;
                return;
            }

            SparkInteractionSession session =
                ActiveSession;

            SparkTargetHit hit =
                session.LastHit;

            Vector2 pointer =
                ReadPointerPosition();

            SparkInteractionContext interactionContext =
                CreateInteractionContext(
                    in hit,
                    session.InteractionType);

            SparkToolContext context =
                new SparkToolContext(
                    this,
                    toolController,
                    session,
                    in hit,
                    pointer,
                    interactionCamera,
                    interactionContext);

            SparkResult result =
                tool.End(context);

            Publish(result);

            ActiveSession = null;
        }

        private void OnCancelPerformed(
            InputAction.CallbackContext _)
        {
            CancelActiveInteraction();
        }

        private void CancelActiveInteraction()
        {
            if (ActiveSession == null)
            {
                return;
            }

            if (toolController == null)
            {
                ActiveSession = null;
                return;
            }

            SparkTool tool =
                toolController.ActiveTool;

            if (tool == null)
            {
                ActiveSession = null;
                return;
            }

            SparkInteractionSession session =
                ActiveSession;

            SparkTargetHit hit =
                session.LastHit;

            Vector2 pointer =
                ReadPointerPosition();

            SparkInteractionContext interactionContext =
                CreateInteractionContext(
                    in hit,
                    session.InteractionType);

            SparkToolContext context =
                new SparkToolContext(
                    this,
                    toolController,
                    session,
                    in hit,
                    pointer,
                    interactionCamera,
                    interactionContext);

            SparkResult result =
                tool.Cancel(context);

            Publish(result);

            ActiveSession = null;
        }

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
                if (CurrentTarget.Value.Target != null)
                {
                    CurrentTarget.Value.Target
                        .SetHovered(false);
                }
            }

            CurrentTarget = next;

            if (CurrentTarget.HasValue)
            {
                if (CurrentTarget.Value.Target != null)
                {
                    CurrentTarget.Value.Target
                        .SetHovered(true);
                }
            }

            TargetChanged?.Invoke(
                CurrentTarget);
        }

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

        private static SparkInteractionType
            MapInteraction(
                SparkToolType type)
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