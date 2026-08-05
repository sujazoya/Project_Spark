using UnityEngine;

namespace ProjectSpark.Presentation.Electronics
{
    public sealed class ComponentAudio
        : MonoBehaviour
    {
        [SerializeField]
        AudioSource source;

        [SerializeField]
        AudioClip powered;

        [SerializeField]
        AudioClip damaged;

        public void PlayPowered()
        {
            source.PlayOneShot(powered);
        }

        public void PlayBroken()
        {
            source.PlayOneShot(damaged);
        }
    }
}
