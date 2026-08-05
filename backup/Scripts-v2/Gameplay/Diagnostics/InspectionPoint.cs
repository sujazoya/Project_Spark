using UnityEngine;

namespace ProjectSpark.Gameplay.Diagnostics
{
    public sealed class InspectionPoint : MonoBehaviour
    {
        [SerializeField]
        private string pointId;

        public string PointId => pointId;
    }
}
