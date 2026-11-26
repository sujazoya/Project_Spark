using System;
using System.Globalization;
using ProjectSpark.Gameplay;

namespace ProjectSpark.SparkV5.Pro
{
    /// <summary>Optional Spark Live route. Integrated only when a safe existing command branch is found.</summary>
    public static class SparkTelescopicArmCommands
    {
        public static bool TryHandleFrom(SparkProExecutive source, string prompt, out string answer)
        {
            answer = null;
            if (!source || string.IsNullOrWhiteSpace(prompt)) return false;
            string text = prompt.Trim();
            string lower = text.ToLowerInvariant();
            bool status = lower == "spark arm status" || lower == "spark arms status";
            bool retract = lower == "spark retract arms" || lower == "spark retract arm";
            const string rightPrefix = "spark extend right arm to ";
            const string leftPrefix = "spark extend left arm to ";
            const string genericPrefix = "spark reach terminal ";
            const string reachPrefix = "spark set arm reach ";
            bool right = lower.StartsWith(rightPrefix, StringComparison.Ordinal);
            bool left = lower.StartsWith(leftPrefix, StringComparison.Ordinal);
            bool generic = lower.StartsWith(genericPrefix, StringComparison.Ordinal);
            bool setReach = lower.StartsWith(reachPrefix, StringComparison.Ordinal);
            if (!status && !retract && !right && !left && !generic && !setReach) return false;
            var arms = source.GetComponent<SparkTelescopicArms>();
            if (!arms) { answer = "Telescopic arms are not bound. Use AAA Laboratory > 38 - Bind Telescopic Arms v18."; return true; }
            if (status) { answer = arms.Status; return true; }
            if (retract) { arms.RetractAll(); answer = "Spark is retracting both telescopic arms."; return true; }
            if (setReach)
            {
                string raw = text.Substring(reachPrefix.Length).Trim();
                if (!float.TryParse(raw, NumberStyles.Float, CultureInfo.InvariantCulture, out float metres))
                { answer = "Use a numeric reach, e.g. spark set arm reach 5"; return true; }
                answer = arms.SetMaximumReach(metres) ? arms.Status : arms.LastError;
                return true;
            }
            string id = text.Substring(right ? rightPrefix.Length : left ? leftPrefix.Length : genericPrefix.Length).Trim();
            var side = left ? SparkTelescopicArms.ManipulatorSide.Left : SparkTelescopicArms.ManipulatorSide.Right;
            bool ok = arms.TryReachTerminal(id, side);
            answer = ok ? "Extending " + (left ? "left" : "right") + " arm toward " + id +
                ". Native wiring remains controlled by the player Wire Tool." :
                "Telescopic reach denied: " + arms.LastError;
            return true;
        }
    }
}
