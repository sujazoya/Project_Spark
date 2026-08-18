using AAAUI.VFX;
using UnityEngine;

namespace ProjectSpark.Scanner
{
    public sealed class ScannerCircuitTraceController : MonoBehaviour
    {
        [Header("Wire System")]
        [SerializeField]
        private SignalPath_Manager pathManager;

        [Header("Scanner Overlay")]
        [SerializeField]
        private Material scannerMaterial;

        [Header("Shader Property")]
        [SerializeField]
        private string progressProperty = "_ScannerProgress";

        private MaterialPropertyBlock propertyBlock;
        private int progressId;

        private float progress;
        private bool tracing;

        public float Progress => progress;
        public bool IsTracing => tracing;

        private void Awake()
        {
            propertyBlock = new MaterialPropertyBlock();

            progressId =
                Shader.PropertyToID(progressProperty);
        }

        public void StartTrace()
        {
            tracing = true;
            progress = 0f;

            RefreshMeshes();
        }

        public void StopTrace()
        {
            tracing = false;
            progress = 0f;

            RefreshMeshes();
        }

        public void CompleteTrace()
        {
            tracing = false;
            progress = 1f;

            RefreshMeshes();
        }

        public void SetProgress(float normalized)
        {
            progress =
                Mathf.Clamp01(normalized);

            RefreshMeshes();
        }

        public void RefreshMeshes()
        {
            if (pathManager == null)
                return;

            SignalPath[] paths =
                pathManager.Paths;

            if (paths == null)
                return;

            for (int i = 0;
                 i < paths.Length;
                 i++)
            {
                SignalPath path = paths[i];

                if (path == null)
                    continue;

                SignalPathMesh mesh =
                    path.GetComponent<SignalPathMesh>();

                if (mesh == null)
                    continue;

                EnsureOverlay(mesh);

                MeshRenderer renderer =
                    mesh.GetComponent<MeshRenderer>();

                if (renderer == null)
                    continue;

                renderer.GetPropertyBlock(
                    propertyBlock);

                propertyBlock.SetFloat(
                    progressId,
                    progress);

                renderer.SetPropertyBlock(
                    propertyBlock);
            }
        }

        private void EnsureOverlay(
            SignalPathMesh mesh)
        {
            if (scannerMaterial == null)
                return;

            mesh.SetScannerOverlayMaterial(
                scannerMaterial);
        }
    }
}