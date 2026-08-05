// ============================================================================
// SolderingController.cs
// ============================================================================

using UnityEngine;

namespace ProjectSpark.Gameplay.Flashlight
{
    public sealed class SolderingController : MonoBehaviour
    {
        [SerializeField]
        float solderTime=2f;

        float timer;

        bool soldering;

        public bool Completed {get;private set;}

        public void Begin()
        {
            soldering=true;
            timer=0;
        }

        void Update()
        {
            if(!soldering)
                return;

            timer+=Time.deltaTime;

            if(timer>=solderTime)
            {
                soldering=false;
                Completed=true;
            }
        }
    }
}