using UnityEngine;

namespace ProjectSpark.Gameplay.Progression
{
    public sealed class SessionStatistics
        : MonoBehaviour
    {
        public float SessionTime
        {
            get;
            private set;
        }

        private void Update()
        {
            SessionTime +=
                Time.deltaTime;
        }
    }
}
