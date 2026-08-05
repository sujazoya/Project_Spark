// Assets/My_Assets/_Project_Spark/Scripts/Gameplay/Wiring/WireFactory.cs

using UnityEngine;

namespace ProjectSpark.Gameplay.Wiring
{
    public sealed class WireFactory : MonoBehaviour
    {
        [SerializeField]
        private WireController prefab;

        public WireController Spawn(
            WireConnector start)
        {
            WireController wire =
                Instantiate(prefab);

            wire.Initialize(start);

            return wire;
        }
    }
}