using System;
using UnityEditor;
namespace ProjectSpark.Level129.Editor
{
    public sealed class SparkL129ImportSettings : AssetPostprocessor
    {
        private void OnPreprocessModel()
        {
            if (!assetPath.StartsWith("Assets/My_Assets/_Project_Spark/Prefabs/Levels/Level_129_Layout/H610ME/Models_FBX/", StringComparison.Ordinal)) return;
            if (!assetPath.EndsWith(".fbx",StringComparison.OrdinalIgnoreCase)) return;
            var importer=(ModelImporter)assetImporter;
            importer.globalScale=1f;
            importer.useFileScale=true;
            importer.importNormals=ModelImporterNormals.Import;
            importer.importTangents=ModelImporterTangents.CalculateMikk;
            importer.importAnimation=false;
            importer.preserveHierarchy=true;
            importer.addCollider=false;
            importer.materialImportMode=ModelImporterMaterialImportMode.None;
        }
    }
}

