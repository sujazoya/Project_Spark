using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ProjectSpark.UI.VisualTeaching
{
    /// <summary>
    /// Read-only UI for solver-based explanations. The optional route whiteboard is
    /// an explicit user action because PresentValidatedPlan replaces a current lesson.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class SparkAdaptiveExplanationPanel : MonoBehaviour
    {
        [Header("Links")]
        [SerializeField] private SparkAdaptiveExplanationEngine engine;
        [SerializeField] private SparkVisualTeachingManager teacher;
        [SerializeField] private SparkVisualTeachingLiveBridge liveBridge;

        [Header("Created by Editor builder")]
        [SerializeField] private GameObject card;
        [SerializeField] private TMP_Text headerText;
        [SerializeField] private TMP_Text componentText;
        [SerializeField] private TMP_Text explanationText;
        [SerializeField] private TMP_Text actionText;
        [SerializeField] private TMP_Text sourceText;
        [SerializeField] private TMP_Text positionText;
        [SerializeField] private Image severityStrip;
        [SerializeField] private Button worldButton;
        [SerializeField] private Button pathButton;
        [SerializeField] private bool startVisible = false;

        private SparkAdaptiveExplanationEngine boundEngine;
        private string lastPromptKey;

        private void Awake()
        {
            Resolve();
            if (card != null) card.SetActive(startVisible);
        }

        private void OnEnable()
        {
            Resolve();
            Bind();
            Refresh();
        }

        private void Start()
        {
            Resolve();
            Bind();
            Refresh();
        }

        private void OnDisable() => Unbind();
        private void OnDestroy() => Unbind();

        private void Resolve()
        {
            if (engine == null) engine = GetComponent<SparkAdaptiveExplanationEngine>();
            if (teacher == null)
                teacher = FindFirstObjectByType<SparkVisualTeachingManager>(FindObjectsInactive.Include);
            if (liveBridge == null)
                liveBridge = FindFirstObjectByType<SparkVisualTeachingLiveBridge>(FindObjectsInactive.Include);
        }

        private void Bind()
        {
            if (boundEngine == engine) return;
            Unbind();
            boundEngine = engine;
            if (boundEngine != null) boundEngine.Changed += Refresh;
        }

        private void Unbind()
        {
            if (boundEngine != null) boundEngine.Changed -= Refresh;
            boundEngine = null;
        }

        public void ShowCard()
        {
            if (card != null) card.SetActive(true);
            Refresh();
        }

        public void CloseCard()
        {
            if (card != null) card.SetActive(false);
        }

        public void ToggleCard()
        {
            if (card == null) return;
            if (card.activeSelf) CloseCard();
            else ShowCard();
        }

        public void NextCue()
        {
            if (engine != null) engine.SelectNext();
        }

        public void PreviousCue()
        {
            if (engine != null) engine.SelectPrevious();
        }

        public void Refresh()
        {
            SparkAdaptiveCue cue = engine != null ? engine.SelectedCue : null;
            if (cue == null)
            {
                Set(headerText, "AWAITING VERIFIED SOLVER DATA");
                Set(componentText, "NO COMPONENT YET");
                Set(explanationText, "No completed electrical measurement frame has been published.");
                Set(actionText, "Run a circuit solve and publish its verified result.");
                Set(sourceText, "SOURCE: NOT CONNECTED / NO FRAME");
                Set(positionText, "0 / 0");
                if (severityStrip != null) severityStrip.color = new Color(.17f, .62f, .92f, 1f);
                if (worldButton != null) worldButton.interactable = false;
                if (pathButton != null) pathButton.interactable = false;
                return;
            }

            Set(headerText, cue.title);
            Set(componentText, string.IsNullOrWhiteSpace(cue.anchorId) ? "CIRCUIT" : cue.anchorId);
            Set(explanationText, cue.explanation);
            Set(actionText, cue.nextAction);
            string circuit = string.IsNullOrWhiteSpace(engine.CurrentFrame?.circuitId)
                ? "UNSPECIFIED CIRCUIT" : engine.CurrentFrame.circuitId;
            Set(sourceText, "VERIFIED SOLVER FRAME • " + circuit);
            Set(positionText, (engine.SelectedIndex + 1) + " / " + engine.CueCount);
            if (severityStrip != null)
                severityStrip.color = cue.severity == SparkAdaptiveSeverity.Critical
                    ? new Color(1f, .31f, .28f, 1f)
                    : cue.severity == SparkAdaptiveSeverity.Warning
                    ? new Color(1f, .73f, .26f, 1f)
                    : new Color(.20f, .83f, .92f, 1f);
            if (worldButton != null) worldButton.interactable = cue.hasWorldTarget && liveBridge != null;
            if (pathButton != null) pathButton.interactable = engine.HasVerifiedCurrentPath && teacher != null;
        }

        public void ShowCurrentInWorld()
        {
            Resolve();
            SparkAdaptiveCue cue = engine != null ? engine.SelectedCue : null;
            if (cue == null || liveBridge == null || !cue.hasWorldTarget) return;
            if (cue.severity == SparkAdaptiveSeverity.Information)
                liveBridge.ExplainKnownComponent(cue.anchorId, cue.title + ": " + cue.explanation);
            else
                liveBridge.ReportVerifiedFault(cue.anchorId, cue.title + ": " + cue.explanation);
        }

        /// <summary>
        /// Explicit opt-in: conceptual current path ONLY. Replaces the current
        /// whiteboard lesson; never called automatically on frame updates.
        /// </summary>
        public void ShowVerifiedRouteWhiteboard()
        {
            Resolve();
            if (engine == null || teacher == null || !engine.HasVerifiedCurrentPath) return;
            SparkVerifiedElectricalFrame frame = engine.CurrentFrame;
            string[] ids = frame.orderedRouteAnchors;
            if (ids == null || ids.Length < 2 || ids.Length > 8) return;

            var parts = new List<SparkSchematicPart>();
            var wires = new List<SparkSchematicWire>();
            var positions = new Vector2[ids.Length];
            for (int i = 0; i < ids.Length; i++)
            {
                // Ordered visual path, NOT a recovered physical connection schematic.
                positions[i] = new Vector2(.12f + .76f * i / (ids.Length - 1), .5f);
                parts.Add(new SparkSchematicPart {
                    id = ids[i], kind = SparkSchematicKind.Generic,
                    label = ids[i], position = positions[i],
                    color = new Color(.25f, .83f, 1f, 1f)
                });
                if (i > 0)
                    wires.Add(new SparkSchematicWire {
                        points = new[] { positions[i - 1], positions[i] },
                        energized = true, color = new Color(.96f, .78f, .25f, 1f)
                    });
            }

            var plan = new SparkVisualTeachingPlan {
                title = "VERIFIED CURRENT ROUTE",
                explanation = "Conceptual order of the solver-verified conventional-current route. " +
                              "This is not a complete wiring schematic.",
                schematic = new SparkSchematicData {
                    caption = "VERIFIED ROUTE • CONCEPTUAL, NOT FULL SCHEMATIC",
                    parts = parts.ToArray(), wires = wires.ToArray()
                },
                annotations = Array.Empty<SparkVisualCommand>()
            };
            if (SparkTeachingValidator.Validate(plan, out string reason))
                teacher.PresentValidatedPlan(plan);
            else
                Debug.LogWarning("[Spark Adaptive] Route plan rejected: " + reason, this);
        }

        private static void Set(TMP_Text label, string value)
        {
            if (label != null) label.text = value ?? "";
        }
    }
}
