using UnityEngine;

namespace ProjectSpark.Gameplay.Scenarios
{
    public class ScenarioManager
        : MonoBehaviour
    {
        [SerializeField]
        private ScenarioDefinition
            currentScenario;

        private readonly ScenarioLoader
            loader =
                new();

        private readonly ObjectiveManager
            objectives =
                new();

        public void StartScenario()
        {
            ScenarioDefinition scenario =
                loader.Load(
                    currentScenario);

            objectives.Initialize(
                scenario.Objectives);

            ScenarioEvents
                .RaiseStarted();
        }
    }
}
