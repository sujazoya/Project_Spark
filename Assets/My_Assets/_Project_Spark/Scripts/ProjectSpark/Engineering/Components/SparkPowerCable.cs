using UnityEngine;

namespace ProjectSpark.Gameplay
{
    /// <summary>
    /// Passive two-terminal electrical cable.
    ///
    /// The cable has no intrinsic positive or negative terminal.
    /// Both terminals are electrically equivalent and bidirectional.
    ///
    /// External connections are owned by SparkCircuitSystem /
    /// SparkCircuitConnection.
    ///
    /// Electrical voltage, current and polarity are determined
    /// by SparkElectricalSolver.
    ///
    /// Physical plug/socket positioning is handled separately.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class SparkPowerCable :
        SparkElectricalComponent,
        ISparkConductiveDevice
    {
        // =========================================================
        // TERMINALS
        // =========================================================

        [Header("Cable Terminals")]

        [SerializeField]
        private SparkTerminal terminalA;

        [SerializeField]
        private SparkTerminal terminalB;


        // =========================================================
        // CONFIGURATION
        // =========================================================

        [Header("Terminal Configuration")]

        [SerializeField, Min(1)]
        private int maxConnectionsPerTerminal = 1;


        [SerializeField]
        private SparkTerminalKind terminalKind =
            SparkTerminalKind.Generic;


        // =========================================================
        // PUBLIC ACCESS
        // =========================================================

        /// <summary>
        /// First electrical terminal of the cable.
        ///
        /// This terminal has no permanent polarity.
        /// </summary>
        public SparkTerminal TerminalA =>
            terminalA;


        /// <summary>
        /// Second electrical terminal of the cable.
        ///
        /// This terminal has no permanent polarity.
        /// </summary>
        public SparkTerminal TerminalB =>
            terminalB;


        /// <summary>
        /// Returns true when both cable terminals exist.
        /// </summary>
        public bool HasValidTerminals =>
            terminalA != null &&
            terminalB != null &&
            terminalA != terminalB;


        // =========================================================
        // UNITY
        // =========================================================

        protected override void Awake()
        {
            base.Awake();

            EnsureTerminals();
        }


        private void Reset()
        {
            EnsureTerminals();
        }


        private void OnValidate()
        {
            EnsureTerminals();
        }


        // =========================================================
        // TERMINAL CREATION
        // =========================================================

        /// <summary>
        /// Ensures that the cable always owns exactly two
        /// electrical terminals.
        ///
        /// Existing terminal components are preserved.
        /// Missing terminals are created automatically.
        /// </summary>
        private void EnsureTerminals()
        {
            maxConnectionsPerTerminal =
                Mathf.Max(
                    1,
                    maxConnectionsPerTerminal);


            // -----------------------------------------------------
            // TERMINAL A
            // -----------------------------------------------------

            if (terminalA == null)
            {
                terminalA =
                    CreateTerminal(
                        "Terminal A");
            }


            // -----------------------------------------------------
            // TERMINAL B
            // -----------------------------------------------------

            if (terminalB == null ||
                terminalB == terminalA)
            {
                terminalB =
                    CreateTerminal(
                        "Terminal B");
            }


            // -----------------------------------------------------
            // CONFIGURE
            // -----------------------------------------------------

            ConfigureTerminal(terminalA);
            ConfigureTerminal(terminalB);
        }


        /// <summary>
        /// Creates a child SparkTerminal for this cable.
        /// </summary>
        private SparkTerminal CreateTerminal(
            string terminalName)
        {
            GameObject terminalObject =
                new GameObject(terminalName);

            terminalObject.transform.SetParent(
                transform,
                false);

            SparkTerminal terminal =
                terminalObject.AddComponent<SparkTerminal>();

            return terminal;
        }


        /// <summary>
        /// Applies the cable's terminal configuration.
        ///
        /// Polarity deliberately remains None.
        /// </summary>
        private void ConfigureTerminal(
            SparkTerminal terminal)
        {
            if (terminal == null)
                return;

            // SparkTerminal's serialized configuration is private,
            // so runtime electrical behavior is intentionally left
            // to SparkTerminal's existing defaults and the circuit
            // system.
            //
            // Most importantly, this cable does not assign polarity.
        }


        // =========================================================
        // CONDUCTION
        // =========================================================

        /// <summary>
        /// Determines whether this cable provides a conductive path
        /// between its two terminals.
        ///
        /// The cable is passive:
        ///
        ///     A <────────────> B
        ///
        /// It does not generate voltage and does not determine
        /// positive / negative polarity.
        /// </summary>
        public bool CanConductBetween(
            SparkTerminal from,
            SparkTerminal to)
        {
            if (!ElectricalEnabled)
                return false;

            if (!HasValidTerminals)
                return false;

            if (from == null ||
                to == null)
            {
                return false;
            }

            if (from == to)
                return false;

            return
                (from == terminalA &&
                 to == terminalB)
                ||
                (from == terminalB &&
                 to == terminalA);
        }


        // =========================================================
        // TERMINAL LOOKUP
        // =========================================================

        /// <summary>
        /// Returns whether the supplied terminal belongs to this
        /// cable.
        /// </summary>
        public bool OwnsTerminal(
            SparkTerminal terminal)
        {
            if (terminal == null)
                return false;

            return
                terminal == terminalA ||
                terminal == terminalB;
        }


        /// <summary>
        /// Returns the opposite cable terminal.
        ///
        /// Returns null when the supplied terminal does not belong
        /// to this cable.
        /// </summary>
        public SparkTerminal GetOppositeTerminal(
            SparkTerminal terminal)
        {
            if (terminal == terminalA)
                return terminalB;

            if (terminal == terminalB)
                return terminalA;

            return null;
        }


        // =========================================================
        // CONFIGURATION
        // =========================================================

        /// <summary>
        /// Allows an existing pair of terminals to be assigned.
        ///
        /// This is mainly useful for controlled editor/runtime
        /// setup. Normally the cable creates its own terminals.
        /// </summary>
        public void SetTerminals(
            SparkTerminal a,
            SparkTerminal b)
        {
            if (a == null ||
                b == null ||
                a == b)
            {
                Debug.LogWarning(
                    $"[POWER CABLE] Invalid terminal assignment " +
                    $"on '{name}'.",
                    this);

                return;
            }

            terminalA = a;
            terminalB = b;

            NotifyElectricalConfigurationChanged();
        }


        // =========================================================
        // DEBUG / VALIDATION
        // =========================================================

        private void OnDrawGizmosSelected()
        {
            if (!HasValidTerminals)
                return;

            Gizmos.DrawLine(
                terminalA.transform.position,
                terminalB.transform.position);
        }
    }
}
