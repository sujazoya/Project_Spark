using UnityEngine;

namespace ProjectSpark.Gameplay
{
    /// <summary>
    /// Small adapter for Project Spark's Activate()-based interactions.
    /// Do not also enable OnMouseDown on the SPDT controller for the same click.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class SparkSpdtInteraction : MonoBehaviour
    {
        [SerializeField] private SparkSpdtSwitchController controller;
        private int lastActivationFrame = -1;

        private void Awake()
        {
            if (controller == null)
                controller = GetComponentInParent<SparkSpdtSwitchController>();
        }

        public void Activate()
        {
            if (!isActiveAndEnabled || lastActivationFrame == Time.frameCount) return;
            if (controller == null) controller = GetComponentInParent<SparkSpdtSwitchController>();
            if (controller != null && controller.isActiveAndEnabled)
            {
                lastActivationFrame = Time.frameCount;
                controller.Toggle();
            }
        }

        public void SelectCharge()
        {
            if (controller != null)
                controller.SelectCharge();
        }

        public void SelectBackup()
        {
            if (controller != null)
                controller.SelectBackup();
        }
    }
}
