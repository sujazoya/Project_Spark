using TMPro;
using UnityEngine;

namespace ProjectSpark.Gameplay.Tutorial.UI
{
    public sealed class TutorialPopup
        : MonoBehaviour
    {
        [SerializeField]
        TMP_Text text;

        public void Show(string message)
        {
            gameObject.SetActive(true);

            text.text = message;
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }
    }
}
