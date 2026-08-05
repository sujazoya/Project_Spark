using UnityEngine;

namespace ProjectSpark.Gameplay.Electronics
{
    public sealed class ComponentFailureSystem
        : MonoBehaviour
    {
        [SerializeField]
        private float overheatingTemperature = 140f;

        public void UpdateFailure(
            ElectronicComponent component)
        {
            if (component.State.Temperature >
                overheatingTemperature)
            {
                component.State.IsBroken = true;
                component.State.IsActive = false;
            }
        }
    }
}
