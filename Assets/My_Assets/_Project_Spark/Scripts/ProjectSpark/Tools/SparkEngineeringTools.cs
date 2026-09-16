using ProjectSpark.Gameplay;
using ProjectSpark.Measurement;
using ProjectSpark.Scan;
using UnityEngine;
using System;

namespace ProjectSpark.Tools
{
    /// <summary>
    /// Central controller for the Project Spark tool system.
    ///
    /// This controller does not implement tool-specific behaviour.
    /// Actual behaviour remains inside individual SparkTool implementations.
    ///
    /// Responsibilities:
    /// - Tool registration
    /// - Tool selection
    /// - Active tool state
    /// - Operation lifecycle
    /// - Begin / Tick / End / Cancel forwarding
    /// - Safe tool switching
    /// - Duplicate prevention
    /// - Runtime registration
    /// - Runtime unregistration
    /// - Default tool
    /// - Controller enable / disable
    /// - Operation locking
    /// - Last result
    /// - Lifecycle events
    ///
    /// Architecture:
    ///
    /// Input
    ///     ↓
    /// SparkToolController
    ///     ↓
    /// External SparkTool
    ///     ↓
    /// Gameplay / Interaction / Simulation
    ///
    /// The controller never contains tool-specific logic.
    /// </summary>
    public sealed class SparkToolController : MonoBehaviour
    {
        // ============================================================
        // INSPECTOR
        // ============================================================

        [Header("Tool Registry")]
        [Tooltip(
            "All externally-created SparkTool components controlled by this controller.")]
        [SerializeField]
        private SparkTool[] tools;

        [Header("Default Tool")]
        [Tooltip(
            "Optional tool selected automatically when the controller initializes.")]
        [SerializeField]
        private SparkTool defaultTool;

        [Header("Behaviour")]
        [Tooltip(
            "Automatically select the default tool during Awake.")]
        [SerializeField]
        private bool selectDefaultToolOnAwake = true;

        [Tooltip(
            "Automatically cancel an active operation when switching tools.")]
        [SerializeField]
        private bool cancelOperationWhenSwitchingTool = true;

        [Tooltip(
            "Automatically cancel an active operation when this controller is disabled.")]
        [SerializeField]
        private bool cancelOperationOnDisable = true;

        [Tooltip(
            "Reject Begin calls while another operation is already active.")]
        [SerializeField]
        private bool lockWhileOperating = true;

        [Tooltip(
            "Reject duplicate tool registrations.")]
        [SerializeField]
        private bool preventDuplicateTools = true;

        [Header("Diagnostics")]
        [SerializeField]
        private bool logToolChanges;

        [SerializeField]
        private bool logOperationLifecycle;


        // ============================================================
        // RUNTIME STATE
        // ============================================================

        private SparkTool activeTool;

        private SparkTool operatingTool;

        private SparkToolContext activeContext;

        private bool operationActive;

        private bool initialized;

        private SparkResult lastResult;


        // ============================================================
        // EVENTS
        // ============================================================

        /// <summary>
        /// Fired whenever the selected tool changes.
        /// Null means no tool is selected.
        /// </summary>
        public event Action<SparkTool> ToolChanged;

        /// <summary>
        /// Fired when a tool operation successfully begins.
        /// </summary>
        public event Action<SparkTool> OperationStarted;

        /// <summary>
        /// Fired when a tool operation ends successfully.
        /// </summary>
        public event Action<SparkTool> OperationEnded;

        /// <summary>
        /// Fired when a tool operation is cancelled.
        /// </summary>
        public event Action<SparkTool> OperationCancelled;

        /// <summary>
        /// Fired whenever the controller receives a new result.
        /// </summary>
        public event Action<SparkResult> ResultProduced;


        // ============================================================
        // PUBLIC STATE
        // ============================================================

        /// <summary>
        /// Currently selected tool.
        /// </summary>
        public SparkTool ActiveTool => activeTool;

        /// <summary>
        /// Tool currently performing an operation.
        /// </summary>
        public SparkTool OperatingTool => operatingTool;

        /// <summary>
        /// True when a tool operation is currently active.
        /// </summary>
        public bool IsOperating => operationActive;

        /// <summary>
        /// True when a tool is selected.
        /// </summary>
        public bool HasActiveTool => activeTool != null;

        /// <summary>
        /// Last result generated by the controller/tool.
        /// </summary>
        public SparkResult LastResult => lastResult;

        /// <summary>
        /// Number of registered tools.
        /// </summary>
        public int ToolCount =>
            tools == null ? 0 : tools.Length;

        /// <summary>
        /// True after initialization has completed.
        /// </summary>
        public bool IsInitialized => initialized;

        /// <summary>
        /// Context belonging to the currently active operation.
        /// </summary>
        public SparkToolContext ActiveContext => activeContext;


        // ============================================================
        // UNITY LIFECYCLE
        // ============================================================

        private void Awake()
        {
            Initialize();
        }

        private void OnDisable()
        {
            if (!cancelOperationOnDisable)
            {
                return;
            }

            if (!operationActive)
            {
                return;
            }

            CancelOperation();
        }


        // ============================================================
        // INITIALIZATION
        // ============================================================

        /// <summary>
        /// Initializes the controller.
        /// Safe to call more than once.
        /// </summary>
        public void Initialize()
        {
            if (initialized)
            {
                return;
            }

            initialized = true;

            CleanupNullTools();

            ValidateRegistry();

            if (!selectDefaultToolOnAwake ||
                defaultTool == null)
            {
                return;
            }

            if (IsRegistered(defaultTool))
            {
                SelectToolInternal(
                    defaultTool,
                    false);

                return;
            }

            Debug.LogWarning(
                $"Default tool '{defaultTool.name}' " +
                "is not registered with this controller.",
                this);
        }


        // ============================================================
        // TOOL SELECTION
        // ============================================================

        /// <summary>
        /// Selects a tool by SparkToolType.
        /// </summary>
        public SparkResult SelectTool(
            SparkToolType toolType)
        {
            SparkTool tool =
                FindTool(toolType);

            if (tool == null)
            {
                return PublishResult(
                    SparkResult.Unavailable(
                        $"Tool '{toolType}' is not registered."));
            }

            return SelectTool(tool);
        }

        /// <summary>
        /// Selects a specific externally-created tool.
        /// </summary>
        public SparkResult SelectTool(
            SparkTool tool)
        {
            if (tool == null)
            {
                return PublishResult(
                    SparkResult.Invalid(
                        "Cannot select a null tool."));
            }

            if (!IsRegistered(tool))
            {
                return PublishResult(
                    SparkResult.Unavailable(
                        $"Tool '{tool.name}' is not registered."));
            }

            return SelectToolInternal(
                tool,
                true);
        }

        private SparkResult SelectToolInternal(
            SparkTool tool,
            bool notify)
        {
            if (activeTool == tool)
            {
                return PublishResult(
                    SparkResult.Success(
                        $"Tool '{tool.ToolType}' is already selected."));
            }

            if (operationActive)
            {
                if (!cancelOperationWhenSwitchingTool)
                {
                    return PublishResult(
                        SparkResult.Rejected(
                            "Cannot switch tools while an operation is active."));
                }

                SparkResult cancelResult =
                    CancelOperation();

                if (!cancelResult.Succeeded)
                {
                    return PublishResult(
                        SparkResult.Rejected(
                            "Tool switch failed because the active operation " +
                            "could not be cancelled."));
                }
            }

            activeTool = tool;

            if (logToolChanges)
            {
                Debug.Log(
                    $"[SparkToolController] " +
                    $"Active Tool → {tool.ToolType}",
                    this);
            }

            if (notify)
            {
                ToolChanged?.Invoke(activeTool);
            }

            return PublishResult(
                SparkResult.Success(
                    $"Tool '{tool.ToolType}' selected."));
        }


        // ============================================================
        // CLEAR TOOL
        // ============================================================

        /// <summary>
        /// Clears the currently selected tool.
        /// </summary>
        public SparkResult ClearTool()
        {
            if (operationActive)
            {
                SparkResult cancelResult =
                    CancelOperation();

                if (!cancelResult.Succeeded)
                {
                    return PublishResult(
                        SparkResult.Rejected(
                            "Cannot clear tool while an operation is active."));
                }
            }

            activeTool = null;

            ToolChanged?.Invoke(null);

            return PublishResult(
                SparkResult.Success(
                    "Tool selection cleared."));
        }


        // ============================================================
        // BEGIN
        // ============================================================

        /// <summary>
        /// Begins an operation on the currently selected tool.
        /// </summary>
        public SparkResult Begin(
            SparkToolContext context)
        {
            if (!initialized)
            {
                Initialize();
            }

            if (context == null)
            {
                return PublishResult(
                    SparkResult.Invalid(
                        "Cannot begin a tool operation with a null context."));
            }

            if (activeTool == null)
            {
                return PublishResult(
                    SparkResult.Unavailable(
                        "No tool is selected."));
            }

            if (lockWhileOperating &&
                operationActive)
            {
                return PublishResult(
                    SparkResult.Rejected(
                        "A tool operation is already active."));
            }

            if (!activeTool.CanBegin(
                    context,
                    out string reason))
            {
                return PublishResult(
                    SparkResult.Rejected(
                        string.IsNullOrEmpty(reason)
                            ? "Tool cannot begin."
                            : reason));
            }

            SparkResult result =
                activeTool.Begin(context);

            if (!result.Succeeded)
            {
                return PublishResult(result);
            }

            operatingTool = activeTool;
            activeContext = context;
            operationActive = true;

            if (logOperationLifecycle)
            {
                Debug.Log(
                    $"[SparkToolController] " +
                    $"BEGIN → {operatingTool.ToolType}",
                    this);
            }

            OperationStarted?.Invoke(
                operatingTool);

            return PublishResult(result);
        }


        // ============================================================
        // UPDATE / TICK
        // ============================================================

        /// <summary>
        /// Updates the currently running operation.
        ///
        /// The operation is always forwarded to the exact tool
        /// that successfully started the operation.
        /// </summary>
       public SparkResult Tick(
    SparkToolContext context)
        {
            if (!operationActive ||
                operatingTool == null)
            {
                return PublishResult(
                    SparkResult.Rejected(
                        "No tool operation is active."));
            }

            if (context == null)
            {
                return PublishResult(
                    SparkResult.Invalid(
                        "Cannot tick a tool operation with a null context."));
            }

            activeContext = context;

            SparkResult result =
                operatingTool.Tick(context);

            return PublishResult(result);
            }


        // ============================================================
        // END
        // ============================================================

        /// <summary>
        /// Completes the active tool operation.
        /// </summary>
        public SparkResult End(
            SparkToolContext context)
        {
            if (!operationActive ||
                operatingTool == null)
            {
                return PublishResult(
                    SparkResult.Rejected(
                        "No tool operation is active."));
            }

            if (context == null)
            {
                return PublishResult(
                    SparkResult.Invalid(
                        "Cannot end a tool operation with a null context."));
            }

            activeContext = context;

            SparkTool tool =
                operatingTool;

            SparkResult result =
                tool.End(context);

            if (result.Succeeded)
            {
                operationActive = false;
                operatingTool = null;
                activeContext = null;

                if (logOperationLifecycle)
                {
                    Debug.Log(
                        $"[SparkToolController] " +
                        $"END → {tool.ToolType}",
                        this);
                }

                OperationEnded?.Invoke(tool);
            }

            return PublishResult(result);
        }


        // ============================================================
        // CANCEL
        // ============================================================

        /// <summary>
        /// Cancels the active tool operation.
        ///
        /// The operation is cancelled on the exact tool that
        /// started it, using its active operation context.
        /// </summary>
        public SparkResult CancelOperation()
        {
            if (!operationActive ||
                operatingTool == null)
            {
                return PublishResult(
                    SparkResult.Cancelled(
                        "No active tool operation."));
            }

            SparkTool tool =
                operatingTool;

            SparkToolContext context =
                activeContext;

            if (context == null)
            {
                return PublishResult(
                    SparkResult.Invalid(
                        "Active tool operation has no context."));
            }

            SparkResult result =
                tool.Cancel(context);

            operationActive = false;
            operatingTool = null;
            activeContext = null;

            if (logOperationLifecycle)
            {
                Debug.Log(
                    $"[SparkToolController] " +
                    $"CANCEL → {tool.ToolType}",
                    this);
            }

            OperationCancelled?.Invoke(tool);

            return PublishResult(result);
        }


        // ============================================================
        // FORCE CANCEL
        // ============================================================

        /// <summary>
        /// Emergency cleanup.
        ///
        /// Guarantees that the controller does not retain
        /// an active operation after this method returns.
        /// </summary>
        public void ForceCancel()
        {
            if (!operationActive ||
                operatingTool == null)
            {
                operationActive = false;
                operatingTool = null;
                activeContext = null;
                return;
            }

            SparkTool tool =
                operatingTool;

            SparkToolContext context =
                activeContext;

            try
            {
                if (context != null)
                {
                    tool.Cancel(context);
                }
            }
            catch (Exception exception)
            {
                Debug.LogException(
                    exception,
                    this);
            }

            operationActive = false;
            operatingTool = null;
            activeContext = null;

            OperationCancelled?.Invoke(tool);
        }


        // ============================================================
        // TOOL LOOKUP
        // ============================================================

        /// <summary>
        /// Finds a registered tool by type.
        /// </summary>
        public SparkTool FindTool(
            SparkToolType toolType)
        {
            if (tools == null)
            {
                return null;
            }

            for (int i = 0; i < tools.Length; i++)
            {
                SparkTool tool =
                    tools[i];

                if (tool == null)
                {
                    continue;
                }

                if (tool.ToolType == toolType)
                {
                    return tool;
                }
            }

            return null;
        }

        /// <summary>
        /// Returns a registered tool by array index.
        /// </summary>
        public SparkTool GetTool(
            int index)
        {
            if (tools == null)
            {
                return null;
            }

            if (index < 0 ||
                index >= tools.Length)
            {
                return null;
            }

            return tools[index];
        }

        /// <summary>
        /// Returns a tool of type T.
        /// </summary>
        public T FindTool<T>()
            where T : SparkTool
        {
            if (tools == null)
            {
                return null;
            }

            for (int i = 0; i < tools.Length; i++)
            {
                if (tools[i] is T typedTool)
                {
                    return typedTool;
                }
            }

            return null;
        }


        // ============================================================
        // REGISTRATION
        // ============================================================

        /// <summary>
        /// Registers an external SparkTool at runtime.
        /// </summary>
        public SparkResult RegisterTool(
            SparkTool tool)
        {
            if (tool == null)
            {
                return PublishResult(
                    SparkResult.Invalid(
                        "Cannot register a null tool."));
            }

            if (IsRegistered(tool))
            {
                if (preventDuplicateTools)
                {
                    return PublishResult(
                        SparkResult.Rejected(
                            $"Tool '{tool.name}' is already registered."));
                }

                return PublishResult(
                    SparkResult.Success(
                        $"Tool '{tool.name}' is already registered."));
            }

            SparkTool existing =
                FindTool(tool.ToolType);

            if (existing != null &&
                existing != tool)
            {
                return PublishResult(
                    SparkResult.Rejected(
                        $"A tool of type '{tool.ToolType}' " +
                        "is already registered."));
            }

            int oldLength =
                tools == null
                    ? 0
                    : tools.Length;

            SparkTool[] newTools =
                new SparkTool[oldLength + 1];

            for (int i = 0; i < oldLength; i++)
            {
                newTools[i] =
                    tools[i];
            }

            newTools[oldLength] =
                tool;

            tools =
                newTools;

            return PublishResult(
                SparkResult.Success(
                    $"Tool '{tool.ToolType}' registered."));
        }


        // ============================================================
        // UNREGISTER
        // ============================================================

        /// <summary>
        /// Removes an external tool from the registry.
        /// </summary>
        public SparkResult UnregisterTool(
            SparkTool tool)
        {
            if (tool == null)
            {
                return PublishResult(
                    SparkResult.Invalid(
                        "Cannot unregister a null tool."));
            }

            if (!IsRegistered(tool))
            {
                return PublishResult(
                    SparkResult.Unavailable(
                        $"Tool '{tool.name}' is not registered."));
            }

            if (operatingTool == tool)
            {
                SparkResult cancelResult =
                    CancelOperation();

                if (!cancelResult.Succeeded)
                {
                    return PublishResult(
                        SparkResult.Rejected(
                            $"Cannot unregister tool '{tool.name}' " +
                            "because its active operation could not be cancelled."));
                }
            }

            bool wasActive =
                activeTool == tool;

            if (wasActive)
            {
                activeTool = null;
            }

            int removeIndex = -1;

            for (int i = 0; i < tools.Length; i++)
            {
                if (tools[i] == tool)
                {
                    removeIndex = i;
                    break;
                }
            }

            if (removeIndex < 0)
            {
                return PublishResult(
                    SparkResult.Unavailable(
                        "Tool registry changed unexpectedly."));
            }

            SparkTool[] newTools =
                new SparkTool[tools.Length - 1];

            int writeIndex = 0;

            for (int i = 0; i < tools.Length; i++)
            {
                if (i == removeIndex)
                {
                    continue;
                }

                newTools[writeIndex] =
                    tools[i];

                writeIndex++;
            }

            tools =
                newTools;

            if (wasActive)
            {
                ToolChanged?.Invoke(null);
            }

            return PublishResult(
                SparkResult.Success(
                    $"Tool '{tool.ToolType}' unregistered."));
        }


        // ============================================================
        // REGISTRATION CHECK
        // ============================================================

        /// <summary>
        /// Returns true if the tool is currently registered.
        /// </summary>
        public bool IsRegistered(
            SparkTool tool)
        {
            if (tool == null ||
                tools == null)
            {
                return false;
            }

            for (int i = 0; i < tools.Length; i++)
            {
                if (tools[i] == tool)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Returns true if a tool of this type is registered.
        /// </summary>
        public bool IsToolAvailable(
            SparkToolType toolType)
        {
            return FindTool(toolType) != null;
        }


        // ============================================================
        // STATE QUERIES
        // ============================================================

        /// <summary>
        /// Checks whether the specified tool is currently selected.
        /// </summary>
        public bool IsSelected(
            SparkTool tool)
        {
            return activeTool == tool;
        }

        /// <summary>
        /// Checks whether the specified tool is currently operating.
        /// </summary>
        public bool IsToolOperating(
            SparkTool tool)
        {
            return operationActive &&
                   operatingTool == tool;
        }

        /// <summary>
        /// Checks whether a tool type is currently selected.
        /// </summary>
        public bool IsSelected(
            SparkToolType toolType)
        {
            return activeTool != null &&
                   activeTool.ToolType == toolType;
        }

        /// <summary>
        /// Checks whether a tool type is currently operating.
        /// </summary>
        public bool IsToolOperating(
            SparkToolType toolType)
        {
            return operationActive &&
                   operatingTool != null &&
                   operatingTool.ToolType == toolType;
        }


        // ============================================================
        // VALIDATION
        // ============================================================

        private void ValidateRegistry()
        {
            if (tools == null)
            {
                return;
            }

            for (int i = 0; i < tools.Length; i++)
            {
                SparkTool current =
                    tools[i];

                if (current == null)
                {
                    Debug.LogWarning(
                        $"[SparkToolController] " +
                        $"Null tool at registry index {i}.",
                        this);

                    continue;
                }

                for (int j = i + 1;
                     j < tools.Length;
                     j++)
                {
                    SparkTool other =
                        tools[j];

                    if (other == null)
                    {
                        continue;
                    }

                    if (current == other)
                    {
                        Debug.LogWarning(
                            $"[SparkToolController] " +
                            $"Duplicate tool reference: " +
                            $"'{current.name}'.",
                            this);
                    }
                    else if (current.ToolType ==
                             other.ToolType)
                    {
                        Debug.LogWarning(
                            $"[SparkToolController] " +
                            $"Multiple tools use SparkToolType " +
                            $"'{current.ToolType}'. " +
                            $"Only the first tool will be returned " +
                            $"by FindTool().",
                            this);
                    }
                }
            }
        }


        // ============================================================
        // CLEANUP
        // ============================================================

        private void CleanupNullTools()
        {
            if (tools == null ||
                tools.Length == 0)
            {
                return;
            }

            int validCount = 0;

            for (int i = 0; i < tools.Length; i++)
            {
                if (tools[i] != null)
                {
                    validCount++;
                }
            }

            if (validCount == tools.Length)
            {
                return;
            }

            SparkTool[] cleaned =
                new SparkTool[validCount];

            int index = 0;

            for (int i = 0; i < tools.Length; i++)
            {
                if (tools[i] == null)
                {
                    continue;
                }

                cleaned[index] =
                    tools[i];

                index++;
            }

            tools =
                cleaned;
        }


        // ============================================================
        // RESULT
        // ============================================================

        private SparkResult PublishResult(
            SparkResult result)
        {
            lastResult =
                result;

            ResultProduced?.Invoke(
                result);

            return result;
        }
    }
}