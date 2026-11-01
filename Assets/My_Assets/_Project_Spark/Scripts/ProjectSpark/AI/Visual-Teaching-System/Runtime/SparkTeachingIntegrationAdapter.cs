using System;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectSpark.UI.VisualTeaching
{
    // Thin, safe adapter for Project Spark's existing tutorial, diagnosis and circuit systems.
    // Uses only known IDs and verified statements supplied by those systems. Never guesses
    // which terminals are connected, whether current flows, or what the voltage actually is.
    [DisallowMultipleComponent]
    public sealed class SparkTeachingIntegrationAdapter : MonoBehaviour
    {
        [SerializeField] private SparkVisualTeachingManager teachingManager;
        [SerializeField] private bool warnOnMissingAnchor = true;

        private void Awake()
        {
            if (teachingManager == null)
                teachingManager = FindFirstObjectByType<SparkVisualTeachingManager>(FindObjectsInactive.Include);
        }

        public bool ExplainComponent(string anchorId, string explanation)
        {
            if (!ValidAnchors(anchorId)) return false;
            return Present("COMPONENT EXPLANATION", explanation,
                new [] {
                    Ring(anchorId, new Color(.12f,.84f,1f)),
                    Label(anchorId, explanation, new Vector2(0, 85))
                });
        }

        public bool HighlightVerifiedFault(string anchorId, string verifiedDiagnosis)
        {
            if (!ValidAnchors(anchorId)) return false;
            return Present("VERIFIED CIRCUIT FAULT", verifiedDiagnosis,
                new [] {
                    Ring(anchorId, new Color(1f,.3f,.27f)),
                    Label(anchorId, verifiedDiagnosis, new Vector2(0, 90))
                });
        }

        public bool GuideConnection(string sourceAnchorId, string targetAnchorId, string instruction)
        {
            if (!ValidAnchors(sourceAnchorId, targetAnchorId)) return false;
            return Present("CONNECTION GUIDANCE", instruction,
                new [] {
                    Ring(sourceAnchorId, new Color(1f,.83f,.28f)),
                    Ring(targetAnchorId, new Color(.22f,1f,.56f)),
                    new SparkVisualCommand {
                        shape=SparkVisualShape.Arrow,
                        from=Anchor(sourceAnchorId),to=Anchor(targetAnchorId),
                        color=new Color(1f,.83f,.28f), thickness=6f,
                        drawSeconds=.55f, persistent=true
                    },
                    Label(targetAnchorId, instruction, new Vector2(0,90))
                });
        }

        // IDs must be provided in the actual verified current route by your circuit solver.
        public bool DrawVerifiedCurrentRoute(string[] orderedAnchorIds, string verifiedExplanation)
        {
            if (orderedAnchorIds == null || orderedAnchorIds.Length < 2 ||
                orderedAnchorIds.Length > 24 || !ValidAnchors(orderedAnchorIds)) return false;
            List<SparkVisualCommand> commands = new List<SparkVisualCommand>();
            for (int i = 0; i + 1 < orderedAnchorIds.Length; i++)
                commands.Add(new SparkVisualCommand {
                    shape=SparkVisualShape.Arrow,
                    from=Anchor(orderedAnchorIds[i]),to=Anchor(orderedAnchorIds[i+1]),
                    color=new Color(1f,.82f,.25f), thickness=5f, drawSeconds=.3f,
                    persistent=true
                });
            return Present("VERIFIED CURRENT ROUTE", verifiedExplanation, commands.ToArray());
        }

        public bool ShowVerifiedCircuitSnapshot(SparkSchematicData schematic, string explanation)
        {
            if (teachingManager == null) return false;
            var plan = new SparkVisualTeachingPlan {
                title="CIRCUIT SNAPSHOT", explanation=explanation, schematic=schematic,
                annotations=Array.Empty<SparkVisualCommand>()
            };
            return teachingManager.PresentValidatedPlan(plan);
        }

        public bool SignalVerifiedTutorialAction(string actionId)
        {
            return teachingManager != null && teachingManager.ReportVerifiedAction(actionId);
        }

        public void ShowDemo() { if (teachingManager != null) teachingManager.PlayDemoLesson(); }

        private bool Present(string title, string explanation, SparkVisualCommand[] commands)
        {
            if (teachingManager == null) return false;
            return teachingManager.PresentValidatedPlan(new SparkVisualTeachingPlan {
                title=title, explanation=explanation,
                annotations=commands, schematic=new SparkSchematicData { caption="World-anchored explanation" }
            });
        }

        private bool ValidAnchors(params string[] anchorIds)
        {
            if (anchorIds == null) return false;
            foreach (string id in anchorIds)
                if (!SparkTeachingAnchor.TryFind(id, out _))
                {
                    if (warnOnMissingAnchor) Debug.LogWarning("[Spark Teaching] Missing active anchor: " + id, this);
                    return false;
                }
            return true;
        }

        private static SparkVisualPoint Anchor(string id) =>
            new SparkVisualPoint { space=SparkPointSpace.WorldAnchor, anchorId=id };

        private static SparkVisualCommand Ring(string id, Color color) => new SparkVisualCommand {
            shape=SparkVisualShape.Ring, from=Anchor(id), color=color,
            normalizedSize=new Vector2(.06f,.11f), thickness=5f, drawSeconds=.35f
        };

        private static SparkVisualCommand Label(string id, string text, Vector2 offset) =>
            new SparkVisualCommand {
                shape=SparkVisualShape.Label,
                from=new SparkVisualPoint {
                    space=SparkPointSpace.WorldAnchor,
                    anchorId=id, pixelOffset=offset
                },
                text=text, color=Color.white, fontSize=21, drawSeconds=.3f
            };
    }
}
