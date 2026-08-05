using UnityEngine;

namespace ProjectSpark.Gameplay.Placement
{
    public sealed class PlacementGhost : MonoBehaviour
    {
        [SerializeField]
        private MeshRenderer[] renderers;

        public void SetValid(bool valid)
        {
            foreach (var r in renderers)
            {
                r.material.color =
                    valid
                    ? Color.green
                    : Color.red;
            }
        }
    }
}
