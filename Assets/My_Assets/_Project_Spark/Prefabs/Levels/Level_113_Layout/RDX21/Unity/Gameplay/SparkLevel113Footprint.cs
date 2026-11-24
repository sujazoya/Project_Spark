using UnityEngine;
using UnityEngine.EventSystems;

namespace ProjectSpark.Gameplay.RedundancyCitadel
{
    public sealed class SparkLevel113Footprint : MonoBehaviour, IPointerClickHandler
    {
        public SparkLevel113Process level;
        public int index;

        public void OnPointerClick(PointerEventData eventData)
        {
            if (eventData.button == PointerEventData.InputButton.Left) level.SelectFootprint(index);
        }
    }
}
