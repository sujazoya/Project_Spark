using System;
using System.Collections;
using UnityEngine;

namespace ProjectSpark.Scanner
{
    public sealed class HolographicScannerController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private ScannerFeed feed;
        [SerializeField] private ScannerSimulationAdapter simulationAdapter;
        [SerializeField] private ScannerAnalyzerView analyzerView;
        [SerializeField] private ScannerVisualController visuals;

        [Header("Timing")]
        [SerializeField, Min(0.05f)] private float acquireDuration = 0.8f;
        [SerializeField, Min(0.05f)] private float scanDuration = 1.8f;
        [SerializeField, Min(0.05f)] private float analyzeDuration = 1.2f;
        [SerializeField, Min(0.05f)] private float resultDuration = 0.8f;

        [Header("Behaviour")]
        [SerializeField] private bool autoStart;
        [SerializeField] private bool allowRescan = true;

        public ScannerStage Stage { get; private set; } = ScannerStage.Idle;
        public ScannerResult Result { get; private set; } = ScannerResult.Unknown;
        public float Progress01 { get; private set; }

        public event Action<ScannerStage> StageChanged;
        public event Action<ScannerResult> ResultChanged;
        public event Action<float> ProgressChanged;

        private Coroutine scanRoutine;
        private readonly ScannerAnalyzer analyzer = new();

        private void Start()
        {
            if (autoStart)
                StartScan();
        }

        public void StartScan()
        {
            if (!allowRescan && Stage != ScannerStage.Idle)
                return;

            if (scanRoutine != null)
                StopCoroutine(scanRoutine);

            scanRoutine = StartCoroutine(RunScan());
        }

        private IEnumerator RunScan()
        {
            Result = ScannerResult.Unknown;
            SetStage(ScannerStage.Acquire);
            yield return RunPhase(acquireDuration);

            SetStage(ScannerStage.Scan);
            visuals?.SetScanning(true);
            yield return RunPhase(scanDuration);

            feed.BeginCapture();
            simulationAdapter.Capture(feed);
            feed.EndCapture();

            SetStage(ScannerStage.Analyze);
            visuals?.SetReconstructing(true);
            yield return RunPhase(analyzeDuration);

            Result = analyzer.Analyze(feed.Capture);
            analyzerView?.ApplyCapture(feed.Capture, analyzer.Faults);
            ResultChanged?.Invoke(Result);

            SetStage(ScannerStage.Result);
            visuals?.SetScanning(false);
            visuals?.SetReconstructing(false);
            visuals?.SetFaultState(Result == ScannerResult.Fault);

            yield return RunPhase(resultDuration);

            Progress01 = 1f;
            ProgressChanged?.Invoke(Progress01);
        }

        private IEnumerator RunPhase(float duration)
        {
            float time = 0f;

            while (time < duration)
            {
                time += Time.deltaTime;
                Progress01 = Mathf.Clamp01(time / duration);
                ProgressChanged?.Invoke(Progress01);
                yield return null;
            }

            Progress01 = 1f;
            ProgressChanged?.Invoke(Progress01);
        }

        private void SetStage(ScannerStage stage)
        {
            Stage = stage;
            StageChanged?.Invoke(stage);
            analyzerView?.SetStage(stage);
        }
    }
}
