using System;
using UnityEngine;

namespace ProjectSpark.UI.VisualTeaching
{
    public static class SparkTeachingValidator
    {
        private const int MaxCommands = 64;
        private const int MaxParts = 48;
        private const int MaxWires = 96;
        private const int MaxWirePoints = 64;

        public static bool Validate(SparkVisualTeachingPlan plan, out string reason)
        {
            if (plan == null) { reason = "Missing plan"; return false; }
            if (!TextOk(plan.title, 100) || !TextOk(plan.explanation, 500))
            { reason = "Explanation exceeds text limit"; return false; }
            if (plan.annotations != null && plan.annotations.Length > MaxCommands)
            { reason = "Too many annotation commands"; return false; }
            if (plan.annotations != null)
                foreach (var command in plan.annotations)
                {
                    if (!Validate(command, out reason)) return false;
                }
            if (!Validate(plan.schematic, out reason)) return false;
            reason = "OK";
            return true;
        }

        public static bool Validate(SparkVisualCommand c, out string reason)
        {
            reason = "OK";
            if (c == null) { reason = "Null command"; return false; }
            if (!Enum.IsDefined(typeof(SparkVisualShape), c.shape))
            { reason = "Unknown command shape"; return false; }
            if (!TextOk(c.text, 320) || !Valid(c.thickness) || c.thickness < .5f || c.thickness > 24f ||
                !Valid(c.drawSeconds) || c.drawSeconds < 0 || c.drawSeconds > 12f ||
                !Valid(c.holdSeconds) || c.holdSeconds < 0 || c.holdSeconds > 20f ||
                c.fontSize < 12 || c.fontSize > 44 || !ValidColor(c.color) ||
                !Valid(c.normalizedSize) || c.normalizedSize.x < 0 || c.normalizedSize.y < 0 ||
                c.normalizedSize.x > 1 || c.normalizedSize.y > 1)
            { reason = "Invalid command values"; return false; }
            if (!Validate(c.from) || !Validate(c.to))
            { reason = "Invalid point or unregistered anchor identifier"; return false; }
            return true;
        }

        private static bool Validate(SparkVisualPoint p)
        {
            if (p == null || !Enum.IsDefined(typeof(SparkPointSpace), p.space) ||
                !Valid(p.normalized) || !Valid(p.pixelOffset)) return false;
            if (Mathf.Abs(p.pixelOffset.x) > 500 || Mathf.Abs(p.pixelOffset.y) > 500) return false;
            if (p.space == SparkPointSpace.WorldAnchor)
                return !string.IsNullOrWhiteSpace(p.anchorId) && p.anchorId.Length <= 80;
            return p.normalized.x >= 0 && p.normalized.x <= 1 &&
                   p.normalized.y >= 0 && p.normalized.y <= 1;
        }

        public static bool Validate(SparkSchematicData data, out string reason)
        {
            reason = "OK";
            if (data == null) return true;
            if (!TextOk(data.caption, 150) ||
                (data.parts != null && data.parts.Length > MaxParts) ||
                (data.wires != null && data.wires.Length > MaxWires))
            { reason = "Schematic too large"; return false; }
            if (data.parts != null)
                foreach (var p in data.parts)
                {
                    if (p == null || !TextOk(p.id, 80) || !TextOk(p.label, 80) ||
                        !Enum.IsDefined(typeof(SparkSchematicKind), p.kind) ||
                        !InUnit(p.position) || !ValidColor(p.color))
                    { reason = "Invalid schematic component"; return false; }
                }
            if (data.wires != null)
                foreach (var w in data.wires)
                {
                    if (w == null || w.points == null || w.points.Length < 2 ||
                        w.points.Length > MaxWirePoints || !ValidColor(w.color))
                    { reason = "Invalid schematic wire"; return false; }
                    foreach (Vector2 point in w.points)
                        if (!InUnit(point))
                        { reason = "Schematic wire point outside bounds"; return false; }
                }
            return true;
        }

        private static bool TextOk(string text, int max) => text == null || text.Length <= max;
        private static bool Valid(float v) => !float.IsNaN(v) && !float.IsInfinity(v);
        private static bool Valid(Vector2 v) => Valid(v.x) && Valid(v.y);
        private static bool InUnit(Vector2 v) => Valid(v) && v.x >= 0 && v.x <= 1 && v.y >= 0 && v.y <= 1;
        private static bool ValidColor(Color c) => Valid(c.r) && Valid(c.g) && Valid(c.b) && Valid(c.a);
    }
}
