using TMPro;
using UnityEngine;

namespace ProjectSpark.HolographicViewer
{
    public sealed class HolographicInspectionHUD : MonoBehaviour
    {
        [Header("Mode")]
        [SerializeField] private TMP_Text modeStatus;
        [SerializeField] private TMP_Text modeDescription;

        [Header("Object")]
        [SerializeField] private TMP_Text objectName;
        [SerializeField] private TMP_Text objectType;
        [SerializeField] private TMP_Text systemStatus;

        private HolographicInspectionMode currentMode =
            HolographicInspectionMode.Normal;

        private void Start()
        {
            Refresh();
        }

        public void SetMode(
            HolographicInspectionMode mode)
        {
            currentMode = mode;

            Refresh();
        }

        public void SetMode(int mode)
        {
            mode = Mathf.Clamp(mode, 0, 4);

            currentMode =
                (HolographicInspectionMode)mode;

            Refresh();
        }

        private void Refresh()
        {
            if (modeStatus != null)
            {
                modeStatus.text =
                    HolographicInspectionModeInfo
                        .GetTitle(currentMode);
            }

            if (modeDescription != null)
            {
                modeDescription.text =
                    HolographicInspectionModeInfo
                        .GetDescription(currentMode);
            }

            if (objectName != null)
            {
                objectName.text =
                    "DC MOTOR";
            }

            if (objectType != null)
            {
                objectType.text =
                    "MECHANICAL / ELECTROMAGNETIC";
            }

            if (systemStatus != null)
            {
                systemStatus.text =
                    "SYSTEM READY";
            }
        }
    }
}