using UnityEngine;

namespace ProjectSpark.UI.Theme
{
    public sealed class UIThemeManager :
        MonoBehaviour
    {
        public static UIThemeManager Instance
        {
            get;
            private set;
        }

        [SerializeField]
        private ProjectSparkTheme theme;

        public ProjectSparkTheme Theme =>
            theme;

        private void Awake()
        {
            if (Instance != null &&
                Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
        }
    }
}