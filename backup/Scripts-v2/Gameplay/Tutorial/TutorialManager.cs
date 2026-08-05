using UnityEngine;

namespace ProjectSpark.Gameplay.Tutorial
{
    public sealed class TutorialManager
        : MonoBehaviour
    {
        [SerializeField]
        TutorialDefinition tutorial;

        private TutorialPlayer _player;

        void Start()
        {
            _player =
                new TutorialPlayer(
                    tutorial);
        }

        public TutorialStepDefinition
            CurrentStep =>
            _player.Current;

        public void CompleteCurrentStep()
        {
            _player.Next();
        }
    }
}
