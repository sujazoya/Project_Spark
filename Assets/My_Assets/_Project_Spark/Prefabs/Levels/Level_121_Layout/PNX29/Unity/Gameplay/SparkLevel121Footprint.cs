using UnityEngine;
using UnityEngine.EventSystems;

namespace ProjectSpark.Gameplay.PerceptionNexus
{
    public sealed class SparkLevel121Footprint : MonoBehaviour, IPointerClickHandler
    {
        public SparkLevel121Process level;
        public int index;

        public void OnPointerClick(PointerEventData eventData)
        {
            if (eventData.button == PointerEventData.InputButton.Left) level.SelectFootprint(index);
        }
    }
}
