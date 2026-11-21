using UnityEngine;
using UnityEngine.EventSystems;

namespace ProjectSpark.Gameplay.MotionCitadel
{
    public sealed class SparkLevel119Footprint : MonoBehaviour, IPointerClickHandler
    {
        public SparkLevel119Process level;
        public int index;

        public void OnPointerClick(PointerEventData eventData)
        {
            if (eventData.button == PointerEventData.InputButton.Left) level.SelectFootprint(index);
        }
    }
}
