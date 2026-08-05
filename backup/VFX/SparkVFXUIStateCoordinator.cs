using System;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectSpark.UI.VFX
{
    /// <summary>
    /// Central UI VFX state coordinator.
    ///
    /// Responsibilities:
    /// - Receives logical UI state requests.
    /// - Maintains active state requests.
    /// - Resolves the highest-priority active state.
    /// - Sends the resolved event to SparkVFXEventRouter.
    /// - Prevents lower-priority states from overriding higher-priority states.
    ///
    /// Example priority:
    ///
    /// Normal    = 0
    /// Hover     = 10
    /// Selected  = 20
    /// Target    = 50
    /// Warning   = 80
    /// Error     = 100
    /// Disabled  = 1000
    ///
    /// This component does NOT:
    /// - Resolve profiles.
    /// - Modify materials.
    /// - Control ISparkVFXController directly.
    /// - Manage SparkVFXLayeredStateMachine.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class SparkVFXUIStateCoordinator
        : MonoBehaviour
    {
        // ============================================================
        // ROUTER
        // ============================================================

        [Header("VFX Router")]

        [SerializeField]
        private SparkVFXEventRouter eventRouter;


        // ============================================================
        // AUTO FIND
        // ============================================================

        [Header("Auto Find")]

        [SerializeField]
        private bool autoFindRouter = true;


        // ============================================================
        // PLAYBACK
        // ============================================================

        [Header("Playback")]

        [SerializeField]
        private bool instantPlayback;


        // ============================================================
        // DEFAULT STATE
        // ============================================================

        [Header("Default State")]

        [SerializeField]
        private SparkVFXEventType defaultState =
            SparkVFXEventType.Normal;


        // ============================================================
        // STATE REQUESTS
        // ============================================================

        private readonly Dictionary<
            string,
            StateRequest
        > requests =
            new Dictionary<
                string,
                StateRequest
            >(
                StringComparer.Ordinal
            );


        // ============================================================
        // CURRENT STATE
        // ============================================================

        private SparkVFXEventType currentState =
            SparkVFXEventType.Normal;


        private int currentPriority =
            int.MinValue;


        private string currentOwner;


        private bool initialized;


        // ============================================================
        // STATE REQUEST
        // ============================================================

        [Serializable]
        private sealed class StateRequest
        {
            public string owner;

            public SparkVFXEventType eventType;

            public int priority;

            public bool instant;

            public long order;
        }


        // ============================================================
        // ORDER
        // ============================================================

        private long requestOrder;


        // ============================================================
        // AWAKE
        // ============================================================

        private void Awake()
        {
            ResolveRouter();
        }


        // ============================================================
        // ENABLE
        // ============================================================

        private void OnEnable()
        {
            ResolveRouter();

            initialized = false;

            Reevaluate();
        }


        // ============================================================
        // ROUTER PROPERTY
        // ============================================================

        public SparkVFXEventRouter EventRouter
        {
            get
            {
                ResolveRouter();

                return eventRouter;
            }
        }


        // ============================================================
        // CURRENT STATE
        // ============================================================

        public SparkVFXEventType CurrentState
        {
            get
            {
                return currentState;
            }
        }


        // ============================================================
        // CURRENT PRIORITY
        // ============================================================

        public int CurrentPriority
        {
            get
            {
                return currentPriority;
            }
        }


        // ============================================================
        // CURRENT OWNER
        // ============================================================

        public string CurrentOwner
        {
            get
            {
                return currentOwner;
            }
        }


        // ============================================================
        // RESOLVE ROUTER
        // ============================================================

        private void ResolveRouter()
        {
            if (eventRouter != null)
            {
                return;
            }


            if (!autoFindRouter)
            {
                return;
            }


            eventRouter =
                GetComponent<
                    SparkVFXEventRouter
                >();


            if (eventRouter != null)
            {
                return;
            }


            eventRouter =
                GetComponentInChildren<
                    SparkVFXEventRouter
                >(
                    true
                );


            if (eventRouter != null)
            {
                return;
            }


            eventRouter =
                GetComponentInParent<
                    SparkVFXEventRouter
                >();
        }


        // ============================================================
        // REQUEST STATE
        // ============================================================

        public void RequestState(
            string owner,
            SparkVFXEventType eventType,
            int priority)
        {
            RequestState(
                owner,
                eventType,
                priority,
                instantPlayback
            );
        }


        // ============================================================
        // REQUEST STATE WITH PLAYBACK
        // ============================================================

        public void RequestState(
            string owner,
            SparkVFXEventType eventType,
            int priority,
            bool instant)
        {
            if (string.IsNullOrWhiteSpace(owner))
            {
                owner =
                    "Anonymous";
            }


            owner =
                owner.Trim();


            StateRequest request;


            if (
                requests.TryGetValue(
                    owner,
                    out request
                )
            )
            {
                request.eventType =
                    eventType;

                request.priority =
                    priority;

                request.instant =
                    instant;

                request.order =
                    ++requestOrder;
            }
            else
            {
                request =
                    new StateRequest
                    {
                        owner =
                            owner,

                        eventType =
                            eventType,

                        priority =
                            priority,

                        instant =
                            instant,

                        order =
                            ++requestOrder
                    };


                requests.Add(
                    owner,
                    request
                );
            }


            Reevaluate();
        }


        // ============================================================
        // CLEAR STATE
        // ============================================================

        public void ClearState(
            string owner)
        {
            if (
                string.IsNullOrWhiteSpace(
                    owner
                )
            )
            {
                return;
            }


            if (
                requests.Remove(
                    owner.Trim()
                )
            )
            {
                Reevaluate();
            }
        }


        // ============================================================
        // CLEAR ALL
        // ============================================================

        public void ClearAll()
        {
            requests.Clear();

            Reevaluate();
        }


        // ============================================================
        // NORMAL
        // ============================================================

        public void SetNormal(
            string owner = "Selectable")
        {
            RequestState(
                owner,
                SparkVFXEventType.Normal,
                0
            );
        }


        // ============================================================
        // HOVER
        // ============================================================

        public void SetHover(
            string owner = "Selectable")
        {
            RequestState(
                owner,
                SparkVFXEventType.HoverEnter,
                10
            );
        }


        // ============================================================
        // PRESSED
        // ============================================================

        public void SetPressed(
            string owner = "Selectable")
        {
            RequestState(
                owner,
                SparkVFXEventType.Press,
                20
            );
        }


        // ============================================================
        // SELECTED
        // ============================================================

        public void SetSelected(
            string owner = "Selectable")
        {
            RequestState(
                owner,
                SparkVFXEventType.Selected,
                30
            );
        }


        // ============================================================
        // TARGET
        // ============================================================

        public void SetTarget(
            string owner = "Target")
        {
            RequestState(
                owner,
                SparkVFXEventType.Target,
                50
            );
        }


        // ============================================================
        // WARNING
        // ============================================================

        public void SetWarning(
            string owner = "Warning")
        {
            RequestState(
                owner,
                SparkVFXEventType.Warning,
                80
            );
        }


        // ============================================================
        // ERROR
        // ============================================================

        public void SetError(
            string owner = "Error")
        {
            RequestState(
                owner,
                SparkVFXEventType.Error,
                100
            );
        }


        // ============================================================
        // DISABLED
        // ============================================================

        public void SetDisabled(
            string owner = "Disabled")
        {
            RequestState(
                owner,
                SparkVFXEventType.Disabled,
                1000
            );
        }


        // ============================================================
        // REEVALUATE
        // ============================================================

        public void Reevaluate()
        {
            StateRequest best =
                ResolveBestRequest();


            if (best == null)
            {
                ApplyResolvedState(
                    "Default",
                    defaultState,
                    int.MinValue,
                    instantPlayback
                );

                return;
            }


            ApplyResolvedState(
                best.owner,
                best.eventType,
                best.priority,
                best.instant
            );
        }


        // ============================================================
        // RESOLVE BEST
        // ============================================================

        private StateRequest
            ResolveBestRequest()
        {
            StateRequest best =
                null;


            foreach (
                KeyValuePair<
                    string,
                    StateRequest
                > pair
                in requests
            )
            {
                StateRequest request =
                    pair.Value;


                if (request == null)
                {
                    continue;
                }


                if (best == null)
                {
                    best =
                        request;

                    continue;
                }


                if (
                    request.priority >
                    best.priority
                )
                {
                    best =
                        request;

                    continue;
                }


                if (
                    request.priority ==
                    best.priority &&
                    request.order >
                    best.order
                )
                {
                    best =
                        request;
                }
            }


            return best;
        }


        // ============================================================
        // APPLY RESOLVED STATE
        // ============================================================

        private void ApplyResolvedState(
            string owner,
            SparkVFXEventType eventType,
            int priority,
            bool instant)
        {
            if (
                initialized &&
                currentState ==
                eventType &&
                currentPriority ==
                priority &&
                string.Equals(
                    currentOwner,
                    owner,
                    StringComparison.Ordinal
                )
            )
            {
                return;
            }


            currentState =
                eventType;


            currentPriority =
                priority;


            currentOwner =
                owner;


            initialized =
                true;


            Route(
                eventType,
                instant
            );
        }


        // ============================================================
        // ROUTE
        // ============================================================

        private void Route(
            SparkVFXEventType eventType,
            bool instant)
        {
            ResolveRouter();


            if (eventRouter == null)
            {
                Debug.LogWarning(
                    "[SparkVFXUIStateCoordinator] " +
                    "SparkVFXEventRouter is not assigned.",
                    this
                );

                return;
            }


            if (instant)
            {
                eventRouter.PlayInstant(
                    eventType
                );

                return;
            }


            eventRouter.Play(
                eventType
            );
        }


        // ============================================================
        // HAS REQUEST
        // ============================================================

        public bool HasRequest(
            string owner)
        {
            if (
                string.IsNullOrWhiteSpace(
                    owner
                )
            )
            {
                return false;
            }


            return requests.ContainsKey(
                owner.Trim()
            );
        }


        // ============================================================
        // REQUEST COUNT
        // ============================================================

        public int RequestCount
        {
            get
            {
                return requests.Count;
            }
        }


        // ============================================================
        // RESET
        // ============================================================

        public void ResetToDefault()
        {
            ClearAll();
        }


        // ============================================================
        // VALIDATION
        // ============================================================

        public bool Validate(
            bool logWarning = true)
        {
            ResolveRouter();


            if (eventRouter != null)
            {
                return true;
            }


            if (logWarning)
            {
                Debug.LogWarning(
                    "[SparkVFXUIStateCoordinator] " +
                    "SparkVFXEventRouter is not assigned " +
                    "or could not be found.",
                    this
                );
            }


            return false;
        }


#if UNITY_EDITOR

        [ContextMenu(
            "Validate UI State Coordinator"
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
                    "[SparkVFXUIStateCoordinator] " +
                    "Validation successful.",
                    this
                );
            }
            else
            {
                Debug.LogError(
                    "[SparkVFXUIStateCoordinator] " +
                    "Validation failed.",
                    this
                );
            }
        }


        [ContextMenu(
            "Reevaluate State"
        )]
        private void ReevaluateFromContextMenu()
        {
            Reevaluate();
        }

#endif
    }
}