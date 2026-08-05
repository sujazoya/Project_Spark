// ============================================================================
// ExplodedViewController.cs
// ============================================================================

using System.Collections;
using UnityEngine;

namespace ProjectSpark.Gameplay.Flashlight
{
    public sealed class ExplodedViewController : MonoBehaviour
    {
        [System.Serializable]
        public class Part
        {
            public Transform Transform;
            public Vector3 Offset;
        }

        [SerializeField]
        Part[] parts;

        [SerializeField]
        float speed = 2f;

        public void Explode()
        {
            StartCoroutine(Animate(true));
        }

        public void Assemble()
        {
            StartCoroutine(Animate(false));
        }

        IEnumerator Animate(bool explode)
        {
            float t = 0;

            Vector3[] start = new Vector3[parts.Length];

            for (int i = 0; i < parts.Length; i++)
                start[i] = parts[i].Transform.localPosition;

            while (t < 1)
            {
                t += Time.deltaTime * speed;

                for (int i = 0; i < parts.Length; i++)
                {
                    Vector3 target =
                        explode
                        ? start[i] + parts[i].Offset
                        : start[i];

                    parts[i].Transform.localPosition =
                        Vector3.Lerp(
                            parts[i].Transform.localPosition,
                            target,
                            t);
                }

                yield return null;
            }
        }
    }
}