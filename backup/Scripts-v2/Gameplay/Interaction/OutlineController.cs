using UnityEngine;

namespace ProjectSpark.Gameplay.Interaction
{
    public sealed class OutlineController : MonoBehaviour
    {
        [SerializeField]
        private Behaviour outline;

        public void Show()
        {
            outline.enabled = true;
        }

        public void Hide()
        {
            outline.enabled = false;
        }
    }
}
