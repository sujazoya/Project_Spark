#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace ProjectSpark.EditorTools
{
    /// <summary>
    /// Creates three preconfigured ParticleSystems under the selected
    /// existing FailureVFX object, IN THE UNITY EDITOR ONLY.
    /// Nothing is instantiated at runtime.
    /// </summary>
    public static class SparkFailureParticleSetupEditor
    {
        private const string MaterialsFolder = "Assets/ProjectSpark_FailureVFX_Materials";

        [MenuItem("Tools/ProjectSpark/VFX/Create Resistor Failure Particle Systems")]
        private static void Create()
        {
            Transform parent = Selection.activeTransform;
            if (parent == null)
            {
                EditorUtility.DisplayDialog("ProjectSpark VFX",
                    "Select your existing FailureVFX GameObject first.", "OK");
                return;
            }

            Material sparksMaterial = GetOrCreateMaterial("SparkParticles", new Color(1f, 0.65f, 0.14f, 1f));
            Material flashMaterial = GetOrCreateMaterial("FlashParticles", new Color(1f, 0.94f, 0.70f, 1f));
            Material debrisMaterial = GetOrCreateMaterial("DebrisParticles", new Color(0.13f, 0.12f, 0.12f, 1f));

            ParticleSystem sparks = GetOrCreate(parent, "Sparks");
            ParticleSystem flash = GetOrCreate(parent, "Flash");
            ParticleSystem debris = GetOrCreate(parent, "Debris");

            ConfigureSparks(sparks, sparksMaterial);
            ConfigureFlash(flash, flashMaterial);
            ConfigureDebris(debris, debrisMaterial);

            EditorUtility.SetDirty(sparks);
            EditorUtility.SetDirty(flash);
            EditorUtility.SetDirty(debris);
            if (PrefabUtility.IsPartOfPrefabInstance(parent))
                PrefabUtility.RecordPrefabInstancePropertyModifications(parent);

            Selection.activeObject = parent.gameObject;
            EditorUtility.DisplayDialog("ProjectSpark VFX",
                "Created/configured Sparks, Flash and Debris under '" + parent.name + "'.\n\n" +
                "Assign their ParticleSystem components to the corresponding emitter fields. " +
                "They are disabled at start and emit only when your failure script calls Emit() or Play().", "OK");
        }

        [MenuItem("Tools/ProjectSpark/VFX/Create Resistor Failure Particle Systems", true)]
        private static bool CanCreate() => Selection.activeTransform != null;

        private static ParticleSystem GetOrCreate(Transform parent, string childName)
        {
            Transform existing = parent.Find(childName);
            GameObject go;
            if (existing != null)
            {
                go = existing.gameObject;
            }
            else
            {
                go = new GameObject(childName);
                Undo.RegisterCreatedObjectUndo(go, "Create ProjectSpark " + childName);
                Undo.SetTransformParent(go.transform, parent, "Parent ProjectSpark " + childName);
                go.transform.localPosition = Vector3.zero;
                go.transform.localRotation = Quaternion.identity;
                go.transform.localScale = Vector3.one;
            }
            ParticleSystem ps = go.GetComponent<ParticleSystem>();
            if (ps == null) ps = Undo.AddComponent<ParticleSystem>(go);
            ParticleSystemRenderer renderer = go.GetComponent<ParticleSystemRenderer>();
            if (renderer == null) renderer = Undo.AddComponent<ParticleSystemRenderer>(go);
            ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            Undo.RecordObject(ps, "Configure " + childName);
            Undo.RecordObject(renderer, "Configure " + childName + " Renderer");
            return ps;
        }

        private static void SetupCommon(ParticleSystem ps, Material mat, float duration,
            ParticleSystem.MinMaxCurve lifetime, ParticleSystem.MinMaxCurve speed,
            ParticleSystem.MinMaxCurve size, Color color)
        {
            var main = ps.main;
            main.duration = duration;
            main.loop = false;
            main.playOnAwake = false;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.maxParticles = 128;
            main.startLifetime = lifetime;
            main.startSpeed = speed;
            main.startSize = size;
            main.startColor = color;
            main.gravityModifier = 0f;
            main.stopAction = ParticleSystemStopAction.None;

            var emission = ps.emission;
            emission.enabled = true;
            emission.rateOverTime = 0f;
            emission.SetBursts(new ParticleSystem.Burst[0]);

            var shape = ps.shape;
            shape.enabled = true;
            shape.shapeType = ParticleSystemShapeType.Sphere;
            shape.radius = 0.035f;
            shape.radiusThickness = 1f;

            var renderer = ps.GetComponent<ParticleSystemRenderer>();
            renderer.renderMode = ParticleSystemRenderMode.Billboard;
            if (mat != null) renderer.sharedMaterial = mat;
            renderer.sortMode = ParticleSystemSortMode.Distance;

            ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }

        private static void ConfigureSparks(ParticleSystem ps, Material mat)
        {
            SetupCommon(ps, mat, 0.2f,
                new ParticleSystem.MinMaxCurve(0.22f, 0.65f),
                new ParticleSystem.MinMaxCurve(1.5f, 4.2f),
                new ParticleSystem.MinMaxCurve(0.012f, 0.027f),
                new Color(1f, 0.63f, 0.13f, 1f));
            var emission = ps.emission;
            emission.SetBursts(new[] { new ParticleSystem.Burst(0f, (short)46) });
            var shape = ps.shape;
            shape.radius = 0.025f;
            var trails = ps.trails;
            trails.enabled = true;
            trails.mode = ParticleSystemTrailMode.PerParticle;
            trails.ratio = 0.6f;
            trails.lifetime = 0.18f;
            trails.dieWithParticles = true;
            var col = ps.colorOverLifetime;
            col.enabled = true;
            var grad = new Gradient();
            grad.SetKeys(new[] {
                new GradientColorKey(new Color(1f, 0.95f, 0.52f), 0f),
                new GradientColorKey(new Color(1f, 0.32f, 0.06f), 1f)
            }, new[] {
                new GradientAlphaKey(1f, 0f),
                new GradientAlphaKey(0f, 1f)
            });
            col.color = new ParticleSystem.MinMaxGradient(grad);
            var sizeLife = ps.sizeOverLifetime;
            sizeLife.enabled = true;
            sizeLife.size = new ParticleSystem.MinMaxCurve(1f,
                AnimationCurve.Linear(0f, 1f, 1f, 0f));
            var velocity = ps.velocityOverLifetime;
            velocity.enabled = false;
            ps.GetComponent<ParticleSystemRenderer>().trailMaterial = mat;
        }

        private static void ConfigureFlash(ParticleSystem ps, Material mat)
        {
            SetupCommon(ps, mat, 0.12f,
                new ParticleSystem.MinMaxCurve(0.08f, 0.15f),
                0f,
                new ParticleSystem.MinMaxCurve(0.10f, 0.18f),
                new Color(1f, 0.95f, 0.65f, 1f));
            var emission = ps.emission;
            emission.SetBursts(new[] { new ParticleSystem.Burst(0f, (short)2) });
            var shape = ps.shape;
            shape.radius = 0.003f;
            var sizeLife = ps.sizeOverLifetime;
            sizeLife.enabled = true;
            sizeLife.size = new ParticleSystem.MinMaxCurve(1f,
                new AnimationCurve(new Keyframe(0f, 0.3f), new Keyframe(0.2f, 1f),
                    new Keyframe(1f, 0f)));
            var colorLife = ps.colorOverLifetime;
            colorLife.enabled = true;
            var grad = new Gradient();
            grad.SetKeys(new[] {
                new GradientColorKey(Color.white, 0f),
                new GradientColorKey(new Color(1f, 0.65f, 0.12f), 1f)
            }, new[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(0f, 1f) });
            colorLife.color = new ParticleSystem.MinMaxGradient(grad);
            var trails = ps.trails;
            trails.enabled = false;
        }

        private static void ConfigureDebris(ParticleSystem ps, Material mat)
        {
            SetupCommon(ps, mat, 0.15f,
                new ParticleSystem.MinMaxCurve(0.3f, 0.95f),
                new ParticleSystem.MinMaxCurve(0.5f, 1.9f),
                new ParticleSystem.MinMaxCurve(0.006f, 0.018f),
                new Color(0.15f, 0.13f, 0.12f, 1f));
            var emission = ps.emission;
            emission.SetBursts(new[] { new ParticleSystem.Burst(0f, (short)15) });
            var main = ps.main;
            main.gravityModifier = 0.8f;
            var shape = ps.shape;
            shape.radius = 0.02f;
            var trails = ps.trails;
            trails.enabled = false;
            var rotationLife = ps.rotationOverLifetime;
            rotationLife.enabled = true;
            rotationLife.z = new ParticleSystem.MinMaxCurve(-9f, 9f);
            var sizeLife = ps.sizeOverLifetime;
            sizeLife.enabled = true;
            sizeLife.size = new ParticleSystem.MinMaxCurve(1f,
                AnimationCurve.Linear(0f, 1f, 1f, 0.25f));
        }

        private static Material GetOrCreateMaterial(string name, Color tint)
        {
            if (!AssetDatabase.IsValidFolder(MaterialsFolder))
                AssetDatabase.CreateFolder("Assets", "ProjectSpark_FailureVFX_Materials");
            string path = MaterialsFolder + "/" + name + ".mat";
            Material existing = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (existing != null) return existing;

            Shader shader = Shader.Find("Universal Render Pipeline/Particles/Unlit");
            if (shader == null) shader = Shader.Find("Particles/Standard Unlit");
            if (shader == null) shader = Shader.Find("Sprites/Default");
            if (shader == null)
            {
                Debug.LogWarning("[ProjectSpark VFX] Could not find a particle shader. Assign a suitable URP particle material manually.");
                return null;
            }
            Material mat = new Material(shader);
            mat.name = name;
            if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", tint);
            if (mat.HasProperty("_Color")) mat.SetColor("_Color", tint);
            if (mat.HasProperty("_Surface")) mat.SetFloat("_Surface", 1f);
            AssetDatabase.CreateAsset(mat, path);
            return mat;
        }
    }
}
#endif
