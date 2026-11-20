using UnityEngine;
using UnityEngine.EventSystems;

namespace ProjectSpark.Gameplay.ExpansionNexus
{
    public sealed class SparkLevel105CardInspector : MonoBehaviour, IPointerClickHandler
    {
        public SparkLevel105Process level;

        public void OnPointerClick(PointerEventData eventData)
        {
            if (eventData.button == PointerEventData.InputButton.Left && level.Stage >= 2) level.InspectSlot();
        }
    }
}
