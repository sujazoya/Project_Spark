// ============================================================================
// CoverController.cs
// ============================================================================

using System.Collections;
using UnityEngine;

namespace ProjectSpark.Gameplay.Flashlight
{
    public sealed class CoverController : MonoBehaviour
    {
        [SerializeField]
        Transform hinge;

        [SerializeField]
        float angle = 120f;

        [SerializeField]
        float speed = 3f;

        bool opened;

        public void Open()
        {
            if (opened)
                return;

            opened = true;

            StartCoroutine(OpenRoutine());
        }

        IEnumerator OpenRoutine()
        {
            Quaternion start = hinge.localRotation;

            Quaternion target =
                Quaternion.Euler(angle,0,0);

            float t = 0;

            while(t<1)
            {
                t+=Time.deltaTime*speed;

                hinge.localRotation=
                    Quaternion.Slerp(
                        start,
                        target,
                        t);

                yield return null;
            }
        }
    }
}