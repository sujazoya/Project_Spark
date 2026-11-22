using UnityEngine;
using UnityEngine.EventSystems;

namespace ProjectSpark.Gameplay.ThermalAegis
{
    public sealed class SparkLevel127Footprint : MonoBehaviour, IPointerClickHandler
    {
        public SparkLevel127Process level;
        public int index;

        public void OnPointerClick(PointerEventData eventData)
        {
            if (eventData.button == PointerEventData.InputButton.Left) level.SelectFootprint(index);
        }
    }
}
