// Assets/My_Assets/_Project_Spark/Scripts/Gameplay/Level01/Level01GameManager.cs

using UnityEngine;

namespace ProjectSpark.Gameplay.Level01
{
    public sealed class Level01GameManager : MonoBehaviour
    {
        [SerializeField]
        ProjectSpark.Gameplay.Wiring.CircuitValidator validator;

        [SerializeField]
        BulbController bulb;

        [SerializeField]
        ObjectiveSystem objectives;

        [SerializeField]
        SuccessPanel success;

        bool finished;

        void Update()
        {
            if (finished)
                return;

            if (!validator.Validate())
                return;

            finished = true;

            bulb.SetPowered(true);

            objectives.Complete();

            success.Show();
        }
    }
}