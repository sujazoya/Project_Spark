using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace ProjectSpark.PCBVirtualStudio
{
    [DisallowMultipleComponent]
    public sealed class SparkVirtualPCBTarget : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [SerializeField] private string targetId = "TP01";
        [SerializeField] private string displayName = "TEST PAD";
        [SerializeField] private string netId = "NET_GND";
        [SerializeField] private SparkPCBTargetKind kind = SparkPCBTargetKind.TestPad;
        [SerializeField] private SparkBoardDefect demoDefect = SparkBoardDefect.None;
        [SerializeField] private float demoVoltage = 3.3f;
        [SerializeField] private float demoTemperature = 31f;
        [SerializeField] private float demoFrequency = 256f;
        [SerializeField] private bool fluxApplied;
        [SerializeField] private bool hotAirPrepared;
        [SerializeField] private bool maskApplied;
        [SerializeField] private bool scanned;
        [SerializeField] private bool touched;
        [SerializeField] private bool visuallyResolved;
        [SerializeField] private SparkVirtualPCBWorkstation workstation;
        [SerializeField] private Graphic background;
        [SerializeField] private Text statusText;
        [SerializeField] private Text titleText;

        public string TargetId => targetId;
        public string DisplayName => displayName;
        public string NetId => netId;
        public SparkPCBTargetKind Kind => kind;
        public SparkBoardDefect DemoDefect => demoDefect;
        public float DemoVoltage => demoVoltage;
        public float DemoTemperature => demoTemperature;
        public float DemoFrequency => demoFrequency;
        public bool FluxApplied => fluxApplied;
        public bool PreparedForRemoval => hotAirPrepared;
        public bool HasSolderMask => maskApplied;
        public bool Scanned => scanned;
        public bool VisuallyResolved => visuallyResolved;

        public void Configure(string id, string title, string net, SparkPCBTargetKind targetKind,
            SparkBoardDefect fault, float voltage, float temperature, float frequency,
            SparkVirtualPCBWorkstation owner, Graphic graphic, Text titleLabel, Text statusLabel)
        {
            targetId = id;
            displayName = title;
            netId = net;
            kind = targetKind;
            demoDefect = fault;
            demoVoltage = voltage;
            demoTemperature = temperature;
            demoFrequency = frequency;
            workstation = owner;
            background = graphic;
            titleText = titleLabel;
            statusText = statusLabel;
            if (titleText != null) titleText.text = title;
            Refresh(false);
        }

        private void Awake()
        {
            var button = GetComponent<Button>();
            if (button != null)
            {
                button.onClick.RemoveListener(OnTargetPressed);
                button.onClick.AddListener(OnTargetPressed);
            }
        }

        private void OnDestroy()
        {
            var button = GetComponent<Button>();
            if (button != null) button.onClick.RemoveListener(OnTargetPressed);
        }

        public void SetWorkstation(SparkVirtualPCBWorkstation owner) { workstation = owner; }

        public void OnTargetPressed()
        {
            if (workstation != null) workstation.UseSelectedToolOn(this);
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (workstation != null) workstation.PreviewTarget(this);
        }

        public void OnPointerExit(PointerEventData eventData) { }

        // Demonstration only: never substitute this for SparkElectricalSolver or board netlist updates.
        public SparkRepairToolResult ApplyLocalDemo(SparkVirtualTool tool)
        {
            touched = true;
            if (SparkPCBToolCatalog.IsDiagnostic(tool))
            {
                scanned |= tool == SparkVirtualTool.FaultScanner || tool == SparkVirtualTool.Microscope;
                Refresh(true);
                return new SparkRepairToolResult(true, false, DemoInspectionText(tool));
            }

            if (tool == SparkVirtualTool.Flux)
            {
                fluxApplied = true;
                Refresh(true);
                return new SparkRepairToolResult(true, false, "Flux applied to " + targetId + ". Ready for solder/reflow.");
            }
            if (tool == SparkVirtualTool.HotAir)
            {
                hotAirPrepared = true;
                Refresh(true);
                return new SparkRepairToolResult(true, false, "Controlled hot-air preparation on " + targetId + ".");
            }
            if (tool == SparkVirtualTool.SolderMask)
            {
                if (demoDefect != SparkBoardDefect.None)
                    return new SparkRepairToolResult(false, false, "Repair underlying defect before applying UV solder mask.");
                maskApplied = true;
                Refresh(true);
                return new SparkRepairToolResult(true, false, "UV solder mask applied on " + targetId + ".");
            }

            bool repaired = false;
            switch (tool)
            {
                case SparkVirtualTool.Solder:
                case SparkVirtualTool.Reflow:
                    repaired = fluxApplied && (demoDefect == SparkBoardDefect.ColdJoint || demoDefect == SparkBoardDefect.OpenPad);
                    break;
                case SparkVirtualTool.Desolder:
                    repaired = demoDefect == SparkBoardDefect.SolderBridge;
                    break;
                case SparkVirtualTool.TracePatch:
                    repaired = demoDefect == SparkBoardDefect.OpenTrace;
                    break;
                case SparkVirtualTool.ViaRepair:
                    repaired = demoDefect == SparkBoardDefect.OpenVia;
                    break;
                case SparkVirtualTool.ReplaceComponent:
                    repaired = (demoDefect == SparkBoardDefect.FailedComponent || demoDefect == SparkBoardDefect.WrongValue || demoDefect == SparkBoardDefect.OverTemperature) && hotAirPrepared;
                    break;
                case SparkVirtualTool.Clean:
                    repaired = demoDefect == SparkBoardDefect.Contamination;
                    if (!repaired && fluxApplied)
                    {
                        fluxApplied = false;
                        Refresh(true);
                        return new SparkRepairToolResult(true, false, "Flux residue cleaned; existing defect remains unchanged.");
                    }
                    if (repaired) fluxApplied = false;
                    break;
                case SparkVirtualTool.BGAReball:
                    repaired = demoDefect == SparkBoardDefect.BGABallFault && fluxApplied && hotAirPrepared;
                    break;
                case SparkVirtualTool.Calibrate:
                    repaired = demoDefect == SparkBoardDefect.CalibrationDrift;
                    break;
            }

            if (repaired)
            {
                demoDefect = SparkBoardDefect.None;
                visuallyResolved = true;
                hotAirPrepared = false;
                Refresh(true);
                return new SparkRepairToolResult(true, true, "DEMO REPAIR: " + targetId + " visual defect resolved. NOT circuit-certified.");
            }
            Refresh(true);
            return new SparkRepairToolResult(false, false,
                "Action not valid here. Inspect " + targetId + ", or prepare with flux / hot air if required.");
        }

        public string DemoInspectionText(SparkVirtualTool tool)
        {
            string prefix = "DEMO / " + targetId + " / " + netId + ": ";
            switch (tool)
            {
                case SparkVirtualTool.Multimeter:
                    return prefix + demoVoltage.ToString("F3") + " V (illustrative, NOT live solver).";
                case SparkVirtualTool.Continuity:
                    return prefix + ((demoDefect == SparkBoardDefect.OpenTrace || demoDefect == SparkBoardDefect.OpenVia || demoDefect == SparkBoardDefect.OpenPad) ? "OPEN path" :
                        (demoDefect == SparkBoardDefect.SolderBridge ? "SHORT SUSPECTED" : "Continuity expected")) + " (demo).";
                case SparkVirtualTool.Oscilloscope:
                case SparkVirtualTool.LogicAnalyzer:
                    return prefix + "illustrative signal " + demoFrequency.ToString("F1") + " Hz. No actual probe binding.";
                case SparkVirtualTool.ThermalCamera:
                    return prefix + demoTemperature.ToString("F1") + " C virtual thermal spot.";
                case SparkVirtualTool.NetTracer:
                    return prefix + "net highlighted for visual guidance. No topology changed.";
                case SparkVirtualTool.FaultScanner:
                case SparkVirtualTool.Microscope:
                    return prefix + "simulated condition: " + demoDefect + ".";
                default:
                    return prefix + displayName + " (" + kind + "), simulated condition: " + demoDefect + ".";
            }
        }

        public void MarkAuthoritativeActionAccepted()
        {
            // Do not directly change defect flags here; an electrical adapter owns true fault state.
            touched = true;
            Refresh(true);
        }

        public void Refresh(bool selected)
        {
            if (background != null)
            {
                Color baseColor = demoDefect == SparkBoardDefect.None
                    ? new Color(0.050f, 0.18f, 0.235f, 0.98f)
                    : new Color(0.19f, 0.12f, 0.095f, 0.98f);
                if (selected) baseColor = new Color(0.10f, 0.27f, 0.34f, 1f);
                background.color = baseColor;
            }
            if (statusText != null)
                statusText.text = demoDefect == SparkBoardDefect.None
                    ? (visuallyResolved ? "DEMO FIXED" : "READY")
                    : (scanned ? demoDefect.ToString().ToUpperInvariant() : "CHECK");
        }
    }
}
