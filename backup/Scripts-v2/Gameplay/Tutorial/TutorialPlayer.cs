using UnityEngine;

namespace ProjectSpark.Gameplay.Tutorial
{
    public sealed class TutorialPlayer
    {
        private readonly TutorialDefinition _definition;

        private readonly TutorialRuntime _runtime =
            new();

        public TutorialPlayer(
            TutorialDefinition definition)
        {
            _definition = definition;
        }

        public TutorialStepDefinition Current
        {
            get
            {
                if (_runtime.CurrentStep >=
                    _definition.Steps.Count)
                    return null;

                return _definition.Steps[
                    _runtime.CurrentStep];
            }
        }

        public void Next()
        {
            _runtime.CurrentStep++;

            if (_runtime.CurrentStep >=
                _definition.Steps.Count)
            {
                _runtime.Finished = true;
            }
        }
    }
}
