using System;
using ProjectSpark.Gameplay.Grid;
using UnityEngine;

namespace ProjectSpark.Gameplay.Levels
{
    [Serializable]
    public class SpawnDefinition
    {
        public string ComponentId;

        public GridCoordinate Coordinate;

        public int Rotation;
    }
}
