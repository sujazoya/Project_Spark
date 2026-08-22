using UnityEngine;
using AAAUI.VFX;

namespace ProjectSpark.Scanner
{
    [DisallowMultipleComponent]
    public sealed class ScannerFaultComponentTarget
        : MonoBehaviour
    {
        private static readonly int FaultActiveID =
            Shader.PropertyToID("_FaultActive");

        private static readonly int FaultSeverityID =
            Shader.PropertyToID("_FaultSeverity");

        [SerializeField]
        private Renderer[] sourceRenderers;

        [SerializeField]
        private Material faultMaterial;

        private Renderer[] overlayRenderers;
        private MaterialPropertyBlock[] propertyBlocks;

        private void Awake()
        {
            CreateOverlays();

            SetFault(false, 0f);
        }

        private void CreateOverlays()
        {
            if (sourceRenderers == null)
                return;

            overlayRenderers =
                new Renderer[sourceRenderers.Length];

            propertyBlocks =
                new MaterialPropertyBlock[
                    sourceRenderers.Length];

            for (int i = 0;
                 i < sourceRenderers.Length;
                 i++)
            {
                Renderer source =
                    sourceRenderers[i];

                if (source == null)
                    continue;

                GameObject overlay =
                    CreateOverlay(
                        source,
                        i);

                if (overlay == null)
                    continue;

                Renderer renderer =
                    overlay.GetComponent<Renderer>();

                if (renderer == null)
                    continue;

                overlayRenderers[i] =
                    renderer;

                propertyBlocks[i] =
                    new MaterialPropertyBlock();

                renderer.enabled =
                    false;
            }
        }

        private GameObject CreateOverlay(
            Renderer source,
            int index)
        {
            if (faultMaterial == null)
                return null;

            GameObject overlay =
                new GameObject(
                    $"__ScannerFault_{index}");

            overlay.transform.SetParent(
                source.transform,
                false);

            overlay.layer =
                source.gameObject.layer;

            if (source is MeshRenderer)
            {
                MeshFilter sourceFilter =
                    source.GetComponent<MeshFilter>();

                if (sourceFilter == null ||
                    sourceFilter.sharedMesh == null)
                {
                    Destroy(overlay);
                    return null;
                }

                MeshFilter filter =
                    overlay.AddComponent<MeshFilter>();

                filter.sharedMesh =
                    sourceFilter.sharedMesh;

                MeshRenderer renderer =
                    overlay.AddComponent<MeshRenderer>();

                renderer.sharedMaterial =
                    faultMaterial;

                overlayRendererSettings(
                    renderer);

                return overlay;
            }

            if (source is SkinnedMeshRenderer skinned)
            {
                SkinnedMeshRenderer renderer =
                    overlay.AddComponent<
                        SkinnedMeshRenderer>();

                renderer.sharedMesh =
                    skinned.sharedMesh;

                renderer.bones =
                    skinned.bones;

                renderer.rootBone =
                    skinned.rootBone;

                renderer.localBounds =
                    skinned.localBounds;

                renderer.sharedMaterial =
                    faultMaterial;

                overlayRendererSettings(
                    renderer);

                return overlay;
            }

            Destroy(overlay);

            return null;
        }

        private static void overlayRendererSettings(
            Renderer renderer)
        {
            renderer.shadowCastingMode =
                UnityEngine.Rendering.ShadowCastingMode.Off;

            renderer.receiveShadows =
                false;
        }

        public void SetFault(
            bool active,
            float severity)
        {
            severity =
                Mathf.Clamp01(severity);

            if (overlayRenderers == null)
                return;

            for (int i = 0;
                 i < overlayRenderers.Length;
                 i++)
            {
                Renderer renderer =
                    overlayRenderers[i];

                if (renderer == null)
                    continue;

                MaterialPropertyBlock block =
                    propertyBlocks[i];

                renderer.GetPropertyBlock(block);

                block.SetFloat(
                    FaultActiveID,
                    active ? 1f : 0f);

                block.SetFloat(
                    FaultSeverityID,
                    severity);

                renderer.SetPropertyBlock(block);

                renderer.enabled =
                    active;
            }
        }
    }
}