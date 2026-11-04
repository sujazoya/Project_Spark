using UnityEngine;
using ProjectSpark.Gameplay.Shelf;

namespace ProjectSpark.Gameplay
{
    /// <summary>Drop target for recycling an unconnected, registered shelf component.</summary>
    [DisallowMultipleComponent]
    public sealed class SparkRecycleBinTarget : MonoBehaviour
    {
        [SerializeField] private bool acceptAllCategories = true;
        [SerializeField] private SparkShelfCategory acceptedCategory = SparkShelfCategory.Any;
        [SerializeField] private bool acceptFromWorkstation = true;
        [SerializeField] private bool acceptFromStorage = true;
        [SerializeField] private ParticleSystem recycleEffect;
        [SerializeField] private AudioSource recycleSound;

        public bool CanAccept(SparkShelfItem item)
        {
            if (!isActiveAndEnabled || !gameObject.activeInHierarchy || item == null)
                return false;
            if (!acceptAllCategories && acceptedCategory != SparkShelfCategory.Any &&
                item.Category != acceptedCategory) return false;
            if (item.IsAtWorkbench) return acceptFromWorkstation;
            if (item.IsInStorage) return acceptFromStorage;
            return false; // Unregistered or unplaced objects are never disposable via this bin.
        }

        public void PlayAcceptedEffect()
        {
            if (recycleEffect != null) recycleEffect.Play();
            if (recycleSound != null) recycleSound.Play();
        }

        private void Reset()
        {
            // This collider must be detectable by Physics.RaycastAll(... Ignore).
            Collider c = GetComponent<Collider>();
            if (c != null) c.isTrigger = false;
        }
    }
}
