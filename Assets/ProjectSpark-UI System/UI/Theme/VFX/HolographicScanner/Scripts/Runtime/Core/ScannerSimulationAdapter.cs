using UnityEngine;

namespace ProjectSpark.Scanner
{
    /// <summary>
    /// Production integration point. Derive this once in Project Spark and map the
    /// existing simulation APIs into the scanner feed.
    /// </summary>
    public abstract class ScannerSimulationAdapter : MonoBehaviour
    {
        public abstract void Capture(ScannerFeed feed);
    }
}
