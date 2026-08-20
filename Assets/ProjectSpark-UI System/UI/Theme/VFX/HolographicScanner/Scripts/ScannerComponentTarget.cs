using UnityEngine;
using UnityEngine.Rendering;

namespace ProjectSpark.Scanner
{
    /// <summary>
    /// Represents one real simulation component as a scanner target.
    ///
    /// One target may contain multiple renderers:
    /// - MeshRenderer
    /// - SkinnedMeshRenderer
    ///
    /// Each renderer receives:
    /// - Effect 07 scanner overlay
    /// - Effect 08 projection overlay
    ///
    /// Original renderer materials are never replaced.
    /// All scanner/projection state is controlled through MaterialPropertyBlock.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class ScannerComponentTarget : MonoBehaviour
    {
        // ---------------------------------------------------------------------
        // Effect 07 shader properties
        // ---------------------------------------------------------------------

        private static readonly int ScanProgressID =
            Shader.PropertyToID("_ComponentScanProgress");

        private static readonly int ScanMinYID =
            Shader.PropertyToID("_ScanMinY");

        private static readonly int ScanMaxYID =
            Shader.PropertyToID("_ScanMaxY");

        private static readonly int IdentifiedID =
            Shader.PropertyToID("_ComponentIdentified");

            private static readonly int InteractionProgressID =
    Shader.PropertyToID("_InteractionProgress");

    [Header("Effect 09 - Component Interaction")]
[SerializeField]
private Material interactionMaterial;

private Renderer[] interactionOverlays;
private MaterialPropertyBlock[] interactionPropertyBlocks;

        // ---------------------------------------------------------------------
        // Effect 08 shader properties
        // ---------------------------------------------------------------------

        private static readonly int ProjectionProgressID =
            Shader.PropertyToID("_ProjectionProgress");

            [SerializeField, Range(0.001f, 0.25f)]
        private float interactionWindow = 0.05f;

        public float InteractionWindow =>
            interactionWindow;

        // ---------------------------------------------------------------------
        // Identity
        // ---------------------------------------------------------------------

        [Header("Identity")]
        [SerializeField]
        private string componentId;

        [SerializeField]
        private string displayName;

        // ---------------------------------------------------------------------
        // Source renderers
        // ---------------------------------------------------------------------

        [Header("Source Renderers")]
        [Tooltip("Every Renderer belonging to this real simulation component.")]
        [SerializeField]
        private Renderer[] sourceRenderers;

        // ---------------------------------------------------------------------
        // Effect 07
        // ---------------------------------------------------------------------

        [Header("Effect 07 - Component Scanner")]
        [SerializeField]
        private Material scannerMaterial;

        // ---------------------------------------------------------------------
        // Effect 08
        // ---------------------------------------------------------------------

        [Header("Effect 08 - Hologram Projection")]
        [SerializeField]
        private Material projectionMaterial;

        // ---------------------------------------------------------------------
        // Global scan range
        // ---------------------------------------------------------------------

        [Header("Scan Range")]
        [SerializeField, Range(0f, 1f)]
        private float scanStart = 0f;

        [SerializeField, Range(0f, 1f)]
        private float scanEnd = 1f;

        // ---------------------------------------------------------------------
        // Runtime overlay renderers
        // ---------------------------------------------------------------------

        private Renderer[] scannerOverlays;
        private Renderer[] projectionOverlays;

        // One MPB per overlay renderer.
        private MaterialPropertyBlock[] scannerPropertyBlocks;
        private MaterialPropertyBlock[] projectionPropertyBlocks;

        // ---------------------------------------------------------------------
        // Public API
        // ---------------------------------------------------------------------

        public string ComponentId => componentId;

        public string DisplayName => displayName;

        public float ScanStart => scanStart;

        public float ScanEnd => scanEnd;

        // ---------------------------------------------------------------------
        // Unity
        // ---------------------------------------------------------------------

        private void Awake()
        {
            Initialize();
        }

        private void OnDestroy()
        {
            DestroyOverlayRenderers(scannerOverlays);
            DestroyOverlayRenderers(projectionOverlays);
        }

        // ---------------------------------------------------------------------
        // Initialization
        // ---------------------------------------------------------------------

        private void Initialize()
        {
            CleanupExistingScannerChildren();

            ValidateSources();

            if (sourceRenderers == null ||
                sourceRenderers.Length == 0)
            {
                return;
            }

            CreateScannerOverlays();
            CreateProjectionOverlays();
            CreateInteractionOverlays();

            SetScanProgress(0f);
            SetIdentified(false);

            SetProjectionProgress(0f);
            SetProjectionVisible(false);
            SetInteractionProgress(0f);
            SetInteractionVisible(false);
        }
        public float EvaluateInteractionPulse(
        float globalProgress)
            {
                float center =
                    scanStart;

                float halfWindow =
                    interactionWindow;

                float distance =
                    Mathf.Abs(
                        globalProgress - center);

                if (distance >= halfWindow)
                    return 0f;

                float normalized =
                    1f -
                    Mathf.Clamp01(
                        distance / halfWindow);

                return normalized;
            }

        private void ValidateSources()
        {
            if (scannerMaterial == null)
            {
                Debug.LogWarning(
                    $"[{name}] Scanner material is not assigned.",
                    this);
            }

            if (projectionMaterial == null)
            {
                Debug.LogWarning(
                    $"[{name}] Projection material is not assigned.",
                    this);
            }

            if (sourceRenderers == null ||
                sourceRenderers.Length == 0)
            {
                Debug.LogWarning(
                    $"[{name}] No source renderers assigned.",
                    this);
            }
        }
        public void SetInteractionProgress(
             float progress)
        {
            progress = Mathf.Clamp01(progress);

            if (interactionOverlays == null)
                return;

            for (int i = 0;
                i < interactionOverlays.Length;
                i++)
            {
                Renderer renderer =
                    interactionOverlays[i];

                if (renderer == null)
                    continue;

                MaterialPropertyBlock block =
                    interactionPropertyBlocks[i];

                if (block == null)
                {
                    block =
                        new MaterialPropertyBlock();

                    interactionPropertyBlocks[i] =
                        block;
                }

                renderer.GetPropertyBlock(block);

                block.SetFloat(
                    InteractionProgressID,
                    progress);

                renderer.SetPropertyBlock(block);
            }
        }

        public void SetInteractionVisible(
            bool visible)
        {
            if (interactionOverlays == null)
                return;

            for (int i = 0;
                i < interactionOverlays.Length;
                i++)
            {
                Renderer renderer =
                    interactionOverlays[i];

                if (renderer == null)
                    continue;

                renderer.enabled = visible;
            }
        }

        // ---------------------------------------------------------------------
        // Prevent duplicate runtime overlays
        // ---------------------------------------------------------------------

        private void CleanupExistingScannerChildren()
        {
            for (int i = transform.childCount - 1; i >= 0; i--)
            {
                Transform child = transform.GetChild(i);

                if (child == null)
                    continue;

                string childName = child.name;

                if (childName.StartsWith("__ScannerOverlay_") ||
                    childName.StartsWith("__ScannerProjection_"))
                {
                    Destroy(child.gameObject);
                }
            }
        }

        // ---------------------------------------------------------------------
        // Effect 07
        // ---------------------------------------------------------------------

        private void CreateScannerOverlays()
        {
            scannerOverlays =
                new Renderer[sourceRenderers.Length];

            scannerPropertyBlocks =
                new MaterialPropertyBlock[sourceRenderers.Length];

            if (scannerMaterial == null)
                return;

            for (int i = 0; i < sourceRenderers.Length; i++)
            {
                Renderer source = sourceRenderers[i];

                if (source == null)
                    continue;

                GameObject overlay =
                    CreateOverlayObject(
                        source,
                        i,
                        "__ScannerOverlay_",
                        scannerMaterial);

                if (overlay == null)
                    continue;

                Renderer overlayRenderer =
                    overlay.GetComponent<Renderer>();

                if (overlayRenderer == null)
                    continue;

                scannerOverlays[i] =
                    overlayRenderer;

                scannerPropertyBlocks[i] =
                    new MaterialPropertyBlock();

                ConfigureScannerBounds(
                    i,
                    source,
                    overlayRenderer);
            }
        }

        // ---------------------------------------------------------------------
        // Effect 08
        // ---------------------------------------------------------------------

        private void CreateProjectionOverlays()
        {
            projectionOverlays =
                new Renderer[sourceRenderers.Length];

            projectionPropertyBlocks =
                new MaterialPropertyBlock[sourceRenderers.Length];

            if (projectionMaterial == null)
                return;

            for (int i = 0; i < sourceRenderers.Length; i++)
            {
                Renderer source = sourceRenderers[i];

                if (source == null)
                    continue;

                GameObject overlay =
                    CreateOverlayObject(
                        source,
                        i,
                        "__ScannerProjection_",
                        projectionMaterial);

                if (overlay == null)
                    continue;

                Renderer overlayRenderer =
                    overlay.GetComponent<Renderer>();

                if (overlayRenderer == null)
                    continue;

                projectionOverlays[i] =
                    overlayRenderer;

                projectionPropertyBlocks[i] =
                    new MaterialPropertyBlock();

                // Projection starts disabled.
                overlayRenderer.enabled = false;
            }
        }

        // ---------------------------------------------------------------------
        // Generic overlay creation
        // ---------------------------------------------------------------------

        private GameObject CreateOverlayObject(
            Renderer source,
            int index,
            string namePrefix,
            Material material)
        {
            if (source == null || material == null)
                return null;

            GameObject overlay =
                new GameObject(
                    $"{namePrefix}{index}");

            overlay.transform.SetParent(
                source.transform,
                false);

            overlay.layer =
                source.gameObject.layer;

            if (source is SkinnedMeshRenderer skinned)
            {
                if (skinned.sharedMesh == null)
                {
                    Destroy(overlay);

                    return null;
                }

                SkinnedMeshRenderer overlayRenderer =
                    overlay.AddComponent<SkinnedMeshRenderer>();

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

                overlayRenderer.sharedMaterial =
                    material;

                ConfigureOverlayRenderer(
                    overlayRenderer);

                return overlay;
            }

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

                MeshRenderer overlayRenderer =
                    overlay.AddComponent<MeshRenderer>();

                overlayRenderer.sharedMaterial =
                    material;

                ConfigureOverlayRenderer(
                    overlayRenderer);

                return overlay;
            }

            Debug.LogWarning(
                $"[{name}] Unsupported source renderer type: " +
                source.GetType().Name,
                source);

            Destroy(overlay);

            return null;
        }

        private static void ConfigureOverlayRenderer(
            Renderer renderer)
        {
            renderer.shadowCastingMode =
                ShadowCastingMode.Off;

            renderer.receiveShadows = false;

            renderer.motionVectorGenerationMode =
                MotionVectorGenerationMode.ForceNoMotion;
        }

        // ---------------------------------------------------------------------
        // Bounds
        // ---------------------------------------------------------------------

        private void ConfigureScannerBounds(
            int index,
            Renderer source,
            Renderer overlay)
        {
            if (source == null ||
                overlay == null)
            {
                return;
            }

            MaterialPropertyBlock block =
                scannerPropertyBlocks[index];

            Bounds bounds =
                GetLocalBounds(source);

            block.Clear();

            block.SetFloat(
                ScanMinYID,
                bounds.min.y);

            block.SetFloat(
                ScanMaxYID,
                bounds.max.y);

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

        // ---------------------------------------------------------------------
        // Effect 07 - progress
        // ---------------------------------------------------------------------

        public void SetScanProgress(
            float progress)
        {
            progress = Mathf.Clamp01(progress);

            if (scannerOverlays == null)
                return;

            for (int i = 0;
                 i < scannerOverlays.Length;
                 i++)
            {
                Renderer overlay =
                    scannerOverlays[i];

                if (overlay == null)
                    continue;

                MaterialPropertyBlock block =
                    scannerPropertyBlocks[i];

                if (block == null)
                {
                    block =
                        new MaterialPropertyBlock();

                    scannerPropertyBlocks[i] =
                        block;
                }

                overlay.GetPropertyBlock(block);

                block.SetFloat(
                    ScanProgressID,
                    progress);

                overlay.SetPropertyBlock(block);
            }
        }

        // ---------------------------------------------------------------------
        // Effect 07 - identified state
        // ---------------------------------------------------------------------

        public void SetIdentified(
            bool identified)
        {
            float value =
                identified ? 1f : 0f;

            if (scannerOverlays != null)
            {
                for (int i = 0;
                     i < scannerOverlays.Length;
                     i++)
                {
                    Renderer overlay =
                        scannerOverlays[i];

                    if (overlay == null)
                        continue;

                    MaterialPropertyBlock block =
                        scannerPropertyBlocks[i];

                    if (block == null)
                    {
                        block =
                            new MaterialPropertyBlock();

                        scannerPropertyBlocks[i] =
                            block;
                    }

                    overlay.GetPropertyBlock(block);

                    block.SetFloat(
                        IdentifiedID,
                        value);

                    overlay.SetPropertyBlock(block);
                }
            }

            // Effect 08 begins only after identification.
            SetProjectionVisible(identified);
        }

        // ---------------------------------------------------------------------
        // Effect 07 - global progress -> local progress
        // ---------------------------------------------------------------------

        public float EvaluateLocalProgress(
            float globalProgress)
        {
            if (scanEnd <= scanStart)
            {
                return globalProgress >= scanStart
                    ? 1f
                    : 0f;
            }

            return Mathf.Clamp01(
                Mathf.InverseLerp(
                    scanStart,
                    scanEnd,
                    globalProgress));
        }

        // ---------------------------------------------------------------------
        // Effect 08 - progress
        // ---------------------------------------------------------------------

        public void SetProjectionProgress(
            float progress)
        {
            progress = Mathf.Clamp01(progress);

            if (projectionOverlays == null)
                return;

            for (int i = 0;
                 i < projectionOverlays.Length;
                 i++)
            {
                Renderer overlay =
                    projectionOverlays[i];

                if (overlay == null)
                    continue;

                MaterialPropertyBlock block =
                    projectionPropertyBlocks[i];

                if (block == null)
                {
                    block =
                        new MaterialPropertyBlock();

                    projectionPropertyBlocks[i] =
                        block;
                }

                overlay.GetPropertyBlock(block);

                block.SetFloat(
                    ProjectionProgressID,
                    progress);

                overlay.SetPropertyBlock(block);
            }
        }

        // ---------------------------------------------------------------------
        // Effect 08 - visibility
        // ---------------------------------------------------------------------

        public void SetProjectionVisible(
            bool visible)
        {
            if (projectionOverlays == null)
                return;

            for (int i = 0;
                 i < projectionOverlays.Length;
                 i++)
            {
                Renderer overlay =
                    projectionOverlays[i];

                if (overlay == null)
                    continue;

                overlay.enabled = visible;
            }
        }

        // ---------------------------------------------------------------------
        // Utility
        // ---------------------------------------------------------------------

        private static void DestroyOverlayRenderers(
            Renderer[] renderers)
        {
            if (renderers == null)
                return;

            for (int i = 0;
                 i < renderers.Length;
                 i++)
            {
                Renderer renderer =
                    renderers[i];

                if (renderer == null)
                    continue;

                if (renderer.gameObject != null)
                    Destroy(renderer.gameObject);
            }
        }
        private void CreateInteractionOverlays()
{
    if (sourceRenderers == null ||
        sourceRenderers.Length == 0)
    {
        return;
    }

    interactionOverlays =
        new Renderer[sourceRenderers.Length];

    interactionPropertyBlocks =
        new MaterialPropertyBlock[sourceRenderers.Length];

    if (interactionMaterial == null)
    {
        Debug.LogWarning(
            $"[{name}] Effect 09 interaction material is not assigned.",
            this);

        return;
    }

    for (int i = 0; i < sourceRenderers.Length; i++)
    {
        Renderer source =
            sourceRenderers[i];

        if (source == null)
            continue;

        GameObject overlay =
            CreateOverlayObject(
                source,
                i,
                "__ScannerInteraction_",
                interactionMaterial);

        if (overlay == null)
            continue;

        Renderer overlayRenderer =
            overlay.GetComponent<Renderer>();

        if (overlayRenderer == null)
        {
            Destroy(overlay);
            continue;
        }

        interactionOverlays[i] =
            overlayRenderer;

        interactionPropertyBlocks[i] =
            new MaterialPropertyBlock();

        overlayRenderer.enabled = false;
    }
}
    }
}