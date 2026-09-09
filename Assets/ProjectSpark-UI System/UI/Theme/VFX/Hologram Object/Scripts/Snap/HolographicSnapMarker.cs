using TMPro;
using UnityEngine;

namespace ProjectSpark.HolographicViewer
{
    [DisallowMultipleComponent]
    public sealed class HolographicSnapMarker
        : MonoBehaviour
    {
        [Header("Marker")]

        [SerializeField]
        private Transform marker;

        [SerializeField]
        private Transform markerPivot;

        [Header("Labels")]

        [SerializeField]
        private TMP_Text typeLabel;

        [SerializeField]
        private TMP_Text positionLabel;

        [SerializeField]
        private TMP_Text indexLabel;

        [Header("Visual")]

        [SerializeField]
        private float surfaceOffset = 0.0025f;

        [SerializeField]
        private float labelOffset = 0.12f;

        public void Hide()
        {
            SetActive(
                marker,
                false
            );

            SetActive(
                typeLabel,
                false
            );

            SetActive(
                positionLabel,
                false
            );

            SetActive(
                indexLabel,
                false
            );
        }

        public void Show(
            HolographicSnapResult result)
        {
            if (!result.Valid)
            {
                Hide();

                return;
            }

            Vector3 normal =
                result.WorldNormal;

            if (normal.sqrMagnitude <
                0.000001f)
            {
                normal =
                    Vector3.up;
            }
            else
            {
                normal.Normalize();
            }

            Vector3 worldPosition =
                result.WorldPosition +
                normal *
                surfaceOffset;

            if (marker != null)
            {
                marker.gameObject.SetActive(
                    true
                );

                marker.position =
                    worldPosition;
            }

            if (markerPivot != null)
            {
                markerPivot.position =
                    worldPosition;

                markerPivot.rotation =
                    CreateSurfaceRotation(
                        normal
                    );
            }

            if (typeLabel != null)
            {
                typeLabel.gameObject.SetActive(
                    true
                );

                typeLabel.text =
                    GetLabel(result.Type);

                typeLabel.transform.position =
                    worldPosition +
                    normal *
                    labelOffset;
            }

            if (positionLabel != null)
            {
                positionLabel.gameObject.SetActive(
                    true
                );

                positionLabel.text =
                    FormatPosition(
                        result.WorldPosition
                    );
            }

            if (indexLabel != null)
            {
                indexLabel.gameObject.SetActive(
                    true
                );

                indexLabel.text =
                    FormatIndex(
                        result
                    );
            }
        }

        private static Quaternion
            CreateSurfaceRotation(
                Vector3 normal)
        {
            Vector3 up =
                Mathf.Abs(
                    Vector3.Dot(
                        normal,
                        Vector3.up
                    )
                ) > 0.95f
                    ? Vector3.forward
                    : Vector3.up;

            Vector3 tangent =
                Vector3.Cross(
                    up,
                    normal
                ).normalized;

            Vector3 correctedUp =
                Vector3.Cross(
                    normal,
                    tangent
                ).normalized;

            return Quaternion.LookRotation(
                normal,
                correctedUp
            );
        }

        private static string GetLabel(
            HolographicSnapType type)
        {
            return type switch
            {
                HolographicSnapType.Vertex =>
                    "VERTEX",

                HolographicSnapType.Edge =>
                    "EDGE",

                HolographicSnapType.FaceCenter =>
                    "FACE CENTER",

                HolographicSnapType.Surface =>
                    "SURFACE",

                _ =>
                    string.Empty
            };
        }

        private static string FormatPosition(
            Vector3 position)
        {
            return
                $"X {position.x:F2}\n" +
                $"Y {position.y:F2}\n" +
                $"Z {position.z:F2}";
        }

        private static string FormatIndex(
            HolographicSnapResult result)
        {
            return result.Type switch
            {
                HolographicSnapType.Vertex =>
                    $"V {result.PrimaryIndex}",

                HolographicSnapType.Edge =>
                    $"E {result.PrimaryIndex} → " +
                    $"{result.SecondaryIndex}   " +
                    $"T {result.EdgeT:F2}",

                HolographicSnapType.FaceCenter =>
                    $"F {result.PrimaryIndex}",

                HolographicSnapType.Surface =>
                    $"TRI {result.PrimaryIndex}",

                _ =>
                    string.Empty
            };
        }

        private static void SetActive(
            Transform target,
            bool state)
        {
            if (target != null)
                target.gameObject.SetActive(
                    state
                );
        }

        private static void SetActive(
            TMP_Text target,
            bool state)
        {
            if (target != null)
                target.gameObject.SetActive(
                    state
                );
        }
    }
}