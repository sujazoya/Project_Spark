using System;
using System.Collections.Generic;
using ProjectSpark.Circuit;
using UnityEngine;

namespace ProjectSpark.Gameplay
{
    [DisallowMultipleComponent]
    public sealed class Level1CircuitChecker : MonoBehaviour
    {
        [Header("Circuit System")]
        [SerializeField]
        private SparkCircuitSystem circuitSystem;

        [Header("Correct Terminals")]

        [Tooltip("Power supply positive terminal.")]
        [SerializeField]
        private SparkTerminal positiveSource;

        [Tooltip("Light/LED positive terminal.")]
        [SerializeField]
        private SparkTerminal positiveLight;

        [Tooltip("Power supply negative/ground terminal.")]
        [SerializeField]
        private SparkTerminal negativeSource;

        [Tooltip("Light/LED negative terminal.")]
        [SerializeField]
        private SparkTerminal negativeLight;

        [Header("Light")]
        [SerializeField]
        private GameObject lightObject;

        [Header("Debug")]
        [SerializeField]
        private bool logConnections = true;

        [SerializeField]
        private bool logTopology = true;

       [SerializeField] private bool positiveConnected;
        [SerializeField] private bool negativeConnected;
        [SerializeField] private bool gameWon;

        private readonly List<SparkCircuitConnection> connectionBuffer =
            new List<SparkCircuitConnection>();

        public bool IsGameWon => gameWon;
        public bool PositiveConnected => positiveConnected;
        public bool NegativeConnected => negativeConnected;

        // =========================================================
        // UNITY
        // =========================================================

        private void Reset()
        {
            circuitSystem =
                GetComponentInParent<SparkCircuitSystem>();
        }

        private void Awake()
        {
            if (circuitSystem == null)
            {
                circuitSystem =
                    GetComponentInParent<SparkCircuitSystem>();
            }

            if (lightObject != null)
                lightObject.SetActive(false);
        }

        private void OnEnable()
        {
            if (circuitSystem == null)
                return;

            circuitSystem.ConnectionCreated +=
                OnConnectionCreated;

            circuitSystem.ConnectionRemoved +=
                OnConnectionRemoved;

            circuitSystem.TopologyChanged +=
                OnTopologyChanged;

            RecalculateCircuit();
        }

        private void OnDisable()
        {
            if (circuitSystem == null)
                return;

            circuitSystem.ConnectionCreated -=
                OnConnectionCreated;

            circuitSystem.ConnectionRemoved -=
                OnConnectionRemoved;

            circuitSystem.TopologyChanged -=
                OnTopologyChanged;
        }

        // =========================================================
        // CONNECTION CREATED
        // =========================================================

        private void OnConnectionCreated(
            SparkCircuitConnection connection)
        {
            if (logConnections)
            {
                Debug.Log(
                    $"[LEVEL 1] Connection Created: " +
                    $"{GetName(connection.A)} → " +
                    $"{GetName(connection.B)}",
                    this);
            }

            RecalculateCircuit();
        }

        // =========================================================
        // CONNECTION REMOVED
        // =========================================================

        private void OnConnectionRemoved(
            SparkCircuitConnection connection)
        {
            if (logConnections)
            {
                Debug.Log(
                    $"[LEVEL 1] Connection Removed: " +
                    $"{GetName(connection.A)} → " +
                    $"{GetName(connection.B)}",
                    this);
            }

            RecalculateCircuit();
        }

        // =========================================================
        // TOPOLOGY CHANGED
        // =========================================================

        private void OnTopologyChanged()
        {
            if (logTopology)
            {
                Debug.Log(
                    $"[LEVEL 1] Topology changed. " +
                    $"Version: {circuitSystem.TopologyVersion}",
                    this);
            }

            RecalculateCircuit();
        }

        // =========================================================
        // RECALCULATE
        // =========================================================

        private void RecalculateCircuit()
        {
            positiveConnected = false;
            negativeConnected = false;

            if (circuitSystem == null)
            {
                SetLight(false);
                return;
            }

            // -----------------------------------------------------
            // CHECK POSITIVE SOURCE
            // -----------------------------------------------------

            if (positiveSource != null)
            {
                circuitSystem.GetConnections(
                    positiveSource,
                    connectionBuffer);

                for (int i = 0;
                     i < connectionBuffer.Count;
                     i++)
                {
                    SparkCircuitConnection connection =
                        connectionBuffer[i];

                    if (IsPair(
                            connection.A,
                            connection.B,
                            positiveSource,
                            positiveLight))
                    {
                        positiveConnected = true;
                        break;
                    }
                }
            }

            // -----------------------------------------------------
            // CHECK NEGATIVE SOURCE
            // -----------------------------------------------------

            if (negativeSource != null)
            {
                circuitSystem.GetConnections(
                    negativeSource,
                    connectionBuffer);

                for (int i = 0;
                     i < connectionBuffer.Count;
                     i++)
                {
                    SparkCircuitConnection connection =
                        connectionBuffer[i];

                    if (IsPair(
                            connection.A,
                            connection.B,
                            negativeSource,
                            negativeLight))
                    {
                        negativeConnected = true;
                        break;
                    }
                }
            }

            // -----------------------------------------------------
            // DEBUG
            // -----------------------------------------------------

            if (logTopology)
            {
                Debug.Log(
                    $"[LEVEL 1] " +
                    $"Positive={positiveConnected} | " +
                    $"Negative={negativeConnected} | " +
                    $"Connections={circuitSystem.ConnectionCount}",
                    this);
            }

            CheckWin();
        }

        // =========================================================
        // PAIR CHECK
        // =========================================================

        private bool IsPair(
            SparkTerminal a,
            SparkTerminal b,
            SparkTerminal first,
            SparkTerminal second)
        {
            if (first == null ||
                second == null)
            {
                return false;
            }

            return
                (a == first && b == second) ||
                (a == second && b == first);
        }

        // =========================================================
        // WIN
        // =========================================================

        private void CheckWin()
        {
            if (gameWon)
                return;

            if (!positiveConnected ||
                !negativeConnected)
            {
                SetLight(false);
                return;
            }

            gameWon = true;

            Debug.Log(
                "[LEVEL 1] GAME WON!",
                this);

            SetLight(true);
        }

        // =========================================================
        // LIGHT
        // =========================================================

        private void SetLight(bool state)
        {
            if (lightObject != null)
                lightObject.SetActive(state);
        }

        // =========================================================
        // RESET
        // =========================================================

        public void ResetLevel()
        {
            positiveConnected = false;
            negativeConnected = false;
            gameWon = false;

            SetLight(false);

            Debug.Log(
                "[LEVEL 1] RESET",
                this);

            RecalculateCircuit();
        }

        // =========================================================
        // DEBUG STATUS
        // =========================================================

        public void DebugStatus()
        {
            Debug.Log(
                "\n" +
                "==============================\n" +
                "[LEVEL 1 STATUS]\n" +
                "==============================\n" +
                $"Positive Source : {GetName(positiveSource)}\n" +
                $"Positive Light  : {GetName(positiveLight)}\n" +
                $"Negative Source : {GetName(negativeSource)}\n" +
                $"Negative Light  : {GetName(negativeLight)}\n" +
                "\n" +
                $"Positive OK     : {positiveConnected}\n" +
                $"Negative OK     : {negativeConnected}\n" +
                $"GAME WON        : {gameWon}\n" +
                "\n" +
                $"Connections     : " +
                $"{(circuitSystem != null ? circuitSystem.ConnectionCount : 0)}\n" +
                $"Topology        : " +
                $"{(circuitSystem != null ? circuitSystem.TopologyVersion : 0)}\n" +
                "==============================",
                this);
        }

        // =========================================================
        // DEBUG CONNECTIONS
        // =========================================================

        public void DebugConnections()
        {
            if (circuitSystem == null)
            {
                Debug.LogError(
                    "[LEVEL 1] Circuit system is missing.",
                    this);

                return;
            }

            Debug.Log(
                $"[LEVEL 1] Total connections: " +
                $"{circuitSystem.ConnectionCount}",
                this);

            DebugTerminal(positiveSource);
            DebugTerminal(positiveLight);
            DebugTerminal(negativeSource);
            DebugTerminal(negativeLight);
        }

        private void DebugTerminal(
            SparkTerminal terminal)
        {
            if (terminal == null)
                return;

            circuitSystem.GetConnections(
                terminal,
                connectionBuffer);

            Debug.Log(
                $"[LEVEL 1] Terminal " +
                $"{terminal.name}: " +
                $"{connectionBuffer.Count} connection(s).",
                this);

            for (int i = 0;
                 i < connectionBuffer.Count;
                 i++)
            {
                SparkCircuitConnection connection =
                    connectionBuffer[i];

                Debug.Log(
                    $"    {connection.Id}: " +
                    $"{GetName(connection.A)} ↔ " +
                    $"{GetName(connection.B)} | " +
                    $"Kind={connection.Kind} | " +
                    $"Direction={connection.Direction}",
                    this);
            }
        }
        // =========================================================
// LEGACY / DIRECT CONNECTION REGISTRATION
// =========================================================

public void RegisterConnection(
    SparkTerminal a,
    SparkTerminal b)
{
    if (a == null || b == null)
        return;

    if (logConnections)
    {
        Debug.Log(
            $"[LEVEL 1] Connection: {a.name} → {b.name}",
            this);
    }

    // -----------------------------------------------------
    // POSITIVE
    // -----------------------------------------------------

    if (IsPair(
        a,
        b,
        positiveSource,
        positiveLight))
    {
        positiveConnected = true;

        Debug.Log(
            "[LEVEL 1] + → + CORRECT",
            this);
    }

    // -----------------------------------------------------
    // NEGATIVE
    // -----------------------------------------------------

    if (IsPair(
        a,
        b,
        negativeSource,
        negativeLight))
    {
        negativeConnected = true;

        Debug.Log(
            "[LEVEL 1] - → - CORRECT",
            this);
    }

    CheckWin();
}

        // =========================================================
        // NAME
        // =========================================================

        private string GetName(
            SparkTerminal terminal)
        {
            return terminal != null
                ? terminal.name
                : "NULL";
        }
    }
}