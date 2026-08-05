using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ProjectSpark.UI.Components
{
    public sealed class PS_StatusBadge : MonoBehaviour
    {
        [SerializeField]
        private Image background;

        [SerializeField]
        private TMP_Text label;

        public UIStatusType CurrentStatus
        {
            get;
            private set;
        }

        public void SetStatus(UIStatusType status)
        {
            CurrentStatus = status;

            if (label != null)
            {
                label.text = GetLabel(status);
            }

            if (background != null)
            {
                background.color = GetColor(status);
            }
        }

        private static string GetLabel(
            UIStatusType status)
        {
            return status switch
            {
                UIStatusType.Success => "COMPLETE",
                UIStatusType.Active => "ACTIVE",
                UIStatusType.Warning => "WARNING",
                UIStatusType.Error => "ERROR",
                UIStatusType.Inactive => "INACTIVE",
                _ => "UNKNOWN"
            };
        }

        private static Color GetColor(
            UIStatusType status)
        {
            return status switch
            {
                UIStatusType.Success =>
                    new Color(0.2f, 0.85f, 0.45f),

                UIStatusType.Active =>
                    new Color(0.0f, 0.75f, 1.0f),

                UIStatusType.Warning =>
                    new Color(1.0f, 0.65f, 0.15f),

                UIStatusType.Error =>
                    new Color(1.0f, 0.25f, 0.25f),

                UIStatusType.Inactive =>
                    new Color(0.45f, 0.48f, 0.50f),

                _ =>
                    Color.white
            };
        }
    }
}