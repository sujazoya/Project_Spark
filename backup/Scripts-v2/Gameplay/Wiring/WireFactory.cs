using Unity.Entities;
using UnityEngine;

namespace ProjectSpark.Gameplay.Wiring
{
    public sealed class WireFactory : MonoBehaviour
    {
        [SerializeField]
        private WireController prefab;
        public Wire Create()
        {
            return new Wire();
        }
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
