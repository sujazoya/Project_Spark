using UnityEngine;
using UnityEngine.EventSystems;

namespace ProjectSpark.Gameplay.AxisForge
{
    public sealed class SparkLevel120Footprint : MonoBehaviour, IPointerClickHandler
    {
        public SparkLevel120Process level;
        public int index;

        public void OnPointerClick(PointerEventData eventData)
        {
            if (eventData.button == PointerEventData.InputButton.Left) level.SelectFootprint(index);
        }
    }
}
