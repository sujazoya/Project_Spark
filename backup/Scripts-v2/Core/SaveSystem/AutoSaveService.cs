using UnityEngine;

namespace ProjectSpark.Core.SaveSystem
{
    public sealed class AutoSaveService
        : MonoBehaviour
    {
        [SerializeField]
        private float interval = 120f;

        private float timer;

        private void Update()
        {
            timer += Time.deltaTime;

            if(timer < interval)
                return;

            timer = 0;

            SaveEvents.RaiseAutoSaved();
        }
    }
}
