// Assets/My_Assets/_Project_Spark/Scripts/Gameplay/Level01/SuccessPanel.cs

using UnityEngine;

namespace ProjectSpark.Gameplay.Level01
{
    public sealed class SuccessPanel : MonoBehaviour
    {
        [SerializeField]
        CanvasGroup panel;

        void Awake()
        {
            Hide();
        }

        public void Show()
        {
            panel.alpha = 1;
            panel.blocksRaycasts = true;
            panel.interactable = true;
        }

        public void Hide()
        {
            panel.alpha = 0;
            panel.blocksRaycasts = false;
            panel.interactable = false;
        }
    }
}