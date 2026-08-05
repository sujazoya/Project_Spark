// ============================================================================
// Assets/My_Assets/_Project_Spark/Scripts/Gameplay/Flashlight/Interactable.cs
// ============================================================================

using UnityEngine;

namespace ProjectSpark.Gameplay.Flashlight
{
    public abstract class Interactable : MonoBehaviour
    {
        [SerializeField]
        protected bool interactable = true;

        public virtual bool CanInteract()
        {
            return interactable;
        }

        public abstract void Interact();
    }
}