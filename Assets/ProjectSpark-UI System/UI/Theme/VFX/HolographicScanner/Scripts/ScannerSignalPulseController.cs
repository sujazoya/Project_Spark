using System.Collections.Generic;
using UnityEngine;
using AAAUI.VFX;

namespace ProjectSpark.Scanner
{
    [DisallowMultipleComponent]
    public sealed class ScannerSignalPulseController
        : MonoBehaviour
    {
        [Header("Existing Circuit")]
        [SerializeField]
        private SignalPath_Manager pathManager;

        [Header("Effect 12")]
        [SerializeField]
        private Material pulseMaterial;

        [SerializeField, Min(0f)]
        private float pulseIntensity = 4f;

        private readonly List<ScannerSignalPulseTarget>
            targets =
                new List<ScannerSignalPulseTarget>();

        private void Awake()
        {
            RefreshRuntimePaths();
        }

        public void RefreshRuntimePaths()
        {
            if (pathManager == null ||
                pulseMaterial == null)
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
                    $"__ScannerPulse_{mesh.name}");

            targetObject.transform.SetParent(
                transform,
                false);

            ScannerSignalPulseTarget target =
                targetObject.AddComponent<
                    ScannerSignalPulseTarget>();

            target.Initialize(
                mesh,
                pulseMaterial);

            targets.Add(target);
        }

        private ScannerSignalPulseTarget FindTarget(
            SignalPathMesh mesh)
        {
            for (int i = 0;
                 i < targets.Count;
                 i++)
            {
                ScannerSignalPulseTarget target =
                    targets[i];

                if (target == null)
                    continue;

                if (target.SourceMesh == mesh)
                    return target;
            }

            return null;
        }

        public void SendPulse(
            SignalPath path,
            float position,
            float direction)
        {
            if (path == null)
                return;

            RefreshRuntimePaths();

            SignalPathMesh mesh =
                pathManager.GetMeshForPath(path);

            if (mesh == null)
                return;

            ScannerSignalPulseTarget target =
                FindTarget(mesh);

            if (target == null)
                return;

            target.SetPulse(
                position,
                direction,
                pulseIntensity);
        }

        public void StopPulse(
            SignalPath path)
        {
            if (path == null)
                return;

            SignalPathMesh mesh =
                pathManager.GetMeshForPath(path);

            if (mesh == null)
                return;

            ScannerSignalPulseTarget target =
                FindTarget(mesh);

            if (target == null)
                return;

            target.StopPulse();
        }

        public void StopAllPulses()
        {
            for (int i = 0;
                 i < targets.Count;
                 i++)
            {
                ScannerSignalPulseTarget target =
                    targets[i];

                if (target == null)
                    continue;

                target.StopPulse();
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