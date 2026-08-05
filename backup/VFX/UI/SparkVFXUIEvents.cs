using UnityEngine;
using UnityEngine.EventSystems;

namespace ProjectSpark.UI.VFX
{
    /// <summary>
    /// UI event bridge for Project Spark VFX.
    ///
    /// Converts Unity UI events into VFX state requests.
    ///
    /// This component does NOT:
    /// - Find VFX controllers.
    /// - Access shader properties.
    /// - Modify materials.
    /// - Control SparkVFXController directly.
    /// - Control SparkTMPVFXController directly.
    ///
    /// It only communicates with SparkVFXLayeredStateMachine.
    ///
    /// Architecture:
    ///
    /// Unity UI Event
    ///       |
    ///       v
    /// SparkVFXUIEvents
    ///       |
    ///       v
    /// SparkVFXLayeredStateMachine
    ///       |
    ///       v
    /// SparkVFXSequencePlayer / SparkVFXLoop
    ///       |
    ///       v
    /// ISparkVFXController
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class SparkVFXUIEvents
        : MonoBehaviour,
          IPointerEnterHandler,
          IPointerExitHandler,
          IPointerDownHandler,
          IPointerUpHandler,
          IPointerClickHandler,
          ISelectHandler,
          IDeselectHandler,
          ISubmitHandler
    {
        // ============================================================
        // REFERENCES
        // ============================================================

        [Header("State Machine")]

        [Tooltip(
            "Layered VFX state machine that receives UI state requests."
        )]
        [SerializeField]
        public SparkVFXLayeredStateMachine stateMachine;


        // ============================================================
        // ENABLE / DISABLE
        // ============================================================

        [Header("Lifecycle")]

        [SerializeField]
        private bool playEnableState = true;


        [SerializeField]
        private bool playDisableState = true;


        // ============================================================
        // POINTER STATES
        // ============================================================

        [Header("Pointer States")]

        [SerializeField]
        private bool usePointerStates = true;


        [SerializeField]
        private string pointerEnterState = "Hover";


        [SerializeField]
        private string pointerExitState = "Normal";


        [SerializeField]
        private string pointerDownState = "Pressed";


        [SerializeField]
        private string pointerUpState = "Hover";


        // ============================================================
        // CLICK
        // ============================================================

        [Header("Click")]

        [SerializeField]
        private bool useClickState = true;


        [SerializeField]
        private string clickState = "Click";


        // ============================================================
        // SELECTION
        // ============================================================

        [Header("Selection")]

        [SerializeField]
        private bool useSelectionStates = true;


        [SerializeField]
        private string selectedState = "Selected";


        [SerializeField]
        private string deselectedState = "Normal";


        // ============================================================
        // SUBMIT
        // ============================================================

        [Header("Submit")]

        [SerializeField]
        private bool useSubmitState = true;


        [SerializeField]
        private string submitState = "Submit";


        // ============================================================
        // RUNTIME
        // ============================================================

        private bool initialized;


        private bool pointerInside;


        private bool pointerPressed;


        // ============================================================
        // UNITY
        // ============================================================

        private void Awake()
        {
            ResolveStateMachine();

            initialized =
                stateMachine != null;
        }


        private void OnEnable()
        {
            if (!initialized)
            {
                ResolveStateMachine();

                initialized =
                    stateMachine != null;
            }


            if (playEnableState)
            {
                RequestState(
                    "Enable"
                );
            }
        }


        private void OnDisable()
        {
            if (playDisableState)
            {
                RequestState(
                    "Disable"
                );
            }
        }


        // ============================================================
        // RESOLVE STATE MACHINE
        // ============================================================

        private void ResolveStateMachine()
        {
            if (stateMachine != null)
            {
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
                return;
            }


            // --------------------------------------------------------
            // CHILDREN
            // --------------------------------------------------------

            stateMachine =
                GetComponentInChildren<
                    SparkVFXLayeredStateMachine
                >(
                    true
                );
        }


        // ============================================================
        // REQUEST STATE
        // ============================================================

        private void RequestState(
     string stateName)
        {
            if (string.IsNullOrWhiteSpace(
                stateName
            ))
            {
                return;
            }


            if (stateMachine == null)
            {
                ResolveStateMachine();
            }


            if (stateMachine == null)
            {
                return;
            }


            stateMachine.RequestState(
                stateName
            );
        }


        // ============================================================
        // POINTER ENTER
        // ============================================================

        public void OnPointerEnter(
            PointerEventData eventData)
        {
            pointerInside =
                true;


            if (!usePointerStates)
            {
                return;
            }


            RequestState(
                pointerEnterState
            );
        }


        // ============================================================
        // POINTER EXIT
        // ============================================================

        public void OnPointerExit(
            PointerEventData eventData)
        {
            pointerInside =
                false;


            pointerPressed =
                false;


            if (!usePointerStates)
            {
                return;
            }


            RequestState(
                pointerExitState
            );
        }


        // ============================================================
        // POINTER DOWN
        // ============================================================

        public void OnPointerDown(
            PointerEventData eventData)
        {
            pointerPressed =
                true;


            if (!usePointerStates)
            {
                return;
            }


            RequestState(
                pointerDownState
            );
        }


        // ============================================================
        // POINTER UP
        // ============================================================

        public void OnPointerUp(
            PointerEventData eventData)
        {
            pointerPressed =
                false;


            if (!usePointerStates)
            {
                return;
            }


            // --------------------------------------------------------
            // If pointer is still over the UI element,
            // return to Hover.
            // Otherwise return to Normal.
            // --------------------------------------------------------

            if (pointerInside)
            {
                RequestState(
                    pointerUpState
                );
            }
            else
            {
                RequestState(
                    pointerExitState
                );
            }
        }


        // ============================================================
        // POINTER CLICK
        // ============================================================

        public void OnPointerClick(
            PointerEventData eventData)
        {
            if (!useClickState)
            {
                return;
            }


            RequestState(
                clickState
            );
        }


        // ============================================================
        // SELECT
        // ============================================================

        public void OnSelect(
            BaseEventData eventData)
        {
            if (!useSelectionStates)
            {
                return;
            }


            RequestState(
                selectedState
            );
        }


        // ============================================================
        // DESELECT
        // ============================================================

        public void OnDeselect(
            BaseEventData eventData)
        {
            if (!useSelectionStates)
            {
                return;
            }


            RequestState(
                deselectedState
            );
        }


        // ============================================================
        // SUBMIT
        // ============================================================

        public void OnSubmit(
            BaseEventData eventData)
        {
            if (!useSubmitState)
            {
                return;
            }


            RequestState(
                submitState
            );
        }


        // ============================================================
        // MANUAL EVENTS
        // ============================================================

        public void TriggerHover()
        {
            RequestState(
                pointerEnterState
            );
        }


        public void TriggerNormal()
        {
            RequestState(
                pointerExitState
            );
        }


        public void TriggerPressed()
        {
            RequestState(
                pointerDownState
            );
        }


        public void TriggerSelected()
        {
            RequestState(
                selectedState
            );
        }


        public void TriggerClick()
        {
            RequestState(
                clickState
            );
        }


        public void TriggerSubmit()
        {
            RequestState(
                submitState
            );
        }


        // ============================================================
        // REFRESH
        // ============================================================

        public void Refresh()
        {
            stateMachine =
                null;


            initialized =
                false;


            ResolveStateMachine();


            initialized =
                stateMachine != null;
        }


        // ============================================================
        // DEBUG
        // ============================================================

        [ContextMenu("TEST / Hover")]
        private void TestHover()
        {
            TriggerHover();
        }


        [ContextMenu("TEST / Normal")]
        private void TestNormal()
        {
            TriggerNormal();
        }


        [ContextMenu("TEST / Pressed")]
        private void TestPressed()
        {
            TriggerPressed();
        }


        [ContextMenu("TEST / Selected")]
        private void TestSelected()
        {
            TriggerSelected();
        }


        [ContextMenu("TEST / Click")]
        private void TestClick()
        {
            TriggerClick();
        }


        [ContextMenu("TEST / Submit")]
        private void TestSubmit()
        {
            TriggerSubmit();
        }


        // ============================================================
        // EDITOR VALIDATION
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