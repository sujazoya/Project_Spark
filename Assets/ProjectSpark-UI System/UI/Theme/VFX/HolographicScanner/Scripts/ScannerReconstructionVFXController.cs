using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

namespace ProjectSpark.Scanner
{
    [DisallowMultipleComponent]
    public sealed class ScannerReconstructionVFXController : MonoBehaviour
    {
        [Header("Target")]
        [SerializeField]
        private Transform targetRoot;

        [SerializeField]
        private bool includeInactiveRenderers = false;

        [Header("Effect04")]
        [SerializeField]
        private VisualEffect effect04Prefab;

        [SerializeField]
        private Transform effectParent;

        [Header("Particle Settings")]
        [SerializeField, Min(0f)]
        private float particleRate = 700f;

        [Header("Rebuild")]
        [SerializeField]
        private bool rebuildOnAwake = true;

        [Header("VFX Property Names")]
        [SerializeField]
        private string reconstructingProperty =
            "_IsReconstructing";

        [SerializeField]
        private string particleRateProperty =
            "_ParticleRate";

        private readonly List<ScannerReconstructionVFXTarget>
            targets =
                new List<ScannerReconstructionVFXTarget>();

        private Bounds scannerBounds;

        private bool scannerBoundsValid;
        private bool isPlaying;
        private float revealHeight;

        public float RevealHeight =>
            revealHeight;

        public bool IsPlaying =>
            isPlaying;

        public IReadOnlyList<
            ScannerReconstructionVFXTarget>
            Targets =>
            targets;

        private void Awake()
        {
            if (!rebuildOnAwake)
                return;

            RebuildTargets();
            ResetVFX();
        }

        private void OnDestroy()
        {
            ClearTargets();
        }

        // =========================================================
        // BUILD
        // =========================================================

        public void RebuildTargets()
        {
            ClearTargets();

            if (targetRoot == null)
            {
                Debug.LogError(
                    $"{name}: Target Root is not assigned.",
                    this);

                return;
            }

            if (effect04Prefab == null)
            {
                Debug.LogError(
                    $"{name}: Effect04 Prefab is not assigned.",
                    this);

                return;
            }

            MeshRenderer[] renderers =
                targetRoot.GetComponentsInChildren<MeshRenderer>(
                    includeInactiveRenderers);

            if (renderers == null ||
                renderers.Length == 0)
            {
                Debug.LogWarning(
                    $"{name}: No MeshRenderers found under Target Root.",
                    this);

                return;
            }

            CalculateScannerBounds(
                renderers);

            if (!scannerBoundsValid)
            {
                Debug.LogWarning(
                    $"{name}: Scanner bounds could not be calculated.",
                    this);

                return;
            }

            Transform parent =
                effectParent != null
                    ? effectParent
                    : transform;

            for (int i = 0;
                 i < renderers.Length;
                 i++)
            {
                MeshRenderer renderer =
                    renderers[i];

                if (renderer == null)
                    continue;

                CreateTarget(
                    renderer,
                    parent);
            }

            revealHeight = 0f;
        }

        private void CreateTarget(
            MeshRenderer renderer,
            Transform parent)
        {
            VisualEffect effect =
                Instantiate(
                    effect04Prefab,
                    parent);

            effect.name =
                $"Effect04_{renderer.gameObject.name}";

            ScannerReconstructionVFXTarget target =
                effect.GetComponent<
                    ScannerReconstructionVFXTarget>();

            if (target == null)
            {
                target =
                    effect.gameObject.AddComponent<
                        ScannerReconstructionVFXTarget>();
            }

            target.Initialize(
                renderer,
                effect,
                scannerBounds,
                particleRate);

            targets.Add(target);
        }

        private void ClearTargets()
        {
            for (int i = targets.Count - 1;
                 i >= 0;
                 i--)
            {
                ScannerReconstructionVFXTarget target =
                    targets[i];

                if (target == null)
                    continue;

                DestroyRuntime(
                    target.gameObject);
            }

            targets.Clear();
        }

        // =========================================================
        // BOUNDS
        // =========================================================

        private void CalculateScannerBounds(
            MeshRenderer[] renderers)
        {
            scannerBounds =
                new Bounds();

            scannerBoundsValid =
                false;

            for (int i = 0;
                 i < renderers.Length;
                 i++)
            {
                MeshRenderer renderer =
                    renderers[i];

                if (renderer == null)
                    continue;

                if (!scannerBoundsValid)
                {
                    scannerBounds =
                        renderer.bounds;

                    scannerBoundsValid =
                        true;
                }
                else
                {
                    scannerBounds.Encapsulate(
                        renderer.bounds);
                }
            }
        }

        // =========================================================
        // START
        // =========================================================

        public void StartReconstruction()
        {
            if (targets.Count == 0)
                RebuildTargets();

            if (targets.Count == 0)
                return;

            isPlaying = true;
            revealHeight = 0f;

            RefreshTargets();

            for (int i = 0;
                 i < targets.Count;
                 i++)
            {
                ScannerReconstructionVFXTarget target =
                    targets[i];

                if (target == null)
                    continue;

                target.SetRevealHeight(
                    revealHeight);

                target.Play();
            }
        }

        // =========================================================
        // STOP
        // =========================================================

        public void StopReconstruction()
        {
            isPlaying = false;

            for (int i = 0;
                 i < targets.Count;
                 i++)
            {
                ScannerReconstructionVFXTarget target =
                    targets[i];

                if (target == null)
                    continue;

                target.Stop();
            }
        }

        // =========================================================
        // RESET
        // =========================================================

        public void ResetVFX()
        {
            isPlaying = false;
            revealHeight = 0f;

            for (int i = 0;
                 i < targets.Count;
                 i++)
            {
                ScannerReconstructionVFXTarget target =
                    targets[i];

                if (target == null)
                    continue;

                target.ResetTarget();
            }
        }

        // =========================================================
        // COMPLETE
        // =========================================================

        public void CompleteReconstruction()
        {
            revealHeight = 1f;

            for (int i = 0;
                 i < targets.Count;
                 i++)
            {
                ScannerReconstructionVFXTarget target =
                    targets[i];

                if (target == null)
                    continue;

                target.Complete();
            }

            isPlaying = true;
        }

        // =========================================================
        // PROGRESS
        // =========================================================

        public void SetProgress(
            float normalized)
        {
            revealHeight =
                Mathf.Clamp01(
                    normalized);

            ApplyReveal();
        }

        public void SetRevealHeight(
            float normalized)
        {
            SetProgress(normalized);
        }

        private void ApplyReveal()
        {
            for (int i = 0;
                 i < targets.Count;
                 i++)
            {
                ScannerReconstructionVFXTarget target =
                    targets[i];

                if (target == null)
                    continue;

                target.SetRevealHeight(
                    revealHeight);
            }
        }

        // =========================================================
        // REFRESH
        // =========================================================

        public void RefreshTargets()
        {
            if (!scannerBoundsValid)
                return;

            for (int i = 0;
                 i < targets.Count;
                 i++)
            {
                ScannerReconstructionVFXTarget target =
                    targets[i];

                if (target == null)
                    continue;

                target.Refresh(
                    scannerBounds);

                target.SetRevealHeight(
                    revealHeight);
            }
        }

        // =========================================================
        // RUNTIME
        // =========================================================

        private void Update()
        {
            if (!isPlaying)
                return;

            /*
             * We deliberately do not update the target bounds
             * every frame.
             *
             * Renderer bounds normally remain stable and
             * constantly rebuilding them would be wasteful.
             */
        }

        // =========================================================
        // CLEANUP
        // =========================================================

        private static void DestroyRuntime(
            GameObject target)
        {
            if (target == null)
                return;

            if (Application.isPlaying)
            {
                Destroy(target);
            }
            else
            {
                DestroyImmediate(target);
            }
        }
    }
}