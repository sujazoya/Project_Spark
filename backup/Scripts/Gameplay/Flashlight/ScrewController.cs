// ============================================================================
// ScrewController.cs
// ============================================================================

using UnityEngine;

namespace ProjectSpark.Gameplay.Flashlight
{
    public sealed class ScrewController : MonoBehaviour
    {
        [SerializeField]
        Transform screw;

        [SerializeField]
        float liftDistance = .02f;

        bool removed;

        public bool Removed => removed;

        void OnMouseDown()
        {
            if (removed)
                return;

            if (!ToolManager.Instance.IsSelected(ToolType.Screwdriver))
                return;

            removed = true;

            screw.position += Vector3.up * liftDistance;
        }
    }
}