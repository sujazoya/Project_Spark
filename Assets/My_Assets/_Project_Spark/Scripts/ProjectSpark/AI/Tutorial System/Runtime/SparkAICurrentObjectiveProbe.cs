
using UnityEngine;
using ProjectSpark.Gameplay;

namespace ProjectSpark.AI.Tutorial
{
    /// <summary>
    /// Associates a runtime resistor with a level
    /// objective's TargetComponentId.
    ///
    /// Read-only. Never changes electrical state.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class SparkAICurrentObjectiveProbe :
        MonoBehaviour
    {
        [Header("Level Scene Binding")]
        [SerializeField]
        private string componentBindingId;

        [Header("Measured Component")]
        [SerializeField]
        private SparkResistor resistor;

        public string ComponentBindingId =>
            componentBindingId;

        public SparkResistor Resistor =>
            resistor;

        public bool CanRead =>
            isActiveAndEnabled &&
            gameObject.activeInHierarchy &&
            resistor != null &&
            resistor.gameObject.activeInHierarchy;

        public float CurrentAmps =>
            CanRead ? resistor.Current : 0f;

        public void Configure(
            string bindingId,
            SparkResistor measuredResistor)
        {
            componentBindingId = bindingId;
            resistor = measuredResistor;
        }

        private void Awake()
        {
            if (resistor == null)
            {
                resistor =
                    GetComponent<SparkResistor>();
            }

            if (resistor == null)
            {
                resistor =
                    GetComponentInChildren<SparkResistor>(true);
            }

            if (resistor == null)
            {
                resistor =
                    GetComponentInParent<SparkResistor>();
            }
        }

        [ContextMenu("Diagnostics / Print Current")]
        private void PrintCurrent()
        {
            Debug.Log(
                "[Spark AI Current Probe] Binding=" +
                componentBindingId +
                " | Resistor=" +
                (resistor != null
                    ? resistor.name
                    : "NONE") +
                " | Readable=" + CanRead +
                " | Current=" +
                (Mathf.Abs(CurrentAmps) * 1000f)
                    .ToString("F3") + " mA",
                this);
        }
    }
}
