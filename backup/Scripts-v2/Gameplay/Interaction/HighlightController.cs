using UnityEngine;

namespace ProjectSpark.Gameplay.Interaction
{
    public class HighlightController : MonoBehaviour
    {
        [SerializeField]
        private Renderer[] renderers;

        [SerializeField]
        private Material highlightMaterial;

        private Material[][] _originalMaterials;

        private void Awake()
        {
            _originalMaterials = new Material[renderers.Length][];

            for (int i = 0; i < renderers.Length; i++)
            {
                _originalMaterials[i] = renderers[i].materials;
            }
        }

        public void SetHighlighted(bool highlighted)
        {
            for (int i = 0; i < renderers.Length; i++)
            {
                if (highlighted)
                {
                    var mats = renderers[i].materials;
                    System.Array.Resize(ref mats, mats.Length + 1);
                    mats[^1] = highlightMaterial;
                    renderers[i].materials = mats;
                }
                else
                {
                    renderers[i].materials = _originalMaterials[i];
                }
            }
        }
    }
}
