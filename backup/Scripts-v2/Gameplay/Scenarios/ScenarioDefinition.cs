using System.Collections.Generic;
using UnityEngine;
using ProjectSpark.Gameplay.Devices;
using ProjectSpark.Gameplay.Repair;

namespace ProjectSpark.Gameplay.Scenarios
{
    [CreateAssetMenu(
        menuName="Project Spark/Scenario")]
    public class ScenarioDefinition
        : ScriptableObject
    {
        public string ScenarioId;

        public string DisplayName;

        public DeviceDefinition Device;

        public List<Fault> Faults =
            new();

        public List<Objective> Objectives =
            new();

        public RewardDefinition Reward;

        public DifficultyProfile Difficulty;
    }
}
