using UnityEngine;
using UnityEngine.EventSystems;

namespace ProjectSpark.Gameplay
{
    public sealed class SparkLevel110Footprint : MonoBehaviour, IPointerClickHandler
    {
        public SparkLevel110Process level;
        public int index;

        public void OnPointerClick(PointerEventData pointer)
        {
            if (pointer.button == PointerEventData.InputButton.Left) level.SelectFootprint(index);
        }
    }
}
