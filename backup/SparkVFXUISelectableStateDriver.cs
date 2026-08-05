using UnityEngine;
using UnityEngine.UI;

namespace ProjectSpark.UI.VFX
{
    /// <summary>
    /// Drives Unity UI Selectable states into
    /// SparkVFXUIStateCoordinator.
    ///
    /// Architecture:
    ///
    /// Unity Selectable
    ///        ↓
    /// SparkVFXUISelectableStateDriver
    ///        ↓
    /// SparkVFXUIStateCoordinator
    ///        ↓
    /// SparkVFXEventRouter
    ///        ↓
    /// SparkVFXRuntime
    ///
    /// Supported states:
    /// - Normal
    /// - Highlighted
    /// - Pressed
    /// - Selected
    /// - Disabled
    ///
    /// This component does NOT directly play VFX.
    /// It only submits state requests.
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Selectable))]
    public sealed class SparkVFXUISelectableStateDriver
        : MonoBehaviour
    {
        // ============================================================
        // COORDINATOR
        // ============================================================

        [Header("State Coordinator")]

        [Tooltip(
            "Central coordinator responsible for resolving " +
            "UI VFX state priority."
        )]
        [SerializeField]
        private SparkVFXUIStateCoordinator coordinator;


        // ============================================================
        // AUTO FIND
        // ============================================================

        [Header("Auto Find")]

        [Tooltip(
            "Automatically searches this object, children, " +
            "and parents for SparkVFXUIStateCoordinator."
        )]
        [SerializeField]
        private bool autoFindCoordinator = true;


        // ============================================================
        // SELECTABLE
        // ============================================================

        [Header("Selectable")]

        [SerializeField]
        private Selectable selectable;


        // ============================================================
        // OWNER
        // ============================================================

        [Header("Request Owner")]

        [Tooltip(
            "Unique owner ID used by the state coordinator."
        )]
        [SerializeField]
        private string ownerID = "Selectable";


        // ============================================================
        // PRIORITIES
        // ============================================================

        [Header("State Priorities")]

        [SerializeField]
        private int normalPriority = 0;

        [SerializeField]
        private int highlightedPriority = 10;

        [SerializeField]
        private int pressedPriority = 20;

        [SerializeField]
        private int selectedPriority = 30;

        [SerializeField]
        private int disabledPriority = 1000;


        // ============================================================
        // ENABLE STATES
        // ============================================================

        [Header("Enabled States")]

        [SerializeField]
        private bool sendNormal = true;

        [SerializeField]
        private bool sendHighlighted = true;

        [SerializeField]
        private bool sendPressed = true;

        [SerializeField]
        private bool sendSelected = true;

        [SerializeField]
        private bool sendDisabled = true;


        // ============================================================
        // PLAYBACK
        // ============================================================

        [Header("Playback")]

        [Tooltip(
            "If enabled, requests from this driver are played instantly."
        )]
        [SerializeField]
        private bool instantPlayback;


        // ============================================================
        // MONITORING
        // ============================================================

        [Header("Monitoring")]

        [Tooltip(
            "Checks the Selectable state every frame. " +
            "A request is sent only when the state changes."
        )]
        [SerializeField]
        private bool monitorState = true;


        // ============================================================
        // INITIALIZATION
        // ============================================================

        [Header("Initialization")]

        [SerializeField]
        private bool applyStateOnEnable = true;


        // ============================================================
        // RUNTIME STATE
        // ============================================================

        private Selectable.SelectionState lastState =
            Selectable.SelectionState.Normal;


        private bool hasState;

        private bool initialized;


        // ============================================================
        // AWAKE
        // ============================================================

        private void Awake()
        {
            ResolveSelectable();

            ResolveCoordinator();
        }


        // ============================================================
        // ENABLE
        // ============================================================

        private void OnEnable()
        {
            ResolveSelectable();

            ResolveCoordinator();


            hasState =
                false;


            initialized =
                false;


            if (applyStateOnEnable)
            {
                RefreshState(
                    true
                );
            }
        }


        // ============================================================
        // DISABLE
        // ============================================================

        private void OnDisable()
        {
            ClearOwnedRequest();
        }


        // ============================================================
        // UPDATE
        // ============================================================

        private void Update()
        {
            if (!monitorState)
            {
                return;
            }


            RefreshState(
                false
            );
        }


        // ============================================================
        // SELECTABLE
        // ============================================================

        public Selectable Selectable
        {
            get
            {
                ResolveSelectable();

                return selectable;
            }
        }


        // ============================================================
        // COORDINATOR
        // ============================================================

        public SparkVFXUIStateCoordinator Coordinator
        {
            get
            {
                ResolveCoordinator();

                return coordinator;
            }
        }


        // ============================================================
        // OWNER
        // ============================================================

        public string OwnerID
        {
            get
            {
                return ownerID;
            }
        }


        // ============================================================
        // RESOLVE SELECTABLE
        // ============================================================

        private void ResolveSelectable()
        {
            if (selectable != null)
            {
                return;
            }


            selectable =
                GetComponent<
                    Selectable
                >();
        }


        // ============================================================
        // RESOLVE COORDINATOR
        // ============================================================

        private void ResolveCoordinator()
        {
            if (coordinator != null)
            {
                return;
            }


            if (!autoFindCoordinator)
            {
                return;
            }


            coordinator =
                GetComponent<
                    SparkVFXUIStateCoordinator
                >();


            if (coordinator != null)
            {
                return;
            }


            coordinator =
                GetComponentInChildren<
                    SparkVFXUIStateCoordinator
                >(
                    true
                );


            if (coordinator != null)
            {
                return;
            }


            coordinator =
                GetComponentInParent<
                    SparkVFXUIStateCoordinator
                >();
        }


        // ============================================================
        // GET CURRENT STATE
        // ============================================================

        private Selectable.SelectionState
            GetCurrentState()
        {
            ResolveSelectable();


            if (selectable == null)
            {
                return
                    Selectable.SelectionState.Normal;
            }


            if (!selectable.IsInteractable())
            {
                return
                    Selectable.SelectionState.Disabled;
            }


            return
                selectable.currentSelectionState;
        }


        // ============================================================
        // REFRESH STATE
        // ============================================================

        public void RefreshState(
            bool force)
        {
            Selectable.SelectionState
                currentState =
                    GetCurrentState();


            if (
                !force &&
                hasState &&
                currentState ==
                lastState
            )
            {
                return;
            }


            lastState =
                currentState;


            hasState =
                true;


            initialized =
                true;


            ApplyState(
                currentState
            );
        }


        // ============================================================
        // APPLY STATE
        // ============================================================

        private void ApplyState(
            Selectable.SelectionState state)
        {
            switch (state)
            {
                case Selectable.SelectionState.Normal:

                    if (sendNormal)
                    {
                        RequestState(
                            SparkVFXEventType.Normal,
                            normalPriority
                        );
                    }

                    break;


                case Selectable.SelectionState.Highlighted:

                    if (sendHighlighted)
                    {
                        RequestState(
                            SparkVFXEventType.HoverEnter,
                            highlightedPriority
                        );
                    }

                    break;


                case Selectable.SelectionState.Pressed:

                    if (sendPressed)
                    {
                        RequestState(
                            SparkVFXEventType.Press,
                            pressedPriority
                        );
                    }

                    break;


                case Selectable.SelectionState.Selected:

                    if (sendSelected)
                    {
                        RequestState(
                            SparkVFXEventType.Selected,
                            selectedPriority
                        );
                    }

                    break;


                case Selectable.SelectionState.Disabled:

                    if (sendDisabled)
                    {
                        RequestState(
                            SparkVFXEventType.Disabled,
                            disabledPriority
                        );
                    }

                    break;


                default:

                    if (sendNormal)
                    {
                        RequestState(
                            SparkVFXEventType.Normal,
                            normalPriority
                        );
                    }

                    break;
            }
        }


        // ============================================================
        // REQUEST STATE
        // ============================================================

        private void RequestState(
            SparkVFXEventType eventType,
            int priority)
        {
            ResolveCoordinator();


            if (coordinator == null)
            {
                Debug.LogWarning(
                    "[SparkVFXUISelectableStateDriver] " +
                    "SparkVFXUIStateCoordinator is not assigned " +
                    "or could not be found.",
                    this
                );

                return;
            }


            coordinator.RequestState(
                ownerID,
                eventType,
                priority,
                instantPlayback
            );
        }


        // ============================================================
        // CLEAR OWNED REQUEST
        // ============================================================

        public void ClearOwnedRequest()
        {
            ResolveCoordinator();


            if (coordinator == null)
            {
                return;
            }


            coordinator.ClearState(
                ownerID
            );
        }


        // ============================================================
        // MANUAL REFRESH
        // ============================================================

        public void Refresh()
        {
            RefreshState(
                true
            );
        }


        // ============================================================
        // FORCE NORMAL
        // ============================================================

        public void ForceNormal()
        {
            RequestState(
                SparkVFXEventType.Normal,
                normalPriority
            );
        }


        // ============================================================
        // FORCE HOVER
        // ============================================================

        public void ForceHover()
        {
            RequestState(
                SparkVFXEventType.HoverEnter,
                highlightedPriority
            );
        }


        // ============================================================
        // FORCE PRESSED
        // ============================================================

        public void ForcePressed()
        {
            RequestState(
                SparkVFXEventType.Press,
                pressedPriority
            );
        }


        // ============================================================
        // FORCE SELECTED
        // ============================================================

        public void ForceSelected()
        {
            RequestState(
                SparkVFXEventType.Selected,
                selectedPriority
            );
        }


        // ============================================================
        // FORCE TARGET
        // ============================================================

        public void ForceTarget()
        {
            RequestState(
                SparkVFXEventType.Target,
                50
            );
        }


        // ============================================================
        // FORCE WARNING
        // ============================================================

        public void ForceWarning()
        {
            RequestState(
                SparkVFXEventType.Warning,
                80
            );
        }


        // ============================================================
        // FORCE DISABLED
        // ============================================================

        public void ForceDisabled()
        {
            RequestState(
                SparkVFXEventType.Disabled,
                disabledPriority
            );
        }


        // ============================================================
        // CURRENT STATE
        // ============================================================

        public Selectable.SelectionState
            CurrentState
        {
            get
            {
                return GetCurrentState();
            }
        }


        // ============================================================
        // LAST STATE
        // ============================================================

        public Selectable.SelectionState
            LastState
        {
            get
            {
                return lastState;
            }
        }


        // ============================================================
        // INITIALIZED
        // ============================================================

        public bool IsInitialized
        {
            get
            {
                return initialized;
            }
        }


        // ============================================================
        // VALIDATION
        // ============================================================

        public bool Validate(
            bool logWarning = true)
        {
            ResolveSelectable();

            ResolveCoordinator();


            bool valid =
                true;


            if (selectable == null)
            {
                valid =
                    false;


                if (logWarning)
                {
                    Debug.LogWarning(
                        "[SparkVFXUISelectableStateDriver] " +
                        "No Unity Selectable component found.",
                        this
                    );
                }
            }


            if (coordinator == null)
            {
                valid =
                    false;


                if (logWarning)
                {
                    Debug.LogWarning(
                        "[SparkVFXUISelectableStateDriver] " +
                        "No SparkVFXUIStateCoordinator found.",
                        this
                    );
                }
            }


            if (
                string.IsNullOrWhiteSpace(
                    ownerID
                )
            )
            {
                valid =
                    false;


                if (logWarning)
                {
                    Debug.LogWarning(
                        "[SparkVFXUISelectableStateDriver] " +
                        "Owner ID is empty.",
                        this
                    );
                }
            }


            return valid;
        }


#if UNITY_EDITOR

        // ============================================================
        // EDITOR VALIDATION
        // ============================================================

        [ContextMenu(
            "Validate Selectable State Driver"
        )]
        private void ValidateFromContextMenu()
        {
            bool valid =
                Validate(
                    true
                );


            if (valid)
            {
                Debug.Log(
                    "[SparkVFXUISelectableStateDriver] " +
                    "Validation successful.",
                    this
                );
            }
            else
            {
                Debug.LogError(
                    "[SparkVFXUISelectableStateDriver] " +
                    "Validation failed.",
                    this
                );
            }
        }


        // ============================================================
        // EDITOR REFRESH
        // ============================================================

        [ContextMenu(
            "Refresh Current Selectable State"
        )]
        private void RefreshFromContextMenu()
        {
            Refresh();
        }

#endif
    }
}