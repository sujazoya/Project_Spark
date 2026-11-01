#if UNITY_EDITOR
using System;
using System.Linq;
using UnityEditor;
using UnityEngine;
using ProjectSpark.Electrical;
using ProjectSpark.Gameplay;

namespace ProjectSpark.EditorTools
{
    [CustomEditor(typeof(SparkLCDMeter))]
    [CanEditMultipleObjects]
    public sealed class SparkLCDMeterEditor : UnityEditor.Editor
    {
        private SerializedProperty terminalA;
        private SerializedProperty terminalB;
        private SerializedProperty measurementMode;
        private SerializedProperty voltageInputResistanceOhms;
        private SerializedProperty inlineShuntResistanceOhms;
        private bool showAllModes;
        private bool showDiagnostics = true;
        private bool showAdvanced;

        private static readonly string[] ModeHelp =
        {
            "Display A-to-B voltage and current of the component chosen in SparkLCDMeterDisplay. The component current is NOT measured through the LCD leads.",
            "High-impedance voltage measurement between terminals A (+) and B (-).",
            "Ground-referenced display: connect B to circuit ground. The LCD cannot infer ground from a floating connection.",
            "Measure the voltage drop between A and B; place both leads across the component.",
            "LOW-RESISTANCE SHUNT. Break the branch and insert A-to-B in SERIES. Never wire A/B across a supply or live voltage node pair.",
            "Read the solved current of the component configured in SparkLCDMeterDisplay; LCD leads do not carry that component current.",
            "Read the configured power source's solved current; confirm the source's current sign convention.",
            "Sum the configured component current magnitudes; not a generic signed Kirchhoff current measurement.",
            "Display a configured component's nominal resistance, not an in-circuit ohmmeter reading.",
            "Compute R1 + R2 using two configured component values; doesn't verify physical topology.",
            "Compute equivalent R1 || R2 using two configured component values; doesn't verify physical topology.",
            "Sum configured component voltage-drop magnitudes; polarity may not be preserved.",
            "Read solved power of the configured component.",
            "Sum solved power of two configured components, not necessarily whole-circuit power.",
            "Read divider output at A-to-B and compare with configured supply voltage. Typically A=VOUT, B=GND.",
            "Compare the solver's electrical graph node IDs for terminals A and B. SAME NODE does not mean low resistance through a component."
        };

        private void OnEnable()
        {
            terminalA = serializedObject.FindProperty("terminalA");
            terminalB = serializedObject.FindProperty("terminalB");
            measurementMode = serializedObject.FindProperty("measurementMode");
            voltageInputResistanceOhms = serializedObject.FindProperty("voltageInputResistanceOhms");
            inlineShuntResistanceOhms = serializedObject.FindProperty("inlineShuntResistanceOhms");
            EditorApplication.update += RepaintIfPlaying;
        }

        private void OnDisable() => EditorApplication.update -= RepaintIfPlaying;
        private double nextRepaint;
        private void RepaintIfPlaying()
        {
            if (!Application.isPlaying || EditorApplication.timeSinceStartup < nextRepaint) return;
            nextRepaint = EditorApplication.timeSinceStartup + 0.25d;
            Repaint();
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            DrawHeader();
            EditorGUILayout.Space(6);
            DrawTerminals();
            EditorGUILayout.Space(6);
            DrawModes();
            EditorGUILayout.Space(6);
            DrawImpedance();
            EditorGUILayout.Space(6);
            DrawLiveStatus();
            EditorGUILayout.Space(6);
            DrawTools();
            EditorGUILayout.Space(6);
            DrawBaseProperties();
            serializedObject.ApplyModifiedProperties();
        }

        private void DrawHeader()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            GUILayout.Label("PROJECT SPARK   /   UNIVERSAL LCD METER", EditorStyles.boldLabel);
            EditorGUILayout.LabelField("Two real terminals • 16 measurement modes • Verified solver readings", EditorStyles.miniLabel);
            EditorGUILayout.EndVertical();
        }

        private void DrawTerminals()
        {
            GUILayout.Label("01   ELECTRICAL TERMINALS", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(terminalA, new GUIContent("Terminal A (+)"));
            EditorGUILayout.PropertyField(terminalB, new GUIContent("Terminal B (-)"));
            if (serializedObject.isEditingMultipleObjects) return;
            var meter = (SparkLCDMeter)target;
            if (meter.TerminalA == null || meter.TerminalB == null)
                EditorGUILayout.HelpBox("Both SparkTerminal references are required. Assign the actual LCD wire terminals, not resistor references.", MessageType.Warning);
            else if (meter.TerminalA == meter.TerminalB)
                EditorGUILayout.HelpBox("A and B reference the same terminal. Assign two distinct terminal objects.", MessageType.Error);
            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Select A") && meter.TerminalA != null)
                    Selection.activeObject = meter.TerminalA.gameObject;
                if (GUILayout.Button("Select B") && meter.TerminalB != null)
                    Selection.activeObject = meter.TerminalB.gameObject;
            }
        }

        private static bool ConfirmInlineMode(SparkLCDMeasurementMode next)
        {
            if (next != SparkLCDMeasurementMode.InlineCurrent) return true;
            return EditorUtility.DisplayDialog(
                "Inline Current — Safety Check",
                "Inline Current inserts a low-resistance shunt between LCD terminals A and B. " +
                "Before applying this mode, disconnect parallel voltage wiring and place the LCD " +
                "IN SERIES with the branch. Never place it directly across a powered supply.\n\n" +
                "Is the LCD already wired in series?",
                "Yes — Apply Inline Current", "Cancel");
        }

        private void DrawModes()
        {
            GUILayout.Label("02   MEASUREMENT MODE", EditorStyles.boldLabel);
            bool multi = serializedObject.isEditingMultipleObjects;
            SparkLCDMeter meter = multi ? null : (SparkLCDMeter)target;
            SparkLCDMeasurementMode oldMode = meter != null
                ? meter.MeasurementMode
                : SparkLCDMeasurementMode.AutoVoltageAndComponentCurrent;

            // A multi-object Play Mode edit would bypass each meter's
            // SetMeasurementMode impedance invalidation. Disallow that case.
            using (new EditorGUI.DisabledScope(multi && Application.isPlaying))
            {
                EditorGUI.BeginChangeCheck();
                EditorGUILayout.PropertyField(measurementMode, new GUIContent("Mode"));
                if (EditorGUI.EndChangeCheck() && meter != null && Application.isPlaying)
                {
                    SparkLCDMeasurementMode next =
                        (SparkLCDMeasurementMode)measurementMode.intValue;

                    // SetMeasurementMode must see the ORIGINAL mode, not the
                    // pending SerializedProperty write. Preserve edits made
                    // earlier in this Inspector pass (e.g. terminal assignments).
                    measurementMode.intValue = (int)oldMode;
                    serializedObject.ApplyModifiedProperties();
                    if (ConfirmInlineMode(next))
                    {
                        meter.SetMeasurementMode(next);
                        EditorUtility.SetDirty(meter);
                    }
                    serializedObject.Update();
                }
            }

            SparkLCDMeasurementMode mode =
                (SparkLCDMeasurementMode)measurementMode.intValue;
            int index = (int)mode;
            EditorGUILayout.HelpBox(
                index >= 0 && index < ModeHelp.Length
                    ? ModeHelp[index] : "Unknown mode.",
                mode == SparkLCDMeasurementMode.InlineCurrent
                    ? MessageType.Warning : MessageType.Info);

            if (multi && Application.isPlaying)
                EditorGUILayout.HelpBox(
                    "To change mode during Play Mode, select one LCD at a time so its solver topology is updated.",
                    MessageType.Info);

            showAllModes = EditorGUILayout.Foldout(
                showAllModes, "Show all 16 mode purposes", true);
            if (showAllModes)
            {
                EditorGUI.indentLevel++;
                foreach (SparkLCDMeasurementMode entry in
                         Enum.GetValues(typeof(SparkLCDMeasurementMode)))
                {
                    int helpIndex = (int)entry;
                    EditorGUILayout.LabelField(entry.ToString(), EditorStyles.boldLabel);
                    EditorGUILayout.LabelField(
                        helpIndex >= 0 && helpIndex < ModeHelp.Length
                            ? ModeHelp[helpIndex] : "No description available.",
                        EditorStyles.wordWrappedMiniLabel);
                    GUILayout.Space(3);
                }
                EditorGUI.indentLevel--;
            }

            if (meter != null && Application.isPlaying)
            {
                bool quickVoltage = false;
                bool quickAuto = false;
                bool quickNext = false;
                using (new EditorGUILayout.HorizontalScope())
                {
                    quickVoltage = GUILayout.Button("Voltage");
                    quickAuto = GUILayout.Button("Auto");
                    quickNext = GUILayout.Button("Next");
                }
                if (quickVoltage || quickAuto || quickNext)
                {
                    serializedObject.ApplyModifiedProperties();
                    SparkLCDMeasurementMode next = quickVoltage
                        ? SparkLCDMeasurementMode.DCVoltage
                        : quickAuto ? SparkLCDMeasurementMode.AutoVoltageAndComponentCurrent
                        : (SparkLCDMeasurementMode)(
                            ((int)meter.MeasurementMode + 1) %
                            Enum.GetValues(typeof(SparkLCDMeasurementMode)).Length);
                    if (ConfirmInlineMode(next))
                    {
                        meter.SetMeasurementMode(next);
                        EditorUtility.SetDirty(meter);
                    }
                    serializedObject.Update();
                }
                EditorGUILayout.HelpBox(
                    "Inline Current requires series wiring. Choosing it triggers a safety confirmation.",
                    MessageType.None);
            }
        }

        private void DrawImpedance()
        {
            showAdvanced = EditorGUILayout.Foldout(
                showAdvanced, "03   ELECTRICAL IMPEDANCE", true);
            if (!showAdvanced) return;

            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(
                voltageInputResistanceOhms, new GUIContent("Voltmeter input (Ω)"));
            EditorGUILayout.PropertyField(
                inlineShuntResistanceOhms, new GUIContent("Current shunt (Ω)"));

            if (EditorGUI.EndChangeCheck())
            {
                serializedObject.ApplyModifiedProperties();
                if (Application.isPlaying)
                {
                    var solver = UnityEngine.Object.FindFirstObjectByType<SparkElectricalSolver>();
                    if (solver != null)
                        solver.MarkTopologyDirty();
                    else
                        Debug.LogWarning("[LCD Inspector] Impedance changed but no active solver found.");
                }
                serializedObject.Update();
            }
            EditorGUILayout.HelpBox(
                "Voltage modes have high input impedance; Inline Current has a " +
                "low-resistance shunt. Play Mode impedance edits mark the solver dirty. " +
                "Run a solve to apply them before relying on readings.",
                MessageType.Info);
        }

        private void DrawLiveStatus()
        {
            showDiagnostics = EditorGUILayout.Foldout(showDiagnostics, "04   SOLVER / LIVE READINGS", true);
            if (!showDiagnostics || serializedObject.isEditingMultipleObjects) return;
            var meter = (SparkLCDMeter)target;
            using (new EditorGUI.DisabledScope(true))
            {
                EditorGUILayout.Toggle("Applied solve", meter.HasAppliedSolve);
                EditorGUILayout.FloatField("Measured A - B (V)", meter.LastTerminalVoltage);
                EditorGUILayout.FloatField("Meter shunt current (A)", meter.LastMeterCurrent);
                EditorGUILayout.FloatField("Effective resistance (Ω)", meter.MeterResistanceOhms);
                EditorGUILayout.Toggle("Stamped", meter.HasStampedResistance);
                EditorGUILayout.FloatField("Last stamped resistance (Ω)", meter.LastStampedResistance);
                EditorGUILayout.Toggle("Topology refresh needed", meter.NeedsTopologyRefresh);
            }
            if (meter.NeedsTopologyRefresh)
                EditorGUILayout.HelpBox("The current meter impedance differs from the last stamped value. Refresh topology before relying on displayed readings.", MessageType.Warning);
            var solver = UnityEngine.Object.FindFirstObjectByType<SparkElectricalSolver>();
            if (solver == null)
                EditorGUILayout.HelpBox("No active SparkElectricalSolver found. Place the LCD under the solver's search root and assign the solver definition asset.", MessageType.Warning);
            else
            {
                bool discovered = solver.ElectricalComponents != null && solver.ElectricalComponents.Contains(meter);
                EditorGUILayout.LabelField("Solver discovered LCD", discovered ? "YES" : "NO");
                EditorGUILayout.LabelField("Solver", solver.IsDirty ? "DIRTY" : solver.LatestSolveResult != null ? "HAS RESULT" : "NO RESULT");
                if (Application.isPlaying && !discovered)
                    EditorGUILayout.HelpBox("LCD not discovered by solver. Check Component Search Root and ensure the LCD is beneath it.", MessageType.Warning);
            }
        }

        private void DrawTools()
        {
            GUILayout.Label("05   SETUP / DIAGNOSTICS", EditorStyles.boldLabel);
            if (serializedObject.isEditingMultipleObjects) return;
            var meter = (SparkLCDMeter)target;
            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Ping LCD")) EditorGUIUtility.PingObject(meter.gameObject);
                if (GUILayout.Button("Find Display"))
                {
                    var display = meter.GetComponent<SparkLCDMeterDisplay>();
                    if (display == null) display = meter.GetComponentInChildren<SparkLCDMeterDisplay>(true);
                    if (display != null) Selection.activeObject = display;
                    else Debug.LogWarning("[LCD Inspector] SparkLCDMeterDisplay not found on LCD or children.", meter);
                }
            }
            using (new EditorGUI.DisabledScope(!Application.isPlaying))
            {
                if (GUILayout.Button("Mark Topology Dirty + Solve Now (Play Mode)"))
                {
                    var solver = UnityEngine.Object.FindFirstObjectByType<SparkElectricalSolver>();
                    if (solver != null) { solver.MarkTopologyDirty(); solver.SolveNow(); }
                    else Debug.LogWarning("[LCD Inspector] No SparkElectricalSolver found.", meter);
                }
            }
            EditorGUILayout.HelpBox("This inspector does not create wiring, change mode to InlineCurrent automatically, or alter other components.", MessageType.None);
        }

        private void DrawBaseProperties()
        {
            var baseFoldout = EditorPrefs.GetBool("SparkLCDMeterEditor.BaseFoldout", false);
            baseFoldout = EditorGUILayout.Foldout(baseFoldout, "06   INHERITED COMPONENT FIELDS", true);
            EditorPrefs.SetBool("SparkLCDMeterEditor.BaseFoldout", baseFoldout);
            if (!baseFoldout) return;
            var iterator = serializedObject.GetIterator();
            bool enterChildren = true;
            while (iterator.NextVisible(enterChildren))
            {
                enterChildren = false;
                if (iterator.name == "m_Script" || iterator.name == "terminalA" ||
                    iterator.name == "terminalB" || iterator.name == "measurementMode" ||
                    iterator.name == "voltageInputResistanceOhms" || iterator.name == "inlineShuntResistanceOhms") continue;
                EditorGUILayout.PropertyField(iterator, true);
            }
        }
    }
}
#endif
