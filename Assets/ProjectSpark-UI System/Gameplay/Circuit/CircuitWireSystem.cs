using AAAUI.VFX;
using ProjectSpark.Circuit;
using ProjectSpark.Gameplay;
using UnityEngine;

namespace AAAUI
{
    [DisallowMultipleComponent]
    public sealed class CircuitWireSystem : MonoBehaviour
    {
        [Header("Wire")]

        [SerializeField]
        private SignalPathMesh wirePrefab;

        [SerializeField]
        private Transform wireRoot;


        [Header("Materials")]

        [SerializeField]
        private Material positiveWireMaterial;

        [SerializeField]
        private Material negativeWireMaterial;

        [SerializeField]
        private Material neutralWireMaterial;


        [Header("Circuit")]

        [SerializeField]
        private SparkCircuitSystem circuitSystem;


        [Header("Level Integration")]

        [SerializeField]
        private Level1CircuitChecker level1Checker;


        [Header("Connection")]

        [SerializeField]
        private SparkConnectionDirection connectionDirection =
            SparkConnectionDirection.Bidirectional;


        private SignalPathMesh currentWire;

        private SparkTerminal currentStartTerminal;

        private WirePolarity currentPolarity;

        private readonly System.Collections.Generic.List<GameObject>
            wires =
            new System.Collections.Generic.List<GameObject>();


        // =========================================================
        // PUBLIC STATE
        // =========================================================

        public bool HasActiveWire =>
            currentWire != null;

        public SparkTerminal CurrentStartTerminal =>
            currentStartTerminal;

        public WirePolarity CurrentPolarity =>
            currentPolarity;


        // =========================================================
        // UNITY
        // =========================================================

        private void Awake()
        {
            if (circuitSystem == null)
            {
                circuitSystem =
                    GetComponentInParent<SparkCircuitSystem>();
            }
        }


        // =========================================================
        // BEGIN WIRE
        // =========================================================

        public bool BeginWire(
            Vector3 startPosition,
            SparkTerminal startTerminal)
        {
            if (startTerminal == null)
            {
                Debug.LogWarning(
                    "[WIRE] Cannot start. Terminal is NULL.",
                    this);

                return false;
            }


            if (wirePrefab == null)
            {
                Debug.LogError(
                    "[WIRE] Wire prefab is missing.",
                    this);

                return false;
            }


            if (wireRoot == null)
            {
                Debug.LogError(
                    "[WIRE] Wire root is missing.",
                    this);

                return false;
            }


            // -----------------------------------------------------
            // CANCEL PREVIOUS
            // -----------------------------------------------------

            CancelCurrentWire();


            // -----------------------------------------------------
            // TERMINAL OWNS POLARITY
            // -----------------------------------------------------

            currentStartTerminal =
                startTerminal;


            currentPolarity =
                ConvertPolarity(
                    startTerminal.Polarity);


            // -----------------------------------------------------
            // CREATE VISUAL
            // -----------------------------------------------------

            SignalPathMesh wire =
                Instantiate(
                    wirePrefab,
                    wireRoot);


            wire.transform.position =
                Vector3.zero;


            // -----------------------------------------------------
            // MATERIAL
            // -----------------------------------------------------

            Material material =
                GetMaterial(
                    startTerminal.Polarity);


            if (material != null)
            {
                wire.SetMaterial(
                    material);
            }


            // -----------------------------------------------------
            // STORE
            // -----------------------------------------------------

            currentWire =
                wire;


            wires.Add(
                wire.gameObject);


            Debug.Log(
                $"[WIRE] START = " +
                $"{startTerminal.name} | " +
                $"Kind = {startTerminal.Kind} | " +
                $"Polarity = {startTerminal.Polarity}",
                startTerminal);


            return true;
        }


        // =========================================================
        // COMPLETE
        // =========================================================

        public bool CompleteWire(
            SparkTerminal endTerminal)
        {
            if (currentWire == null)
            {
                Debug.LogWarning(
                    "[WIRE] Cannot complete. " +
                    "No active wire.",
                    this);

                return false;
            }


            if (currentStartTerminal == null)
            {
                Debug.LogWarning(
                    "[WIRE] Start terminal is missing.",
                    this);

                CancelCurrentWire();

                return false;
            }


            if (endTerminal == null)
            {
                Debug.LogWarning(
                    "[WIRE] End terminal is NULL.",
                    this);

                return false;
            }


            if (endTerminal ==
                currentStartTerminal)
            {
                Debug.LogWarning(
                    "[WIRE] Cannot connect terminal to itself.",
                    this);

                return false;
            }


            Debug.Log(
                $"[WIRE] TERMINAL HIT = " +
                $"{endTerminal.name}",
                endTerminal);


            // -----------------------------------------------------
            // CIRCUIT SYSTEM
            // -----------------------------------------------------

            if (circuitSystem == null)
            {
                Debug.LogError(
                    "[WIRE] SparkCircuitSystem is missing.",
                    this);

                return false;
            }


            SparkResult result =
                circuitSystem.TryCreateConnection(
                    currentStartTerminal,
                    endTerminal,
                    SparkConnectionKind.Wire,
                    connectionDirection,
                    out SparkCircuitConnection connection);


            if (!result.Succeeded)
            {
                Debug.LogWarning(
                    $"[WIRE] Connection rejected: " +
                    $"{result.Message}",
                    this);

                return false;
            }


            // -----------------------------------------------------
            // SUCCESS
            // -----------------------------------------------------

            Debug.Log(
                $"[WIRE] CONNECTED = " +
                $"{connection.A.name} → " +
                $"{connection.B.name} | " +
                $"Polarity = " +
                $"{connection.A.Polarity}",
                this);


            // -----------------------------------------------------
            // OPTIONAL LEVEL CALLBACK
            // -----------------------------------------------------

            if (level1Checker != null)
            {
                level1Checker.RegisterConnection(
                    connection.A,
                    connection.B);
            }


            // -----------------------------------------------------
            // FINISH
            // -----------------------------------------------------

            currentWire = null;

            currentStartTerminal = null;

            currentPolarity =
                WirePolarity.Neutral;


            return true;
        }


        // =========================================================
        // CANCEL
        // =========================================================

        public void CancelCurrentWire()
        {
            if (currentWire != null)
            {
                GameObject wireObject =
                    currentWire.gameObject;


                if (wireObject != null)
                {
                    wires.Remove(
                        wireObject);

                    Destroy(
                        wireObject);
                }
            }


            currentWire = null;

            currentStartTerminal = null;

            currentPolarity =
                WirePolarity.Neutral;
        }


        // =========================================================
        // CLEAR
        // =========================================================

        public void ClearWires()
        {
            for (
                int i = wires.Count - 1;
                i >= 0;
                i--)
            {
                if (wires[i] != null)
                {
                    Destroy(
                        wires[i]);
                }
            }


            wires.Clear();


            currentWire = null;

            currentStartTerminal = null;

            currentPolarity =
                WirePolarity.Neutral;
        }


        // =========================================================
        // POLARITY
        // =========================================================

        private static WirePolarity ConvertPolarity(
            SparkTerminalPolarity polarity)
        {
            switch (polarity)
            {
                case SparkTerminalPolarity.Positive:
                    return WirePolarity.Positive;

                case SparkTerminalPolarity.Negative:
                    return WirePolarity.Negative;

                default:
                    return WirePolarity.Neutral;
            }
        }


        // =========================================================
        // MATERIAL
        // =========================================================

        private Material GetMaterial(
            SparkTerminalPolarity polarity)
        {
            switch (polarity)
            {
                case SparkTerminalPolarity.Positive:
                    return positiveWireMaterial;

                case SparkTerminalPolarity.Negative:
                    return negativeWireMaterial;

                default:
                    return neutralWireMaterial;
            }
        }
    }
}