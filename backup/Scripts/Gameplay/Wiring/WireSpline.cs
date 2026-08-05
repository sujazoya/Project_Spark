// Assets/My_Assets/_Project_Spark/Scripts/Gameplay/Wiring/WireSpline.cs

using UnityEngine;

namespace ProjectSpark.Gameplay.Wiring
{
    public sealed class WireSpline : MonoBehaviour
    {
        [SerializeField]
        private Transform start;

        [SerializeField]
        private Transform end;

        [SerializeField]
        private WireSettings settings;

        private Vector3[] points = new Vector3[4];

        public Vector3 GetPoint(float t)
        {
            return Bezier(
                points[0],
                points[1],
                points[2],
                points[3],
                t);
        }

        private void LateUpdate()
        {
            UpdateCurve();
        }

        private void UpdateCurve()
        {
            Vector3 a = start.position;
            Vector3 d = end.position;

            Vector3 dir = d - a;

            float len = dir.magnitude;

            Vector3 up = Vector3.down * settings.Sag * len;

            points[0] = a;
            points[1] = a + dir * .33f + up;
            points[2] = a + dir * .66f + up;
            points[3] = d;
        }

        private static Vector3 Bezier(
            Vector3 a,
            Vector3 b,
            Vector3 c,
            Vector3 d,
            float t)
        {
            float u = 1f - t;

            return
                u * u * u * a +
                3f * u * u * t * b +
                3f * u * t * t * c +
                t * t * t * d;
        }
    }
}