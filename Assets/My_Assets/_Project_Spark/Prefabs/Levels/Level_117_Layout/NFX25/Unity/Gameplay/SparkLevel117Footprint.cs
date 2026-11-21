using UnityEngine;
using UnityEngine.EventSystems;

namespace ProjectSpark.Gameplay.PacketCitadel
{
    public sealed class SparkLevel117Footprint : MonoBehaviour, IPointerClickHandler
    {
        public SparkLevel117Process level;
        public int index;

        public void OnPointerClick(PointerEventData eventData)
        {
            if (eventData.button == PointerEventData.InputButton.Left) level.SelectFootprint(index);
        }
    }
}
