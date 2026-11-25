using System;
using System.Text.RegularExpressions;

namespace ProjectSpark.SparkV5.Pro
{
    /// <summary>Deterministic command recognition. Does not grant permissions or infer scene targets.</summary>
    public static class SparkLabFocusIntent
    {
        public enum Kind { None, Selected, NextConnection, PositiveTerminal, NegativeTerminal, Clear }

        public static Kind Classify(string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return Kind.None;
            string q = Regex.Replace(text.ToLowerInvariant().Trim(), @"\s+", " ");
            if (Regex.IsMatch(q, @"\b(clear|hide|close|remove|stop)\b.*\b(highlight|marker|beacon|visual|pointing)\b"))
                return Kind.Clear;
            bool visual = Regex.IsMatch(q, @"\b(show|highlight|point|indicate|locate|find|inspect|look|guide|where)\b");
            if (!visual) return Kind.None;
            if (Regex.IsMatch(q, @"\b(next|incomplete|required|missing|which|what)\b.*\b(connection|wire|terminal|pin|endpoint)\b") ||
                Regex.IsMatch(q, @"\b(connection|wire|terminal|pin|endpoint)\b.*\b(next|inspect|check|missing|required|should)\b"))
                return Kind.NextConnection;
            if (Regex.IsMatch(q, @"\b(anode|positive terminal|positive pin)\b"))
                return Kind.PositiveTerminal;
            if (Regex.IsMatch(q, @"\b(cathode|negative terminal|negative pin|return terminal)\b"))
                return Kind.NegativeTerminal;
            if (Regex.IsMatch(q, @"\b(selected|this component|this terminal|this pin|point at this|point to this|show me this)\b"))
                return Kind.Selected;
            return Kind.None;
        }

        public static bool MayDraw(bool restricted, bool warning, bool hasLayout, bool hasFreshEvaluation, Kind kind)
        {
            if (kind == Kind.Clear) return true;
            return !restricted && !warning && hasLayout && hasFreshEvaluation && kind != Kind.None;
        }
    }
}
