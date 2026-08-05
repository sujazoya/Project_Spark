using UnityEngine;

namespace AAAUI
{
    [CreateAssetMenu(fileName = "UIAnimationProfile", menuName = "Project Spark/UI Animation Profile")]
    public sealed class UIAnimationProfile : ScriptableObject
    {
        [SerializeReference] private UIAnimationSequence openSequence = new UIAnimationSequence();
        [SerializeReference] private UIAnimationSequence closeSequence = new UIAnimationSequence();
        [SerializeReference] private UIAnimationSequence loopSequence;

        [Header("Shared Defaults")]
        [SerializeField, Min(0f)] private float defaultDuration = 0.2f;
        [SerializeField] private UIEase defaultEase = UIEase.EaseOut;

        public UIAnimationSequence OpenSequence => openSequence;
        public UIAnimationSequence CloseSequence => closeSequence;
        public UIAnimationSequence LoopSequence => loopSequence;
        public float DefaultDuration => defaultDuration;
        public UIEase DefaultEase => defaultEase;

        public UIAnimationSequence GetSequence(UIAnimationSequenceType type)
        {
            switch (type)
            {
                case UIAnimationSequenceType.Close: return closeSequence;
                case UIAnimationSequenceType.Loop: return loopSequence;
                default: return openSequence;
            }
        }
    }
}