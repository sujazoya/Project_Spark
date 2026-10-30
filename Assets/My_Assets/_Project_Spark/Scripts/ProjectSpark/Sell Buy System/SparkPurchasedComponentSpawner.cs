using UnityEngine;
using ProjectSpark.Gameplay.Shelf;

namespace ProjectSpark.Gameplay
{
    /// <summary>Moves a saved owned component into a free shelf storage slot.</summary>
    [DisallowMultipleComponent]
    public sealed class SparkPurchasedComponentSpawner : MonoBehaviour
    {
        [SerializeField] private SparkEconomyService economy;
        [SerializeField] private SparkObjectShelf shelf;
        [SerializeField] private Transform stagingPoint;
        [SerializeField] private Transform spawnedItemsParent;
        [SerializeField] private string selectedItemId = "resistor_220";
        [SerializeField]
        private SparkLevelShelfController levelShelfController;

        public void SetSelectedItem(string itemId) { selectedItemId = itemId; }
        public void PlaceSelected() { TryPlaceOwnedItem(selectedItemId); }

        public bool TryPlaceOwnedItem(string itemId)
        {
            if (economy == null) economy = SparkEconomyService.Instance;
            if (shelf == null) shelf = FindFirstObjectByType<SparkObjectShelf>();
            if (economy == null || economy.Catalog == null || shelf == null || stagingPoint == null) return false;
            SparkEconomyCatalog.Item entry = economy.Catalog.Find(itemId);
            if (entry == null || entry.prefab == null || economy.GetItemCount(itemId) == 0) return false;
            if (entry.prefab.GetComponentInChildren<SparkShelfItem>(true) == null)
            {
                Debug.LogError("[Economy] Purchased prefab must contain SparkShelfItem.", this);
                return false;
            }

            GameObject clone = Instantiate(entry.prefab, stagingPoint.position, stagingPoint.rotation, spawnedItemsParent);
            SparkShelfItem item = clone != null ? clone.GetComponentInChildren<SparkShelfItem>(true) : null;
            if (item == null || !shelf.TryRegisterPurchasedItem(item))
            {
                if (clone != null) Destroy(clone);
                Debug.LogWarning("[Economy] No compatible empty storage slot; inventory was not consumed.", this);
                return false;
            }

            if (!economy.TryConsumeItem(itemId))
            {
                shelf.TryUnregisterItem(item);
                Destroy(clone);
                return false;
            }
            // Inform the level inventory about the delivered item.
            if (levelShelfController == null)
            {
                levelShelfController =
                    FindFirstObjectByType<SparkLevelShelfController>();
            }

            if (levelShelfController != null)
            {
                levelShelfController.RegisterPurchasedItem(item);
            }
            else
            {
                Debug.LogWarning(
                    "[Economy] LevelShelfController missing. " +
                    "Purchased item was placed, but manual " +
                    "placement availability was not updated.",
                    this);
            }

            Debug.Log("[Economy] Placed purchased component: " + itemId, clone);
            return true;
        }
    }
}
