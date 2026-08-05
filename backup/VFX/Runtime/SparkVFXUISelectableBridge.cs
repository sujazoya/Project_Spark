using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace ProjectSpark.UI.VFX
{
    /// <summary>
    /// Bridges Unity UI Selectable state changes to Spark VFX events.
    ///
    /// Designed for:
    /// - Button
    /// - Toggle
    /// - Slider
    /// - Dropdown
    /// - Any Unity UI Selectable
    ///
    /// Supported visual states:
    /// - Normal
    /// - HoverEnter
    /// - HoverExit
    /// - Press
    /// - Release
    /// - Selected
    /// - Disabled
    ///
    /// Responsibilities:
    /// - Observes Unity UI pointer/selection events.
    /// - Converts them into SparkVFXEventType.
    /// - Sends events through SparkVFXEventRouter.
    ///
    /// Does NOT:
    /// - Control materials directly.
    /// - Resolve SparkVFXProfile assets.
    /// - Modify SparkVFXRuntime.
    /// - Manage layered state priority.
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Selectable))]
    public sealed class SparkVFXUISelectableBridge
        : MonoBehaviour,
          IPointerEnterHandler,
          IPointerExitHandler,
          IPointerDownHandler,
          IPointerUpHandler,
          ISelectHandler,
          IDeselectHandler
    {
        // ============================================================
        // ROUTER
        // ============================================================

        [Header("VFX Router")]

        [Tooltip(
            "SparkVFXEventRouter used to send UI VFX events."
        )]
        [SerializeField]
        private SparkVFXEventRouter eventRouter;


        // ============================================================
        // AUTO FIND
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
            "If enabled, VFX events are applied instantly."
        )]
        [SerializeField]
        private bool instantPlayback;


        // ============================================================
        // STATE EVENTS
        // ============================================================

        [Header("State Events")]

        [SerializeField]
        private bool sendNormal = true;

        [SerializeField]
        private bool sendHoverEnter = true;

        [SerializeField]
        private bool sendHoverExit = true;

        [SerializeField]
        private bool sendPress = true;

        [SerializeField]
        private bool sendRelease = true;

        [SerializeField]
        private bool sendSelected = true;

        [SerializeField]
        private bool sendDisabled = true;


        // ============================================================
        // INTERNAL STATE
        // ============================================================

        private bool pointerInside;

        private bool pointerPressed;

        private bool selected;


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

            ResetRuntimeState();

            RefreshCurrentState();
        }


        // ============================================================
        // DISABLE
        // ============================================================

        private void OnDisable()
        {
            ResetRuntimeState();
        }


        // ============================================================
        // ROUTER
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
        // RESET STATE
        // ============================================================

        private void ResetRuntimeState()
        {
            pointerInside = false;

            pointerPressed = false;

            selected = false;
        }


        // ============================================================
        // ROUTE EVENT
        // ============================================================

        private void Route(
            SparkVFXEventType eventType)
        {
            ResolveRouter();


            if (eventRouter == null)
            {
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
        // POINTER ENTER
        // ============================================================

        public void OnPointerEnter(
            PointerEventData eventData)
        {
            pointerInside = true;


            if (!IsInteractable())
            {
                return;
            }


            if (!sendHoverEnter)
            {
                return;
            }


            Route(
                SparkVFXEventType.HoverEnter
            );
        }


        // ============================================================
        // POINTER EXIT
        // ============================================================

        public void OnPointerExit(
            PointerEventData eventData)
        {
            pointerInside = false;


            if (!IsInteractable())
            {
                return;
            }


            if (!sendHoverExit)
            {
                return;
            }


            Route(
                SparkVFXEventType.HoverExit
            );
        }


        // ============================================================
        // POINTER DOWN
        // ============================================================

        public void OnPointerDown(
            PointerEventData eventData)
        {
            pointerPressed = true;


            if (!IsInteractable())
            {
                return;
            }


            if (!sendPress)
            {
                return;
            }


            Route(
                SparkVFXEventType.Press
            );
        }


        // ============================================================
        // POINTER UP
        // ============================================================

        public void OnPointerUp(
            PointerEventData eventData)
        {
            pointerPressed = false;


            if (!IsInteractable())
            {
                return;
            }


            if (!sendRelease)
            {
                return;
            }


            Route(
                SparkVFXEventType.Release
            );
        }


        // ============================================================
        // SELECT
        // ============================================================

        public void OnSelect(
            BaseEventData eventData)
        {
            selected = true;


            if (!IsInteractable())
            {
                return;
            }


            if (!sendSelected)
            {
                return;
            }


            Route(
                SparkVFXEventType.Selected
            );
        }


        // ============================================================
        // DESELECT
        // ============================================================

        public void OnDeselect(
            BaseEventData eventData)
        {
            selected = false;


            if (!IsInteractable())
            {
                return;
            }


            if (!sendNormal)
            {
                return;
            }


            Route(
                SparkVFXEventType.Normal
            );
        }


        // ============================================================
        // CHECK INTERACTABLE
        // ============================================================

        private bool IsInteractable()
        {
            ResolveSelectable();


            if (selectable == null)
            {
                return true;
            }


            return selectable.IsInteractable();
        }


        // ============================================================
        // REFRESH CURRENT STATE
        // ============================================================

        public void RefreshCurrentState()
        {
            ResolveSelectable();


            if (selectable == null)
            {
                return;
            }


            if (!selectable.IsInteractable())
            {
                if (sendDisabled)
                {
                    Route(
                        SparkVFXEventType.Disabled
                    );
                }

                return;
            }


            if (selected)
            {
                if (sendSelected)
                {
                    Route(
                        SparkVFXEventType.Selected
                    );
                }

                return;
            }


            if (pointerPressed)
            {
                if (sendPress)
                {
                    Route(
                        SparkVFXEventType.Press
                    );
                }

                return;
            }


            if (pointerInside)
            {
                if (sendHoverEnter)
                {
                    Route(
                        SparkVFXEventType.HoverEnter
                    );
                }

                return;
            }


            if (sendNormal)
            {
                Route(
                    SparkVFXEventType.Normal
                );
            }
        }


        // ============================================================
        // MANUAL REFRESH
        // ============================================================

        public void Refresh()
        {
            RefreshCurrentState();
        }


        // ============================================================
        // MANUAL STATE
        // ============================================================

        public void PlayNormal()
        {
            Route(
                SparkVFXEventType.Normal
            );
        }


        public void PlayHoverEnter()
        {
            Route(
                SparkVFXEventType.HoverEnter
            );
        }


        public void PlayHoverExit()
        {
            Route(
                SparkVFXEventType.HoverExit
            );
        }


        public void PlayPress()
        {
            Route(
                SparkVFXEventType.Press
            );
        }


        public void PlayRelease()
        {
            Route(
                SparkVFXEventType.Release
            );
        }


        public void PlaySelected()
        {
            Route(
                SparkVFXEventType.Selected
            );
        }


        public void PlayDisabled()
        {
            Route(
                SparkVFXEventType.Disabled
            );
        }


        // ============================================================
        // VALIDATION
        // ============================================================

        public bool Validate(
            bool logWarning = true)
        {
            ResolveSelectable();

            ResolveRouter();


            bool valid = true;


            if (selectable == null)
            {
                valid = false;


                if (logWarning)
                {
                    Debug.LogWarning(
                        "[SparkVFXUISelectableBridge] " +
                        "No Unity Selectable component found.",
                        this
                    );
                }
            }


            if (eventRouter == null)
            {
                valid = false;


                if (logWarning)
                {
                    Debug.LogWarning(
                        "[SparkVFXUISelectableBridge] " +
                        "No SparkVFXEventRouter found.",
                        this
                    );
                }
            }


            return valid;
        }


        // ============================================================
        // EDITOR
        // ============================================================

#if UNITY_EDITOR

        [ContextMenu(
            "Validate Selectable Bridge"
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
                    "[SparkVFXUISelectableBridge] " +
                    "Validation successful.",
                    this
                );
            }
            else
            {
                Debug.LogError(
                    "[SparkVFXUISelectableBridge] " +
                    "Validation failed.",
                    this
                );
            }
        }


        [ContextMenu(
            "Refresh Current VFX State"
        )]
        private void RefreshFromContextMenu()
        {
            RefreshCurrentState();
        }

#endif
    }
}