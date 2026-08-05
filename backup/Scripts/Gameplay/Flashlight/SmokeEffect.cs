// ============================================================================
// SmokeEffect.cs
// ============================================================================

using UnityEngine;

namespace ProjectSpark.Gameplay.Flashlight
{
    public sealed class SmokeEffect : MonoBehaviour
    {
        [SerializeField]
        ParticleSystem smoke;

        public void Play()
        {
            smoke.Play();
        }

        public void Stop()
        {
            smoke.Stop();
        }
    }
}