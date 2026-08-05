// Assets/My_Assets/_Project_Spark/Scripts/Gameplay/Level01/ObjectiveSystem.cs

using TMPro;
using UnityEngine;

namespace ProjectSpark.Gameplay.Level01
{
    public sealed class ObjectiveSystem : MonoBehaviour
    {
        [SerializeField]
        TMP_Text objectiveText;

        [TextArea]
        [SerializeField]
        string startObjective =
            "Connect the blue wire to complete the circuit.";

        [TextArea]
        [SerializeField]
        string completedObjective =
            "Circuit completed.";

        void Start()
        {
            objectiveText.text = startObjective;
        }

        public void Complete()
        {
            objectiveText.text = completedObjective;
        }
    }
}