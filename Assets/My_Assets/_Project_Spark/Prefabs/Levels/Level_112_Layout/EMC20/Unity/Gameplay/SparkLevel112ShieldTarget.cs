using UnityEngine;
using UnityEngine.EventSystems;

namespace ProjectSpark.Gameplay.EmcGauntlet
{
    public sealed class SparkLevel112ShieldTarget : MonoBehaviour, IPointerClickHandler
    {
        public SparkLevel112Process board;
        public int index;

        public void OnPointerClick(PointerEventData eventData)
        {
            if (eventData.button == PointerEventData.InputButton.Left) board.InspectShield(index);
        }
    }
}
