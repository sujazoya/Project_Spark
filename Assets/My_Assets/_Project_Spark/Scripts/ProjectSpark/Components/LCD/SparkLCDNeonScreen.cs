using System;
using System.Globalization;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using ProjectSpark.Electrical;
using ProjectSpark.Gameplay.Shelf;

namespace ProjectSpark.Gameplay
{
    /// <summary>
    /// Read-only world-space LCD skin for the existing SparkLCDMeterDisplay.
    /// MeterDisplay still owns measurement verification and source selection.
    /// No solver stamping, circuit modifications, or runtime object creation.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class SparkLCDNeonScreen : MonoBehaviour
    {
        [Header("Existing electrical system")]
        [SerializeField] private SparkLCDMeter meter;
        [SerializeField] private SparkLCDMeterDisplay verifiedDisplay;
        [SerializeField] private SparkElectricalSolver solver;
        [Tooltip("Invisible two-line TMP output written by SparkLCDMeterDisplay.")]
        [SerializeField] private TMP_Text sourceText;

        [Header("Optional component for Current / Ohms / Power cards")]
        [Tooltip("Stable active-level binding ID, e.g. r2. Takes priority over slot and direct reference.")]
        [SerializeField] private string statsLevelBindingId;
        [Tooltip("If assigned, exactly one enabled electrical component must occupy this slot.")]
        [SerializeField] private Transform statsReplacementSlotRoot;
        [SerializeField] private SparkElectricalComponent statsDirectComponent;
        [SerializeField] private SparkLevelLayoutLoader levelLoader;
        [SerializeField] private SparkLevelSceneBindings manualBindings;

        [Header("Generated screen references")]
        [SerializeField] private TMP_Text mainValue;
        [SerializeField] private TMP_Text mainUnit;
        [SerializeField] private TMP_Text mainHeading;
        [SerializeField] private TMP_Text secondaryLine;
        [SerializeField] private TMP_Text headerStatus;
        [SerializeField] private TMP_Text currentText;
        [SerializeField] private TMP_Text resistanceText;
        [SerializeField] private TMP_Text powerText;
        [SerializeField] private TMP_Text footerText;
        [SerializeField] private TMP_Text modeText;
        [SerializeField] private Image[] modeHighlights = new Image[4];
        [SerializeField] private Button[] modeButtons = new Button[4];
        [SerializeField] private Button nextModeButton;
        [Tooltip("False protects against accidentally shorting a supply when stepping through modes.")]
        [SerializeField] private bool allowInlineModeFromNextButton;
        [SerializeField, Min(0.05f)] private float visualRefreshSeconds = 0.10f;

        private static readonly Regex ValueExpression = new Regex(
            @"(?<v>[+-]?\d+(?:\.\d+)?)\s*(?<unit>kOHM|OHM|mA|mV|mW|V|A|W|%)",
            RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
        private static readonly Color Cyan = new Color(0.24f, 0.98f, 1f, 1f);
        private static readonly Color WhiteCyan = new Color(0.78f, 1f, 1f, 1f);
        private static readonly Color DimButton = new Color(0.015f, 0.19f, 0.23f, 1f);
        private static readonly Color ActiveButton = new Color(0.10f, 0.67f, 0.52f, 1f);

        private float nextRefresh;
        private float changedAt;
        private string displayedValue = "";
        private bool listenersAdded;

        private void OnEnable()
        {
            if (meter == null) meter = GetComponent<SparkLCDMeter>();
            if (verifiedDisplay == null) verifiedDisplay = GetComponent<SparkLCDMeterDisplay>();
            if (solver == null) solver = FindFirstObjectByType<SparkElectricalSolver>();
            if (levelLoader == null) levelLoader = FindFirstObjectByType<SparkLevelLayoutLoader>();
            AddListeners();
            nextRefresh = 0f;
        }

        private void OnDisable() => RemoveListeners();

        private void AddListeners()
        {
            if (listenersAdded) return;
            if (modeButtons == null || modeButtons.Length < 4) return;
            if (modeButtons[0] != null) modeButtons[0].onClick.AddListener(SelectVoltage);
            if (modeButtons[1] != null) modeButtons[1].onClick.AddListener(SelectComponentCurrent);
            if (modeButtons[2] != null) modeButtons[2].onClick.AddListener(SelectResistance);
            if (modeButtons[3] != null) modeButtons[3].onClick.AddListener(SelectPower);
            if (nextModeButton != null) nextModeButton.onClick.AddListener(NextSafeMode);
            listenersAdded = true;
        }

        private void RemoveListeners()
        {
            if (!listenersAdded) return;
            if (modeButtons != null && modeButtons.Length >= 4)
            {
                if (modeButtons[0] != null) modeButtons[0].onClick.RemoveListener(SelectVoltage);
                if (modeButtons[1] != null) modeButtons[1].onClick.RemoveListener(SelectComponentCurrent);
                if (modeButtons[2] != null) modeButtons[2].onClick.RemoveListener(SelectResistance);
                if (modeButtons[3] != null) modeButtons[3].onClick.RemoveListener(SelectPower);
            }
            if (nextModeButton != null) nextModeButton.onClick.RemoveListener(NextSafeMode);
            listenersAdded = false;
        }

        private void SelectVoltage() => SetMode(SparkLCDMeasurementMode.DCVoltage);
        private void SelectComponentCurrent() => SetMode(SparkLCDMeasurementMode.ComponentCurrent);
        private void SelectResistance() => SetMode(SparkLCDMeasurementMode.Resistance);
        private void SelectPower() => SetMode(SparkLCDMeasurementMode.ComponentPower);

        private void SetMode(SparkLCDMeasurementMode next)
        {
            if (meter == null) return;
            meter.SetMeasurementMode(next);
            if (verifiedDisplay != null) verifiedDisplay.RefreshNow();
            nextRefresh = 0f;
        }

        /// <summary>Convenience cycling; skips unsafe inline shunt unless explicitly allowed.</summary>
        public void NextSafeMode()
        {
            if (meter == null) return;
            int count = Enum.GetValues(typeof(SparkLCDMeasurementMode)).Length;
            int index = (int)meter.MeasurementMode;
            for (int step = 0; step < count; step++)
            {
                index = (index + 1) % count;
                var candidate = (SparkLCDMeasurementMode)index;
                if (candidate == SparkLCDMeasurementMode.InlineCurrent && !allowInlineModeFromNextButton)
                    continue;
                SetMode(candidate);
                return;
            }
        }

        private void Update()
        {
            if (Time.unscaledTime >= nextRefresh)
            {
                nextRefresh = Time.unscaledTime + Mathf.Max(0.05f, visualRefreshSeconds);
                RefreshVisuals();
            }
            if (mainValue != null && Application.isPlaying)
                mainValue.color = Color.Lerp(WhiteCyan, Cyan,
                    Mathf.Clamp01((Time.unscaledTime - changedAt) / 0.60f));
        }

        public void RefreshVisuals()
        {
            if (meter == null || sourceText == null || mainValue == null) return;
            bool solverReady = solver != null && !solver.IsDirty && !solver.IsSolving &&
                               solver.LatestSolveResult != null;
            string data = sourceText.text ?? "";
            bool hasReading = solverReady && verifiedDisplay != null && verifiedDisplay.isActiveAndEnabled &&
                              !string.IsNullOrWhiteSpace(data) &&
                              !data.StartsWith("SPARK LCD / WAIT", StringComparison.OrdinalIgnoreCase) &&
                              !data.StartsWith("NO VERIFIED DATA", StringComparison.OrdinalIgnoreCase) &&
                              !data.StartsWith("CHECK LCD WIRES", StringComparison.OrdinalIgnoreCase) &&
                              !data.StartsWith("METER NOT FOUND", StringComparison.OrdinalIgnoreCase) &&
                              !data.StartsWith("LCD DISABLED", StringComparison.OrdinalIgnoreCase);
            string[] lines = data.Split(new[] { '\n' }, 2);
            string first = lines.Length > 0 ? lines[0].Trim() : "";
            string second = lines.Length > 1 ? lines[1].Trim() : "";
            var mode = meter.MeasurementMode;
            SetText(modeText, "MODE  " + ModeLabel(mode));
            UpdateModes(mode);

            if (!hasReading)
            {
                UpdateBigValue("--.---", "", false);
                SetText(mainHeading, "MEASUREMENT UNAVAILABLE");
                SetText(secondaryLine, string.IsNullOrEmpty(second) ? "AWAITING VERIFIED SOLVE" : second);
                SetText(headerStatus, "● WAITING");
                SetText(currentText, "--");
                SetText(resistanceText, "--");
                SetText(powerText, "--");
                SetText(footerText, "CONNECT A / B  •  SOLVE CIRCUIT");
                return;
            }

            Match number = ValueExpression.Match(first);
            string value = number.Success ? number.Groups["v"].Value : first;
            string unit = number.Success ? number.Groups["unit"].Value : "";
            UpdateBigValue(value, unit, true);
            string heading = number.Success
                ? first.Substring(0, number.Index).Trim()
                : "SOLVED ELECTRICAL STATE";
            SetText(mainHeading, string.IsNullOrWhiteSpace(heading) ? ModeLabel(mode) : heading);
            SetText(secondaryLine, second);
            SetText(headerStatus, "● SOLVER VERIFIED");
            SetText(footerText, "A (+) / B (-)   •   " + ModeLabel(mode).ToUpperInvariant() + "   •   LIVE");

            // For these three cards, use one explicitly chosen, solver-discovered
            // component. Never fabricate an ohm/power value from arbitrary probes.
            SparkElectricalComponent source = ResolveStatsSource();
            float current;
            bool hasCurrent = TrySolvedCurrent(source, out current);
            float ohms;
            bool hasOhms = TryNominalResistance(source, out ohms);
            float volts;
            bool hasDrop = TrySolvedVoltage(source, out volts);
            SetText(currentText, hasCurrent ? FormatCurrent(current) : ExtractCurrent(second));
            SetText(resistanceText, hasOhms ? FormatResistance(ohms) : "--");
            SetText(powerText, hasCurrent && hasDrop
                ? FormatPower(Mathf.Abs(current * volts)) : "--");
        }

        private void UpdateBigValue(string value, string unit, bool valid)
        {
            if (value != displayedValue)
            {
                displayedValue = value;
                changedAt = Time.unscaledTime;
            }
            SetText(mainValue, value);
            SetText(mainUnit, unit);
            if (!valid && mainValue != null) mainValue.color = Cyan;
        }

        private void UpdateModes(SparkLCDMeasurementMode mode)
        {
            int selected = 0;
            switch (mode)
            {
                case SparkLCDMeasurementMode.InlineCurrent:
                case SparkLCDMeasurementMode.ComponentCurrent:
                case SparkLCDMeasurementMode.SourceCurrent:
                case SparkLCDMeasurementMode.CurrentSum: selected = 1; break;
                case SparkLCDMeasurementMode.Resistance:
                case SparkLCDMeasurementMode.SeriesResistance:
                case SparkLCDMeasurementMode.ParallelResistance: selected = 2; break;
                case SparkLCDMeasurementMode.ComponentPower:
                case SparkLCDMeasurementMode.TotalPower: selected = 3; break;
            }
            if (modeHighlights != null)
                for (int i = 0; i < modeHighlights.Length; i++)
                    if (modeHighlights[i] != null)
                        modeHighlights[i].color = i == selected ? ActiveButton : DimButton;
        }

        private SparkElectricalComponent ResolveStatsSource()
        {
            SparkElectricalComponent source = null;
            if (!string.IsNullOrWhiteSpace(statsLevelBindingId))
            {
                var bindings = levelLoader != null ? levelLoader.ActiveBindings : manualBindings;
                if (bindings == null || !bindings.TryGetComponent(statsLevelBindingId, out source))
                    return null;
            }
            else if (statsReplacementSlotRoot != null)
            {
                SparkElectricalComponent[] candidates =
                    statsReplacementSlotRoot.GetComponentsInChildren<SparkElectricalComponent>(false);
                foreach (SparkElectricalComponent candidate in candidates)
                {
                    if (!IsValidComponent(candidate)) continue;
                    if (source != null) return null; // Ambiguous source.
                    source = candidate;
                }
            }
            else source = statsDirectComponent;
            if (!IsValidComponent(source) || solver == null) return null;
            var discovered = solver.ElectricalComponents;
            for (int i = 0; i < discovered.Count; i++)
                if (discovered[i] == source) return source;
            return null;
        }

        private bool IsValidComponent(SparkElectricalComponent component)
        {
            if (component == null || component == meter || !component.isActiveAndEnabled ||
                !component.ElectricalEnabled) return false;
            SparkShelfItem shelf = component.GetComponentInParent<SparkShelfItem>();
            return shelf == null || shelf.IsAtWorkbench;
        }

        private bool TrySolvedCurrent(SparkElectricalComponent component, out float current)
        {
            current = 0f;
            return component != null && solver != null && !solver.IsDirty &&
                   solver.TryCalculateComponentCurrent(component, out current) && IsFinite(current);
        }

        private bool TrySolvedVoltage(SparkElectricalComponent component, out float voltage)
        {
            voltage = 0f;
            if (component == null || solver == null || solver.IsDirty || solver.LatestSolveResult == null)
                return false;
            var result = solver.LatestSolveResult;
            if (component is SparkResistor r && r.TerminalA != null && r.TerminalB != null)
                return result.TryGetVoltage(r.TerminalA, r.TerminalB, out voltage) && IsFinite(voltage);
            if (component is SparkLED led && led.AnodeTerminal != null && led.CathodeTerminal != null)
                return result.TryGetVoltage(led.AnodeTerminal, led.CathodeTerminal, out voltage) && IsFinite(voltage);
            return false;
        }

        private static bool TryNominalResistance(SparkElectricalComponent component, out float ohms)
        {
            ohms = 0f;
            if (!(component is SparkResistor resistor)) return false;
            SparkResistorBurn burn = resistor.GetComponent<SparkResistorBurn>();
            if (burn != null && burn.IsFailed) return false;
            ohms = resistor.ResistanceOhms;
            return ohms > 0f && IsFinite(ohms);
        }

        private static string ExtractCurrent(string line)
        {
            if (string.IsNullOrWhiteSpace(line) || line.IndexOf(" I ", StringComparison.OrdinalIgnoreCase) < 0)
                return "--";
            var match = ValueExpression.Match(line);
            if (!match.Success) return "--";
            string unit = match.Groups["unit"].Value;
            return unit.Equals("mA", StringComparison.OrdinalIgnoreCase) ||
                   unit.Equals("A", StringComparison.OrdinalIgnoreCase)
                ? match.Groups["v"].Value + " " + unit : "--";
        }

        private static string FormatCurrent(float value) =>
            Mathf.Abs(value) < 1f ? (value * 1000f).ToString("0.00", CultureInfo.InvariantCulture) + " mA" :
                value.ToString("0.000", CultureInfo.InvariantCulture) + " A";
        private static string FormatResistance(float value) => value >= 1000f
            ? (value / 1000f).ToString("0.###", CultureInfo.InvariantCulture) + " kΩ"
            : value.ToString("0.##", CultureInfo.InvariantCulture) + " Ω";
        private static string FormatPower(float value) => value < 1f
            ? (value * 1000f).ToString("0.00", CultureInfo.InvariantCulture) + " mW"
            : value.ToString("0.000", CultureInfo.InvariantCulture) + " W";
        private static bool IsFinite(float n) => !float.IsNaN(n) && !float.IsInfinity(n);
        private static void SetText(TMP_Text label, string text)
        {
            if (label != null && label.text != text) label.text = text;
        }
        private static string ModeLabel(SparkLCDMeasurementMode mode)
        {
            switch (mode)
            {
                case SparkLCDMeasurementMode.AutoVoltageAndComponentCurrent: return "AUTO";
                case SparkLCDMeasurementMode.DCVoltage: return "DC VOLTAGE";
                case SparkLCDMeasurementMode.VoltageToGround: return "V TO GROUND";
                case SparkLCDMeasurementMode.VoltageDrop: return "VOLT DROP";
                case SparkLCDMeasurementMode.InlineCurrent: return "INLINE CURRENT";
                case SparkLCDMeasurementMode.ComponentCurrent: return "COMPONENT CURRENT";
                case SparkLCDMeasurementMode.SourceCurrent: return "SOURCE CURRENT";
                case SparkLCDMeasurementMode.CurrentSum: return "CURRENT SUM";
                case SparkLCDMeasurementMode.Resistance: return "RESISTANCE";
                case SparkLCDMeasurementMode.SeriesResistance: return "SERIES OHMS";
                case SparkLCDMeasurementMode.ParallelResistance: return "PARALLEL OHMS";
                case SparkLCDMeasurementMode.VoltageSum: return "VOLT SUM";
                case SparkLCDMeasurementMode.ComponentPower: return "COMPONENT POWER";
                case SparkLCDMeasurementMode.TotalPower: return "TOTAL POWER";
                case SparkLCDMeasurementMode.VoltageDivider: return "VOLT DIVIDER";
                case SparkLCDMeasurementMode.SameElectricalNode: return "NODE CHECK";
                default: return mode.ToString();
            }
        }
    }
}
