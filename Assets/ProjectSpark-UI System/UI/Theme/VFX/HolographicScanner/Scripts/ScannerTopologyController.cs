using System.Collections.Generic;
using UnityEngine;
using AAAUI.VFX;

namespace ProjectSpark.Scanner
{
    [DisallowMultipleComponent]
    public sealed class ScannerTopologyController : MonoBehaviour
    {
        [Header("Existing Circuit System")]
        [SerializeField]
        private SignalPath_Manager pathManager;

        [Header("Effect 10")]
        [SerializeField]
        private Material topologyMaterial;

        private readonly List<ScannerTopologyConnectionTarget>
            runtimeTargets =
                new List<ScannerTopologyConnectionTarget>();

        private float progress;
        private bool scanning;

        public float Progress => progress;

        private void Awake()
        {
            if (pathManager == null)
            {
                Debug.LogWarning(
                    $"[{name}] SignalPath_Manager is not assigned.",
                    this);
            }

            if (topologyMaterial == null)
            {
                Debug.LogWarning(
                    $"[{name}] Topology Material is not assigned.",
                    this);
            }
        }

        public void StartTopologyScan()
        {
            scanning = true;
            progress = 0f;

            RefreshRuntimePaths();
            ApplyProgress();
        }

        public void SetProgress(float value)
        {
            progress = Mathf.Clamp01(value);
            scanning = true;

            RefreshRuntimePaths();
            ApplyProgress();
        }

        public void CompleteTopology()
        {
            scanning = true;
            progress = 1f;

            RefreshRuntimePaths();
            ApplyProgress();
        }

        public void ResetTopology()
        {
            progress = 0f;
            scanning = false;

            DisableAllTargets();
        }

        public void RefreshRuntimePaths()
        {
            if (pathManager == null ||
                topologyMaterial == null)
            {
                return;
            }

            SignalPath[] paths =
                pathManager.Paths;

            if (paths == null)
                return;

            RemoveNullTargets();

            for (int i = 0; i < paths.Length; i++)
            {
                SignalPath path = paths[i];

                if (path == null)
                    continue;

                SignalPathMesh mesh =
                    pathManager.GetMeshForPath(path);

                if (mesh == null)
                {
                    Debug.LogWarning(
                        $"[{name}] No SignalPathMesh found for " +
                        $"path '{path.name}'.",
                        path);

                    continue;
                }

                if (FindTarget(mesh) != null)
                    continue;

                CreateRuntimeTarget(mesh);
            }
        }

        private ScannerTopologyConnectionTarget FindTarget(
            SignalPathMesh mesh)
        {
            if (mesh == null)
                return null;

            for (int i = 0;
                 i < runtimeTargets.Count;
                 i++)
            {
                ScannerTopologyConnectionTarget target =
                    runtimeTargets[i];

                if (target == null)
                    continue;

                if (target.SourceMesh == mesh)
                    return target;
            }

            return null;
        }

        private void CreateRuntimeTarget(
            SignalPathMesh mesh)
        {
            if (mesh == null)
                return;

            GameObject targetObject =
                new GameObject(
                    $"__ScannerTopology_{mesh.name}");

            targetObject.transform.SetParent(
                transform,
                false);

            ScannerTopologyConnectionTarget target =
                targetObject.AddComponent<
                    ScannerTopologyConnectionTarget>();

            target.Initialize(
                mesh,
                topologyMaterial);

            runtimeTargets.Add(target);

            Debug.Log(
                $"[ScannerTopology] Overlay created for REAL wire: " +
                $"{mesh.name}",
                mesh);
        }

        private void ApplyProgress()
        {
            SignalPath[] paths =
                pathManager != null
                    ? pathManager.Paths
                    : null;

            if (paths == null ||
                paths.Length == 0)
            {
                return;
            }

            /*
             * IMPORTANT:
             *
             * The order here follows the actual
             * SignalPath_Manager.Paths array.
             *
             * This is only the temporary Effect 10
             * reconstruction order.
             *
             * Later we will replace this with
             * CircuitTerminal topology order.
             */

            int count = paths.Length;

            for (int i = 0;
                 i < paths.Length;
                 i++)
            {
                SignalPath path = paths[i];

                if (path == null)
                    continue;

                SignalPathMesh mesh =
                    pathManager.GetMeshForPath(path);

                if (mesh == null)
                    continue;

                ScannerTopologyConnectionTarget target =
                    FindTarget(mesh);

                if (target == null)
                    continue;

                float start =
                    (float)i / count;

                float end =
                    (float)(i + 1) / count;

                float localProgress =
                    Mathf.InverseLerp(
                        start,
                        end,
                        progress);

                target.SetProgress(
                    localProgress);

                target.SetVisible(
                    progress >= start);
            }
        }

        private void RemoveNullTargets()
        {
            for (int i = runtimeTargets.Count - 1;
                 i >= 0;
                 i--)
            {
                if (runtimeTargets[i] == null)
                    runtimeTargets.RemoveAt(i);
            }
        }

        private void DisableAllTargets()
        {
            for (int i = 0;
                 i < runtimeTargets.Count;
                 i++)
            {
                ScannerTopologyConnectionTarget target =
                    runtimeTargets[i];

                if (target == null)
                    continue;

                target.SetProgress(0f);
                target.SetVisible(false);
            }
        }
    }
}