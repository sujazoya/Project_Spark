#if UNITY_EDITOR

using UnityEditor;
using UnityEngine;

namespace ProjectSpark.Scanner.Editor
{
    public static class ScannerEnableMeshReadWrite
    {
        [MenuItem("Project Spark/Scanner/Enable Mesh Read Write")]
        private static void EnableMeshReadWrite()
        {
            string[] guids =
                AssetDatabase.FindAssets(
                    "t:Model");

            int changed = 0;

            try
            {
                for (int i = 0;
                     i < guids.Length;
                     i++)
                {
                    string path =
                        AssetDatabase.GUIDToAssetPath(
                            guids[i]);

                    ModelImporter importer =
                        AssetImporter.GetAtPath(path)
                        as ModelImporter;

                    if (importer == null)
                        continue;

                    if (importer.isReadable)
                        continue;

                    importer.isReadable = true;

                    importer.SaveAndReimport();

                    changed++;
                }

                AssetDatabase.Refresh();

                Debug.Log(
                    $"Project Spark Scanner: " +
                    $"Enabled Read/Write on {changed} model assets.");
            }
            catch (System.Exception exception)
            {
                Debug.LogException(exception);
            }
        }
    }
}

#endif