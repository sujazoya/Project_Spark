using UnityEngine;

namespace ProjectSpark.Gameplay.Electronics
{
    [RequireComponent(typeof(AudioSource))]
    public sealed class BuzzerComponent : ElectronicComponent
    {
        private AudioSource audioSource;

        private void Awake()
        {
            audioSource =
                GetComponent<AudioSource>();
        }

        public override void Simulate(float deltaTime)
        {
            bool play =
                State.Voltage > 2f &&
                !State.IsBroken;

            if (play && !audioSource.isPlaying)
                audioSource.Play();

            if (!play && audioSource.isPlaying)
                audioSource.Stop();

            State.IsActive = play;
        }
    }
}
