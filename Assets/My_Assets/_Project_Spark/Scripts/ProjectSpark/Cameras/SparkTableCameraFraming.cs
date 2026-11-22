using ProjectSpark.Gameplay;
using UnityEngine;

namespace ProjectSpark.Cameras
{
    // Opt-in for tabletop layouts that extend outside the default opening view.
    [DisallowMultipleComponent]
    public sealed class SparkTableCameraFraming : MonoBehaviour
    {
        [SerializeField] private float openingEyeHeight = 4.65f;
        private SparkTerminal[] terminals;
        internal float OpeningEyeHeight => openingEyeHeight;

        private void Awake() => terminals = GetComponentsInChildren<SparkTerminal>();

        internal float OpeningFieldOfView(Vector3 eye, Quaternion rotation, float aspect, float minimum)
        {
            if (terminals == null || aspect <= 0f) return minimum;

            var inverse = Quaternion.Inverse(rotation);
            float tangent = 0f;
            foreach (var terminal in terminals)
            {
                if (terminal == null || !terminal.isActiveAndEnabled) continue;
                var point = inverse * (terminal.transform.position - eye);
                if (point.z <= .12f) continue;
                tangent = Mathf.Max(tangent, Mathf.Max(
                    Mathf.Abs(point.x) / (point.z * aspect * .88f),
                    Mathf.Abs(point.y) / (point.z * .78f)));
            }
            return Mathf.Clamp(2f * Mathf.Atan(tangent) * Mathf.Rad2Deg, minimum, 95f);
        }
    }
}
