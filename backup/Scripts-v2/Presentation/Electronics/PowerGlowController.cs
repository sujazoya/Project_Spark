using UnityEngine;

namespace ProjectSpark.Presentation.Electronics
{
    public sealed class PowerGlowController
        : MonoBehaviour
    {
        [SerializeField]
        Renderer[] renderers;

        [SerializeField]
        float emissionIntensity = 5;

        public void SetPowered(bool powered)
        {
            foreach(var r in renderers)
            {
                foreach(var m in r.materials)
                {
                    if(powered)
                    {
                        m.EnableKeyword("_EMISSION");

                        m.SetColor(
                            "_EmissionColor",
                            Color.cyan *
                            emissionIntensity);
                    }
                    else
                    {
                        m.SetColor(
                            "_EmissionColor",
                            Color.black);
                    }
                }
            }
        }
    }
}
