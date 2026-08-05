using TMPro;
using UnityEngine;

namespace ProjectSpark.UI.Gameplay
{
    public sealed class MissionHeaderUI :
        MonoBehaviour
    {
        [SerializeField]
        private TMP_Text missionNumberText;

        [SerializeField]
        private TMP_Text missionTitleText;

        public void SetMission(
            string missionNumber,
            string missionTitle)
        {
            if (missionNumberText != null)
            {
                missionNumberText.text =
                    missionNumber;
            }

            if (missionTitleText != null)
            {
                missionTitleText.text =
                    missionTitle;
            }
        }
    }
}