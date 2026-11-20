using UnityEngine;

namespace ProjectSpark.AutoLevelGenerator
{
    /// <summary>Inspectable workshop fit created in Unity by the Level 105 Editor generator.</summary>
    [DisallowMultipleComponent]
    public sealed class SparkAutoL105WorkshopFit : MonoBehaviour
    {
        [SerializeField] private Transform modelScaleRoot;
        [SerializeField] private Vector2 tabletopSize = new Vector2(15.79f, 8.54f);
        [SerializeField] private Vector2 boardSize = new Vector2(10.13f, 7.51f);
        [SerializeField] private Vector2 sourceImportSize;
        [SerializeField] private bool sourceWasXYPlane;
        [SerializeField] private float importedUnitsPerMillimeter;
        public Vector2 TabletopSize => tabletopSize;
        public Vector2 BoardSize => boardSize;
        public Vector2 CenteredClearanceEachSide => 0.5f * (tabletopSize - boardSize);
        public Transform ModelScaleRoot => modelScaleRoot;
        public void Configure(Transform root, float tableWidth, float tableDepth, float boardWidth,
            float boardDepth, float importWidth, float importDepth, bool planeXY, float unitPerMm)
        {
            modelScaleRoot = root;
            tabletopSize = new Vector2(tableWidth, tableDepth);
            boardSize = new Vector2(boardWidth, boardDepth);
            sourceImportSize = new Vector2(importWidth, importDepth);
            sourceWasXYPlane = planeXY;
            importedUnitsPerMillimeter = unitPerMm;
        }
    }
}
