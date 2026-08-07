using UnityEngine;
using UnityEngine.VFX;

namespace ProjectSpark.Hologram
{
    public sealed class HologramVFXController : MonoBehaviour
    {
        [SerializeField]
        private VisualEffect visualEffect;

        [Header("VFX Properties")]
        [SerializeField]
        private string playEvent = "OnScan";

        [SerializeField]
        private string stopEvent = "OnStop";

        [SerializeField]
        private string revealProperty = "Reveal";

        [SerializeField]
        private string scanPositionProperty = "ScanPosition";

        private int revealID;
        private int scanPositionID;

        private void Awake()
        {
            if (visualEffect == null)
                return;

            revealID =
                Shader.PropertyToID(revealProperty);

            scanPositionID =
                Shader.PropertyToID(scanPositionProperty);
        }

        public void PlayScan()
        {
            if (visualEffect == null)
                return;

            visualEffect.SendEvent(playEvent);
        }

        public void Stop()
        {
            if (visualEffect == null)
                return;

            visualEffect.SendEvent(stopEvent);
        }

        public void SetReveal(float value)
        {
            if (visualEffect == null)
                return;

            visualEffect.SetFloat(revealID, value);
        }

        public void SetScanPosition(float value)
        {
            if (visualEffect == null)
                return;

            visualEffect.SetFloat(
                scanPositionID,
                value);
        }
    }
}