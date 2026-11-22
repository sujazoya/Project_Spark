using System;
using ProjectSpark.Circuit;
using UnityEngine;
using ProjectSpark.UI.VisualTeaching;

namespace ProjectSpark.Gameplay.SiliconWorkshop
{
    public sealed partial class SparkL129Workshop : MonoBehaviour, ISparkAILevelTeachingFocus
    {
        public const string Revision = "H610-EDU-1";
        public const string CheckpointKey = "ProjectSpark.Level129.H610-EDU-1";
        public static string Proof(int index) => $"L129_Proof_{index + 1}";

        [Serializable] public sealed class Site
        {
            public string name;
            public Transform anchor;
            public SparkSelectable selectable;
            public GameObject model;
        }

        [Serializable] public sealed class Channel
        {
            public SparkTerminal pin;
            public SparkSwitch contact;
            public SparkResistor load;
        }

        public SparkLevelLayout layout;
        public SparkLevelSceneBindings bindings;
        public Transform board, substrate;
        public Site[] service, faultSites, interfaceSites;
        public Transform[] railSites;
        public GameObject cover, retention, nvmeScrew;
        public Renderer[] debugLamps;
        public SparkPowerSupply serviceVerifier;
        public SparkPowerSupply[] supplies;
        public SparkTerminal ground;
        public Channel[] rails, ports;
        public Channel[] dimms;
        public Channel storage, fan, boost;
        public SparkSwitch vrm;
        public Transform[] mountPoints;

        private LevelGamePlayManager manager;
        private SparkSelectionController selection;
        private readonly System.Collections.Generic.List<SparkCircuitConnection> connections = new();
        private int serviceIndex, pickedPart, rotation, mounts, installed, checkedRails, checkedPorts, pickedRail,
            pickedPort, pickedMode, repaired, pickedSpare, selectedFault = -1, identified = -1;
        private bool opened, seated, dimmLatched, standby, running, postPassed, faultProbed, candidateRepair, faultLatched,
            guardObserved, fanRestored, heatQualified, freshReboot, topologyDirty, resumed;
        private int activeFault = -1;
        private Coroutine trial;
        private float temperature = 28f, peak = 28f;
        private string message = "Open the socket guard. Select the matching module, align the key and secure each footprint.";
        private string detail = "";
        private PostSignal post;
        private readonly int[] mountOrder = { 0, 3, 1, 2 };
        public static readonly string[] ServiceParts = { "LGA training CPU", "Tower cooler", "DDR5 DIMM", "M.2 NVMe", "DDR4 distractor" };
        public static readonly string[] RailNames = { "5V standby", "EPS input", "CPU core", "DDR5", "I/O" };
        public static readonly string[] PortNames = { "PCIe ×16", "PCIe ×1", "M.2", "SATA", "HDMI", "VGA", "USB", "LAN" };
        public static readonly string[] PortModes = { "Lane test", "Block CRC", "Digital pattern", "Analog levels", "USB echo", "Frame echo" };
        public static readonly int[] CorrectPortModes = { 0, 0, 1, 1, 2, 3, 4, 5 };
        private static readonly string[] FaultNames = { "PQ04", "DDR5_2", "M2_1" };
        private static readonly string[] SpareNames = { "VRM module", "DDR5 contact", "M.2 contact" };
        private static readonly float[] Nominal = { 5f, 12f, 1.10f, 1.10f, 3.30f };
        private static readonly float[] RailMinimumAmps = { .020f, .060f, .070f, .025f, .018f };
        private static readonly int[] PartForSite = { 0, 1, 2, 2, 3 };
        private static readonly int[] KeyForSite = { 0, 0, 0, 0, 0 };

        public enum PostSignal { Off, CPU, DRAM, VGA, BOOT, Ready }
        public int Stage { get; private set; }
        public int Installed => installed;
        public int Repaired => repaired;
        public int RailCount => CountBits(checkedRails, 5);
        public int PortCount => CountBits(checkedPorts, 8);
        public int Fault => activeFault;
        public int Diagnostic => identified;
        public bool IsBusy => trial != null;
        public bool Running => running;
        public bool Standby => standby;
        public bool Tripped => faultLatched;
        public bool PostPassed => postPassed;
        public bool ProtectionObserved => guardObserved;
        public bool FreshReboot => freshReboot;
        public float Peak => peak;
        public PostSignal Post => post;
        public string Detail => detail;
        public string Message => message;
        public bool HasCheckpoint => !resumed && PlayerPrefs.HasKey(CheckpointKey);

        public bool TryGetTeachingFocus(out Transform target, out string instruction)
        {
            instruction = Message;
            target = Stage < 6 ? Focus() : null;
            return target && isActiveAndEnabled;
        }

        private void Start()
        {
            var scene = gameObject.scene;
            foreach (var root in scene.GetRootGameObjects())
            {
                if (!manager) manager = root.GetComponentInChildren<LevelGamePlayManager>(true);
                if (!selection) selection = root.GetComponentInChildren<SparkSelectionController>(true);
            }
            if (!manager || !manager.CircuitSystem || !manager.ElectricalSolver) { enabled = false; return; }
            manager.CircuitSystem.TopologyChanged += MarkTopology;
            if (selection) selection.SelectionChanged += Selected;
            InitializeView();
            RefreshPanel();
        }

        private void OnDisable()
        {
            if (manager && manager.CircuitSystem) manager.CircuitSystem.TopologyChanged -= MarkTopology;
            if (selection)
            {
                selection.SelectionChanged -= Selected;
                if (selection.SelectedObject && selection.SelectedObject.transform.IsChildOf(transform))
                    selection.ClearSelection();
            }
            RestoreView();
        }

        private void MarkTopology() => topologyDirty = true;

        private void Update()
        {
            if (!topologyDirty || !manager) return;
            topologyDirty = false;
            if (!HasForeignWire()) return;
            AbortTrial();
            ShutdownMain(false);
            checkedRails = checkedPorts = 0;
            postPassed = false;
            faultProbed = candidateRepair = false;
            activeFault = identified = selectedFault = -1;
            heatQualified = guardObserved = freshReboot = false;
            if (Stage > 1 && Stage < 6)
            {
                for (int i = 1; i < Stage; i++) bindings.ClearRuntimeComponentBinding(Proof(i));
                Stage = 1;
                manager.QueueEvaluation();
            }
            Reject("A foreign wire touched the training bench. Remove it and repeat qualification.");
        }

        private void Selected(SparkSelectable oldSite, SparkSelectable newSite)
        {
            if (!newSite) return;
            if (Stage == 0)
            {
                int index = Array.FindIndex(service, site => site.selectable == newSite);
                if (index >= 0) ChooseSite(index);
            }
            else if (Stage == 4 && activeFault >= 0)
            {
                int index = Array.FindIndex(faultSites, site => site.selectable == newSite);
                if (index >= 0) SetRepairSite(index);
            }
            else if (Stage >= 3 && activeFault < 0)
            {
                int index = Array.FindIndex(interfaceSites, site => site.selectable == newSite);
                if (index >= 0) ChoosePort(index);
            }
        }

        public bool OpenGuard()
        {
            if (Stage != 0 || standby || opened || (installed & 1) != 0) return Reject("Isolate power and open the socket guard before fitting the processor.");
            opened = true;
            cover.SetActive(false);
            return Note("Socket exposed. Align the keyed CPU to the pin-one triangle.");
        }

        public void ChooseSite(int index)
        {
            if (Stage != 0 || index < 0 || index >= service.Length) return;
            if (serviceIndex != index) DiscardSeat();
            serviceIndex = index;
            if (selection && selection.SelectedObject != service[index].selectable)
                selection.Select(service[index].selectable);
            RefreshPanel();
        }

        public void ChoosePart(int index)
        {
            if (Stage != 0 || index < 0 || index >= ServiceParts.Length) return;
            if (pickedPart != index) DiscardSeat();
            pickedPart = index;
            RefreshPanel();
        }

        public void RotatePart()
        {
            if (Stage != 0) return;
            DiscardSeat();
            rotation = (rotation + 90) % 360;
            RefreshPanel();
        }

        public bool Seat()
        {
            int bit = 1 << serviceIndex;
            if (Stage != 0 || standby || (installed & bit) != 0 || pickedPart != PartForSite[serviceIndex])
                return Reject("The selected part does not belong on this free footprint. Check the slot and memory key.");
            if (rotation != KeyForSite[serviceIndex])
                return Reject("The key marks do not align. Rotate before seating; forcing the module will not fit.");
            if ((serviceIndex == 0 && !opened) || (serviceIndex == 1 && (installed & 1) == 0))
                return Reject("Open the socket guard, secure the CPU and then mount the cooler.");
            seated = true;
            service[serviceIndex].model.SetActive(true);
            mounts = 0;
            dimmLatched = false;
            return Note(serviceIndex == 1 ? "Cooler seated. Fasten opposite corners in sequence 1, 4, 2, 3." :
                serviceIndex is 2 or 3 ? "DIMM seated. Close both retention clips before confirming." :
                serviceIndex == 4 ? "Drive seated. Install the M.2 standoff screw." : "Key and seating verified. Secure the retention frame.");
        }

        public bool Tighten(int index)
        {
            if (Stage != 0 || serviceIndex != 1 || !seated || mounts >= 4) return Reject("Seat the cooler before tightening its four corners.");
            if (index != mountOrder[mounts])
            {
                mounts = 0;
                return Reject("Uneven clamp pressure. Start again and tighten opposite corners 1, 4, 2, 3.");
            }
            mounts++;
            return Note($"Cooler cross-clamp {mounts}/4.");
        }

        public bool LatchDimm()
        {
            if (Stage != 0 || serviceIndex is not (2 or 3) || !seated) return Reject("Seat the keyed DDR5 module before closing its clips.");
            dimmLatched = true;
            return Note("Both DDR5 retention clips are locked.");
        }

        public bool Secure()
        {
            if (Stage != 0 || standby || !seated || (installed & (1 << serviceIndex)) != 0)
                return Reject("A correctly seated, isolated footprint is required.");
            if ((serviceIndex == 1 && mounts != 4) || (serviceIndex is 2 or 3 && !dimmLatched))
                return Reject("Secure the cross-clamp or close the DIMM latches first.");
            SetInstalled(serviceIndex, true);
            seated = false;
            if (installed == 31)
            {
                Award();
                SaveCheckpoint();
                return Note("Assembly secured. Apply standby and probe 5V before requesting main power.");
            }
            int next = Array.FindIndex(service, site => !site.model.activeSelf);
            if (next >= 0) ChooseSite(next);
            return Note("Footprint secured. Line up the next service part.");
        }

        public bool Remove()
        {
            if (Stage != 0 || standby || (installed & (1 << serviceIndex)) == 0 ||
                (serviceIndex == 0 && (installed & 2) != 0)) return Reject("Remove only an isolated fitted part. Remove the cooler before the CPU.");
            SetInstalled(serviceIndex, false);
            seated = false;
            mounts = 0;
            return Note("Module removed while isolated.");
        }

        private void DiscardSeat()
        {
            if (!seated) return;
            service[serviceIndex].model.SetActive(false);
            seated = false;
        }

        private void SetInstalled(int index, bool value)
        {
            if (value) installed |= 1 << index;
            else installed &= ~(1 << index);
            service[index].model.SetActive(value);
            if (index == 0 && retention) retention.SetActive(value);
            if (index == 4) nvmeScrew.SetActive(value);
        }

        public bool Resume()
        {
            if (Stage != 0 || installed != 0 || !HasCheckpoint) return Reject("Resume is available from an empty, isolated bench.");
            if (!int.TryParse(PlayerPrefs.GetString(CheckpointKey), out var count) || count is < 0 or > 3)
                return Reject("The saved record does not match this bench revision.");
            resumed = opened = true;
            repaired = count;
            cover.SetActive(false);
            for (int index = 0; index < service.Length; index++) SetInstalled(index, true);
            Award();
            return Note($"Restored assembly and {repaired}/3 verified repairs. Begin fresh electrical qualification.");
        }

        private void SaveCheckpoint()
        {
            if (installed != 31 || Stage == 6) return;
            PlayerPrefs.SetString(CheckpointKey, repaired.ToString(System.Globalization.CultureInfo.InvariantCulture));
            PlayerPrefs.Save();
        }

        private bool Award()
        {
            if (Stage >= 6 || !bindings.SetRuntimeComponentBinding(Proof(Stage), serviceVerifier)) return false;
            Stage++;
            if (Stage == 6) { PlayerPrefs.DeleteKey(CheckpointKey); PlayerPrefs.Save(); }
            manager.QueueEvaluation();
            return true;
        }

        private static int CountBits(int mask, int total)
        {
            int count = 0;
            for (int i = 0; i < total; i++) count += (mask >> i) & 1;
            return count;
        }

        private bool Note(string text)
        {
            message = text;
            RefreshPanel();
            return true;
        }

        private bool Reject(string text)
        {
            Note(text);
            return false;
        }
    }
}


