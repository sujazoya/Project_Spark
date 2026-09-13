using UnityEngine;
using UnityEngine.EventSystems;

namespace ProjectSpark.UI.Animation
{
    public sealed class UIButtonClickAnimation :
        MonoBehaviour,
        IPointerClickHandler
    {
        [SerializeField]
        private UIButtonAnimator animator;

        private void Reset()
        {
            animator =
                GetComponent<UIButtonAnimator>();
        }

        public void OnPointerClick(
            PointerEventData eventData)
        {
           // if (animator != null)
               // animator.PlayPunch();
        }
    }
}