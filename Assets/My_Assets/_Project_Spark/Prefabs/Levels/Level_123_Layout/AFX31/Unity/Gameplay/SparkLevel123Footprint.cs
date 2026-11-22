using UnityEngine;
using UnityEngine.EventSystems;

namespace ProjectSpark.Gameplay.FlightSovereign
{
    public sealed class SparkLevel123Footprint : MonoBehaviour, IPointerClickHandler
    {
        public SparkLevel123Process level;
        public int index;

        public void OnPointerClick(PointerEventData eventData)
        {
            if (eventData.button == PointerEventData.InputButton.Left) level.SelectFootprint(index);
        }
    }
}
