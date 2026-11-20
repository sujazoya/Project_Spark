using UnityEngine;
using UnityEngine.EventSystems;

namespace ProjectSpark.Gameplay.SignalPhantom
{
    public sealed class SparkLevel102Footprint : MonoBehaviour, IPointerClickHandler
    {
        public SparkLevel102Process level;
        public int index;

        public void OnPointerClick(PointerEventData eventData)
        {
            if (eventData.button == PointerEventData.InputButton.Left) level.SelectFootprint(index);
        }
    }
}
