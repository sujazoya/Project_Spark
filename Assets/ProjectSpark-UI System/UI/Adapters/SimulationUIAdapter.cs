using System;
using UnityEngine;
using ProjectSpark.UI.Data;

namespace ProjectSpark.UI.Adapters
{
    public sealed class SimulationUIAdapter :
        MonoBehaviour
    {
        public event Action<
            SimulationViewModel>
            ViewModelChanged;

        public SimulationViewModel Current
        {
            get;
            private set;
        }

        public void UpdateSimulation(
            bool isRunning,
            bool hasFault,
            float voltage,
            float current,
            float power,
            int faultCount,
            string statusText)
        {
            Current =
                new SimulationViewModel(
                    isRunning,
                    hasFault,
                    voltage,
                    current,
                    power,
                    faultCount,
                    statusText);

            ViewModelChanged?.Invoke(
                Current);
        }
    }
}