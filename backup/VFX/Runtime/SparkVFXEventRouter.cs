using UnityEngine;

namespace ProjectSpark.UI.VFX
{
    /// <summary>
    /// Central event router for Project Spark UI VFX.
    ///
    /// Responsibilities:
    /// - Receives SparkVFXEventType requests.
    /// - Routes events to SparkVFXRuntime.
    /// - Supports normal playback.
    /// - Supports instant playback.
    /// - Provides strongly-typed helper methods for all
    ///   SparkVFXEventType values.
    ///
    /// Does NOT:
    /// - Resolve profiles directly.
    /// - Modify materials.
    /// - Control ISparkVFXController directly.
    /// - Manage layered state priority.
    /// - Replace SparkVFXLayeredStateMachine.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class SparkVFXEventRouter
        : MonoBehaviour
    {
        // ============================================================
        // RUNTIME
        // ============================================================

        [Header("Runtime")]

        [Tooltip(
            "SparkVFXRuntime that receives routed VFX events."
        )]
        [SerializeField]
        private SparkVFXRuntime runtime;


        // ============================================================
        // AUTO FIND
        // ============================================================

        [Header("Auto Find")]

        [Tooltip(
            "Automatically searches this object, children, " +
            "and parents for SparkVFXRuntime."
        )]
        [SerializeField]
        private bool autoFindRuntime = true;


        // ============================================================
        // DEBUG
        // ============================================================

        [Header("Debug")]

        [Tooltip(
            "Logs routed events to the Unity Console."
        )]
        [SerializeField]
        private bool logEvents;


        // ============================================================
        // INITIALIZATION
        // ============================================================

        private void Awake()
        {
            ResolveRuntime();
        }


        private void OnEnable()
        {
            ResolveRuntime();
        }


        // ============================================================
        // RUNTIME PROPERTY
        // ============================================================

        public SparkVFXRuntime Runtime
        {
            get
            {
                ResolveRuntime();

                return runtime;
            }
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


            if (!autoFindRuntime)
            {
                return;
            }


            // --------------------------------------------------------
            // SAME OBJECT
            // --------------------------------------------------------

            runtime =
                GetComponent<
                    SparkVFXRuntime
                >();


            if (runtime != null)
            {
                return;
            }


            // --------------------------------------------------------
            // CHILDREN
            // --------------------------------------------------------

            runtime =
                GetComponentInChildren<
                    SparkVFXRuntime
                >(
                    true
                );


            if (runtime != null)
            {
                return;
            }


            // --------------------------------------------------------
            // PARENT
            // --------------------------------------------------------

            runtime =
                GetComponentInParent<
                    SparkVFXRuntime
                >();
        }


        // ============================================================
        // SEND EVENT
        // ============================================================

        /// <summary>
        /// Routes a VFX event using normal profile playback.
        /// </summary>
        public void SendEvent(
            SparkVFXEventType eventType)
        {
            SendEvent(
                eventType,
                false
            );
        }


        // ============================================================
        // SEND EVENT
        // ============================================================

        /// <summary>
        /// Routes a VFX event.
        ///
        /// If instant is true, the runtime uses instant playback.
        /// Otherwise it uses normal playback.
        /// </summary>
        public void SendEvent(
            SparkVFXEventType eventType,
            bool instant)
        {
            ResolveRuntime();


            if (runtime == null)
            {
                Debug.LogWarning(
                    "[SparkVFXEventRouter] " +
                    "SparkVFXRuntime is not assigned " +
                    "and could not be found.",
                    this
                );

                return;
            }


            if (
                eventType ==
                SparkVFXEventType.None
            )
            {
                if (logEvents)
                {
                    Debug.Log(
                        "[SparkVFXEventRouter] " +
                        "Ignoring None event.",
                        this
                    );
                }

                return;
            }


            if (logEvents)
            {
                Debug.Log(
                    "[SparkVFXEventRouter] " +
                    "Routing event: " +
                    eventType +
                    " | Instant: " +
                    instant,
                    this
                );
            }


            if (instant)
            {
                runtime.PlayProfileInstant(
                    eventType
                );

                return;
            }


            runtime.PlayProfile(
                eventType
            );
        }


        // ============================================================
        // NORMAL PLAYBACK
        // ============================================================

        public void Play(
            SparkVFXEventType eventType)
        {
            SendEvent(
                eventType,
                false
            );
        }


        // ============================================================
        // INSTANT PLAYBACK
        // ============================================================

        public void PlayInstant(
            SparkVFXEventType eventType)
        {
            SendEvent(
                eventType,
                true
            );
        }


        // ============================================================
        // BASE STATES
        // ============================================================

        public void PlayNormal()
        {
            Play(
                SparkVFXEventType.Normal
            );
        }


        public void PlayHoverEnter()
        {
            Play(
                SparkVFXEventType.HoverEnter
            );
        }


        public void PlayHoverExit()
        {
            Play(
                SparkVFXEventType.HoverExit
            );
        }


        public void PlaySelected()
        {
            Play(
                SparkVFXEventType.Selected
            );
        }


        public void PlayTarget()
        {
            Play(
                SparkVFXEventType.Target
            );
        }


        public void PlayDisabled()
        {
            Play(
                SparkVFXEventType.Disabled
            );
        }


        public void PlayLocked()
        {
            Play(
                SparkVFXEventType.Locked
            );
        }


        // ============================================================
        // INTERACTION
        // ============================================================

        public void PlayPress()
        {
            Play(
                SparkVFXEventType.Press
            );
        }


        public void PlayRelease()
        {
            Play(
                SparkVFXEventType.Release
            );
        }


        public void PlaySubmit()
        {
            Play(
                SparkVFXEventType.Submit
            );
        }


        // ============================================================
        // FEEDBACK
        // ============================================================

        public void PlaySuccess()
        {
            Play(
                SparkVFXEventType.Success
            );
        }


        public void PlayError()
        {
            Play(
                SparkVFXEventType.Error
            );
        }


        public void PlayWarning()
        {
            Play(
                SparkVFXEventType.Warning
            );
        }


        public void PlayConfirm()
        {
            Play(
                SparkVFXEventType.Confirm
            );
        }


        public void PlayCancel()
        {
            Play(
                SparkVFXEventType.Cancel
            );
        }


        // ============================================================
        // SYSTEM
        // ============================================================

        public void PlayUnlock()
        {
            Play(
                SparkVFXEventType.Unlock
            );
        }


        public void PlayLevelComplete()
        {
            Play(
                SparkVFXEventType.LevelComplete
            );
        }


        public void PlayNotification()
        {
            Play(
                SparkVFXEventType.Notification
            );
        }


        public void PlayAlert()
        {
            Play(
                SparkVFXEventType.Alert
            );
        }


        // ============================================================
        // INSTANT BASE STATES
        // ============================================================

        public void PlayNormalInstant()
        {
            PlayInstant(
                SparkVFXEventType.Normal
            );
        }


        public void PlayHoverEnterInstant()
        {
            PlayInstant(
                SparkVFXEventType.HoverEnter
            );
        }


        public void PlayHoverExitInstant()
        {
            PlayInstant(
                SparkVFXEventType.HoverExit
            );
        }


        public void PlaySelectedInstant()
        {
            PlayInstant(
                SparkVFXEventType.Selected
            );
        }


        public void PlayTargetInstant()
        {
            PlayInstant(
                SparkVFXEventType.Target
            );
        }


        public void PlayDisabledInstant()
        {
            PlayInstant(
                SparkVFXEventType.Disabled
            );
        }


        public void PlayLockedInstant()
        {
            PlayInstant(
                SparkVFXEventType.Locked
            );
        }


        // ============================================================
        // INSTANT INTERACTION
        // ============================================================

        public void PlayPressInstant()
        {
            PlayInstant(
                SparkVFXEventType.Press
            );
        }


        public void PlayReleaseInstant()
        {
            PlayInstant(
                SparkVFXEventType.Release
            );
        }


        public void PlaySubmitInstant()
        {
            PlayInstant(
                SparkVFXEventType.Submit
            );
        }


        // ============================================================
        // INSTANT FEEDBACK
        // ============================================================

        public void PlaySuccessInstant()
        {
            PlayInstant(
                SparkVFXEventType.Success
            );
        }


        public void PlayErrorInstant()
        {
            PlayInstant(
                SparkVFXEventType.Error
            );
        }


        public void PlayWarningInstant()
        {
            PlayInstant(
                SparkVFXEventType.Warning
            );
        }


        public void PlayConfirmInstant()
        {
            PlayInstant(
                SparkVFXEventType.Confirm
            );
        }


        public void PlayCancelInstant()
        {
            PlayInstant(
                SparkVFXEventType.Cancel
            );
        }


        // ============================================================
        // INSTANT SYSTEM
        // ============================================================

        public void PlayUnlockInstant()
        {
            PlayInstant(
                SparkVFXEventType.Unlock
            );
        }


        public void PlayLevelCompleteInstant()
        {
            PlayInstant(
                SparkVFXEventType.LevelComplete
            );
        }


        public void PlayNotificationInstant()
        {
            PlayInstant(
                SparkVFXEventType.Notification
            );
        }


        public void PlayAlertInstant()
        {
            PlayInstant(
                SparkVFXEventType.Alert
            );
        }


        // ============================================================
        // VALIDATION
        // ============================================================

        public bool HasRuntime()
        {
            ResolveRuntime();

            return runtime != null;
        }


        public bool Validate(
            bool logWarning = true)
        {
            ResolveRuntime();


            if (runtime != null)
            {
                return true;
            }


            if (logWarning)
            {
                Debug.LogWarning(
                    "[SparkVFXEventRouter] " +
                    "Validation failed. " +
                    "No SparkVFXRuntime is assigned or found.",
                    this
                );
            }


            return false;
        }


        // ============================================================
        // EDITOR
        // ============================================================

#if UNITY_EDITOR

        [ContextMenu(
            "Validate Event Router"
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
                    "[SparkVFXEventRouter] " +
                    "Validation successful.",
                    this
                );
            }
            else
            {
                Debug.LogError(
                    "[SparkVFXEventRouter] " +
                    "Validation failed.",
                    this
                );
            }
        }


        [ContextMenu(
            "Find SparkVFXRuntime"
        )]
        private void FindRuntimeFromContextMenu()
        {
            ResolveRuntime();


            if (runtime != null)
            {
                Debug.Log(
                    "[SparkVFXEventRouter] " +
                    "SparkVFXRuntime found: " +
                    runtime.name,
                    this
                );
            }
            else
            {
                Debug.LogWarning(
                    "[SparkVFXEventRouter] " +
                    "SparkVFXRuntime was not found.",
                    this
                );
            }
        }

#endif
    }
}