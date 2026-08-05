using UnityEngine;

namespace ProjectSpark.UI
{
    public abstract class UIWindow
        : UIScreen
    {
        [SerializeField]
        private bool pauseGame;

        public override void Open()
        {
            base.Open();

            if (pauseGame)
                Time.timeScale = 0;
        }

        public override void Close()
        {
            base.Close();

            if (pauseGame)
                Time.timeScale = 1;
        }
    }
}
