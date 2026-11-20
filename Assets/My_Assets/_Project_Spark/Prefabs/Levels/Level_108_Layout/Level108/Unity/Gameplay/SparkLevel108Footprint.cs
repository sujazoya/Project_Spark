using UnityEngine;
using UnityEngine.EventSystems;

namespace ProjectSpark.Gameplay
{
    public sealed class SparkLevel108Footprint : MonoBehaviour, IPointerClickHandler
    {
        public SparkLevel108Process level;
        public int index;
        public void OnPointerClick(PointerEventData eventData) => level.SelectFootprint(index);
    }
}
