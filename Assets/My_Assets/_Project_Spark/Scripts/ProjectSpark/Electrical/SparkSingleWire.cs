using System.Collections.Generic;
using UnityEngine;
using ProjectSpark.Circuit;

namespace ProjectSpark.Gameplay
{
    /// <summary>
    /// Standalone single-connection conductive wire.
    ///
    /// The wire owns two internal SparkTerminals for compatibility,
    /// but external physical contacts are represented by ONE
    /// electrical connection through Terminal A.
    ///
    /// Behavior:
    ///     External Terminal
    ///            |
    ///            v
    ///       Terminal A
    ///            |
    ///     SparkSingleWire
    ///            |
    ///       Terminal B
    ///
    /// Terminal A is the external electrical attachment point.
    /// The wire itself is the continuous conductive component.
    ///
    /// Electrical topology remains owned by SparkCircuitSystem.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class SparkSingleWire :
        SparkElectricalComponent,
        ISparkConductiveDevice
    {
        // ============================================================
        // TERMINALS
        // ============================================================

        [Header("Terminals")]

        [Tooltip(
            "Primary electrical attachment terminal. " +
            "All physical contacts connect through this terminal.")]
        [SerializeField]
        private SparkTerminal terminalA;

        [Tooltip(
            "Secondary internal terminal retained for compatibility " +
            "and normal two-terminal conductive behavior.")]
        [SerializeField]
        private SparkTerminal terminalB;

        [SerializeField]
        [Min(1)]
        private int maxConnectionsPerTerminal = 1;

        [SerializeField]
        private SparkTerminalKind terminalKind =
            SparkTerminalKind.Generic;


        // ============================================================
        // VISUALS
        // ============================================================

        [Header("Visuals")]

        [SerializeField]
        private Renderer[] wireRenderers;

        [Tooltip(
            "Optional local neutral material.")]
        [SerializeField]
        private Material neutralMaterial;

        [Tooltip(
            "Optional local positive-voltage material.")]
        [SerializeField]
        private Material positiveVoltageMaterial;

        [Tooltip(
            "Optional local negative-voltage material.")]
        [SerializeField]
        private Material negativeVoltageMaterial;

        [Tooltip(
            "Optional local positive-current material.")]
        [SerializeField]
        private Material positiveCurrentMaterial;

        [Tooltip(
            "Optional local negative-current material.")]
        [SerializeField]
        private Material negativeCurrentMaterial;

        [Tooltip(
            "Optional local short-circuit material.")]
        [SerializeField]
        private Material shortCircuitMaterial;


        // ============================================================
        // CIRCUIT SYSTEM
        // ============================================================

        [Header("Circuit System")]

        [SerializeField]
        private SparkCircuitSystem circuitSystem;


        // ============================================================
        // VISUAL THRESHOLDS
        // ============================================================

        [Header("Visual Thresholds")]

        [SerializeField]
        [Min(0f)]
        private float voltageVisualThreshold = 0.001f;

        [SerializeField]
        [Min(0f)]
        private float currentVisualThreshold = 0.001f;


        // ============================================================
        // FLOW SHADER
        // ============================================================

        [Header("Flow Shader")]

        [SerializeField]
        private bool enableFlowShader = true;

        [SerializeField]
        [Min(0f)]
        private float flowIntensity = 1f;

        [SerializeField]
        private float flowSpeed = 1f;

        [SerializeField]
        private bool flowDirectionPositive = true;


        // ============================================================
        // ENUM
        // ============================================================

        public enum ElectricFlowVisualState
        {
            Connected,
            PositiveVoltage,
            NegativeVoltage,
            PositiveCurrent,
            NegativeCurrent,
            ShortCircuit
        }


        // ============================================================
        // RUNTIME
        // ============================================================

       private MaterialPropertyBlock propertyBlock;

        private ulong physicalContactConnectionId;

        private SparkTerminal physicalContactTerminal;

        private bool hasPhysicalContactConnection;

        private Vector3 lastClickedPoint;

        private Vector3 lastClickedNormal;


        // ============================================================
        // PUBLIC TERMINALS
        // ============================================================

        public SparkTerminal TerminalA
        {
            get
            {
                return terminalA;
            }
        }

        public SparkTerminal TerminalB
        {
            get
            {
                return terminalB;
            }
        }

        public bool HasValidTerminals
        {
            get
            {
                return
                    terminalA != null &&
                    terminalB != null;
            }
        }


        // ============================================================
        // PUBLIC ELECTRICAL STATE
        // ============================================================

        public float Voltage
        {
            get
            {
                if (terminalA == null)
                    return 0f;

                return terminalA.Voltage;
            }
        }

        public float Current
        {
            get
            {
                if (terminalA == null)
                    return 0f;

                return terminalA.Current;
            }
        }

        public float Power
        {
            get
            {
                return Voltage * Current;
            }
        }

        public bool HasVoltage
        {
            get
            {
                return
                    Mathf.Abs(Voltage) >
                    voltageVisualThreshold;
            }
        }

        public bool HasCurrent
        {
            get
            {
                return
                    Mathf.Abs(Current) >
                    currentVisualThreshold;
            }
        }

        public bool IsShortCircuit
        {
            get
            {
                return
                    circuitSystem != null &&
                    circuitSystem.GetElectricFlowVisualState(
                        Voltage,
                        Current,
                        false) ==
                    SparkCircuitSystem.ElectricFlowVisualState.Short;
            }
        }


        public ElectricFlowVisualState VisualState
        {
            get
            {
                return CalculateVisualState();
            }
        }

        public SparkTerminalPolarity EffectivePolarity
        {
            get
            {
                if (terminalA == null)
                    return SparkTerminalPolarity.None;

                return terminalA.EffectivePolarity;
            }
        }


        // ============================================================
        // PHYSICAL CONTACT STATE
        // ============================================================

        /// <summary>
        /// Returns true when the wire currently has one physical
        /// electrical contact.
        /// </summary>
        public bool HasActiveContact
        {
            get
            {
                return hasPhysicalContactConnection;
            }
        }

        /// <summary>
        /// Single-connection wire can have only one active external
        /// electrical contact.
        /// </summary>
        public int ActiveContactCount
        {
            get
            {
                return hasPhysicalContactConnection ? 1 : 0;
            }
        }

        /// <summary>
        /// Returns true when the wire can accept another physical
        /// contact.
        /// </summary>
        public bool HasAvailableContactSlot
        {
            get
            {
                return !hasPhysicalContactConnection;
            }
        }

        public Vector3 LastClickedPoint
        {
            get
            {
                return lastClickedPoint;
            }
        }

        public Vector3 LastClickedNormal
        {
            get
            {
                return lastClickedNormal;
            }
        }


        // ============================================================
        // UNITY
        // ============================================================

        protected override void Awake()
        {
            base.Awake();

            EnsureTerminals();

            ResolveCircuitSystem();

            Subscribe();

            RefreshVisuals();
             propertyBlock = new MaterialPropertyBlock();
        }
        


        private void Reset()
        {
            EnsureTerminals();
            ResolveCircuitSystem();
        }


        private void OnEnable()
        {
            Subscribe();
        }


        private void OnDisable()
        {
            Unsubscribe();
        }


        private void OnDestroy()
        {
            RemovePhysicalContact();

            Unsubscribe();
        }


        private void OnValidate()
        {
            if (maxConnectionsPerTerminal < 1)
            {
                maxConnectionsPerTerminal = 1;
            }

            if (voltageVisualThreshold < 0f)
            {
                voltageVisualThreshold = 0f;
            }

            if (currentVisualThreshold < 0f)
            {
                currentVisualThreshold = 0f;
            }

            if (flowIntensity < 0f)
            {
                flowIntensity = 0f;
            }

            EnsureTerminals();
        }


        // ============================================================
        // TERMINAL CREATION
        // ============================================================

        private void EnsureTerminals()
        {
            if (terminalA == null)
            {
                terminalA =
                    CreateTerminal(
                        "Terminal_A");
            }

            if (terminalB == null)
            {
                terminalB =
                    CreateTerminal(
                        "Terminal_B");
            }
        }


        private SparkTerminal CreateTerminal(
            string terminalName)
        {
            Transform existing =
                transform.Find(terminalName);

            GameObject terminalObject;

            if (existing != null)
            {
                terminalObject =
                    existing.gameObject;
            }
            else
            {
                terminalObject =
                    new GameObject(terminalName);

                terminalObject.transform.SetParent(
                    transform,
                    false);
            }

            SparkTerminal terminal =
                terminalObject.GetComponent<SparkTerminal>();

            if (terminal == null)
            {
                terminal =
                    terminalObject.AddComponent<SparkTerminal>();
            }

            return terminal;
        }


        // ============================================================
        // CIRCUIT SYSTEM
        // ============================================================

        private void ResolveCircuitSystem()
        {
            if (circuitSystem != null)
                return;

            circuitSystem =
                FindCircuitSystem();
        }


        private SparkCircuitSystem FindCircuitSystem()
        {
            SparkCircuitSystem[] systems =
                FindObjectsByType<SparkCircuitSystem>(
                    FindObjectsInactive.Exclude,
                    FindObjectsSortMode.None);

            if (systems == null ||
                systems.Length == 0)
            {
                return null;
            }

            return systems[0];
        }


        // ============================================================
        // SUBSCRIPTION
        // ============================================================

        private void Subscribe()
        {
            if (terminalA != null)
            {
                terminalA.ElectricalStateChanged -=
                    HandleTerminalElectricalStateChanged;

                terminalA.ElectricalStateChanged +=
                    HandleTerminalElectricalStateChanged;
            }

            if (terminalB != null)
            {
                terminalB.ElectricalStateChanged -=
                    HandleTerminalElectricalStateChanged;

                terminalB.ElectricalStateChanged +=
                    HandleTerminalElectricalStateChanged;
            }

            if (circuitSystem != null)
            {
                circuitSystem.ElectricalFlowVisualsChanged -=
                    HandleElectricalFlowVisualsChanged;

                circuitSystem.ElectricalFlowVisualsChanged +=
                    HandleElectricalFlowVisualsChanged;
            }
        }


        private void Unsubscribe()
        {
            if (terminalA != null)
            {
                terminalA.ElectricalStateChanged -=
                    HandleTerminalElectricalStateChanged;
            }

            if (terminalB != null)
            {
                terminalB.ElectricalStateChanged -=
                    HandleTerminalElectricalStateChanged;
            }

            if (circuitSystem != null)
            {
                circuitSystem.ElectricalFlowVisualsChanged -=
                    HandleElectricalFlowVisualsChanged;
            }
        }


        private void HandleTerminalElectricalStateChanged(
            SparkTerminal terminal)
        {
            RefreshVisuals();
        }


        private void HandleElectricalFlowVisualsChanged()
        {
            RefreshVisuals();
        }


        // ============================================================
        // PHYSICAL CONTACT
        // ============================================================

        /// <summary>
        /// Creates ONE electrical connection between this wire and
        /// the supplied external terminal.
        ///
        /// All physical contacts on this wire use Terminal A.
        /// </summary>
        public bool AddPhysicalContact(
            SparkTerminal terminal,
            SparkWireContact source)
        {
            if (!isActiveAndEnabled)
                return false;

            if (terminal == null)
                return false;

            if (source == null)
                return false;

            EnsureTerminals();

            if (!HasValidTerminals)
                return false;

            if (terminal == terminalA ||
                terminal == terminalB)
            {
                return false;
            }

            ResolveCircuitSystem();

            if (circuitSystem == null)
            {
                Debug.LogWarning(
                    $"[SparkSingleWire] No SparkCircuitSystem found for {name}.",
                    this);

                return false;
            }

            // --------------------------------------------------------
            // Existing connection
            // --------------------------------------------------------

            if (hasPhysicalContactConnection)
            {
                if (physicalContactTerminal == terminal)
                {
                    return true;
                }

                // Single-connection behavior:
                // another terminal cannot replace the existing one
                // until the current physical contact is removed.
                return false;
            }

            // --------------------------------------------------------
            // Terminal A is the ONLY external attachment point.
            // --------------------------------------------------------

            if (terminalA.AtCapacity)
            {
                Debug.LogWarning(
                    $"[SparkSingleWire] Terminal A is already at capacity: {name}",
                    this);

                return false;
            }

            bool connected =
                circuitSystem.TryCreateConnection(
                    terminalA,
                    terminal,
                    SparkConnectionKind.Wire,
                    SparkConnectionDirection.Bidirectional,
                    out SparkCircuitConnection connection);

            if (!connected)
            {
                return false;
            }

            physicalContactConnectionId =
                connection.Id;

            physicalContactTerminal =
                terminal;

            hasPhysicalContactConnection =
                true;

            RefreshVisuals();

            return true;
        }


        /// <summary>
        /// Removes the single physical electrical contact.
        /// </summary>
        public bool RemovePhysicalContact(
            SparkTerminal terminal,
            SparkWireContact source)
        {
            if (!hasPhysicalContactConnection)
                return false;

            if (terminal != null &&
                physicalContactTerminal != terminal)
            {
                return false;
            }

            ResolveCircuitSystem();

            ulong connectionId =
                physicalContactConnectionId;

            physicalContactConnectionId =
                0UL;

            physicalContactTerminal =
                null;

            hasPhysicalContactConnection =
                false;

            if (circuitSystem != null &&
                connectionId != 0UL)
            {
                circuitSystem.TryRemoveConnection(
                    connectionId,
                    out _);
            }

            RefreshVisuals();

            return true;
        }


        /// <summary>
        /// Removes the current physical electrical contact.
        /// </summary>
        public void RemovePhysicalContact()
        {
            RemovePhysicalContact(
                physicalContactTerminal,
                null);
        }


        // ============================================================
        // CLICK
        // ============================================================

        /// <summary>
        /// Called by SparkWireContact when the wire collider is
        /// clicked.
        ///
        /// Clicking alone does not create an electrical connection.
        /// This method stores the exact clicked world position.
        /// </summary>
        public void OnWireClicked(
            Vector3 clickedPoint,
            Vector3 clickedNormal)
        {
            lastClickedPoint =
                clickedPoint;

            lastClickedNormal =
                clickedNormal;

            Debug.Log(
                $"[SparkSingleWire] Wire clicked: {name}\n" +
                $"Point: {clickedPoint}",
                this);
        }


        // ============================================================
        // CONDUCTIVE DEVICE
        // ============================================================

        public bool CanConductBetween(
            SparkTerminal from,
            SparkTerminal to)
        {
            if (!ElectricalEnabled)
                return false;

            if (!HasValidTerminals)
                return false;

            return
                (from == terminalA && to == terminalB) ||
                (from == terminalB && to == terminalA);
        }


        public bool OwnsTerminal(
            SparkTerminal terminal)
        {
            return
                terminal != null &&
                (terminal == terminalA ||
                 terminal == terminalB);
        }


        public SparkTerminal GetOppositeTerminal(
            SparkTerminal terminal)
        {
            if (terminal == terminalA)
                return terminalB;

            if (terminal == terminalB)
                return terminalA;

            return null;
        }


        // ============================================================
        // TERMINAL CONFIGURATION
        // ============================================================

        public void SetTerminals(
            SparkTerminal a,
            SparkTerminal b)
        {
            Unsubscribe();

            terminalA = a;
            terminalB = b;

            Subscribe();

            RefreshVisuals();
        }


        // ============================================================
        // VISUAL STATE
        // ============================================================

        private ElectricFlowVisualState CalculateVisualState()
        {
            float voltage =
                Voltage;

            float current =
                Current;

            if (IsShortCircuit)
            {
                return ElectricFlowVisualState.ShortCircuit;
            }

            if (Mathf.Abs(current) >
                currentVisualThreshold)
            {
                if (current > 0f)
                {
                    return ElectricFlowVisualState.PositiveCurrent;
                }

                return ElectricFlowVisualState.NegativeCurrent;
            }

            if (Mathf.Abs(voltage) >
                voltageVisualThreshold)
            {
                if (voltage > 0f)
                {
                    return ElectricFlowVisualState.PositiveVoltage;
                }

                return ElectricFlowVisualState.NegativeVoltage;
            }

            return ElectricFlowVisualState.Connected;
        }


        private void RefreshVisuals()
        {
            if (wireRenderers == null ||
                wireRenderers.Length == 0)
            {
                return;
            }

            ElectricFlowVisualState state =
                CalculateVisualState();

            Material material =
                GetMaterialForState(state);

            for (int i = 0;
                 i < wireRenderers.Length;
                 i++)
            {
                Renderer renderer =
                    wireRenderers[i];

                if (renderer == null)
                    continue;

                if (material != null)
                {
                    renderer.sharedMaterial =
                        material;
                }

                ApplyFlowProperties(
                    renderer,
                    state);
            }
        }

        public bool TryConnectToTarget(
    SparkTerminal target,
    SparkWireContact source)
{
    if (target == null)
    {
        return false;
    }

    if (source == null)
    {
        return false;
    }

    return AddPhysicalContact(
        target,
        source);
}


      private Material GetMaterialForState(
    ElectricFlowVisualState state)
{
    Material localMaterial = null;

    switch (state)
    {
        case ElectricFlowVisualState.ShortCircuit:
            localMaterial = shortCircuitMaterial;
            break;

        case ElectricFlowVisualState.PositiveCurrent:
            localMaterial = positiveCurrentMaterial;
            break;

        case ElectricFlowVisualState.NegativeCurrent:
            localMaterial = negativeCurrentMaterial;
            break;

        case ElectricFlowVisualState.PositiveVoltage:
            localMaterial = positiveVoltageMaterial;
            break;

        case ElectricFlowVisualState.NegativeVoltage:
            localMaterial = negativeVoltageMaterial;
            break;

        case ElectricFlowVisualState.Connected:
        default:
            localMaterial = neutralMaterial;
            break;
    }

    if (localMaterial != null)
    {
        return localMaterial;
    }

    if (circuitSystem == null)
    {
        return null;
    }

    SparkCircuitSystem.ElectricFlowVisualState circuitState =
        circuitSystem.GetElectricFlowVisualState(
            Voltage,
            Current,
            state == ElectricFlowVisualState.ShortCircuit
        );

    return circuitSystem.GetMaterialForFlowState(circuitState);
}


        // ============================================================
        // FLOW SHADER
        // ============================================================

        private void ApplyFlowProperties(
            Renderer renderer,
            ElectricFlowVisualState state)
        {
             if (renderer == null)
    {
        return;
    }

    if (propertyBlock == null)
    {
        propertyBlock = new MaterialPropertyBlock();
    }

    renderer.GetPropertyBlock(propertyBlock);
            if (!enableFlowShader)
            {
                return;
            }

            renderer.GetPropertyBlock(
                propertyBlock);

            bool flowEnabled =
                state ==
                    ElectricFlowVisualState.PositiveCurrent ||
                state ==
                    ElectricFlowVisualState.NegativeCurrent;

            propertyBlock.SetFloat(
                "_FlowEnabled",
                flowEnabled ? 1f : 0f);

            propertyBlock.SetFloat(
                "_FlowIntensity",
                flowIntensity);

            propertyBlock.SetFloat(
                "_FlowSpeed",
                flowSpeed);

            float direction =
                flowDirectionPositive
                    ? 1f
                    : -1f;

            if (state ==
                ElectricFlowVisualState.NegativeCurrent)
            {
                direction *= -1f;
            }

            propertyBlock.SetFloat(
                "_FlowDirection",
                direction);

            renderer.SetPropertyBlock(
                propertyBlock);
        }


        // ============================================================
        // GIZMOS
        // ============================================================

        private void OnDrawGizmosSelected()
        {
            if (terminalA != null)
            {
                Gizmos.DrawWireSphere(
                    terminalA.transform.position,
                    0.025f);
            }

            if (terminalB != null)
            {
                Gizmos.DrawWireSphere(
                    terminalB.transform.position,
                    0.025f);
            }
        }
    }
}