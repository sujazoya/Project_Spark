using UnityEngine;
using UnityEngine.Rendering;
using AAAUI.VFX;

namespace ProjectSpark.Scanner
{
    [DisallowMultipleComponent]
    public sealed class ScannerTopologyConnectionTarget
        : MonoBehaviour
    {
        private static readonly int TopologyProgressID =
            Shader.PropertyToID("_TopologyProgress");

        private SignalPathMesh sourceMesh;
        private MeshRenderer sourceRenderer;

        private Material topologyMaterial;

        private MeshFilter overlayFilter;
        private MeshRenderer overlayRenderer;

        private MaterialPropertyBlock propertyBlock;

        public SignalPathMesh SourceMesh =>
            sourceMesh;

        public MeshRenderer SourceRenderer =>
            sourceRenderer;

        public void Initialize(
            SignalPathMesh mesh,
            Material material)
        {
            sourceMesh = mesh;
            topologyMaterial = material;

            if (sourceMesh == null)
            {
                Debug.LogWarning(
                    $"[{name}] Source SignalPathMesh is null.",
                    this);

                return;
            }

            if (topologyMaterial == null)
            {
                Debug.LogWarning(
                    $"[{name}] Topology material is null.",
                    this);

                return;
            }

            sourceRenderer =
                sourceMesh.MeshRenderer;

            if (sourceRenderer == null)
            {
                Debug.LogWarning(
                    $"[{name}] SignalPathMesh '{sourceMesh.name}' " +
                    "has no MeshRenderer.",
                    sourceMesh);

                return;
            }

            propertyBlock =
                new MaterialPropertyBlock();

            CreateOverlay();
        }

        private void CreateOverlay()
        {
            if (sourceRenderer == null ||
                topologyMaterial == null)
            {
                return;
            }

            overlayFilter =
                gameObject.AddComponent<MeshFilter>();

            overlayFilter.sharedMesh =
                sourceMesh.GeneratedMesh;

            overlayRenderer =
                gameObject.AddComponent<MeshRenderer>();

            overlayRenderer.sharedMaterial =
                topologyMaterial;

            overlayRenderer.shadowCastingMode =
                ShadowCastingMode.Off;

            overlayRenderer.receiveShadows =
                false;

            overlayRenderer.motionVectorGenerationMode =
                MotionVectorGenerationMode.ForceNoMotion;

            overlayRenderer.enabled =
                false;

            /*
             * The overlay GameObject itself is positioned
             * exactly on the source wire transform.
             */
            transform.SetParent(
                sourceRenderer.transform,
                false);

            transform.localPosition =
                Vector3.zero;

            transform.localRotation =
                Quaternion.identity;

            transform.localScale =
                Vector3.one;
        }

        public void SetProgress(
            float progress)
        {
            if (overlayRenderer == null)
                return;

            progress =
                Mathf.Clamp01(progress);

            overlayRenderer.GetPropertyBlock(
                propertyBlock);

            propertyBlock.SetFloat(
                TopologyProgressID,
                progress);

            overlayRenderer.SetPropertyBlock(
                propertyBlock);
        }

        public void SetVisible(
            bool visible)
        {
            if (overlayRenderer == null)
                return;

            overlayRenderer.enabled =
                visible;
        }

        private void OnDestroy()
        {
            sourceMesh = null;
            sourceRenderer = null;
            overlayFilter = null;
            overlayRenderer = null;
        }
    }
}