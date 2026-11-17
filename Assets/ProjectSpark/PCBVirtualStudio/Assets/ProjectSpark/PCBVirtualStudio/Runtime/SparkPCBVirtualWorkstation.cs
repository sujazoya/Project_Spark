using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ProjectSpark.PCBVirtualStudio
{
    // AAA-style UI shell: inspection and visual repairs only until a board authority is attached.
    [DisallowMultipleComponent]
    public sealed class SparkVirtualPCBWorkstation : MonoBehaviour
    {
        [SerializeField] private bool demoMode = true;
        [SerializeField] private MonoBehaviour repairAuthorityBehaviour;
        [SerializeField] private SparkVirtualTool activeTool = SparkVirtualTool.Inspect;
        [SerializeField] private int activeGroup;
        [SerializeField] private SparkVirtualPCBTarget selectedTarget;
        [SerializeField] private SparkPCBTouchVFX visualEffects;
        [SerializeField] private SparkPCBWaveformGraphic waveform;
        [SerializeField] private Text toolNameText;
        [SerializeField] private Text toolHintText;
        [SerializeField] private Text targetTitleText;
        [SerializeField] private Text targetInfoText;
        [SerializeField] private Text measuredReadingText;
        [SerializeField] private Text activityLogText;
        [SerializeField] private Text footerText;
        [SerializeField] private Text statusText;
        [SerializeField] private Text sessionTitleText;
        [SerializeField] private List<SparkPCBToolButton> toolButtons = new List<SparkPCBToolButton>();
        [SerializeField] private List<SparkPCBToolTab> tabs = new List<SparkPCBToolTab>();
        [SerializeField] private List<SparkVirtualPCBTarget> targets = new List<SparkVirtualPCBTarget>();

        private readonly Queue<string> recentEvents = new Queue<string>();
        public SparkVirtualTool ActiveTool => activeTool;
        public bool DemoMode => demoMode;
        public event Action<SparkVirtualPCBTarget, SparkVirtualTool> RepairActionAccepted;

        public void Configure(SparkPCBTouchVFX fx, SparkPCBWaveformGraphic graph, Text toolName, Text hint,
            Text targetTitle, Text targetInfo, Text measurement, Text log, Text footer,
            Text status, Text session)
        {
            visualEffects = fx;
            waveform = graph;
            toolNameText = toolName;
            toolHintText = hint;
            targetTitleText = targetTitle;
            targetInfoText = targetInfo;
            measuredReadingText = measurement;
            activityLogText = log;
            footerText = footer;
            statusText = status;
            sessionTitleText = session;
            RefreshView();
        }

        public void AddToolButton(SparkPCBToolButton button)
        {
            if (button != null && !toolButtons.Contains(button)) toolButtons.Add(button);
        }

        public void AddTab(SparkPCBToolTab tab)
        {
            if (tab != null && !tabs.Contains(tab)) tabs.Add(tab);
        }

        public void AddTarget(SparkVirtualPCBTarget target)
        {
            if (target != null && !targets.Contains(target)) targets.Add(target);
        }

        public void BindAuthority(MonoBehaviour authorityBehaviour)
        {
            repairAuthorityBehaviour = authorityBehaviour;
            demoMode = authorityBehaviour == null;
            RefreshView();
        }

        public void SetDemoMode(bool value)
        {
            demoMode = value;
            RefreshView();
        }

        private void Awake()
        {
            RefreshView();
            SelectToolGroup(activeGroup);
            SelectTool(activeTool);
        }

        public void SelectToolGroup(int group)
        {
            activeGroup = Mathf.Clamp(group, 0, 2);
            for (int i = 0; i < tabs.Count; i++)
                if (tabs[i] != null) tabs[i].Refresh(i == activeGroup);
            foreach (var b in toolButtons)
            {
                if (b == null) continue;
                b.gameObject.SetActive(b.Group == activeGroup);
                b.Refresh(b.Tool == activeTool);
            }
            RefreshView();
        }

        public void SelectTool(SparkVirtualTool tool)
        {
            activeTool = tool;
            foreach (var b in toolButtons)
                if (b != null) b.Refresh(b.Tool == activeTool);
            RefreshView();
            Log("TOOL ARMED  > " + SparkPCBToolCatalog.DisplayName(tool));
        }

        public void PreviewTarget(SparkVirtualPCBTarget target)
        {
            if (target != null && footerText != null)
                footerText.text = "HOVER  /  " + target.TargetId + "  /  " + target.DisplayName + "  /  " + target.NetId;
        }

        public void UseSelectedToolOn(SparkVirtualPCBTarget target)
        {
            UseSelectedToolOn(target, null);
        }

        // Optional world-space contact point for colliders on an existing physical PCB.
        public void UseSelectedToolOn(SparkVirtualPCBTarget target, Vector3? physicalPoint)
        {
            if (target == null) return;
            selectedTarget = target;
            SparkRepairToolResult result;
            ISparkPCBRepairAuthority authority = repairAuthorityBehaviour as ISparkPCBRepairAuthority;
            if (demoMode)
            {
                result = target.ApplyLocalDemo(activeTool);
            }
            else if (authority == null)
            {
                result = new SparkRepairToolResult(false, false,
                    "No ISparkPCBRepairAuthority assigned. Live board topology unchanged.");
            }
            else if (SparkPCBToolCatalog.IsDiagnostic(activeTool))
            {
                string reading;
                bool ok = authority.TryReadProbe(target, activeTool, out reading);
                result = new SparkRepairToolResult(ok, false, reading ?? "No measurement returned.");
            }
            else
            {
                string message;
                bool ok = authority.TryApplyRepair(target, activeTool, out message);
                if (ok) target.MarkAuthoritativeActionAccepted();
                result = new SparkRepairToolResult(ok, ok, message ?? "Operation completed by authority.");
            }

            if (visualEffects != null)
            {
                RectTransform uiTarget = target.GetComponent<RectTransform>();
                if (uiTarget != null) visualEffects.PlayAt(uiTarget, activeTool, result.accepted);
                else visualEffects.PlayAtWorld(physicalPoint ?? target.transform.position, null,
                    activeTool, result.accepted);
            }
            if (measuredReadingText != null) measuredReadingText.text = result.message;
            if (statusText != null) statusText.text = result.accepted ? "ACTION ACCEPTED" : "ACTION REJECTED";
            Log((result.accepted ? "OK" : "NO") + "  " + target.TargetId + "  /  " + result.message);
            RefreshView(false);
            if (result.accepted) RepairActionAccepted?.Invoke(target, activeTool);
        }

        private void RefreshView(bool resetReadout = true)
        {
            if (toolNameText != null) toolNameText.text = SparkPCBToolCatalog.DisplayName(activeTool);
            if (toolHintText != null) toolHintText.text = SparkPCBToolCatalog.Guidance(activeTool);
            if (sessionTitleText != null)
                sessionTitleText.text = demoMode ? "DEMO TRAINING  /  NOT CIRCUIT-CERTIFIED" :
                    (repairAuthorityBehaviour is ISparkPCBRepairAuthority ? "LIVE BOARD AUTHORITY CONNECTED" : "LIVE MODE  /  AUTHORITY MISSING");
            if (selectedTarget != null)
            {
                if (targetTitleText != null) targetTitleText.text = selectedTarget.DisplayName + "  /  " + selectedTarget.TargetId;
                if (targetInfoText != null) targetInfoText.text =
                    "NET  " + selectedTarget.NetId + "    |    TYPE  " + selectedTarget.Kind +
                    "\nCONDITION (DEMO)  " + selectedTarget.DemoDefect +
                    "\nFLUX  " + (selectedTarget.FluxApplied ? "YES" : "NO") +
                    "   HEAT PREP  " + (selectedTarget.PreparedForRemoval ? "YES" : "NO");
                if (resetReadout && measuredReadingText != null)
                    measuredReadingText.text = demoMode ? selectedTarget.DemoInspectionText(activeTool) : "Select instrument and tap the target.";
                if (waveform != null) waveform.SetDemoSignal(selectedTarget.DemoFrequency, .44f);
            }
            else
            {
                if (targetTitleText != null) targetTitleText.text = "NO TARGET SELECTED";
                if (targetInfoText != null) targetInfoText.text = "Tap a board pad, trace or component to begin inspection.";
                if (resetReadout && measuredReadingText != null) measuredReadingText.text = "READY  /  Virtual instrument idle";
            }
            foreach (var t in targets)
                if (t != null) t.Refresh(t == selectedTarget);
        }

        private void Log(string message)
        {
            if (recentEvents.Count >= 5) recentEvents.Dequeue();
            recentEvents.Enqueue(message);
            if (activityLogText != null) activityLogText.text = string.Join("\n", recentEvents.ToArray());
        }
    }
}
