using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace ProjectSpark.Gameplay.UI
{
    /// <summary>
    /// Displays the Level Complete panel.
    /// </summary>
    public sealed class SuccessPanel : MonoBehaviour
    {
        [Header("Root")]
        [SerializeField]
        private GameObject root;

        [Header("Texts")]
        [SerializeField]
        private TMP_Text titleText;

        [SerializeField]
        private TMP_Text rewardText;

        [SerializeField]
        private TMP_Text coinText;

        [SerializeField]
        private TMP_Text xpText;

        [SerializeField]
        private TMP_Text reputationText;

        [Header("Buttons")]
        [SerializeField]
        private Button continueButton;

        [SerializeField]
        private Button replayButton;

        [Header("Events")]
        [SerializeField]
        private UnityEvent onContinue;

        [SerializeField]
        private UnityEvent onReplay;

        private bool isVisible;

        public bool IsVisible => isVisible;

        private void Awake()
        {
            HideImmediate();

            if (continueButton != null)
                continueButton.onClick.AddListener(ContinueClicked);

            if (replayButton != null)
                replayButton.onClick.AddListener(ReplayClicked);
        }

        private void OnDestroy()
        {
            if (continueButton != null)
                continueButton.onClick.RemoveListener(ContinueClicked);

            if (replayButton != null)
                replayButton.onClick.RemoveListener(ReplayClicked);
        }

        public void Show(
            int coins = 100,
            int xp = 50,
            int reputation = 5)
        {
            isVisible = true;

            if (root != null)
                root.SetActive(true);

            if (titleText != null)
                titleText.text = "REPAIR COMPLETE";

            if (rewardText != null)
                rewardText.text = "Flashlight Successfully Repaired";

            if (coinText != null)
                coinText.text = $"+{coins} Coins";

            if (xpText != null)
                xpText.text = $"+{xp} XP";

            if (reputationText != null)
                reputationText.text = $"+{reputation} Reputation";

            Time.timeScale = 0f;
        }

        public void Hide()
        {
            isVisible = false;

            Time.timeScale = 1f;

            if (root != null)
                root.SetActive(false);
        }

        public void HideImmediate()
        {
            isVisible = false;

            Time.timeScale = 1f;

            if (root != null)
                root.SetActive(false);
        }

        private void ContinueClicked()
        {
            Hide();

            onContinue?.Invoke();
        }

        private void ReplayClicked()
        {
            Hide();

            onReplay?.Invoke();
        }
    }
}