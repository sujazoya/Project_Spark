using UnityEngine;

namespace ProjectSpark.AI
{
    /// <summary>
    /// Starts the adaptive teaching session after the active lesson
    /// has been selected.
    ///
    /// This component does not evaluate gameplay and does not modify
    /// the circuit. It only asks the existing adaptive teaching
    /// controller to build and apply a teaching plan.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class SparkAILessonTeachingBridge : MonoBehaviour
    {
        [Header("References")]
        [SerializeField]
        private SparkAILesson lesson;

        [SerializeField]
        private SparkAIAdaptiveTeachingController adaptiveTeachingController;

        [Header("Behaviour")]
        [SerializeField]
        private bool startTeachingOnEnable = false;

        [SerializeField]
        private bool requireActiveLesson = true;

        private bool initialized;

        public bool IsInitialized =>
            initialized;

        private void Awake()
        {
            initialized = false;

            if (lesson == null)
            {
                Debug.LogError(
                    "[AI LESSON TEACHING] " +
                    "SparkAILesson is not assigned.",
                    this);

                return;
            }

            if (adaptiveTeachingController == null)
            {
                Debug.LogError(
                    "[AI LESSON TEACHING] " +
                    "SparkAIAdaptiveTeachingController " +
                    "is not assigned.",
                    this);

                return;
            }

            initialized = true;
        }

        private void Start()
        {
            if (!initialized)
                return;

            if (!startTeachingOnEnable)
                return;

            BeginTeaching();
        }

        /// <summary>
        /// Builds the current adaptive teaching plan and
        /// applies it to SparkAITeachingSession.
        /// </summary>
        public bool BeginTeaching()
        {
            if (!initialized)
                return false;

            if (requireActiveLesson &&
                !lesson.HasActiveLesson)
            {
                Debug.LogWarning(
                    "[AI LESSON TEACHING] " +
                    "No active lesson is available.",
                    this);

                return false;
            }

            return adaptiveTeachingController.RefreshAndApply();
        }
    }
}