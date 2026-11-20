using UnityEngine;
using UnityEngine.EventSystems;

namespace ProjectSpark.Gameplay
{
    public sealed class SparkLevel100Footprint : MonoBehaviour, IPointerClickHandler
    {
        public SparkLevel100Process level;
        public int index;

        public void OnPointerClick(PointerEventData eventData)
        {
            if (eventData.button == PointerEventData.InputButton.Left) level.SelectFootprint(index);
        }
    }
}
