// ============================================================================
// CableCurrentAnimation.cs
// ============================================================================

using UnityEngine;

namespace ProjectSpark.Gameplay.Wiring
{
    [RequireComponent(typeof(Renderer))]
    public sealed class CableCurrentAnimation : MonoBehaviour
    {
        [SerializeField]
        float speed = 2f;

        Material material;

        static readonly int Offset =
            Shader.PropertyToID("_CurrentOffset");

        float current;

        bool powered;

        void Awake()
        {
            material =
                GetComponent<Renderer>().material;
        }

        public void Power(bool value)
        {
            powered = value;
        }

        void Update()
        {
            if (!powered)
                return;

            current += Time.deltaTime * speed;

            material.SetFloat(
                Offset,
                current);
        }
    }
}