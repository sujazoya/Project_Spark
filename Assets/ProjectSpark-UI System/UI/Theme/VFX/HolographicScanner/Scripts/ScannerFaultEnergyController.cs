using System.Collections.Generic;
using UnityEngine;
using AAAUI.VFX;

namespace ProjectSpark.Scanner
{
    [DisallowMultipleComponent]
    public sealed class ScannerFaultEnergyController
        : MonoBehaviour
    {
        [Header("Wire System")]
        [SerializeField]
        private SignalPath_Manager pathManager;

        [Header("Effect 15")]
        [SerializeField]
        private Material energyMaterial;

        private readonly List<ScannerFaultEnergyTarget>
            targets =
                new List<ScannerFaultEnergyTarget>();

        private void Awake()
        {
            RefreshPaths();
        }

        public void RefreshPaths()
        {
            if (pathManager == null ||
                energyMaterial == null)
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
                    $"__ScannerFaultEnergy_{mesh.name}");

            targetObject.transform.SetParent(
                transform,
                false);

            ScannerFaultEnergyTarget target =
                targetObject.AddComponent<
                    ScannerFaultEnergyTarget>();

            target.Initialize(
                mesh,
                energyMaterial);

            targets.Add(target);
        }

        private ScannerFaultEnergyTarget FindTarget(
            SignalPathMesh mesh)
        {
            for (int i = 0;
                 i < targets.Count;
                 i++)
            {
                ScannerFaultEnergyTarget target =
                    targets[i];

                if (target == null)
                    continue;

                if (target.SourceMesh == mesh)
                    return target;
            }

            return null;
        }

        public void SetFaultEnergy(
            SignalPath path,
            bool active,
            float position,
            float severity,
            float energy)
        {
            if (path == null)
                return;

            RefreshPaths();

            SignalPathMesh mesh =
                pathManager.GetMeshForPath(path);

            if (mesh == null)
                return;

            ScannerFaultEnergyTarget target =
                FindTarget(mesh);

            if (target == null)
                return;

            target.SetEnergy(
                active,
                position,
                severity,
                energy);
        }

        public void ClearFaultEnergy(
            SignalPath path)
        {
            if (path == null)
                return;

            SignalPathMesh mesh =
                pathManager.GetMeshForPath(path);

            if (mesh == null)
                return;

            ScannerFaultEnergyTarget target =
                FindTarget(mesh);

            if (target == null)
                return;

            target.ClearEnergy();
        }

        public void ClearAll()
        {
            for (int i = 0;
                 i < targets.Count;
                 i++)
            {
                ScannerFaultEnergyTarget target =
                    targets[i];

                if (target == null)
                    continue;

                target.ClearEnergy();
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