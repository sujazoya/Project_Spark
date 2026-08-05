using UnityEngine;

namespace ProjectSpark.Core.SaveSystem
{
    public sealed class AutoSaveSystem : MonoBehaviour
    {
        [SerializeField]
        private float interval = 120f;

        [SerializeField]
        private SaveGame autoSave;

        private float timer;

        public SaveManager SaveManager;

        private void Update()
        {
            if (SaveManager == null || autoSave == null)
                return;

            timer += Time.deltaTime;

            if (timer < interval)
                return;

            timer = 0f;

            SaveManager.Save(autoSave);

        }
    }
}