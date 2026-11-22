using UnityEngine;
using UnityEngine.EventSystems;

namespace ProjectSpark.Gameplay.EclipseCore
{
    public sealed class SparkLevel126Footprint : MonoBehaviour, IPointerClickHandler
    {
        public SparkLevel126Process level;
        public int index;

        public void OnPointerClick(PointerEventData eventData)
        {
            if (eventData.button == PointerEventData.InputButton.Left) level.SelectFootprint(index);
        }
    }
}
