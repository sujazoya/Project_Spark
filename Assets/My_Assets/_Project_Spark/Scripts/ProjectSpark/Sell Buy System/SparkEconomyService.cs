using System;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectSpark.Gameplay
{
    /// <summary>Local offline prototype economy. Real-money grants must NOT call this directly.</summary>
    [DisallowMultipleComponent]
    public sealed class SparkEconomyService : MonoBehaviour
    {
        [Serializable] private sealed class Entry { public string id; public int count; }
        [Serializable] private sealed class SaveData
        {
            public int coins;
            public int diamonds;
            public List<Entry> inventory = new List<Entry>();
            public List<string> rewardedLevelIds = new List<string>();
        }

        [SerializeField] private SparkEconomyCatalog catalog;
        [SerializeField, Min(0)] private int startingCoins = 250;
        [SerializeField, Min(0)] private int startingDiamonds = 5;
        [SerializeField] private bool persistAcrossScenes = true;
        [SerializeField] private string saveKey = "ProjectSpark.Economy.v1";

        private SaveData data;
        public static SparkEconomyService Instance { get; private set; }
        public SparkEconomyCatalog Catalog => catalog;
        public int Coins => data != null ? data.coins : 0;
        public int Diamonds => data != null ? data.diamonds : 0;
        public event Action Changed;

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            if (persistAcrossScenes) DontDestroyOnLoad(gameObject);
            Load();
        }

        private void OnDestroy() { if (Instance == this) Instance = null; }

        private void Load()
        {
            data = null;
            if (PlayerPrefs.HasKey(saveKey))
            {
                try { data = JsonUtility.FromJson<SaveData>(PlayerPrefs.GetString(saveKey)); }
                catch (Exception ex) { Debug.LogWarning("[Economy] Could not read save: " + ex.Message, this); }
            }
            if (data == null) data = new SaveData { coins = startingCoins, diamonds = startingDiamonds };
            if (data.inventory == null) data.inventory = new List<Entry>();
            if (data.rewardedLevelIds == null) data.rewardedLevelIds = new List<string>();
            data.coins = Mathf.Max(0, data.coins);
            data.diamonds = Mathf.Max(0, data.diamonds);
            Changed?.Invoke();
        }

        private void Save()
        {
            PlayerPrefs.SetString(saveKey, JsonUtility.ToJson(data));
            PlayerPrefs.Save();
            Changed?.Invoke();
        }

        public int GetItemCount(string itemId)
        {
            foreach (Entry entry in data.inventory)
                if (entry.id == itemId) return entry.count;
            return 0;
        }

        private Entry GetOrCreateEntry(string id)
        {
            foreach (Entry entry in data.inventory) if (entry.id == id) return entry;
            Entry created = new Entry { id = id, count = 0 };
            data.inventory.Add(created);
            return created;
        }

        public bool TryPurchase(string itemId, bool useDiamonds, out string message)
        {
            message = "Item unavailable.";
            if (catalog == null) return false;
            SparkEconomyCatalog.Item item = catalog.Find(itemId);
            if (item == null || item.prefab == null) return false;
            if (useDiamonds ? !item.sellForDiamonds : !item.sellForCoins)
            { message = "This payment method is unavailable."; return false; }
            int price = useDiamonds ? item.diamondPrice : item.coinPrice;
            if (price < 0) return false;
            if (useDiamonds ? data.diamonds < price : data.coins < price)
            { message = useDiamonds ? "Not enough diamonds." : "Not enough coins."; return false; }
            Entry entry = GetOrCreateEntry(itemId);
            if (entry.count == int.MaxValue) { message = "Inventory full."; return false; }
            if (useDiamonds) data.diamonds -= price;
            else data.coins -= price;
            entry.count++;
            Save();
            message = "Purchased " + item.displayName + ". Open your inventory to place it.";
            return true;
        }

        public bool TryConsumeItem(string itemId)
        {
            foreach (Entry entry in data.inventory)
            {
                if (entry.id != itemId || entry.count <= 0) continue;
                entry.count--;
                Save();
                return true;
            }
            return false;
        }

        public bool TryGrantRecycleCoins(int amount)
        {
            if (amount <= 0 || data.coins > int.MaxValue - amount) return false;
            data.coins += amount;
            Save();
            return true;
        }

        /// <summary>Call ONLY after the existing level manager confirms a permanent win.</summary>
        public bool TryAwardLevelCompletion(string levelId, int coinReward, int diamondReward)
        {
            if (string.IsNullOrWhiteSpace(levelId) || coinReward < 0 || diamondReward < 0) return false;
            if (data.rewardedLevelIds.Contains(levelId)) return false;
            if (data.coins > int.MaxValue - coinReward || data.diamonds > int.MaxValue - diamondReward) return false;
            data.coins += coinReward;
            data.diamonds += diamondReward;
            data.rewardedLevelIds.Add(levelId);
            Save();
            return true;
        }

        [ContextMenu("DEBUG / Reset Local Economy Save")]
        private void DebugResetLocalSave()
        {
            if (!Application.isPlaying) return;
            PlayerPrefs.DeleteKey(saveKey);
            Load();
        }
        
public bool TryPurchaseQuantity(
    string itemId,
    bool useDiamonds,
    int quantity,
    out string message)
{
    message = "Item unavailable.";

    if (data == null || catalog == null)
        return false;

    if (quantity <= 0)
    {
        message = "Invalid purchase quantity.";
        return false;
    }

    SparkEconomyCatalog.Item item =
        catalog.Find(itemId);

    if (item == null || item.prefab == null)
        return false;

    if (useDiamonds
        ? !item.sellForDiamonds
        : !item.sellForCoins)
    {
        message = "This payment method is unavailable.";
        return false;
    }

    int unitPrice = useDiamonds
        ? item.diamondPrice
        : item.coinPrice;

    if (unitPrice < 0)
        return false;

    // Use long to prevent integer overflow.
    long totalPrice = (long)unitPrice * quantity;

    long availableCurrency = useDiamonds
        ? data.diamonds
        : data.coins;

    if (totalPrice > availableCurrency)
    {
        message = useDiamonds
            ? "Not enough diamonds."
            : "Not enough coins.";

        return false;
    }

    Entry entry = GetOrCreateEntry(itemId);

    if ((long)entry.count + quantity > int.MaxValue)
    {
        message = "Inventory full.";
        return false;
    }

    // All validation passed.
    // Apply the entire purchase together.
    if (useDiamonds)
        data.diamonds -= (int)totalPrice;
    else
        data.coins -= (int)totalPrice;

    entry.count += quantity;

    // Save once and notify UI once.
    Save();

    message =
        $"Purchased {quantity} × {item.displayName}. " +
        "Open your inventory to place them.";

    return true;
}

    }
}
