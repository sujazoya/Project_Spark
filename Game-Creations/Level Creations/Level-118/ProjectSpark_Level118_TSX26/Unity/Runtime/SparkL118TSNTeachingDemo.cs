using System;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectSpark.Level118
{
    /// <summary>
    /// Educational, deterministic time-aware gate demonstration. Not electrical authority,
    /// not an IEEE 802.1Qbv conformance test, and cannot change objective progress.
    /// Explicit execution only: no Update(), no network access, no Project Spark gameplay API calls.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class SparkL118TSNTeachingDemo : MonoBehaviour
    {
        public enum TrafficClass { SafetyControl, ClockSync, BestEffort }

        [Serializable]
        public struct GateWindow
        {
            public TrafficClass trafficClass;
            public int opensAtMicroseconds;
            public int closesAtMicroseconds;
        }

        [Serializable]
        public struct TeachingResult
        {
            public TrafficClass trafficClass;
            public int arrivalTimeMicroseconds;
            public int transmissionStartMicroseconds;
            public int queueDelayMicroseconds;
            public int wireTimeMicroseconds;
            public bool acceptedByIllustrativeSchedule;
        }

        [Header("Illustrative parameters; NOT hardware measurements")]
        [SerializeField, Min(100)] private int cycleMicroseconds = 1000;
        [SerializeField, Min(10)] private int linkMegabitsPerSecond = 1000;
        [SerializeField] private GateWindow[] windows =
        {
            new GateWindow { trafficClass = TrafficClass.SafetyControl, opensAtMicroseconds = 0, closesAtMicroseconds = 200 },
            new GateWindow { trafficClass = TrafficClass.ClockSync, opensAtMicroseconds = 200, closesAtMicroseconds = 400 },
            new GateWindow { trafficClass = TrafficClass.BestEffort, opensAtMicroseconds = 400, closesAtMicroseconds = 1000 }
        };
        [SerializeField] private TeachingResult[] lastResults = new TeachingResult[0];
        [SerializeField, TextArea] private string notice =
            "ILLUSTRATIVE TIMELINE ONLY. No gPTP, FPGA, certified TSN link, real PHY, solder connectivity or gameplay objective authority.";

        public bool IsAuthoritative => false;
        public IReadOnlyList<TeachingResult> LastResults => lastResults;

        [ContextMenu("Run non-certifiable TSN gate teaching sample")]
        public void RunSample()
        {
            // Explicit arrival data: deterministic repeatability. Values use microseconds and bytes.
            TrafficClass[] classes = { TrafficClass.BestEffort, TrafficClass.SafetyControl,
                TrafficClass.ClockSync, TrafficClass.SafetyControl, TrafficClass.BestEffort };
            int[] arrivals = { 25, 80, 125, 480, 520 };
            int[] frameBytes = { 512, 128, 96, 128, 900 };
            int cycle = Mathf.Max(cycleMicroseconds, 100);
            int rate = Mathf.Max(linkMegabitsPerSecond, 10);
            lastResults = new TeachingResult[classes.Length];
            for (int p = 0; p < classes.Length; p++)
            {
                int wireTime = Mathf.CeilToInt((frameBytes[p] * 8f) / rate);
                int start = -1;
                for (int candidate = arrivals[p]; candidate < arrivals[p] + cycle * 8; candidate++)
                {
                    int phase = candidate % cycle;
                    if (phase < 0) phase += cycle;
                    for (int w = 0; w < windows.Length; w++)
                    {
                        GateWindow gate = windows[w];
                        if (gate.trafficClass == classes[p] && phase >= gate.opensAtMicroseconds &&
                            phase + wireTime <= gate.closesAtMicroseconds &&
                            gate.closesAtMicroseconds <= cycle)
                        {
                            start = candidate;
                            break;
                        }
                    }
                    if (start >= 0) break;
                }
                lastResults[p] = new TeachingResult
                {
                    trafficClass = classes[p],
                    arrivalTimeMicroseconds = arrivals[p],
                    transmissionStartMicroseconds = start,
                    queueDelayMicroseconds = start < 0 ? -1 : start - arrivals[p],
                    wireTimeMicroseconds = wireTime,
                    acceptedByIllustrativeSchedule = start >= 0
                };
            }
            Debug.Log("[L118 DEMO] Teaching gate sample generated. This is not actual TSN or electrical evidence.");
        }
    }
}
