using System;
using System.Text.RegularExpressions;

namespace ProjectSpark.SparkV5.Pro
{
    /// <summary>
    /// Small, curated factual explanations for foundational terminology.
    /// Not an electrical measurement service, AI provider, assessment or lesson controller.
    /// This intentionally does not use live solver values or modify gameplay state.
    /// </summary>
    internal static class SparkProCoreConcepts
    {
        internal static bool TryExplain(string normalizedQuestion, out string explanation)
        {
            explanation = null;
            if (string.IsNullOrWhiteSpace(normalizedQuestion)) return false;
            string q = normalizedQuestion.Trim();
            if (!Regex.IsMatch(q, @"^(what (is|are|does)|define|explain|tell me about)\b")) return false;

            if (Has(q, "anode"))
            {
                explanation = "An anode is a device terminal defined by its electrochemical or electronic role. " +
                    "In a normally operating forward-biased LED, conventional current enters the anode and exits the cathode. " +
                    "Check the particular LED's package marking or datasheet before wiring it; a lead's length is not a universal identifier.";
                return true;
            }
            if (Has(q, "cathode"))
            {
                explanation = "A cathode is a device terminal defined by its electrochemical or electronic role. " +
                    "In a normally operating forward-biased LED, conventional current leaves through the cathode. " +
                    "Check the actual LED's package marking or datasheet before connecting it.";
                return true;
            }
            if (Has(q, "forward bias") || Has(q, "forward biased"))
            {
                explanation = "An ordinary LED is forward biased when its anode is at a sufficiently higher electric potential than its cathode " +
                    "for current to flow in the intended direction. Use a suitable current-limiting circuit; forward bias does not by itself ensure a safe current.";
                return true;
            }
            if (Has(q, "reverse bias") || Has(q, "reverse biased"))
            {
                explanation = "An ordinary LED is reverse biased when its cathode is at a higher potential than its anode. " +
                    "It normally does not emit light under that condition, but excessive reverse voltage can damage it.";
                return true;
            }
            if (Has(q, "current limiting resistor") || Has(q, "current limiter"))
            {
                explanation = "A current-limiting resistor helps keep LED current within the chosen design range. " +
                    "For a simple series LED circuit, estimate the resistance from the supply voltage, expected forward drop and target current, " +
                    "then check resistor power and the actual circuit conditions.";
                return true;
            }
            if (Has(q, "closed circuit") || Has(q, "closed path") || Has(q, "return path"))
            {
                explanation = "A closed current path forms a continuous conductive loop between the supply terminals through the load. " +
                    "An open switch or broken return wire interrupts that loop. The actual current must still be determined from the circuit conditions.";
                return true;
            }
            if (Has(q, "led") || Has(q, "light emitting diode"))
            {
                explanation = "An LED, or light-emitting diode, is a semiconductor that can emit light when properly forward biased and supplied with suitable current. " +
                    "Its anode and cathode identify its normal current direction. Its required voltage and safe current depend on the specific device.";
                return true;
            }
            return false;
        }

        private static bool Has(string q, string expression) =>
            Regex.IsMatch(q, @"(?<![\p{L}\d])" + Regex.Escape(expression) + @"(?![\p{L}\d])", RegexOptions.IgnoreCase);
    }
}
