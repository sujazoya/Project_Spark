using UnityEngine;
using UnityEngine.UI;

namespace AAAUI
{
    public class ShaderMaterialTest : MonoBehaviour
    {
        [SerializeField] private Graphic graphic;

        private Material material;
        private int propertyId;

        private void Awake()
        {
            if (graphic == null)
                graphic = GetComponent<Graphic>();

            material = graphic.material;

            propertyId = Shader.PropertyToID("_Glow");
        }

        private void Update()
        {
            if (material == null)
                return;

            float value = Mathf.PingPong(Time.time, 1f);

            material.SetFloat(propertyId, value);
        }
    }
}