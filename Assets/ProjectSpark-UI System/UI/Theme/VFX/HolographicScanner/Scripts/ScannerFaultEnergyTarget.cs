using UnityEngine;
using UnityEngine.Rendering;
using AAAUI.VFX;

namespace ProjectSpark.Scanner
{
    [DisallowMultipleComponent]
    public sealed class ScannerFaultEnergyTarget
        : MonoBehaviour
    {
        private static readonly int FaultEnergyActiveID =
            Shader.PropertyToID("_FaultEnergyActive");

        private static readonly int FaultPositionID =
            Shader.PropertyToID("_FaultPosition");

        private static readonly int FaultSeverityID =
            Shader.PropertyToID("_FaultSeverity");

        private static readonly int FaultEnergyID =
            Shader.PropertyToID("_FaultEnergy");

        private SignalPathMesh sourceMesh;

        private MeshFilter overlayFilter;
        private MeshRenderer overlayRenderer;

        private MaterialPropertyBlock propertyBlock;

        public SignalPathMesh SourceMesh =>
            sourceMesh;

        public void Initialize(
            SignalPathMesh mesh,
            Material energyMaterial)
        {
            sourceMesh = mesh;

            if (sourceMesh == null ||
                energyMaterial == null)
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
                    "__ScannerFaultEnergy");

            overlay.transform.SetParent(
                sourceRenderer.transform,
                false);

            overlay.transform.localPosition =
                Vector3.zero;

            overlay.transform.localRotation =
                Quaternion.identity;

            overlay.transform.localScale =
                Vector3.one;

            overlay.layer =
                sourceRenderer.gameObject.layer;

            overlayFilter =
                overlay.AddComponent<MeshFilter>();

            overlayFilter.sharedMesh =
                sourceMesh.GeneratedMesh;

            overlayRenderer =
                overlay.AddComponent<MeshRenderer>();

            overlayRenderer.sharedMaterial =
                energyMaterial;

            overlayRenderer.shadowCastingMode =
                ShadowCastingMode.Off;

            overlayRenderer.receiveShadows =
                false;

            overlayRenderer.motionVectorGenerationMode =
                MotionVectorGenerationMode.ForceNoMotion;

            overlayRenderer.enabled =
                false;
        }

        public void SetEnergy(
            bool active,
            float position,
            float severity,
            float energy)
        {
            if (overlayRenderer == null)
                return;

            position =
                Mathf.Clamp01(position);

            severity =
                Mathf.Clamp01(severity);

            energy =
                Mathf.Clamp01(energy);

            overlayRenderer.GetPropertyBlock(
                propertyBlock);

            propertyBlock.SetFloat(
                FaultEnergyActiveID,
                active ? 1f : 0f);

            propertyBlock.SetFloat(
                FaultPositionID,
                position);

            propertyBlock.SetFloat(
                FaultSeverityID,
                severity);

            propertyBlock.SetFloat(
                FaultEnergyID,
                energy);

            overlayRenderer.SetPropertyBlock(
                propertyBlock);

            overlayRenderer.enabled =
                active && energy > 0.001f;
        }

        public void ClearEnergy()
        {
            if (overlayRenderer == null)
                return;

            overlayRenderer.GetPropertyBlock(
                propertyBlock);

            propertyBlock.SetFloat(
                FaultEnergyActiveID,
                0f);

            propertyBlock.SetFloat(
                FaultEnergyID,
                0f);

            overlayRenderer.SetPropertyBlock(
                propertyBlock);

            overlayRenderer.enabled =
                false;
        }
    }
}