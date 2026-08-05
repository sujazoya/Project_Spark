// ============================================================================
// PlacementValidator.cs
// ============================================================================

using UnityEngine;

namespace ProjectSpark.Gameplay.Flashlight
{
    public sealed class PlacementValidator : MonoBehaviour
    {
        [SerializeField]
        ComponentSocket[] sockets;

        public bool Completed
        {
            get
            {
                foreach (ComponentSocket socket in sockets)
                {
                    if (!socket.Occupied)
                        return false;
                }

                return true;
            }
        }
    }
}