using UnityEngine;
using ProjectSpark.Gameplay.Electronics;

namespace ProjectSpark.Presentation.Electronics
{
    public abstract class ElectronicComponentView
        : MonoBehaviour
    {
        public ElectronicComponent Component
        {
            get;
            private set;
        }

        public virtual void Initialize(
            ElectronicComponent component)
        {
            Component = component;
        }

        public virtual void Tick()
        {

        }
    }
}
