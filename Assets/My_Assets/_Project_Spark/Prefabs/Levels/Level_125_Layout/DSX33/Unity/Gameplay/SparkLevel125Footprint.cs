using UnityEngine;
using UnityEngine.EventSystems;

namespace ProjectSpark.Gameplay.SignalHorizon
{
    public sealed class SparkLevel125Footprint : MonoBehaviour, IPointerClickHandler
    {
        public SparkLevel125Process level;
        public int index;

        public void OnPointerClick(PointerEventData eventData)
        {
            if (eventData.button == PointerEventData.InputButton.Left) level.SelectFootprint(index);
        }
    }
}
