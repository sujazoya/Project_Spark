using UnityEngine;

namespace ProjectSpark.UI.Core
{
    public abstract class PSUIScreenController
        : MonoBehaviour
    {
        protected virtual void Awake()
        {
            Initialize();
        }

        protected virtual void Initialize()
        {
        }

        public virtual void Open()
        {
        }

        public virtual void Close()
        {
        }
    }
}