using UnityEngine;

namespace ProjectSpark.Core.Performance
{
    public sealed class FPSCounter
        : MonoBehaviour
    {
        public float FPS
        {
            get;
            private set;
        }

        private void Update()
        {
            FPS =
                1f / Time.unscaledDeltaTime;

            PerformanceEvents
                .RaiseFPS(FPS);
        }
    }
}
