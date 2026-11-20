using System;
using System.Collections.Generic;
using ProjectSpark.Gameplay;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace ProjectSpark.EditorTools
{
    internal static class SparkLevel107Finish
    {
        private const string Folder = SparkLevel107Builder.Folder;
        private const string Finishes = Folder + "/Phoenix Finishes";

        public static void Apply(SparkLevel107Process process, Dictionary<string, Material> materials)
        {
            if (!AssetDatabase.IsValidFolder(Finishes)) AssetDatabase.CreateFolder(Folder, "Phoenix Finishes");
            var old = process.modelSpace.Find("Phoenix board finish");
            if (old) Object.DestroyImmediate(old.gameObject);
            var root = Child(process.modelSpace, "Phoenix board finish", Vector3.zero);
            var ink = Finish("Solder mask", materials["PCB"], new Color(.17f, .48f, .54f), .19f, .53f, "PCB");
            var plated = Finish("Brushed gold", materials["GOLD"], new Color(1f, .77f, .34f), .78f, .47f, "GOLD");
            var ceramic = Finish("Ceramic packages", materials["CERAMIC"], new Color(.58f, .54f, .49f), .17f, .38f, "CERAMIC");
            var shield = Finish("Dark shield", materials["DARK_METAL"], new Color(.70f, .72f, .79f), .62f, .39f, "DARK_METAL");
            var role = new Dictionary<string, Material> {["PCB"] = ink, ["GOLD"] = plated, ["CERAMIC"] = ceramic, ["DARK_METAL"] = shield};
            Remap(process.modelSpace.Find("L107_BoardArtRoot"), role);
            Remap(process.modelSpace.Find("L107_StructuralReferences"), role);
            foreach (var part in process.parts) Remap(part.model.transform, role);
            var lampMaterial = Finish("Status lamps", materials["CYAN"], Color.white, .12f, .28f);
            lampMaterial.EnableKeyword("_EMISSION");
            lampMaterial.SetColor("_EmissionColor", new Color(.03f, .3f, .26f));

            const float width = .278f, depth = .197f, y = .0039f;
            void Border(string name, Vector3 origin, Vector3 size) =>
                Primitive(root, name, PrimitiveType.Cube, origin, size, plated);
            Border("Rear plated perimeter", new Vector3(0, y, depth * .5f), new Vector3(width, .00034f, .00085f));
            Border("Front plated perimeter", new Vector3(0, y, -depth * .5f), new Vector3(width, .00034f, .00085f));
            Border("Left plated perimeter", new Vector3(-width * .5f, y, 0), new Vector3(.00085f, .00034f, depth));
            Border("Right plated perimeter", new Vector3(width * .5f, y, 0), new Vector3(.00085f, .00034f, depth));

            Label(root, "PFX–15   /   PHOENIX PROTOCOL", new Vector3(-.047f, .0045f, .087f), .0041f, new Color(.9f, .79f, .54f));
            Label(root, "DUAL-BANK RECOVERY CONTROLLER", new Vector3(-.060f, .0045f, .078f), .0023f, new Color(.70f, .84f, .87f));
            Label(root, "PFX15-EDU-1    •    10 COPPER LAYERS", new Vector3(-.070f, .0045f, -.091f), .0025f, new Color(.78f, .86f, .86f));
            process.primaryLamp = ModuleStatus(process.primaryIndex, "U6   PRIMARY", new Color(1f, .75f, .39f));
            process.recoveryLamp = ModuleStatus(process.recoveryIndex, "U7   RECOVERY", new Color(.49f, 1f, .89f));
            process.trustLamp = ModuleStatus(process.jumperIndex, "W149   TRUST", new Color(1f, .84f, .53f));
            Illuminate(root, "Amber bench key", new Vector3(-.093f, .108f, -.047f), new Color(1f, .83f, .65f), 3.7f);
            Illuminate(root, "Cool firmware fill", new Vector3(.082f, .112f, .036f), new Color(.58f, .78f, 1f), 2.45f);

            Renderer ModuleStatus(int index, string label, Color tint)
            {
                Vector3 center = process.parts[index].footprint.localPosition;
                Vector3 labelPosition = new Vector3(center.x, .009f, center.z - .015f);
                Label(root, label, labelPosition, .00225f, tint);
                Primitive(root, label + " silkscreen rule", PrimitiveType.Cube, labelPosition + new Vector3(0f, -.001f, -.0033f),
                    new Vector3(.021f, .00022f, .00042f), plated);
                return Primitive(root, label + " status", PrimitiveType.Sphere, labelPosition + new Vector3(-.012f, .0011f, 0f),
                    new Vector3(.0033f, .0013f, .0033f), lampMaterial);
            }
        }

        private static void Remap(Transform root, Dictionary<string, Material> finishes)
        {
            if (!root) return;
            foreach (var renderer in root.GetComponentsInChildren<Renderer>(true))
            {
                var materials = renderer.sharedMaterials;
                for (int i = 0; i < materials.Length; i++)
                {
                    if (!materials[i]) continue;
                    string name = materials[i].name.ToUpperInvariant().Replace("_AUTOPBR", "").Replace(" (MATERIAL)", "");
                    foreach (var finish in finishes)
                        if (name == finish.Key || name.EndsWith("_" + finish.Key, StringComparison.Ordinal))
                        { materials[i] = finish.Value; break; }
                }
                renderer.sharedMaterials = materials;
            }
        }

        private static Material Finish(string name, Material original, Color tint, float metallic, float smoothness, string texture = "")
        {
            string path = $"{Finishes}/{name}.mat";
            var material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (!material)
            {
                material = new Material(original) {name = name};
                AssetDatabase.CreateAsset(material, path);
            }
            material.SetColor("_BaseColor", tint);
            material.SetFloat("_Metallic", metallic);
            material.SetFloat("_Smoothness", smoothness);
            if (!string.IsNullOrEmpty(texture))
            {
                string map = $"{Folder}/Textures/L106_{texture}_Normal.png";
                var importer = AssetImporter.GetAtPath(map) as TextureImporter;
                if (importer && importer.textureType != TextureImporterType.NormalMap)
                { importer.textureType = TextureImporterType.NormalMap; importer.SaveAndReimport(); }
                var normal = AssetDatabase.LoadAssetAtPath<Texture2D>(map);
                if (normal)
                {
                    material.SetTexture("_BumpMap", normal);
                    material.SetFloat("_BumpScale", .42f);
                    material.EnableKeyword("_NORMALMAP");
                }
            }
            EditorUtility.SetDirty(material);
            return material;
        }

        private static Transform Child(Transform parent, string name, Vector3 place)
        {
            var node = new GameObject(name).transform;
            node.SetParent(parent, false);
            node.localPosition = place;
            return node;
        }

        private static Renderer Primitive(Transform parent, string name, PrimitiveType type, Vector3 origin, Vector3 size, Material material)
        {
            var node = GameObject.CreatePrimitive(type);
            node.name = name;
            node.transform.SetParent(parent, false);
            node.transform.localPosition = origin;
            node.transform.localScale = size;
            Object.DestroyImmediate(node.GetComponent<Collider>());
            var renderer = node.GetComponent<Renderer>();
            renderer.sharedMaterial = material;
            renderer.receiveShadows = false;
            return renderer;
        }

        private static void Label(Transform root, string label, Vector3 place, float size, Color color)
        {
            var node = Child(root, label, place);
            node.localRotation = Quaternion.Euler(90, 0, 0);
            node.localScale = Vector3.one * .18f;
            var text = node.gameObject.AddComponent<TextMesh>();
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = 74;
            text.characterSize = size;
            text.anchor = TextAnchor.MiddleCenter;
            text.color = color;
            text.text = label;
            text.GetComponent<Renderer>().sharedMaterial = text.font.material;
        }

        private static void Illuminate(Transform root, string name, Vector3 place, Color color, float intensity)
        {
            var light = Child(root, name, place).gameObject.AddComponent<Light>();
            light.type = LightType.Point;
            light.range = 8.5f;
            light.color = color;
            light.intensity = intensity;
            light.shadows = LightShadows.None;
        }
    }
}
