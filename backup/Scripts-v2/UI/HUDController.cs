using TMPro;
using UnityEngine;

namespace ProjectSpark.UI
{
    public sealed class HUDController
        : MonoBehaviour
    {
        [SerializeField]
        private TMP_Text fpsText;

        [SerializeField]
        private TMP_Text objectiveText;

        public void SetObjective(string text)
        {
            objectiveText.text = text;
        }

        private void Update()
        {
            fpsText.text =
                $"{(1f / Time.deltaTime):0} FPS";
        }
    }
}
