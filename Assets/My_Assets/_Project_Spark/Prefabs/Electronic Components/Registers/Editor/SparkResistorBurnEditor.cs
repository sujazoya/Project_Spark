#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.VFX;
using ProjectSpark.Gameplay;

namespace ProjectSpark.Editor
{
    [CustomEditor(typeof(SparkResistorBurn))]
    [CanEditMultipleObjects]
    public sealed class SparkResistorBurnEditor : UnityEditor.Editor
    {
        private static readonly string[] RendererPaths =
        {
            "Resistor/Body",
            "Resistor/Strip1",
            "Resistor/Strip2",
            "Resistor/Strip3",
            "Resistor/Strip4"
        };

        private string lastReport;

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.Space(5);
            EditorGUILayout.LabelField("ProjectSpark • Burn References", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "Fetches five resistor renderers, smoke/flame VFX, Sparks/Debris/Flash particles, and Burn_Light from the specified child paths. Other values are left untouched.",
                MessageType.Info);

            if (GUILayout.Button("FETCH PARTICLES + VFX + HEAT RENDERERS", GUILayout.Height(34)))
            {
                // Preserve any pending changes in the Inspector before modifying objects.
                serializedObject.ApplyModifiedProperties();
                int assigned = 0;
                var report = new List<string>();
                foreach (Object obj in targets)
                {
                    SparkResistorBurn burn = obj as SparkResistorBurn;
                    if (burn == null) continue;
                    assigned += Fetch(burn, report);
                }
                lastReport = "Assigned " + assigned + " fields across " + targets.Length + " object(s).\n" + string.Join("\n", report);
                serializedObject.Update();
            }

            if (!string.IsNullOrEmpty(lastReport))
                EditorGUILayout.HelpBox(lastReport, lastReport.Contains("MISSING") ? MessageType.Warning : MessageType.Info);

            EditorGUILayout.Space(5);
            DrawDefaultInspector();
        }

        private static int Fetch(SparkResistorBurn burn, List<string> report)
        {
            Transform root = burn.transform;
            var so = new SerializedObject(burn);
            so.Update();
            Undo.RecordObject(burn, "Fetch Resistor Burn References");
            int assigned = 0;
            var missing = new List<string>();

            SerializedProperty renderers = so.FindProperty("heatedRenderers");
            if (renderers != null)
            {
                renderers.arraySize = RendererPaths.Length;
                for (int i = 0; i < RendererPaths.Length; ++i)
                {
                    Transform child = root.Find(RendererPaths[i]);
                    Renderer renderer = child != null ? child.GetComponent<Renderer>() : null;
                    renderers.GetArrayElementAtIndex(i).objectReferenceValue = renderer;
                    if (renderer != null) assigned++;
                    else missing.Add(RendererPaths[i] + " (Renderer)");
                }
            }

            assigned += Bind<VisualEffect>(so, root, "smokeVfx", "_BurnVFX/VFX_ResistorSmoke", missing);
            assigned += Bind<VisualEffect>(so, root, "flameVfx", "_BurnVFX/VFX_ResistorFlame", missing);
            assigned += Bind<VisualEffect>(so, root, "failureVfx", "_BurnVFX/VFX_ResistorFlame", missing);

            assigned += Bind<ParticleSystem>(so, root, "sparkParticles", "_BurnVFX/Sparks", missing);
            assigned += Bind<ParticleSystem>(so, root, "debrisParticles", "_BurnVFX/Debris", missing);
            assigned += Bind<ParticleSystem>(so, root, "flashParticles", "_BurnVFX/Flash", missing);

            assigned += Bind<Light>(so, root, "heatLight", "_BurnVFX/Burn_Light", missing);
            assigned += Bind<Light>(so, root, "failureFlashLight", "_BurnVFX/Burn_Light", missing);

            // No Ember/Smoke ParticleSystem locations were specified, so leave those untouched.
            bool changed = so.ApplyModifiedProperties();
            if (changed)
            {
                EditorUtility.SetDirty(burn);
                if (PrefabUtility.IsPartOfPrefabInstance(burn))
                    PrefabUtility.RecordPrefabInstancePropertyModifications(burn);
            }

            report.Add("[" + burn.name + "] " + (missing.Count == 0
                ? "All requested references found."
                : "MISSING: " + string.Join(", ", missing)));
            return assigned;
        }

        private static int Bind<T>(SerializedObject so, Transform root, string propertyName,
            string path, List<string> missing) where T : Component
        {
            SerializedProperty property = so.FindProperty(propertyName);
            if (property == null)
            {
                missing.Add("serialized field '" + propertyName + "'");
                return 0;
            }

            Transform child = root.Find(path);
            T component = child != null ? child.GetComponent<T>() : null;
            property.objectReferenceValue = component;
            if (component == null)
            {
                missing.Add(path + " (" + typeof(T).Name + ")");
                return 0;
            }
            return 1;
        }
    }
}
#endif
