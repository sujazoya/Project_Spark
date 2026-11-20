using UnityEngine;
using UnityEngine.EventSystems;

namespace ProjectSpark.Gameplay.HiddenLayers
{
    public sealed class SparkLevel101Footprint : MonoBehaviour, IPointerClickHandler
    {
        public SparkLevel101Process level;
        public int index;

        public void OnPointerClick(PointerEventData eventData)
        {
            if (eventData.button == PointerEventData.InputButton.Left) level.SelectFootprint(index);
        }
    }
}
