using UnityEngine;
using UnityEngine.Rendering;
using AAAUI.VFX;

namespace ProjectSpark.Scanner
{
    [DisallowMultipleComponent]
    public sealed class ScannerFaultPathTarget
        : MonoBehaviour
    {
        private static readonly int FaultActiveID =
            Shader.PropertyToID("_FaultActive");

        private static readonly int FaultPositionID =
            Shader.PropertyToID("_FaultPosition");

        private static readonly int FaultSeverityID =
            Shader.PropertyToID("_FaultSeverity");

        private SignalPathMesh sourceMesh;

        private MeshRenderer overlayRenderer;
        private MaterialPropertyBlock propertyBlock;

        public SignalPathMesh SourceMesh =>
            sourceMesh;

        public void Initialize(
            SignalPathMesh mesh,
            Material material)
        {
            sourceMesh = mesh;

            if (sourceMesh == null ||
                material == null)
            {
                return;
            }

            MeshRenderer sourceRenderer =
                sourceMesh.MeshRenderer;

            if (sourceRenderer == null)
                return;

            propertyBlock =
                new MaterialPropertyBlock();

            GameObject overlay =
                new GameObject(
                    "__ScannerFaultPath");

            overlay.transform.SetParent(
                sourceRenderer.transform,
                false);

            overlay.transform.localPosition =
                Vector3.zero;

            overlay.transform.localRotation =
                Quaternion.identity;

            overlay.transform.localScale =
                Vector3.one;

            MeshFilter filter =
                overlay.AddComponent<MeshFilter>();

            filter.sharedMesh =
                sourceMesh.GeneratedMesh;

            overlayRenderer =
                overlay.AddComponent<MeshRenderer>();

            overlayRenderer.sharedMaterial =
                material;

            overlayRenderer.shadowCastingMode =
                ShadowCastingMode.Off;

            overlayRenderer.receiveShadows =
                false;

            overlayRenderer.enabled =
                false;
        }

        public void SetFault(
            bool active,
            float position,
            float severity)
        {
            if (overlayRenderer == null)
                return;

            position =
                Mathf.Clamp01(position);

            severity =
                Mathf.Clamp01(severity);

            overlayRenderer.GetPropertyBlock(
                propertyBlock);

            propertyBlock.SetFloat(
                FaultActiveID,
                active ? 1f : 0f);

            propertyBlock.SetFloat(
                FaultPositionID,
                position);

            propertyBlock.SetFloat(
                FaultSeverityID,
                severity);

            overlayRenderer.SetPropertyBlock(
                propertyBlock);

            overlayRenderer.enabled =
                active;
        }

        public void ClearFault()
        {
            if (overlayRenderer == null)
                return;

            overlayRenderer.GetPropertyBlock(
                propertyBlock);

            propertyBlock.SetFloat(
                FaultActiveID,
                0f);

            overlayRenderer.SetPropertyBlock(
                propertyBlock);

            overlayRenderer.enabled =
                false;
        }
    }
}