// ============================================================================
// InspectionMode.cs
// ============================================================================

using UnityEngine;

namespace ProjectSpark.Gameplay.Flashlight
{
    public sealed class InspectionMode : MonoBehaviour
    {
        [SerializeField]
        UnityEngine.Camera gameplayCamera;

        [SerializeField]
        UnityEngine.Camera inspectionCamera;

        public void Enter()
        {
            gameplayCamera.enabled = false;
            inspectionCamera.enabled = true;
        }

        public void Exit()
        {
            gameplayCamera.enabled = true;
            inspectionCamera.enabled = false;
        }
    }
}