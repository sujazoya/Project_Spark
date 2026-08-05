using TMPro;
using UnityEngine;

namespace ProjectSpark.UI
{
    public sealed class ToolPanel
        : UIScreen
    {
        [SerializeField]
        private TMP_Text toolName;

        public void SetTool(
            string value)
        {
            toolName.text = value;
        }
    }
}
