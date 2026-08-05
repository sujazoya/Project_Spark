using TMPro;
using UnityEngine;

namespace ProjectSpark.UI
{
    public sealed class DiagnosticsPanel
        : UIScreen
    {
        [SerializeField]
        private TMP_Text mode;

        public void SetMode(
            string value)
        {
            mode.text = value;
        }
    }
}
