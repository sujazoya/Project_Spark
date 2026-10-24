
using System;
using System.Collections.Generic;
using UnityEngine;
using ProjectSpark.Circuit;
using ProjectSpark.Gameplay;
using ProjectSpark.Electrical;

namespace AAAUI.VFX
{
    /// <summary>
    /// Manages dynamically drawn signal wires.
    ///
    /// Features:
    /// - Creates and stores visual wire paths.
    /// - Binds paths to electrical connection IDs.
    /// - Removes one wire and its electrical connection.
    /// - Undoes the most recently created wire.
    /// - Clears all player-drawn wires.
    /// - Automatically removes a visual wire when its
    ///   electrical connection is removed externally.
    /// - Updates wire materials from terminal voltage.
    ///
    /// This manager does not manage SparkSingleWire objects.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class SignalPath_Manager : MonoBehaviour
    {
        #region Inspector

        [Header("Wire Prefab")]
        [SerializeField]
        private SignalPathMesh meshPrefab;

        [Header("Current Visual Publisher")]
[SerializeField]
private SparkWireCurrentVisualPublisher currentVisualPublisher;

        [Header("Wire Materials")]
        [SerializeField]
        private Material positiveMaterial;

        [SerializeField]
        private Material negativeMaterial;

        [SerializeField]
        private Material neutralMaterial;

        [Header("Electrical System")]
        [SerializeField]
        private SparkCircuitSystem circuitSystem;

        [Header("Runtime Paths")]
        [SerializeField]
        private SignalPath[] paths =
            Array.Empty<SignalPath>();

        [Header("Debug")]
        [SerializeField]
        private bool logOperations;

        #endregion

        #region Runtime

        private readonly Dictionary<
            SignalPath,
            SparkCircuitConnection> electricalBindings =
                new Dictionary<
                    SignalPath,
                    SparkCircuitConnection>();

        private readonly Dictionary<ulong, SignalPath>
            pathsByConnectionId =
                new Dictionary<ulong, SignalPath>();

        private readonly HashSet<SparkTerminal>
            subscribedTerminals =
                new HashSet<SparkTerminal>();

        private readonly List<SignalPath>
            creationOrder =
                new List<SignalPath>();

                [Header("Electrical Glow")]
        [SerializeField]
        private bool enableElectricalGlow = true;

        [Header("Wire Current Popups")]
[SerializeField]
private bool enableWireCurrentFeedback = true;

        [SerializeField]
        private Material electricalGlowMaterial;

        private int currentIndex = -1;

        private SignalPath currentPath;
        private SignalPathMesh currentMesh;

        private bool clearingPaths;

        private const float VoltageThreshold = 0.05f;

        #endregion

        #region Properties

        public SignalPath CurrentPath => currentPath;

        public SignalPathMesh CurrentMesh => currentMesh;

        public int CurrentIndex => currentIndex;

        public int Count => paths.Length;

        public SignalPath[] Paths => paths;

        public SparkCircuitSystem Circuit => circuitSystem;

        public bool HasWires => paths.Length > 0;

        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            ResolveCircuitSystem();
        }

        private void OnEnable()
        {
            ResolveCircuitSystem();
            SubscribeCircuit();
        }

        private void OnDisable()
        {
            UnsubscribeCircuit();
        }

        private void OnDestroy()
        {
            UnsubscribeCircuit();
            UnsubscribeAllTerminals();
        }

        #endregion

        #region Circuit Reference

        private void ResolveCircuitSystem()
        {
            if (circuitSystem != null)
                return;

            circuitSystem =
                GetComponentInParent<SparkCircuitSystem>();

            if (circuitSystem == null)
            {
                circuitSystem =
                    FindFirstObjectByType<SparkCircuitSystem>();
            }
        }

        private void SubscribeCircuit()
        {
            if (circuitSystem == null)
                return;

            circuitSystem.ConnectionRemoved -=
                OnCircuitConnectionRemoved;

            circuitSystem.ConnectionRemoved +=
                OnCircuitConnectionRemoved;
        }

        private void UnsubscribeCircuit()
        {
            if (circuitSystem == null)
                return;

            circuitSystem.ConnectionRemoved -=
                OnCircuitConnectionRemoved;
        }

        /// <summary>
        /// Synchronize visual wires when the circuit system
        /// removes a connection independently.
        /// </summary>
        private void OnCircuitConnectionRemoved(
            SparkCircuitConnection connection)
        {
            if (connection == null || clearingPaths)
                return;

            if (pathsByConnectionId.TryGetValue(
                connection.Id,
                out SignalPath path))
            {
                RemovePathInternal(path, false);
            }
        }

        #endregion

        #region Wire Creation

        public SignalPathMesh CreateNextWire(
            WirePolarity polarity)
        {
            currentIndex++;

            SignalPathMesh newMesh =
                CreateWireMesh(currentIndex);

            if (newMesh == null)
                return null;

            SignalPath newPath =
                newMesh.GetComponent<SignalPath>();

            if (newPath == null)
            {
                newPath =
                    newMesh.gameObject.AddComponent<SignalPath>();
            }

            newMesh.SetPath(newPath);
            newMesh.SetMaterial(GetMaterial(polarity));
           // -------------------------------------------------
// RUNTIME ELECTRICAL GLOW
// -------------------------------------------------

if (enableElectricalGlow &&
    electricalGlowMaterial != null)
{
    SignalWireElectricalGlow glow =
        newMesh.GetComponent<SignalWireElectricalGlow>();

    if (glow == null)
    {
        glow = newMesh.gameObject
            .AddComponent<SignalWireElectricalGlow>();
    }

    glow.Initialize(
        newMesh,
        electricalGlowMaterial);
}

// -------------------------------------------------
// RUNTIME CURRENT FEEDBACK
// -------------------------------------------------

if (enableWireCurrentFeedback)
{
    SignalWireCurrentFeedback feedback =
        newMesh.GetComponent<SignalWireCurrentFeedback>();

    if (feedback == null)
    {
        feedback = newMesh.gameObject
            .AddComponent<SignalWireCurrentFeedback>();
    }
}

            int count = paths.Length;

            Array.Resize(ref paths, count + 1);
            paths[count] = newPath;

            creationOrder.Add(newPath);

            currentPath = newPath;
            currentMesh = newMesh;        

            Log(
                $"Created wire: {newPath.name}");

            return newMesh;
        }

                private void RefreshWireGlow(
            SignalPath path,
            SparkCircuitConnection connection)
        {
            if (path == null || connection == null)
                return;

            SignalWireElectricalGlow glow =
                path.GetComponent<SignalWireElectricalGlow>();

            if (glow == null)
                return;

            SparkTerminal a = connection.A;
            SparkTerminal b = connection.B;

            if (a == null || b == null)
            {
                glow.SetVoltage(0f);
                return;
            }

            float voltageA = a.Voltage;
            float voltageB = b.Voltage;

            // For ideal wires, endpoints should share
            // the same solved electrical node.
            float representativeVoltage =
                (voltageA + voltageB) * 0.5f;

            glow.SetVoltage(representativeVoltage);
        }

        private SignalPathMesh CreateWireMesh(
            int index)
        {
            if (meshPrefab != null)
            {
                SignalPathMesh instance =
                    Instantiate(meshPrefab, transform);

                instance.name =
                    $"SignalWire_{index}";

                return instance;
            }

            GameObject wireObject =
                new GameObject(
                    $"SignalWire_{index}");

            wireObject.transform.SetParent(
                transform,
                false);

            wireObject.AddComponent<MeshFilter>();
            wireObject.AddComponent<MeshRenderer>();

            return wireObject
                .AddComponent<SignalPathMesh>();
        }

        #endregion

        #region Electrical Bindings

        
public void BindElectricalConnection(
    SignalPath path,
    SparkCircuitConnection connection)
{
    
    if (path == null || connection == null)
        return;

    ResolveCircuitSystem();
    SubscribeCircuit();

    // Remove any previous binding for this path.
    UnbindElectricalConnection(path);

    // A connection ID must identify only one path.
    if (pathsByConnectionId.TryGetValue(
            connection.Id,
            out SignalPath existingPath) &&
        existingPath != path)
    {
        // Remove stale duplicate visual without
        // deleting the electrical connection.
        RemovePathInternal(
            existingPath,
            false);
    }

    electricalBindings[path] = connection;
    pathsByConnectionId[connection.Id] = path;

    SubscribeTerminal(connection.A);
    SubscribeTerminal(connection.B);

    // -------------------------------------------------
    // BIND WIRE CURRENT FEEDBACK
    // -------------------------------------------------

    SignalWireCurrentFeedback feedback =
        path.GetComponent<SignalWireCurrentFeedback>();

    if (feedback == null)
    {
        feedback =
            path.gameObject.AddComponent<SignalWireCurrentFeedback>();
    }

    feedback.Bind(connection);

    Debug.Log(
        $"[WIRE CURRENT BIND] " +
        $"Wire={path.name} | " +
        $"ConnectionId={connection.Id} | " +
        $"Feedback={feedback.name}",
        path);

    // -------------------------------------------------
    // REFRESH VISUAL STATE
    // -------------------------------------------------

            RefreshWireMaterial(path, connection);
            if (currentVisualPublisher == null)
        {
            currentVisualPublisher =
                FindFirstObjectByType<
                    SparkWireCurrentVisualPublisher>();
        }

        if (currentVisualPublisher != null)
        {
            currentVisualPublisher.RefreshCurrentVisuals();
        }

    Log(
        $"Bound {path.name} to connection " +
        $"{connection.Id}");
}


public void UnbindElectricalConnection(
    SignalPath path)
{
    if (path == null)
        return;

    // -----------------------------------------------------
    // 1. CLEAR RUNTIME CURRENT FEEDBACK
    // -----------------------------------------------------

    SignalWireCurrentFeedback currentFeedback =
        path.GetComponent<SignalWireCurrentFeedback>();

    if (currentFeedback != null)
    {
        currentFeedback.Unbind();
    }

    // -----------------------------------------------------
    // 2. GET EXISTING ELECTRICAL CONNECTION
    // -----------------------------------------------------

    if (!electricalBindings.TryGetValue(
        path,
        out SparkCircuitConnection connection))
    {
        return;
    }

    // -----------------------------------------------------
    // 3. REMOVE PATH-TO-CONNECTION BINDING
    // -----------------------------------------------------

    electricalBindings.Remove(path);

    // -----------------------------------------------------
    // 4. REMOVE CONNECTION ID LOOKUP
    // -----------------------------------------------------

    if (connection != null &&
        pathsByConnectionId.TryGetValue(
            connection.Id,
            out SignalPath mappedPath) &&
        mappedPath == path)
    {
        pathsByConnectionId.Remove(
            connection.Id);
    }

    // -----------------------------------------------------
    // 5. CLEAN UP TERMINAL EVENT SUBSCRIPTIONS
    // -----------------------------------------------------

    CleanupTerminalSubscriptions();
}


        public bool TryGetPath(
            ulong connectionId,
            out SignalPath path)
        {
            return pathsByConnectionId.TryGetValue(
                connectionId,
                out path) &&
                path != null;
        }

        public bool TryGetConnectionId(
            SignalPath path,
            out ulong connectionId)
        {
            connectionId = 0UL;

            if (path == null)
                return false;

            if (!electricalBindings.TryGetValue(
                path,
                out SparkCircuitConnection connection))
            {
                return false;
            }

            if (connection == null)
                return false;

            connectionId = connection.Id;
            return true;
        }

        #endregion

        #region Terminal Subscriptions

        private void SubscribeTerminal(
            SparkTerminal terminal)
        {
            if (terminal == null)
                return;

            if (!subscribedTerminals.Add(terminal))
                return;

            terminal.ElectricalStateChanged +=
                OnTerminalElectricalStateChanged;
        }

        private void CleanupTerminalSubscriptions()
        {
            if (subscribedTerminals.Count == 0)
                return;

            List<SparkTerminal> unused =
                new List<SparkTerminal>();

            foreach (SparkTerminal terminal
                in subscribedTerminals)
            {
                if (!IsTerminalStillUsed(terminal))
                    unused.Add(terminal);
            }

            foreach (SparkTerminal terminal in unused)
            {
                if (terminal != null)
                {
                    terminal.ElectricalStateChanged -=
                        OnTerminalElectricalStateChanged;
                }

                subscribedTerminals.Remove(terminal);
            }
        }

        private bool IsTerminalStillUsed(
            SparkTerminal terminal)
        {
            if (terminal == null)
                return false;

            foreach (
                KeyValuePair<
                    SignalPath,
                    SparkCircuitConnection> pair
                in electricalBindings)
            {
                SparkCircuitConnection connection =
                    pair.Value;

                if (connection == null)
                    continue;

                if (connection.A == terminal ||
                    connection.B == terminal)
                {
                    return true;
                }
            }

            return false;
        }

        private void UnsubscribeAllTerminals()
        {
            foreach (SparkTerminal terminal
                in subscribedTerminals)
            {
                if (terminal != null)
                {
                    terminal.ElectricalStateChanged -=
                        OnTerminalElectricalStateChanged;
                }
            }

            subscribedTerminals.Clear();
        }

        private void OnTerminalElectricalStateChanged(
            SparkTerminal terminal)
        {
            if (terminal == null)
                return;

            foreach (
                KeyValuePair<
                    SignalPath,
                    SparkCircuitConnection> pair
                in electricalBindings)
            {
                SparkCircuitConnection connection =
                    pair.Value;

                if (connection == null)
                    continue;

                if (connection.A != terminal &&
                    connection.B != terminal)
                {
                    continue;
                }

                RefreshWireMaterial(
                    pair.Key,
                    connection);
            }
        }

        #endregion

        #region Electrical Visuals

      
private void RefreshWireMaterial(
    SignalPath path,
    SparkCircuitConnection connection)
{
    if (path == null || connection == null)
        return;

    SparkTerminal a = connection.A;
    SparkTerminal b = connection.B;

    // -----------------------------------------------------
    // VALIDATE TERMINALS
    // -----------------------------------------------------

    if (a == null || b == null)
    {
        SignalWireElectricalGlow missingGlow =
            path.GetComponent<SignalWireElectricalGlow>();

        if (missingGlow != null)
        {
            missingGlow.SetVoltage(0f);
        }

        return;
    }

    // -----------------------------------------------------
    // READ SOLVED TERMINAL VOLTAGES
    // -----------------------------------------------------

    float voltageA = a.ElectricalState.Voltage;
    float voltageB = b.ElectricalState.Voltage;

    // -----------------------------------------------------
    // UPDATE PHYSICAL WIRE MATERIAL
    // -----------------------------------------------------

    SparkTerminalPolarity polarity =
        DeterminePolarity(
            voltageA,
            voltageB,
            a,
            b);

    UpdateWireMaterial(path, polarity);

    // -----------------------------------------------------
    // UPDATE RUNTIME ELECTRICAL GLOW
    // -----------------------------------------------------

    SignalWireElectricalGlow glow =
        path.GetComponent<SignalWireElectricalGlow>();

    if (glow == null)
        return;

    // Ideal wires normally connect terminals belonging
    // to the same electrical node.
    //
    // Use their average voltage as a representative
    // voltage relative to the solver reference.
    float representativeVoltage =
        (voltageA + voltageB) * 0.5f;

    glow.SetVoltage(representativeVoltage);
}

        private SparkTerminalPolarity DeterminePolarity(
            float voltageA,
            float voltageB,
            SparkTerminal terminalA,
            SparkTerminal terminalB)
        {
            bool aPowered =
                Mathf.Abs(voltageA) > VoltageThreshold;

            bool bPowered =
                Mathf.Abs(voltageB) > VoltageThreshold;

            if (aPowered && !bPowered)
                return GetPolarity(voltageA);

            if (!aPowered && bPowered)
                return GetPolarity(voltageB);

            if (aPowered && bPowered)
            {
                if (Mathf.Sign(voltageA) !=
                    Mathf.Sign(voltageB))
                {
                    return SparkTerminalPolarity.None;
                }

                return GetPolarity(
                    (voltageA + voltageB) * 0.5f);
            }

            SparkTerminalPolarity polarityA =
                terminalA != null
                    ? terminalA.EffectivePolarity
                    : SparkTerminalPolarity.None;

            SparkTerminalPolarity polarityB =
                terminalB != null
                    ? terminalB.EffectivePolarity
                    : SparkTerminalPolarity.None;

            if (polarityA != SparkTerminalPolarity.None &&
                polarityB == SparkTerminalPolarity.None)
            {
                return polarityA;
            }

            if (polarityB != SparkTerminalPolarity.None &&
                polarityA == SparkTerminalPolarity.None)
            {
                return polarityB;
            }

            if (polarityA == polarityB &&
                polarityA != SparkTerminalPolarity.None)
            {
                return polarityA;
            }

            return SparkTerminalPolarity.None;
        }

        private SparkTerminalPolarity GetPolarity(
            float voltage)
        {
            if (voltage > VoltageThreshold)
                return SparkTerminalPolarity.Positive;

            if (voltage < -VoltageThreshold)
                return SparkTerminalPolarity.Negative;

            return SparkTerminalPolarity.None;
        }

        public void UpdateWireMaterial(
            SignalPath path,
            SparkTerminalPolarity polarity)
        {
            if (path == null)
                return;

            SignalPathMesh mesh =
                path.GetComponent<SignalPathMesh>();

            if (mesh == null)
                return;

            mesh.SetMaterial(GetMaterial(polarity));
        }

        private Material GetMaterial(
            WirePolarity polarity)
        {
            switch (polarity)
            {
                case WirePolarity.Positive:
                    return positiveMaterial;

                case WirePolarity.Negative:
                    return negativeMaterial;

                default:
                    return neutralMaterial;
            }
        }

        private Material GetMaterial(
            SparkTerminalPolarity polarity)
        {
            switch (polarity)
            {
                case SparkTerminalPolarity.Positive:
                    return positiveMaterial;

                case SparkTerminalPolarity.Negative:
                    return negativeMaterial;

                default:
                    return neutralMaterial;
            }
        }

        #endregion

        #region Path Access

        public SignalPath GetCurrentPath()
        {
            return currentPath;
        }

        public SignalPathMesh GetCurrentMesh()
        {
            return currentMesh;
        }

        public SignalPathMesh GetMeshForPath(
            SignalPath path)
        {
            if (path == null)
                return null;

            return path.GetComponent<SignalPathMesh>();
        }

        public void FinishCurrentPath()
        {
            currentPath = null;
            currentMesh = null;
        }

        #endregion

        #region Advanced Wire Management

        /// <summary>
        /// Remove a specific wire by connection ID.
        /// </summary>
        public bool RemoveWire(ulong connectionId)
        {
            if (!pathsByConnectionId.TryGetValue(
                connectionId,
                out SignalPath path))
            {
                return false;
            }

            return RemovePathInternal(path, true);
        }

        /// <summary>
        /// Remove a specific wire by visual path.
        /// </summary>
        public bool RemoveWire(SignalPath path)
        {
            return RemovePathInternal(path, true);
        }

        /// <summary>
        /// Compatibility method used by SignalWireBuilder.
        /// </summary>
        public bool RemovePath(SignalPath path)
        {
            return RemovePathInternal(path, true);
        }

        /// <summary>
        /// Undo the newest visual wire.
        /// </summary>
        public bool UndoLastWire()
        {
            for (int i = creationOrder.Count - 1;
                 i >= 0;
                 i--)
            {
                SignalPath path = creationOrder[i];

                if (path == null)
                {
                    creationOrder.RemoveAt(i);
                    continue;
                }

                return RemovePathInternal(path, true);
            }

            return false;
        }

        /// <summary>
        /// Remove all drawn wires.
        /// </summary>
        public int UndoAllWires()
        {
            return ClearAllWiresInternal();
        }

        /// <summary>
        /// Reset drawn wires during a level change.
        /// </summary>
        public void ClearLevelWires()
        {
            ClearAllWiresInternal();
        }

        /// <summary>
        /// Compatibility with older code.
        /// </summary>
        public void ClearPaths()
        {
            ClearAllWiresInternal();
        }

       
private bool RemovePathInternal(
    SignalPath path,
    bool removeElectricalConnection)
{
    if (path == null)
        return false;

    int index = Array.IndexOf(paths, path);

    if (index < 0)
        return false;

    bool hasConnection =
    electricalBindings.TryGetValue(
        path,
        out SparkCircuitConnection connection);

ulong connectionId =
    hasConnection ? connection.Id : 0UL;
    // First remove this path from the manager's
    // lookup tables. This prevents recursive removal
    // when the circuit raises ConnectionRemoved.
    UnbindElectricalConnection(path);

    List<SignalPath> updated =
        new List<SignalPath>(paths);

    updated.RemoveAt(index);
    paths = updated.ToArray();

    creationOrder.Remove(path);

    if (currentPath == path)
    {
        currentPath = null;
        currentMesh = null;
    }

    // Hide the visual cable immediately.
    path.gameObject.SetActive(false);

    // Remove the actual electrical connection.
   if (removeElectricalConnection && hasConnection)
    {
        if (circuitSystem == null)
            ResolveCircuitSystem();

        
        if (circuitSystem != null)
        {
            int connectionsBefore =
                circuitSystem.ConnectionCount;

            bool removed =
                circuitSystem.TryRemoveConnection(
                    connectionId,
                    out SparkCircuitConnection removedConnection);

            int connectionsAfter =
                circuitSystem.ConnectionCount;

            if (removed)
            {
                Debug.Log(
                    $"[WIRE REMOVE SUCCESS] " +
                    $"Connection ID: {connectionId} | " +
                    $"Connections: {connectionsBefore} -> {connectionsAfter}",
                    this);
            }
            else
            {
                Debug.LogWarning(
                    $"[WIRE REMOVE FAILED] " +
                    $"Connection ID: {connectionId} | " +
                    $"Connections: {connectionsBefore} -> {connectionsAfter}. " +
                    "Connection was not found in SparkCircuitSystem.",
                    this);
            }
        }
        else
        {
            Debug.LogError(
                "[WIRE REMOVE ERROR] SparkCircuitSystem is missing. " +
                "The visual wire may be removed, but its electrical " +
                "connection cannot be removed.",
                this);
        }

            }

            if (Application.isPlaying)
                Destroy(path.gameObject);
            else
                DestroyImmediate(path.gameObject);

            return true;
        }

        private int ClearAllWiresInternal()
        {
            if (clearingPaths)
                return 0;

            clearingPaths = true;

            int removed = 0;

            try
            {
                SignalPath[] snapshot =
                    (SignalPath[])paths.Clone();

                for (int i = snapshot.Length - 1;
                     i >= 0;
                     i--)
                {
                    if (snapshot[i] != null &&
                        RemovePathInternal(
                            snapshot[i],
                            true))
                    {
                        removed++;
                    }
                }

                paths = Array.Empty<SignalPath>();

                electricalBindings.Clear();
                pathsByConnectionId.Clear();
                creationOrder.Clear();

                UnsubscribeAllTerminals();

                currentPath = null;
                currentMesh = null;
                currentIndex = -1;
            }
            finally
            {
                clearingPaths = false;
            }

            Log(
                $"Cleared {removed} drawn wires.");

            return removed;
        }

        #endregion

        #region Unity UI Button Callbacks

        public void OnUndoLastWireClicked()
        {
            UndoLastWire();
        }

        public void OnUndoAllWiresClicked()
        {
            UndoAllWires();
        }

        public void OnClearLevelWiresClicked()
        {
            ClearLevelWires();
        }

        #endregion

        #region Debug

        private void Log(string message)
        {
            if (!logOperations)
                return;

            Debug.Log(
                "[SignalPath_Manager] " + message,
                this);
        }

        #endregion
    }
}
