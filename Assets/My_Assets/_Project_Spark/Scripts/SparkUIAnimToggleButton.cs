using UnityEngine;
using UnityEngine.UI;

namespace ProjectSpark.UI
{
    public class SparkUIAnimToggleButton : MonoBehaviour
    {
       
        [SerializeField] private SparkUIAnimationController animationController;
        [SerializeField] private Toggle toggle;

        private void Awake()
        {
            if (toggle == null)
                toggle = GetComponent<Toggle>();

            toggle.onValueChanged.AddListener(OnToggleChanged);
        }

        private void OnDestroy()
        {
            if (toggle != null)
                toggle.onValueChanged.RemoveListener(OnToggleChanged);
        }

        private void OnToggleChanged(bool isOn)
        {
            if (animationController == null)
                return;

            if (isOn)
                animationController.PlayIn();
            else
                animationController.PlayOut();
        }
    }
}