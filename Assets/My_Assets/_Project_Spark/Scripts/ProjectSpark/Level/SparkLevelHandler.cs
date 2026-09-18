using System;
using System.Collections.Generic;
using UnityEngine;

using ProjectSpark.Circuit;

namespace ProjectSpark.Gameplay
{
    [DisallowMultipleComponent]
    public sealed class SparkLevelHandler : MonoBehaviour
    {
        // =========================================================
        // CONFIGURATION
        // =========================================================

        [Header("Circuit")]
        [SerializeField]
        private SparkCircuitSystem circuitSystem;

        [Header("Power Sources")]
        [SerializeField]
        private SparkPowerSupply[] powerSupplies =
            Array.Empty<SparkPowerSupply>();

        [Header("Targets")]
        [SerializeField]
        private SparkLED[] targetLEDs =
            Array.Empty<SparkLED>();

        [Header("Electrical Validation")]
        [SerializeField, Min(0f)]
        private float minimumVoltage = 0.01f;

        [SerializeField, Min(0f)]
        private float minimumCurrent = 0.001f;

        [SerializeField, Min(0f)]
        private float maximumLEDCurrentMultiplier = 1f;

        [Header("Completion")]
        [SerializeField]
        private bool requireAllTargets = true;

        [SerializeField]
        private bool requireSupplyActive = true;

        [SerializeField]
        private bool requireConnectedCircuit = true;

        [Header("Diagnostics")]
        [SerializeField]
        private bool logStateChanges;

        // =========================================================
        // RUNTIME
        // =========================================================

        private readonly List<SparkCircuitConnection>
            connectionBuffer =
                new List<SparkCircuitConnection>(32);

        private readonly List<SparkTerminal>
            terminalBuffer =
                new List<SparkTerminal>(32);

        private readonly HashSet<SparkTerminal>
            visited =
                new HashSet<SparkTerminal>();

        private readonly Queue<SparkTerminal>
            searchQueue =
                new Queue<SparkTerminal>(64);

        private readonly HashSet<SparkElectronicObject>
            subscribedObjects =
                new HashSet<SparkElectronicObject>();

      [SerializeField]  private bool initialized;
      [SerializeField]   private bool completed;
       [SerializeField]  private bool lastEvaluationValid;
        private int evaluationVersion;

        private string lastFailureReason;

        // =========================================================
        // PUBLIC STATE
        // =========================================================

        public bool IsCompleted =>
            completed;

        public bool IsCircuitValid =>
            lastEvaluationValid;

        public int EvaluationVersion =>
            evaluationVersion;

        public string LastFailureReason =>
            lastFailureReason;

        public event Action<bool>
            CompletionChanged;

        public event Action
            CircuitEvaluated;

        // =========================================================
        // UNITY
        // =========================================================

        private void Awake()
        {
            Initialize();
        }

        private void OnEnable()
        {
            Initialize();

            SubscribeToCircuit();
            SubscribeToPowerSupplies();
            SubscribeToTargets();

            Evaluate();
        }

        private void OnDisable()
        {
            UnsubscribeFromCircuit();
            UnsubscribeFromObjects();
        }

        // =========================================================
        // INITIALIZATION
        // =========================================================

        private void Initialize()
        {
            if (initialized)
            {
                return;
            }

            if (circuitSystem == null)
            {
                circuitSystem =
                    GetComponent<SparkCircuitSystem>();
            }

            initialized = true;
        }

        // =========================================================
        // CIRCUIT EVENTS
        // =========================================================

        private void SubscribeToCircuit()
        {
            if (circuitSystem == null)
            {
                return;
            }

            circuitSystem.TopologyChanged -=
                HandleTopologyChanged;

            circuitSystem.TopologyChanged +=
                HandleTopologyChanged;

            circuitSystem.ConnectionCreated -=
                HandleConnectionChanged;

            circuitSystem.ConnectionCreated +=
                HandleConnectionChanged;

            circuitSystem.ConnectionRemoved -=
                HandleConnectionChanged;

            circuitSystem.ConnectionRemoved +=
                HandleConnectionChanged;
        }

        private void UnsubscribeFromCircuit()
        {
            if (circuitSystem == null)
            {
                return;
            }

            circuitSystem.TopologyChanged -=
                HandleTopologyChanged;

            circuitSystem.ConnectionCreated -=
                HandleConnectionChanged;

            circuitSystem.ConnectionRemoved -=
                HandleConnectionChanged;
        }

        private void HandleTopologyChanged()
        {
            Evaluate();
        }

        private void HandleConnectionChanged(
            SparkCircuitConnection connection)
        {
            Evaluate();
        }

        // =========================================================
        // DEVICE EVENTS
        // =========================================================

        private void SubscribeToPowerSupplies()
        {
            if (powerSupplies == null)
            {
                return;
            }

            for (int i = 0;
                 i < powerSupplies.Length;
                 i++)
            {
                SubscribeToObject(
                    powerSupplies[i]);
            }
        }

        private void SubscribeToTargets()
        {
            if (targetLEDs == null)
            {
                return;
            }

            for (int i = 0;
                 i < targetLEDs.Length;
                 i++)
            {
                SubscribeToObject(
                    targetLEDs[i]);
            }
        }

        private void SubscribeToObject(
            SparkElectronicObject objectReference)
        {
            if (objectReference == null)
            {
                return;
            }

            if (!subscribedObjects.Add(
                    objectReference))
            {
                return;
            }

            SparkElectricalComponent electrical =
                objectReference as SparkElectricalComponent;

            if (electrical != null)
            {
                electrical.ElectricalStateChanged -=
                    HandleElectricalStateChanged;

                electrical.ElectricalStateChanged +=
                    HandleElectricalStateChanged;

                electrical.ElectricalConfigurationChanged -=
                    HandleElectricalConfigurationChanged;

                electrical.ElectricalConfigurationChanged +=
                    HandleElectricalConfigurationChanged;
            }

            objectReference.OperationalStateChanged -=
                HandleOperationalStateChanged;

            objectReference.OperationalStateChanged +=
                HandleOperationalStateChanged;
        }

        private void UnsubscribeFromObjects()
        {
            foreach (
                SparkElectronicObject objectReference
                in subscribedObjects)
            {
                if (objectReference == null)
                {
                    continue;
                }

                SparkElectricalComponent electrical =
                    objectReference as SparkElectricalComponent;

                if (electrical != null)
                {
                    electrical.ElectricalStateChanged -=
                        HandleElectricalStateChanged;

                    electrical.ElectricalConfigurationChanged -=
                        HandleElectricalConfigurationChanged;
                }

                objectReference.OperationalStateChanged -=
                    HandleOperationalStateChanged;
            }

            subscribedObjects.Clear();
        }

        private void HandleElectricalStateChanged(
            SparkElectricalState state)
        {
            Evaluate();
        }

        private void HandleElectricalConfigurationChanged()
        {
            Evaluate();
        }

        private void HandleOperationalStateChanged(
            SparkOperationalState state)
        {
            Evaluate();
        }

        // =========================================================
        // EVALUATION
        // =========================================================

        public void Evaluate()
        {
            evaluationVersion++;

            bool valid =
                EvaluateCircuit(
                    out string reason);

            lastEvaluationValid = valid;
            lastFailureReason = reason;

            bool previousCompleted =
                completed;

            completed = valid;

            if (previousCompleted != completed)
            {
                CompletionChanged?.Invoke(
                    completed);
            }

            CircuitEvaluated?.Invoke();

            if (logStateChanges)
            {
                LogEvaluation(
                    valid,
                    reason);
            }
        }

        // =========================================================
        // CIRCUIT VALIDATION
        // =========================================================

        private bool EvaluateCircuit(
            out string reason)
        {
            reason = null;

            if (circuitSystem == null)
            {
                reason =
                    "Circuit system is missing.";

                return false;
            }

            if (targetLEDs == null ||
                targetLEDs.Length == 0)
            {
                reason =
                    "No target LED is configured.";

                return false;
            }

            if (powerSupplies == null ||
                powerSupplies.Length == 0)
            {
                reason =
                    "No power supply is configured.";

                return false;
            }

            bool anyPoweredTarget = false;

            for (int i = 0;
                 i < targetLEDs.Length;
                 i++)
            {
                SparkLED led =
                    targetLEDs[i];

                if (led == null)
                {
                    if (requireAllTargets)
                    {
                        reason =
                            "A configured target LED is missing.";

                        return false;
                    }

                    continue;
                }

                if (!led.isActiveAndEnabled)
                {
                    if (requireAllTargets)
                    {
                        reason =
                            $"Target LED '{led.name}' is disabled.";

                        return false;
                    }

                    continue;
                }

                if (!EvaluateLED(
                        led,
                        out bool powered,
                        out string ledReason))
                {
                    if (requireAllTargets)
                    {
                        reason = ledReason;
                        return false;
                    }

                    continue;
                }

                if (powered)
                {
                    anyPoweredTarget = true;
                }
            }

            if (!anyPoweredTarget)
            {
                reason =
                    "No target LED is electrically powered.";

                return false;
            }

            return true;
        }

        // =========================================================
        // LED VALIDATION
        // =========================================================

        private bool EvaluateLED(
            SparkLED led,
            out bool powered,
            out string reason)
        {
            powered = false;
            reason = null;

            if (led == null)
            {
                reason =
                    "LED reference is missing.";

                return false;
            }

            SparkTerminal positive =
                FindTerminal(
                    led,
                    SparkTerminalPolarity.Positive);

            SparkTerminal negative =
                FindTerminal(
                    led,
                    SparkTerminalPolarity.Negative);

            if (positive == null)
            {
                reason =
                    $"LED '{led.name}' has no positive terminal.";

                return false;
            }

            if (negative == null)
            {
                reason =
                    $"LED '{led.name}' has no negative terminal.";

                return false;
            }

            if (requireConnectedCircuit)
            {
                if (!IsTerminalConnected(positive))
                {
                    reason =
                        $"LED '{led.name}' positive terminal " +
                        "is not connected.";

                    return false;
                }

                if (!IsTerminalConnected(negative))
                {
                    reason =
                        $"LED '{led.name}' negative terminal " +
                        "is not connected.";

                    return false;
                }
            }

            if (!TryFindActiveSupply(
                    out SparkPowerSupply source))
            {
                reason =
                    $"LED '{led.name}' has no active power supply.";

                return false;
            }

            SparkElectricalState state =
                led.ElectricalState;

            if (state.Conduction !=
                SparkConductionState.Conducting)
            {
                reason =
                    $"LED '{led.name}' is not conducting.";

                return false;
            }

            if (state.Current <=
                minimumCurrent)
            {
                reason =
                    $"LED '{led.name}' current is too low.";

                return false;
            }

            if (state.Voltage < minimumVoltage)
            {
                reason =
                    $"LED '{led.name}' voltage is too low.";

                return false;
            }

            if (led.IsReverseVoltageExceeded)
            {
                reason =
                    $"LED '{led.name}' has excessive reverse voltage.";

                return false;
            }

            float allowedCurrent =
                led.MaximumForwardCurrent *
                maximumLEDCurrentMultiplier;

            if (allowedCurrent > 0f &&
                state.Current > allowedCurrent)
            {
                reason =
                    $"LED '{led.name}' is over-current.";

                return false;
            }

            if (source.IsCurrentLimited)
            {
                reason =
                    $"Power supply '{source.name}' is current limited.";

                return false;
            }

            powered = true;

            return true;
        }

        // =========================================================
        // ACTIVE SUPPLY
        // =========================================================

        private bool TryFindActiveSupply(
            out SparkPowerSupply source)
        {
            source = null;

            if (powerSupplies == null)
            {
                return false;
            }

            for (int i = 0;
                 i < powerSupplies.Length;
                 i++)
            {
                SparkPowerSupply supply =
                    powerSupplies[i];

                if (supply == null)
                {
                    continue;
                }

                if (!supply.isActiveAndEnabled)
                {
                    continue;
                }

                if (requireSupplyActive &&
                    !supply.IsOutputActive)
                {
                    continue;
                }

                source = supply;

                return true;
            }

            return false;
        }

        // =========================================================
        // TERMINAL CONNECTIVITY
        // =========================================================

        private bool IsTerminalConnected(
            SparkTerminal terminal)
        {
            if (terminal == null)
            {
                return false;
            }

            connectionBuffer.Clear();

            circuitSystem.GetConnections(
                terminal,
                connectionBuffer);

            for (int i = 0;
                 i < connectionBuffer.Count;
                 i++)
            {
                SparkCircuitConnection connection =
                    connectionBuffer[i];

                if (connection.A == null ||
                    connection.B == null)
                {
                    continue;
                }

                if (!IsConnectionValid(
                        connection))
                {
                    continue;
                }

                return true;
            }

            return false;
        }

        private bool IsConnectionValid(
            SparkCircuitConnection connection)
        {
            if (connection.A == null ||
                connection.B == null)
            {
                return false;
            }

            if (connection.Kind ==
                SparkConnectionKind.Probe)
            {
                return false;
            }

            SparkElectronicObject ownerA =
                connection.A.Owner;

            SparkElectronicObject ownerB =
                connection.B.Owner;

            if (ownerA == null ||
                ownerB == null)
            {
                return false;
            }

            if (!ownerA.isActiveAndEnabled ||
                !ownerB.isActiveAndEnabled)
            {
                return false;
            }

            if (ownerA.OperationalState ==
                SparkOperationalState.Disabled)
            {
                return false;
            }

            if (ownerB.OperationalState ==
                SparkOperationalState.Disabled)
            {
                return false;
            }

            return true;
        }

        // =========================================================
        // TERMINAL LOOKUP
        // =========================================================

        private SparkTerminal FindTerminal(
            SparkElectronicObject owner,
            SparkTerminalPolarity polarity)
        {
            if (owner == null)
            {
                return null;
            }

            terminalBuffer.Clear();

            SparkTerminal[] terminals =
                owner.GetComponentsInChildren<SparkTerminal>(
                    true);

            if (terminals == null)
            {
                return null;
            }

            for (int i = 0;
                 i < terminals.Length;
                 i++)
            {
                SparkTerminal terminal =
                    terminals[i];

                if (terminal == null)
                {
                    continue;
                }

                if (terminal.Polarity ==
                    polarity)
                {
                    return terminal;
                }
            }

            return null;
        }

        // =========================================================
        // DEBUG
        // =========================================================

        private void LogEvaluation(
            bool valid,
            string reason)
        {
            if (valid)
            {
                Debug.Log(
                    $"[SPARK LEVEL] {name} COMPLETE\n" +
                    "Electrical target conditions satisfied.",
                    this);

                return;
            }

            Debug.Log(
                $"[SPARK LEVEL] {name} NOT COMPLETE\n" +
                $"Reason: {reason}",
                this);
        }

        // =========================================================
        // MANUAL CONTROL
        // =========================================================

        public void ForceEvaluate()
        {
            Evaluate();
        }

        public void ResetCompletion()
        {
            bool changed =
                completed;

            completed = false;
            lastEvaluationValid = false;

            lastFailureReason =
                "Level completion reset.";

            if (changed)
            {
                CompletionChanged?.Invoke(false);
            }
        }

        // =========================================================
        // VALIDATION
        // =========================================================

        private void OnValidate()
        {
            minimumVoltage =
                Mathf.Max(
                    0f,
                    minimumVoltage);

            minimumCurrent =
                Mathf.Max(
                    0f,
                    minimumCurrent);

            maximumLEDCurrentMultiplier =
                Mathf.Max(
                    0f,
                    maximumLEDCurrentMultiplier);
        }
    }
}