using UnityEngine;

namespace ProjectSpark.Presentation.Electronics
{
    public sealed class ComponentViewUpdater
        : MonoBehaviour
    {
        [SerializeField]
        ElectronicComponentView view;

        [SerializeField]
        PowerGlowController glow;

        [SerializeField]
        HeatEffectController heat;

        [SerializeField]
        ComponentAnimator animator;

        void Update()
        {
            var c=view.Component;

            if(c==null)
                return;

            glow.SetPowered(
                c.State.IsPowered);

            heat.UpdateHeat(
                c.State.Temperature);

            animator.Powered(
                c.State.IsPowered);
        }
    }
}
