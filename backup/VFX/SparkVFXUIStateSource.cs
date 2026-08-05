using UnityEngine;

namespace ProjectSpark.UI.VFX
{
    /// <summary>
    /// Reusable external state source for the Spark VFX UI system.
    ///
    /// Designed for:
    /// - Target highlighting
    /// - Tutorial highlighting
    /// - Quest markers
    /// - Objective indicators
    /// - Warning states
    /// - Interaction states
    /// - Level selection
    /// - Gameplay-driven UI states
    ///
    /// Architecture:
    ///
    /// External System
    ///        ↓
    /// SparkVFXUIStateSource
    ///        ↓
    /// SparkVFXUIStateCoordinator
    ///        ↓
    /// SparkVFXEventRouter
    ///        ↓
    /// SparkVFXRuntime
    ///
    /// This component owns exactly one coordinator request.
    ///
    /// It does NOT:
    /// - Modify materials.
    /// - Resolve profiles.
    /// - Control SparkVFXRuntime.
    /// - Directly call SparkVFXEventRouter.
    /// - Manage other state sources.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class SparkVFXUIStateSource
        : MonoBehaviour
    {
        // ============================================================
        // COORDINATOR
        // ============================================================

        [Header("State Coordinator")]

        [Tooltip(
            "Central coordinator that resolves the final UI VFX state."
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
        // SOURCE ID
        // ============================================================

        [Header("Source Identity")]

        [Tooltip(
            "Unique ID used by the coordinator to identify " +
            "this state source."
        )]
        [SerializeField]
        private string sourceID = "External";


        // ============================================================
        // STATE
        // ============================================================

        [Header("State")]

        [SerializeField]
        private SparkVFXEventType eventType =
            SparkVFXEventType.Target;


        // ============================================================
        // PRIORITY
        // ============================================================

        [Header("Priority")]

        [Tooltip(
            "Higher priority states override lower priority states."
        )]
        [SerializeField]
        private int priority = 50;


        // ============================================================
        // PLAYBACK
        // ============================================================

        [Header("Playback")]

        [Tooltip(
            "If enabled, the requested state is played instantly."
        )]
        [SerializeField]
        private bool instantPlayback;


        // ============================================================
        // AUTO ACTIVATE
        // ============================================================

        [Header("Activation")]

        [Tooltip(
            "Automatically submits this state when the component " +
            "becomes enabled."
        )]
        [SerializeField]
        private bool activateOnEnable;


        // ============================================================
        // RUNTIME
        // ============================================================

        private bool isActive;


        // ============================================================
        // AWAKE
        // ============================================================

        private void Awake()
        {
            ResolveCoordinator();
        }


        // ============================================================
        // ENABLE
        // ============================================================

        private void OnEnable()
        {
            ResolveCoordinator();


            if (activateOnEnable)
            {
                Activate();
            }
        }


        // ============================================================
        // DISABLE
        // ============================================================

        private void OnDisable()
        {
            Deactivate();
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
        // SOURCE ID
        // ============================================================

        public string SourceID
        {
            get
            {
                return sourceID;
            }
        }


        // ============================================================
        // EVENT TYPE
        // ============================================================

        public SparkVFXEventType EventType
        {
            get
            {
                return eventType;
            }

            set
            {
                eventType =
                    value;


                if (isActive)
                {
                    Submit();
                }
            }
        }


        // ============================================================
        // PRIORITY
        // ============================================================

        public int Priority
        {
            get
            {
                return priority;
            }

            set
            {
                priority =
                    value;


                if (isActive)
                {
                    Submit();
                }
            }
        }


        // ============================================================
        // ACTIVE
        // ============================================================

        public bool IsActive
        {
            get
            {
                return isActive;
            }
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
        // ACTIVATE
        // ============================================================

        public void Activate()
        {
            isActive =
                true;


            Submit();
        }


        // ============================================================
        // DEACTIVATE
        // ============================================================

        public void Deactivate()
        {
            if (!isActive)
            {
                return;
            }


            isActive =
                false;


            Clear();
        }


        // ============================================================
        // SUBMIT
        // ============================================================

        public void Submit()
        {
            ResolveCoordinator();


            if (coordinator == null)
            {
                Debug.LogWarning(
                    "[SparkVFXUIStateSource] " +
                    "SparkVFXUIStateCoordinator is not assigned " +
                    "or could not be found.",
                    this
                );

                return;
            }


            if (
                string.IsNullOrWhiteSpace(
                    sourceID
                )
            )
            {
                Debug.LogWarning(
                    "[SparkVFXUIStateSource] " +
                    "Source ID is empty.",
                    this
                );

                return;
            }


            coordinator.RequestState(
                sourceID,
                eventType,
                priority,
                instantPlayback
            );
        }


        // ============================================================
        // CLEAR
        // ============================================================

        public void Clear()
        {
            ResolveCoordinator();


            if (coordinator == null)
            {
                return;
            }


            if (
                string.IsNullOrWhiteSpace(
                    sourceID
                )
            )
            {
                return;
            }


            coordinator.ClearState(
                sourceID
            );
        }


        // ============================================================
        // SET STATE
        // ============================================================

        public void SetState(
            SparkVFXEventType newEventType)
        {
            eventType =
                newEventType;


            if (isActive)
            {
                Submit();
            }
        }


        // ============================================================
        // SET PRIORITY
        // ============================================================

        public void SetPriority(
            int newPriority)
        {
            priority =
                newPriority;


            if (isActive)
            {
                Submit();
            }
        }


        // ============================================================
        // SET STATE + PRIORITY
        // ============================================================

        public void SetState(
            SparkVFXEventType newEventType,
            int newPriority)
        {
            eventType =
                newEventType;


            priority =
                newPriority;


            if (isActive)
            {
                Submit();
            }
        }


        // ============================================================
        // SET INSTANT
        // ============================================================

        public void SetInstantPlayback(
            bool instant)
        {
            instantPlayback =
                instant;


            if (isActive)
            {
                Submit();
            }
        }


        // ============================================================
        // TARGET
        // ============================================================

        public void SetTarget()
        {
            SetState(
                SparkVFXEventType.Target,
                50
            );
        }


        // ============================================================
        // WARNING
        // ============================================================

        public void SetWarning()
        {
            SetState(
                SparkVFXEventType.Warning,
                80
            );
        }


        // ============================================================
        // ERROR
        // ============================================================

        public void SetError()
        {
            SetState(
                SparkVFXEventType.Error,
                100
            );
        }


        // ============================================================
        // SELECTED
        // ============================================================

        public void SetSelected()
        {
            SetState(
                SparkVFXEventType.Selected,
                30
            );
        }


        // ============================================================
        // HOVER
        // ============================================================

        public void SetHover()
        {
            SetState(
                SparkVFXEventType.HoverEnter,
                10
            );
        }


        // ============================================================
        // NORMAL
        // ============================================================

        public void SetNormal()
        {
            SetState(
                SparkVFXEventType.Normal,
                0
            );
        }


        // ============================================================
        // DISABLED
        // ============================================================

        public void SetDisabled()
        {
            SetState(
                SparkVFXEventType.Disabled,
                1000
            );
        }


        // ============================================================
        // VALIDATION
        // ============================================================

        public bool Validate(
            bool logWarning = true)
        {
            ResolveCoordinator();


            bool valid =
                true;


            if (coordinator == null)
            {
                valid =
                    false;


                if (logWarning)
                {
                    Debug.LogWarning(
                        "[SparkVFXUIStateSource] " +
                        "SparkVFXUIStateCoordinator is not assigned.",
                        this
                    );
                }
            }


            if (
                string.IsNullOrWhiteSpace(
                    sourceID
                )
            )
            {
                valid =
                    false;


                if (logWarning)
                {
                    Debug.LogWarning(
                        "[SparkVFXUIStateSource] " +
                        "Source ID is empty.",
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
            "Validate State Source"
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
                    "[SparkVFXUIStateSource] " +
                    "Validation successful.",
                    this
                );
            }
            else
            {
                Debug.LogError(
                    "[SparkVFXUIStateSource] " +
                    "Validation failed.",
                    this
                );
            }
        }


        // ============================================================
        // EDITOR ACTIVATE
        // ============================================================

        [ContextMenu(
            "Activate State Source"
        )]
        private void ActivateFromContextMenu()
        {
            Activate();
        }


        // ============================================================
        // EDITOR DEACTIVATE
        // ============================================================

        [ContextMenu(
            "Deactivate State Source"
        )]
        private void DeactivateFromContextMenu()
        {
            Deactivate();
        }

#endif
    }
}