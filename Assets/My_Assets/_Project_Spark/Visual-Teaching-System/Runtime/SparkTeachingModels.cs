using System;
using UnityEngine;

namespace ProjectSpark.UI.VisualTeaching
{
    public enum SparkVisualShape { Line, Arrow, Ring, Rectangle, Label, Wait, Clear }
    public enum SparkPointSpace { NormalizedScreen, WorldAnchor }
    public enum SparkSchematicKind { Battery, Resistor, LED, Diode, Capacitor, Switch, Ground, Motor, Meter, Generic }
    public enum SparkTeachingView { Both, WhiteboardOnly, WorldOnly }

    [Serializable]
    public class SparkVisualPoint
    {
        public SparkPointSpace space = SparkPointSpace.NormalizedScreen;
        public Vector2 normalized = new Vector2(.5f, .5f);
        public string anchorId = "";
        public Vector2 pixelOffset = Vector2.zero;
    }

    [Serializable]
    public class SparkVisualCommand
    {
        public SparkVisualShape shape = SparkVisualShape.Arrow;
        public SparkVisualPoint from = new SparkVisualPoint();
        public SparkVisualPoint to = new SparkVisualPoint { normalized = new Vector2(.7f, .5f) };
        public Vector2 normalizedSize = new Vector2(.12f, .1f);
        public Color color = new Color(.12f, .82f, 1f, 1f);
        [Min(.5f)] public float thickness = 4f;
        [Min(0f)] public float drawSeconds = .35f;
        [Min(0f)] public float holdSeconds = .1f;
        public string text = "";
        [Range(12, 44)] public int fontSize = 22;
        public bool persistent = true;
    }

    [Serializable]
    public class SparkSchematicPart
    {
        public string id;
        public SparkSchematicKind kind;
        public Vector2 position = new Vector2(.5f, .5f);
        public string label;
        public Color color = new Color(.25f, .85f, 1f, 1f);
    }

    // Explicit diagram routes are supplied by level-authoring or verified simulator data.
    // They are NOT inferred from the AI response or the physical scene hierarchy.
    [Serializable]
    public class SparkSchematicWire
    {
        public Vector2[] points = Array.Empty<Vector2>();
        public bool energized;
        public Color color = new Color(.45f, .77f, .97f, 1f);
    }

    [Serializable]
    public class SparkSchematicData
    {
        public string caption = "Circuit schematic";
        public SparkSchematicPart[] parts = Array.Empty<SparkSchematicPart>();
        public SparkSchematicWire[] wires = Array.Empty<SparkSchematicWire>();
    }

    [Serializable]
    public class SparkTeachingStep
    {
        public string title = "Step";
        [TextArea(2, 5)] public string instruction;
        [TextArea(2, 5)] public string hint;
        // Requires a VERIFIED gameplay event from a separate trusted adapter.
        public string requiredVerifiedActionId = "";
        public bool advanceWhenVerified = true;
        public SparkVisualCommand[] annotations = Array.Empty<SparkVisualCommand>();
        public SparkSchematicData schematic = new SparkSchematicData();
    }

    [CreateAssetMenu(menuName = "Project Spark/AI/Visual Teaching Lesson", fileName = "SparkVisualLesson")]
    public sealed class SparkVisualLesson : ScriptableObject
    {
        public string lessonId = "tutorial";
        public string title = "Circuit Tutorial";
        public string goal = "Learn the circuit";
        public SparkTeachingStep[] steps = Array.Empty<SparkTeachingStep>();
    }

    // Safe, data-only input boundary for an AI adapter. No code, prefab, file, URL,
    // shader, script type or arbitrary Unity method can be supplied by the AI.
    [Serializable]
    public class SparkVisualTeachingPlan
    {
        public string title = "AI explanation";
        public string explanation = "";
        public SparkVisualCommand[] annotations = Array.Empty<SparkVisualCommand>();
        public SparkSchematicData schematic = new SparkSchematicData();
    }
}
