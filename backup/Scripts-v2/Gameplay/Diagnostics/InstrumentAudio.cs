using UnityEngine;

namespace ProjectSpark.Gameplay.Diagnostics
{
    [RequireComponent(typeof(AudioSource))]
    public sealed class InstrumentAudio
        : MonoBehaviour
    {
        private AudioSource source;

        private void Awake()
        {
            source =
                GetComponent<AudioSource>();
        }

        public void Beep()
        {
            source.Play();
        }
    }
}
