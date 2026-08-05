// ============================================================================
// ResistorController.cs
// ============================================================================

using UnityEngine;

namespace ProjectSpark.Gameplay.Flashlight
{
    public sealed class ResistorController : MonoBehaviour
    {
        [SerializeField]
        bool burnt=true;

        public bool IsInstalled { get; private set; }

        public bool IsBurnt => burnt;

        public void Remove()
        {
            IsInstalled=false;

            gameObject.SetActive(false);
        }

        public void InstallReplacement(GameObject replacement)
        {
            Instantiate(
                replacement,
                transform.position,
                transform.rotation,
                transform.parent);

            burnt=false;

            IsInstalled=true;
        }
    }
}