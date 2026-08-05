using UnityEngine;

namespace ProjectSpark.Presentation.Electronics
{
    public sealed class HeatEffectController
        : MonoBehaviour
    {
        [SerializeField]
        ParticleSystem smoke;

        public void UpdateHeat(
            float temperature)
        {
            if(temperature>80)
            {
                if(!smoke.isPlaying)
                    smoke.Play();
            }
            else
            {
                smoke.Stop();
            }
        }
    }
}
