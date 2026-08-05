using System.Collections.Generic;
using ProjectSpark.UI.Core;
using UnityEngine;
using ProjectSpark.UI.Gameplay;

namespace ProjectSpark.UI.Screens
{
    public sealed class LevelSelectScreen :
        UIScreen
    {
        [Header("Levels")]

        [SerializeField]
        private List<LevelSelectData> levels =
            new List<LevelSelectData>();

        [SerializeField]
        private LevelCardUI cardPrefab;

        [SerializeField]
        private Transform cardContainer;

        [Header("Briefing")]

        [SerializeField]
        private LevelBriefingScreen briefingScreen;

        private readonly List<LevelCardUI> cards =
            new List<LevelCardUI>();

        private int selectedIndex = -1;

        protected override void OnOpen()
        {
            base.OnOpen();

            BuildCards();
        }

        private void BuildCards()
        {
            ClearCards();

            for (int i = 0;
                 i < levels.Count;
                 i++)
            {
                LevelSelectData level =
                    levels[i];

                LevelCardUI card =
                    Instantiate(
                        cardPrefab,
                        cardContainer);

                // Temporary default state.
                // Real progression state will come
                // from LevelManager / SaveManager.
                bool unlocked =
                    i == 0;

                bool completed =
                    false;

                card.Bind(
                    level,
                    unlocked,
                    completed);

                cards.Add(card);
            }
        }

        private void ClearCards()
        {
            for (int i = 0;
                 i < cards.Count;
                 i++)
            {
                if (cards[i] != null)
                {
                    Destroy(
                        cards[i].gameObject);
                }
            }

            cards.Clear();

            selectedIndex = -1;
        }

        public void SelectLevel(
            int index)
        {
            if (index < 0 ||
                index >= levels.Count)
            {
                return;
            }

            if (selectedIndex >= 0 &&
                selectedIndex < cards.Count)
            {
                cards[selectedIndex]
                    .SetSelected(false);
            }

            selectedIndex = index;

            cards[selectedIndex]
                .SetSelected(true);
        }

        public void OpenSelectedLevel()
        {
            if (selectedIndex < 0 ||
                selectedIndex >= levels.Count)
            {
                return;
            }

            LevelSelectData selected =
                levels[selectedIndex];

            if (briefingScreen == null)
            {
                Debug.LogError(
                    "Level Select requires " +
                    "a Level Briefing Screen.",
                    this);

                return;
            }

            briefingScreen.SetLevel(
                selected);

            UIManager.Instance.ShowScreen(
                UIScreenIds.LevelBriefing);
        }

        public void BackToMainMenu()
        {
            UIManager.Instance.ShowScreen(
                UIScreenIds.MainMenu);
        }
    }
}