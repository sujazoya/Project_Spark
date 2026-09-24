using UnityEngine;

namespace ProjectSpark.Display
{
    public abstract class SparkDisplaySource : MonoBehaviour
    {
        [SerializeField] private SparkAdvancedDisplay display;

        protected SparkAdvancedDisplay Display => display;

        protected virtual void OnEnable()
        {
            PushDisplayData();
        }

        protected virtual void Update()
        {
            PushDisplayData();
        }

        protected abstract void BuildDisplayData(SparkDisplayData data);

        private void PushDisplayData()
        {
            if (display == null)
                return;

            SparkDisplayData data = SparkDisplayData.CreateDefault();
            BuildDisplayData(data);
            display.SetData(data);
        }
    }
}
