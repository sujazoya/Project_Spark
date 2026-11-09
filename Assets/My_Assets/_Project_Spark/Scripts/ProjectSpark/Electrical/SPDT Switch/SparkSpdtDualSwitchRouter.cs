using UnityEngine;

namespace ProjectSpark.Gameplay
{
    /// <summary>
    /// Electrically routes SPDT using TWO existing SparkSwitch devices.
    /// No new solver or graph edges: each switch points to the SAME COM SparkTerminal.
    /// The existing SparkSwitchSolverDefinition handles closed/open stamping.
    /// </summary>
    [DisallowMultipleComponent]
    [AddComponentMenu("Project Spark/Electrical/SPDT Dual Switch Router")]
    public sealed class SparkSpdtDualSwitchRouter : MonoBehaviour, ISparkSpdtCircuitRouter
    {
        [Header("Existing electrical components")]
        [SerializeField] private SparkSwitch commonToA;
        [SerializeField] private SparkSwitch commonToB;

        [Header("Physical terminals - assign all three")]
        [SerializeField] private SparkTerminal commonTerminal;
        [SerializeField] private SparkTerminal terminalA;
        [SerializeField] private SparkTerminal terminalB;

        [Header("Diagnostics")]
        [SerializeField] private bool logChanges;

        public bool IsReady => ValidateSetup(false);

        public void SetSpdtConnection(bool chargeSelected)
        {
            // Never operate on partially configured references.
            if (!ValidateSetup(true)) return;

            bool alreadyCorrect = chargeSelected
                ? commonToA.IsClosed && commonToB.IsOpen
                : commonToA.IsOpen && commonToB.IsClosed;
            if (alreadyCorrect) return;

            // Break-before-make. Both devices notify the electrical solver via SetState.
            // Note: this is sequential state modification within one Unity call, not a
            // physical transition delay, and any synchronous solver callbacks can see
            // the temporary both-open state.
            commonToA.SetState(SparkSwitchState.Open);
            commonToB.SetState(SparkSwitchState.Open);

            if (chargeSelected)
                commonToA.SetState(SparkSwitchState.Closed);
            else
                commonToB.SetState(SparkSwitchState.Closed);

            if (logChanges)
                Debug.Log(chargeSelected
                    ? "[SPDT] CHARGE: COM-A CLOSED, COM-B OPEN"
                    : "[SPDT] BACKUP: COM-A OPEN, COM-B CLOSED", this);
        }

        private bool ValidateSetup(bool report)
        {
            if (commonToA == null || commonToB == null || commonToA == commonToB ||
                commonTerminal == null || terminalA == null || terminalB == null ||
                commonTerminal == terminalA || commonTerminal == terminalB || terminalA == terminalB)
            {
                if (report) Debug.LogError("[SPDT] Assign two distinct SparkSwitch devices and three distinct terminals COM, A, B.", this);
                return false;
            }

            // Require exact terminal identity, not name/suffix similarity.
            bool aCorrect = commonToA.InputTerminal == commonTerminal &&
                            commonToA.OutputTerminal == terminalA;
            bool bCorrect = commonToB.InputTerminal == commonTerminal &&
                            commonToB.OutputTerminal == terminalB;
            if (!aCorrect || !bCorrect)
            {
                if (report) Debug.LogError("[SPDT] Terminal mismatch: COM-A must be COM -> A, COM-B must be COM -> B. Reassign SparkSwitch terminal references.", this);
                return false;
            }
            if (!commonToA.isActiveAndEnabled || !commonToB.isActiveAndEnabled ||
                !commonToA.ElectricalEnabled || !commonToB.ElectricalEnabled)
            {
                if (report) Debug.LogError("[SPDT] Both electrical SparkSwitch components must be active and ElectricalEnabled.", this);
                return false;
            }
            return true;
        }

        [ContextMenu("Validate SPDT wiring")]
        private void ValidateWiring()
        {
            if (ValidateSetup(true))
                Debug.Log("[SPDT] Wiring references valid. Verify BOTH switches appear in the electrical network graph in Play Mode.", this);
        }
    }
}
