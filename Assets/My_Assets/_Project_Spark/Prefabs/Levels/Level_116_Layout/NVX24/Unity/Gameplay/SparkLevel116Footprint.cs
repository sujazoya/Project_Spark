using UnityEngine;
using UnityEngine.EventSystems;

namespace ProjectSpark.Gameplay.DataPhoenix
{
    public sealed class SparkLevel116Footprint : MonoBehaviour, IPointerClickHandler
    {
        public SparkLevel116Process level;
        public int index;

        public void OnPointerClick(PointerEventData eventData)
        {
            if (eventData.button == PointerEventData.InputButton.Left) level.SelectFootprint(index);
        }
    }
}
