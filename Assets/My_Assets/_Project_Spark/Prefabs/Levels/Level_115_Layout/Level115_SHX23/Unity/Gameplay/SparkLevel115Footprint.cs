using UnityEngine;
using UnityEngine.EventSystems;

namespace ProjectSpark.Gameplay.SecureEnclave
{
    public sealed class SparkLevel115Footprint : MonoBehaviour, IPointerClickHandler
    {
        public SparkLevel115Process level;
        public int index;

        public void OnPointerClick(PointerEventData eventData)
        {
            if (eventData.button == PointerEventData.InputButton.Left) level.SelectFootprint(index);
        }
    }
}
