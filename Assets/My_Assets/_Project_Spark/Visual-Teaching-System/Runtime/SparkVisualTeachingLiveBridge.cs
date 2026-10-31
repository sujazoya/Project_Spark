using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectSpark.UI.VisualTeaching
{
    /// <summary>
    /// Receives already-verified game facts and presents temporary 3D teaching cues.
    /// Does not read/modify circuit state, infer faults, or replace an active lesson.
    /// Works with ProjectSparkVisualTeachingAAA's existing manager and world renderer.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class SparkVisualTeachingLiveBridge : MonoBehaviour
    {
        [Serializable]
        public sealed class LevelLessonBinding
        {
            public string levelId;
            public SparkVisualLesson lesson;
        }

        [Serializable]
        public sealed class VerifiedFaultPreset
        {
            public string eventId;
            [Tooltip("ID of SparkTeachingAnchor on the actual scene component.")]
            public string anchorId;
            [TextArea(2, 4)] public string verifiedExplanation;
        }

        [Header("AAA Visual Teaching System")]
        [SerializeField] private SparkVisualTeachingManager teacher;
        [SerializeField] private SparkWorldAnnotationRenderer worldAnnotations;

        [Header("Lessons (explicit level-ID mapping)")]
        [SerializeField] private LevelLessonBinding[] levelLessons = Array.Empty<LevelLessonBinding>();
        [SerializeField] private bool autoStartMappedLessonOnLevelLoaded = true;
        [SerializeField] private bool closeBoardOnLevelUnload = true;

        [Header("Trusted diagnostic event presets")]
        [SerializeField] private VerifiedFaultPreset[] verifiedFaultPresets = Array.Empty<VerifiedFaultPreset>();

        [Header("Temporary annotations")]
        [SerializeField, Min(0.05f)] private float drawDuration = 0.35f;
        [SerializeField, Min(0f)] private float cueLifetime = 6f;
        [SerializeField] private bool automaticallyOpenBoard = true;
        [SerializeField] private bool logMissingAnchors = true;
        [SerializeField] private Color faultColor = new Color(1f, 0.29f, 0.27f, 1f);
        [SerializeField] private Color explanationColor = new Color(0.14f, 0.84f, 1f, 1f);
        [SerializeField] private Color connectionColor = new Color(0.96f, 0.78f, 0.23f, 1f);

        private readonly List<SparkWorldAnnotationRenderer.Annotation> activeCues =
            new List<SparkWorldAnnotationRenderer.Annotation>();
        private Coroutine activeAnimation;

        private void Awake() => Resolve();

        private void OnDisable() => ClearMyCues();

        private void Resolve()
        {
            if (teacher == null)
                teacher = FindFirstObjectByType<SparkVisualTeachingManager>(FindObjectsInactive.Include);
            if (worldAnnotations == null)
                worldAnnotations = FindFirstObjectByType<SparkWorldAnnotationRenderer>(FindObjectsInactive.Include);
        }

        /// <summary>
        /// Call from your level manager AFTER it has finished instantiating its level layout.
        /// Scene anchors should already be components on the level's prefabs.
        /// </summary>
        public bool OnLevelLoaded(string levelId)
        {
            Resolve();
            ClearMyCues();
            if (!autoStartMappedLessonOnLevelLoaded || teacher == null ||
                string.IsNullOrWhiteSpace(levelId)) return false;

            foreach (var binding in levelLessons)
            {
                if (binding == null || binding.lesson == null) continue;
                if (!string.Equals(binding.levelId, levelId, StringComparison.Ordinal)) continue;
                teacher.StartLesson(binding.lesson);
                return true;
            }
            return false;
        }

        public void OnLevelUnloaded()
        {
            ClearMyCues();
            if (closeBoardOnLevelUnload && teacher != null)
                teacher.ClosePanel();
        }

        // These methods are intended for verified game-controller callbacks, not direct AI output.
        public bool ReportVerifiedFault(string anchorId, string verifiedExplanation)
        {
            if (!ReadyWithAnchors(anchorId)) return false;
            ClearMyCues();
            AddCue(Ring(anchorId, faultColor));
            AddCue(Label(anchorId, verifiedExplanation, faultColor));
            return ShowCues();
        }

        public bool ReportVerifiedFaultPreset(string eventId)
        {
            if (string.IsNullOrWhiteSpace(eventId)) return false;
            foreach (var preset in verifiedFaultPresets)
            {
                if (preset == null || !string.Equals(preset.eventId, eventId, StringComparison.Ordinal)) continue;
                return ReportVerifiedFault(preset.anchorId, preset.verifiedExplanation);
            }
            Debug.LogWarning("[Spark Teaching Bridge] Unknown fault preset: " + eventId, this);
            return false;
        }

        public bool ExplainKnownComponent(string anchorId, string explanation)
        {
            if (!ReadyWithAnchors(anchorId)) return false;
            ClearMyCues();
            AddCue(Ring(anchorId, explanationColor));
            AddCue(Label(anchorId, explanation, explanationColor));
            return ShowCues();
        }

        public bool GuideVerifiedConnection(string sourceAnchorId, string targetAnchorId, string instruction)
        {
            if (!ReadyWithAnchors(sourceAnchorId, targetAnchorId)) return false;
            ClearMyCues();
            AddCue(Ring(sourceAnchorId, connectionColor));
            AddCue(Ring(targetAnchorId, new Color(.26f, 1f, .59f, 1f)));
            AddCue(new SparkVisualCommand {
                shape = SparkVisualShape.Arrow,
                from = Point(sourceAnchorId),
                to = Point(targetAnchorId),
                color = connectionColor,
                thickness = 5f,
                drawSeconds = drawDuration,
                persistent = true
            });
            AddCue(Label(targetAnchorId, instruction, explanationColor));
            return ShowCues();
        }

        /// <summary>Only call after a trusted gameplay/physics verification.</summary>
        public bool SignalVerifiedAction(string requiredActionId)
        {
            Resolve();
            return teacher != null && teacher.ReportVerifiedAction(requiredActionId);
        }

        public void StartLesson(SparkVisualLesson lesson)
        {
            Resolve();
            ClearMyCues();
            if (teacher != null && lesson != null) teacher.StartLesson(lesson);
        }

        public void OpenTeachingBoard()
        {
            Resolve();
            if (teacher != null) teacher.OpenTeachingBoard();
        }

        public void ClearMyCues()
        {
            if (activeAnimation != null)
            {
                StopCoroutine(activeAnimation);
                activeAnimation = null;
            }
            if (worldAnnotations != null)
                foreach (var cue in activeCues)
                    if (cue != null) worldAnnotations.Remove(cue);
            activeCues.Clear();
        }

        private bool ReadyWithAnchors(params string[] anchorIds)
        {
            Resolve();
            if (teacher == null || worldAnnotations == null)
            {
                Debug.LogWarning("[Spark Teaching Bridge] AAA teacher or world renderer not found.", this);
                return false;
            }
            foreach (string id in anchorIds)
            {
                if (SparkTeachingAnchor.TryFind(id, out _)) continue;
                if (logMissingAnchors)
                    Debug.LogWarning("[Spark Teaching Bridge] No active anchor with ID: " + id, this);
                return false;
            }
            return true;
        }

        private void AddCue(SparkVisualCommand command)
        {
            if (!SparkTeachingValidator.Validate(command, out string error))
            {
                Debug.LogWarning("[Spark Teaching Bridge] Invalid annotation: " + error, this);
                return;
            }
            var cue = worldAnnotations.Add(command);
            if (cue != null) activeCues.Add(cue);
        }

        private bool ShowCues()
        {
            if (activeCues.Count == 0) return false;
            if (automaticallyOpenBoard)
            {
                teacher.ViewBoth();
                teacher.OpenPanel();
            }
            activeAnimation = StartCoroutine(AnimateCues());
            return true;
        }

        private IEnumerator AnimateCues()
        {
            float elapsed = 0f;
            float duration = Mathf.Max(0.05f, drawDuration);
            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float progress = Mathf.Clamp01(elapsed / duration);
                foreach (var cue in activeCues)
                    if (cue != null) worldAnnotations.SetProgress(cue, progress);
                yield return null;
            }
            foreach (var cue in activeCues)
                if (cue != null) worldAnnotations.SetProgress(cue, 1f);

            if (cueLifetime > 0f)
            {
                float remaining = cueLifetime;
                while (remaining > 0f)
                {
                    remaining -= Time.unscaledDeltaTime;
                    yield return null;
                }
                activeAnimation = null;
                ClearMyCues();
            }
            else activeAnimation = null;
        }

        private SparkVisualCommand Ring(string id, Color color) => new SparkVisualCommand
        {
            shape = SparkVisualShape.Ring,
            from = Point(id),
            color = color,
            normalizedSize = new Vector2(.075f, .14f),
            thickness = 4f,
            drawSeconds = drawDuration,
            persistent = true
        };

        private SparkVisualCommand Label(string id, string message, Color color) => new SparkVisualCommand
        {
            shape = SparkVisualShape.Label,
            from = new SparkVisualPoint {
                space = SparkPointSpace.WorldAnchor,
                anchorId = id,
                pixelOffset = new Vector2(0f, 92f)
            },
            text = Trim(message, 180),
            color = color,
            fontSize = 20,
            drawSeconds = drawDuration,
            persistent = true
        };

        private static SparkVisualPoint Point(string anchorId) => new SparkVisualPoint
        {
            space = SparkPointSpace.WorldAnchor,
            anchorId = anchorId
        };

        private static string Trim(string value, int maxLength)
        {
            if (string.IsNullOrWhiteSpace(value)) return "Look at this component.";
            return value.Length <= maxLength ? value : value.Substring(0, maxLength);
        }
    }
}
