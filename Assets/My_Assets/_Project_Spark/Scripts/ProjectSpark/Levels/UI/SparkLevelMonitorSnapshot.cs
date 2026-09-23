using System;
using UnityEngine;

namespace ProjectSpark.Gameplay
{
    /// <summary>
    /// Lightweight presentation snapshot for Project Spark's in-world
    /// diagnostic monitor.
    ///
    /// Contains no UI references and performs no simulation work.
    /// All electrical values and validation state originate from
    /// LevelGamePlayManager.
    /// </summary>
    [Serializable]
    public struct SparkLevelMonitorSnapshot
    {
        public string levelId;
        public string levelName;
        public string levelDescription;

        public SparkLevelValidationState state;
        public string stateText;
        public string message;

        public int satisfiedTargets;
        public int totalTargets;
        public float progress01;

        public string sourceName;
        public float voltage;
        public float current;
        public float power;

        public bool sourceAvailable;
        public bool returnClosed;
        public bool faultActive;

        public string faultText;
        public string affectedTerminalName;
        public string affectedTargetName;

        // ============================================================
        // CREATE SNAPSHOT
        // ============================================================

        public static SparkLevelMonitorSnapshot FromManager(
            LevelGamePlayManager manager)
        {
            SparkLevelMonitorSnapshot snapshot =
                new SparkLevelMonitorSnapshot();

            if (manager == null)
            {
                snapshot.state =
                    SparkLevelValidationState.InvalidConfiguration;

                snapshot.stateText =
                    "NO MANAGER";

                snapshot.message =
                    "Level gameplay manager is not assigned.";

                snapshot.faultActive = true;

                snapshot.faultText =
                    snapshot.message;

                return snapshot;
            }

            SparkLevelDefinition level =
                manager.ActiveLevel;

            SparkLevelValidationResult result =
                manager.ValidationResult;

            // --------------------------------------------------------
            // LEVEL
            // --------------------------------------------------------

            snapshot.levelId =
                level != null
                    ? level.LevelId
                    : string.Empty;

            snapshot.levelName =
                level != null
                    ? level.DisplayName
                    : "NO LEVEL";

            snapshot.levelDescription =
                level != null
                    ? level.Description
                    : string.Empty;

            // --------------------------------------------------------
            // VALIDATION
            // --------------------------------------------------------

            snapshot.state =
                result.state;

            snapshot.stateText =
                GetStateText(
                    result.state);

            snapshot.message =
                string.IsNullOrWhiteSpace(
                    result.message)
                    ? "System ready."
                    : result.message;

            // --------------------------------------------------------
            // TARGET PROGRESS
            // --------------------------------------------------------

            snapshot.satisfiedTargets =
                Mathf.Max(
                    0,
                    result.satisfiedTargets);

            snapshot.totalTargets =
                Mathf.Max(
                    0,
                    result.totalTargets);

            snapshot.progress01 =
                snapshot.totalTargets > 0
                    ? Mathf.Clamp01(
                        (float)snapshot.satisfiedTargets /
                        snapshot.totalTargets)
                    : 0f;

            // --------------------------------------------------------
            // ELECTRICAL DATA
            //
            // IMPORTANT:
            // No recalculation occurs here.
            // The evaluator remains authoritative.
            // --------------------------------------------------------

            snapshot.sourceName =
                string.IsNullOrWhiteSpace(
                    result.activeSourceName)
                    ? "NONE"
                    : result.activeSourceName;

            snapshot.voltage =
                result.targetVoltage;

            snapshot.current =
                result.targetCurrent;

            snapshot.power =
                result.targetPower;

            // --------------------------------------------------------
            // CONNECTION STATE
            // --------------------------------------------------------

            snapshot.sourceAvailable =
                manager.HasValidPowerSource;

            snapshot.returnClosed =
                manager.ClosedReturn;

            // --------------------------------------------------------
            // FAULT
            // --------------------------------------------------------

            snapshot.faultActive =
                result.IsFault;

            snapshot.faultText =
                result.IsFault
                    ? snapshot.message
                    : "NONE";

            // --------------------------------------------------------
            // AFFECTED OBJECTS
            // --------------------------------------------------------

            snapshot.affectedTerminalName =
                result.affectedTerminal != null
                    ? result.affectedTerminal.name
                    : string.Empty;

            snapshot.affectedTargetName =
                result.affectedTarget != null
                    ? result.affectedTarget.DisplayName
                    : string.Empty;

            return snapshot;
        }

        // ============================================================
        // STATE TEXT
        // ============================================================

        public static string GetStateText(
            SparkLevelValidationState state)
        {
            switch (state)
            {
                case SparkLevelValidationState.Playing:
                    return "RUNNING";

                case SparkLevelValidationState.Completed:
                    return "COMPLETE";

                case SparkLevelValidationState.WrongConnection:
                    return "WRONG CONNECTION";

                case SparkLevelValidationState.ShortCircuit:
                    return "SHORT CIRCUIT";

                case SparkLevelValidationState.TargetShort:
                    return "TARGET SHORT";

                case SparkLevelValidationState.Overload:
                    return "OVERLOAD";

                case SparkLevelValidationState.SolverFault:
                    return "SOLVER FAULT";

                case SparkLevelValidationState.InvalidConfiguration:
                    return "CONFIGURATION FAULT";

                default:
                    return "UNKNOWN";
            }
        }

        // ============================================================
        // DISPLAY HELPERS
        // ============================================================

        public string GetProgressText()
        {
            return
                $"{satisfiedTargets}/{totalTargets}";
        }

        public string GetElectricalText()
        {
            return
                $"V {voltage:0.00} V   " +
                $"I {current:0.000} A   " +
                $"P {power:0.000} W";
        }

        public string GetConnectionText()
        {
            return
                $"SOURCE {(sourceAvailable ? "OK" : "WAIT")}   " +
                $"RETURN {(returnClosed ? "CLOSED" : "OPEN")}";
        }
    }
}
