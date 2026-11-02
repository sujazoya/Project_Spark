using System;
using UnityEngine;

namespace ProjectSpark.UI.VisualTeaching
{
    [CreateAssetMenu(fileName = "Level_Drawing", menuName = "Project Spark/Visual Teaching/Level Drawing")]
    public sealed class SparkLevelDrawingAsset : ScriptableObject
    {
        [Tooltip("Exactly matches SparkLevelDefinition.LevelId, for example level_08.")]
        public string levelId = "level_08";
        [Tooltip("Optional human-friendly title displayed on the whiteboard.")]
        public string levelTitle = "Level Drawing";
        public int initialPage;
        public SparkLevelDrawingPage[] pages = Array.Empty<SparkLevelDrawingPage>();

        public int PageCount => pages == null ? 0 : pages.Length;

        public SparkVisualTeachingPlan BuildPlan(int index)
        {
            if (pages == null || index < 0 || index >= pages.Length || pages[index] == null)
                return null;
            SparkLevelDrawingPage page = pages[index];
            return new SparkVisualTeachingPlan
            {
                title = string.IsNullOrWhiteSpace(page.title) ? levelTitle : page.title,
                explanation = (page.explanation ?? "") + "\nILLUSTRATIVE AUTHOR DRAWING — NOT A LIVE CIRCUIT VERIFICATION.",
                annotations = Array.Empty<SparkVisualCommand>(),
                schematic = new SparkSchematicData
                {
                    caption = page.caption ?? "",
                    parts = page.parts ?? Array.Empty<SparkSchematicPart>(),
                    wires = page.wires ?? Array.Empty<SparkSchematicWire>()
                }
            };
        }
    }

    [Serializable]
    public sealed class SparkLevelDrawingPage
    {
        public string title = "Circuit Overview";
        [TextArea(3, 8)] public string explanation = "Illustrative intended wiring.";
        public string caption = "CIRCUIT DRAWING";
        public SparkSchematicPart[] parts = Array.Empty<SparkSchematicPart>();
        public SparkSchematicWire[] wires = Array.Empty<SparkSchematicWire>();
    }
}
