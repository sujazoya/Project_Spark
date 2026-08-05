// ============================================================================
// BurntResistorInteraction.cs
// ============================================================================

using UnityEngine;

namespace ProjectSpark.Gameplay.Flashlight
{
    public sealed class BurntResistorInteraction : MonoBehaviour
    {
        [SerializeField]
        ResistorController resistor;

        [SerializeField]
        SolderingController solder;

        void OnMouseDown()
        {
            if (!ToolManager.Instance.IsSelected(ToolType.DesolderPump))
                return;

            if (!solder.Completed)
                return;

            resistor.Remove();
        }
    }
}