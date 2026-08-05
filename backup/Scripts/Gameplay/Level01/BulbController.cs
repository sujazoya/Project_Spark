// Assets/My_Assets/_Project_Spark/Scripts/Gameplay/Level01/BulbController.cs

using UnityEngine;

namespace ProjectSpark.Gameplay.Level01
{
    [RequireComponent(typeof(AudioSource))]
    public sealed class BulbController : MonoBehaviour
    {
        [SerializeField] MeshRenderer bulb;

        [SerializeField] Light pointLight;

        [SerializeField] float intensity = 9f;

        [SerializeField] AudioClip turnOn;

        AudioSource audioSource;

        Material material;

        static readonly int Emission =
            Shader.PropertyToID("_EmissionColor");

        bool powered;

        void Awake()
        {
            audioSource = GetComponent<AudioSource>();

            material = bulb.material;

            SetPowered(false);
        }

        public void SetPowered(bool value)
        {
            if (powered == value)
                return;

            powered = value;

            if (powered)
            {
                material.EnableKeyword("_EMISSION");

                material.SetColor(
                    Emission,
                    Color.yellow * intensity);

                pointLight.enabled = true;

                if (turnOn != null)
                    audioSource.PlayOneShot(turnOn);
            }
            else
            {
                material.SetColor(
                    Emission,
                    Color.black);

                pointLight.enabled = false;
            }
        }
    }
}