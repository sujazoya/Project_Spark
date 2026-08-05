using UnityEngine;

namespace ProjectSpark.UI.VFX
{
    /// <summary>
    /// Runtime debugging and testing utility for Project Spark UI VFX.
    ///
    /// Provides:
    /// - Manual state testing.
    /// - Custom state testing.
    /// - Keyboard shortcuts.
    /// - Automatic state machine resolution.
    /// - Optional debug logging.
    ///
    /// This component does not directly access:
    /// - Materials.
    /// - Shader properties.
    /// - SparkVFXController.
    /// - SparkTMPVFXController.
    ///
    /// All state requests are routed through:
    ///
    /// SparkVFXEditorDebugger
    ///          |
    ///          v
    /// SparkVFXLayeredStateMachine
    ///          |
    ///          v
    /// SparkVFXSequencePlayer
    ///          |
    ///          v
    /// ISparkVFXController
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class SparkVFXEditorDebugger
        : MonoBehaviour
    {
        // ============================================================
        // STATE MACHINE
        // ============================================================

        [Header("State Machine")]

        [Tooltip(
            "Layered state machine used for debug state requests."
        )]
        [SerializeField]
        public SparkVFXLayeredStateMachine stateMachine;


        // ============================================================
        // DEBUG OPTIONS
        // ============================================================

        [Header("Debug Options")]

        [SerializeField]
        private bool enableDebugLogging = true;


        [SerializeField]
        private bool enableKeyboardShortcuts = true;


        // ============================================================
        // KEYBOARD
        // ============================================================

        [Header("Keyboard Shortcuts")]

        [SerializeField]
        private KeyCode normalKey = KeyCode.Alpha1;


        [SerializeField]
        private KeyCode hoverKey = KeyCode.Alpha2;


        [SerializeField]
        private KeyCode pressedKey = KeyCode.Alpha3;


        [SerializeField]
        private KeyCode selectedKey = KeyCode.Alpha4;


        [SerializeField]
        private KeyCode clickKey = KeyCode.Alpha5;


        [SerializeField]
        private KeyCode submitKey = KeyCode.Alpha6;


        [SerializeField]
        private KeyCode warningKey = KeyCode.Alpha7;


        [SerializeField]
        private KeyCode disableKey = KeyCode.Alpha8;


        // ============================================================
        // CUSTOM STATE
        // ============================================================

        [Header("Custom State")]

        [Tooltip(
            "State name used by the custom state test."
        )]
        [SerializeField]
        private string customState =
            "Hover";


        // ============================================================
        // RUNTIME
        // ============================================================

        private bool initialized;


        // ============================================================
        // AWAKE
        // ============================================================

        private void Awake()
        {
            ResolveStateMachine();
        }


        // ============================================================
        // UPDATE
        // ============================================================

        private void Update()
        {
            if (!enableKeyboardShortcuts)
            {
                return;
            }


            ProcessKeyboardShortcuts();
        }


        // ============================================================
        // RESOLVE
        // ============================================================

        private void ResolveStateMachine()
        {
            if (stateMachine != null)
            {
                initialized =
                    true;

                return;
            }


            // --------------------------------------------------------
            // SAME GAMEOBJECT
            // --------------------------------------------------------

            stateMachine =
                GetComponent<
                    SparkVFXLayeredStateMachine
                >();


            if (stateMachine != null)
            {
                initialized =
                    true;

                return;
            }


            // --------------------------------------------------------
            // PARENT
            // --------------------------------------------------------

            stateMachine =
                GetComponentInParent<
                    SparkVFXLayeredStateMachine
                >(
                    true
                );


            if (stateMachine != null)
            {
                initialized =
                    true;

                return;
            }


            // --------------------------------------------------------
            // CHILD
            // --------------------------------------------------------

            stateMachine =
                GetComponentInChildren<
                    SparkVFXLayeredStateMachine
                >(
                    true
                );


            initialized =
                stateMachine != null;
        }


        // ============================================================
        // REQUEST STATE
        // ============================================================

        public bool RequestState(
            string stateName)
        {
            if (string.IsNullOrWhiteSpace(
                stateName
            ))
            {
                Debug.LogWarning(
                    "[SparkVFXEditorDebugger] " +
                    "Cannot request an empty state name.",
                    this
                );

                return false;
            }


            if (stateMachine == null)
            {
                ResolveStateMachine();
            }


            if (stateMachine == null)
            {
                Debug.LogError(
                    "[SparkVFXEditorDebugger] " +
                    "No SparkVFXLayeredStateMachine found.",
                    this
                );

                return false;
            }


            if (enableDebugLogging)
            {
                Debug.Log(
                    "[SparkVFXEditorDebugger] " +
                    "Requesting state: " +
                    stateName,
                    this
                );
            }


            return stateMachine.RequestState(
                stateName
            );
        }


        // ============================================================
        // STANDARD STATES
        // ============================================================

        public bool TestNormal()
        {
            return RequestState(
                "Normal"
            );
        }


        public bool TestHover()
        {
            return RequestState(
                "Hover"
            );
        }


        public bool TestPressed()
        {
            return RequestState(
                "Pressed"
            );
        }


        public bool TestSelected()
        {
            return RequestState(
                "Selected"
            );
        }


        public bool TestClick()
        {
            return RequestState(
                "Click"
            );
        }


        public bool TestSubmit()
        {
            return RequestState(
                "Submit"
            );
        }


        public bool TestWarning()
        {
            return RequestState(
                "Warning"
            );
        }


        public bool TestDisable()
        {
            return RequestState(
                "Disable"
            );
        }


        // ============================================================
        // CUSTOM STATE
        // ============================================================

        public bool TestCustomState()
        {
            return RequestState(
                customState
            );
        }


        // ============================================================
        // KEYBOARD SHORTCUTS
        // ============================================================

        private void ProcessKeyboardShortcuts()
        {
            if (UnityEngine.Input.GetKeyDown(
                normalKey
            ))
            {
                TestNormal();

                return;
            }


            if (UnityEngine.Input.GetKeyDown(
                hoverKey
            ))
            {
                TestHover();

                return;
            }


            if (UnityEngine.Input.GetKeyDown(
                pressedKey
            ))
            {
                TestPressed();

                return;
            }


            if (UnityEngine.Input.GetKeyDown(
                selectedKey
            ))
            {
                TestSelected();

                return;
            }


            if (UnityEngine.Input.GetKeyDown(
                clickKey
            ))
            {
                TestClick();

                return;
            }


            if (UnityEngine.Input.GetKeyDown(
                submitKey
            ))
            {
                TestSubmit();

                return;
            }


            if (UnityEngine.Input.GetKeyDown(
                warningKey
            ))
            {
                TestWarning();

                return;
            }


            if (UnityEngine.Input.GetKeyDown(
                disableKey
            ))
            {
                TestDisable();
            }
        }


        // ============================================================
        // REFRESH
        // ============================================================

        [ContextMenu(
            "Spark VFX / Refresh State Machine"
        )]
        public void Refresh()
        {
            stateMachine =
                null;


            initialized =
                false;


            ResolveStateMachine();


            if (enableDebugLogging)
            {
                if (stateMachine != null)
                {
                    Debug.Log(
                        "[SparkVFXEditorDebugger] " +
                        "State machine resolved successfully.",
                        this
                    );
                }
                else
                {
                    Debug.LogWarning(
                        "[SparkVFXEditorDebugger] " +
                        "State machine could not be found.",
                        this
                    );
                }
            }
        }


        // ============================================================
        // CONTEXT MENU
        // ============================================================

        [ContextMenu(
            "Spark VFX / TEST / Normal"
        )]
        private void ContextTestNormal()
        {
            TestNormal();
        }


        [ContextMenu(
            "Spark VFX / TEST / Hover"
        )]
        private void ContextTestHover()
        {
            TestHover();
        }


        [ContextMenu(
            "Spark VFX / TEST / Pressed"
        )]
        private void ContextTestPressed()
        {
            TestPressed();
        }


        [ContextMenu(
            "Spark VFX / TEST / Selected"
        )]
        private void ContextTestSelected()
        {
            TestSelected();
        }


        [ContextMenu(
            "Spark VFX / TEST / Click"
        )]
        private void ContextTestClick()
        {
            TestClick();
        }


        [ContextMenu(
            "Spark VFX / TEST / Submit"
        )]
        private void ContextTestSubmit()
        {
            TestSubmit();
        }


        [ContextMenu(
            "Spark VFX / TEST / Warning"
        )]
        private void ContextTestWarning()
        {
            TestWarning();
        }


        [ContextMenu(
            "Spark VFX / TEST / Disable"
        )]
        private void ContextTestDisable()
        {
            TestDisable();
        }


        [ContextMenu(
            "Spark VFX / TEST / Custom State"
        )]
        private void ContextTestCustomState()
        {
            TestCustomState();
        }


        // ============================================================
        // PROPERTIES
        // ============================================================

        public SparkVFXLayeredStateMachine StateMachine
        {
            get
            {
                if (stateMachine == null)
                {
                    ResolveStateMachine();
                }


                return stateMachine;
            }
        }


        public bool IsReady
        {
            get
            {
                return stateMachine != null;
            }
        }


        // ============================================================
        // VALIDATION
        // ============================================================

        private void OnValidate()
        {
            if (stateMachine == null)
            {
                stateMachine =
                    GetComponent<
                        SparkVFXLayeredStateMachine
                    >();
            }
        }
    }
}