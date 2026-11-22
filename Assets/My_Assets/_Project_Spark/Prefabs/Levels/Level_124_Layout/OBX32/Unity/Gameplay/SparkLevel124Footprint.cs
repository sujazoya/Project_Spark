using UnityEngine;
using UnityEngine.EventSystems;

namespace ProjectSpark.Gameplay.OrbitalBastion
{
    public sealed class SparkLevel124Footprint : MonoBehaviour, IPointerClickHandler
    {
        public SparkLevel124Process level;
        public int index;

        public void OnPointerClick(PointerEventData eventData)
        {
            if (eventData.button == PointerEventData.InputButton.Left) level.SelectFootprint(index);
        }
    }
}
