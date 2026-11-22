using UnityEngine;
using UnityEngine.EventSystems;

namespace ProjectSpark.Gameplay.AutonomySentinel
{
    public sealed class SparkLevel122Footprint : MonoBehaviour, IPointerClickHandler
    {
        public SparkLevel122Process level;
        public int index;

        public void OnPointerClick(PointerEventData eventData)
        {
            if (eventData.button == PointerEventData.InputButton.Left) level.SelectFootprint(index);
        }
    }
}
