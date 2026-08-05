using UnityEngine;

namespace ProjectSpark.Gameplay.Electronics
{
    public abstract class ElectronicComponent : MonoBehaviour
    {
        [SerializeField]
        private string componentId;

        [SerializeField]
        private ComponentType componentType;

        [SerializeField]
        private ComponentPin[] pins;

        public string Id => componentId;

        public ComponentType Type => componentType;

        public ComponentPin[] Pins => pins;

        public ComponentState State { get; }
            = new ComponentState();           
          
            public GameObject Prefab; 
            public Sprite Icon; public 
            float DefaultVoltage; public 
            float DefaultResistance; public 
            float MaxCurrent; 

        public virtual void Initialize()
        {
        }

        public virtual void Simulate(float deltaTime)
        {
        }

        public virtual void ResetComponent()
        {
            State.IsPowered = false;
            State.IsActive = false;
            State.IsBroken = false;
            State.Voltage = 0f;
            State.Current = 0f;
            State.Temperature = 20f;
        }
        public virtual void Simulate() { } 
    }
    
}

