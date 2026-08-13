using System.Collections.Generic;
using UnityEngine;

namespace ProjectSpark.Scanner
{
    /// <summary>
    /// Presentation-side data sink. Connect this to the player's existing AAAUI/UI layer.
    /// </summary>
    public sealed class ScannerAnalyzerView : MonoBehaviour
    {
        [SerializeField] private ScannerHUDController hud;

        public void SetStage(ScannerStage stage)
        {
            hud?.SetStage(stage);
        }

        public void ApplyCapture(
            ScannerCapture capture,
            IReadOnlyList<ScannerFault> faults)
        {
            hud?.SetCapture(capture, faults);
        }
    }
}
