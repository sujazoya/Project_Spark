using System;
using System.Text.RegularExpressions;

namespace ProjectSpark.SparkV5.Pro
{
    /// <summary>Explicit demonstration requests only; ordinary teacher answers stay in Cortex.</summary>
    public static class SparkLabDemoIntent
    {
        public enum Kind { None, StartDiode, StartSwitch, StartMeter, Next, Repeat, Status, Stop }

        public static Kind Classify(string input)
        {
            if (string.IsNullOrWhiteSpace(input)) return Kind.None;
            string text = Regex.Replace(input.Trim().ToLowerInvariant(), @"\s+", " ");
            text = text.TrimEnd('.', '?', '!');
            if (Regex.IsMatch(text, @"^(stop|end|cancel|close) (the )?(laboratory |lab )?(demonstration|demo)$")) return Kind.Stop;
            if (Regex.IsMatch(text, @"^(next|continue|advance|proceed)( (the|my))? (laboratory |lab )?(demonstration|demo)( step)?$")) return Kind.Next;
            if (Regex.IsMatch(text, @"^(repeat|retry|redo|show again)( (the|this|last))? (laboratory |lab )?(demonstration|demo)( step)?$")) return Kind.Repeat;
            if (Regex.IsMatch(text, @"^(demonstration|demo) (status|progress)$|^where are we in the (demonstration|demo)$")) return Kind.Status;

            // Do not intercept generic 'teach me level 1', lessons, diagnostics or electrical answers.
            bool explicitDemo = Regex.IsMatch(text, @"\b(demonstrat(e|ion)|demo)\b") ||
                                text.StartsWith("show me how to demonstrate ", StringComparison.Ordinal);
            bool start = Regex.IsMatch(text, @"^(start|begin|run|show|teach|demonstrate|launch)\b");
            if (!explicitDemo || !start) return Kind.None;
            if (Regex.IsMatch(text, @"\b(diode|led|anode|cathode|polarity)\b")) return Kind.StartDiode;
            if (Regex.IsMatch(text, @"\b(switch|open circuit|closed circuit|return path|continuity)\b")) return Kind.StartSwitch;
            if (Regex.IsMatch(text, @"\b(multimeter|meter|voltage measurement|voltage reading|measure voltage|probe|measurement)\b")) return Kind.StartMeter;
            return Kind.None;
        }
    }
}
