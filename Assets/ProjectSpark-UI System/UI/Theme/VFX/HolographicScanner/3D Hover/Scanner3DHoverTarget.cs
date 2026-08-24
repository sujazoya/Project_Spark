using UnityEngine;
using UnityEngine.Rendering;

namespace ProjectSpark.Scanner
{
    [DisallowMultipleComponent]
    public sealed class Scanner3DHoverTarget : MonoBehaviour
    {
        [SerializeField]string interactionProgressID=
            "_InteractionProgress";
        private static readonly int InteractionProgressID =
            Shader.PropertyToID("interactionProgressID");

        private static readonly int ScanMinYID =
            Shader.PropertyToID("_ScanMinY");

        private static readonly int ScanMaxYID =
            Shader.PropertyToID("_ScanMaxY");

        [Header("Source")]
        [SerializeField]
        private Renderer[] sourceRenderers;

        [Header("Hover Material")]
        [SerializeField]
        private Material hoverMaterial;

        [Header("Animation")]
        [SerializeField, Min(0.01f)]
        private float hoverInSpeed = 8f;

        [SerializeField, Min(0.01f)]
        private float hoverOutSpeed = 6f;

        private Renderer[] overlayRenderers;
        private MaterialPropertyBlock[] propertyBlocks;

        private float interactionProgress;
        private bool hovered;

        public bool IsHovered =>
            hovered;

        public float Progress =>
            interactionProgress;

        private void Awake()
        {
            InitializeSources();
            CreateOverlays();
            SetImmediate(0f);
        }

        private void InitializeSources()
        {
            if (sourceRenderers != null &&
                sourceRenderers.Length > 0)
            {
                return;
            }

            sourceRenderers =
                GetComponentsInChildren<Renderer>(true);
        }

        private void CreateOverlays()
        {
            if (sourceRenderers == null ||
                sourceRenderers.Length == 0)
            {
                Debug.LogWarning(
                    $"[{name}] No source renderers found.",
                    this);

                return;
            }

            if (hoverMaterial == null)
            {
                Debug.LogWarning(
                    $"[{name}] Hover material is not assigned.",
                    this);

                return;
            }

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

                ConfigureBounds(
                    i,
                    source,
                    renderer);

                renderer.enabled = false;
            }
        }

        private GameObject CreateOverlay(
            Renderer source,
            int index)
        {
            GameObject overlay =
                new GameObject(
                    $"__HoverOverlay_{index}");

            overlay.transform.SetParent(
                source.transform,
                false);

            overlay.layer =
                source.gameObject.layer;

            // ---------------------------------------------------------
            // MeshRenderer
            // ---------------------------------------------------------

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

                MeshFilter overlayFilter =
                    overlay.AddComponent<MeshFilter>();

                overlayFilter.sharedMesh =
                    sourceFilter.sharedMesh;

                MeshRenderer renderer =
                    overlay.AddComponent<MeshRenderer>();

                renderer.sharedMaterial =
                    hoverMaterial;

                ConfigureRenderer(renderer);

                return overlay;
            }

            // ---------------------------------------------------------
            // SkinnedMeshRenderer
            // ---------------------------------------------------------

            if (source is SkinnedMeshRenderer skinned)
            {
                if (skinned.sharedMesh == null)
                {
                    Destroy(overlay);
                    return null;
                }

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

                renderer.updateWhenOffscreen =
                    skinned.updateWhenOffscreen;

                renderer.sharedMaterial =
                    hoverMaterial;

                ConfigureRenderer(renderer);

                return overlay;
            }

            Destroy(overlay);
            return null;
        }

        private static void ConfigureRenderer(
            Renderer renderer)
        {
            renderer.shadowCastingMode =
                ShadowCastingMode.Off;

            renderer.receiveShadows =
                false;

            renderer.motionVectorGenerationMode =
                MotionVectorGenerationMode.ForceNoMotion;
        }

        private void ConfigureBounds(
            int index,
            Renderer source,
            Renderer overlay)
        {
            MaterialPropertyBlock block =
                propertyBlocks[index];

            Bounds bounds =
                GetLocalBounds(source);

            block.Clear();

            block.SetFloat(
                ScanMinYID,
                bounds.min.y);

            block.SetFloat(
                ScanMaxYID,
                bounds.max.y);

            block.SetFloat(
                InteractionProgressID,
                0f);

            overlay.SetPropertyBlock(block);
        }

        private static Bounds GetLocalBounds(
            Renderer source)
        {
            if (source is SkinnedMeshRenderer skinned)
                return skinned.localBounds;

            MeshFilter filter =
                source.GetComponent<MeshFilter>();

            if (filter != null &&
                filter.sharedMesh != null)
            {
                return filter.sharedMesh.bounds;
            }

            return source.localBounds;
        }

        // =============================================================
        // HOVER STATE
        // =============================================================

        public void SetHovered(
            bool value)
        {
            if (hovered == value)
                return;

            hovered = value;

            if (hovered)
                SetOverlayVisible(true);
        }

        // =============================================================
        // CENTRALIZED ANIMATION TICK
        //
        // Returns true while the target still needs updating.
        // =============================================================

        public bool TickHover(
            float deltaTime)
        {
            float targetProgress =
                hovered ? 1f : 0f;

            float speed =
                hovered
                    ? hoverInSpeed
                    : hoverOutSpeed;

            interactionProgress =
                Mathf.MoveTowards(
                    interactionProgress,
                    targetProgress,
                    speed * deltaTime);

            ApplyProgress(
                interactionProgress);

            // Still animating or still hovered.
            if (hovered ||
                interactionProgress > 0f)
            {
                return true;
            }

            SetOverlayVisible(false);

            return false;
        }

        // =============================================================
        // IMMEDIATE
        // =============================================================

        public void SetImmediate(
            float progress)
        {
            interactionProgress =
                Mathf.Clamp01(progress);

            ApplyProgress(
                interactionProgress);

            SetOverlayVisible(
                interactionProgress > 0f);
        }

        // =============================================================
        // APPLY
        // =============================================================

        private void ApplyProgress(
            float progress)
        {
            if (overlayRenderers == null)
                return;

            progress =
                Mathf.Clamp01(progress);

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

                if (block == null)
                {
                    block =
                        new MaterialPropertyBlock();

                    propertyBlocks[i] =
                        block;
                }

                renderer.GetPropertyBlock(block);

                block.SetFloat(
                    InteractionProgressID,
                    progress);

                renderer.SetPropertyBlock(block);
            }
        }

        // =============================================================
        // VISIBILITY
        // =============================================================

        private void SetOverlayVisible(
            bool visible)
        {
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

                renderer.enabled =
                    visible;
            }
        }

        // =============================================================
        // RESET
        // =============================================================

        private void OnDisable()
        {
            hovered = false;
            interactionProgress = 0f;

            SetImmediate(0f);
        }
    }
}