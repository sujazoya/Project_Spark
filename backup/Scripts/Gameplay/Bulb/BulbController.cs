// Assets/My_Assets/_Project_Spark/Scripts/Gameplay/Bulb/BulbController.cs

using UnityEngine;

namespace ProjectSpark.Gameplay
{
    public sealed class BulbController : MonoBehaviour
    {
        [SerializeField]
        private MeshRenderer bulbRenderer;

        [SerializeField]
        private Light bulbLight;

        [SerializeField]
        private float emission = 8f;

        private Material material;

        private static readonly int EmissionColor =
            Shader.PropertyToID("_EmissionColor");

        private void Awake()
        {
            material = bulbRenderer.material;

            SetPowered(false);
        }

        public void SetPowered(bool powered)
        {
            if (powered)
            {
                material.EnableKeyword("_EMISSION");

                material.SetColor(
                    EmissionColor,
                    Color.yellow * emission);

                bulbLight.enabled = true;
            }
            else
            {
                material.SetColor(
                    EmissionColor,
                    Color.black);

                bulbLight.enabled = false;
            }
        }
    }
}