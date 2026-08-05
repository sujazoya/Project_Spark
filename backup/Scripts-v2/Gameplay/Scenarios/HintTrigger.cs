using UnityEngine;

namespace ProjectSpark.Gameplay.Scenarios
{
    public class HintTrigger
        : MonoBehaviour
    {
        [SerializeField]
        private string hintId;

        [SerializeField]
        private float delay = 60;

        public string HintId => hintId;

        public float Delay => delay;
    }
}
