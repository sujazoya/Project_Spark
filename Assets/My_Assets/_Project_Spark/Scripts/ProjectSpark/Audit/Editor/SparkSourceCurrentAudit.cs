#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Text;
using UnityEditor;
using UnityEngine;
using ProjectSpark.Gameplay;
using ProjectSpark.Electrical;
using ProjectSpark.Gameplay.Shelf;

namespace ProjectSpark.EditorTools
{
    /// <summary>
    /// Read-only snapshot of solved component state. Does not re-solve,
    /// edit bindings, alter scene objects, or assume that every detected
    /// electrical component contributes current to the selected source.
    /// </summary>
    public static class SparkSourceCurrentAudit
    {
        private const string MenuPath = "Tools/Project Spark/Diagnostics/Audit Solved Source Current";

        [MenuItem(MenuPath)]
        private static void Run()
        {
            if (!Application.isPlaying)
            {
                Debug.LogWarning("[SPARK CURRENT AUDIT] Enter Play Mode, complete the circuit, allow a solver result, then run this command.");
                return;
            }

            SparkPowerSupply[] sources = UnityEngine.Object.FindObjectsByType<SparkPowerSupply>(FindObjectsSortMode.None);
            SparkElectricalComponent[] components = UnityEngine.Object.FindObjectsByType<SparkElectricalComponent>(FindObjectsSortMode.None);
            Array.Sort(sources, (a, b) => string.CompareOrdinal(PathOf(a != null ? a.transform : null), PathOf(b != null ? b.transform : null)));
            Array.Sort(components, (a, b) => string.CompareOrdinal(PathOf(a != null ? a.transform : null), PathOf(b != null ? b.transform : null)));

            var sb = new StringBuilder(8192);
            sb.AppendLine("========== PROJECT SPARK / SOLVED SOURCE CURRENT AUDIT ==========");
            sb.AppendLine("Read-only snapshot of component ElectricalState; no graph-membership claims.");
            sb.AppendLine("Time.unscaledTime=" + Time.unscaledTime.ToString("F3") + " | Sources=" + sources.Length + " | Components=" + components.Length);
            if (sources.Length == 0) sb.AppendLine("WARNING: No active SparkPowerSupply instances found.");

            foreach (SparkPowerSupply source in sources)
            {
                if (source == null) continue;
                var state = source.ElectricalState;
                sb.AppendLine("\n--- SOURCE ---");
                sb.AppendLine(PathOf(source.transform));
                sb.AppendLine("Enabled=" + source.ElectricalEnabled + " | OutputActive=" + source.IsOutputActive +
                    " | State=" + source.State + " | CurrentLimit=" + FormatA(source.CurrentLimit) +
                    " | IsCurrentLimited=" + source.IsCurrentLimited);
                sb.AppendLine("Solved V=" + FormatV(state.Voltage) + " | I=" + FormatA(state.Current) +
                    " | |I|=" + FormatmA(Mathf.Abs(state.Current)) + " | P=" + FormatW(state.Power) +
                    " | Conduction=" + state.Conduction);
            }

            sb.AppendLine("\n--- RESISTORS (all active objects, including storage) ---");
            int resistorCount = 0;
            foreach (SparkElectricalComponent component in components)
            {
                SparkResistor resistor = component as SparkResistor;
                if (resistor == null) continue;
                resistorCount++;
                SparkShelfItem item = resistor.GetComponentInParent<SparkShelfItem>();
                string shelfStatus = item == null ? "not a shelf item" :
                    "IsAtWorkbench=" + item.IsAtWorkbench +
                    " | IsMoving=" + item.IsMoving +
                    " | CurrentSlot=" + (item.CurrentSlot != null ? item.CurrentSlot.SlotId : "null") +
                    " | OccupantMatches=" + (item.CurrentSlot != null && item.CurrentSlot.Occupant == item);
                var st = resistor.ElectricalState;
                sb.AppendLine(PathOf(resistor.transform));
                sb.AppendLine("  Enabled=" + resistor.ElectricalEnabled + " | Burned=" + resistor.IsBurned +
                    " | V=" + FormatV(st.Voltage) + " | I=" + FormatA(st.Current) +
                    " | |I|=" + FormatmA(Mathf.Abs(st.Current)) + " | Conduction=" + st.Conduction);
                sb.AppendLine("  " + shelfStatus);
            }
            if (resistorCount == 0) sb.AppendLine("No active SparkResistor objects found.");

            sb.AppendLine("\n--- OTHER ELECTRICAL COMPONENTS (do NOT sum blindly) ---");
            foreach (SparkElectricalComponent component in components)
            {
                if (component == null || component is SparkPowerSupply || component is SparkResistor) continue;
                var st = component.ElectricalState;
                sb.AppendLine(PathOf(component.transform) + " | Type=" + component.GetType().Name +
                    " | Enabled=" + component.ElectricalEnabled + " | V=" + FormatV(st.Voltage) +
                    " | I=" + FormatA(st.Current) + " | |I|=" + FormatmA(Mathf.Abs(st.Current)) +
                    " | Conduction=" + st.Conduction);
            }

            sb.AppendLine("\n--- LEVEL 11 THEORETICAL CHECK (illustrative, not graph proof) ---");
            sb.AppendLine("At measured ~4.995 V and ONLY 330 ohm, 220 ohm, 470 ohm in parallel:");
            const float referenceVoltage = 4.995f;
            float expected = referenceVoltage / 330f + referenceVoltage / 220f + referenceVoltage / 470f;
            sb.AppendLine("Expected total=" + FormatmA(expected) +
                "; compare with source |I| above. Unexpected difference suggests extra load or solver topology/accounting, not necessarily a bug in the source component.");
            sb.AppendLine("IMPORTANT: Only the circuit graph/solver stamp list can prove which components contribute to the source current.");
            sb.AppendLine("========== END CURRENT AUDIT ==========");
            Debug.Log(sb.ToString());
        }

        [MenuItem(MenuPath, true)]
        private static bool Validate() => EditorApplication.isPlaying;

        private static string PathOf(Transform transform)
        {
            if (transform == null) return "<null>";
            string path = transform.name;
            while (transform.parent != null)
            {
                transform = transform.parent;
                path = transform.name + "/" + path;
            }
            return path;
        }
        private static string FormatA(float x) => x.ToString("F6") + " A";
        private static string FormatmA(float x) => (x * 1000f).ToString("F3") + " mA";
        private static string FormatV(float x) => x.ToString("F6") + " V";
        private static string FormatW(float x) => x.ToString("F6") + " W";
    }
}
#endif
