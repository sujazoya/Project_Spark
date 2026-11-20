using UnityEngine;
using UnityEngine.EventSystems;

namespace ProjectSpark.Gameplay.MemoryMatrix
{
    public sealed class SparkLevel104Footprint : MonoBehaviour, IPointerClickHandler
    {
        public SparkLevel104Process level;
        public int index;

        public void OnPointerClick(PointerEventData eventData)
        {
            if (eventData.button == PointerEventData.InputButton.Left) level.SelectFootprint(index);
        }
    }
}
