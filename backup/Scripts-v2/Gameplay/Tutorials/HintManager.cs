using UnityEngine;

namespace ProjectSpark.Gameplay.Tutorials
{
    public sealed class HintManager
        : MonoBehaviour
    {
        [SerializeField]
        private HintDatabase database;

        public HintData GetHint(
            int index)
        {
            if (index < 0 ||
                index >= database.Hints.Count)
                return null;

            return database.Hints[index];
        }
    }
}
