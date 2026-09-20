using System;
using System.Collections.Generic;
using UnityEngine;
using ProjectSpark.Circuit;
using ProjectSpark.Electrical;
using ProjectSpark.Gameplay;

namespace AAAUI.VFX
{
    /// <summary>
    /// Manages visual signal paths and synchronizes their materials
    /// with the electrical state of their connected terminals.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class SignalPath_Manager : MonoBehaviour
    {
        #region Inspector

        [Header("Wire")]
        [SerializeField]
        private SignalPathMesh meshPrefab;

        [SerializeField]
        private Material positiveMaterial;

        [SerializeField]
        private Material negativeMaterial;

        [SerializeField]
        private Material neutralMaterial;

        [Header("Paths")]
        [SerializeField]
        private SignalPath[] paths = Array.Empty<SignalPath>();

        #endregion

        #region Runtime State

        private readonly Dictionary<SignalPath, SparkCircuitConnection>
            electricalBindings =
                new Dictionary<SignalPath, SparkCircuitConnection>();

        private readonly HashSet<SparkTerminal>
            subscribedTerminals =
                new HashSet<SparkTerminal>();

        private int currentIndex = -1;

        private SignalPath currentPath;
        private SignalPathMesh currentMesh;

        #endregion

        #region Constants

        private const float VoltageThreshold = 0.05f;

        #endregion

        #region Properties

        /// <summary>
        /// Gets the currently active signal path.
        /// </summary>
        public SignalPath CurrentPath => currentPath;

        /// <summary>
        /// Gets the mesh belonging to the currently active signal path.
        /// </summary>
        public SignalPathMesh CurrentMesh => currentMesh;

        /// <summary>
        /// Gets the index of the currently active path.
        /// </summary>
        public int CurrentIndex => currentIndex;

        /// <summary>
        /// Gets the number of created paths.
        /// </summary>
        public int Count => paths.Length;

        /// <summary>
        /// Gets all created signal paths.
        /// </summary>
        public SignalPath[] Paths => paths;

        #endregion

        #region Electrical Binding

        /// <summary>
        /// Binds a signal path to an electrical connection.
        /// The wire material is immediately synchronized with
        /// the current electrical state.
        /// </summary>
        public void BindElectricalConnection(
            SignalPath path,
            SparkCircuitConnection connection)
        {
            if (path == null || connection == null)
                return;

            UnbindElectricalConnection(path);

            electricalBindings[path] = connection;

            SubscribeTerminal(connection.A);
            SubscribeTerminal(connection.B);

            RefreshWireMaterial(path, connection);
        }

        /// <summary>
        /// Removes the electrical binding from a signal path.
        /// </summary>
        public void UnbindElectricalConnection(
            SignalPath path)
        {
            if (path == null)
                return;

            if (!electricalBindings.Remove(path))
                return;

            CleanupTerminalSubscriptions();
        }

        #endregion

        #region Terminal Subscription

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

            List<SparkTerminal> terminalsToRemove =
                new List<SparkTerminal>();

            foreach (SparkTerminal terminal in subscribedTerminals)
            {
                if (!IsTerminalStillUsed(terminal))
                    terminalsToRemove.Add(terminal);
            }

            foreach (SparkTerminal terminal in terminalsToRemove)
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

        #region Electrical Visual State

       private void RefreshWireMaterial(
    SignalPath path,
    SparkCircuitConnection connection)
{
    if (path == null || connection == null)
        return;

    SparkTerminal terminalA = connection.A;
    SparkTerminal terminalB = connection.B;

    if (terminalA == null || terminalB == null)
        return;

   SparkTerminalPolarity polarity =
    DeterminePolarity(
        terminalA.ElectricalState.Voltage,
        terminalB.ElectricalState.Voltage,
        terminalA,
        terminalB);

            Debug.Log(
    $"[WIRE POLARITY] {path.name} | " +
    $"A={terminalA.name} " +
    $"V={terminalA.ElectricalState.Voltage:F3} " +
    $"P={terminalA.SolvedPolarity} | " +
    $"B={terminalB.name} " +
    $"V={terminalB.ElectricalState.Voltage:F3} " +
    $"P={terminalB.SolvedPolarity} | " +
    $"WIRE={polarity}");

    UpdateWireMaterial(
        path,
        polarity);
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

    // Only A has meaningful solved voltage.
    if (aPowered && !bPowered)
        return GetPolarity(voltageA);

    // Only B has meaningful solved voltage.
    if (!aPowered && bPowered)
        return GetPolarity(voltageB);

    // Both have meaningful solved voltage.
    if (aPowered && bPowered)
    {
        // Opposite voltage signs indicate a polarity conflict.
        if (Mathf.Sign(voltageA) !=
            Mathf.Sign(voltageB))
        {
            return SparkTerminalPolarity.None;
        }

        return GetPolarity(
            (voltageA + voltageB) * 0.5f);
    }

    // Neither terminal has meaningful solved voltage.
    //
    // Use authored/runtime polarity only as a
    // visual fallback.
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

        #endregion

        #region Wire Creation

        /// <summary>
        /// Creates a new signal wire.
        /// Existing wires remain untouched.
        /// </summary>
        public SignalPathMesh CreateNextWire(
            WirePolarity polarity)
        {
            currentIndex++;

            SignalPathMesh newMesh =
                CreateWireMesh(currentIndex);

            SignalPath newPath =
                newMesh.GetComponent<SignalPath>();

            if (newPath == null)
            {
                newPath =
                    newMesh.gameObject.AddComponent<SignalPath>();
            }

            newMesh.SetPath(newPath);

            newMesh.SetMaterial(
                GetMaterial(polarity));

            Array.Resize(
                ref paths,
                currentIndex + 1);

            paths[currentIndex] =
                newPath;

            currentPath =
                newPath;

            currentMesh =
                newMesh;

            return newMesh;
        }

        private SignalPathMesh CreateWireMesh(
            int index)
        {
            if (meshPrefab != null)
            {
                SignalPathMesh mesh =
                    Instantiate(
                        meshPrefab,
                        transform);

                mesh.name =
                    $"SignalWire_{index}";

                return mesh;
            }

            GameObject wireObject =
                new GameObject(
                    $"SignalWire_{index}");

            wireObject.transform.SetParent(
                transform,
                false);

            wireObject.AddComponent<MeshFilter>();
            wireObject.AddComponent<MeshRenderer>();

            return wireObject.AddComponent<SignalPathMesh>();
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

                case WirePolarity.Neutral:
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

                case SparkTerminalPolarity.None:
                default:
                    return neutralMaterial;
            }
        }

        #endregion

        #region Path Access

        /// <summary>
        /// Gets the path currently being edited.
        /// </summary>
        public SignalPath GetCurrentPath()
        {
            return currentPath;
        }

        /// <summary>
        /// Gets the mesh belonging to the current path.
        /// </summary>
        public SignalPathMesh GetCurrentMesh()
        {
            return currentMesh;
        }

        /// <summary>
        /// Gets the mesh associated with a specific path.
        /// </summary>
        public SignalPathMesh GetMeshForPath(
            SignalPath path)
        {
            if (path == null)
                return null;

            return path.GetComponent<SignalPathMesh>();
        }

        /// <summary>
        /// Ends editing of the current path without deleting it.
        /// </summary>
        public void FinishCurrentPath()
        {
            currentPath = null;
            currentMesh = null;
        }

        #endregion

        #region Material Control

        /// <summary>
        /// Updates the visual material of a signal path.
        /// </summary>
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

            mesh.SetMaterial(
                GetMaterial(polarity));
        }

        #endregion

        #region Cleanup

        /// <summary>
        /// Deletes all created signal paths and clears
        /// their electrical bindings.
        /// </summary>
        public void ClearPaths()
        {
            UnsubscribeAllTerminals();

            electricalBindings.Clear();

            for (int i = 0; i < paths.Length; i++)
            {
                SignalPath path = paths[i];

                if (path == null)
                    continue;

                Destroy(path.gameObject);
            }

            paths =
                Array.Empty<SignalPath>();

            currentIndex = -1;

            currentPath = null;
            currentMesh = null;
        }

        private void UnsubscribeAllTerminals()
        {
            foreach (SparkTerminal terminal
                in subscribedTerminals)
            {
                if (terminal == null)
                    continue;

                terminal.ElectricalStateChanged -=
                    OnTerminalElectricalStateChanged;
            }

            subscribedTerminals.Clear();
        }

        private void OnDestroy()
        {
            UnsubscribeAllTerminals();
        }

        #endregion
    }
}
