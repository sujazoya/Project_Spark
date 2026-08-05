using UnityEngine;

namespace ProjectSpark.Gameplay.Repair
{
    public sealed class DeviceHealth
        : MonoBehaviour
    {
        [Range(0,100)]

        [SerializeField]
        private float health = 100;

        public float Health => health;

        public void Damage(
            float value)
        {
            health =
                Mathf.Max(
                    0,
                    health - value);
        }

        public void Repair(
            float value)
        {
            health =
                Mathf.Min(
                    100,
                    health + value);
        }
    }
}
