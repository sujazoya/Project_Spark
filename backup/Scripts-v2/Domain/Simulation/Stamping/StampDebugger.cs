using UnityEngine;

namespace ProjectSpark.Domain.Simulation.Stamping
{
    public sealed class StampDebugger
        : MonoBehaviour
    {
        public bool EnableLogging = true;

        public void Log(
            string message)
        {
            if (!EnableLogging)
                return;

            Debug.Log(
                "[STAMP] " + message);
        }
    }
}
