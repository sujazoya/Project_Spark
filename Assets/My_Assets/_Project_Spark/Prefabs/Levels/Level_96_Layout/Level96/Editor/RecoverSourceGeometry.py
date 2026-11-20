import bpy
import os
import glob
import json
import time

project = r"D:\Project_Spark_Master_Project\Project_Spark"
source = os.path.join(project, "Assets", "ProjectSpark", "Level103", "Models_GLB")
output = os.path.join(project, "Assets", "ProjectSpark", "Level103", "Models_Rebuilt")
os.makedirs(output, exist_ok=True)
records = []
for path in sorted(glob.glob(os.path.join(source, "*.glb"))):
    bpy.ops.wm.read_factory_settings(use_empty=True)
    bpy.ops.import_scene.gltf(filepath=path)
    meshes = [obj for obj in bpy.data.objects if obj.type == "MESH"]
    if not meshes:
        raise RuntimeError("No GLB mesh geometry in " + path)
    for obj in meshes:
        obj.hide_render = False
        obj.hide_viewport = False
        obj.hide_set(False)
    target = os.path.join(output, os.path.splitext(os.path.basename(path))[0] + ".fbx")
    bpy.context.scene.unit_settings.system = "METRIC"
    bpy.context.scene.unit_settings.scale_length = 1.0
    bpy.ops.export_scene.fbx(
        filepath=target,
        use_selection=False,
        object_types={"MESH", "EMPTY"},
        add_leaf_bones=False,
        bake_anim=False,
        apply_unit_scale=True,
        axis_forward="-Z",
        axis_up="Y",
        path_mode="AUTO",
    )
    records.append({"model": os.path.basename(path), "mesh_objects": len(meshes),
                    "vertices": sum(len(obj.data.vertices) for obj in meshes),
                    "output": target})
    print("L103_REEXPORTED", os.path.basename(path), len(meshes), flush=True)
with open(os.path.join(project, "Logs", "Level103_Model_Recovery.json"), "w", encoding="utf-8") as stream:
    json.dump(records, stream, indent=2)
print("PASS", len(records), "source models re-exported from provided GLB geometry", flush=True)