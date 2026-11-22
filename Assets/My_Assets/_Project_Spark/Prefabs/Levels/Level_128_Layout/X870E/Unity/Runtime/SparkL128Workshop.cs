using System;
using ProjectSpark.Circuit;
using UnityEngine;
using ProjectSpark.UI.VisualTeaching;

namespace ProjectSpark.Gameplay.AorusWorkshop
{
    public sealed partial class SparkL128Workshop : MonoBehaviour, ISparkAILevelTeachingFocus
    {
        public const string Revision = "X870E-EDU-1";
        public const string CheckpointKey = "ProjectSpark.Level128.X870E-EDU-1";
        public static string Proof(int index) => $"L128_Proof_{index + 1}";

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
        public GameObject retention, socketLever, vrmArmor;
        public GameObject[] driveModels, driveArmor;
        public Renderer[] debugLamps;
        public SparkPowerSupply serviceVerifier;
        public SparkPowerSupply[] supplies;
        public SparkTerminal ground;
        public Channel[] rails, ports;
        public Channel[] dimms;
        public Channel[] storageSlots;
        public Channel fan, boost, upperGpuLanes;
        public SparkSwitch vrm;

        private LevelGamePlayManager manager;
        private SparkSelectionController selection;
        private readonly System.Collections.Generic.List<SparkCircuitConnection> connections = new();
        private int serviceIndex, pickedPart, rotation, mounts, installed, checkedRails, checkedPorts, pickedRail,
            pickedPort, pickedMode, repaired, pickedSpare, selectedFault = -1, identified = -1;
        private bool opened, seated, dimmLatched, standby, running, postPassed, faultProbed, candidateRepair, faultLatched,
            guardObserved, fanRestored, heatQualified, freshReboot, topologyDirty, resumed;
        private int activeFault = -1;
        private int driveMask, laneScenarios, gpuWidth, coldBoots;
        private bool regulators, inspectedFault;
        private Coroutine trial;
        private float temperature = 28f, peak = 28f;
        private string message = "Open the socket guard. Select the matching module, align the key and secure each footprint.";
        private string detail = "";
        private PostSignal post;
        private readonly int[] mountOrder = { 0, 3, 1, 2 };
        public static readonly string[] ServiceParts = { "AM5 training CPU", "AM5 tower cooler", "DDR5 DIMM", "M.2 NVMe", "DDR4 distractor" };
        public static readonly string[] RailNames = { "5V standby", "EPS A", "CPU core", "DDR5", "I/O", "EPS B", "Power good" };
        public static readonly string[] PortNames = { "GPU PCIe", "M.2 A · CPU", "M.2 B · CPU", "M.2 C · CPU", "M.2 D · chipset" };
        public static readonly string[] PortModes = { "×16 expected", "×8 expected" };
        private static readonly string[] FaultNames = { "VRM_Q03", "DDR5_A2", "M2B_CPU" };
        private static readonly string[] SpareNames = { "VRM module", "DDR5 contact", "M.2 contact" };
        private static readonly float[] Nominal = { 5f, 12f, 1.10f, 1.10f, 3.30f, 12f, 3.30f };
        private static readonly float[] RailMinimumAmps = { .020f, .060f, .070f, .025f, .018f, .047f, .006f };
        private const int AllRails = 127, AllLinks = 31;
        private static readonly int[] PartForSite = { 0, 1, 2, 2, 3 };
        private static readonly int[] KeyForSite = { 0, 0, 0, 0, 0 };

        public enum PostSignal { Off, CPU, DRAM, VGA, BOOT, Ready }
        public int Stage { get; private set; }
        public int Installed => installed;
        public int Repaired => repaired;
        public int RailCount => CountBits(checkedRails, 7);
        public int PortCount => CountBits(checkedPorts, 5);
        public int DriveMask => driveMask;
        public int LaneScenarios => laneScenarios;
        public int GpuWidth => gpuWidth;
        public int ColdBoots => coldBoots;
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
            CloseAccess();
            foreach (var spreader in driveArmor) spreader.SetActive(true);
            checkedRails = checkedPorts = 0;
            postPassed = false;
            faultProbed = candidateRepair = false;
            activeFault = identified = selectedFault = -1;
            laneScenarios = coldBoots = 0;
            heatQualified = guardObserved = fanRestored = freshReboot = faultLatched = false;
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
            retention.SetActive(false);
            socketLever.SetActive(false);
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
            if (serviceIndex == 4 && driveArmor[0].activeSelf)
                return Reject("Lift the M.2 A heat spreader before seating the boot drive.");
            seated = true;
            service[serviceIndex].model.SetActive(true);
            mounts = 0;
            dimmLatched = false;
            return Note(serviceIndex == 1 ? "Cooler seated. Fasten opposite corners in sequence 1, 4, 2, 3." :
                serviceIndex is 2 or 3 ? "DIMM seated. Close both retention clips before confirming." :
                serviceIndex == 4 ? "Drive seated. Secure the M.2 retention tab and reinstall the heat spreader." : "Key and seating verified. Secure the retention frame.");
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
            if (index == 0)
            {
                retention.SetActive(value);
                socketLever.SetActive(value);
            }
            if (index == 4)
            {
                if (value) driveMask |= 1;
                else driveMask &= ~1;
                driveArmor[0].SetActive(value);
            }
        }

        public bool Resume()
        {
            if (Stage != 0 || installed != 0 || !HasCheckpoint) return Reject("Resume is available from an empty, isolated bench.");
            string[] record = PlayerPrefs.GetString(CheckpointKey).Split(':');
            if (record.Length != 2 || !int.TryParse(record[0], out var count) || count is < 0 or > 3 ||
                !int.TryParse(record[1], out int configuration) || configuration is < 1 or > 15 || (configuration & 1) == 0)
                return Reject("The saved record does not match this bench revision.");
            resumed = opened = true;
            repaired = count;
            for (int index = 0; index < service.Length; index++) SetInstalled(index, true);
            driveMask = configuration;
            for (int slot = 1; slot < driveModels.Length; slot++)
                driveModels[slot].SetActive((driveMask & (1 << slot)) != 0);
            Award();
            return Note($"Restored assembly and {repaired}/3 verified repairs. Begin fresh electrical qualification.");
        }

        private void SaveCheckpoint()
        {
            if (installed != 31 || Stage == 6) return;
            PlayerPrefs.SetString(CheckpointKey, FormattableString.Invariant($"{repaired}:{driveMask}"));
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


