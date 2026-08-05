using UnityEngine;

namespace ProjectSpark.Gameplay.Tutorials
{
    public sealed class HighlightTarget
        : MonoBehaviour
    {
        [SerializeField]
        private string targetID;

        public string TargetID => targetID;
    }
}
