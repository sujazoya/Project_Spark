using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using ProjectSpark.Circuit;
using ProjectSpark.Gameplay.Shelf;

namespace ProjectSpark.Gameplay
{
    /// <summary>Safely dismantles burned resistors AFTER shelf removal and circuit validation.</summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Collider))]
    public sealed class SparkRecycleBin : MonoBehaviour
    {
        [SerializeField] private SparkEconomyService economy;
        [SerializeField] private SparkObjectShelf shelf;
        [SerializeField] private SparkCircuitSystem circuitSystem;
        [SerializeField, Min(0)] private int coinReward = 5;
        [SerializeField] private bool recycleOnTriggerEnter = true;
        [SerializeField] private AudioSource successSound;
        [SerializeField] private ParticleSystem successParticles;
        [SerializeField] private UnityEvent onAccepted;
        [SerializeField] private UnityEvent onRejected;

        [SerializeField]
        private SparkLevelShelfController levelShelfController;
        private readonly List<SparkCircuitConnection> buffer = new List<SparkCircuitConnection>(8);
        private readonly HashSet<int> processing = new HashSet<int>();

        private void Awake()
        {
            Resolve();
            if (!GetComponent<Collider>().isTrigger)
                Debug.LogWarning("[Recycle] Collider should have Is Trigger enabled.", this);
        }

        private void Resolve()
        {
            if (economy == null) economy = SparkEconomyService.Instance;
            if (shelf == null) shelf = FindFirstObjectByType<SparkObjectShelf>();
            if (circuitSystem == null) circuitSystem = FindFirstObjectByType<SparkCircuitSystem>();
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!recycleOnTriggerEnter || other == null) return;
            SparkResistor resistor = other.GetComponentInParent<SparkResistor>();
            if (resistor != null) TryRecycle(resistor);
        }

        // Call from an inventory / selected-component recycling button if physical
        // shelf items cannot travel into the trigger volume.
        public bool TryRecycleSelected(SparkShelfItem item)
        {
            if (item == null) return false;
            SparkResistor resistor = item.GetComponentInChildren<SparkResistor>(true);
            return TryRecycle(resistor);
        }

        public bool TryRecycle(SparkResistor resistor)
        {
            Resolve();
            if (resistor == null || !resistor.IsBurned) return Reject("Only burned resistors may be recycled.");
            if (economy == null || shelf == null || circuitSystem == null) return Reject("Economy, shelf or circuit missing.");
            SparkShelfItem item = resistor.GetComponentInParent<SparkShelfItem>();
            if (item == null) return Reject("Resistor is not a registered shelf item.");
            if (processing.Contains(item.GetInstanceID())) return false;
            if (item.IsMoving) return Reject("Wait until item movement finishes.");
            if (item.IsAtWorkbench) return Reject("Return resistor to storage before recycling.");
            if (!shelf.CanUnregisterItem(item)) return Reject("Item is not safely registered for removal.");
            SparkTerminal[] terminals = item.GetComponentsInChildren<SparkTerminal>(true);
            foreach (SparkTerminal terminal in terminals)
            {
                if (terminal == null) continue;
                buffer.Clear();
                circuitSystem.GetConnections(terminal, buffer);
                foreach (SparkCircuitConnection connection in buffer)
                    if (connection != null && connection.IsValid)
                        return Reject("Disconnect ALL component wires before recycling.");
            }
            if (!shelf.TryUnregisterItem(item)) return Reject("Shelf cleanup failed.");

                        if (levelShelfController == null)
            {
                levelShelfController =
                    FindFirstObjectByType<SparkLevelShelfController>();
            }

            if (levelShelfController != null)
            {
                levelShelfController.UnregisterPurchasedItem(item);
            }
            // The payout follows successful unregistration, not merely trigger detection.
            if (coinReward > 0 && !economy.TryGrantRecycleCoins(coinReward))
                Debug.LogWarning("[Recycle] Item disposed, but wallet could not accept reward.", this);
            processing.Add(item.GetInstanceID());
            if (successSound != null) successSound.Play();
            if (successParticles != null) successParticles.Play();
            onAccepted?.Invoke();
            GameObject objectToDestroy = item.gameObject;
            objectToDestroy.SetActive(false);
            Destroy(objectToDestroy);
            return true;
        }

        private bool Reject(string reason)
        {
            Debug.LogWarning("[Recycle] " + reason, this);
            onRejected?.Invoke();
            return false;
        }
    }
}
