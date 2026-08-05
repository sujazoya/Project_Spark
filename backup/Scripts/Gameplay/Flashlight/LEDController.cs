// ============================================================================
// LEDController.cs
// ============================================================================

using UnityEngine;

namespace ProjectSpark.Gameplay.Flashlight
{
    public sealed class LEDController : MonoBehaviour
    {
        [SerializeField]
        MeshRenderer led;

        [SerializeField]
        Light pointLight;

        static readonly int Emission =
            Shader.PropertyToID("_EmissionColor");

        Material mat;

        void Awake()
        {
            mat=led.material;
        }

        public void TurnOn()
        {
            mat.EnableKeyword("_EMISSION");

            mat.SetColor(
                Emission,
                Color.white*10);

            pointLight.enabled=true;
        }

        public void TurnOff()
        {
            mat.SetColor(
                Emission,
                Color.black);

            pointLight.enabled=false;
        }
    }
}