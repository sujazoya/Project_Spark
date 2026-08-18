using AAAUI.VFX;
using UnityEngine;

namespace ProjectSpark.Scanner
{
    public sealed class ScannerReconstructionController_2 : MonoBehaviour
    {
        [Header("Wire System")]
        [SerializeField]
        private SignalPath_Manager pathManager;

        [Header("Scanner Material")]
        [SerializeField]
        private Material reconstructionMaterial;

        [Header("Board Scan Bounds")]
        [SerializeField]
        private Transform scanBounds;

        [SerializeField]
        private Vector3 localMin = new Vector3(-1f, 0f, -1f);

        [SerializeField]
        private Vector3 localMax = new Vector3(1f, 1f, 1f);

        [Header("Shader Properties")]
        [SerializeField]
        private string progressProperty =
            "_ReconstructionProgress";

        [SerializeField]
        private string startProperty =
            "_ScanStart";

        [SerializeField]
        private string endProperty =
            "_ScanEnd";
        [SerializeField]
        private Vector3 reconstructionDirection =
    Vector3.up;

        private MaterialPropertyBlock propertyBlock;

        private int progressId;
        private int startId;
        private int endId;
        private int directionId;

        private float progress;

        private void Awake()
        {
            propertyBlock =
                new MaterialPropertyBlock();

            progressId =
                Shader.PropertyToID(
                    progressProperty);

            startId =
                Shader.PropertyToID(
                    startProperty);

            endId =
                Shader.PropertyToID(
                    endProperty);

            directionId =
                Shader.PropertyToID(
                    "_ReconstructionDirection");
        }

        public void StartReconstruction()
        {
            progress = 0f;

            ApplyToWires();
        }

        public void StopReconstruction()
        {
            progress = 0f;

            ApplyToWires();
        }

        public void CompleteReconstruction()
        {
            progress = 1f;

            ApplyToWires();
        }

        public void SetProgress(
            float normalized)
        {
            progress =
                Mathf.Clamp01(normalized);

            ApplyToWires();
        }


        private void ApplyToWires()
        {
            if (pathManager == null)
                return;

            SignalPath[] paths =
                pathManager.Paths;

            if (paths == null)
                return;

            CalculateScanRange(
                out float scanStart,
                out float scanEnd);

            Vector3 direction =
                reconstructionDirection;

            if (direction.sqrMagnitude <
                0.000001f)
            {
                direction = Vector3.up;
            }

            direction.Normalize();

            for (int i = 0;
                 i < paths.Length;
                 i++)
            {
                SignalPath path =
                    paths[i];

                if (path == null)
                    continue;

                SignalPathMesh mesh =
                    path.GetComponent<
                        SignalPathMesh>();

                if (mesh == null)
                    continue;

                if (reconstructionMaterial != null)
                {
                    mesh.SetScannerOverlayMaterial(
                        reconstructionMaterial);
                }

                MeshRenderer renderer =
                    mesh.GetComponent<
                        MeshRenderer>();

                if (renderer == null)
                    continue;

                renderer.GetPropertyBlock(
                    propertyBlock);

                propertyBlock.SetFloat(
                    progressId,
                    progress);

                propertyBlock.SetFloat(
                    startId,
                    scanStart);

                propertyBlock.SetFloat(
                    endId,
                    scanEnd);

                propertyBlock.SetVector(
                    directionId,
                    direction);

                renderer.SetPropertyBlock(
                    propertyBlock);
            }
        }

        private void CalculateScanRange(
            out float scanStart,
            out float scanEnd)
        {
            Transform reference =
                scanBounds != null
                    ? scanBounds
                    : transform;

            Vector3 min =
                Vector3.Min(
                    localMin,
                    localMax);

            Vector3 max =
                Vector3.Max(
                    localMin,
                    localMax);

            Vector3 localStart =
                new Vector3(
                    min.x,
                    min.y,
                    min.z);

            Vector3 localEnd =
                new Vector3(
                    max.x,
                    max.y,
                    max.z);

            Vector3 worldStart =
                reference.TransformPoint(
                    localStart);

            Vector3 worldEnd =
                reference.TransformPoint(
                    localEnd);

            Vector3 direction =
     reconstructionDirection;

            if (direction.sqrMagnitude <
                0.000001f)
            {
                direction = Vector3.up;
            }

            direction.Normalize();

            scanStart =
                Vector3.Dot(
                    worldStart,
                    direction);

            scanEnd =
                Vector3.Dot(
                    worldEnd,
                    direction);

            if (scanEnd < scanStart)
            {
                float temp = scanStart;

                scanStart = scanEnd;
                scanEnd = temp;
            }

            if (Mathf.Abs(
                    scanEnd - scanStart) <
                0.0001f)
            {
                scanEnd =
                    scanStart + 1f;
            }
        }
    }
}