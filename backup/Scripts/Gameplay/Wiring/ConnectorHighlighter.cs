// Assets/My_Assets/_Project_Spark/Scripts/Gameplay/Wiring/ConnectorHighlighter.cs

using UnityEngine;

namespace ProjectSpark.Gameplay.Wiring
{
    [RequireComponent(typeof(Renderer))]
    public sealed class ConnectorHighlighter : MonoBehaviour
    {
        [SerializeField]
        private Color hoverColor = Color.cyan;

        [SerializeField]
        private float intensity = 5f;

        private Material material;

        private static readonly int Emission =
            Shader.PropertyToID("_EmissionColor");

        private void Awake()
        {
            material = GetComponent<Renderer>().material;
        }

        public void SetHighlight(bool value)
        {
            if (value)
            {
                material.EnableKeyword("_EMISSION");
                material.SetColor(
                    Emission,
                    hoverColor * intensity);
            }
            else
            {
                material.SetColor(
                    Emission,
                    Color.black);
            }
        }
    }
}