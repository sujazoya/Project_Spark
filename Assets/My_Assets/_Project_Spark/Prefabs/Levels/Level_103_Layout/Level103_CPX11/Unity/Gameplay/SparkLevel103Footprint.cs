using UnityEngine;
using UnityEngine.EventSystems;

namespace ProjectSpark.Gameplay.PowerCore
{
    public sealed class SparkLevel103Footprint : MonoBehaviour, IPointerClickHandler
    {
        public SparkLevel103Process level;
        public int index;

        public void OnPointerClick(PointerEventData eventData)
        {
            if (eventData.button == PointerEventData.InputButton.Left) level.SelectFootprint(index);
        }
    }
}
