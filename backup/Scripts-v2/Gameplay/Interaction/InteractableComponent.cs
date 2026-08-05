using UnityEngine;

namespace ProjectSpark.Gameplay.Interaction
{
    [RequireComponent(typeof(Collider))]
    public class InteractableComponent :
        MonoBehaviour,
        IInteractable
    {
        [SerializeField]
        private OutlineController outline;

        public virtual bool CanInteract => true;

        public virtual void HoverEnter()
        {
            outline?.Show();
        }

        public virtual void HoverExit()
        {
            outline?.Hide();
        }

        public virtual void Select()
        {
        }

        public virtual void Deselect()
        {
        }

        public virtual void Interact()
        {
        }
    }
}
