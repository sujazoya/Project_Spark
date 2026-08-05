
using UnityEngine;

namespace ProjectSpark.UI.VFX
{
    /// <summary>
    /// Runtime execution and asset-resolution service for Spark VFX.
    ///
    /// Responsibilities:
    /// - Profile lookup and application.
    /// - Theme color management.
    /// - Feedback sequence resolution.
    /// - Compatibility event routing.
    ///
    /// Layer priority and sequence playback are owned by:
    /// SparkVFXLayeredStateMachine.
    ///
    /// SparkVFXRuntime does NOT directly control:
    /// - SparkVFXSequencePlayer
    /// - SparkVFXLoop
    /// - Layer priority
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(SparkVFXController))]
    public sealed class SparkVFXRuntime :
        MonoBehaviour
    {
        // ============================================================
        // REFERENCES
        // ============================================================
        [SerializeField]
        private SparkVFXTarget target;

        private ISparkVFXController controller;

        [Header("References")]

       // [SerializeField]
       // private SparkVFXController controller;


        [SerializeField]
        private SparkVFXProfileLibrary profileLibrary;


        [SerializeField]
        private SparkVFXLayeredStateMachine layeredStateMachine;


        // ============================================================
        // THEME
        // ============================================================

        [Header("Theme")]

        [SerializeField]
        private Color themeColor =
            Color.white;


        // ============================================================
        // FEEDBACK SEQUENCES
        // ============================================================

        [Header("Feedback Sequences")]

        [SerializeField]
        private SparkVFXSequence showSequence;


        [SerializeField]
        private SparkVFXSequence hideSequence;


        [SerializeField]
        private SparkVFXSequence pressSequence;


        [SerializeField]
        private SparkVFXSequence successSequence;


        [SerializeField]
        private SparkVFXSequence errorSequence;


        [SerializeField]
        private SparkVFXSequence warningSequence;


        [SerializeField]
        private SparkVFXSequence confirmSequence;


        [SerializeField]
        private SparkVFXSequence cancelSequence;


        [SerializeField]
        private SparkVFXSequence unlockSequence;


        [SerializeField]
        private SparkVFXSequence levelCompleteSequence;


        [SerializeField]
        private SparkVFXSequence notificationSequence;


        [SerializeField]
        private SparkVFXSequence alertSequence;


        // ============================================================
        // CURRENT PROFILE
        // ============================================================

        private SparkVFXProfile currentProfile;


        // ============================================================
        // PROPERTIES
        // ============================================================

        public ISparkVFXController Controller
        {
            get
            {
                return controller;
            }
        }


        public SparkVFXProfile CurrentProfile
        {
            get
            {
                return currentProfile;
            }
        }


        public Color CurrentThemeColor
        {
            get
            {
                return themeColor;
            }
        }


        public SparkVFXSequence ShowSequence
        {
            get
            {
                return showSequence;
            }
        }


        public SparkVFXSequence HideSequence
        {
            get
            {
                return hideSequence;
            }
        }


        public SparkVFXSequence PressSequence
        {
            get
            {
                return pressSequence;
            }
        }


        public SparkVFXSequence SuccessSequence
        {
            get
            {
                return successSequence;
            }
        }


        public SparkVFXSequence ErrorSequence
        {
            get
            {
                return errorSequence;
            }
        }


        public SparkVFXSequence WarningSequence
        {
            get
            {
                return warningSequence;
            }
        }


        public SparkVFXSequence ConfirmSequence
        {
            get
            {
                return confirmSequence;
            }
        }


        public SparkVFXSequence CancelSequence
        {
            get
            {
                return cancelSequence;
            }
        }


        public SparkVFXSequence UnlockSequence
        {
            get
            {
                return unlockSequence;
            }
        }


        public SparkVFXSequence LevelCompleteSequence
        {
            get
            {
                return levelCompleteSequence;
            }
        }


        public SparkVFXSequence NotificationSequence
        {
            get
            {
                return notificationSequence;
            }
        }


        public SparkVFXSequence AlertSequence
        {
            get
            {
                return alertSequence;
            }
        }


        // ============================================================
        // UNITY
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
            if (controller == null)
            {
                controller =
                    GetComponent<
                        SparkVFXController
                    >();
            }


            if (layeredStateMachine == null)
            {
                layeredStateMachine =
                    GetComponent<
                        SparkVFXLayeredStateMachine
                    >();
            }


            if (controller == null)
            {
                Debug.LogError(
                    "[SparkVFXRuntime] " +
                    "SparkVFXController is missing.",
                    this
                );


                return;
            }


            controller.Initialize();


            controller.ApplyThemeColor(
                themeColor
            );
        }


        // ============================================================
        // THEME
        // ============================================================

        public void SetThemeColor(
            Color color)
        {
            themeColor =
                color;


            if (controller == null)
            {
                return;
            }


            controller.ApplyThemeColor(
                color
            );
        }


        // ============================================================
        // PROFILE
        // ============================================================

        public void PlayProfile(
            SparkVFXEventType eventType)
        {
            if (profileLibrary == null)
            {
                Debug.LogWarning(
                    "[SparkVFXRuntime] " +
                    "Profile Library is not assigned.",
                    this
                );


                return;
            }


            if (controller == null)
            {
                return;
            }


            SparkVFXProfile profile =
                profileLibrary.GetProfile(
                    eventType
                );


            if (profile == null)
            {
                Debug.LogWarning(
                    "[SparkVFXRuntime] " +
                    "No profile found for event: " +
                    eventType,
                    this
                );


                return;
            }


            currentProfile =
                profile;


            controller.ApplyProfile(
                profile,
                themeColor
            );
        }


        // ============================================================
        // PROFILE INSTANT
        // ============================================================

        public void PlayProfileInstant(
            SparkVFXEventType eventType)
        {
            if (profileLibrary == null)
            {
                Debug.LogWarning(
                    "[SparkVFXRuntime] " +
                    "Profile Library is not assigned.",
                    this
                );


                return;
            }


            if (controller == null)
            {
                return;
            }


            SparkVFXProfileLibrary.ProfileState profileState;


            if (
                !TryConvertEventToProfileState(
                    eventType,
                    out profileState
                )
            )
            {
                Debug.LogWarning(
                    "[SparkVFXRuntime] " +
                    "No ProfileState mapping exists for event: " +
                    eventType,
                    this
                );


                return;
            }


            SparkVFXProfile profile =
                profileLibrary.GetProfile(
                    profileState
                );


            if (profile == null)
            {
                Debug.LogWarning(
                    "[SparkVFXRuntime] " +
                    "No profile found for event: " +
                    eventType +
                    " / ProfileState: " +
                    profileState,
                    this
                );


                return;
            }


            currentProfile =
                profile;


            controller.ApplyProfile(
                profile,
                themeColor,
                true
            );
        }


        // ============================================================
        // FEEDBACK RESOLUTION
        // ============================================================

        public SparkVFXSequence ResolveFeedbackSequence(
            SparkVFXEventType eventType)
        {
            switch (eventType)
            {
                case SparkVFXEventType.Press:

                    return pressSequence;


                case SparkVFXEventType.Submit:

                    return pressSequence;


                case SparkVFXEventType.Success:

                    return successSequence;


                case SparkVFXEventType.Error:

                    return errorSequence;


                case SparkVFXEventType.Warning:

                    return warningSequence;


                case SparkVFXEventType.Confirm:

                    return confirmSequence;


                case SparkVFXEventType.Cancel:

                    return cancelSequence;


                case SparkVFXEventType.Unlock:

                    return unlockSequence;


                case SparkVFXEventType.LevelComplete:

                    return levelCompleteSequence;


                case SparkVFXEventType.Notification:

                    return notificationSequence;


                case SparkVFXEventType.Alert:

                    return alertSequence;


                default:

                    return null;
            }
        }


        // ============================================================
        // FEEDBACK EXECUTION
        // ============================================================

        private void ExecuteFeedback(
            SparkVFXEventType eventType)
        {
            if (layeredStateMachine == null)
            {
                layeredStateMachine =
                    GetComponent<
                        SparkVFXLayeredStateMachine
                    >();
            }


            if (layeredStateMachine == null)
            {
                Debug.LogWarning(
                    "[SparkVFXRuntime] " +
                    "SparkVFXLayeredStateMachine is missing. " +
                    "Feedback cannot be executed.",
                    this
                );


                return;
            }


            layeredStateMachine.PlayFeedback(
                eventType
            );
        }


        // ============================================================
        // FEEDBACK METHODS
        // ============================================================

        public void Show()
        {
            if (layeredStateMachine == null)
            {
                return;
            }

            layeredStateMachine.Show();
        }


        public void Hide()
        {
            if (layeredStateMachine == null)
            {
                return;
            }

            layeredStateMachine.Hide();
        }

        public void Press()
        {
            ExecuteFeedback(
                SparkVFXEventType.Press
            );
        }


        public void Success()
        {
            ExecuteFeedback(
                SparkVFXEventType.Success
            );
        }


        public void Error()
        {
            ExecuteFeedback(
                SparkVFXEventType.Error
            );
        }


        public void Warning()
        {
            ExecuteFeedback(
                SparkVFXEventType.Warning
            );
        }


        public void Confirm()
        {
            ExecuteFeedback(
                SparkVFXEventType.Confirm
            );
        }


        public void Cancel()
        {
            ExecuteFeedback(
                SparkVFXEventType.Cancel
            );
        }


        public void Unlock()
        {
            ExecuteFeedback(
                SparkVFXEventType.Unlock
            );
        }


        public void LevelComplete()
        {
            ExecuteFeedback(
                SparkVFXEventType.LevelComplete
            );
        }


        public void Notification()
        {
            ExecuteFeedback(
                SparkVFXEventType.Notification
            );
        }


        public void Alert()
        {
            ExecuteFeedback(
                SparkVFXEventType.Alert
            );
        }


        // ============================================================
        // GENERIC EVENT ROUTER
        // ============================================================

        public void PlayEvent(
            SparkVFXEventType eventType)
        {
            switch (eventType)
            {
                case SparkVFXEventType.Normal:

                case SparkVFXEventType.HoverEnter:

                case SparkVFXEventType.Selected:

                case SparkVFXEventType.Target:

                case SparkVFXEventType.Disabled:

                case SparkVFXEventType.Locked:

                    PlayProfile(
                        eventType
                    );

                    break;


                case SparkVFXEventType.HoverExit:

                case SparkVFXEventType.Release:

                    PlayProfile(
                        SparkVFXEventType.Normal
                    );

                    break;


                case SparkVFXEventType.Press:

                case SparkVFXEventType.Submit:

                case SparkVFXEventType.Success:

                case SparkVFXEventType.Error:

                case SparkVFXEventType.Warning:

                case SparkVFXEventType.Confirm:

                case SparkVFXEventType.Cancel:

                case SparkVFXEventType.Unlock:

                case SparkVFXEventType.LevelComplete:

                case SparkVFXEventType.Notification:

                case SparkVFXEventType.Alert:

                    ExecuteFeedback(
                        eventType
                    );

                    break;


                default:

                    Debug.LogWarning(
                        "[SparkVFXRuntime] " +
                        "Unhandled VFX event: " +
                        eventType,
                        this
                    );

                    break;
            }
        }


        // ============================================================
        // STRING PROFILE
        // ============================================================

        public void PlayProfile(
            string eventName)
        {
            if (
                !System.Enum.TryParse(
                    eventName,
                    true,
                    out SparkVFXEventType eventType
                )
            )
            {
                Debug.LogWarning(
                    "[SparkVFXRuntime] " +
                    "Unknown event: " +
                    eventName,
                    this
                );


                return;
            }


            PlayProfile(
                eventType
            );
        }


        // ============================================================
        // EVENT → PROFILE STATE
        // ============================================================

        private bool TryConvertEventToProfileState(
            SparkVFXEventType eventType,
            out SparkVFXProfileLibrary.ProfileState profileState)
        {
            profileState =
                SparkVFXProfileLibrary.ProfileState.Normal;


            switch (eventType)
            {
                case SparkVFXEventType.Normal:

                    profileState =
                        SparkVFXProfileLibrary.ProfileState.Normal;

                    return true;


                case SparkVFXEventType.HoverEnter:

                    profileState =
                        SparkVFXProfileLibrary.ProfileState.Hover;

                    return true;


                case SparkVFXEventType.Selected:

                    profileState =
                        SparkVFXProfileLibrary.ProfileState.Selected;

                    return true;


                case SparkVFXEventType.Target:

                    profileState =
                        SparkVFXProfileLibrary.ProfileState.Target;

                    return true;


                case SparkVFXEventType.Warning:

                    profileState =
                        SparkVFXProfileLibrary.ProfileState.Warning;

                    return true;


                default:

                    return false;
            }
        }
    }
}

