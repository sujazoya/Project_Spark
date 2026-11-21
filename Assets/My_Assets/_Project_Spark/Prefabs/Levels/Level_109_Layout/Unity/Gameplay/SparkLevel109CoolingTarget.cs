using UnityEngine;
using UnityEngine.EventSystems;

namespace ProjectSpark.Gameplay.ThermalEndurance
{
    public sealed class SparkLevel109CoolingTarget : MonoBehaviour, IPointerClickHandler
    {
        public SparkLevel109Process board;
        public bool cpu;

        public void OnPointerClick(PointerEventData eventData)
        {
            if (eventData.button == PointerEventData.InputButton.Left) board.InspectCooling(cpu);
        }
    }
}
