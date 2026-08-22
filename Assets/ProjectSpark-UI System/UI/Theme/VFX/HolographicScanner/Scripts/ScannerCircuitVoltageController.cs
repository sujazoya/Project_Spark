using System.Collections.Generic;
using UnityEngine;
using AAAUI.VFX;

namespace ProjectSpark.Scanner
{
    [DisallowMultipleComponent]
    public sealed class ScannerCircuitVoltageController
        : MonoBehaviour
    {
        [Header("Existing Circuit")]
        [SerializeField]
        private SignalPath_Manager pathManager;

        [Header("Effect 13")]
        [SerializeField]
        private Material voltageMaterial;

        [Header("Normalization")]
        [SerializeField, Min(0.0001f)]
        private float maximumVoltage = 24f;

        private readonly List<ScannerCircuitVoltageTarget>
            targets =
                new List<ScannerCircuitVoltageTarget>();

        private void Awake()
        {
            RefreshRuntimePaths();
        }

        public void RefreshRuntimePaths()
        {
            if (pathManager == null ||
                voltageMaterial == null)
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
                    $"__ScannerVoltage_{mesh.name}");

            targetObject.transform.SetParent(
                transform,
                false);

            ScannerCircuitVoltageTarget target =
                targetObject.AddComponent<
                    ScannerCircuitVoltageTarget>();

            target.Initialize(
                mesh,
                voltageMaterial);

            targets.Add(target);
        }

        private ScannerCircuitVoltageTarget FindTarget(
            SignalPathMesh mesh)
        {
            for (int i = 0;
                 i < targets.Count;
                 i++)
            {
                ScannerCircuitVoltageTarget target =
                    targets[i];

                if (target == null)
                    continue;

                if (target.SourceMesh == mesh)
                    return target;
            }

            return null;
        }

        public void SetVoltage(
            SignalPath path,
            float voltage)
        {
            if (path == null)
                return;

            RefreshRuntimePaths();

            SignalPathMesh mesh =
                pathManager.GetMeshForPath(path);

            if (mesh == null)
                return;

            ScannerCircuitVoltageTarget target =
                FindTarget(mesh);

            if (target == null)
                return;

            float normalized =
                Mathf.Abs(voltage) /
                maximumVoltage;

            target.SetVoltage(
                Mathf.Clamp01(normalized));
        }

        public void SetNormalizedVoltage(
            SignalPath path,
            float normalizedVoltage)
        {
            if (path == null)
                return;

            RefreshRuntimePaths();

            SignalPathMesh mesh =
                pathManager.GetMeshForPath(path);

            if (mesh == null)
                return;

            ScannerCircuitVoltageTarget target =
                FindTarget(mesh);

            if (target == null)
                return;

            target.SetVoltage(
                Mathf.Clamp01(
                    normalizedVoltage));
        }

        public void ClearVoltage(
            SignalPath path)
        {
            if (path == null)
                return;

            SignalPathMesh mesh =
                pathManager.GetMeshForPath(path);

            if (mesh == null)
                return;

            ScannerCircuitVoltageTarget target =
                FindTarget(mesh);

            if (target == null)
                return;

            target.ClearVoltage();
        }

        public void ClearAllVoltage()
        {
            for (int i = 0;
                 i < targets.Count;
                 i++)
            {
                ScannerCircuitVoltageTarget target =
                    targets[i];

                if (target == null)
                    continue;

                target.ClearVoltage();
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