using UnityEngine;
using UnityEngine.Rendering;

namespace ProjectSpark.Scanner
{
    /// <summary>
    /// Provides production hover presentation for a scanner component.
    ///
    /// The target creates a dedicated overlay using the source mesh and applies
    /// _InteractionProgress through a MaterialPropertyBlock.
    ///
    /// Interaction state is controlled externally through SetHovered().
    /// Animation is advanced by TickHover().
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class Scanner3DHoverTarget : MonoBehaviour
    {
        private static readonly int InteractionProgressID =
            Shader.PropertyToID("_InteractionProgress");

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

        [Header("Overlay Transform")]
        [SerializeField]
        private Vector3 overlayScale = Vector3.one;

        [SerializeField]
        private Vector3 overlayRotation = Vector3.zero;

        [SerializeField]
        private Vector3 overlayOffset = Vector3.zero;

        private Renderer[] overlayRenderers;
        private MaterialPropertyBlock[] propertyBlocks;

        private float interactionProgress;
        private bool hovered;
        private bool initialized;

        public bool IsHovered => hovered;

        public float Progress => interactionProgress;

        private void Awake()
        {
            Initialize();
        }

        private void OnEnable()
        {
            if (!initialized)
                Initialize();

            SetImmediate(
                hovered
                    ? 1f
                    : 0f);
        }

        private void Initialize()
        {
            if (initialized)
                return;

            initialized = true;

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
                GetComponentsInChildren<Renderer>(
                    true);
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
            if (source == null)
                return null;

            GameObject overlay =
                new GameObject(
                    $"__HoverOverlay_{index}");

            overlay.transform.SetParent(
                source.transform.parent,
                false);

            overlay.layer =
                source.gameObject.layer;

            ApplyOverlayTransform(
                source.transform,
                overlay.transform);

            if (source is MeshRenderer meshRenderer)
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

                MeshRenderer overlayRenderer =
                    overlay.AddComponent<MeshRenderer>();

                overlayRenderer.sharedMaterial =
                    hoverMaterial;

                CopyRendererSettings(
                    meshRenderer,
                    overlayRenderer);

                return overlay;
            }

            if (source is SkinnedMeshRenderer skinned)
            {
                if (skinned.sharedMesh == null)
                {
                    Destroy(overlay);
                    return null;
                }

                SkinnedMeshRenderer overlayRenderer =
                    overlay.AddComponent<
                        SkinnedMeshRenderer>();

                overlayRenderer.sharedMesh =
                    skinned.sharedMesh;

                overlayRenderer.bones =
                    skinned.bones;

                overlayRenderer.rootBone =
                    skinned.rootBone;

                overlayRenderer.localBounds =
                    skinned.localBounds;

                overlayRenderer.updateWhenOffscreen =
                    skinned.updateWhenOffscreen;

                overlayRenderer.quality =
                    skinned.quality;

                overlayRenderer.skinnedMotionVectors =
                    skinned.skinnedMotionVectors;

                overlayRenderer.allowOcclusionWhenDynamic =
                    skinned.allowOcclusionWhenDynamic;

                overlayRenderer.sharedMaterial =
                    hoverMaterial;

                CopyRendererSettings(
                    skinned,
                    overlayRenderer);

                return overlay;
            }

            Destroy(overlay);

            return null;
        }

        private static void CopyRendererSettings(
            Renderer source,
            Renderer target)
        {
            target.shadowCastingMode =
                ShadowCastingMode.Off;

            target.receiveShadows =
                false;

            target.motionVectorGenerationMode =
                MotionVectorGenerationMode.ForceNoMotion;

            target.allowOcclusionWhenDynamic =
                source.allowOcclusionWhenDynamic;

            target.lightProbeUsage =
                source.lightProbeUsage;

            target.reflectionProbeUsage =
                source.reflectionProbeUsage;

            target.renderingLayerMask =
                source.renderingLayerMask;
        }

        private void ConfigureBounds(
            int index,
            Renderer source,
            Renderer overlay)
        {
            MaterialPropertyBlock block =
                propertyBlocks[index];

            if (block == null)
            {
                block =
                    new MaterialPropertyBlock();

                propertyBlocks[index] =
                    block;
            }

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

        private void ApplyOverlayTransform(
            Transform source,
            Transform overlay)
        {
            overlay.localPosition =
                source.localPosition +
                overlayOffset;

            overlay.localRotation =
                source.localRotation *
                Quaternion.Euler(
                    overlayRotation);

            overlay.localScale =
                Vector3.Scale(
                    source.localScale,
                    overlayScale);
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

        // ============================================================
        // HOVER STATE
        // ============================================================

        /// <summary>
        /// Changes the logical hover state.
        /// The visual animation is advanced by TickHover().
        /// </summary>
        public void SetHovered(
            bool value)
        {
            if (hovered == value)
                return;

            hovered = value;

            if (hovered)
                SetOverlayVisible(true);
        }

        // ============================================================
        // ANIMATION
        // ============================================================

        /// <summary>
        /// Advances the hover animation.
        ///
        /// Returns true while the animation still requires updates.
        /// </summary>
        public bool TickHover(
            float deltaTime)
        {
            deltaTime =
                Mathf.Max(
                    0f,
                    deltaTime);

            float targetProgress =
                hovered
                    ? 1f
                    : 0f;

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

            if (hovered ||
                interactionProgress > 0f)
            {
                return true;
            }

            SetOverlayVisible(false);

            return false;
        }

        // ============================================================
        // IMMEDIATE
        // ============================================================

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

        // ============================================================
        // APPLY
        // ============================================================

        private void ApplyProgress(
            float progress)
        {
            if (overlayRenderers == null ||
                propertyBlocks == null)
            {
                return;
            }

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

                renderer.GetPropertyBlock(
                    block);

                block.SetFloat(
                    InteractionProgressID,
                    progress);

                renderer.SetPropertyBlock(
                    block);
            }
        }

        // ============================================================
        // VISIBILITY
        // ============================================================

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

        // ============================================================
        // RESET
        // ============================================================

        private void OnDisable()
        {
            hovered = false;
            interactionProgress = 0f;

            if (initialized)
                SetImmediate(0f);
        }
    }
}