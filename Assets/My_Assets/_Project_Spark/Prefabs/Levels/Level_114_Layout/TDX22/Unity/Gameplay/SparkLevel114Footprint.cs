using UnityEngine;
using UnityEngine.EventSystems;

namespace ProjectSpark.Gameplay.ThermalDominion
{
    public sealed class SparkLevel114Footprint : MonoBehaviour, IPointerClickHandler
    {
        public SparkLevel114Process level;
        public int index;

        public void OnPointerClick(PointerEventData eventData)
        {
            if (eventData.button == PointerEventData.InputButton.Left) level.SelectFootprint(index);
        }
    }
}
