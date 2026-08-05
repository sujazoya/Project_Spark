using UnityEngine;

namespace ProjectSpark.UI.Core
{
    /// <summary>
    /// Controls whether UI blocks gameplay input.
    /// </summary>
    public sealed class UIInputBlocker : MonoBehaviour
    {
        [SerializeField]
        private GameObject blockerVisual;

        public UIInputState State { get; private set; }
            = UIInputState.Gameplay;

        public bool IsBlocking =>
            State == UIInputState.UIOnly;

        public void SetState(UIInputState state)
        {
            State = state;

            if (blockerVisual != null)
            {
                blockerVisual.SetActive(
                    IsBlocking);
            }
        }
    }
}