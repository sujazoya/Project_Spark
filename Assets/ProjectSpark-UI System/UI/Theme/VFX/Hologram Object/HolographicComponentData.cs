using UnityEngine;

namespace ProjectSpark.HolographicViewer
{
    public sealed class HolographicComponentData : MonoBehaviour
    {
        [Header("Identity")]
        [SerializeField] private string componentName = "Component";
        [SerializeField] private string partNumber = "N/A";
        [SerializeField] private string componentType = "Mechanical";

        [Header("Engineering")]
        [TextArea(2, 5)]
        [SerializeField] private string description =
            "No description available.";

        [SerializeField] private string specification01;
        [SerializeField] private string specification02;
        [SerializeField] private string specification03;

        public string ComponentName => componentName;
        public string PartNumber => partNumber;
        public string ComponentType => componentType;
        public string Description => description;

        public string Specification01 => specification01;
        public string Specification02 => specification02;
        public string Specification03 => specification03;
    }
}