// ============================================================================
// ObjectiveController.cs
// ============================================================================

using TMPro;
using UnityEngine;

namespace ProjectSpark.Gameplay.Flashlight
{
    public sealed class ObjectiveController : MonoBehaviour
    {
        [SerializeField]
        TMP_Text objectiveText;

        readonly string[] objectives =
        {
            "Open Flashlight",
            "Remove Battery",
            "Inspect PCB",
            "Measure Battery",
            "Measure LED",
            "Measure Resistor",
            "Find Burnt Resistor",
            "Unsolder Resistor",
            "Install New Resistor",
            "Solder Component",
            "Insert Battery",
            "Close Flashlight",
            "Power Test",
            "Repair Complete"
        };

        int current;

        void Start()
        {
            UpdateObjective();
        }

        public void Next()
        {
            current++;

            if (current >= objectives.Length)
                current = objectives.Length - 1;

            UpdateObjective();
        }

        void UpdateObjective()
        {
            objectiveText.text =
                objectives[current];
        }
    }
}