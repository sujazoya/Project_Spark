using UnityEngine;
using UnityEngine.EventSystems;

namespace ProjectSpark.Gameplay.DeterministicGrid
{
    public sealed class SparkLevel118Footprint : MonoBehaviour, IPointerClickHandler
    {
        public SparkLevel118Process level;
        public int index;

        public void OnPointerClick(PointerEventData eventData)
        {
            if (eventData.button == PointerEventData.InputButton.Left) level.SelectFootprint(index);
        }
    }
}
