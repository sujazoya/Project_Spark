using UnityEngine;
using UnityEngine.EventSystems;

namespace ProjectSpark.Gameplay.EmcGauntlet
{
    public sealed class SparkLevel112Footprint : MonoBehaviour, IPointerClickHandler
    {
        public SparkLevel112Process level;
        public int index;

        public void OnPointerClick(PointerEventData eventData)
        {
            if (eventData.button == PointerEventData.InputButton.Left) level.SelectFootprint(index);
        }
    }
}
