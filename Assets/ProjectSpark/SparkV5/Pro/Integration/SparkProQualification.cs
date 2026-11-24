using System;
using ProjectSpark.AI.Tutorial;
using UnityEngine;

namespace ProjectSpark.SparkV5.Pro
{
    internal static class SparkProQualification
    {
        [Serializable] internal sealed class Source
        {
            public int level;
            public bool certification_enabled, schematic_approved, mechanical_model_approved;
            public string[] objective_certification;
        }

        internal static bool CanQualify(Source source, bool matchingLevel, int verifiedObjectives,
            bool theory, bool practical, bool diagnosis, bool safety) =>
            source?.level == 120 && source.certification_enabled && source.schematic_approved &&
            source.mechanical_model_approved && source.objective_certification?.Length == 6 &&
            Array.TrueForAll(source.objective_certification, status => status == "APPROVED") && matchingLevel &&
            verifiedObjectives == 6 && theory && practical && diagnosis && safety;

        internal static string Status(SparkProGameCatalog catalog, SparkAILevelContext context)
        {
            if (!catalog || !catalog.motherboardQualificationSource)
                return "Level 120 is the motherboard qualification mission. Its approval record could not be found.";
            Source source;
            try { source = JsonUtility.FromJson<Source>(catalog.motherboardQualificationSource.text); }
            catch (ArgumentException) { return "The authored Level 120 approval record is unreadable."; }
            string mission = context.Definition && context.Definition.LevelNumber == 120 && context.HasEvaluation ?
                $"Current Axis Forge mission: {context.Manager.SatisfiedTargetCount}/6 objectives. " : "";
            if (source == null || !source.certification_enabled || !source.schematic_approved || !source.mechanical_model_approved)
                return mission + "Motherboard Master certification is disabled by the Level 120 source. It requires an approved RAX-28 schematic and six-axis mechanical model.";
            return mission + "The engineering source is approved. A dedicated authored theory assessment and mandatory practice, diagnostics and safety evidence must also be bound before issuing a certificate.";
        }
    }
}
