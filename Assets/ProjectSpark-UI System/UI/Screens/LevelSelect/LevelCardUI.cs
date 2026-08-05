using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ProjectSpark.UI.Screens
{
    public sealed class LevelCardUI :
        MonoBehaviour
    {
        [Header("Content")]

        [SerializeField]
        private TMP_Text missionNumberText;

        [SerializeField]
        private TMP_Text titleText;

        [SerializeField]
        private TMP_Text descriptionText;

        [SerializeField]
        private Image previewImage;

        [Header("State")]

        [SerializeField]
        private GameObject lockedState;

        [SerializeField]
        private GameObject unlockedState;

        [SerializeField]
        private GameObject completedState;

        [SerializeField]
        private GameObject selectedState;

        private LevelSelectData data;

        public string LevelId =>
            data != null
                ? data.LevelId
                : string.Empty;

        public void Bind(
            LevelSelectData levelData,
            bool unlocked,
            bool completed)
        {
            data = levelData;

            if (data == null)
            {
                return;
            }

            if (missionNumberText != null)
            {
                missionNumberText.text =
                    data.MissionNumber;
            }

            if (titleText != null)
            {
                titleText.text =
                    data.Title;
            }

            if (descriptionText != null)
            {
                descriptionText.text =
                    data.Description;
            }

            if (previewImage != null)
            {
                previewImage.sprite =
                    data.PreviewImage;
            }

            if (lockedState != null)
            {
                lockedState.SetActive(
                    !unlocked);
            }

            if (unlockedState != null)
            {
                unlockedState.SetActive(
                    unlocked);
            }

            if (completedState != null)
            {
                completedState.SetActive(
                    completed);
            }

            if (selectedState != null)
            {
                selectedState.SetActive(
                    false);
            }
        }

        public void SetSelected(
            bool selected)
        {
            if (selectedState != null)
            {
                selectedState.SetActive(
                    selected);
            }
        }
    }
}