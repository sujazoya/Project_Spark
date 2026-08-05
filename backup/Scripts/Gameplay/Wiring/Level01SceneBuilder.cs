// Assets/My_Assets/_Project_Spark/Scripts/Gameplay/Wiring/Level01SceneBuilder.cs

using UnityEngine;
using ProjectSpark.Gameplay.Level01;

namespace ProjectSpark.Gameplay.Wiring
{
    public sealed class Level01SceneBuilder : MonoBehaviour
    {
        [SerializeField]
        GameObject board;

        [SerializeField]
        BatteryController battery;

        [SerializeField]
        BulbController bulb;

        [SerializeField]
        WireFactory wireFactory;

        [SerializeField]
        WireConnector batteryPositive;

        void Start()
        {
            wireFactory.Spawn(
                batteryPositive);
        }
    }
}