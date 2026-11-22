using System;
using UnityEditor;
namespace ProjectSpark.Level130.Editor
{
    public sealed class SparkL130ImportSettings : AssetPostprocessor
    {
        private void OnPreprocessModel()
        {
            if (!assetPath.StartsWith("Assets/ProjectSpark/Level130/H610ME/Models_FBX/", StringComparison.Ordinal)) return;
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
