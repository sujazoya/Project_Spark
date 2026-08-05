// Assets/My_Assets/_Project_Spark/Scripts/Gameplay/Wiring/CurrentFlowController.cs

using UnityEngine;

namespace ProjectSpark.Gameplay.Wiring
{
    public sealed class CurrentFlowController : MonoBehaviour
    {
        [SerializeField]
        private WireController[] wires;

        [SerializeField]
        private BulbController bulb;

        public bool Powered { get; private set; }

        public void SetPowered(bool value)
        {
            Powered = value;

            foreach (WireController wire in wires)
                wire.SetPowered(value);

            bulb.SetPowered(value);
        }
    }
}