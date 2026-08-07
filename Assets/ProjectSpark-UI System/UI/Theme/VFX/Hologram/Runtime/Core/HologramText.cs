using TMPro;
using UnityEngine;

namespace ProjectSpark.Hologram
{
    public sealed class HologramText : MonoBehaviour
    {
        [Header("Text")]
        [SerializeField]
        private TMP_Text titleText;

        [SerializeField]
        private TMP_Text typeText;

        [SerializeField]
        private TMP_Text statusText;

        [SerializeField]
        private TMP_Text informationText;

        [SerializeField]
        private TMP_Text messageText;

        [Header("Material")]
        [SerializeField]
        private Renderer textRenderer;

        [SerializeField]
        private string revealProperty = "_Reveal";

        [SerializeField]
        private string scanPositionProperty = "_ScanPosition";

        private Material material;

        private int revealID;
        private int scanPositionID;

        private void Awake()
        {
            if (textRenderer != null)
            {
                material = textRenderer.material;

                revealID =
                    Shader.PropertyToID(revealProperty);

                scanPositionID =
                    Shader.PropertyToID(scanPositionProperty);
            }
        }

        public void Show(HologramData data)
        {
            if (data == null)
                return;

            if (titleText != null)
                titleText.text = data.title;

            if (typeText != null)
                typeText.text = data.type;

            if (statusText != null)
                statusText.text =
                    $"STATUS  //  {data.status}";

            if (informationText != null)
            {
                informationText.text =
                    BuildInformation(data);
            }

            if (messageText != null)
                messageText.text = data.message;

            gameObject.SetActive(true);
        }

        public void ShowMessage(HologramMessage message)
        {
            if (messageText == null)
                return;

            messageText.text =
                $"[{message.type.ToString().ToUpperInvariant()}]\n" +
                message.text;
        }

        public void SetReveal(float value)
        {
            if (material != null)
                material.SetFloat(revealID, value);
        }

        public void SetScanPosition(float value)
        {
            if (material != null)
                material.SetFloat(scanPositionID, value);
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }

        private string BuildInformation(HologramData data)
        {
            string result = data.description;

            if (!string.IsNullOrEmpty(data.id))
                result += $"\nID       // {data.id}";

            if (!string.IsNullOrEmpty(data.value))
                result += $"\nVALUE    // {data.value}";

            if (!string.IsNullOrEmpty(data.polarity))
                result += $"\nPOLARITY // {data.polarity}";

            if (!string.IsNullOrEmpty(data.voltage))
                result += $"\nVOLTAGE  // {data.voltage}";

            if (!string.IsNullOrEmpty(data.current))
                result += $"\nCURRENT  // {data.current}";

            if (!string.IsNullOrEmpty(data.signal))
                result += $"\nSIGNAL   // {data.signal}";

            return result;
        }
    }
}