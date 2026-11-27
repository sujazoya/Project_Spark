using System;
using System.IO;
using System.Linq;
using System.Text;
using ProjectSpark.Gameplay;
using UnityEditor;
using UnityEngine;

namespace ProjectSpark.SparkV5.Pro.Editor
{
    /// <summary>Read-only rig and mechanical sleeve diagnostic. Never creates connections.</summary>
    public static class SparkTelescopicAAAEditor
    {
        private const string ReportPath = "Logs/SparkTelescopicArms/v18_4_Precision_Diagnostic.txt";

        [Serializable]
        private sealed class WiringSnapshot
        {
            public string avatar, level, layout, engineeringMode, authorization, step, phase, movement, lastFailure;
            public string bindingStatus, portState, lastDirectorStatus;
            public bool handBound, rightArmResting, hasGrip, hasLiveCable, registeredConnection;
            public Vector3 shoulder, elbow, wrist, grip, cableEnd;
            public float maximumReach, gripError, cableRestLength, cableRouteLength, cableExtension;
        }

        [MenuItem("Project Spark/Spark V5 Pro/AAA Laboratory/Export Native Wiring Diagnostics")]
        private static void ExportNativeWiring()
        {
            const string destination = "Logs/SparkTelescopicArms/NativeWiringDiagnostics.json";
            var directors = UnityEngine.Object.FindObjectsByType<SparkNativeWireDirector>(FindObjectsSortMode.None);
            if (directors.Length != 1)
            {
                Debug.LogWarning("Expected one active native wire director for this diagnostic.");
                return;
            }

            var director = directors[0];
            var character = director.GetComponent<SparkProCharacter>();
            var executive = director.GetComponent<SparkProExecutive>();
            var hub = director.GetComponent<SparkEngineeringControlHub>();
            var performer = director.GetComponent<SparkNativeWireCinematic>();
            var bridge = director.GetComponent<SparkTelescopicNativeWireBridge>();
            var arms = director.GetComponent<SparkTelescopicArms>();
            var port = director.GetComponent<SparkAINativeSignalWirePort>();
            var flight = director.GetComponent<SparkV5FlightController>();
            var manager = executive ? executive.ActiveLevelManager : null;
            var layout = manager && manager.LayoutLoader ? manager.LayoutLoader.ActiveLayout : null;
            var snapshot = new WiringSnapshot
            {
                avatar = director.name,
                level = manager && manager.ActiveLevel ? manager.ActiveLevel.LevelId : "",
                layout = layout ? layout.name : "",
                engineeringMode = hub ? hub.Mode.ToString() : "unavailable",
                authorization = hub ? hub.Phase.ToString() : "unavailable",
                step = hub ? hub.CurrentAction : "",
                phase = performer ? performer.Stage.ToString() : "unavailable",
                movement = flight && flight.IsFlying ? "Flight" :
                    character && character.NavigationAgent && character.NavigationAgent.velocity.sqrMagnitude > .001f
                        ? "Walking" : "Stationary",
                lastFailure = performer ? performer.LastError : "",
                handBound = bridge && bridge.IsConfigured,
                rightArmResting = arms && arms.IsRightArmResting,
                maximumReach = arms ? arms.MaximumReach : 0,
                gripError = arms ? arms.RightGripError : 0,
                portState = port ? port.IsWiring ? port.UsesPlayerPointer ? "Pointer" : "Physical" : "Idle" : "Missing",
                registeredConnection = port && manager && manager.CircuitSystem && port.LastAIConnectionId != 0 &&
                    manager.CircuitSystem.HasConnection(port.LastAIConnectionId),
                lastDirectorStatus = director.LastStatus
            };
            if (character)
            {
                snapshot.shoulder = character.GetAnchor("Shoulder_R")?.position ?? default;
                snapshot.elbow = character.GetAnchor("Elbow_R")?.position ?? default;
                snapshot.wrist = character.GetAnchor("Wrist_R")?.position ?? default;
            }
            if (arms)
                snapshot.hasGrip = arms.TryGetGripPosition(SparkTelescopicArms.ManipulatorSide.Right, out snapshot.grip);
            if (performer)
                snapshot.hasLiveCable = performer.TryMeasureHeldCable(out snapshot.cableEnd,
                    out snapshot.cableRestLength, out snapshot.cableRouteLength, out snapshot.cableExtension);
            if (layout && layout.SceneBindings && manager.ActiveLevel)
            {
                var plans = Resources.LoadAll<SparkNativeWiringPlan>("SparkNativeWirePlans")
                    .Where(plan => plan.IsApproved && plan.LevelNumber == manager.ActiveLevel.LevelNumber &&
                        string.Equals(plan.LayoutName, layout.name.Replace("(Clone)", "").Trim(), StringComparison.Ordinal))
                    .ToArray();
                snapshot.bindingStatus = plans.Length == 1
                    ? string.Join("\n", plans[0].Wires.Select(step =>
                        step.fromId + " " + layout.SceneBindings.ResolveLiveTerminal(layout, step.fromId, out _) +
                        " -> " + step.toId + " " + layout.SceneBindings.ResolveLiveTerminal(layout, step.toId, out _)))
                    : "No unique approved layout plan";
            }
            Directory.CreateDirectory(Path.GetDirectoryName(destination));
            File.WriteAllText(destination, JsonUtility.ToJson(snapshot, true));
            Debug.Log("Native wiring diagnostics written to " + Path.GetFullPath(destination), director);
        }

        [MenuItem("Project Spark/Spark V5 Pro/AAA Laboratory/42 - Diagnose Original-Hand Precision v18.4")]
        private static void DiagnosePrecision() { Diagnose(); }

        [MenuItem("Project Spark/Spark V5 Pro/AAA Laboratory/41 - Diagnose Telescopic AAA v18.3")]
        private static void Diagnose()
        {
            var report = new StringBuilder(4096);
            report.AppendLine("SPARK v18.4 — ORIGINAL HAND PRECISION + MECHANICAL LINK DIAGNOSTIC");
            report.AppendLine("Read-only: does not move Spark or manipulate the electrical circuit.");
            report.AppendLine("PlayMode=" + EditorApplication.isPlaying);
            var controllers = UnityEngine.Object.FindObjectsByType<SparkTelescopicArms>(
                FindObjectsInactive.Include, FindObjectsSortMode.None);
            report.AppendLine("Controllers=" + controllers.Length);
            foreach (SparkTelescopicArms controller in controllers)
            {
                if (!controller) continue;
                report.AppendLine("Avatar=" + controller.name);
                report.AppendLine("Status=" + controller.Status);
                var character = controller.GetComponent<SparkProCharacter>();
                if (!character) { report.AppendLine("FAIL: SparkProCharacter missing"); continue; }
                var transforms = character.GetComponentsInChildren<Transform>(true);
                int wrists = 0, thumbs = 0, tips = 0, stages = 0, collars = 0, legacyProxies = 0;
                foreach (Transform item in transforms)
                {
                    if (!item) continue;
                    string name = item.name;
                    if (name == "Wrist_R" || name == "Wrist_L") wrists++;
                    if (name == "Thumb_R" || name == "Thumb_L" ||
                        name == "ThumbTip_R" || name == "ThumbTip_L") thumbs++;
                    if (name == "FingerTip0_R" || name == "FingerTip0_L") tips++;
                    if (name.StartsWith("Spark AAA Telescopic ", StringComparison.Ordinal))
                    {
                        if (name.Contains(" Stage ")) stages++;
                        if (name.Contains(" Sliding Collar ")) collars++;
                    }
                    if (name.StartsWith("Spark v18.2 Joint Bridge", StringComparison.Ordinal) ||
                        name.StartsWith("Spark Telescopic ", StringComparison.Ordinal))
                        legacyProxies++;
                }
                report.AppendLine("Existing real wrists=" + wrists + " thumbs=" + thumbs + " fingertip-0=" + tips);
                report.AppendLine("Runtime structural stages=" + stages + " joint collars=" + collars);
                report.AppendLine("Legacy proxy structures=" + legacyProxies);
                report.AppendLine("Grip contact threshold=" + controller.ContactTolerance.ToString("F4") + "m");
                report.AppendLine("Right contact verified=" + controller.RightContactConfirmed);
                report.AppendLine("Left contact verified=" + controller.LeftContactConfirmed);
                report.AppendLine("Active mechanical shaft stages=" + controller.ActiveStageCount);
                report.AppendLine("Right grip-to-target error=" + controller.RightGripError.ToString("F4") + "m");
                report.AppendLine("Left grip-to-target error=" + controller.LeftGripError.ToString("F4") + "m");
                if (wrists < 2) report.AppendLine("WARNING: Original wrist rigs missing");
                if (legacyProxies > 0) report.AppendLine("WARNING: Legacy geometry still loaded; reload the level");
                if (controller.TryGetGripPosition(SparkTelescopicArms.ManipulatorSide.Right, out Vector3 right))
                    report.AppendLine("Right actual grip world=" + right.ToString("F4"));
                if (controller.TryGetGripPosition(SparkTelescopicArms.ManipulatorSide.Left, out Vector3 left))
                    report.AppendLine("Left actual grip world=" + left.ToString("F4"));
            }
            string path = Path.GetFullPath(ReportPath);
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            File.WriteAllText(path, report.ToString());
            Debug.Log("[Spark AAA Telescopic v18.4] Diagnostic: " + path + "\n" + report);
        }
    }
}
