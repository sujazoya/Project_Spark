// Assets/My_Assets/_Project_Spark/Scripts/Gameplay/Wiring/WireAnimator.cs

using UnityEngine;

namespace ProjectSpark.Gameplay.Wiring
{
    [RequireComponent(typeof(Renderer))]
    public sealed class WireAnimator : MonoBehaviour
    {
        [SerializeField]
        private float speed = 2f;

        [SerializeField]
        private string textureProperty = "_BaseMap";

        private Material material;

        private Vector2 offset;

        private bool powered;

        public void SetPowered(bool value)
        {
            powered = value;
        }

        void Awake()
        {
            material = GetComponent<Renderer>().material;
        }

        void Update()
        {
            if (!powered)
                return;

            offset.x += Time.deltaTime * speed;

            material.SetTextureOffset(
                textureProperty,
                offset);
        }
    }
}