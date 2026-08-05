using TMPro;
using UnityEngine;

namespace ProjectSpark.UI
{
    public sealed class ObjectivePanel
        : UIScreen
    {
        [SerializeField]
        private TMP_Text objective;

        public void SetText(
            string text)
        {
            objective.text = text;
        }
    }
}
