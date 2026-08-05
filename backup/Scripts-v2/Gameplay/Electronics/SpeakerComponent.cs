using UnityEngine;

namespace ProjectSpark.Gameplay.Electronics
{
    [RequireComponent(typeof(AudioSource))]
    public sealed class SpeakerComponent
        : ElectronicComponent
    {
        private AudioSource audioSource;

        private void Awake()
        {
            audioSource =
                GetComponent<AudioSource>();
        }

        public override void Simulate(float deltaTime)
        {
            audioSource.volume =
                Mathf.Clamp01(
                    State.Current);

            if (State.IsActive &&
                !audioSource.isPlaying)
            {
                audioSource.Play();
            }
        }
    }
}
