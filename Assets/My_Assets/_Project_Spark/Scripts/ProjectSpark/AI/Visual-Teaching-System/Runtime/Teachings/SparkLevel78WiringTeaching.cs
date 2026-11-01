using System;
using UnityEngine;

namespace ProjectSpark.UI.VisualTeaching
{
    /// <summary>
    /// Hand-authored Level 7/8 teaching schematics. These describe the intended circuit,
    /// not the player's currently verified electrical graph. Uses the existing manager.
    /// Coordinates are normalized to the schematic content RectTransform.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class SparkLevel78WiringTeaching : MonoBehaviour
    {
        [SerializeField] private SparkVisualTeachingManager teachingManager;
        [SerializeField] private bool openWhiteboard = true;

        private static readonly Color Plus = new Color(.98f, .31f, .41f, 1f);
        private static readonly Color Minus = new Color(.23f, .70f, 1f, 1f);
        private static readonly Color Mid = new Color(1f, .78f, .23f, 1f);
        private static readonly Color Component = new Color(1f, .83f, .42f, 1f);
        private static readonly Color MeterColor = new Color(.27f, .96f, .86f, 1f);

        private void Awake() => Resolve();
        private void Resolve()
        {
            if (teachingManager == null)
                teachingManager = FindFirstObjectByType<SparkVisualTeachingManager>(FindObjectsInactive.Include);
        }

        [ContextMenu("Teaching / Level 7 - Voltage Divider")]
        public void ShowLevel7() => Show(BuildLevel7());

        [ContextMenu("Teaching / Level 8 - Parallel Divider")]
        public void ShowLevel8() => Show(BuildLevel8());

        private void Show(SparkVisualTeachingPlan plan)
        {
            Resolve();
            if (teachingManager == null)
            {
                Debug.LogWarning("[Spark Teaching] Visual teaching manager not found.", this);
                return;
            }
            if (!teachingManager.PresentValidatedPlan(plan)) return;
            if (openWhiteboard) teachingManager.OpenTeachingBoard();
        }

        private static Vector2 P(float x, float y) => new Vector2(x, y);
        private static SparkSchematicPart Part(string id, SparkSchematicKind kind,
            float x, float y, string label, Color color) =>
            new SparkSchematicPart { id = id, kind = kind, position = P(x, y), label = label, color = color };

        private static SparkSchematicWire Wire(Color color, params Vector2[] points) =>
            new SparkSchematicWire { color = color, points = points, energized = false };

        private static SparkVisualTeachingPlan BuildLevel7()
        {
            // Horizontal symbols, each connected by explicitly drawn orthogonal wires.
            // Physical topology: switch -> 220 -> VOUT -> 330 -> battery negative.
            // LCD is a separate high-impedance voltage sensing branch VOUT -> GND.
            // The compact labels are intentional: the stock renderer uses 175px label boxes.
            var parts = new[]
            {
                Part("battery", SparkSchematicKind.Battery, .11f, .51f, "5 V SUPPLY", Plus),
                Part("switch", SparkSchematicKind.Switch, .29f, .79f, "SWITCH", Component),
                Part("r1", SparkSchematicKind.Resistor, .46f, .79f, "R1 220 Ohm", Component),
                Part("vout", SparkSchematicKind.Generic, .585f, .79f, "", Mid),
                Part("r2", SparkSchematicKind.Resistor, .73f, .79f, "R2 330 Ohm", Component),
                Part("lcd", SparkSchematicKind.Meter, .65f, .36f, "LCD 3.00 V", MeterColor),
                Part("ground", SparkSchematicKind.Ground, .44f, .18f, "GND", Minus)
            };
            var wires = new[]
            {
                // Main current path, independent of meter.
                Wire(Plus, P(.11f,.65f), P(.11f,.79f), P(.23f,.79f)),
                Wire(Plus, P(.35f,.79f), P(.40f,.79f)),
                Wire(Plus, P(.52f,.79f), P(.585f,.79f)),
                Wire(Mid, P(.585f,.79f), P(.67f,.79f)),
                Wire(Minus, P(.79f,.79f), P(.89f,.79f), P(.89f,.18f), P(.44f,.18f)),
                Wire(Minus, P(.44f,.18f), P(.11f,.18f), P(.11f,.37f)),
                // High impedance LCD sense connections A=midpoint and B=GND.
                // Route A on left side of LCD to avoid crossing the body of R2.
                Wire(Mid, P(.585f,.79f), P(.585f,.54f), P(.59f,.54f), P(.59f,.36f)),
                Wire(Minus, P(.71f,.36f), P(.83f,.36f), P(.83f,.18f))
            };
            return new SparkVisualTeachingPlan
            {
                title = "LEVEL 7 | Voltage Divider | Correct Wiring",
                explanation = "ILLUSTRATIVE / NOT LIVE VERIFIED. Main path: Battery + to Switch, SwitchOutput to 220 INPUT, 220 OUTPUT to 330 INPUT (VOUT), 330 OUTPUT to Battery_negative. LCD A to VOUT, LCD B to Battery_negative. Target: VOUT 3.00 V, divider current 9.09 mA.",
                annotations = Array.Empty<SparkVisualCommand>(),
                schematic = new SparkSchematicData
                {
                    caption = "LEVEL 7 | SERIES 220 + 330 Ohm | VOUT 3.00 V | I 9.09 mA | LCD A=VOUT B=GND",
                    parts = parts, wires = wires
                }
            };
        }

        private static SparkVisualTeachingPlan BuildLevel8()
        {
            // Horizontal parallel branches, common +5V bus (left) and GND bus (right).
            // Both branches have their own true resistor-to-ground return; LCD is not a return.
            var parts = new[]
            {
                Part("battery", SparkSchematicKind.Battery, .11f, .50f, "5 V SUPPLY", Plus),
                Part("switch", SparkSchematicKind.Switch, .29f, .81f, "SWITCH", Component),
                Part("posbus", SparkSchematicKind.Generic, .40f, .81f, "", Plus),
                Part("r1", SparkSchematicKind.Resistor, .57f, .68f, "R1 330 Ohm", Component),
                Part("r2", SparkSchematicKind.Resistor, .57f, .42f, "R2 470 Ohm", Component),
                Part("lcd", SparkSchematicKind.Meter, .86f, .51f, "LCD 5.00 V", MeterColor),
                Part("ground", SparkSchematicKind.Ground, .69f, .17f, "GND", Minus)
            };
            var wires = new[]
            {
                Wire(Plus, P(.11f,.64f), P(.11f,.81f), P(.23f,.81f)),
                Wire(Plus, P(.35f,.81f), P(.40f,.81f), P(.40f,.42f)),
                Wire(Plus, P(.40f,.68f), P(.51f,.68f)),
                Wire(Plus, P(.40f,.42f), P(.51f,.42f)),
                Wire(Minus, P(.63f,.68f), P(.72f,.68f)),
                Wire(Minus, P(.63f,.42f), P(.72f,.42f)),
                Wire(Minus, P(.72f,.68f), P(.72f,.17f), P(.69f,.17f)),
                Wire(Minus, P(.69f,.17f), P(.11f,.17f), P(.11f,.36f)),
                // Separate sense branch; do not series-connect LCD through either resistor.
                Wire(Plus, P(.40f,.81f), P(.86f,.81f), P(.86f,.63f)),
                Wire(Minus, P(.86f,.39f), P(.86f,.17f), P(.72f,.17f))
            };
            return new SparkVisualTeachingPlan
            {
                title = "LEVEL 8 | Parallel Current Divider | Correct Wiring",
                explanation = "ILLUSTRATIVE / NOT LIVE VERIFIED. SwitchOutput connects to BOTH resistor INPUTs. BOTH resistor OUTPUTs connect to Battery_negative. LCD A connects to SwitchOutput and B to Battery_negative. 330 Ohm branch: 15.15 mA; 470 Ohm branch: 10.64 mA; total source current: 25.79 mA.",
                annotations = Array.Empty<SparkVisualCommand>(),
                schematic = new SparkSchematicData
                {
                    caption = "LEVEL 8 | PARALLEL 330 || 470 Ohm | 5.00 V EACH | TOTAL 25.79 mA | LCD A=+5 B=GND",
                    parts = parts, wires = wires
                }
            };
        }
    }
}
