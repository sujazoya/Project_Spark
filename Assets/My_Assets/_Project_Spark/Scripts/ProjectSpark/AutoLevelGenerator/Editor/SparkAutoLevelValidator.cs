#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectSpark.AutoLevelGenerator.Editor
{
    public static class SparkAutoLevelValidator
    {
        public static void Validate(SparkAutoLevelRecipe recipe, SparkAutoLevelBlueprint blueprint, SparkAutoLevelBuildReport report)
        {
            if (blueprint == null) { report.Error("Blueprint null"); return; }
            if (recipe.expectedRoutes > 0 && blueprint.Routes.Count != recipe.expectedRoutes)
                report.Error("Route count mismatch: " + blueprint.Routes.Count + " vs " + recipe.expectedRoutes);
            if (recipe.expectedVias > 0 && blueprint.Vias.Count != recipe.expectedVias)
                report.Error("Via count mismatch: " + blueprint.Vias.Count + " vs " + recipe.expectedVias);
            if (recipe.expectedObjectives > 0 && blueprint.Objectives.Count != recipe.expectedObjectives)
                report.Error("Objective count mismatch: " + blueprint.Objectives.Count + " vs " + recipe.expectedObjectives);

            int approvedRoutes = 0, approvedVias = 0;
            for (int i = 0; i < blueprint.Routes.Count; i++)
                if (blueprint.Routes[i].HasSchematicFields(blueprint.CopperLayers)) approvedRoutes++;
            for (int i = 0; i < blueprint.Vias.Count; i++)
                if (blueprint.Vias[i].HasSchematicFields(blueprint.CopperLayers)) approvedVias++;
            if (approvedRoutes != blueprint.Routes.Count)
                report.Warn("SCHEMATIC_REQUIRED: " + (blueprint.Routes.Count - approvedRoutes) + " routes lack approved pins/nets/layers; electrical objective blocked");
            if (approvedVias != blueprint.Vias.Count)
                report.Warn("SCHEMATIC_REQUIRED: " + (blueprint.Vias.Count - approvedVias) + " vias lack approved nets/layer spans; electrical objective blocked");
            if (blueprint.Safety == null || !blueprint.Safety.HasApprovedPhysicalPins)
                report.Warn("SCHEMATIC_REQUIRED: independent safety jumper pins not approved; safety objective blocked");

            int duplicateObjectives = 0;
            var ids = new HashSet<string>(StringComparer.Ordinal);
            for (int i = 0; i < blueprint.Objectives.Count; i++)
            {
                string id = blueprint.Objectives[i].Id;
                if (string.IsNullOrWhiteSpace(id) || !ids.Add(id)) duplicateObjectives++;
            }
            if (duplicateObjectives > 0) report.Error("Missing/duplicate objective IDs: " + duplicateObjectives);
            ids.Clear();
            for (int i = 0; i < blueprint.Faults.Count; i++)
            {
                string id = blueprint.Faults[i].Id;
                if (string.IsNullOrWhiteSpace(id) || !ids.Add(id)) report.Error("Missing/duplicate fault ID: " + id);
            }
            report.Info("Blueprint: " + blueprint.LevelId + " / " + blueprint.BoardId + ", installable " + blueprint.InstallableCount +
                        ", models " + blueprint.Parts.Count + ", routes " + blueprint.Routes.Count + ", vias " + blueprint.Vias.Count);
            report.Info("Approvals: routes " + approvedRoutes + "/" + blueprint.Routes.Count +
                        ", vias " + approvedVias + "/" + blueprint.Vias.Count);
            report.Info("The generator does not certify connectivity, provide a solver, repair real components, or award objectives by itself");
        }
    }
}
#endif
