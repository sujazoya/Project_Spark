using System;
using UnityEngine;

namespace ProjectSpark.Gameplay.ThermalDominion
{
    internal sealed class SparkLevel114ThermalPlant
    {
        public const float Step = .05f;
        private const int U1 = 0, U2 = 1, Bank = 2, HS1 = 3, HS2 = 4, Plane = 5, Substrate = 6;
        private static readonly float[] Capacities = { 1.1f, 1.25f, 1.15f, 3.2f, 3.0f, 2.0f, 4.5f };
        private readonly float[] heat = new float[Capacities.Length];
        private readonly float[] temperature = { 25f, 25f, 25f, 25f, 25f, 25f, 25f };

        public float Cpu => temperature[U1];
        public float Accelerator => temperature[U2];
        public float Vrm => temperature[Bank];
        public float Sink => temperature[HS1];
        public float SecondSink => temperature[HS2];
        public float Board => temperature[Substrate];
        public float Airflow { get; private set; }
        public float Tach { get; private set; }
        public float TimFlux { get; private set; }
        public float VrmFlux { get; private set; }

        public float Sensor(int index) => index switch
        {
            0 => Cpu, 1 => Accelerator, 2 => Vrm, 3 => Sink, 4 => SecondSink,
            5 => temperature[Plane], 6 => Board, _ => (Board + SecondSink) * .5f
        };

        public void Advance(float cpuWatts, float acceleratorWatts, float vrmWatts, float request,
            bool cpuSink, bool secondSink, bool pipeA, bool pipeB, bool delaminated, bool stalled, bool viaOpen)
        {
            Airflow = Mathf.MoveTowards(Airflow, request * (stalled ? .17f : 1f), Step * 1.5f);
            Tach = request <= 0f ? 0f : stalled ? .96f : Airflow;
            Array.Clear(heat, 0, heat.Length);
            heat[U1] = cpuWatts;
            heat[U2] = acceleratorWatts;
            heat[Bank] = vrmWatts;
            TimFlux = Exchange(U1, HS1, cpuSink ? delaminated ? .36f : 1.75f : 0f);
            Exchange(U2, HS2, secondSink ? 1.52f : 0f);
            Exchange(U1, Substrate, .11f);
            Exchange(U2, Substrate, .13f);
            VrmFlux = Exchange(Bank, Plane, viaOpen ? .08f : 1.15f);
            Exchange(Bank, Substrate, .085f);
            Exchange(Plane, HS1, pipeA ? .63f : 0f);
            Exchange(Plane, HS2, pipeB ? .63f : 0f);
            Exchange(HS1, HS2, pipeA && pipeB ? .36f : .04f);
            Exchange(Plane, Substrate, .24f);
            Cool(U1, .07f);
            Cool(U2, .08f);
            Cool(Bank, .084f + .11f * Airflow);
            Cool(HS1, .14f + .87f * Airflow);
            Cool(HS2, .16f + .80f * Airflow);
            Cool(Substrate, .33f + .19f * Airflow);
            Cool(Plane, .21f + .22f * Airflow);
            for (int node = 0; node < temperature.Length; node++)
                temperature[node] += Step * heat[node] / Capacities[node];
        }

        private float Exchange(int from, int to, float conductance)
        {
            float watts = conductance * (temperature[from] - temperature[to]);
            heat[from] -= watts;
            heat[to] += watts;
            return watts;
        }

        private void Cool(int node, float conductance) => heat[node] -= (temperature[node] - 25f) * conductance;
    }

    internal sealed class SparkLevel114HeatRun
    {
        public const int Samples = 960;
        public const float VrmRiseLimit = 14f;
        public int Count { get; private set; }
        public float PeakCpu { get; private set; }
        public float PeakAccelerator { get; private set; }
        public float PeakVrm { get; private set; }
        public float PeakDelta { get; private set; }
        public float SustainedFlow { get; private set; }
        public float TachError { get; private set; }
        public float PeakVrmDrop { get; private set; }
        public float IdleCpu { get; private set; }
        public bool RailsStable { get; private set; } = true;
        public bool Throttled { get; private set; }
        public bool Complete => Count == Samples;
        public bool Healthy => Complete && RailsStable && !Throttled &&
            PeakCpu < 75f && PeakAccelerator < 74f && PeakVrm < 68f && PeakDelta < 18f &&
            PeakVrmDrop < VrmRiseLimit && SustainedFlow >= .68f && TachError < .15f && PeakCpu > IdleCpu + 5f;

        public void Record(SparkLevel114ThermalPlant plant, bool rails, bool throttled)
        {
            if (Count >= Samples) return;
            if (Count == 59) IdleCpu = plant.Cpu;
            PeakCpu = Mathf.Max(PeakCpu, plant.Cpu);
            PeakAccelerator = Mathf.Max(PeakAccelerator, plant.Accelerator);
            PeakVrm = Mathf.Max(PeakVrm, plant.Vrm);
            PeakDelta = Mathf.Max(PeakDelta, plant.Cpu - plant.Sink);
            TachError = Mathf.Max(TachError, Mathf.Abs(plant.Tach - plant.Airflow));
            if (Count >= 300 && Count < 840)
            {
                SustainedFlow = Mathf.Max(SustainedFlow, plant.Airflow);
                PeakVrmDrop = Mathf.Max(PeakVrmDrop, plant.Vrm - plant.Sensor(5));
            }
            RailsStable &= rails;
            Throttled |= throttled;
            Count++;
        }
    }
}
