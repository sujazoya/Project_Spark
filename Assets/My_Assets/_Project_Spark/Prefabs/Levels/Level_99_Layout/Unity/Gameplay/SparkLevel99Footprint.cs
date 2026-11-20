using UnityEngine;
using UnityEngine.EventSystems;

namespace ProjectSpark.Gameplay
{
    public sealed class SparkLevel99Footprint : MonoBehaviour, IPointerClickHandler
    {
        public SparkLevel99Process level;
        public int index;

        public void OnPointerClick(PointerEventData eventData)
        {
            if (eventData.button == PointerEventData.InputButton.Left && level &&
                (level.Stage == 0 || level.Stage == 4))
                level.SelectFootprint(index);
        }
    }
}
