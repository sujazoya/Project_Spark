using UnityEngine;

namespace ProjectSpark.UI.Theme
{
    public abstract class UIThemeElement :
        MonoBehaviour
    {
        protected ProjectSparkTheme Theme
        {
            get
            {
                if (UIThemeManager.Instance ==
                    null)
                {
                    return null;
                }

                return UIThemeManager.Instance
                    .Theme;
            }
        }

        protected virtual void OnEnable()
        {
            ApplyTheme();
        }

        protected abstract void ApplyTheme();
    }
}