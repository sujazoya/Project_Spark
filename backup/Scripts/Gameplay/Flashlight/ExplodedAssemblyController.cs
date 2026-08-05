// ============================================================================
// ExplodedAssemblyController.cs
// ============================================================================

using UnityEngine;

namespace ProjectSpark.Gameplay.Flashlight
{
    public sealed class ExplodedAssemblyController : MonoBehaviour
    {
        [SerializeField]
        Transform[] parts;

        [SerializeField]
        Vector3 explodeDirection =
            new(0,0.03f,0);

        [SerializeField]
        float amount = .05f;

        Vector3[] original;

        void Awake()
        {
            original = new Vector3[parts.Length];

            for(int i=0;i<parts.Length;i++)
                original[i]=parts[i].localPosition;
        }

        public void Explode()
        {
            for(int i=0;i<parts.Length;i++)
            {
                parts[i].localPosition =
                    original[i] +
                    explodeDirection *
                    amount *
                    (i+1);
            }
        }

        public void Assemble()
        {
            for(int i=0;i<parts.Length;i++)
                parts[i].localPosition =
                    original[i];
        }
    }
}