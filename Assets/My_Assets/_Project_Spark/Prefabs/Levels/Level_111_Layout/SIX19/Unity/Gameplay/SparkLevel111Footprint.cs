using UnityEngine;
using UnityEngine.EventSystems;

namespace ProjectSpark.Gameplay.SignalCrucible
{
    public sealed class SparkLevel111Footprint : MonoBehaviour, IPointerClickHandler
    {
        public SparkLevel111Process level;
        public int index;

        public void OnPointerClick(PointerEventData eventData)
        {
            if (eventData.button == PointerEventData.InputButton.Left) level.SelectFootprint(index);
        }
    }
}
