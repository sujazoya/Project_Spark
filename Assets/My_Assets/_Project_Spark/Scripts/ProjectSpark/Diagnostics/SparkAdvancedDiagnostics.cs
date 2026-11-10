using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using ProjectSpark.Electrical;
using ProjectSpark.Gameplay;

namespace ProjectSpark.Diagnostics
{
    // Observes existing results. Never solves, completes a level, or changes wiring.
    [DisallowMultipleComponent]
    public sealed class SparkAdvancedDiagnostics : MonoBehaviour
    {
        [SerializeField] private LevelGamePlayManager manager;
        [SerializeField] private SparkLevelLayoutLoader loader;
        [SerializeField] private bool visible = true;
        [SerializeField, Min(.1f)] private float refreshSeconds = .3f;
        private SparkElectricalSolver subscribed;
        private SparkLevelSceneBindings bindings;
        private UnityEngine.Object phaseLayout;
        private readonly StringBuilder report = new StringBuilder();
        private readonly List<string> findings = new List<string>();
        private string snapshot = "Waiting for level systems...", exportStatus = "";
        private float nextRefresh, peakVoltage, backupStartVoltage, backupLowest, ledSince = -1, longestLED;
        private bool sawCharge, sawBackup, previousBackup;
        private int solves;
        private Vector2 scroll;
        private Rect window = new Rect(16, 60, 780, 650);
        private int windowId;
        private RectTransform inputShield;

        private void Awake()
        {
            windowId = GetInstanceID();
            // Let the existing EventSystem/UI guard block world clicks behind the panel.
            var canvasObject = new GameObject("Diagnostics Input Shield", typeof(RectTransform), typeof(Canvas), typeof(GraphicRaycaster));
            canvasObject.transform.SetParent(transform, false);
            var canvas = canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay; canvas.sortingOrder = 32000;
            var image = new GameObject("Panel Bounds", typeof(RectTransform), typeof(Image));
            image.transform.SetParent(canvasObject.transform, false);
            image.GetComponent<Image>().color = Color.clear;
            inputShield = image.GetComponent<RectTransform>();
            inputShield.anchorMin = inputShield.anchorMax = inputShield.pivot = new Vector2(0, 1);
        }
        private void OnEnable() { nextRefresh = 0; }
        private void OnDisable() { Unsubscribe(); if (inputShield != null) inputShield.gameObject.SetActive(false); }
        private void LateUpdate()
        {
            if (inputShield == null) return;
            inputShield.gameObject.SetActive(true);
            inputShield.anchoredPosition = visible ? new Vector2(window.x, -window.y) : new Vector2(16, -60);
            inputShield.sizeDelta = visible ? new Vector2(window.width, window.height) : new Vector2(180, 30);
        }
        private void Unsubscribe()
        {
            if (subscribed != null) subscribed.SolveCompleted -= ObserveSolve;
            subscribed = null;
        }
        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.F8)) visible = !visible;
            if (Time.unscaledTime < nextRefresh) return;
            nextRefresh = Time.unscaledTime + Mathf.Max(.1f, refreshSeconds);
            RefreshNow();
        }
        [ContextMenu("Diagnostics / Refresh")]
        public void RefreshNow()
        {
            if (manager == null) manager = FindFirstObjectByType<LevelGamePlayManager>();
            if (loader == null) loader = FindFirstObjectByType<SparkLevelLayoutLoader>();
            var solver = manager != null ? manager.ElectricalSolver : null;
            if (solver != subscribed)
            {
                Unsubscribe(); subscribed = solver;
                if (subscribed != null) subscribed.SolveCompleted += ObserveSolve;
            }
            var active = loader != null ? loader.ActiveLayout : null;
            bindings = loader != null ? loader.ActiveBindings : null;
            if (phaseLayout != active)
            {
                phaseLayout = active;
                ResetObservations();
            }
            BuildReport();
        }
        private bool IsLevel28 => manager != null && manager.ActiveLevel != null && manager.ActiveLevel.LevelId == "level_28";
        private void ObserveSolve()
        {
            // Called after the solver publishes component state; no extra Solve call.
            if (!IsLevel28 || bindings == null || loader == null || loader.ActiveLayout != phaseLayout) return;
            solves++;
            var cap = Component("C_BACKUP_Component") as SparkCapacitor;
            var a = Component("SPDT_COM_A") as SparkSwitch;
            var b = Component("SPDT_COM_B") as SparkSwitch;
            var led = Component("led") as SparkLED;
            if (cap == null || a == null || b == null) return;
            float voltage = cap.ElectricalState.Voltage;
            if (float.IsNaN(voltage) || float.IsInfinity(voltage)) return;
            peakVoltage = Mathf.Max(peakVoltage, voltage);
            bool backup = a.IsOpen && b.IsClosed;
            sawCharge |= a.IsClosed && b.IsOpen;
            if (backup && !previousBackup)
            {
                backupStartVoltage = voltage; backupLowest = voltage; sawBackup = true;
            }
            if (backup) backupLowest = Mathf.Min(backupLowest, voltage);
            if (backup && led != null && led.IsOn)
            {
                if (ledSince < 0) ledSince = Time.unscaledTime;
                longestLED = Mathf.Max(longestLED, Time.unscaledTime - ledSince);
            }
            else ledSince = -1;
            previousBackup = backup;
        }
        private SparkElectricalComponent Component(string id)
        {
            SparkElectricalComponent c;
            return bindings != null && bindings.TryGetComponent(id, out c) ? c : null;
        }
        private SparkTerminal Terminal(string id)
        {
            SparkTerminal t;
            return bindings != null && bindings.TryGetTerminal(id, out t) ? t : null;
        }
        private bool SameNode(SparkTerminal a, SparkTerminal b)
        {
            var g = subscribed != null ? subscribed.CurrentGraph : null;
            int x, y;
            return g != null && a != null && b != null && g.TryGetNode(a, out x) && g.TryGetNode(b, out y) && x == y;
        }
        private void Finding(string severity, string message) { findings.Add("[" + severity + "] " + message); }
        private void BuildReport()
        {
            report.Clear(); findings.Clear();
            report.AppendLine("PROJECT SPARK — ADVANCED DIAGNOSTICS");
            report.AppendLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " | observation only");
            if (manager == null || manager.ActiveLevel == null)
            { snapshot = report + "\nWAITING: no active level manager/definition."; return; }
            var level = manager.ActiveLevel;
            report.AppendLine(level.DisplayName + " [" + level.LevelId + "]");
            report.AppendLine("AUTHORITATIVE LEVEL STATE: " + manager.RuntimeState);
            report.AppendLine($"Objectives: {manager.SatisfiedTargetCount}/{manager.ActiveTargetCount}");
            if (manager.IsCompleted) report.AppendLine("LEVEL COMPLETE — confirmed by the gameplay manager.");
            else if (manager.IsFailed) Finding("FAIL", "Level failed: " + manager.LastFailureReason);
            else report.AppendLine("Level is not marked complete. Observed phase history does not override its evaluator.");
            report.AppendLine("Evaluator: " + manager.LastEvaluationMessage);
            if (bindings != null && manager.SceneBindings != bindings)
                Finding("CHECK", "Manager and active layout expose different binding tables. Component/target readings may describe different objects; inspect loader/manager wiring.");
            report.AppendLine($"Power source valid={manager.HasValidPowerSource}; return={manager.ClosedReturn}; source short={manager.SourceShorted}; target short={manager.TargetShorted}; wrong connection={manager.WrongConnection}; overload={manager.Overloaded}");
            if (manager.SourceShorted || manager.TargetShorted) Finding("FAIL", "Evaluator reports a short circuit. Inspect the affected source/target net.");
            if (manager.Overloaded) Finding("FAIL", "Evaluator reports an overload. Check load values and current limits.");
            if (manager.WrongConnection) Finding("FAIL", "Evaluator reports a wrong connection. Compare the target nets with the level wiring.");
            if (!manager.HasValidPowerSource) Finding("CHECK", "Evaluator has no valid source. Check source assignment, output enable and graph membership.");
            if (!manager.ClosedReturn) Finding("CONTEXT", "Evaluator reports no source return. This can occur in an isolated backup phase or after capacitor charging; it alone does not prove a broken wire.");
            report.AppendLine("\nOBJECTIVE RESULTS (manager's latest evaluation)");
            if (manager.TargetResults != null)
                foreach (var t in manager.TargetResults)
                    report.AppendLine((t.satisfied ? "PASS " : "PENDING ") + t.displayName + " [" + t.targetId + "]");
            report.AppendLine("\nCONFIGURED OBJECTIVES (limits apply according to each target type)");
            foreach (var t in level.Targets)
            {
                if (t == null) continue;
                report.AppendLine($"{t.DisplayName}: {t.Type}; component={t.TargetComponentId}; terminal={t.TargetTerminalId}; pair={t.RequiredPositiveTerminalId} -> {t.RequiredNegativeTerminalId}");
                report.AppendLine($"  Minimum: {t.MinimumVoltage:G4} V / {t.MinimumCurrent * 1000:G4} mA / {t.MinimumPower:G4} W; enabled required={t.RequireElectricalEnabled}; conduction required={t.RequireConduction}; inverted={t.Inverted}");
                if (t.UseMaximumVoltage || t.UseMaximumCurrent || t.UseMaximumPower)
                    report.AppendLine($"  Maximum enabled V/I/P={t.UseMaximumVoltage}/{t.UseMaximumCurrent}/{t.UseMaximumPower}: {t.MaximumVoltage:G4} V / {t.MaximumCurrent * 1000:G4} mA / {t.MaximumPower:G4} W");
            }
            if (subscribed == null || subscribed.CurrentGraph == null || subscribed.LatestSolveResult == null)
                Finding("WAIT", "No solved graph/result from the manager's solver. Numerical and connectivity diagnosis unavailable.");
            else
            {
                report.AppendLine($"\nSOLVER: nodes={subscribed.CurrentGraph.NodeCount}; dirty={subscribed.IsDirty}; solving={subscribed.IsSolving}");
                report.AppendLine("Readings are the latest published component state. Dirty/solving means updates may be pending; this panel does not force a solve.");
            }
            if (IsLevel28) InspectLevel28();
            report.AppendLine("\nFINDINGS / NEXT ACTIONS");
            if (findings.Count == 0) report.AppendLine("No supported fault detected. This is not a guarantee that every project behavior is correct.");
            foreach (string f in findings) report.AppendLine(f);
            report.AppendLine("\nACTIVE LEVEL COMPONENT READINGS");
            var root = loader != null ? loader.ActiveLayout : null;
            if (root != null)
                foreach (var c in root.GetComponentsInChildren<SparkElectricalComponent>(true))
                {
                    bool discovered = false;
                    if (subscribed != null) foreach (var candidate in subscribed.ElectricalComponents) discovered |= candidate == c;
                    var s = c.ElectricalState;
                    report.AppendLine($"{c.name} ({c.GetType().Name}) | active={c.isActiveAndEnabled}, electrical={c.ElectricalEnabled}, discovered={discovered} | {s.Voltage:F4} V, {s.Current * 1000:F3} mA, {s.Conduction}");
                    if (c is SparkSwitch sw) report.AppendLine("  Contact state=" + sw.State);
                }
            snapshot = report.ToString();
        }
        private void InspectLevel28()
        {
            report.AppendLine("\nLEVEL 28 — SPDT / CAPACITOR / LED");
            if (bindings == null) { Finding("FAIL", "Active layout bindings missing."); return; }
            var a = Component("SPDT_COM_A") as SparkSwitch;
            var b = Component("SPDT_COM_B") as SparkSwitch;
            var cap = Component("C_BACKUP_Component") as SparkCapacitor;
            var led = Component("led") as SparkLED;
            foreach (string id in new[] { "SPDT_COM_A", "SPDT_COM_B", "C_BACKUP_Component", "led", "R_LED_Component", "R_BLEED_Component", "R_CHARGE_Component", "D_PROTECT_Component" })
                if (Component(id) == null) Finding("FAIL", "Missing component binding: " + id + ". Check installation and alias declarations.");
            if (a != null && b != null)
            {
                report.AppendLine($"SPDT: COM-A={a.State}; COM-B={b.State}");
                if (a == b || a.IsClosed && b.IsClosed) Finding("FAIL", "SPDT has a duplicated branch or both paths Closed. Check router assignments and competing handlers.");
                if (a.IsOpen && b.IsOpen) Finding("CHECK", "Both SPDT paths are Open. A brief break-before-make interval is normal; a persistent state is not.");
                foreach (var sw in new[] { a, b })
                {
                    if (!sw.isActiveAndEnabled || !sw.ElectricalEnabled) Finding("FAIL", sw.name + " is inactive or electrically disabled.");
                    if (subscribed != null && subscribed.CurrentGraph != null && !subscribed.CurrentGraph.ComponentTerminals.ContainsKey(sw))
                        Finding("FAIL", sw.name + " has no component entry in the current graph. Check terminal parenting and component discovery.");
                }
                if (a.InputTerminal != Terminal("SPDT_COM") || a.OutputTerminal != Terminal("SPDT_A") ||
                    b.InputTerminal != Terminal("SPDT_COM") || b.OutputTerminal != Terminal("SPDT_B"))
                    Finding("FAIL", "SPDT branch references do not match the COM/A/B bindings.");
            }
            if (cap != null)
            {
                report.AppendLine($"Capacitor: {cap.ElectricalState.Voltage:F3} V; {cap.CapacitanceFarads * 1000000:F0} uF; rating {cap.VoltageRating:F1} V");
                if (Mathf.Abs(cap.CapacitanceFarads - .0047f) > .000047f) Finding("SPEC", "Level 28 specifies 4700 uF. Installed capacitance differs; backup duration will differ.");
                if (cap.IsOverVoltage) Finding("FAIL", "Capacitor voltage exceeds its rating.");
            }
            if (led != null)
            {
                var s = led.ElectricalState;
                report.AppendLine($"LED: {s.Voltage:F4} V, {s.Current * 1000:F3} mA; IsOn={led.IsOn}; Vf={led.ForwardVoltage:F2} V; visual assigned={led.HasVisualReference}; visual active={led.IsVisualActiveInHierarchy}; direct mesh enabled={led.IsVisualRendererEnabled}");
                if (!led.HasVisualReference) Finding("FAIL", "LED Helix reference is missing. Assign its emitting visual.");
                if (led.IsOn && !led.IsVisualActiveInHierarchy) Finding("FAIL", "LED conducts but its assigned visual is inactive. Check visual ownership/lifecycle.");
                if (led.IsOn && led.HasVisualRenderer && !led.IsVisualRendererEnabled) Finding("FAIL", "LED conducts but the assigned mesh renderer is disabled.");
                var glow = led.GetComponentInChildren<SparkLEDHelixGlow>(true);
                if (glow != null) report.AppendLine($"Glow: enabled={glow.isActiveAndEnabled}; received {glow.SolvedVoltage:F3} V / {glow.SolvedCurrent * 1000:F3} mA; heat={glow.Brightness:F3}");
                if (s.Voltage < -.05f) Finding("CHECK", "LED is reverse biased in this snapshot. Check Anode/Cathode and branch polarity.");
                if (a != null && b != null && a.IsOpen && b.IsClosed && cap != null && !led.IsOn)
                {
                    if (cap.ElectricalState.Voltage <= led.ForwardVoltage) Finding("INFO", "Backup capacitor is below LED forward voltage. A dark LED is expected at this snapshot.");
                    else Finding("CHECK", "Backup selected and capacitor exceeds LED Vf, but LED does not conduct. Check the 330 ohm branch nets below; if they pass, inspect nonlinear solver/current results.");
                }
                var r = Component("R_LED_Component");
                if (r != null && SameNode(Terminal("R_LED_Output"), led.AnodeTerminal) &&
                    Mathf.Abs(Mathf.Abs(r.ElectricalState.Current) - Mathf.Abs(s.Current)) > .0002f)
                    Finding("CHECK", "330 ohm and LED current magnitudes differ by over 0.2 mA. Check extra branches on their shared net and result consistency; this alone does not prove a solver fault.");
            }
            report.AppendLine("\nEXPECTED WIRE NETS (node equality; not proof of a particular physical wire)");
            string[,] wires = {
                {"Battery_positive","SwitchInput"}, {"SwitchOutput","D_PROTECT_Anode"},
                {"D_PROTECT_Cathode","R_CHARGE_Input"}, {"R_CHARGE_Output","SPDT_A"},
                {"SPDT_COM","C_BACKUP_Positive"}, {"C_BACKUP_Negative","Battery_negative"},
                {"SPDT_B","R_LED_Input"}, {"R_LED_Output","LED_Anode"},
                {"LED_Cathode","Battery_negative"}, {"SPDT_B","R_BLEED_Input"}, {"R_BLEED_Output","Battery_negative"} };
            for (int i = 0; i < wires.GetLength(0); i++)
            {
                var x = Terminal(wires[i, 0]); var y = Terminal(wires[i, 1]);
                string state = x == null || y == null ? "UNBOUND" : subscribed == null || subscribed.CurrentGraph == null ? "WAIT" : SameNode(x, y) ? "PASS" : "NOT SAME NET";
                report.AppendLine($"{i + 1}: {state} | {wires[i, 0]} -> {wires[i, 1]}");
                if (state == "UNBOUND" || state == "NOT SAME NET") Finding("CHECK", $"Wire {i + 1}: {state}. Inspect {wires[i, 0]} and {wires[i, 1]}.");
            }
            report.AppendLine($"\nOBSERVED SINCE MONITOR RESET: solves={solves}, Charge seen={sawCharge}, Backup seen={sawBackup}, peak={peakVoltage:F3} V, latest backup start={backupStartVoltage:F3} V, drop={Mathf.Max(0, backupStartVoltage - backupLowest):F3} V, longest observed LED conduction={longestLED:F3} s");
            report.AppendLine("History is observational, sampled at solve events. It does not implement or certify the level's temporal completion rules.");
        }
        [ContextMenu("Diagnostics / Reset Observed History")]
        public void ResetObservations()
        {
            peakVoltage = backupStartVoltage = backupLowest = longestLED = 0;
            sawCharge = sawBackup = previousBackup = false; ledSince = -1; solves = 0;
        }
        [ContextMenu("Diagnostics / Copy Report")]
        public void CopyReport() { RefreshNow(); GUIUtility.systemCopyBuffer = snapshot; exportStatus = "Report copied."; }
        [ContextMenu("Diagnostics / Export Report")]
        public void ExportReport()
        {
            try
            {
                RefreshNow();
                string folder = Path.Combine(Application.persistentDataPath, "Diagnostics");
                Directory.CreateDirectory(folder);
                string path = Path.Combine(folder, "Spark-" + DateTime.Now.ToString("yyyyMMdd-HHmmss-fff") + ".txt");
                File.WriteAllText(path, snapshot); exportStatus = "Saved: " + path;
                Debug.Log(exportStatus, this);
            }
            catch (Exception e) { exportStatus = "Export failed: " + e.Message; }
        }
        private void OnGUI()
        {
            if (!visible)
            {
                if (GUI.Button(new Rect(16, 60, 180, 30), "Diagnostics [F8]")) visible = true;
                return;
            }
            window.width = Mathf.Min(900, Mathf.Max(300, Screen.width - 32));
            window.height = Mathf.Min(720, Mathf.Max(200, Screen.height - 80));
            window.x = Mathf.Clamp(window.x, 0, Mathf.Max(0, Screen.width - window.width));
            window.y = Mathf.Clamp(window.y, 0, Mathf.Max(0, Screen.height - window.height));
            window = GUI.Window(windowId, window, DrawWindow, "PROJECT SPARK | DIAGNOSTICS");
        }
        private void DrawWindow(int id)
        {
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Refresh")) RefreshNow();
            if (GUILayout.Button("Copy report")) CopyReport();
            if (GUILayout.Button("Export .txt")) ExportReport();
            if (GUILayout.Button("Reset history")) ResetObservations();
            if (GUILayout.Button("Hide [F8]")) visible = false;
            GUILayout.EndHorizontal();
            if (!string.IsNullOrEmpty(exportStatus)) GUILayout.Label(exportStatus);
            scroll = GUILayout.BeginScrollView(scroll);
            GUILayout.Label(snapshot, new GUIStyle(GUI.skin.label) { wordWrap = true, richText = false, fontSize = 13 });
            GUILayout.EndScrollView();
            GUI.DragWindow(new Rect(0, 0, window.width, 20));
        }
    }
}
