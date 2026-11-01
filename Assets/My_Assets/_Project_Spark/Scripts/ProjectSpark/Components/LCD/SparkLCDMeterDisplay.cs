using System;
using System.Globalization;
using TMPro;
using UnityEngine;
using ProjectSpark.Electrical;
using ProjectSpark.Gameplay.Shelf;

namespace ProjectSpark.Gameplay
{
    /// <summary>
    /// Optional source for a mode that reports a component current, voltage drop,
    /// resistance, or power. Prefer stable level-binding IDs for fixed components;
    /// use a NARROW replacement slot root for shelf-placed components.
    /// Never silently guesses which shelf resistor is installed.
    /// </summary>
    [Serializable]
    public sealed class SparkLCDComponentSource
    {
        [Tooltip("Stable ID in the active SparkLevelSceneBindings; preferred if assigned.")]
        public string levelBindingId;
        [Tooltip("Optional slot that contains exactly one active installed electrical component.")]
        public Transform replacementSlotRoot;
        [Tooltip("Direct reference, useful for fixed components and meter-only test scenes.")]
        public SparkElectricalComponent directComponent;
    }

    [DisallowMultipleComponent]
    [RequireComponent(typeof(SparkLCDMeter))]
    public sealed class SparkLCDMeterDisplay : MonoBehaviour
    {
        [Header("1602 LCD display")]
        [SerializeField] private TMP_Text screenText;
        [SerializeField] private SparkLCDMeter meter;
        [SerializeField] private SparkElectricalSolver solver;
        [SerializeField, Min(0.05f)] private float refreshIntervalSeconds = 0.15f;
        [SerializeField] private bool fitTo16Characters = true;

        [Header("Current level (one Unity scene)")]
        [SerializeField] private SparkLevelLayoutLoader levelLoader;
        [Tooltip("Used only when Level Loader is not assigned/found.")]
        [SerializeField] private SparkLevelSceneBindings manualBindings;

        [Header("Analysis component sources")]
        [SerializeField] private SparkLCDComponentSource resistor1 = new SparkLCDComponentSource();
        [Tooltip("LCD label for Source 1; e.g. R1, R2, LED1.")]
        [SerializeField] private string source1DisplayName = "R1";
        [SerializeField] private SparkLCDComponentSource resistor2 = new SparkLCDComponentSource();
        [Tooltip("Source-current mode must read an actual solver-supported supply component.")]
        [SerializeField] private SparkLCDComponentSource powerSource = new SparkLCDComponentSource();

        [Header("Voltage divider (optional solver references, NOT extra LCD pins)")]
        [Tooltip("Select a point representing the input supply positive for VIN.")]
        [SerializeField] private SparkTerminal supplyPositive;
        [Tooltip("Select the same electrical reference as LCD terminal B / ground.")]
        [SerializeField] private SparkTerminal supplyGround;

        private SparkElectricalSolver subscribedSolver;
        private SparkLevelLayoutLoader subscribedLoader;
        private SparkLevelLayout knownLayout;
        private SparkElectricalSolveResult verifiedResult;
        private float nextRefresh;
        private string lastPrinted;

        private void OnEnable()
        {
            FindReferences();
            BindEvents();
            knownLayout = levelLoader != null ? levelLoader.ActiveLayout : null;
            // A read already computed for this same component is acceptable, but
            // every subsequent level switch requires a NEW successful solve.
            verifiedResult = solver != null && !solver.IsDirty ? solver.LatestSolveResult : null;
            nextRefresh = 0f;
            RefreshNow();
        }

        private void OnDisable()
        {
            UnbindEvents();
            verifiedResult = null;
        }

        private void Update()
        {
            if (meter == null || solver == null)
            {
                FindReferences();
                BindEvents();
            }
            if (levelLoader != null && knownLayout != levelLoader.ActiveLayout)
                InvalidateForNewLevel();

            // Handles Inspector mode/impedance changes made during Play Mode;
            // never forces a solve every frame or calls SolveNow for clean DC data.
            if (meter != null && meter.NeedsTopologyRefresh && solver != null &&
                !solver.IsSolving)
            {
                verifiedResult = null;
                solver.MarkTopologyDirty();
                solver.SolveNow();
            }
            if (Time.unscaledTime < nextRefresh) return;
            nextRefresh = Time.unscaledTime + Mathf.Max(0.05f, refreshIntervalSeconds);
            RefreshNow();
        }

        private void FindReferences()
        {
            if (meter == null) meter = GetComponent<SparkLCDMeter>();
            if (solver == null) solver = FindFirstObjectByType<SparkElectricalSolver>();
            if (levelLoader == null)
                levelLoader = FindFirstObjectByType<SparkLevelLayoutLoader>();
        }

        private void BindEvents()
        {
            if (subscribedSolver != solver)
            {
                if (subscribedSolver != null)
                {
                    subscribedSolver.SolveCompleted -= HandleSolveCompleted;
                    subscribedSolver.SolveFailed -= HandleSolveFailed;
                }
                subscribedSolver = solver;
                if (subscribedSolver != null)
                {
                    subscribedSolver.SolveCompleted += HandleSolveCompleted;
                    subscribedSolver.SolveFailed += HandleSolveFailed;
                }
            }
            if (subscribedLoader != levelLoader)
            {
                if (subscribedLoader != null)
                {
                    subscribedLoader.LayoutLoaded -= HandleLayoutLoaded;
                    subscribedLoader.LayoutUnloaded -= HandleLayoutUnloaded;
                }
                subscribedLoader = levelLoader;
                if (subscribedLoader != null)
                {
                    subscribedLoader.LayoutLoaded += HandleLayoutLoaded;
                    subscribedLoader.LayoutUnloaded += HandleLayoutUnloaded;
                }
            }
        }

        private void UnbindEvents()
        {
            if (subscribedSolver != null)
            {
                subscribedSolver.SolveCompleted -= HandleSolveCompleted;
                subscribedSolver.SolveFailed -= HandleSolveFailed;
                subscribedSolver = null;
            }
            if (subscribedLoader != null)
            {
                subscribedLoader.LayoutLoaded -= HandleLayoutLoaded;
                subscribedLoader.LayoutUnloaded -= HandleLayoutUnloaded;
                subscribedLoader = null;
            }
        }

        private void InvalidateForNewLevel()
        {
            knownLayout = levelLoader != null ? levelLoader.ActiveLayout : null;
            verifiedResult = null;
            if (meter != null) meter.ClearSolvedMeterState();
            Show("SPARK LCD / WAIT", "AWAITING SOLVE");
        }

        private void HandleLayoutUnloaded() => InvalidateForNewLevel();
        private void HandleLayoutLoaded(SparkLevelLayout _) => InvalidateForNewLevel();
        private void HandleSolveFailed() => verifiedResult = null;
        private void HandleSolveCompleted()
        {
            if (solver == null || solver.IsDirty) return;
            if (levelLoader != null && !levelLoader.HasActiveLayout) return;
            knownLayout = levelLoader != null ? levelLoader.ActiveLayout : null;
            verifiedResult = solver.LatestSolveResult;
            RefreshNow();
        }

        public void RefreshNow()
        {
            if (meter == null || screenText == null) return;
            if (!meter.ElectricalEnabled)
            {
                Show("LCD DISABLED", "NO MEASUREMENT");
                return;
            }
            if (solver == null || solver.IsDirty || solver.IsSolving ||
                verifiedResult == null || solver.LatestSolveResult != verifiedResult ||
                (levelLoader != null && !levelLoader.HasActiveLayout))
            {
                Show("SPARK LCD / WAIT", "AWAITING SOLVE");
                return;
            }
            if (!IsDiscoveredBySolver())
            {
                Show("METER NOT FOUND", "CHECK SEARCH ROOT");
                return;
            }
            if (meter.TerminalA == null || meter.TerminalB == null ||
                !verifiedResult.TryGetVoltage(meter.TerminalA, meter.TerminalB, out float vab))
            {
                Show("CHECK LCD WIRES", "A / B NO VOLTAGE");
                return;
            }
            SparkLCDMeasurementMode mode = meter.MeasurementMode;
            string c1Label = string.IsNullOrWhiteSpace(source1DisplayName)
                ? "C1" : source1DisplayName.Trim();
            SparkElectricalComponent c1 = Resolve(resistor1);
            SparkElectricalComponent c2 = Resolve(resistor2);
            SparkElectricalComponent source = Resolve(powerSource);

            switch (mode)
            {
                case SparkLCDMeasurementMode.DCVoltage:
                    Show("DC V  " + V(vab), "A(+)-B(-) LIVE"); break;
                case SparkLCDMeasurementMode.VoltageToGround:
                    Show("VGND  " + V(vab), "B MUST BE GND"); break;
                case SparkLCDMeasurementMode.VoltageDrop:
                    Show("V DROP " + V(vab), "BETWEEN A AND B"); break;
                case SparkLCDMeasurementMode.InlineCurrent:
                    Show("INLINE " + I(vab / meter.MeterResistanceOhms), "SERIES SHUNT"); break;
                case SparkLCDMeasurementMode.ComponentCurrent:
                    if (TryCurrent(c1, out float ci))
                        Show(c1Label + " I " + I(ci), "SOLVED COMPONENT");
                    else Missing("R1 CURRENT");
                    break;
                case SparkLCDMeasurementMode.SourceCurrent:
                    if (TryCurrent(source, out float si))
                        Show("SRC I " + I(si), "SOLVED SUPPLY");
                    else Missing("SOURCE CURRENT");
                    break;
                case SparkLCDMeasurementMode.CurrentSum:
                    if (c1 != c2 && TryCurrent(c1, out float i1) && TryCurrent(c2, out float i2))
                        Show("SUM I " + I(Mathf.Abs(i1) + Mathf.Abs(i2)), "TWO BRANCHES");
                    else Missing("2 CURRENTS");
                    break;
                case SparkLCDMeasurementMode.Resistance:
                    if (TryResistance(c1, out float r1))
                        Show(c1Label + " " + R(r1), "NOMINAL VALUE");
                    else Missing("R1 RESISTANCE");
                    break;
                case SparkLCDMeasurementMode.SeriesResistance:
                    if (c1 != c2 && TryResistance(c1, out float rs1) && TryResistance(c2, out float rs2))
                        Show("R1+R2 " + R(rs1 + rs2), "CALCULATED ONLY");
                    else Missing("2 RESISTORS");
                    break;
                case SparkLCDMeasurementMode.ParallelResistance:
                    if (c1 != c2 && TryResistance(c1, out float rp1) && TryResistance(c2, out float rp2))
                        Show("R1||R2 " + R(rp1 * rp2 / (rp1 + rp2)), "CALCULATED ONLY");
                    else Missing("2 RESISTORS");
                    break;
                case SparkLCDMeasurementMode.VoltageSum:
                    if (c1 != c2 && TryComponentVoltage(c1, out float v1) && TryComponentVoltage(c2, out float v2))
                        Show("SUM V " + V(Mathf.Abs(v1) + Mathf.Abs(v2)), "DROP MAGNITUDES");
                    else Missing("2 V DROPS");
                    break;
                case SparkLCDMeasurementMode.ComponentPower:
                    if (TryComponentPower(c1, out float p1))
                        Show(c1Label + " P " + P(p1), "ACTUAL SOLVED");
                    else Missing("R1 POWER");
                    break;
                case SparkLCDMeasurementMode.TotalPower:
                    if (c1 != c2 && TryComponentPower(c1, out float ps1) && TryComponentPower(c2, out float ps2))
                        Show("SUM P " + P(ps1 + ps2), "TWO COMPONENTS");
                    else Missing("2 POWERS");
                    break;
                case SparkLCDMeasurementMode.VoltageDivider:
                    if (supplyPositive != null && supplyGround != null &&
                        verifiedResult.TryGetVoltage(supplyPositive, supplyGround, out float vin) &&
                        Mathf.Abs(vin) > 0.000001f)
                    {
                        Show("VOUT " + V(vab), "RATIO " + F(100f * vab / vin, "0.0") + "%");
                    }
                    else Missing("VIN TERMINALS");
                    break;
                case SparkLCDMeasurementMode.SameElectricalNode:
                    if (verifiedResult.Context.Graph.TryGetNode(meter.TerminalA, out int na) &&
                        verifiedResult.Context.Graph.TryGetNode(meter.TerminalB, out int nb))
                        Show(na == nb ? "SAME NODE" : "DIFFERENT NODES", "NOT OHM TEST");
                    else Missing("NODE MAP");
                    break;
                default:
                    if (TryCurrent(c1, out float selectedCurrent))
                        Show("VAB " + V(vab), c1Label + " I " + I(selectedCurrent));
                    else Show("VAB " + V(vab), "I: SELECT SOURCE");
                    break;
            }
        }

        private bool IsDiscoveredBySolver()
        {
            var all = solver.ElectricalComponents;
            for (int i = 0; i < all.Count; i++) if (all[i] == meter) return true;
            return false;
        }

        private SparkElectricalComponent Resolve(SparkLCDComponentSource binding)
        {
            if (binding == null) return null;
            if (!string.IsNullOrWhiteSpace(binding.levelBindingId))
            {
                SparkLevelSceneBindings b = levelLoader != null
                    ? levelLoader.ActiveBindings : manualBindings;
                if (b != null && b.TryGetComponent(binding.levelBindingId,
                        out SparkElectricalComponent bound) && IsUsable(bound))
                    return bound;
                return null; // No stale direct-reference fallback for a declared level ID.
            }
            if (binding.replacementSlotRoot != null)
            {
                SparkElectricalComponent found = null;
                var all = binding.replacementSlotRoot
                    .GetComponentsInChildren<SparkElectricalComponent>(false);
                for (int i = 0; i < all.Length; i++)
                {
                    if (!IsUsable(all[i]) || all[i] == meter) continue;
                    if (found != null) return null; // Ambiguous slot: never guess.
                    found = all[i];
                }
                return found;
            }
            return IsUsable(binding.directComponent) ? binding.directComponent : null;
        }

        private static bool IsUsable(SparkElectricalComponent c)
        {
            if (c == null || !c.isActiveAndEnabled || !c.ElectricalEnabled) return false;
            SparkShelfItem shelfItem = c.GetComponentInParent<SparkShelfItem>();
            return shelfItem == null || shelfItem.IsAtWorkbench;
        }

        private bool TryCurrent(SparkElectricalComponent c, out float current)
        {
            current = 0f;
            return IsUsable(c) && solver.TryCalculateComponentCurrent(c, out current) &&
                !float.IsNaN(current) && !float.IsInfinity(current);
        }

        private static bool TryResistance(SparkElectricalComponent c, out float resistance)
        {
            resistance = 0f;
            if (!(c is SparkResistor resistor) || !IsUsable(c)) return false;
            SparkResistorBurn burn = resistor.GetComponent<SparkResistorBurn>();
            if (burn != null && burn.IsFailed) return false;
            resistance = resistor.ResistanceOhms;
            return resistance > 0f && !float.IsNaN(resistance) && !float.IsInfinity(resistance);
        }

        private bool TryComponentVoltage(SparkElectricalComponent c, out float voltage)
        {
            voltage = 0f;
            if (!IsUsable(c)) return false;
            if (c is SparkResistor resistor)
                return resistor.TerminalA != null && resistor.TerminalB != null &&
                    verifiedResult.TryGetVoltage(resistor.TerminalA, resistor.TerminalB, out voltage);
            if (c is SparkLED led)
                return led.AnodeTerminal != null && led.CathodeTerminal != null &&
                    verifiedResult.TryGetVoltage(led.AnodeTerminal, led.CathodeTerminal, out voltage);
            return false; // Never invent a terminal pair for an unknown component type.
        }

        private bool TryComponentPower(SparkElectricalComponent c, out float watts)
        {
            watts = 0f;
            if (!TryCurrent(c, out float current) ||
                !TryComponentVoltage(c, out float voltage)) return false;
            watts = Mathf.Abs(voltage * current);
            return true;
        }

        private void Missing(string what) => Show("NO VERIFIED DATA", what);
        private static string F(float value, string format) =>
            value.ToString(format, CultureInfo.InvariantCulture);
        private static string V(float value) => F(value, "+0.000;-0.000;0.000") + "V";
        private static string I(float value) =>
            Mathf.Abs(value) < 1f ? F(value * 1000f, "0.00") + "mA"
                                  : F(value, "0.000") + "A";
        private static string R(float value) =>
            value >= 1000f ? F(value / 1000f, "0.###") + "kOHM"
                           : F(value, "0.##") + "OHM";
        private static string P(float value) =>
            value < 1f ? F(value * 1000f, "0.00") + "mW" : F(value, "0.000") + "W";

        private void Show(string line1, string line2)
        {
            if (screenText == null) return;
            if (fitTo16Characters)
            {
                if (line1.Length > 16) line1 = line1.Substring(0, 16);
                if (line2.Length > 16) line2 = line2.Substring(0, 16);
            }
            string value = line1 + "\n" + line2;
            if (value == lastPrinted) return;
            lastPrinted = value;
            screenText.text = value;
        }

        public void AssignSceneReferences(
            TMP_Text text, SparkLCDMeter target, SparkElectricalSolver electricalSolver)
        {
            screenText = text;
            meter = target;
            solver = electricalSolver;
        }
    }
}
