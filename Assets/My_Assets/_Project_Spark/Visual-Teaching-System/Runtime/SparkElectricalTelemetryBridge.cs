using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace ProjectSpark.UI.VisualTeaching
{
    // The simulator, not the AI, supplies these verified states.
    public enum SparkElectricalCondition
    {
        Normal,
        OpenCircuit,
        ReversePolarity,
        OverCurrent,
        Failed,
        Unknown
    }

    /// <summary>
    /// Non-invasive, bounded read-only telemetry layer for the AAA Visual Teaching canvas.
    /// Existing solver code explicitly publishes values; this component never estimates them.
    /// It owns/removes only the world annotations it creates, preserving LiveBridge cues.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class SparkElectricalTelemetryBridge : MonoBehaviour
    {
        [Header("Existing AAA renderer")]
        [SerializeField] private SparkVisualTeachingManager teacher;
        [SerializeField] private SparkWorldAnnotationRenderer worldRenderer;

        [Header("Generated HUD")]
        [SerializeField] private GameObject telemetryCard;
        [SerializeField] private TMP_Text sourceText;
        [SerializeField] private TMP_Text componentText;
        [SerializeField] private TMP_Text voltageText;
        [SerializeField] private TMP_Text currentText;
        [SerializeField] private TMP_Text resistanceText;
        [SerializeField] private TMP_Text conditionText;
        [SerializeField] private TMP_Text informationText;

        [Header("Display")]
        [SerializeField] private bool showWorldCues = true;
        [SerializeField] private bool openTeacherWhenInspecting = false;
        [SerializeField, Min(0.05f)] private float uiRefreshSeconds = 0.2f;
        [Tooltip("0 keeps last sample until cleared. Positive values mark old readings STALE.")]
        [SerializeField, Min(0f)] private float sampleExpirySeconds = 5f;
        [SerializeField, Range(2, 16)] private int maxRouteAnchors = 8;
        [SerializeField] private Color healthyColor = new Color(.16f, .91f, .72f, 1f);
        [SerializeField] private Color warningColor = new Color(1f, .77f, .26f, 1f);
        [SerializeField] private Color faultColor = new Color(1f, .29f, .27f, 1f);

        private sealed class Reading
        {
            public string id;
            public float volts, amps, ohms;
            public SparkElectricalCondition condition;
            public string note;
            public float timestamp;
            public bool demo;
        }

        private readonly Dictionary<string, Reading> readings = new Dictionary<string, Reading>(StringComparer.Ordinal);
        private readonly List<SparkWorldAnnotationRenderer.Annotation> ownedAnnotations =
            new List<SparkWorldAnnotationRenderer.Annotation>();
        private readonly List<SparkWorldAnnotationRenderer.Annotation> routeAnnotations =
            new List<SparkWorldAnnotationRenderer.Annotation>();

        private string selectedAnchorId;
        private string[] verifiedRoute;
        private float routeReportedTime;
        private bool dirty = true;
        private float nextRefresh;
        private float nextRouteFrame;
        private bool lastStale;
        private bool annotationsDirty = true;

        private void Awake()
        {
            Resolve();
            if (telemetryCard != null) telemetryCard.SetActive(true);
        }

        private void OnEnable() { dirty = true; annotationsDirty = true; }

        private void OnDisable() { RemoveOwnedAnnotations(); }
        private void OnDestroy() { RemoveOwnedAnnotations(); }

        private void Resolve()
        {
            if (teacher == null)
                teacher = FindFirstObjectByType<SparkVisualTeachingManager>(FindObjectsInactive.Include);
            if (worldRenderer == null)
                worldRenderer = FindFirstObjectByType<SparkWorldAnnotationRenderer>(FindObjectsInactive.Include);
        }

        /// <summary>
        /// Call ONLY from the trusted electrical solver or its adapter after it has computed
        /// component-specific voltage/current/resistance and classified the electrical condition.
        /// Units: volts (V), amps (A), ohms (Ω). No speculative AI values.
        /// </summary>
        public bool ReportVerifiedReading(string anchorId, float volts, float amps,
            float resistanceOhms, SparkElectricalCondition condition, string verifiedNote = "")
        {
            return StoreReading(anchorId, volts, amps, resistanceOhms, condition, verifiedNote, false);
        }

        // Demo deliberately cannot claim to be measured by the circuit simulator.
        public void RunIllustrativeDemo()
        {
            string id = SparkTeachingAnchor.TryFind("resistor", out _) ? "resistor" : "demo_resistor";
            StoreReading(id, 2.2f, 0.01f, 220f, SparkElectricalCondition.Normal,
                "ILLUSTRATIVE values only. Connect the actual circuit solver for live data.", true);
        }

        private bool StoreReading(string id, float volts, float amps, float ohms,
            SparkElectricalCondition condition, string note, bool demo)
        {
            if (string.IsNullOrWhiteSpace(id) || id.Length > 80 ||
                !Finite(volts) || !Finite(amps) || !Finite(ohms) || ohms < 0f ||
                !Enum.IsDefined(typeof(SparkElectricalCondition), condition))
            {
                Debug.LogWarning("[Spark Telemetry] Rejected invalid reading.", this);
                return false;
            }

            // Avoid creating an unbounded tracking collection across level transitions.
            if (!readings.ContainsKey(id) && readings.Count >= 32)
            {
                Debug.LogWarning("[Spark Telemetry] Capacity reached (32). Clear on level unload.", this);
                return false;
            }

            readings[id] = new Reading {
                id = id, volts = volts, amps = amps, ohms = ohms,
                condition = condition, note = Shorten(note, 130),
                demo = demo, timestamp = Time.unscaledTime
            };
            selectedAnchorId = id;
            dirty = true;
            annotationsDirty = true;
            return true;
        }

        /// <summary>
        /// The solver supplies anchor IDs in the VERIFIED conventional-current direction.
        /// This is an illustrative straight-arrow route between those anchors, not an
        /// automatically reconstructed physical wire path. Pass false if not energized.
        /// </summary>
        public bool SetVerifiedCurrentRoute(string[] orderedAnchorIds, bool energized)
        {
            if (!energized)
            {
                verifiedRoute = null;
                annotationsDirty = true;
                return true;
            }
            if (orderedAnchorIds == null || orderedAnchorIds.Length < 2 ||
                orderedAnchorIds.Length > maxRouteAnchors)
                return false;
            foreach (string id in orderedAnchorIds)
                if (!SparkTeachingAnchor.TryFind(id, out _))
                {
                    Debug.LogWarning("[Spark Telemetry] Current route anchor missing: " + id, this);
                    return false;
                }
            verifiedRoute = (string[])orderedAnchorIds.Clone();
            routeReportedTime = Time.unscaledTime;
            annotationsDirty = true;
            return true;
        }

        public void SelectComponent(string anchorId)
        {
            if (string.IsNullOrWhiteSpace(anchorId) || !readings.ContainsKey(anchorId)) return;
            selectedAnchorId = anchorId;
            dirty = true;
            annotationsDirty = true;
        }

        public void ShowSelectedInWorld()
        {
            showWorldCues = true;
            Resolve();
            annotationsDirty = true;
            dirty = true;
            if (teacher != null)
            {
                if (openTeacherWhenInspecting) teacher.OpenTeachingBoard();
                else { teacher.ViewWorld(); teacher.OpenPanel(); }
            }
        }

        public void HideWorldCues()
        {
            showWorldCues = false;
            RemoveOwnedAnnotations();
            annotationsDirty = false;
        }

        // Call during level teardown, before/after replacing the level layout.
        public void ClearTelemetry()
        {
            readings.Clear();
            selectedAnchorId = null;
            verifiedRoute = null;
            RemoveOwnedAnnotations();
            dirty = true;
            annotationsDirty = false;
        }

        private void Update()
        {
            if (verifiedRoute != null && sampleExpirySeconds > 0f &&
                Time.unscaledTime - routeReportedTime > sampleExpirySeconds)
            {
                verifiedRoute = null;
                annotationsDirty = true;
            }
            bool stale = IsStale(Selected());
            if (stale != lastStale)
            {
                lastStale = stale;
                dirty = true;
                annotationsDirty = true;
            }

            if (Time.unscaledTime >= nextRefresh)
            {
                nextRefresh = Time.unscaledTime + uiRefreshSeconds;
                if (dirty)
                {
                    RefreshHUD();
                    dirty = false;
                }
                if (annotationsDirty)
                {
                    RebuildOwnedAnnotations();
                    annotationsDirty = false;
                }
            }

            // Re-play verified current direction without instantiating new GameObjects.
            if (showWorldCues && worldRenderer != null && worldRenderer.isActiveAndEnabled &&
                verifiedRoute != null && Time.unscaledTime >= nextRouteFrame)
            {
                nextRouteFrame = Time.unscaledTime + 0.1f; // Cap shared renderer updates at 10 Hz.
                float phase = Mathf.Repeat(Time.unscaledTime / 1.25f, 1f);
                foreach (var item in routeAnnotations)
                    if (item != null) worldRenderer.SetProgress(item, phase);
            }
        }

        private Reading Selected()
        {
            if (selectedAnchorId == null) return null;
            readings.TryGetValue(selectedAnchorId, out Reading reading);
            return reading;
        }

        private bool IsStale(Reading reading) => reading != null && sampleExpirySeconds > 0f &&
            Time.unscaledTime - reading.timestamp > sampleExpirySeconds;

        private void RefreshHUD()
        {
            Reading reading = Selected();
            bool exists = reading != null;
            bool stale = IsStale(reading);
            if (sourceText != null) sourceText.text = !exists ? "AWAITING SIMULATOR" :
                reading.demo ? "DEMO — NOT MEASURED" : stale ? "STALE READING" : "VERIFIED SIMULATOR DATA";
            if (componentText != null) componentText.text = exists ? reading.id : "NO COMPONENT SELECTED";
            if (voltageText != null) voltageText.text = exists ? Format(reading.volts, "V") : "— V";
            if (currentText != null) currentText.text = exists ? Format(reading.amps, "A") : "— A";
            if (resistanceText != null) resistanceText.text = exists ? Format(reading.ohms, "Ω") : "— Ω";
            if (conditionText != null)
            {
                conditionText.text = exists ? reading.condition.ToString() : "WAITING";
                conditionText.color = !exists ? warningColor : StateColor(reading.condition);
            }
            if (informationText != null) informationText.text = !exists
                ? "Connect your solver's verified results to ReportVerifiedReading()."
                : stale ? "Last sample is old. Waiting for a new solver update."
                : string.IsNullOrEmpty(reading.note) ? "Values supplied by the connected simulator." : reading.note;
        }

        private void RebuildOwnedAnnotations()
        {
            RemoveOwnedAnnotations();
            if (!showWorldCues) return;
            Resolve();
            if (worldRenderer == null) return;
            Reading reading = Selected();
            if (reading != null && !IsStale(reading) && SparkTeachingAnchor.TryFind(reading.id, out _))
            {
                Color color = StateColor(reading.condition);
                AddOwned(new SparkVisualCommand {
                    shape = SparkVisualShape.Ring, from = Anchor(reading.id),
                    normalizedSize = new Vector2(.055f, .105f),
                    color = color, thickness = 3f, persistent = true
                }, false);
                AddOwned(new SparkVisualCommand {
                    shape = SparkVisualShape.Label,
                    from = new SparkVisualPoint {
                        space = SparkPointSpace.WorldAnchor, anchorId = reading.id,
                        pixelOffset = new Vector2(0f, 92f)
                    },
                    text = reading.demo ? "DEMO • " + Format(reading.volts, "V") :
                        Format(reading.volts, "V") + " | " + Format(reading.amps, "A"),
                    color = color, fontSize = 19, persistent = true
                }, false);
            }
            if (verifiedRoute != null)
            {
                for (int i = 0; i + 1 < verifiedRoute.Length; i++)
                {
                    if (!SparkTeachingAnchor.TryFind(verifiedRoute[i], out _) ||
                        !SparkTeachingAnchor.TryFind(verifiedRoute[i + 1], out _)) continue;
                    AddOwned(new SparkVisualCommand {
                        shape = SparkVisualShape.Arrow,
                        from = Anchor(verifiedRoute[i]), to = Anchor(verifiedRoute[i + 1]),
                        color = warningColor, thickness = 4f, persistent = true
                    }, true);
                }
            }
        }

        private void AddOwned(SparkVisualCommand command, bool route)
        {
            if (!SparkTeachingValidator.Validate(command, out _)) return;
            var cue = worldRenderer.Add(command);
            if (cue == null) return;
            ownedAnnotations.Add(cue);
            if (route) routeAnnotations.Add(cue);
            else worldRenderer.SetProgress(cue, 1f);
        }

        private void RemoveOwnedAnnotations()
        {
            if (worldRenderer != null)
                foreach (var a in ownedAnnotations)
                    if (a != null) worldRenderer.Remove(a);
            ownedAnnotations.Clear();
            routeAnnotations.Clear();
        }

        private Color StateColor(SparkElectricalCondition condition)
        {
            switch (condition)
            {
                case SparkElectricalCondition.Normal: return healthyColor;
                case SparkElectricalCondition.Unknown: return warningColor;
                case SparkElectricalCondition.OpenCircuit: return warningColor;
                default: return faultColor;
            }
        }

        private static SparkVisualPoint Anchor(string id) => new SparkVisualPoint {
            space = SparkPointSpace.WorldAnchor, anchorId = id
        };
        private static bool Finite(float x) => !float.IsNaN(x) && !float.IsInfinity(x);
        private static string Shorten(string s, int max) => string.IsNullOrEmpty(s) ? "" :
            s.Length > max ? s.Substring(0, max) : s;
        private static string Format(float value, string unit)
        {
            float abs = Mathf.Abs(value);
            if (unit == "A")
            {
                if (abs < .000001f && abs > 0f) return (value * 1000000000f).ToString("0.##") + " nA";
                if (abs < .001f && abs > 0f) return (value * 1000000f).ToString("0.##") + " µA";
                if (abs < 1f && abs > 0f) return (value * 1000f).ToString("0.##") + " mA";
            }
            if (unit == "Ω" && abs >= 1000f)
                return (value / 1000f).ToString("0.###") + " kΩ";
            return value.ToString("0.###") + " " + unit;
        }
    }
}
