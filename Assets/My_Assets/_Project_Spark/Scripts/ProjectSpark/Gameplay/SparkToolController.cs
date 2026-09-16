using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace ProjectSpark.Gameplay
{
    [DisallowMultipleComponent]
    public sealed class SparkToolController : MonoBehaviour
    {
        // ============================================================
        // TOOLS
        // ============================================================

        [Header("Tools")]
        [SerializeField]
        private List<SparkTool> tools = new();

        private readonly Dictionary<SparkToolType, SparkTool> map =
            new();


        // ============================================================
        // INPUT
        // ============================================================

        [Header("Tool Input")]
        [SerializeField]
        private InputActionReference selectToolAction;

        [SerializeField]
        private InputActionReference inspectToolAction;

        [SerializeField]
        private InputActionReference moveToolAction;

        [SerializeField]
        private InputActionReference rotateToolAction;

        [SerializeField]
        private InputActionReference wireToolAction;

        [SerializeField]
        private InputActionReference measureToolAction;

        [SerializeField]
        private InputActionReference scanToolAction;


        // ============================================================
        // ACTIVE TOOL
        // ============================================================

        public SparkTool ActiveTool
        {
            get;
            private set;
        }

        public SparkToolType ActiveToolType
        {
            get;
            private set;
        } = SparkToolType.Select;


        // ============================================================
        // EVENTS
        // ============================================================

        public event Action<SparkTool, SparkTool>
            ActiveToolChanged;


        // ============================================================
        // UNITY
        // ============================================================

        private void Awake()
        {
            BuildToolMap();

            TrySetTool(
                ActiveToolType,
                out _);
        }

        private void OnEnable()
        {
            RegisterInput(
                selectToolAction,
                OnSelectToolInput);

            RegisterInput(
                inspectToolAction,
                OnInspectToolInput);

            RegisterInput(
                moveToolAction,
                OnMoveToolInput);

            RegisterInput(
                rotateToolAction,
                OnRotateToolInput);

            RegisterInput(
                wireToolAction,
                OnWireToolInput);

            RegisterInput(
                measureToolAction,
                OnMeasureToolInput);

            RegisterInput(
                scanToolAction,
                OnScanToolInput);
        }

        private void OnDisable()
        {
            UnregisterInput(
                selectToolAction,
                OnSelectToolInput);

            UnregisterInput(
                inspectToolAction,
                OnInspectToolInput);

            UnregisterInput(
                moveToolAction,
                OnMoveToolInput);

            UnregisterInput(
                rotateToolAction,
                OnRotateToolInput);

            UnregisterInput(
                wireToolAction,
                OnWireToolInput);

            UnregisterInput(
                measureToolAction,
                OnMeasureToolInput);

            UnregisterInput(
                scanToolAction,
                OnScanToolInput);
        }


        // ============================================================
        // BUILD TOOL MAP
        // ============================================================

        private void BuildToolMap()
        {
            map.Clear();

            for (int i = 0; i < tools.Count; i++)
            {
                SparkTool tool = tools[i];

                if (tool == null)
                    continue;

                SparkToolType type =
                    tool.ToolType;

                if (map.ContainsKey(type))
                {
                    Debug.LogError(
                        $"Duplicate Spark tool: {type}",
                        this);

                    continue;
                }

                map.Add(
                    type,
                    tool);
            }
        }


        // ============================================================
        // TOOL SWITCHING
        // ============================================================

        public bool TrySetTool(
            SparkToolType type,
            out string reason)
        {
            // --------------------------------------------------------
            // Current tool is busy
            // --------------------------------------------------------

            if (ActiveTool != null &&
                ActiveTool.IsBusy)
            {
                reason =
                    "Active tool has an interaction session.";

                return false;
            }


            // --------------------------------------------------------
            // Find requested tool
            // --------------------------------------------------------

            if (!map.TryGetValue(
                    type,
                    out SparkTool next) ||
                next == null)
            {
                reason =
                    $"Tool '{type}' is not configured.";

                return false;
            }


            // --------------------------------------------------------
            // Already active
            // --------------------------------------------------------

            if (ActiveTool == next)
            {
                reason = null;
                return true;
            }


            // --------------------------------------------------------
            // Change tool
            // --------------------------------------------------------

            SparkTool previous =
                ActiveTool;

            ActiveTool =
                next;

            ActiveToolType =
                type;


            // --------------------------------------------------------
            // Notify listeners
            // --------------------------------------------------------

            ActiveToolChanged?.Invoke(
                previous,
                ActiveTool);


            reason = null;

            return true;
        }


        // ============================================================
        // INPUT REGISTRATION
        // ============================================================

        private void RegisterInput(
            InputActionReference actionReference,
            Action<InputAction.CallbackContext> callback)
        {
            if (actionReference == null)
                return;

            InputAction action =
                actionReference.action;

            if (action == null)
                return;

            action.performed += callback;
            action.Enable();
        }

        private void UnregisterInput(
            InputActionReference actionReference,
            Action<InputAction.CallbackContext> callback)
        {
            if (actionReference == null)
                return;

            InputAction action =
                actionReference.action;

            if (action == null)
                return;

            action.performed -= callback;
            action.Disable();
        }


        // ============================================================
        // INPUT → SELECT
        // ============================================================

        private void OnSelectToolInput(
            InputAction.CallbackContext context)
        {
            TryActivateTool(
                SparkToolType.Select);
        }


        // ============================================================
        // INPUT → INSPECT
        // ============================================================

        private void OnInspectToolInput(
            InputAction.CallbackContext context)
        {
            TryActivateTool(
                SparkToolType.Inspect);
        }


        // ============================================================
        // INPUT → MOVE
        // ============================================================

        private void OnMoveToolInput(
            InputAction.CallbackContext context)
        {
            TryActivateTool(
                SparkToolType.Move);
        }


        // ============================================================
        // INPUT → ROTATE
        // ============================================================

        private void OnRotateToolInput(
            InputAction.CallbackContext context)
        {
            TryActivateTool(
                SparkToolType.Rotate);
        }


        // ============================================================
        // INPUT → WIRE
        // ============================================================

        private void OnWireToolInput(
            InputAction.CallbackContext context)
        {
            TryActivateTool(
                SparkToolType.Wire);
        }


        // ============================================================
        // INPUT → MEASURE
        // ============================================================

        private void OnMeasureToolInput(
            InputAction.CallbackContext context)
        {
            TryActivateTool(
                SparkToolType.Measure);
        }


        // ============================================================
        // INPUT → SCAN
        // ============================================================

        private void OnScanToolInput(
            InputAction.CallbackContext context)
        {
            TryActivateTool(
                SparkToolType.Scan);
        }
        // ============================================================
        // UI BUTTON → SELECT
        // ============================================================

        public void OnSelectButton()
        {
            TryActivateTool(
                SparkToolType.Select);
        }


        // ============================================================
        // UI BUTTON → INSPECT
        // ============================================================

        public void OnInspectButton()
        {
            TryActivateTool(
                SparkToolType.Inspect);
        }


        // ============================================================
        // UI BUTTON → MOVE
        // ============================================================

        public void OnMoveButton()
        {
            TryActivateTool(
                SparkToolType.Move);
        }


        // ============================================================
        // UI BUTTON → ROTATE
        // ============================================================

        public void OnRotateButton()
        {
            TryActivateTool(
                SparkToolType.Rotate);
        }


        // ============================================================
        // UI BUTTON → WIRE
        // ============================================================

        public void OnWireButton()
        {
            TryActivateTool(
                SparkToolType.Wire);
        }


        // ============================================================
        // UI BUTTON → MEASURE
        // ============================================================

        public void OnMeasureButton()
        {
            TryActivateTool(
                SparkToolType.Measure);
        }


        // ============================================================
        // UI BUTTON → SCAN
        // ============================================================

        public void OnScanButton()
        {
            TryActivateTool(
                SparkToolType.Scan);
        }


        // ============================================================
        // ACTIVATE TOOL
        // ============================================================

        private void TryActivateTool(
            SparkToolType type)
        {
            if (TrySetTool(
                    type,
                    out string reason))
            {
                return;
            }

            if (!string.IsNullOrEmpty(reason))
            {
                Debug.Log(
                    $"Spark Tool: {reason}",
                    this);
            }
        }
    }
}