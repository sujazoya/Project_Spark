// ============================================================================
// WireVisualController.cs
// ============================================================================

using UnityEngine;

namespace ProjectSpark.Gameplay.Wiring
{
    public sealed class WireVisualController : MonoBehaviour
    {
        [SerializeField]
        CableRenderer cable;

        [SerializeField]
        CableCurrentAnimation current;

        public void Idle()
        {
            cable.SetIdle();
            current.Power(false);
        }

        public void Drag()
        {
            cable.SetDragging();
            current.Power(false);
        }

        public void Powered()
        {
            cable.SetPowered();
            current.Power(true);
        }
    }
}