using ProjectSpark.UI.Core;
using UnityEngine;

namespace ProjectSpark.UI.Screens
{
    /// <summary>
    /// Base class for screens that receive
    /// presentation data before being shown.
    /// </summary>
    public abstract class UIDataScreen<T> : UIScreen
    {
        protected T Data
        {
            get;
            private set;
        }

        public void SetData(T data)
        {
            Data = data;

            OnDataReceived(data);
        }

        protected virtual void OnDataReceived(
            T data)
        {
        }
    }
}