using TMPro;
using UnityEngine;

namespace ProjectSpark.UI.Components
{
    public enum UIStatusType
    {
        Ready,
        Active,
        Inactive,
        Success,
        Warning,
        Error,
        Locked,
        Disabled
    }

    public sealed class PSStatusBadge :
        MonoBehaviour
    {
        [SerializeField]
        private TMP_Text label;

        public void SetStatus(
            UIStatusType status)
        {
            if (label == null)
            {
                return;
            }

            label.text =
                status.ToString()
                    .ToUpperInvariant();
        }
    }
}