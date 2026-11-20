#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

namespace ProjectSpark.AutoLevelGenerator.Tests
{
    public sealed class SparkAutoLevelGeneratorTests
    {
        private GameObject root;
        private SparkAutoLevelBlueprint blueprint;
        private SparkAutoLevelRuntime runtime;
        private SparkAutoLevelApprovingFixtureBridge bridge;

        [SetUp]
        public void SetUp()
        {
            root = new GameObject("TestLevelRoot");
            blueprint = ScriptableObject.CreateInstance<SparkAutoLevelBlueprint>();
            var parts = new List<SparkAutoPart> { new SparkAutoPart { Id = "U1", Type = "CPU", Installable = true } };
            var routes = new List<SparkAutoRoute> { new SparkAutoRoute {
                Id = "W001", NetId = "UNASSIGNED", SourceComponent = "U1", SourcePad = "UNASSIGNED",
                DestinationComponent = "U2", DestinationPad = "UNASSIGNED", Layer = "UNASSIGNED", SourceStatus = "NOT_CERTIFIABLE" } };
            var vias = new List<SparkAutoVia> { new SparkAutoVia { Id = "V001", NetId = "UNASSIGNED", FromLayer = "UNASSIGNED", ToLayer = "UNASSIGNED", SourceStatus = "NOT_CERTIFIABLE" } };
            var objs = new List<SparkAutoObjective> {
                new SparkAutoObjective { Id = "01", Title = "Assembly" },
                new SparkAutoObjective { Id = "02", Title = "Electrical" },
                new SparkAutoObjective { Id = "06", Title = "Safety" } };
            blueprint.Populate(110, "PWR18", "Test", 310, 230, 12, 0.001f, parts, routes, vias, objs,
                new List<SparkAutoFault>(), new SparkAutoSafety { JumperId = "W173", PhysicalPinStatus = "UNASSIGNED", NetlistStatus = "NOT_CERTIFIABLE" });
            runtime = root.AddComponent<SparkAutoLevelRuntime>();
            runtime.Configure(blueprint, false);
            bridge = root.AddComponent<SparkAutoLevelApprovingFixtureBridge>();
            runtime.BindAuthority(bridge);
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(root);
            Object.DestroyImmediate(blueprint);
        }

        [Test]
        public void MissingSchematicPreventsCopperAndViaCompletionEvenWithPermissiveTestBridge()
        {
            string reason;
            Assert.IsFalse(runtime.TryCommitRoute("W001", out reason));
            StringAssert.Contains("SCHEMATIC_REQUIRED", reason);
            Assert.IsFalse(runtime.TryCommitVia("V001", out reason));
            Assert.AreEqual(0, runtime.CompletedRouteCount);
            Assert.AreEqual(0, runtime.CompletedViaCount);
        }

        [Test]
        public void AssembledReferenceHasZeroGameplayAuthority()
        {
            runtime.Configure(blueprint, true);
            runtime.BindAuthority(bridge);
            string reason;
            Assert.IsFalse(runtime.TryAwardObjective("01", out reason));
            Assert.AreEqual(0, runtime.PassedObjectiveCount);
        }

        [Test]
        public void UnverifiedSafetyResetCannotBeConfirmed()
        {
            string reason;
            Assert.IsFalse(runtime.TryConfirmSafetyReset(out reason));
            Assert.IsFalse(runtime.SafetyResetConfirmed);
        }

        [Test]
        public void FullCircuitObjectiveNeedsApprovedRoutesAndVias()
        {
            string reason;
            Assert.IsFalse(runtime.TryAwardObjective("02", out reason));
            Assert.AreEqual(0, runtime.PassedObjectiveCount);
        }

        [Test]
        public void RouteDefinitionsRequireAllPinsNetAndApprovedLayer()
        {
            var route = new SparkAutoRoute {
                Id = "W001", NetId = "VCORE", SourceComponent = "U1", SourcePad = "P1",
                DestinationComponent = "U2", DestinationPad = "P2", Layer = "3", SourceStatus = "APPROVED" };
            Assert.IsTrue(route.HasSchematicFields(12));
            route.Layer = "13";
            Assert.IsFalse(route.HasSchematicFields(12));
        }
    }
}
#endif
