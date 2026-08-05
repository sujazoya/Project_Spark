using System.Collections.Generic;
using UnityEngine;

namespace AAAUI.VFX
{
    [DisallowMultipleComponent]
    public sealed class SignalPath : MonoBehaviour
    {
        [SerializeField]
        private Vector3[] points = new Vector3[0];

        private readonly List<Vector3> runtimePoints =
            new List<Vector3>();

        private bool hasPreview;
        private Vector3 previewPoint;

        public int PointCount => runtimePoints.Count;

        public Vector3[] Points => runtimePoints.ToArray();

        public bool HasPreview => hasPreview;

        public Vector3 PreviewPoint => previewPoint;

        public void Clear()
        {
            runtimePoints.Clear();

            hasPreview = false;
        }

        public void AddPoint(Vector3 worldPosition)
        {
            if (runtimePoints.Count > 0)
            {
                Vector3 last =
                    runtimePoints[runtimePoints.Count - 1];

                if ((last - worldPosition).sqrMagnitude < 0.000001f)
                    return;
            }

            runtimePoints.Add(worldPosition);
        }

        public void SetPreviewPoint(Vector3 worldPosition)
        {
            previewPoint = worldPosition;
            hasPreview = true;
        }

        public void ClearPreview()
        {
            hasPreview = false;
        }

        public Vector3[] GetRenderPoints()
        {
            int count = runtimePoints.Count;

            if (count == 0)
                return new Vector3[0];

            if (!hasPreview)
                return runtimePoints.ToArray();

            Vector3[] result =
                new Vector3[count + 1];

            for (int i = 0; i < count; i++)
                result[i] = runtimePoints[i];

            result[count] = previewPoint;

            return result;
        }

        public Vector3 GetPosition(float progress)
        {
            Vector3[] renderPoints =
                GetRenderPoints();

            if (renderPoints.Length == 0)
                return transform.position;

            if (renderPoints.Length == 1)
                return renderPoints[0];

            progress = Mathf.Clamp01(progress);

            float totalLength = 0f;

            for (int i = 1; i < renderPoints.Length; i++)
            {
                totalLength += Vector3.Distance(
                    renderPoints[i - 1],
                    renderPoints[i]
                );
            }

            if (totalLength <= 0.0001f)
                return renderPoints[0];

            float target =
                totalLength * progress;

            float distance = 0f;

            for (int i = 1; i < renderPoints.Length; i++)
            {
                float segment =
                    Vector3.Distance(
                        renderPoints[i - 1],
                        renderPoints[i]
                    );

                if (distance + segment >= target)
                {
                    float t =
                        (target - distance) /
                        Mathf.Max(segment, 0.0001f);

                    return Vector3.Lerp(
                        renderPoints[i - 1],
                        renderPoints[i],
                        t
                    );
                }

                distance += segment;
            }

            return renderPoints[renderPoints.Length - 1];
        }
    }
}