// Assets/My_Assets/_Project_Spark/Scripts/Gameplay/Wiring/WireHighlighter.cs

using UnityEngine;

namespace ProjectSpark.Gameplay.Wiring
{
    public sealed class WireHighlighter : MonoBehaviour
    {
        [SerializeField]
        Renderer target;

        Material mat;

        static readonly int Emission =
            Shader.PropertyToID("_EmissionColor");

        void Awake()
        {
            mat = target.material;
        }

        public void Highlight(bool value)
        {
            if (value)
            {
                mat.EnableKeyword("_EMISSION");
                mat.SetColor(
                    Emission,
                    Color.cyan * 4f);
            }
            else
            {
                mat.SetColor(
                    Emission,
                    Color.black);
            }
        }
    }
}