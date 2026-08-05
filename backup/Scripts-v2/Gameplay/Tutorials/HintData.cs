using UnityEngine;

namespace ProjectSpark.Gameplay.Tutorials
{
    [CreateAssetMenu(
        menuName="Project Spark/Hints/Hint")]
    public sealed class HintData
        : ScriptableObject
    {
        [TextArea]

        public string Text;

        public Sprite Image;
    }
}
