// Assets/My_Assets/_Project_Spark/Scripts/Gameplay/Wiring/CurrentShaderController.cs

using UnityEngine;

namespace ProjectSpark.Gameplay.Wiring
{
    [RequireComponent(typeof(Renderer))]
    public sealed class CurrentShaderController : MonoBehaviour
    {
        [SerializeField]
        float speed = 2f;

        [SerializeField]
        string property = "_Flow";

        Material material;

        float value;

        bool active;

        void Awake()
        {
            material =
                GetComponent<Renderer>().material;
        }

        public void SetActive(bool state)
        {
            active = state;
        }

        void Update()
        {
            if (!active)
                return;

            value += Time.deltaTime * speed;

            material.SetFloat(
                property,
                value);
        }
    }
}