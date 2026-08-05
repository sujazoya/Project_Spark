// ============================================================================
// ReplacementResistorInteraction.cs
// ============================================================================

using UnityEngine;

namespace ProjectSpark.Gameplay.Flashlight
{
    public sealed class ReplacementResistorInteraction : MonoBehaviour
    {
        [SerializeField]
        ResistorController resistor;

        [SerializeField]
        GameObject replacementPrefab;

        void OnMouseDown()
        {
            if (!ToolManager.Instance.IsSelected(ToolType.ReplacementResistor))
                return;

            if (resistor.IsInstalled)
                return;

            resistor.InstallReplacement(replacementPrefab);
        }
    }
}