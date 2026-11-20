using UnityEngine;
using UnityEngine.EventSystems;
namespace ProjectSpark.Gameplay
{
    public sealed class SparkLevel107Footprint : MonoBehaviour, IPointerClickHandler
    {
        public SparkLevel107Process level;
        public int index;
        public void OnPointerClick(PointerEventData eventData) => level.SelectFootprint(index);
    }
}
