using UnityEngine;

namespace ProjectSpark.Domain.Diagnostics
{
    public sealed class FaultOverlay
    {
        public Color GetColor(
            bool fault)
        {
            return fault
                ? Color.red
                : Color.green;
        }
    }
}
