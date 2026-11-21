using UnityEngine;
using UnityEngine.EventSystems;

namespace ProjectSpark.Gameplay.ThermalEndurance
{
    public sealed class SparkLevel109Footprint : MonoBehaviour, IPointerClickHandler
    {
        public SparkLevel109Process level;
        public int index;

        public void OnPointerClick(PointerEventData eventData)
        {
            if (eventData.button == PointerEventData.InputButton.Left) level.SelectFootprint(index);
        }
    }
}
