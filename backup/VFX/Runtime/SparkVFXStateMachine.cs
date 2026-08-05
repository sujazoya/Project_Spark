using UnityEngine;
using System.Collections;

namespace ProjectSpark.UI.VFX
{
    [DisallowMultipleComponent]
    public sealed class SparkVFXStateMachine : MonoBehaviour
    {
        // ============================================================
        // REFERENCES
        // ============================================================

        [Header("References")]

        [SerializeField]
        private SparkVFXRuntime runtime;

        [SerializeField]
        private SparkVFXController controller;

        [SerializeField]
        private SparkVFXLoop loop;

        [SerializeField]
        private SparkVFXSequencePlayer sequencePlayer;


        // ============================================================
        // CURRENT STATE
        // ============================================================

        [Header("Current State")]

        [SerializeField]
        private SparkVFXEventType currentState =
            SparkVFXEventType.Normal;


        [SerializeField]
        private SparkVFXEventType previousState =
            SparkVFXEventType.Normal;


        // ============================================================
        // PRIORITY
        // ============================================================

        [Header("Priority")]

        [SerializeField]
        private int currentPriority = 0;


        // ============================================================
        // STATE
        // ============================================================

        private bool initialized;


        // ============================================================
        // PROPERTIES
        // ============================================================

        public SparkVFXEventType CurrentState
        {
            get
            {
                return currentState;
            }
        }


        public SparkVFXEventType PreviousState
        {
            get
            {
                return previousState;
            }
        }


        public int CurrentPriority
        {
            get
            {
                return currentPriority;
            }
        }

        // ============================================================
        // PRESS OVERRIDE
        // ============================================================

        [Header("Press Override")]

        [SerializeField]
        private float pressDuration = 0.18f;


        private Coroutine pressRoutine;

        // ============================================================
        // AWAKE
        // ============================================================

        private void Awake()
        {
            Initialize();
        }


        // ============================================================
        // INITIALIZE
        // ============================================================

        public void Initialize()
        {
            if (initialized)
            {
                return;
            }


            if (runtime == null)
            {
                runtime =
                    GetComponent<
                        SparkVFXRuntime>();
            }


            if (controller == null)
            {
                controller =
                    GetComponent<
                        SparkVFXController>();
            }


            if (loop == null)
            {
                loop =
                    GetComponent<
                        SparkVFXLoop>();
            }


            if (sequencePlayer == null)
            {
                sequencePlayer =
                    GetComponent<
                        SparkVFXSequencePlayer>();
            }


            initialized =
                true;
        }


        // ============================================================
        // CHANGE STATE
        // ============================================================

        public bool SetState(
            SparkVFXEventType newState)
        {
            return SetState(
                newState,
                GetPriority(
                    newState
                )
            );
        }


        // ============================================================
        // CHANGE STATE WITH PRIORITY
        // ============================================================

        public bool SetState(
     SparkVFXEventType newState,
     int priority)
        {
            Initialize();


            if (
                newState ==
                currentState
            )
            {
                return true;
            }


            previousState =
                currentState;


            currentState =
                newState;


            currentPriority =
                priority;


            ApplyState(
                newState
            );


            return true;
        }

        // ============================================================
        // APPLY STATE
        // ============================================================

        private void ApplyState(
            SparkVFXEventType state)
        {
            if (runtime == null)
            {
                return;
            }


            switch (state)
            {
                case SparkVFXEventType.Normal:

                    StopLoop();

                    runtime.PlayProfile(
                        SparkVFXEventType.Normal
                    );

                    break;


                case SparkVFXEventType.HoverEnter:

                    StopLoop();

                    runtime.PlayProfile(
                        SparkVFXEventType.HoverEnter
                    );

                    break;


                case SparkVFXEventType.HoverExit:

                    StopLoop();

                    runtime.PlayProfile(
                        SparkVFXEventType.Normal
                    );

                    break;


                case SparkVFXEventType.Selected:

                    StopLoop();

                    runtime.PlayProfile(
                        SparkVFXEventType.Selected
                    );

                    break;


                case SparkVFXEventType.Target:

                    runtime.PlayProfile(
                        SparkVFXEventType.Target
                    );

                    StartLoop();

                    break;


                case SparkVFXEventType.Disabled:

                    StopLoop();

                    runtime.PlayProfile(
                        SparkVFXEventType.Disabled
                    );

                    break;


                case SparkVFXEventType.Locked:

                    runtime.PlayProfile(
                        SparkVFXEventType.Locked
                    );

                    StartLoop();

                    break;


                case SparkVFXEventType.Warning:

                    runtime.PlayProfile(
                        SparkVFXEventType.Warning
                    );

                    StartLoop();

                    break;


                case SparkVFXEventType.Error:

                    StopLoop();

                    runtime.PlayProfile(
                        SparkVFXEventType.Error
                    );

                    break;


                case SparkVFXEventType.Success:

                    StopLoop();

                    runtime.PlayProfile(
                        SparkVFXEventType.Success
                    );

                    break;
            }
        }


        // ============================================================
        // PRESS
        // ============================================================


        public void Press()
        {
            Initialize();


            if (runtime == null)
            {
                return;
            }


            if (pressRoutine != null)
            {
                StopCoroutine(
                    pressRoutine
                );
            }


            pressRoutine =
                StartCoroutine(
                    PressRoutine()
                );
        }


        // ============================================================
        // PRESS ROUTINE
        // ============================================================

        private IEnumerator PressRoutine()
        {
            runtime.Press();


            yield return
                new WaitForSecondsRealtime(
                    pressDuration
                );


            pressRoutine =
                null;


            ReapplyCurrentState();
        }

        // ============================================================
        // REAPPLY CURRENT STATE
        // ============================================================

        private void ReapplyCurrentState()
        {
            ApplyState(
                currentState
            );
        }


        // ============================================================
        // SHOW
        // ============================================================

        public void Show()
        {
            Initialize();


            runtime.Show();
        }


        // ============================================================
        // HIDE
        // ============================================================

        public void Hide()
        {
            Initialize();


            StopLoop();


            runtime.Hide();
        }


        // ============================================================
        // SUCCESS
        // ============================================================

        public void PlaySuccess()
        {
            Initialize();


            StopLoop();


            runtime.Success();


            currentState =
                SparkVFXEventType.Success;


            currentPriority =
                GetPriority(
                    SparkVFXEventType.Success
                );
        }


        // ============================================================
        // ERROR
        // ============================================================

        public void PlayError()
        {
            Initialize();


            StopLoop();


            runtime.Error();


            currentState =
                SparkVFXEventType.Error;


            currentPriority =
                GetPriority(
                    SparkVFXEventType.Error
                );
        }


        // ============================================================
        // START LOOP
        // ============================================================

        public void StartLoop()
        {
            if (loop == null)
            {
                return;
            }


            loop.Play();
        }


        // ============================================================
        // STOP LOOP
        // ============================================================

        public void StopLoop()
        {
            if (loop == null)
            {
                return;
            }


            loop.Stop();
        }


        // ============================================================
        // RESET PRIORITY
        // ============================================================

        public void ResetPriority()
        {
            currentPriority =
                GetPriority(
                    currentState
                );
        }


        // ============================================================
        // GET PRIORITY
        // ============================================================

        private int GetPriority(
            SparkVFXEventType state)
        {
            switch (state)
            {
                case SparkVFXEventType.Normal:
                    return 0;


                case SparkVFXEventType.HoverEnter:
                    return 10;


                case SparkVFXEventType.HoverExit:
                    return 0;


                case SparkVFXEventType.Selected:
                    return 20;


                case SparkVFXEventType.Target:
                    return 30;


                case SparkVFXEventType.Disabled:
                    return 100;


                case SparkVFXEventType.Locked:
                    return 40;


                case SparkVFXEventType.Warning:
                    return 60;


                case SparkVFXEventType.Error:
                    return 80;


                case SparkVFXEventType.Success:
                    return 90;


                case SparkVFXEventType.Press:
                    return 70;


                default:
                    return 0;
            }
        }
    }
}