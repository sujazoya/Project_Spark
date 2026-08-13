using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ProjectSpark.Scanner
{
    public sealed class ScannerHUDController : MonoBehaviour
    {
        [Header("Stage")]
        [SerializeField] private TMP_Text stageTitle;
        [SerializeField] private TMP_Text stageSubtitle;

        [Header("Status")]
        [SerializeField] private TMP_Text initializationValue;
        [SerializeField] private TMP_Text acquiringValue;
        [SerializeField] private TMP_Text scanningValue;
        [SerializeField] private TMP_Text analyzingValue;
        [SerializeField] private TMP_Text faultValue;

        [Header("Result")]
        [SerializeField] private TMP_Text resultCode;
        [SerializeField] private TMP_Text resultTitle;
        [SerializeField] private TMP_Text resultDetail;

        [Header("Metrics")]
        [SerializeField] private TMP_Text componentCount;
        [SerializeField] private TMP_Text connectionCount;
        [SerializeField] private TMP_Text analysisPercent;
        [SerializeField] private Slider powerLevel;

        private static readonly string[] StageTitles =
        {
            "IDLE", "1. ACQUIRE", "2. SCAN", "3. ANALYZE", "4. RESULT"
        };

        public void SetStage(ScannerStage stage)
        {
            int index = Mathf.Clamp((int)stage, 0, StageTitles.Length - 1);
            stageTitle.text = StageTitles[index];

            switch (stage)
            {
                case ScannerStage.Acquire:
                    stageSubtitle.text = "TARGET LOCK";
                    MarkStatus(initializationValue, true);
                    break;

                case ScannerStage.Scan:
                    stageSubtitle.text = "SCANNING...";
                    MarkStatus(acquiringValue, true);
                    MarkStatus(scanningValue, true);
                    break;

                case ScannerStage.Analyze:
                    stageSubtitle.text = "RECONSTRUCTING...";
                    MarkStatus(analyzingValue, true);
                    break;

                case ScannerStage.Result:
                    stageSubtitle.text = "ANALYSIS COMPLETE";
                    break;

                default:
                    stageSubtitle.text = string.Empty;
                    break;
            }
        }

        public void SetCapture(
            ScannerCapture capture,
            IReadOnlyList<ScannerFault> faults)
        {
            componentCount.text = capture.Components.Count.ToString();
            connectionCount.text = capture.Connections.Count.ToString();
            analysisPercent.text = "100%";

            if (faults == null || faults.Count == 0)
            {
                faultValue.text = "NO FAULTS";
                resultCode.text = "PASS";
                resultTitle.text = "CIRCUIT VALID";
                resultDetail.text = "Electrical reconstruction completed successfully.";
                return;
            }

            ScannerFault fault = faults[0];

            faultValue.text = fault.code;
            resultCode.text = fault.code;
            resultTitle.text = fault.title;
            resultDetail.text = fault.detail;
        }

        public void SetPower(float value01)
        {
            if (powerLevel != null)
                powerLevel.value = Mathf.Clamp01(value01);
        }

        private static void MarkStatus(TMP_Text target, bool complete)
        {
            if (target != null)
                target.text = complete ? "✓" : "—";
        }
    }
}
