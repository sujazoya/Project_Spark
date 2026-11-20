using UnityEngine;
using UnityEngine.EventSystems;

namespace ProjectSpark.Gameplay
{
    public sealed class SparkLevel98Footprint : MonoBehaviour, IPointerClickHandler
    {
        public SparkLevel98Process level;
        public int index;

        public void OnPointerClick(PointerEventData eventData)
        {
            if (eventData.button == PointerEventData.InputButton.Left && level &&
                (level.Stage == 0 || level.Stage == 4))
                level.SelectFootprint(index);
        }
    }
}
