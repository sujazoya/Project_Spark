using UnityEngine;
using ProjectSpark.HolographicViewer;

namespace ProjectSpark.Gameplay
{
    /// <summary>
    /// Identifies a GameObject as a selectable Project Spark object.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class SparkSelectable : MonoBehaviour
    {
        [SerializeField]
        private HolographicObjectVisualState visualState;

        public Transform ObjectTransform
        {
            get { return transform; }
        }

        private void Awake()
        {
            if (visualState == null)
            {
                visualState =
                    GetComponent<HolographicObjectVisualState>();
            }
        }

        public void SetSelected(bool selected)
        {
            if (visualState == null)
            {
                return;
            }

            // Temporary connection to the existing visual system.
            visualState.SetHover(
                selected ? 1f : 0f);
        }
    }
}