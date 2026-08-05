using UnityEngine;

namespace ProjectSpark.UI.Navigation
{
    public abstract class PopupBase :
        MonoBehaviour
    {
        [SerializeField]
        private string popupId;

        public string PopupId =>
            popupId;

        public virtual void Initialize()
        {
        }

        public virtual void Open()
        {
            gameObject.SetActive(true);
        }

        public virtual void Close()
        {
            gameObject.SetActive(false);
        }
    }
}