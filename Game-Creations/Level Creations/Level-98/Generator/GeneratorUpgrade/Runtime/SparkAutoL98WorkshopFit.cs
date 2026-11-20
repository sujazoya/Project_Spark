using UnityEngine;
namespace ProjectSpark.AutoLevelGenerator
{
    /// <summary>Source/preview geometry scaling only; solver units remain real and unmodified.</summary>
    [DisallowMultipleComponent]
    public sealed class SparkAutoL98WorkshopFit : MonoBehaviour
    {
        [SerializeField] private Transform scaledVisualRoot;
        [SerializeField] private Vector2 tableSize = new Vector2(15.79f, 8.54f);
        [SerializeField] private Vector2 modelSize;
        [SerializeField] private Vector2 importedSize;
        [SerializeField] private float nativeUnitsPerMillimeter;
        [SerializeField] private bool sourceIsXZPlane = true;
        public Vector2 TableSize => tableSize;
        public Vector2 ModelSize => modelSize;
        public Vector2 MarginsEachSide => (tableSize - modelSize) * 0.5f;
        public Transform ScaledVisualRoot => scaledVisualRoot;
        public void Configure(Transform root, Vector2 table, Vector2 board, Vector2 source, float mmScale)
        {
            scaledVisualRoot = root;
            tableSize = table;
            modelSize = board;
            importedSize = source;
            nativeUnitsPerMillimeter = mmScale;
        }
    }
}
