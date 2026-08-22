using System.Collections.Generic;
using UnityEngine;
 using AAAUI.VFX;

namespace ProjectSpark.Scanner
{
    [DisallowMultipleComponent]
    public sealed class ScannerCircuitFlowController
        : MonoBehaviour
    {
        [Header("Existing Circuit System")]
        [SerializeField]
        private SignalPath_Manager pathManager;

        [Header("Effect 11")]
        [SerializeField]
        private Material flowMaterial;

        [SerializeField, Min(0f)]
        private float defaultIntensity = 1.5f;

        private readonly List<ScannerCircuitFlowTarget>
            targets =
                new List<ScannerCircuitFlowTarget>();

        private void Awake()
        {
            RefreshRuntimePaths();
        }

        public void RefreshRuntimePaths()
        {
            if (pathManager == null ||
                flowMaterial == null)
            {
                return;
            }

            SignalPath[] paths =
                pathManager.Paths;

            if (paths == null)
                return;

            RemoveNullTargets();

            for (int i = 0;
                 i < paths.Length;
                 i++)
            {
                SignalPath path =
                    paths[i];

                if (path == null)
                    continue;

                SignalPathMesh mesh =
                    pathManager.GetMeshForPath(path);

                if (mesh == null)
                    continue;

                if (FindTarget(mesh) != null)
                    continue;

                CreateTarget(mesh);
            }
        }

        private void CreateTarget(
            SignalPathMesh mesh)
        {
            GameObject targetObject =
                new GameObject(
                    $"__ScannerFlow_{mesh.name}");

            targetObject.transform.SetParent(
                transform,
                false);

            ScannerCircuitFlowTarget target =
                targetObject.AddComponent<
                    ScannerCircuitFlowTarget>();

            target.Initialize(
                mesh,
                flowMaterial);

            targets.Add(target);
        }

        private ScannerCircuitFlowTarget FindTarget(
            SignalPathMesh mesh)
        {
            for (int i = 0;
                 i < targets.Count;
                 i++)
            {
                ScannerCircuitFlowTarget target =
                    targets[i];

                if (target == null)
                    continue;

                if (target.SourceMesh == mesh)
                    return target;
            }

            return null;
        }

        public void SetFlow(
            SignalPath path,
            bool active,
            float direction,
            float intensity)
        {
            if (path == null)
                return;

            RefreshRuntimePaths();

            SignalPathMesh mesh =
                pathManager.GetMeshForPath(path);

            if (mesh == null)
                return;

            ScannerCircuitFlowTarget target =
                FindTarget(mesh);

            if (target == null)
                return;

            target.SetFlow(
                active,
                direction,
                intensity);
        }

        public void SetFlow(
            SignalPath path,
            bool active,
            float direction)
        {
            SetFlow(
                path,
                active,
                direction,
                defaultIntensity);
        }

        public void StopFlow(
            SignalPath path)
        {
            if (path == null)
                return;

            SignalPathMesh mesh =
                pathManager.GetMeshForPath(path);

            if (mesh == null)
                return;

            ScannerCircuitFlowTarget target =
                FindTarget(mesh);

            if (target == null)
                return;

            target.StopFlow();
        }

        public void StopAllFlow()
        {
            for (int i = 0;
                 i < targets.Count;
                 i++)
            {
                ScannerCircuitFlowTarget target =
                    targets[i];

                if (target == null)
                    continue;

                target.StopFlow();
            }
        }

        private void RemoveNullTargets()
        {
            for (int i = targets.Count - 1;
                 i >= 0;
                 i--)
            {
                if (targets[i] == null)
                    targets.RemoveAt(i);
            }
        }
    }
}