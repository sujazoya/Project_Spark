// ============================================================================
// WorkbenchInteraction.cs
// ============================================================================

using UnityEngine;

namespace ProjectSpark.Gameplay.Flashlight
{
    public sealed class WorkbenchInteraction : MonoBehaviour
    {
        [SerializeField]
        InspectionCamera inspectionCamera;

        [SerializeField]
        Transform workbenchCenter;

        public void FocusWorkbench()
        {
            inspectionCamera.Focus(workbenchCenter);
        }
    }
}