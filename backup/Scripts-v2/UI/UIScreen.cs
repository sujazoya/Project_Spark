using UnityEngine;

namespace ProjectSpark.UI
{
    public abstract class UIScreen : MonoBehaviour
    {
        public virtual void Open()
        {
            gameObject.SetActive(true);
        }

        public virtual void Close()
        {
            gameObject.SetActive(false);
        }

        public virtual void Refresh()
        {
        }
    }
}
