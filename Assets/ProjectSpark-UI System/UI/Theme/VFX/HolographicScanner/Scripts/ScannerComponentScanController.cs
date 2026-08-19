using UnityEngine;

namespace ProjectSpark.Scanner
{
    /// <summary>
    /// Controls visual scanning of real component targets.
    ///
    /// Simulation remains the source of truth.
    /// This controller only drives scanner presentation.
    /// </summary>
    public sealed class ScannerComponentScanController : MonoBehaviour
    {
        [SerializeField]
        private ScannerComponentTarget[] targets;

        public ScannerComponentTarget[] Targets =>
            targets;

        // ---------------------------------------------------------------------
        // Effect 07
        // ---------------------------------------------------------------------

        public void SetProgress(
            float globalProgress)
        {
            globalProgress =
                Mathf.Clamp01(globalProgress);

            if (targets == null)
                return;

            for (int i = 0;
                 i < targets.Length;
                 i++)
            {
                ScannerComponentTarget target =
                    targets[i];

                if (target == null)
                    continue;

                float localProgress =
                    target.EvaluateLocalProgress(
                        globalProgress);

                target.SetScanProgress(
                    localProgress);

                bool identified =
                    localProgress >= 1f;

                target.SetIdentified(
                    identified);
            }
        }

        // ---------------------------------------------------------------------
        // Reset
        // ---------------------------------------------------------------------

        public void ResetScan()
        {
            if (targets == null)
                return;

            for (int i = 0;
                 i < targets.Length;
                 i++)
            {
                ScannerComponentTarget target =
                    targets[i];

                if (target == null)
                    continue;

                target.SetScanProgress(0f);
                target.SetIdentified(false);

                target.SetProjectionProgress(0f);
                target.SetProjectionVisible(false);
            }
        }

        // ---------------------------------------------------------------------
        // Complete component scan
        // ---------------------------------------------------------------------

        public void CompleteScan()
        {
            if (targets == null)
                return;

            for (int i = 0;
                 i < targets.Length;
                 i++)
            {
                ScannerComponentTarget target =
                    targets[i];

                if (target == null)
                    continue;

                target.SetScanProgress(1f);
                target.SetIdentified(true);
            }
        }

        // ---------------------------------------------------------------------
        // Effect 08
        // ---------------------------------------------------------------------

        public void SetProjectionProgress(
            float progress)
        {
            progress =
                Mathf.Clamp01(progress);

            if (targets == null)
                return;

            for (int i = 0;
                 i < targets.Length;
                 i++)
            {
                ScannerComponentTarget target =
                    targets[i];

                if (target == null)
                    continue;

                target.SetProjectionProgress(
                    progress);
            }
        }

        public void SetProjectionVisible(
            bool visible)
        {
            if (targets == null)
                return;

            for (int i = 0;
                 i < targets.Length;
                 i++)
            {
                ScannerComponentTarget target =
                    targets[i];

                if (target == null)
                    continue;

                target.SetProjectionVisible(
                    visible);
            }
        }

        // ---------------------------------------------------------------------
        // Complete projection
        // ---------------------------------------------------------------------

        public void CompleteProjection()
        {
            if (targets == null)
                return;

            for (int i = 0;
                 i < targets.Length;
                 i++)
            {
                ScannerComponentTarget target =
                    targets[i];

                if (target == null)
                    continue;

                target.SetProjectionProgress(1f);
                target.SetProjectionVisible(true);
            }
        }

        // ---------------------------------------------------------------------
        // Utility
        // ---------------------------------------------------------------------

        public void ClearProjection()
        {
            if (targets == null)
                return;

            for (int i = 0;
                 i < targets.Length;
                 i++)
            {
                ScannerComponentTarget target =
                    targets[i];

                if (target == null)
                    continue;

                target.SetProjectionProgress(0f);
                target.SetProjectionVisible(false);
            }
        }
    }
}