using TMPro;
using UnityEngine;

namespace ProjectSpark.Gameplay.Interaction
{
    public sealed class TooltipController : MonoBehaviour
    {
        [SerializeField]
        private TMP_Text title;

        [SerializeField]
        private TMP_Text description;

        public void Show(
            string componentName,
            string info)
        {
            gameObject.SetActive(true);

            title.text = componentName;

            description.text = info;
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }
    }
}
