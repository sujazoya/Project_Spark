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

            [Header("Electrical Wire")]

        [SerializeField, Min(0.000001f)]
        private float wireResistance = 0.001f;

        public float WireResistance =>
            Mathf.Max(0.000001f, wireResistance);

    

            
        public bool IsConducting =>
            isActiveAndEnabled &&
            ElectricalEnabled &&
            HasValidTerminals;



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
            private ulong terminalAConnectionId;
            private ulong terminalBConnectionId;

            private SparkTerminal physicalContactTerminalA;
            private SparkTerminal physicalContactTerminalB;

            private SparkWireContact physicalSourceA;
            private SparkWireContact physicalSourceB;

            private bool hasTerminalAConnection;
            private bool hasTerminalBConnection;

            [Header("Physical Wire Ends")]
            [SerializeField] private SparkWireContact contactA;
            [SerializeField] private SparkWireContact contactB;




            public SparkWireContact ContactA => contactA;
            public SparkWireContact ContactB => contactB;

            public SparkWireContact ConnectedSourceA => physicalSourceA;
            public SparkWireContact ConnectedSourceB => physicalSourceB;

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

       public bool HasValidTerminals =>
    terminalA != null &&
    terminalB != null &&
    terminalA != terminalB;


        // ============================================================
        // PUBLIC ELECTRICAL STATE
        // ============================================================

     
public float Voltage => ElectricalState.Voltage;

public float Current => ElectricalState.Current;

public float Power => ElectricalState.Power;


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


        public bool HasTerminalAConnection =>
            hasTerminalAConnection;

        public bool HasTerminalBConnection =>
            hasTerminalBConnection;

        public bool IsFullyConnected =>
            hasTerminalAConnection &&
            hasTerminalBConnection;

        public SparkTerminal ConnectedTerminalA =>
            physicalContactTerminalA;

        public SparkTerminal ConnectedTerminalB =>
            physicalContactTerminalB;

        public int ActiveContactCount
        {
            get
            {
                int count = 0;

                if (hasTerminalAConnection)
                    count++;

                if (hasTerminalBConnection)
                    count++;

                return count;
            }
        }



        // ============================================================
        // UNITY
        // ============================================================

        protected override void Awake()
        {
            base.Awake();          

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
    RemoveAllPhysicalContacts();
    Unsubscribe();
}


        private void OnDestroy()
        {
            RemovePhysicalContact();

            Unsubscribe();
        }
public bool RemovePhysicalContact(
    SparkTerminal target)
{
    if (target == null)
        return false;

    if (hasTerminalAConnection &&
        physicalContactTerminalA == target)
    {
        return RemovePhysicalContact(
            target,
            physicalSourceA);
    }

    if (hasTerminalBConnection &&
        physicalContactTerminalB == target)
    {
        return RemovePhysicalContact(
            target,
            physicalSourceB);
    }

    return false;
}

public void RemovePhysicalContact()
{
    RemoveAllPhysicalContacts();
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
    SparkTerminal target,
    SparkWireContact source)
{
    if (!isActiveAndEnabled)
        return false;

    if (target == null || source == null)
        return false;

    if (!HasValidTerminals)
        return false;

    if (terminalA == terminalB)
        return false;

    if (contactA == null ||
        contactB == null ||
        contactA == contactB)
    {
        Debug.LogError(
            $"[SparkSingleWire] Invalid physical contact setup: {name}",
            this);

        return false;
    }

    if (source.Wire != this)
        return false;

    if (target == terminalA ||
        target == terminalB)
    {
        return false;
    }

    bool isA = source == contactA;
    bool isB = source == contactB;

    if (!isA && !isB)
        return false;

    ResolveCircuitSystem();

    if (circuitSystem == null)
        return false;

    SparkTerminal internalTerminal =
        isA ? terminalA : terminalB;

    bool alreadyConnected =
        isA
            ? hasTerminalAConnection
            : hasTerminalBConnection;

    SparkTerminal previousTarget =
        isA
            ? physicalContactTerminalA
            : physicalContactTerminalB;

    ulong previousId =
        isA
            ? terminalAConnectionId
            : terminalBConnectionId;

    // Validate an existing connection against the actual
    // circuit system, not just our local cached state.
    if (alreadyConnected)
    {
        bool stillExists =
            previousId != 0UL &&
            circuitSystem.TryGetConnection(
                previousId,
                out SparkCircuitConnection existing) &&
            existing.Connects(
                internalTerminal,
                previousTarget);

        if (stillExists)
        {
            // This end is already connected to this target.
            if (previousTarget == target)
                return true;

            // One physical end cannot connect to two
            // different external terminals simultaneously.
            return false;
        }

        // Circuit connection was removed externally.
        // Clear stale local state.
        if (isA)
        {
            ClearContactA();
        }
        else
        {
            ClearContactB();
        }
    }

    // Prevent both ends attaching to the same external
    // terminal as two independent physical contacts.
    if (isA &&
        hasTerminalBConnection &&
        physicalContactTerminalB == target)
    {
        return false;
    }

    if (isB &&
        hasTerminalAConnection &&
        physicalContactTerminalA == target)
    {
        return false;
    }

    if (internalTerminal.AtCapacity)
        return false;

    bool created =
        circuitSystem.TryCreateConnection(
            internalTerminal,
            target,
            SparkConnectionKind.Wire,
            SparkConnectionDirection.Bidirectional,
            out SparkCircuitConnection connection);

    if (!created)
        return false;

    if (isA)
    {
        terminalAConnectionId = connection.Id;
        physicalContactTerminalA = target;
        physicalSourceA = source;
        hasTerminalAConnection = true;
    }
    else
    {
        terminalBConnectionId = connection.Id;
        physicalContactTerminalB = target;
        physicalSourceB = source;
        hasTerminalBConnection = true;
    }

#if UNITY_EDITOR || DEVELOPMENT_BUILD
    Debug.Log(
        $"[WIRE CONNECTED] {name} | " +
        $"End={(isA ? "A" : "B")} | " +
        $"Internal={internalTerminal.name} | " +
        $"External={target.name} | " +
        $"ID={connection.Id}",
        this);
#endif

    RefreshVisuals();

    return true;
}
private void ClearContactA()
{
    terminalAConnectionId = 0UL;
    physicalContactTerminalA = null;
    physicalSourceA = null;
    hasTerminalAConnection = false;
}

private void ClearContactB()
{
    terminalBConnectionId = 0UL;
    physicalContactTerminalB = null;
    physicalSourceB = null;
    hasTerminalBConnection = false;
}

public bool RemovePhysicalContact(
    SparkTerminal target,
    SparkWireContact source)
{
    if (target == null || source == null)
        return false;

    bool isA =
        source == contactA &&
        hasTerminalAConnection &&
        physicalSourceA == source &&
        physicalContactTerminalA == target;

    bool isB =
        source == contactB &&
        hasTerminalBConnection &&
        physicalSourceB == source &&
        physicalContactTerminalB == target;

    if (!isA && !isB)
        return false;

    ResolveCircuitSystem();

    ulong id =
        isA
            ? terminalAConnectionId
            : terminalBConnectionId;

    bool removed = false;

    if (circuitSystem != null && id != 0UL)
    {
        removed =
            circuitSystem.TryRemoveConnection(
                id,
                out _);
    }

    if (isA)
        ClearContactA();
    else
        ClearContactB();

    RefreshVisuals();

    return removed;
}


        /// <summary>
        /// Removes the single physical electrical contact.
        /// </summary>
      

        /// <summary>
        /// Removes the current physical electrical contact.
        /// </summary>
public void RemoveAllPhysicalContacts()
{
    ResolveCircuitSystem();

    ulong idA = terminalAConnectionId;
    ulong idB = terminalBConnectionId;

    // Clear local bookkeeping before notifying the circuit.
    // This prevents callbacks from observing stale contact state.
    ClearContactA();
    ClearContactB();

    if (circuitSystem != null)
    {
        if (idA != 0UL)
        {
            circuitSystem.TryRemoveConnection(
                idA,
                out _);
        }

        if (idB != 0UL && idB != idA)
        {
            circuitSystem.TryRemoveConnection(
                idB,
                out _);
        }
    }

    RefreshVisuals();
}

       

        // ============================================================
        // CONDUCTIVE DEVICE
        // ============================================================

       
        public bool CanConductBetween(
            SparkTerminal from,
            SparkTerminal to)
        {
            if (!IsConducting)
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
    if (a == null || b == null || a == b)
    {
        Debug.LogError(
            "[SparkSingleWire] Invalid terminal assignment.",
            this);

        return;
    }

    if (terminalA == a && terminalB == b)
        return;

    RemoveAllPhysicalContacts();

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
    propertyBlock.SetFloat("_FlowEnabled", 0f);
    renderer.SetPropertyBlock(propertyBlock);
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
        [ContextMenu("Diagnostics / Check Wire Electrical Path")]
private void CheckWireElectricalPath()
{
    ResolveCircuitSystem();

    bool connectionAExists =
        circuitSystem != null &&
        terminalAConnectionId != 0UL &&
        circuitSystem.HasConnection(
            terminalAConnectionId);

    bool connectionBExists =
        circuitSystem != null &&
        terminalBConnectionId != 0UL &&
        circuitSystem.HasConnection(
            terminalBConnectionId);

    Debug.Log(
        $"========== WIRE DIAGNOSTIC ==========\n" +
        $"Wire: {name}\n" +
        $"ElectricalEnabled: {ElectricalEnabled}\n" +
        $"Terminal A: {(terminalA != null ? terminalA.name : "NULL")}\n" +
        $"Terminal B: {(terminalB != null ? terminalB.name : "NULL")}\n" +
        $"Contact A: {(contactA != null ? contactA.name : "NULL")}\n" +
        $"Contact B: {(contactB != null ? contactB.name : "NULL")}\n" +
        $"External A: {(physicalContactTerminalA != null ? physicalContactTerminalA.name : "NULL")}\n" +
        $"External B: {(physicalContactTerminalB != null ? physicalContactTerminalB.name : "NULL")}\n" +
        $"Connection A Exists: {connectionAExists}\n" +
        $"Connection B Exists: {connectionBExists}\n" +
        $"Fully Connected: {IsFullyConnected}\n" +
        $"Can Conduct A-B: {CanConductBetween(terminalA, terminalB)}\n" +
        $"Resistance: {WireResistance:F6} Ω\n" +
        $"Voltage A: {(terminalA != null ? terminalA.Voltage : 0f):F6} V\n" +
        $"Voltage B: {(terminalB != null ? terminalB.Voltage : 0f):F6} V\n" +
        $"Current: {Current:F6} A\n" +
        $"====================================",
        this);
}
    }
}