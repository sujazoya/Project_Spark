// ============================================================================
// InspectionCamera.cs
// ============================================================================

using System.Collections;
using UnityEngine;

namespace ProjectSpark.Gameplay.Flashlight
{
    public sealed class InspectionCamera : MonoBehaviour
    {
        [SerializeField]
        UnityEngine.Camera inspectionCamera;

        [SerializeField]
        float speed = 5f;

        Coroutine routine;

        public void Focus(Transform target)
        {
            if (routine != null)
                StopCoroutine(routine);

            routine =
                StartCoroutine(
                    FocusRoutine(target));
        }

        IEnumerator FocusRoutine(Transform target)
        {
            Vector3 pos =
                target.position -
                target.forward * .20f +
                Vector3.up * .05f;

            Quaternion rot =
                Quaternion.LookRotation(
                    target.position - pos);

            while (Vector3.Distance(
                inspectionCamera.transform.position,
                pos) > .001f)
            {
                inspectionCamera.transform.position =
                    Vector3.Lerp(
                        inspectionCamera.transform.position,
                        pos,
                        Time.deltaTime * speed);

                inspectionCamera.transform.rotation =
                    Quaternion.Slerp(
                        inspectionCamera.transform.rotation,
                        rot,
                        Time.deltaTime * speed);

                yield return null;
            }
        }
    }
}