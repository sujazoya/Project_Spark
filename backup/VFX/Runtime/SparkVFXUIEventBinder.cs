using UnityEngine;
using UnityEngine.EventSystems;

namespace ProjectSpark.UI.VFX
{
    /// <summary>
    /// Connects Unity UI pointer and selection events
    /// to SparkVFXEventRouter.
    ///
    /// Supports:
    /// - Hover enter
    /// - Hover exit
    /// - Select
    /// - Deselect
    /// - Pointer press
    /// - Pointer release
    /// - Submit
    ///
    /// This component does not:
    /// - Resolve profiles
    /// - Control materials
    /// - Access ISparkVFXController directly
    /// - Manage VFX state priority
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class SparkVFXUIEventBinder
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
        // ROUTER
        // ============================================================

        [Header("Event Router")]

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

        [Tooltip(
            "If enabled, UI events are applied instantly. " +
            "Otherwise normal profile transitions are used."
        )]
        [SerializeField]
        private bool instant;


        // ============================================================
        // EVENT ENABLE FLAGS
        // ============================================================

        [Header("Events")]

        [SerializeField]
        private bool enableHoverEnter = true;

        [SerializeField]
        private bool enableHoverExit = true;

        [SerializeField]
        private bool enablePointerDown = true;

        [SerializeField]
        private bool enablePointerUp = true;

        [SerializeField]
        private bool enableSelect = true;

        [SerializeField]
        private bool enableDeselect = true;

        [SerializeField]
        private bool enableSubmit = true;


        // ============================================================
        // AWAKE
        // ============================================================

        private void Awake()
        {
            ResolveRouter();
        }


        private void OnEnable()
        {
            ResolveRouter();
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
        // ROUTE EVENT
        // ============================================================

        private void Route(
            SparkVFXEventType eventType)
        {
            ResolveRouter();


            if (eventRouter == null)
            {
                Debug.LogWarning(
                    "[SparkVFXUIEventBinder] " +
                    "SparkVFXEventRouter is not assigned " +
                    "or could not be found.",
                    this
                );

                return;
            }


            eventRouter.SendEvent(
                eventType,
                instant
            );
        }


        // ============================================================
        // POINTER ENTER
        // ============================================================

        public void OnPointerEnter(
            PointerEventData eventData)
        {
            if (!enableHoverEnter)
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
            if (!enableHoverExit)
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
            if (!enablePointerDown)
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
            if (!enablePointerUp)
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
            if (!enableSelect)
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
            if (!enableDeselect)
            {
                return;
            }


            Route(
                SparkVFXEventType.Normal
            );
        }


        // ============================================================
        // SUBMIT
        // ============================================================

        public void OnSubmit(
            BaseEventData eventData)
        {
            if (!enableSubmit)
            {
                return;
            }


            Route(
                SparkVFXEventType.Submit
            );
        }


        // ============================================================
        // MANUAL EVENTS
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


        public void PlaySelected()
        {
            Route(
                SparkVFXEventType.Selected
            );
        }


        public void PlayTarget()
        {
            Route(
                SparkVFXEventType.Target
            );
        }


        public void PlayWarning()
        {
            Route(
                SparkVFXEventType.Warning
            );
        }


        public void PlayError()
        {
            Route(
                SparkVFXEventType.Error
            );
        }


        public void PlaySuccess()
        {
            Route(
                SparkVFXEventType.Success
            );
        }


        public void PlayConfirm()
        {
            Route(
                SparkVFXEventType.Confirm
            );
        }


        public void PlayCancel()
        {
            Route(
                SparkVFXEventType.Cancel
            );
        }


        public void PlayAlert()
        {
            Route(
                SparkVFXEventType.Alert
            );
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
                    "[SparkVFXUIEventBinder] " +
                    "No SparkVFXEventRouter found.",
                    this
                );
            }


            return false;
        }


#if UNITY_EDITOR

        [ContextMenu(
            "Validate UI Event Binder"
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
                    "[SparkVFXUIEventBinder] " +
                    "Validation successful.",
                    this
                );
            }
            else
            {
                Debug.LogError(
                    "[SparkVFXUIEventBinder] " +
                    "Validation failed.",
                    this
                );
            }
        }

#endif
    }
}