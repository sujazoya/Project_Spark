using System.Collections.Generic;
using UnityEngine;
using AAAUI.VFX;

namespace ProjectSpark.Scanner
{
    [DisallowMultipleComponent]
    public sealed class ScannerFaultLocalizationController
        : MonoBehaviour
    {
        [Header("Wire System")]
        [SerializeField]
        private SignalPath_Manager pathManager;

        [Header("Fault Material")]
        [SerializeField]
        private Material faultMaterial;

        private readonly List<ScannerFaultPathTarget>
            pathTargets =
                new List<ScannerFaultPathTarget>();

        private void Awake()
        {
            RefreshPaths();
        }

        public void RefreshPaths()
        {
            if (pathManager == null ||
                faultMaterial == null)
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

                if (FindPathTarget(mesh) != null)
                    continue;

                CreatePathTarget(mesh);
            }
        }

        private void CreatePathTarget(
            SignalPathMesh mesh)
        {
            GameObject targetObject =
                new GameObject(
                    $"__ScannerFault_{mesh.name}");

            targetObject.transform.SetParent(
                transform,
                false);

            ScannerFaultPathTarget target =
                targetObject.AddComponent<
                    ScannerFaultPathTarget>();

            target.Initialize(
                mesh,
                faultMaterial);

            pathTargets.Add(target);
        }

        private ScannerFaultPathTarget FindPathTarget(
            SignalPathMesh mesh)
        {
            for (int i = 0;
                 i < pathTargets.Count;
                 i++)
            {
                ScannerFaultPathTarget target =
                    pathTargets[i];

                if (target == null)
                    continue;

                if (target.SourceMesh == mesh)
                    return target;
            }

            return null;
        }

        public void ShowPathFault(
            SignalPath path,
            float position,
            float severity)
        {
            if (path == null)
                return;

            RefreshPaths();

            SignalPathMesh mesh =
                pathManager.GetMeshForPath(path);

            if (mesh == null)
                return;

            ScannerFaultPathTarget target =
                FindPathTarget(mesh);

            if (target == null)
                return;

            target.SetFault(
                true,
                position,
                severity);
        }

        public void ClearPathFault(
            SignalPath path)
        {
            if (path == null)
                return;

            SignalPathMesh mesh =
                pathManager.GetMeshForPath(path);

            if (mesh == null)
                return;

            ScannerFaultPathTarget target =
                FindPathTarget(mesh);

            if (target == null)
                return;

            target.ClearFault();
        }

        public void ClearAllFaults()
        {
            for (int i = 0;
                 i < pathTargets.Count;
                 i++)
            {
                ScannerFaultPathTarget target =
                    pathTargets[i];

                if (target == null)
                    continue;

                target.ClearFault();
            }
        }

        private void RemoveNullTargets()
        {
            for (int i = pathTargets.Count - 1;
                 i >= 0;
                 i--)
            {
                if (pathTargets[i] == null)
                    pathTargets.RemoveAt(i);
            }
        }
    }
}