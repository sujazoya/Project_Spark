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
        [SerializeField, Min(0.1f)] private float interactionDistance = 8f;
        [SerializeField] private QueryTriggerInteraction triggerInteraction = QueryTriggerInteraction.Ignore;

        [Header("Input")]
        [SerializeField] private InputActionReference primaryAction;
        [SerializeField] private InputActionReference cancelAction;
        [SerializeField] private InputActionReference pointerAction;

        [Header("UI")]
        [SerializeField] private bool blockWorldInputOverUI = true;

        public SparkTargetHit? CurrentTarget { get; private set; }
        public SparkInteractionSession ActiveSession { get; private set; }
        public event Action<SparkTargetHit?> TargetChanged;
        public event Action<SparkResult> ResultProduced;

        private SparkElectronicObject selectedTarget;

        private void Reset()
        {
            toolController = GetComponent<SparkToolController>();
            interactionCamera = Camera.main;
            eventSystem = EventSystem.current;
        }

        private void Awake()
        {
            if (toolController == null) toolController = GetComponent<SparkToolController>();
            if (interactionCamera == null) interactionCamera = Camera.main;
            if (eventSystem == null) eventSystem = EventSystem.current;
        }

        private void OnEnable()
        {
            if (primaryAction != null)
            {
                primaryAction.action.started += OnPrimaryStarted;
                primaryAction.action.canceled += OnPrimaryCanceled;
                primaryAction.action.Enable();
            }
            if (cancelAction != null)
            {
                cancelAction.action.performed += OnCancelPerformed;
                cancelAction.action.Enable();
            }
            if (pointerAction != null) pointerAction.action.Enable();
        }

        private void OnDisable()
        {
            if (primaryAction != null)
            {
                primaryAction.action.started -= OnPrimaryStarted;
                primaryAction.action.canceled -= OnPrimaryCanceled;
                primaryAction.action.Disable();
            }
            if (cancelAction != null)
            {
                cancelAction.action.performed -= OnCancelPerformed;
                cancelAction.action.Disable();
            }
            if (pointerAction != null) pointerAction.action.Disable();
            CancelActiveInteraction();
            ClearSelection();
            SetCurrentTarget(null);
        }

        private void Update()
        {
            RefreshTarget();
            if (ActiveSession == null || toolController.ActiveTool == null) return;

            if (CurrentTarget.HasValue) ActiveSession.UpdateHit(CurrentTarget.Value);
            var hit = ActiveSession.LastHit;
            var context = CreateContext(hit, ActiveSession.InteractionType);
            var toolContext = new SparkToolContext(this, toolController, ActiveSession, hit);
            var result = toolController.ActiveTool.Tick(toolContext);
            if (!result.Succeeded) Publish(result);
        }

        private void RefreshTarget()
        {
            if (interactionCamera == null) return;
            if (blockWorldInputOverUI && IsPointerOverUI()) { SetCurrentTarget(null); return; }
            Vector2 pointer = pointerAction != null ? pointerAction.action.ReadValue<Vector2>() :
                (Pointer.current != null ? Pointer.current.position.ReadValue() : new Vector2(Screen.width * .5f, Screen.height * .5f));
            var ray = interactionCamera.ScreenPointToRay(pointer);
            if (!Physics.Raycast(ray, out var hit, interactionDistance, interactionLayers, triggerInteraction))
            { SetCurrentTarget(null); return; }
            var target = hit.collider.GetComponentInParent<SparkElectronicObject>();
            if (target == null || !target.isActiveAndEnabled || !target.InteractionsEnabled)
            { SetCurrentTarget(null); return; }
            SetCurrentTarget(new SparkTargetHit(target, hit.collider, hit.point, hit.normal, hit.distance));
        }

        private void OnPrimaryStarted(InputAction.CallbackContext _)
        {
            if (blockWorldInputOverUI && IsPointerOverUI()) return;
            if (ActiveSession != null || !CurrentTarget.HasValue) return;
            var tool = toolController.ActiveTool;
            if (tool == null) { Publish(SparkResult.Unavailable("No active tool.")); return; }

            var hit = CurrentTarget.Value;
            var type = MapInteraction(tool.ToolType);
            var context = CreateContext(hit, type);

            if (!hit.Target.CanInteract(context, out var reason))
            { Publish(SparkResult.Rejected(reason)); return; }

            if (type == SparkInteractionType.Select)
            {
                SetSelectedTarget(hit.Target);
                Publish(SparkResult.Success());
                return;
            }

            var session = new SparkInteractionSession(type, tool.ToolType, hit.Target, hit);
            var toolContext = new SparkToolContext(this, toolController, session, hit);
            var result = tool.Begin(toolContext);
            Publish(result);
            if (result.Succeeded) ActiveSession = session;
        }

        private void OnPrimaryCanceled(InputAction.CallbackContext _)
        {
            if (ActiveSession == null) return;
            var session = ActiveSession;
            var hit = session.LastHit;
            var context = CreateContext(hit, session.InteractionType);
            var toolContext = new SparkToolContext(this, toolController, session, hit);
            Publish(toolController.ActiveTool.End(toolContext));
            ActiveSession = null;
        }

        private void OnCancelPerformed(InputAction.CallbackContext _) => CancelActiveInteraction();

        private void CancelActiveInteraction()
        {
            if (ActiveSession == null || toolController.ActiveTool == null) return;
            var session = ActiveSession;
            var hit = session.LastHit;
            var context = CreateContext(hit, session.InteractionType);
            var toolContext = new SparkToolContext(this, toolController, session, hit);
            Publish(toolController.ActiveTool.Cancel(toolContext));
            ActiveSession = null;
        }

        private SparkInteractionContext CreateContext(in SparkTargetHit hit, SparkInteractionType type)
        {
            Vector2 pointer = pointerAction != null ? pointerAction.action.ReadValue<Vector2>() :
                (Pointer.current != null ? Pointer.current.position.ReadValue() : Vector2.zero);
            return new SparkInteractionContext(this, hit.Target, type, toolController.ActiveToolType,
                hit.Point, hit.Normal, pointer, hit.Collider != null ? hit.Collider.gameObject : null);
        }

        private void SetSelectedTarget(SparkElectronicObject target)
        {
            if (target == null)
            {
                ClearSelection();
                return;
            }

            if (selectedTarget == target) return;
            if (selectedTarget != null) selectedTarget.SetSelected(false);
            selectedTarget = target;
            selectedTarget.SetSelected(true);
        }

        private void ClearSelection()
        {
            if (selectedTarget == null) return;
            selectedTarget.SetSelected(false);
            selectedTarget = null;
        }

        private void SetCurrentTarget(SparkTargetHit? next)
        {
            if (CurrentTarget.HasValue == next.HasValue &&
                (!CurrentTarget.HasValue || CurrentTarget.Value.Target == next.Value.Target)) return;
            if (CurrentTarget.HasValue) CurrentTarget.Value.Target.SetHovered(false);
            CurrentTarget = next;
            if (CurrentTarget.HasValue) CurrentTarget.Value.Target.SetHovered(true);
            TargetChanged?.Invoke(CurrentTarget);
        }

        private bool IsPointerOverUI() => eventSystem != null && eventSystem.IsPointerOverGameObject();
        private void Publish(SparkResult result) => ResultProduced?.Invoke(result);

        private static SparkInteractionType MapInteraction(SparkToolType type) => type switch
        {
            SparkToolType.Select => SparkInteractionType.Select,
            SparkToolType.Inspect => SparkInteractionType.Inspect,
            SparkToolType.Move => SparkInteractionType.Move,
            SparkToolType.Rotate => SparkInteractionType.Rotate,
            SparkToolType.Wire => SparkInteractionType.Connect,
            SparkToolType.Measure => SparkInteractionType.Measure,
            SparkToolType.Probe => SparkInteractionType.Probe,
            SparkToolType.Scan => SparkInteractionType.Scan,
            _ => SparkInteractionType.None
        };
    }
}
