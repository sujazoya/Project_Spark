using System;
using System.Text.RegularExpressions;

namespace ProjectSpark.SparkV5.Pro
{
    /// <summary>Explicit allowlisted commands. Does not treat generic chat as an action.</summary>
    public static class SparkLabActionCommand
    {
        public enum Kind
        {
            None, Help, PreviewWire, ConfirmWire, CancelWire, UndoLastWire,
            PreviewPlan, RunApprovedPlan, StopPlan, ShowNextObjective, Status
        }

        public readonly struct Request
        {
            public readonly Kind Type;
            public readonly string From;
            public readonly string To;
            public Request(Kind type, string from = null, string to = null)
            { Type = type; From = from; To = to; }
        }

      
private static readonly Regex WireRequest = new Regex(
    @"^(?:spark\s+)?(?:preview|draw|create|connect|make)\s+" +
    @"(?:a\s+)?wire\s+from\s+" +
    @"<?(?<a>[A-Za-z0-9_.-]+)>?\s+to\s+" +
    @"<?(?<b>[A-Za-z0-9_.-]+)>?$",
    RegexOptions.IgnoreCase |
    RegexOptions.CultureInvariant |
    RegexOptions.Compiled);

        public static Request Parse(string utterance)
        {
            if (string.IsNullOrWhiteSpace(utterance)) return new Request(Kind.None);
            string q = Regex.Replace(utterance.Trim().ToLowerInvariant(), @"\s+", " ").TrimEnd('.', '?', '!');
            if (q.StartsWith("spark ", StringComparison.Ordinal)) q = q.Substring(6);
            Match match = WireRequest.Match(Regex.Replace(utterance.Trim(), @"\s+", " ").TrimEnd('.', '?', '!'));
            if (match.Success) return new Request(Kind.PreviewWire, match.Groups["a"].Value, match.Groups["b"].Value);
            switch (q)
            {
                case "agent help": case "laboratory actions": case "what can you do in the laboratory": return new Request(Kind.Help);
                case "confirm wire": case "execute wire": return new Request(Kind.ConfirmWire);
                case "cancel wire": case "discard wire": return new Request(Kind.CancelWire);
                case "undo last ai wire": case "undo last spark wire": return new Request(Kind.UndoLastWire);
                case "preview level solution": case "preview assisted plan": case "show level wiring plan": return new Request(Kind.PreviewPlan);
                case "auto solve practice level": case "complete practice level": case "run approved level plan": return new Request(Kind.RunApprovedPlan);
                case "stop auto solve": case "cancel auto solve": case "stop spark agent": return new Request(Kind.StopPlan);
                case "show next objective in 3d":
                case "demonstrate next objective automatically":
                case "guide me to next objective": return new Request(Kind.ShowNextObjective);
                case "agent status": case "auto solve status": return new Request(Kind.Status);
                default: return new Request(Kind.None);
            }
        }
    }
}
