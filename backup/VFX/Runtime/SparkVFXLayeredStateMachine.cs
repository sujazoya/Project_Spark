using System.Collections;
using UnityEngine;

namespace ProjectSpark.UI.VFX
{
    /// <summary>
    /// Central layered VFX state machine for Project Spark.
    ///
    /// Layer priority:
    ///
    /// 1. Override
    /// 2. Sequence
    /// 3. Loop
    /// 4. Base State
    ///
    /// Controller architecture:
    ///
    /// SparkVFXLayeredStateMachine
    ///          |
    ///          v
    ///     SparkVFXTarget
    ///          |
    ///          v
    ///  ISparkVFXController
    ///
    /// Responsibilities:
    /// - Manages the complete VFX layer hierarchy.
    /// - Manages base states.
    /// - Manages continuous loop effects.
    /// - Manages sequence/timeline playback.
    /// - Manages temporary override effects.
    /// - Resolves logical states through SparkVFXStateResolver.
    /// - Resolves feedback sequences through SparkVFXRuntime.
    /// - Restores lower-priority layers when higher-priority layers finish.
    ///
    /// IMPORTANT:
    /// - Does NOT serialize ISparkVFXController.
    /// - SparkVFXTarget resolves the concrete controller.
    /// - Does NOT use SparkVFXStateResolver for visual layer priority.
    /// - SparkVFXStateResolver is used for logical state requests.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class SparkVFXLayeredStateMachine
        : MonoBehaviour
    {
        // ============================================================
        // REFERENCES
        // ============================================================

        [Header("References")]

        [Tooltip(
            "Central resolver for SparkVFXController " +
            "and SparkTMPVFXController."
        )]
        [SerializeField]
        private SparkVFXTarget target;


        [Tooltip(
            "Runtime system responsible for VFX event/profile playback."
        )]
        [SerializeField]
        private SparkVFXRuntime runtime;


        [Tooltip(
            "Optional continuous VFX loop layer."
        )]
        [SerializeField]
        private SparkVFXLoop loop;


        [Tooltip(
            "Optional VFX sequence/timeline layer."
        )]
        [SerializeField]
        private SparkVFXSequencePlayer sequencePlayer;


        // ============================================================
        // RUNTIME CONTROLLER
        // ============================================================

        private ISparkVFXController controller;


        // ============================================================
        // STATE RESOLVER
        // ============================================================

        [Header("State Resolver")]

        [Tooltip(
            "Resolves logical VFX state IDs into state definitions " +
            "and VFX profiles."
        )]
        [SerializeField]
        private SparkVFXStateResolver stateResolver;


        private string currentStateID;


        // ============================================================
        // LAYER STATE
        // ============================================================

        [Header("Layer State")]

        [SerializeField]
        private SparkVFXBaseState baseState =
            SparkVFXBaseState.Normal;


        [SerializeField]
        private SparkVFXLoopType loopType =
            SparkVFXLoopType.None;


        [SerializeField]
        private SparkVFXOverrideType overrideType =
            SparkVFXOverrideType.None;


        // ============================================================
        // ACTIVE SEQUENCE
        // ============================================================

        [Header("Active Sequence")]

        [SerializeField]
        private SparkVFXSequence activeSequence;


        // ============================================================
        // OVERRIDE SETTINGS
        // ============================================================

        [Header("Override Settings")]

        [Min(0f)]
        [SerializeField]
        private float defaultOverrideDuration =
            0.25f;


        // ============================================================
        // RUNTIME
        // ============================================================

        private Coroutine overrideRoutine;

        private bool initialized;

        private bool resolving;


        // ============================================================
        // PUBLIC PROPERTIES
        // ============================================================

        public SparkVFXBaseState BaseState
        {
            get
            {
                return baseState;
            }
        }


        public SparkVFXLoopType LoopType
        {
            get
            {
                return loopType;
            }
        }


        public SparkVFXOverrideType OverrideType
        {
            get
            {
                return overrideType;
            }
        }


        public SparkVFXSequence ActiveSequence
        {
            get
            {
                return activeSequence;
            }
        }


        public bool HasOverride
        {
            get
            {
                return
                    overrideType !=
                    SparkVFXOverrideType.None;
            }
        }


        public bool HasSequence
        {
            get
            {
                return activeSequence != null;
            }
        }


        public bool IsSequencePlaying
        {
            get
            {
                return
                    sequencePlayer != null &&
                    sequencePlayer.IsPlaying;
            }
        }


        public bool IsSequencePaused
        {
            get
            {
                return
                    sequencePlayer != null &&
                    sequencePlayer.IsPaused;
            }
        }


        public float SequencePlaybackTime
        {
            get
            {
                if (sequencePlayer == null)
                {
                    return 0f;
                }


                return sequencePlayer.PlaybackTime;
            }
        }


        public bool IsInitialized
        {
            get
            {
                return initialized;
            }
        }


        public string CurrentStateID
        {
            get
            {
                return currentStateID;
            }
        }


        public ISparkVFXController Controller
        {
            get
            {
                if (!EnsureReady())
                {
                    return null;
                }


                return controller;
            }
        }


        public SparkVFXTarget Target
        {
            get
            {
                ResolveTarget();

                return target;
            }
        }


        public SparkVFXRuntime Runtime
        {
            get
            {
                ResolveRuntime();

                return runtime;
            }
        }


        public SparkVFXLoop Loop
        {
            get
            {
                ResolveLoop();

                return loop;
            }
        }


        public SparkVFXSequencePlayer SequencePlayer
        {
            get
            {
                ResolveSequencePlayer();

                SubscribeToSequencePlayer();

                return sequencePlayer;
            }
        }


        public SparkVFXStateResolver StateResolver
        {
            get
            {
                ResolveStateResolver();

                return stateResolver;
            }
        }


        // ============================================================
        // UNITY AWAKE
        // ============================================================

        private void Awake()
        {
            Initialize();
        }


        // ============================================================
        // UNITY ENABLE
        // ============================================================

        private void OnEnable()
        {
            if (!initialized)
            {
                Initialize();
            }
            else
            {
                SubscribeToSequencePlayer();
            }
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


            if (resolving)
            {
                return;
            }


            resolving =
                true;


            // --------------------------------------------------------
            // RESOLVE REFERENCES
            // --------------------------------------------------------

            ResolveTarget();

            ResolveRuntime();

            ResolveLoop();

            ResolveSequencePlayer();

            ResolveStateResolver();


            // --------------------------------------------------------
            // SUBSCRIBE TO SEQUENCE EVENTS
            // --------------------------------------------------------

            SubscribeToSequencePlayer();


            // --------------------------------------------------------
            // RESOLVE CONTROLLER THROUGH TARGET
            // --------------------------------------------------------

            if (target != null)
            {
                controller =
                    target.Controller;
            }


            // --------------------------------------------------------
            // CONTROLLER REQUIRED
            // --------------------------------------------------------

            if (controller == null)
            {
                Debug.LogError(
                    "[SparkVFXLayeredStateMachine] " +
                    "No compatible VFX controller found. " +
                    "Add SparkVFXTarget with either " +
                    "SparkVFXController or SparkTMPVFXController.",
                    this
                );


                resolving =
                    false;


                return;
            }


            // --------------------------------------------------------
            // INITIALIZE CONTROLLER
            // --------------------------------------------------------

            controller.Initialize();


            // --------------------------------------------------------
            // COMPLETE
            // --------------------------------------------------------

            initialized =
                true;


            resolving =
                false;
        }


        // ============================================================
        // RESOLVE TARGET
        // ============================================================

        private void ResolveTarget()
        {
            if (target != null)
            {
                return;
            }


            target =
                GetComponent<
                    SparkVFXTarget
                >();


            if (target != null)
            {
                return;
            }


            target =
                GetComponentInParent<
                    SparkVFXTarget
                >(
                    true
                );


            if (target != null)
            {
                return;
            }


            target =
                GetComponentInChildren<
                    SparkVFXTarget
                >(
                    true
                );
        }


        // ============================================================
        // RESOLVE RUNTIME
        // ============================================================

        private void ResolveRuntime()
        {
            if (runtime != null)
            {
                return;
            }


            runtime =
                GetComponent<
                    SparkVFXRuntime
                >();


            if (runtime != null)
            {
                return;
            }


            runtime =
                GetComponentInChildren<
                    SparkVFXRuntime
                >(
                    true
                );
        }


        // ============================================================
        // RESOLVE LOOP
        // ============================================================

        private void ResolveLoop()
        {
            if (loop != null)
            {
                return;
            }


            loop =
                GetComponent<
                    SparkVFXLoop
                >();


            if (loop != null)
            {
                return;
            }


            loop =
                GetComponentInChildren<
                    SparkVFXLoop
                >(
                    true
                );
        }


        // ============================================================
        // RESOLVE SEQUENCE PLAYER
        // ============================================================

        private void ResolveSequencePlayer()
        {
            if (sequencePlayer != null)
            {
                return;
            }


            sequencePlayer =
                GetComponent<
                    SparkVFXSequencePlayer
                >();


            if (sequencePlayer != null)
            {
                return;
            }


            sequencePlayer =
                GetComponentInChildren<
                    SparkVFXSequencePlayer
                >(
                    true
                );
        }


        // ============================================================
        // RESOLVE STATE RESOLVER
        // ============================================================

        private void ResolveStateResolver()
        {
            if (stateResolver != null)
            {
                return;
            }


            stateResolver =
                GetComponent<
                    SparkVFXStateResolver
                >();


            if (stateResolver != null)
            {
                return;
            }


            stateResolver =
                GetComponentInChildren<
                    SparkVFXStateResolver
                >(
                    true
                );
        }


        // ============================================================
        // SEQUENCE EVENTS
        // ============================================================

        private void SubscribeToSequencePlayer()
        {
            if (sequencePlayer == null)
            {
                return;
            }


            sequencePlayer.SequenceCompleted -=
                OnSequenceCompleted;


            sequencePlayer.SequenceCompleted +=
                OnSequenceCompleted;
        }


        private void UnsubscribeFromSequencePlayer()
        {
            if (sequencePlayer == null)
            {
                return;
            }


            sequencePlayer.SequenceCompleted -=
                OnSequenceCompleted;
        }


        // ============================================================
        // SEQUENCE COMPLETED
        // ============================================================

        private void OnSequenceCompleted()
        {
            // --------------------------------------------------------
            // OVERRIDE HAS PRIORITY
            // --------------------------------------------------------

            if (HasOverride)
            {
                return;
            }


            // --------------------------------------------------------
            // CLEAR ACTIVE SEQUENCE
            // --------------------------------------------------------

            activeSequence =
                null;


            // --------------------------------------------------------
            // IF LOOP IS ACTIVE
            // --------------------------------------------------------

            if (
                loop != null &&
                loop.IsPlaying
            )
            {
                return;
            }


            // --------------------------------------------------------
            // RESTORE BASE STATE
            // --------------------------------------------------------

            RestoreLayers();
        }


        // ============================================================
        // ENSURE READY
        // ============================================================

        private bool EnsureReady()
        {
            if (
                initialized &&
                controller != null
            )
            {
                return true;
            }


            if (resolving)
            {
                return false;
            }


            Initialize();


            return
                initialized &&
                controller != null;
        }


        // ============================================================
        // BASE STATE
        // ============================================================

        public void SetBaseState(
            SparkVFXBaseState newState)
        {
            if (!EnsureReady())
            {
                return;
            }


            if (
                baseState ==
                newState
            )
            {
                return;
            }


            baseState =
                newState;


            if (HasOverride)
            {
                return;
            }


            if (IsSequencePlaying)
            {
                return;
            }


            ApplyBaseState();

            ApplyLoopLayer();
        }


        // ============================================================
        // SET BASE STATE INSTANT
        // ============================================================

        public void SetBaseStateInstant(
            SparkVFXBaseState newState)
        {
            if (!EnsureReady())
            {
                return;
            }


            baseState =
                newState;


            if (HasOverride)
            {
                return;
            }


            if (IsSequencePlaying)
            {
                return;
            }


            ApplyBaseState();

            ApplyLoopLayer();
        }


        // ============================================================
        // APPLY BASE STATE
        // ============================================================

        private void ApplyBaseState()
        {
            if (runtime == null)
            {
                return;
            }


            switch (baseState)
            {
                case SparkVFXBaseState.Normal:

                    runtime.PlayProfile(
                        SparkVFXEventType.Normal
                    );

                    break;


                case SparkVFXBaseState.Hover:

                    runtime.PlayProfile(
                        SparkVFXEventType.HoverEnter
                    );

                    break;


                case SparkVFXBaseState.Selected:

                    runtime.PlayProfile(
                        SparkVFXEventType.Selected
                    );

                    break;


                case SparkVFXBaseState.Target:

                    runtime.PlayProfile(
                        SparkVFXEventType.Target
                    );

                    break;


                case SparkVFXBaseState.Disabled:

                    StopLoop();

                    runtime.PlayProfile(
                        SparkVFXEventType.Disabled
                    );

                    break;


                case SparkVFXBaseState.Locked:

                    runtime.PlayProfile(
                        SparkVFXEventType.Locked
                    );

                    break;
            }
        }


        // ============================================================
        // LOOP
        // ============================================================

        public void SetLoop(
            SparkVFXLoopType newLoop)
        {
            if (!EnsureReady())
            {
                return;
            }


            loopType =
                newLoop;


            if (HasOverride)
            {
                return;
            }


            if (IsSequencePlaying)
            {
                return;
            }


            ApplyLoopLayer();
        }


        // ============================================================
        // APPLY LOOP LAYER
        // ============================================================

        private void ApplyLoopLayer()
        {
            if (HasOverride)
            {
                return;
            }


            if (IsSequencePlaying)
            {
                return;
            }


            if (loop == null)
            {
                return;
            }


            if (
                loopType ==
                SparkVFXLoopType.None
            )
            {
                StopLoop();

                return;
            }


            ConfigureLoop(
                loopType
            );


            loop.Play();
        }


        // ============================================================
        // CONFIGURE LOOP
        // ============================================================

        private void ConfigureLoop(
            SparkVFXLoopType type)
        {
            if (loop == null)
            {
                return;
            }


            switch (type)
            {
                case SparkVFXLoopType.Target:

                    loop.Configure(
                        1.2f,
                        true,
                        0.8f,
                        1.3f,
                        0.3f,
                        0.7f
                    );

                    break;


                case SparkVFXLoopType.Selected:

                    loop.Configure(
                        1.5f,
                        true,
                        0.7f,
                        1.1f,
                        0.2f,
                        0.5f
                    );

                    break;


                case SparkVFXLoopType.Warning:

                    loop.Configure(
                        0.8f,
                        true,
                        0.8f,
                        1.5f,
                        0.0f,
                        0.3f
                    );

                    break;


                case SparkVFXLoopType.Error:

                    loop.Configure(
                        0.6f,
                        true,
                        0.6f,
                        1.4f,
                        0.0f,
                        0.2f
                    );

                    break;


                case SparkVFXLoopType.Active:

                    loop.Configure(
                        2.0f,
                        true,
                        0.7f,
                        1.1f,
                        0.2f,
                        0.5f
                    );

                    break;


                case SparkVFXLoopType.Scan:

                    loop.Configure(
                        1.5f,
                        false,
                        0.6f,
                        1.0f,
                        0.0f,
                        1.0f
                    );

                    break;


                case SparkVFXLoopType.Processing:

                    loop.Configure(
                        1.2f,
                        false,
                        0.6f,
                        1.0f,
                        0.0f,
                        0.0f
                    );

                    break;


                case SparkVFXLoopType.Locked:

                    loop.Configure(
                        2.0f,
                        true,
                        0.4f,
                        0.8f,
                        0.0f,
                        0.2f
                    );

                    break;
            }
        }


        // ============================================================
        // PLAY SEQUENCE
        // ============================================================

        public bool PlaySequence(
            SparkVFXSequence newSequence)
        {
            return PlaySequence(
                newSequence,
                true
            );
        }


        public bool PlaySequence(
            SparkVFXSequence newSequence,
            bool restart)
        {
            if (!EnsureReady())
            {
                return false;
            }


            if (newSequence == null)
            {
                Debug.LogWarning(
                    "[SparkVFXLayeredStateMachine] " +
                    "Cannot play a null SparkVFXSequence.",
                    this
                );

                return false;
            }


            if (HasOverride)
            {
                return false;
            }


            if (sequencePlayer == null)
            {
                Debug.LogError(
                    "[SparkVFXLayeredStateMachine] " +
                    "No SparkVFXSequencePlayer found.",
                    this
                );

                return false;
            }


            // --------------------------------------------------------
            // STOP LOWER LOOP LAYER
            // --------------------------------------------------------

            StopLoop();


            // --------------------------------------------------------
            // ASSIGN ACTIVE SEQUENCE
            // --------------------------------------------------------

            activeSequence =
                newSequence;


            // --------------------------------------------------------
            // CONFIGURE PLAYER
            // --------------------------------------------------------

            sequencePlayer.SetLoop(
                false
            );


            sequencePlayer.SetSequence(
                newSequence
            );


            // --------------------------------------------------------
            // PLAY
            // --------------------------------------------------------

            if (restart)
            {
                sequencePlayer.Restart();
            }
            else
            {
                sequencePlayer.Play();
            }


            return true;
        }


        // ============================================================
        // PLAY SEQUENCE FORWARD
        // ============================================================

        public bool PlaySequenceForward(
            SparkVFXSequence newSequence)
        {
            if (!EnsureReady())
            {
                return false;
            }


            if (newSequence == null)
            {
                return false;
            }


            if (HasOverride)
            {
                return false;
            }


            if (sequencePlayer == null)
            {
                return false;
            }


            StopLoop();


            activeSequence =
                newSequence;


            sequencePlayer.SetLoop(
                false
            );


            sequencePlayer.SetSequence(
                newSequence
            );


            sequencePlayer.PlayForward();


            return true;
        }


        // ============================================================
        // PLAY SEQUENCE REVERSE
        // ============================================================

        public bool PlaySequenceReverse(
            SparkVFXSequence newSequence)
        {
            if (!EnsureReady())
            {
                return false;
            }


            if (newSequence == null)
            {
                return false;
            }


            if (HasOverride)
            {
                return false;
            }


            if (sequencePlayer == null)
            {
                return false;
            }


            StopLoop();


            activeSequence =
                newSequence;


            sequencePlayer.SetLoop(
                false
            );


            sequencePlayer.SetSequence(
                newSequence
            );


            sequencePlayer.PlayReverse();


            return true;
        }


        // ============================================================
        // PAUSE SEQUENCE
        // ============================================================

        public void PauseSequence()
        {
            if (sequencePlayer == null)
            {
                return;
            }


            sequencePlayer.Pause();
        }


        // ============================================================
        // RESUME SEQUENCE
        // ============================================================

        public void ResumeSequence()
        {
            if (sequencePlayer == null)
            {
                return;
            }


            sequencePlayer.Resume();
        }


        // ============================================================
        // STOP SEQUENCE
        // ============================================================

        public void StopSequence()
        {
            if (sequencePlayer != null)
            {
                sequencePlayer.Stop();
            }


            activeSequence =
                null;


            if (HasOverride)
            {
                return;
            }


            RestoreLayers();
        }


        // ============================================================
        // SHOW
        // ============================================================

        public void Show()
        {
            if (!EnsureReady())
            {
                return;
            }


            if (HasOverride)
            {
                return;
            }


            if (IsSequencePlaying)
            {
                return;
            }


            ApplyLoopLayer();
        }


        // ============================================================
        // HIDE
        // ============================================================

        public void Hide()
        {
            if (IsSequencePlaying)
            {
                StopSequence();

                return;
            }


            StopLoop();
        }


        // ============================================================
        // SHOW INSTANT
        // ============================================================

        public void ShowInstant()
        {
            if (!EnsureReady())
            {
                return;
            }


            if (HasOverride)
            {
                return;
            }


            if (IsSequencePlaying)
            {
                return;
            }


            ApplyLoopLayer();
        }


        // ============================================================
        // HIDE INSTANT
        // ============================================================

        public void HideInstant()
        {
            StopLoop();
        }


        // ============================================================
        // PLAY OVERRIDE
        // ============================================================

        public void PlayOverride(
            SparkVFXOverrideType type)
        {
            PlayOverride(
                type,
                defaultOverrideDuration
            );
        }


        // ============================================================
        // PLAY OVERRIDE WITH DURATION
        // ============================================================

        public void PlayOverride(
            SparkVFXOverrideType type,
            float duration)
        {
            if (!EnsureReady())
            {
                return;
            }


            if (
                type ==
                SparkVFXOverrideType.None
            )
            {
                return;
            }


            if (
                overrideRoutine !=
                null
            )
            {
                StopCoroutine(
                    overrideRoutine
                );
            }


            overrideRoutine =
                StartCoroutine(
                    OverrideRoutine(
                        type,
                        duration
                    )
                );
        }


        // ============================================================
        // OVERRIDE ROUTINE
        // ============================================================

        private IEnumerator OverrideRoutine(
            SparkVFXOverrideType type,
            float duration)
        {
            // --------------------------------------------------------
            // SET OVERRIDE
            // --------------------------------------------------------

            overrideType =
                type;


            // --------------------------------------------------------
            // OVERRIDE HAS HIGHEST PRIORITY
            // --------------------------------------------------------

            if (sequencePlayer != null)
            {
                sequencePlayer.Stop();
            }


            activeSequence =
                null;


            StopLoop();


            // --------------------------------------------------------
            // PLAY FEEDBACK SEQUENCE
            // --------------------------------------------------------

            PlayOverrideSequence(
                type
            );


            // --------------------------------------------------------
            // WAIT FOR OVERRIDE DURATION
            // --------------------------------------------------------

            if (duration > 0f)
            {
                yield return
                    new WaitForSecondsRealtime(
                        duration
                    );
            }


            // --------------------------------------------------------
            // CLEAR OVERRIDE
            // --------------------------------------------------------

            overrideType =
                SparkVFXOverrideType.None;


            overrideRoutine =
                null;


            // --------------------------------------------------------
            // RESTORE LOWER PRIORITY LAYERS
            // --------------------------------------------------------

            RestoreLayers();
        }


        // ============================================================
        // PLAY OVERRIDE SEQUENCE
        // ============================================================

        private void PlayOverrideSequence(
            SparkVFXOverrideType type)
        {
            if (runtime == null)
            {
                Debug.LogWarning(
                    "[SparkVFXLayeredStateMachine] " +
                    "SparkVFXRuntime is missing.",
                    this
                );

                return;
            }


            if (sequencePlayer == null)
            {
                Debug.LogWarning(
                    "[SparkVFXLayeredStateMachine] " +
                    "SparkVFXSequencePlayer is missing.",
                    this
                );

                return;
            }


            SparkVFXEventType eventType;


            // --------------------------------------------------------
            // CONVERT OVERRIDE → EVENT
            // --------------------------------------------------------

            switch (type)
            {
                case SparkVFXOverrideType.Press:

                    eventType =
                        SparkVFXEventType.Press;

                    break;


                case SparkVFXOverrideType.Success:

                    eventType =
                        SparkVFXEventType.Success;

                    break;


                case SparkVFXOverrideType.Error:

                    eventType =
                        SparkVFXEventType.Error;

                    break;


                case SparkVFXOverrideType.Warning:

                    eventType =
                        SparkVFXEventType.Warning;

                    break;


                case SparkVFXOverrideType.Confirm:

                    eventType =
                        SparkVFXEventType.Confirm;

                    break;


                case SparkVFXOverrideType.Cancel:

                    eventType =
                        SparkVFXEventType.Cancel;

                    break;


                case SparkVFXOverrideType.Unlock:

                    eventType =
                        SparkVFXEventType.Unlock;

                    break;


                case SparkVFXOverrideType.LevelComplete:

                    eventType =
                        SparkVFXEventType.LevelComplete;

                    break;


                case SparkVFXOverrideType.Notification:

                    eventType =
                        SparkVFXEventType.Notification;

                    break;


                case SparkVFXOverrideType.Alert:

                    eventType =
                        SparkVFXEventType.Alert;

                    break;


                default:

                    return;
            }


            // --------------------------------------------------------
            // RESOLVE FEEDBACK SEQUENCE
            // --------------------------------------------------------

            SparkVFXSequence sequence =
                runtime.ResolveFeedbackSequence(
                    eventType
                );


            if (sequence == null)
            {
                Debug.LogWarning(
                    "[SparkVFXLayeredStateMachine] " +
                    "No feedback sequence configured for override: " +
                    type,
                    this
                );

                return;
            }


            // --------------------------------------------------------
            // OVERRIDE OWNS THE SEQUENCE PLAYER
            // --------------------------------------------------------

            activeSequence =
                sequence;


            sequencePlayer.SetLoop(
                false
            );


            sequencePlayer.SetSequence(
                sequence
            );


            sequencePlayer.Restart();
        }


        // ============================================================
        // RESTORE LAYERS
        // ============================================================

        private void RestoreLayers()
        {
            if (!EnsureReady())
            {
                return;
            }


            // --------------------------------------------------------
            // OVERRIDE
            // --------------------------------------------------------

            if (HasOverride)
            {
                return;
            }


            // --------------------------------------------------------
            // SEQUENCE
            // --------------------------------------------------------

            if (
                activeSequence != null &&
                sequencePlayer != null &&
                sequencePlayer.IsPlaying
            )
            {
                return;
            }


            // --------------------------------------------------------
            // CLEAR STALE SEQUENCE
            // --------------------------------------------------------

            activeSequence =
                null;


            // --------------------------------------------------------
            // BASE STATE
            // --------------------------------------------------------

            ApplyBaseState();


            // --------------------------------------------------------
            // LOOP
            // --------------------------------------------------------

            if (
                loopType !=
                SparkVFXLoopType.None
            )
            {
                ApplyLoopLayer();
            }
            else
            {
                StopLoop();
            }
        }


        // ============================================================
        // STOP LOOP
        // ============================================================

        private void StopLoop()
        {
            if (loop == null)
            {
                return;
            }


            loop.Stop();
        }


        // ============================================================
        // CLEAR OVERRIDE
        // ============================================================

        public void ClearOverride()
        {
            if (
                overrideRoutine !=
                null
            )
            {
                StopCoroutine(
                    overrideRoutine
                );
            }


            overrideRoutine =
                null;


            overrideType =
                SparkVFXOverrideType.None;


            RestoreLayers();
        }


        // ============================================================
        // RESET STATE
        // ============================================================

        public void ResetState()
        {
            if (!EnsureReady())
            {
                return;
            }


            if (
                overrideRoutine !=
                null
            )
            {
                StopCoroutine(
                    overrideRoutine
                );
            }


            overrideRoutine =
                null;


            overrideType =
                SparkVFXOverrideType.None;


            if (sequencePlayer != null)
            {
                sequencePlayer.Stop();
            }


            activeSequence =
                null;


            loopType =
                SparkVFXLoopType.None;


            StopLoop();


            baseState =
                SparkVFXBaseState.Normal;


            currentStateID =
                null;


            controller.ResetVFX();


            ApplyBaseState();
        }


        // ============================================================
        // REFRESH RESOLUTION
        // ============================================================

        public void Refresh()
        {
            UnsubscribeFromSequencePlayer();


            if (
                overrideRoutine !=
                null
            )
            {
                StopCoroutine(
                    overrideRoutine
                );
            }


            if (sequencePlayer != null)
            {
                sequencePlayer.Stop();
            }


            overrideRoutine =
                null;


            overrideType =
                SparkVFXOverrideType.None;


            activeSequence =
                null;


            controller =
                null;


            target =
                null;


            runtime =
                null;


            loop =
                null;


            sequencePlayer =
                null;


            stateResolver =
                null;


            initialized =
                false;


            resolving =
                false;


            Initialize();
        }


        // ============================================================
        // DISABLE
        // ============================================================

        private void OnDisable()
        {
            UnsubscribeFromSequencePlayer();


            if (
                overrideRoutine !=
                null
            )
            {
                StopCoroutine(
                    overrideRoutine
                );
            }


            overrideRoutine =
                null;


            overrideType =
                SparkVFXOverrideType.None;


            if (sequencePlayer != null)
            {
                sequencePlayer.Stop();
            }


            activeSequence =
                null;


            StopLoop();
        }


        // ============================================================
        // DESTROY
        // ============================================================

        private void OnDestroy()
        {
            UnsubscribeFromSequencePlayer();
        }


        // ============================================================
        // VALIDATION
        // ============================================================

        private void OnValidate()
        {
            defaultOverrideDuration =
                Mathf.Max(
                    0f,
                    defaultOverrideDuration
                );


            if (target == null)
            {
                target =
                    GetComponent<
                        SparkVFXTarget
                    >();
            }


            if (runtime == null)
            {
                runtime =
                    GetComponent<
                        SparkVFXRuntime
                    >();
            }


            if (loop == null)
            {
                loop =
                    GetComponent<
                        SparkVFXLoop
                    >();
            }


            if (sequencePlayer == null)
            {
                sequencePlayer =
                    GetComponent<
                        SparkVFXSequencePlayer
                    >();
            }


            if (stateResolver == null)
            {
                stateResolver =
                    GetComponent<
                        SparkVFXStateResolver
                    >();
            }
        }


        // ============================================================
        // REQUEST STATE
        // ============================================================

        /// <summary>
        /// Requests a logical VFX state by ID.
        ///
        /// The state is resolved through SparkVFXStateResolver.
        /// The resolved profile is applied to the active
        /// VFX controller.
        /// </summary>
        public bool RequestState(
            string stateID)
        {
            return RequestState(
                stateID,
                false
            );
        }


        // ============================================================
        // REQUEST STATE
        // ============================================================

        public bool RequestState(
            string stateID,
            bool instant)
        {
            if (!EnsureReady())
            {
                return false;
            }


            ResolveStateResolver();


            if (stateResolver == null)
            {
                Debug.LogError(
                    "[SparkVFXLayeredStateMachine] " +
                    "No SparkVFXStateResolver found.",
                    this
                );


                return false;
            }


            if (
                string.IsNullOrWhiteSpace(
                    stateID
                )
            )
            {
                Debug.LogWarning(
                    "[SparkVFXLayeredStateMachine] " +
                    "RequestState received an empty state ID.",
                    this
                );


                return false;
            }


            SparkVFXStateDefinition state =
                stateResolver.ResolveState(
                    stateID
                );


            if (state == null)
            {
                Debug.LogWarning(
                    "[SparkVFXLayeredStateMachine] " +
                    "Unable to resolve state: " +
                    stateID,
                    this
                );


                return false;
            }


            SparkVFXProfile profile =
                stateResolver.ResolveProfile(
                    stateID
                );


            if (profile == null)
            {
                Debug.LogWarning(
                    "[SparkVFXLayeredStateMachine] " +
                    "State '" +
                    stateID +
                    "' has no resolvable VFX profile.",
                    this
                );


                return false;
            }


            // --------------------------------------------------------
            // APPLY PROFILE
            // --------------------------------------------------------

            controller.ApplyProfile(
                profile,
                controller.CurrentThemeColor,
                instant
            );


            // --------------------------------------------------------
            // UPDATE CURRENT STATE ID
            // --------------------------------------------------------

            currentStateID =
                state.GetNormalizedID();


            return true;
        }


        // ============================================================
        // PLAY FEEDBACK
        // ============================================================

        public bool PlayFeedback(
            SparkVFXEventType eventType)
        {
            if (!EnsureReady())
            {
                return false;
            }


            if (runtime == null)
            {
                Debug.LogWarning(
                    "[SparkVFXLayeredStateMachine] " +
                    "SparkVFXRuntime is missing.",
                    this
                );


                return false;
            }


            // --------------------------------------------------------
            // CONVERT EVENT TO OVERRIDE TYPE
            // --------------------------------------------------------

            SparkVFXOverrideType convertedOverrideType =
                ConvertEventToOverrideType(
                    eventType
                );


            // --------------------------------------------------------
            // INVALID / UNSUPPORTED EVENT
            // --------------------------------------------------------

            if (
                convertedOverrideType ==
                SparkVFXOverrideType.None
            )
            {
                Debug.LogWarning(
                    "[SparkVFXLayeredStateMachine] " +
                    "Event cannot be used as a feedback override: " +
                    eventType,
                    this
                );


                return false;
            }


            // --------------------------------------------------------
            // PLAY AS OVERRIDE
            // --------------------------------------------------------

            PlayOverride(
                convertedOverrideType
            );


            return true;
        }


        // ============================================================
        // EVENT → OVERRIDE TYPE
        // ============================================================

        private SparkVFXOverrideType ConvertEventToOverrideType(
            SparkVFXEventType eventType)
        {
            switch (eventType)
            {
                case SparkVFXEventType.Press:

                    return SparkVFXOverrideType.Press;


                case SparkVFXEventType.Submit:

                    return SparkVFXOverrideType.Press;


                case SparkVFXEventType.Success:

                    return SparkVFXOverrideType.Success;


                case SparkVFXEventType.Error:

                    return SparkVFXOverrideType.Error;


                case SparkVFXEventType.Warning:

                    return SparkVFXOverrideType.Warning;


                case SparkVFXEventType.Confirm:

                    return SparkVFXOverrideType.Confirm;


                case SparkVFXEventType.Cancel:

                    return SparkVFXOverrideType.Cancel;


                case SparkVFXEventType.Unlock:

                    return SparkVFXOverrideType.Unlock;


                case SparkVFXEventType.LevelComplete:

                    return SparkVFXOverrideType.LevelComplete;


                case SparkVFXEventType.Notification:

                    return SparkVFXOverrideType.Notification;


                case SparkVFXEventType.Alert:

                    return SparkVFXOverrideType.Alert;


                default:

                    return SparkVFXOverrideType.None;
            }
        }


        // ============================================================
        // PLAY SEQUENCE AS OVERRIDE
        // ============================================================

        private void PlaySequenceAsOverride(
            SparkVFXSequence newSequence)
        {
            if (newSequence == null)
            {
                Debug.LogWarning(
                    "[SparkVFXLayeredStateMachine] " +
                    "Cannot play a null override sequence.",
                    this
                );

                return;
            }


            if (sequencePlayer == null)
            {
                Debug.LogWarning(
                    "[SparkVFXLayeredStateMachine] " +
                    "Sequence Player is missing.",
                    this
                );

                return;
            }


            // --------------------------------------------------------
            // STOP LOWER PRIORITY LAYERS
            // --------------------------------------------------------

            StopLoop();


            if (sequencePlayer.IsPlaying)
            {
                sequencePlayer.Stop();
            }


            // --------------------------------------------------------
            // SET ACTIVE SEQUENCE
            // --------------------------------------------------------

            activeSequence =
                newSequence;


            // --------------------------------------------------------
            // CONFIGURE PLAYER
            // --------------------------------------------------------

            sequencePlayer.SetLoop(
                false
            );


            sequencePlayer.SetSequence(
                newSequence
            );


            // --------------------------------------------------------
            // PLAY
            // --------------------------------------------------------

            sequencePlayer.Restart();
        }
    }
}