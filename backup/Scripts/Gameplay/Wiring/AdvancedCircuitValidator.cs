// ============================================================================
// AdvancedCircuitValidator.cs
// ============================================================================

using UnityEngine;

namespace ProjectSpark.Gameplay.Wiring
{
    public sealed class AdvancedCircuitValidator : MonoBehaviour
    {
        [SerializeField]
        CircuitValidator validator;

        [SerializeField]
        LevelProgression progression;

        bool completed;

        void Update()
        {
            if (completed)
                return;

            if (!validator.Validate())
                return;

            completed = true;

            progression.CompleteLevel();
        }
    }
}