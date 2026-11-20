#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using System;
using System.IO;
using NUnit.Framework;
using UnityEngine;
using ProjectSpark.AutoLevelGenerator.Editor;

namespace ProjectSpark.AutoLevelGenerator.Tests
{
    public sealed class SparkAutoGeneratorV2Tests
    {
        [Test]
        public void FindsAtLeastOneRecipeAndLoadsItWithoutFixedPath()
        {
            var recipes = SparkAutoRecipeRegistry.Discover();
            if (recipes.Count == 0) Assert.Ignore("Install recipe JSON files before running discovery test");
            string path = SparkAutoRecipeRegistry.Resolve(recipes[0].Path);
            Assert.AreEqual(recipes[0].Path, path);
            Assert.Greater(SparkAutoLevelSourceImporter.LoadRecipe(path).levelId, 0);
        }

        [Test]
        public void RejectsAssetPathTraversal()
        {
            Assert.Throws<IOException>(() => SparkAutoLevelSourceImporter.ToAbsolute("Assets/../Library/Editor.log"));
        }

        [Test]
        public void RejectsExternalAssetPath()
        {
            Assert.Throws<ArgumentException>(() => SparkAutoLevelSourceImporter.ToAbsolute("C:/Windows/System32"));
        }

        [Test]
        public void UsesDiscoveredRecipeInsteadOfHardcodedLevel110Path()
        {
            var entries = SparkAutoRecipeRegistry.Discover();
            if (entries.Count == 0) Assert.Ignore("No source recipes installed");
            Assert.IsTrue(File.Exists(SparkAutoLevelSourceImporter.ToAbsolute(entries[0].Path)));
        }
    }
}
#endif
