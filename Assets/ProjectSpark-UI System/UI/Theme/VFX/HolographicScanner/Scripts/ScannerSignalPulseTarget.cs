using UnityEngine;
using UnityEngine.Rendering;
using AAAUI.VFX;

namespace ProjectSpark.Scanner
{
    [DisallowMultipleComponent]
    public sealed class ScannerSignalPulseTarget : MonoBehaviour
    {
        private static readonly int PulsePositionID =
            Shader.PropertyToID("_PulsePosition");

        private static readonly int PulseActiveID =
            Shader.PropertyToID("_PulseActive");

        private static readonly int PulseDirectionID =
            Shader.PropertyToID("_PulseDirection");

        private static readonly int PulseIntensityID =
            Shader.PropertyToID("_PulseIntensity");

        private SignalPathMesh sourceMesh;
        private MeshFilter overlayFilter;
        private MeshRenderer overlayRenderer;

        private MaterialPropertyBlock propertyBlock;

        public SignalPathMesh SourceMesh =>
            sourceMesh;

        public void Initialize(
            SignalPathMesh mesh,
            Material pulseMaterial)
        {
            sourceMesh = mesh;

            if (sourceMesh == null)
                return;

            if (pulseMaterial == null)
                return;

            MeshRenderer sourceRenderer =
                sourceMesh.MeshRenderer;

            if (sourceRenderer == null)
                return;

            propertyBlock =
                new MaterialPropertyBlock();

            GameObject overlay =
                new GameObject(
                    "__ScannerPulseOverlay");

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
                pulseMaterial;

            overlayRenderer.shadowCastingMode =
                ShadowCastingMode.Off;

            overlayRenderer.receiveShadows =
                false;

            overlayRenderer.motionVectorGenerationMode =
                MotionVectorGenerationMode.ForceNoMotion;

            overlayRenderer.enabled =
                false;
        }

        public void SetPulse(
            float position,
            float direction,
            float intensity)
        {
            if (overlayRenderer == null)
                return;

            position =
                Mathf.Clamp01(position);

            direction =
                Mathf.Clamp(
                    direction,
                    -1f,
                    1f);

            intensity =
                Mathf.Max(
                    0f,
                    intensity);

            overlayRenderer.GetPropertyBlock(
                propertyBlock);

            propertyBlock.SetFloat(
                PulsePositionID,
                position);

            propertyBlock.SetFloat(
                PulseDirectionID,
                direction);

            propertyBlock.SetFloat(
                PulseIntensityID,
                intensity);

            propertyBlock.SetFloat(
                PulseActiveID,
                1f);

            overlayRenderer.SetPropertyBlock(
                propertyBlock);

            overlayRenderer.enabled =
                intensity > 0f;
        }

        public void StopPulse()
        {
            if (overlayRenderer == null)
                return;

            overlayRenderer.GetPropertyBlock(
                propertyBlock);

            propertyBlock.SetFloat(
                PulseActiveID,
                0f);

            overlayRenderer.SetPropertyBlock(
                propertyBlock);

            overlayRenderer.enabled =
                false;
        }
    }
}