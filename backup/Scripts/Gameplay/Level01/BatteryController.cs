// Assets/My_Assets/_Project_Spark/Scripts/Gameplay/Level01/BatteryController.cs

using UnityEngine;
using ProjectSpark.Gameplay.Wiring;

namespace ProjectSpark.Gameplay.Level01
{
    public sealed class BatteryController : MonoBehaviour
    {
        [SerializeField] private float voltage = 1.5f;

        [SerializeField] private WireConnector positive;

        [SerializeField] private WireConnector negative;

        public float Voltage => voltage;

        public WireConnector Positive => positive;

        public WireConnector Negative => negative;
    }
}