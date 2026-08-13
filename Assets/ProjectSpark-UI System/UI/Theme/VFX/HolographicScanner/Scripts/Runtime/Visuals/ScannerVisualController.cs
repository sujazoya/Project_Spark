using UnityEngine;
using UnityEngine.VFX;

namespace ProjectSpark.Scanner
{
    public sealed class ScannerVisualController : MonoBehaviour
    {
        [Header("VFX")]
        [SerializeField] private VisualEffect scanVfx;
        [SerializeField] private VisualEffect faultVfx;

        [Header("Shader")]
        [SerializeField] private Renderer hologramRenderer;
        [SerializeField] private Renderer boardRenderer;
        [SerializeField] private string scanIntensityProperty = "_ScanIntensity";
        [SerializeField] private string faultIntensityProperty = "_FaultIntensity";

        private MaterialPropertyBlock block;

        private void Awake()
        {
            block = new MaterialPropertyBlock();
        }

        public void SetScanning(bool enabled)
        {
            SetVfxBool(scanVfx, "IsScanning", enabled);
            SetFloat(hologramRenderer, scanIntensityProperty, enabled ? 1f : 0f);
        }

        public void SetReconstructing(bool enabled)
        {
            SetVfxBool(scanVfx, "IsReconstructing", enabled);
        }

        public void SetFaultState(bool enabled)
        {
            SetVfxBool(faultVfx, "IsFault", enabled);
            SetFloat(boardRenderer, faultIntensityProperty, enabled ? 1f : 0f);
        }

        public void SetScanHeight(float normalized)
        {
            SetVfxFloat(scanVfx, "ScanHeight", Mathf.Clamp01(normalized));
        }

        private void SetFloat(Renderer target, string propertyName, float value)
        {
            if (target == null)
                return;

            block.Clear();
            target.GetPropertyBlock(block);
            block.SetFloat(Shader.PropertyToID(propertyName), value);
            target.SetPropertyBlock(block);
        }

        private static void SetVfxBool(VisualEffect target, string name, bool value)
        {
            if (target != null && target.HasBool(name))
                target.SetBool(name, value);
        }

        private static void SetVfxFloat(VisualEffect target, string name, float value)
        {
            if (target != null && target.HasFloat(name))
                target.SetFloat(name, value);
        }
    }
}
