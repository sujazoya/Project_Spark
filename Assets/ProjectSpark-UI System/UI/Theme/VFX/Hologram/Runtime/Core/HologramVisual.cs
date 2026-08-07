using UnityEngine;

namespace ProjectSpark.Hologram
{
    public sealed class HologramVisual : MonoBehaviour
    {
        [Header("References")]
        [SerializeField]
        private Renderer surfaceRenderer;

        [SerializeField]
        private HologramText hologramText;

        [SerializeField]
        private HologramVFXController vfx;

        [Header("Shader Properties")]
        [SerializeField]
        private string revealProperty = "_Reveal";

        [SerializeField]
        private string scanPositionProperty = "_ScanPosition";

        [SerializeField]
        private string glitchProperty = "_Glitch";

        private Material material;

        private int revealID;
        private int scanPositionID;
        private int glitchID;

        private void Awake()
        {
            if (surfaceRenderer != null)
            {
                material = surfaceRenderer.material;

                revealID = Shader.PropertyToID(revealProperty);
                scanPositionID = Shader.PropertyToID(scanPositionProperty);
                glitchID = Shader.PropertyToID(glitchProperty);
            }

            Hide();
        }

        public void Show(HologramTarget target)
        {
            if (target == null)
                return;

            transform.position =
                target.ProjectionPoint.position;

            transform.rotation =
                target.ProjectionPoint.rotation;

            if (hologramText != null)
                hologramText.Show(target.Data);

            if (vfx != null)
                vfx.PlayScan();

            SetReveal(0f);
            SetGlitch(0f);

            gameObject.SetActive(true);
        }

        public void Hide()
        {
            if (hologramText != null)
                hologramText.Hide();

            if (vfx != null)
                vfx.Stop();

            gameObject.SetActive(false);
        }

        public void SetReveal(float value)
        {
            value = Mathf.Clamp01(value);

            if (material != null)
                material.SetFloat(revealID, value);

            if (hologramText != null)
                hologramText.SetReveal(value);

            if (vfx != null)
                vfx.SetReveal(value);
        }

        public void SetScanPosition(float value)
        {
            if (material != null)
                material.SetFloat(scanPositionID, value);

            if (hologramText != null)
                hologramText.SetScanPosition(value);
        }

        public void SetGlitch(float value)
        {
            if (material != null)
                material.SetFloat(glitchID, value);
        }

        public void ShowMessage(HologramMessage message)
        {
            if (hologramText != null)
                hologramText.ShowMessage(message);
        }
    }
}