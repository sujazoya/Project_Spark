// Assets/My_Assets/_Project_Spark/Scripts/Gameplay/Wiring/WireAudio.cs

using UnityEngine;

namespace ProjectSpark.Gameplay.Wiring
{
    [RequireComponent(typeof(AudioSource))]
    public sealed class WireAudio : MonoBehaviour
    {
        [SerializeField]
        AudioClip pickup;

        [SerializeField]
        AudioClip snap;

        [SerializeField]
        AudioClip power;

        AudioSource source;

        void Awake()
        {
            source = GetComponent<AudioSource>();
        }

        public void PlayPickup()
        {
            source.PlayOneShot(pickup);
        }

        public void PlaySnap()
        {
            source.PlayOneShot(snap);
        }

        public void PlayPower()
        {
            source.PlayOneShot(power);
        }
    }
}