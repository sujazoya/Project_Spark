using UnityEngine;

namespace ProjectSpark.Display
{
    public abstract class SparkDisplaySource : MonoBehaviour
    {
        [SerializeField]
        private SparkAdvancedDisplay display;

        [Header("Update")]
        [SerializeField]
        private bool updateContinuously = true;

        [SerializeField, Min(0.001f)]
        private float updateInterval = 0.05f;

        [SerializeField, Min(0f)]
        private float valueTolerance = 0.000001f;

        private float updateTimer;

        protected SparkAdvancedDisplay Display => display;

        protected virtual void OnEnable()
        {
            updateTimer = 0f;
            PushDisplayData(true);
        }

        protected virtual void Update()
        {
            if (!updateContinuously)
                return;

            updateTimer += Time.unscaledDeltaTime;

            if (updateTimer < updateInterval)
                return;

            updateTimer = 0f;

            PushDisplayData(false);
        }

        protected abstract void BuildDisplayData(
            SparkDisplayData data);

        private void PushDisplayData(bool force)
        {
            if (display == null)
                return;

            SparkDisplayData data =
                SparkDisplayData.CreateDefault();

            BuildDisplayData(data);

            display.SetData(
                data,
                force,
                valueTolerance);
        }
    }
}