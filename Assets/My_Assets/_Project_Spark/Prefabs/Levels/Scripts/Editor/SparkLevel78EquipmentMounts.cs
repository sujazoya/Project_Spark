#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using ProjectSpark.Gameplay;
using Unity.Collections;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace ProjectSpark.EditorTools
{
    public static partial class SparkLevel78ModelInstaller
    {
        private const string SurfaceMaps = "Assets/My_Assets/_Project_Spark/Prefabs/Levels/Level_80_Layout/Textures/";
        private const string WorkbenchMeshes = Folder + "Models/Workbench";
        private const float FittedContactLift = .08f;
        private const float ContactFace = .042f;

        internal static IEnumerable<string[]> ContactRows(string file)
        {
            var data = AssetDatabase.LoadAssetAtPath<TextAsset>(Folder + file);
            if (!data) throw new InvalidOperationException("Missing Level 78 equipment data: " + file);
            return data.text.Split('\n').Skip(1).Where(row => !string.IsNullOrWhiteSpace(row))
                .Select(row => row.TrimEnd('\r').Split(','));
        }

        internal static Vector3 MountPosition(string[] values)
        {
            Vector3 position = SparkEquipmentWorkbenchFit.SourcePosition(values);
            if (values[6] == "Modeled")
                position += new Vector3(0, FittedContactLift, -ContactFace) * SparkEquipmentWorkbenchFit.SourceArtUnit;
            return position;
        }

        private static string SurfaceFor(string mesh, string tint) => tint switch
        {
            "glass" => "OpticalGlass",
            "tintDC803E" or "tintDD813E" => "CopperBus",
            "tintE9BC56" => "GoldContact",
            "tint47657C" or "tintC2D0D6" or "tint97B3C4" => "BrushedSteel",
            "tint0C1620" when mesh.Contains("Cable") => "Elastomer",
            "tint2D3034" => "Elastomer",
            "tint0C1620" or "tintE0DED3" => "BlackCeramic",
            "tint0D1D2F" when mesh.StartsWith(BaseModule, StringComparison.Ordinal) => "CleanroomFloor",
            "tint0D1D2F" when mesh.Contains("Wafer") => "SiliconInterposer",
            "tint0D1D2F" or "tint1D303F" => "AnodizedPanel",
            _ => null
        };

        private static void ReplaceBakedContactBanks(Transform assembly)
        {
            int stripped = 0;
            foreach (var group in ContactRows("Molded_Contact_Centers.csv").GroupBy(row => row[0]))
            {
                var module = assembly.Find(group.Key);
                if (!module) throw new InvalidOperationException("Missing equipment " + group.Key);
                var centers = group.Select(row => SparkEquipmentWorkbenchFit.SourcePosition(
                    new[] { "", row[0], row[1], row[2], row[3] }) / SparkEquipmentWorkbenchFit.SourceArtUnit +
                    Vector3.up * FittedContactLift).ToArray();
                bool removedCopper = false;
                foreach (var filter in module.GetComponentsInChildren<MeshFilter>())
                {
                    string tint = filter.name.Substring(filter.name.LastIndexOf("__", StringComparison.Ordinal) + 2);
                    bool copper = tint is "tintDC803E" or "tintDD813E";
                    if (!copper && tint is not ("tint47657C" or "tintED484F" or "tint27D8F6" or "tint1B76D6" or
                        "tint32E095" or "tintFDB344" or "tintA65CF2" or "tintA65DF2" or "tintC078FF" or "tint3FCAD2"))
                        continue;

                    Mesh original = filter.sharedMesh;
                    using var data = MeshUtility.AcquireReadOnlyMeshData(original);
                    var source = data[0];
                    using var vertices = new NativeArray<Vector3>(source.vertexCount, Allocator.Temp);
                    source.GetVertices(vertices);
                    var toArt = assembly.worldToLocalMatrix * filter.transform.localToWorldMatrix;
                    var contacts = new bool[vertices.Length];
                    for (int i = 0; i < vertices.Length; i++)
                    {
                        Vector3 point = toArt.MultiplyPoint3x4(vertices[i]) / SparkEquipmentWorkbenchFit.SourceArtUnit;
                        for (int contact = 0; contact < centers.Length; contact++)
                        {
                            Vector3 offset = point - centers[contact];
                            if (Mathf.Abs(offset.x) >= .053f || offset.y is < -.106f or > .054f ||
                                offset.z is < -.050f or > .047f) continue;
                            contacts[i] = true;
                            break;
                        }
                    }
                    if (!contacts.Any(contact => contact)) continue;

                    var submeshes = new List<int>[source.subMeshCount];
                    bool changed = false;
                    for (int sub = 0; sub < submeshes.Length; sub++)
                    {
                        using var triangles = new NativeArray<int>(source.GetSubMesh(sub).indexCount, Allocator.Temp);
                        source.GetIndices(triangles, sub);
                        var remaining = new List<int>(triangles.Length);
                        for (int i = 0; i < triangles.Length; i += 3)
                        {
                            int a = triangles[i], b = triangles[i + 1], c = triangles[i + 2];
                            // The supplied molded banks overlap on some cabinets. Treat them as
                            // one removable layer before adding individual interactive jack objects.
                            if (contacts[a] && contacts[b] && contacts[c])
                            {
                                changed = true;
                                removedCopper |= copper;
                                continue;
                            }
                            remaining.Add(a);
                            remaining.Add(b);
                            remaining.Add(c);
                        }
                        submeshes[sub] = remaining;
                    }
                    if (!changed) continue;
                    if (submeshes.All(indices => indices.Count == 0))
                    {
                        filter.gameObject.SetActive(false);
                        continue;
                    }

                    if (!AssetDatabase.IsValidFolder(WorkbenchMeshes)) AssetDatabase.CreateFolder(Folder + "Models", "Workbench");
                    string path = $"{WorkbenchMeshes}/{filter.name}.asset";
                    var mesh = AssetDatabase.LoadAssetAtPath<Mesh>(path);
                    bool newAsset = !mesh;
                    if (newAsset) mesh = new Mesh { name = filter.name };
                    var writable = Mesh.AllocateWritableMeshData(1);
                    writable[0].SetVertexBufferParams(source.vertexCount, original.GetVertexAttributes());
                    for (int stream = 0; stream < source.vertexBufferCount; stream++)
                        writable[0].GetVertexData<byte>(stream).CopyFrom(source.GetVertexData<byte>(stream));
                    Mesh.ApplyAndDisposeWritableMeshData(writable, mesh);
                    mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
                    mesh.subMeshCount = submeshes.Length;
                    for (int sub = 0; sub < submeshes.Length; sub++)
                        mesh.SetTriangles(submeshes[sub], sub, false);
                    mesh.bounds = original.bounds;
                    if (newAsset) AssetDatabase.CreateAsset(mesh, path);
                    else EditorUtility.SetDirty(mesh);
                    filter.sharedMesh = mesh;
                }
                if (!removedCopper)
                    throw new InvalidOperationException("Could not identify the molded bank on " + module.name);
                stripped += centers.Length;
            }
            Debug.Log($"[LEVEL 78] Replaced molded banks containing {stripped} source contacts.");
        }

        private static void SeatOriginalTerminals(Transform bench, Transform assembly, SparkLevelSceneBindings bindings,
            SparkLevel78Process loop, Font font, Shader shader)
        {
            int pinLayer = LayerMask.NameToLayer("Pin");
            if (pinLayer < 0) throw new InvalidOperationException("The Pin layer is missing.");
            var specs = Sockets.ToDictionary(socket => socket.id);
            var indices = loop.pins.Select((pin, index) => (pin.name, index)).ToDictionary(entry => entry.name, entry => entry.index);
            var positions = new HashSet<Vector3>();
            var assigned = new HashSet<string>();
            loop.pinLabels = new TextMesh[loop.pins.Length];
            float unit = bench.InverseTransformVector(assembly.TransformVector(
                Vector3.right * SparkEquipmentWorkbenchFit.SourceArtUnit)).magnitude;
            var housing = MaterialFor(shader, "tint47657C", "BrushedSteel");

            foreach (var row in ContactRows("Equipment_Terminal_Mounts.csv"))
            {
                if (row.Length != 7 || !(row[6] is "Modeled" or "Planned") || !assigned.Add(row[5]) ||
                    !bindings.TryGetTerminal(row[5], out var terminal))
                    throw new InvalidOperationException("Duplicate or unresolved equipment contact " + string.Join(",", row));
                var module = assembly.Find(row[1]);
                Vector3 position = MountPosition(row);
                if (!module || !positions.Add(position))
                    throw new InvalidOperationException("Missing equipment or overlapping contact " + string.Join(",", row));

                var mount = Child(bench, $"{row[0]} {row[5]}", Vector3.zero);
                mount.SetPositionAndRotation(assembly.TransformPoint(position), bench.rotation);
                mount.SetParent(module, true);
                var jack = terminal.transform;
                jack.SetParent(mount, false);
                jack.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
                jack.localScale = Vector3.one;
                jack.gameObject.layer = pinLayer;

                var collider = jack.GetComponent<BoxCollider>();
                foreach (var oldCollider in jack.GetComponents<Collider>())
                    if (oldCollider != collider) Object.DestroyImmediate(oldCollider);
                if (!collider) collider = jack.gameObject.AddComponent<BoxCollider>();
                collider.size = new Vector3(.113f, .113f, .050f) * unit;
                collider.center = Vector3.zero;
                if (jack.TryGetComponent<MeshRenderer>(out var renderer)) Object.DestroyImmediate(renderer);
                if (jack.TryGetComponent<MeshFilter>(out var mesh)) Object.DestroyImmediate(mesh);
                foreach (var child in jack.Cast<Transform>().ToArray())
                    if (child.name is "Jack recess" or "Panel socket housing" or "Contact face")
                        Object.DestroyImmediate(child.gameObject);

                bool modeled = row[6] == "Modeled";
                var insert = MaterialFor(shader, specs[terminal.name].color);
                SocketSleeve(jack, "Panel socket housing", unit, modeled ? .108f : .106f, .071f, .071f, housing);
                SocketSleeve(jack, "Contact face", unit, modeled ? .076f : .093f, .009f, 0f, insert);

                int index = indices[row[5]];
                var pinData = new SerializedObject(terminal);
                pinData.FindProperty("maxConnections").intValue = SparkLevel78Process.Wires.Count(wire => wire.a == index || wire.b == index);
                pinData.ApplyModifiedPropertiesWithoutUndo();
                var tag = Label(mount, font, "Label " + terminal.name, specs[terminal.name].label,
                    new Vector3(0, .026f, -.011f), .018f);
                tag.transform.localRotation = Quaternion.Euler(30, 0, 0);
                tag.gameObject.SetActive(false);
                loop.pinLabels[index] = tag;
            }
            if (assigned.Count != loop.pins.Length || loop.pinLabels.Any(label => !label))
                throw new InvalidOperationException("Level 78 must mount every circuit terminal on equipment.");
        }

        private static void SocketSleeve(Transform jack, string name, float unit, float diameter, float halfDepth,
            float offset, Material material)
        {
            var sleeve = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            Object.DestroyImmediate(sleeve.GetComponent<Collider>());
            sleeve.name = name;
            sleeve.transform.SetParent(jack, false);
            sleeve.transform.SetLocalPositionAndRotation(Vector3.forward * (unit * offset), Quaternion.Euler(90, 0, 0));
            sleeve.transform.localScale = new Vector3(diameter, halfDepth, diameter) * unit;
            sleeve.GetComponent<Renderer>().sharedMaterial = material;
        }
    }
}
#endif

