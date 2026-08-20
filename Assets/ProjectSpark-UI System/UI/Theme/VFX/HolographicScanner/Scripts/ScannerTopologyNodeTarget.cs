using UnityEngine;

namespace ProjectSpark.Scanner
{
    [DisallowMultipleComponent]
    public sealed class ScannerTopologyNodeTarget
        : MonoBehaviour
    {
        private static readonly int NodeProgressID =
            Shader.PropertyToID("_NodeProgress");

        [SerializeField]
        private Renderer sourceRenderer;

        [SerializeField]
        private Material nodeMaterial;

        private Renderer overlayRenderer;

        private MaterialPropertyBlock propertyBlock;

        private void Awake()
        {
            Initialize();
        }

        private void Initialize()
        {
            if (sourceRenderer == null)
                sourceRenderer =
                    GetComponent<Renderer>();

            if (sourceRenderer == null ||
                nodeMaterial == null)
            {
                return;
            }

            propertyBlock =
                new MaterialPropertyBlock();

            CreateOverlay();
        }

        private void CreateOverlay()
        {
            GameObject overlay =
                new GameObject(
                    "__ScannerTopologyNode");

            overlay.transform.SetParent(
                sourceRenderer.transform,
                false);

            overlay.layer =
                sourceRenderer.gameObject.layer;

            if (sourceRenderer is MeshRenderer)
            {
                MeshFilter sourceFilter =
                    sourceRenderer.GetComponent<MeshFilter>();

                if (sourceFilter == null ||
                    sourceFilter.sharedMesh == null)
                {
                    Destroy(overlay);
                    return;
                }

                MeshFilter filter =
                    overlay.AddComponent<MeshFilter>();

                filter.sharedMesh =
                    sourceFilter.sharedMesh;

                MeshRenderer renderer =
                    overlay.AddComponent<MeshRenderer>();

                renderer.sharedMaterial =
                    nodeMaterial;

                overlayRenderer =
                    renderer;
            }
            else if (sourceRenderer
                is SkinnedMeshRenderer source)
            {
                SkinnedMeshRenderer renderer =
                    overlay.AddComponent<
                        SkinnedMeshRenderer>();

                renderer.sharedMesh =
                    source.sharedMesh;

                renderer.bones =
                    source.bones;

                renderer.rootBone =
                    source.rootBone;

                renderer.localBounds =
                    source.localBounds;

                renderer.sharedMaterial =
                    nodeMaterial;

                overlayRenderer =
                    renderer;
            }

            if (overlayRenderer == null)
                return;

            overlayRenderer.shadowCastingMode =
                UnityEngine.Rendering
                    .ShadowCastingMode.Off;

            overlayRenderer.receiveShadows = false;
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
                NodeProgressID,
                progress);

            overlayRenderer.SetPropertyBlock(
                propertyBlock);
        }
    }
}