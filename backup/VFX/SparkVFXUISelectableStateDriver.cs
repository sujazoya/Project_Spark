using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace ProjectSpark.UI.VFX
{
    /// <summary>
    /// Drives Spark VFX from a Unity UI Selectable.
    ///
    /// Supported states:
    /// - Normal
    /// - Hovered
    /// - Pressed
    /// - Selected
    /// - Disabled
    ///
    /// State detection uses public Unity UI/EventSystem APIs.
    ///
    /// This component does NOT:
    /// - Resolve profiles.
    /// - Modify materials.
    /// - Control VFX controllers directly.
    /// - Manage layered VFX priorities.
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Selectable))]
    public sealed class SparkVFXUISelectableStateDriver
        : MonoBehaviour,
          IPointerEnterHandler,
          IPointerExitHandler,
          IPointerDownHandler,
          IPointerUpHandler,
          ISelectHandler,
          IDeselectHandler,
          ISubmitHandler
    {
        // ============================================================
        // STATE ENUM
        // ============================================================

        public enum SparkVFXUISelectableState
        {
            Normal,
            Hovered,
            Pressed,
            Selected,
            Disabled
        }


        // ============================================================
        // ROUTER
        // ============================================================

        [Header("VFX Router")]

        [Tooltip(
            "SparkVFXEventRouter used to play UI VFX states."
        )]
        [SerializeField]
        private SparkVFXEventRouter eventRouter;


        // ============================================================
        // AUTO FIND ROUTER
        // ============================================================

        [Header("Auto Find")]

        [Tooltip(
            "Automatically searches this object, children, " +
            "and parents for SparkVFXEventRouter."
        )]
        [SerializeField]
        private bool autoFindRouter = true;


        // ============================================================
        // SELECTABLE
        // ============================================================

        [Header("Selectable")]

        [SerializeField]
        private Selectable selectable;


        // ============================================================
        // PLAYBACK
        // ============================================================

        [Header("Playback")]

        [Tooltip(
            "If enabled, state changes use instant VFX playback."
        )]
        [SerializeField]
        private bool instantPlayback;


        // ============================================================
        // INITIALIZATION
        // ============================================================

        [Header("Initialization")]

        [Tooltip(
            "Automatically applies the current logical state " +
            "when this component becomes enabled."
        )]
        [SerializeField]
        private bool applyStateOnEnable = true;


        // ============================================================
        // MONITORING
        // ============================================================

        [Header("State Monitoring")]

        [Tooltip(
            "Continuously checks interactability and state changes."
        )]
        [SerializeField]
        private bool monitorState = true;


        // ============================================================
        // STATE MAPPING
        // ============================================================

        [Header("State Mapping")]

        [Tooltip(
            "Send Normal when the Selectable is in its normal state."
        )]
        [SerializeField]
        private bool sendNormal = true;


        [Tooltip(
            "Send HoverEnter when the Selectable is hovered."
        )]
        [SerializeField]
        private bool sendHighlighted = true;


        [Tooltip(
            "Send Press when the Selectable is pressed."
        )]
        [SerializeField]
        private bool sendPressed = true;


        [Tooltip(
            "Send Selected when the Selectable is selected."
        )]
        [SerializeField]
        private bool sendSelected = true;


        [Tooltip(
            "Send Disabled when the Selectable is not interactable."
        )]
        [SerializeField]
        private bool sendDisabled = true;


        // ============================================================
        // RUNTIME INPUT STATE
        // ============================================================

        private bool isPointerInside;

        private bool isPointerDown;

        private bool isSelected;


        // ============================================================
        // RUNTIME STATE
        // ============================================================

        private SparkVFXUISelectableState lastState =
            SparkVFXUISelectableState.Normal;


        private SparkVFXUISelectableState currentState =
            SparkVFXUISelectableState.Normal;


        private bool hasState;

        private bool initialized;


        // ============================================================
        // AWAKE
        // ============================================================

        private void Awake()
        {
            ResolveSelectable();

            ResolveRouter();
        }


        // ============================================================
        // ENABLE
        // ============================================================

        private void OnEnable()
        {
            ResolveSelectable();

            ResolveRouter();


            isPointerInside =
                false;


            isPointerDown =
                false;


            isSelected =
                false;


            initialized =
                false;


            hasState =
                false;


            currentState =
                SparkVFXUISelectableState.Normal;


            lastState =
                SparkVFXUISelectableState.Normal;


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
            isPointerInside =
                false;


            isPointerDown =
                false;


            isSelected =
                false;


            initialized =
                false;
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
        // POINTER ENTER
        // ============================================================

        public void OnPointerEnter(
            PointerEventData eventData)
        {
            isPointerInside =
                true;


            RefreshState(
                false
            );
        }


        // ============================================================
        // POINTER EXIT
        // ============================================================

        public void OnPointerExit(
            PointerEventData eventData)
        {
            isPointerInside =
                false;


            isPointerDown =
                false;


            RefreshState(
                false
            );
        }


        // ============================================================
        // POINTER DOWN
        // ============================================================

        public void OnPointerDown(
            PointerEventData eventData)
        {
            if (
                selectable != null &&
                !selectable.IsInteractable()
            )
            {
                return;
            }


            isPointerDown =
                true;


            RefreshState(
                false
            );
        }


        // ============================================================
        // POINTER UP
        // ============================================================

        public void OnPointerUp(
            PointerEventData eventData)
        {
            isPointerDown =
                false;


            RefreshState(
                false
            );
        }


        // ============================================================
        // SELECT
        // ============================================================

        public void OnSelect(
            BaseEventData eventData)
        {
            isSelected =
                true;


            RefreshState(
                false
            );
        }


        // ============================================================
        // DESELECT
        // ============================================================

        public void OnDeselect(
            BaseEventData eventData)
        {
            isSelected =
                false;


            RefreshState(
                false
            );
        }


        // ============================================================
        // SUBMIT
        // ============================================================

        public void OnSubmit(
            BaseEventData eventData)
        {
            if (
                selectable != null &&
                !selectable.IsInteractable()
            )
            {
                return;
            }


            Route(
                SparkVFXEventType.Press
            );
        }


        // ============================================================
        // SELECTABLE PROPERTY
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
        // GET CURRENT STATE
        // ============================================================

        private SparkVFXUISelectableState
            GetCurrentState()
        {
            ResolveSelectable();


            if (selectable == null)
            {
                return
                    SparkVFXUISelectableState.Normal;
            }


            if (!selectable.IsInteractable())
            {
                return
                    SparkVFXUISelectableState.Disabled;
            }


            if (isPointerDown)
            {
                return
                    SparkVFXUISelectableState.Pressed;
            }


            if (isSelected)
            {
                return
                    SparkVFXUISelectableState.Selected;
            }


            if (isPointerInside)
            {
                return
                    SparkVFXUISelectableState.Hovered;
            }


            return
                SparkVFXUISelectableState.Normal;
        }


        // ============================================================
        // REFRESH STATE
        // ============================================================

        public void RefreshState(
            bool force)
        {
            SparkVFXUISelectableState
                resolvedState =
                    GetCurrentState();


            currentState =
                resolvedState;


            if (
                !force &&
                hasState &&
                resolvedState ==
                lastState
            )
            {
                initialized =
                    true;

                return;
            }


            lastState =
                resolvedState;


            hasState =
                true;


            initialized =
                true;


            ApplyState(
                resolvedState
            );
        }


        // ============================================================
        // APPLY STATE
        // ============================================================

        private void ApplyState(
            SparkVFXUISelectableState state)
        {
            switch (state)
            {
                case SparkVFXUISelectableState.Normal:

                    if (sendNormal)
                    {
                        Route(
                            SparkVFXEventType.Normal
                        );
                    }

                    break;


                case SparkVFXUISelectableState.Hovered:

                    if (sendHighlighted)
                    {
                        Route(
                            SparkVFXEventType.HoverEnter
                        );
                    }

                    break;


                case SparkVFXUISelectableState.Pressed:

                    if (sendPressed)
                    {
                        Route(
                            SparkVFXEventType.Press
                        );
                    }

                    break;


                case SparkVFXUISelectableState.Selected:

                    if (sendSelected)
                    {
                        Route(
                            SparkVFXEventType.Selected
                        );
                    }

                    break;


                case SparkVFXUISelectableState.Disabled:

                    if (sendDisabled)
                    {
                        Route(
                            SparkVFXEventType.Disabled
                        );
                    }

                    break;


                default:

                    if (sendNormal)
                    {
                        Route(
                            SparkVFXEventType.Normal
                        );
                    }

                    break;
            }
        }


        // ============================================================
        // ROUTE
        // ============================================================

        private void Route(
            SparkVFXEventType eventType)
        {
            ResolveRouter();


            if (eventRouter == null)
            {
                Debug.LogWarning(
                    "[SparkVFXUISelectableStateDriver] " +
                    "SparkVFXEventRouter is not assigned " +
                    "or could not be found.",
                    this
                );

                return;
            }


            if (instantPlayback)
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
            currentState =
                SparkVFXUISelectableState.Normal;


            lastState =
                SparkVFXUISelectableState.Normal;


            hasState =
                true;


            initialized =
                true;


            Route(
                SparkVFXEventType.Normal
            );
        }


        // ============================================================
        // FORCE SELECTED
        // ============================================================

        public void ForceSelected()
        {
            currentState =
                SparkVFXUISelectableState.Selected;


            lastState =
                SparkVFXUISelectableState.Selected;


            hasState =
                true;


            initialized =
                true;


            Route(
                SparkVFXEventType.Selected
            );
        }


        // ============================================================
        // FORCE TARGET
        // ============================================================

        public void ForceTarget()
        {
            currentState =
                SparkVFXUISelectableState.Selected;


            Route(
                SparkVFXEventType.Target
            );
        }


        // ============================================================
        // FORCE DISABLED
        // ============================================================

        public void ForceDisabled()
        {
            currentState =
                SparkVFXUISelectableState.Disabled;


            lastState =
                SparkVFXUISelectableState.Disabled;


            hasState =
                true;


            initialized =
                true;


            Route(
                SparkVFXEventType.Disabled
            );
        }


        // ============================================================
        // RESET
        // ============================================================

        public void ResetStateTracking()
        {
            hasState =
                false;


            initialized =
                false;


            currentState =
                SparkVFXUISelectableState.Normal;


            lastState =
                SparkVFXUISelectableState.Normal;
        }


        // ============================================================
        // CURRENT STATE
        // ============================================================

        public SparkVFXUISelectableState
            CurrentState
        {
            get
            {
                return
                    currentState;
            }
        }


        // ============================================================
        // LAST STATE
        // ============================================================

        public SparkVFXUISelectableState
            LastState
        {
            get
            {
                return
                    lastState;
            }
        }


        // ============================================================
        // VALIDATION
        // ============================================================

        public bool Validate(
            bool logWarning = true)
        {
            ResolveSelectable();

            ResolveRouter();


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


            if (eventRouter == null)
            {
                valid =
                    false;


                if (logWarning)
                {
                    Debug.LogWarning(
                        "[SparkVFXUISelectableStateDriver] " +
                        "No SparkVFXEventRouter found.",
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