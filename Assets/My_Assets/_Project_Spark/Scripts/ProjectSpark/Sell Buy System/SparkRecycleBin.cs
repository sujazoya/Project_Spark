using System.Collections;
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
        [Header("References")]
        [SerializeField] private SparkEconomyService economy;
        [SerializeField] private SparkObjectShelf shelf;
        [SerializeField] private SparkCircuitSystem circuitSystem;
        [SerializeField] private SparkLevelShelfController levelShelfController;

        [Header("Activation")]
        [Tooltip("Whether the recycle bin accepts items when the scene starts.")]
        [SerializeField] private bool startActive = true;
        [SerializeField] private bool disableTriggerWhenInactive = true;
        [SerializeField] private UnityEvent onActivated;
        [SerializeField] private UnityEvent onDeactivated;

        [Header("Recycling")]
        [SerializeField, Min(0)] private int coinReward = 5;
        [SerializeField] private bool recycleOnTriggerEnter = true;
        [Tooltip("Automatically recycle burned registered items once they are returned to a storage slot. Requires all wires disconnected.")]
        [SerializeField] private bool recycleBurnedOnStoragePlacement = false;
        [SerializeField] private AudioSource successSound;
        [SerializeField] private ParticleSystem successParticles;
        [SerializeField] private UnityEvent onAccepted;
        [SerializeField] private UnityEvent onRejected;

        [Header("Burned Resistor: One-Click Fly To Bin")]
        [SerializeField] private Transform dustbinDropPoint;
        [SerializeField, Min(.05f)] private float flyDuration = .75f;
        [SerializeField, Min(0f)] private float arcHeight = .45f;
        [SerializeField] private AnimationCurve flyEase = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
        [SerializeField] private bool autoDisconnectBurnedResistor = true;

        public bool CanAcceptBurnedFlight => isActive;

        private readonly List<SparkCircuitConnection> buffer = new List<SparkCircuitConnection>(8);
        private readonly HashSet<int> processing = new HashSet<int>();
        private Collider triggerCollider;
        private bool isActive;
        private bool subscribed;

        public bool IsRecycleBinActive => isActive;

        private void Awake()
        {
            Resolve();
            triggerCollider = GetComponent<Collider>();
            if (triggerCollider != null && !triggerCollider.isTrigger)
                Debug.LogWarning("[Recycle] Collider should have Is Trigger enabled.", this);
            // Initialize activation without emitting an event before other listeners initialize.
            SetActivationInternal(startActive, false);
        }

        private void OnEnable()
        {
            Resolve();
            SubscribeShelf();
            SetActivationInternal(isActive, false);
        }

        private void OnDisable()
        {
            UnsubscribeShelf();
        }

        private void SubscribeShelf()
        {
            if (subscribed || shelf == null) return;
            shelf.ItemPlaced += OnShelfItemPlaced;
            subscribed = true;
        }

        private void UnsubscribeShelf()
        {
            if (!subscribed) return;
            if (shelf != null) shelf.ItemPlaced -= OnShelfItemPlaced;
            subscribed = false;
        }

        private void OnShelfItemPlaced(SparkShelfItem item, SparkShelfSlot slot)
        {
            if (!isActive || !recycleBurnedOnStoragePlacement ||
                item == null || slot == null ||
                slot.Kind != SparkShelfSlotKind.Storage) return;

            // SparkObjectShelf completes release/CurrentSlot/IsMoving before
            // invoking ItemPlaced, so storage validation is safe here.
            SparkResistor resistor = item.GetComponentInChildren<SparkResistor>(true);
            if (resistor != null && resistor.IsBurned)
                TryRecycleRegisteredBurnedItem(item);
        }

        private void Resolve()
        {
            if (economy == null) economy = SparkEconomyService.Instance;
            if (shelf == null) shelf = FindFirstObjectByType<SparkObjectShelf>();
            if (circuitSystem == null) circuitSystem = FindFirstObjectByType<SparkCircuitSystem>();
            if (levelShelfController == null)
                levelShelfController = FindFirstObjectByType<SparkLevelShelfController>();
            if (isActiveAndEnabled) SubscribeShelf();
        }

        [ContextMenu("Activate Recycle Bin")]
        public void ActivateRecycleBin() => SetRecycleBinActive(true);

        [ContextMenu("Deactivate Recycle Bin")]
        public void DeactivateRecycleBin() => SetRecycleBinActive(false);

        public void SetRecycleBinActive(bool active) => SetActivationInternal(active, true);

        private void SetActivationInternal(bool active, bool notify)
        {
            bool changed = isActive != active;
            isActive = active;
            if (triggerCollider == null) triggerCollider = GetComponent<Collider>();
            if (disableTriggerWhenInactive && triggerCollider != null)
                triggerCollider.enabled = active;
            else if (triggerCollider != null && active)
                triggerCollider.enabled = true;

            if (notify && changed)
            {
                if (active) onActivated?.Invoke();
                else onDeactivated?.Invoke();
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!isActive || !recycleOnTriggerEnter || other == null) return;
            // The collider can belong to a child mesh, terminal, or shelf item.
            SparkShelfItem item = other.GetComponentInParent<SparkShelfItem>();
            if (item != null)
            {
                TryRecycleRegisteredBurnedItem(item);
                return;
            }
            SparkResistor resistor = other.GetComponentInParent<SparkResistor>();
            if (resistor != null) TryRecycle(resistor);
        }

        // Can also be connected to an inventory/selected-component recycling button.
        public bool TryRecycleSelected(SparkShelfItem item)
        {
            if (!isActive) return false;
            if (item == null) return false;
            return TryRecycleRegisteredBurnedItem(item);
        }

        public bool TryRecycle(SparkResistor resistor)
        {
            if (resistor == null) return Reject("No resistor supplied.");
            SparkShelfItem item = resistor.GetComponentInParent<SparkShelfItem>();
            return TryRecycleRegisteredBurnedItem(item);
        }

        // Call this directly from a UI recycle button, or from the shelf event.
        // A registered burned item can be removed only after being returned to
        // storage and fully disconnected from the electrical network.
        public bool TryRecycleRegisteredBurnedItem(SparkShelfItem item)
        {
            if (!isActive) return Reject("Recycle bin is inactive.");
            Resolve();
            if (item == null) return Reject("No registered shelf item supplied.");
            SparkResistor resistor = item.GetComponentInChildren<SparkResistor>(true);
            if (resistor == null || !resistor.IsBurned)
                return Reject("Only burned resistors may be recycled.");
            if (economy == null || shelf == null || circuitSystem == null)
                return Reject("Economy, shelf or circuit missing.");
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
            if (levelShelfController != null)
                levelShelfController.UnregisterPurchasedItem(item);

            if (coinReward > 0 && !economy.TryGrantRecycleCoins(coinReward))
                Debug.LogWarning("[Recycle] Item disposed, but wallet could not accept reward.", this);

            RetireBurnedObjectBindings(item.transform);
            processing.Add(item.GetInstanceID());
            if (successSound != null) successSound.Play();
            if (successParticles != null) successParticles.Play();
            onAccepted?.Invoke();
            GameObject objectToDestroy = item.gameObject;
            objectToDestroy.SetActive(false);
            Destroy(objectToDestroy);
            return true;
        }

        /// <summary>
        /// Called by the floating button above a burned shelf resistor.
        /// Disconnects only this item's wires, frees its slot, and flies it into
        /// the existing scene dustbin before destroying the GameObject.
        /// </summary>
        public bool TryRecycleBurnedWithFlight(SparkShelfItem item)
        {
            Resolve();
            if (!isActive) return Reject("Recycle bin is inactive.");
            if (item == null || shelf == null || circuitSystem == null)
                return Reject("Missing item, shelf, or circuit system.");
            SparkResistor resistor = item.GetComponentInChildren<SparkResistor>(true);
            if (resistor == null || !resistor.IsBurned)
                return Reject("Only burned resistors may be recycled.");
            if (item.IsMoving || processing.Contains(item.GetInstanceID())) return false;

            // Remove the burned component's own terminal connections from the
            // circuit before unregistering it. This uses the existing topology API.
            if (autoDisconnectBurnedResistor)
                circuitSystem.RemoveConnectionsForLayout(item.transform);

            foreach (SparkTerminal terminal in item.GetComponentsInChildren<SparkTerminal>(true))
            {
                if (terminal == null) continue;
                buffer.Clear();
                circuitSystem.GetConnections(terminal, buffer);
                foreach (SparkCircuitConnection connection in buffer)
                    if (connection != null && connection.IsValid)
                        return Reject("Resistor still has wires. Disconnect them before recycling.");
            }

            if (!shelf.TryUnregisterBurnedItemForRecycling(item))
                return Reject("Could not release registered burned resistor slot.");

            RetireBurnedObjectBindings(item.transform);
            processing.Add(item.GetInstanceID());
            if (levelShelfController == null)
                levelShelfController = FindFirstObjectByType<SparkLevelShelfController>();
            if (levelShelfController != null)
                levelShelfController.UnregisterPurchasedItem(item);

            // The object is no longer shelf-owned. Keep it visible for the flight.
            item.transform.SetParent(null, true);
            StartCoroutine(FlyAndRecycle(item));
            return true;
        }

        private IEnumerator FlyAndRecycle(SparkShelfItem item)
        {
            if (item == null) yield break;
            Transform moving = item.transform;
            Vector3 start = moving.position;
            Quaternion startRotation = moving.rotation;
            Vector3 destination = dustbinDropPoint != null
                ? dustbinDropPoint.position : transform.position;
            Rigidbody[] bodies = item.GetComponentsInChildren<Rigidbody>(true);
            foreach (Rigidbody body in bodies)
            {
                if (body == null) continue;
                body.linearVelocity = Vector3.zero;
                body.angularVelocity = Vector3.zero;
                body.isKinematic = true;
                body.useGravity = false;
            }
            foreach (Collider collider in item.GetComponentsInChildren<Collider>(true))
                if (collider != null) collider.enabled = false;

            float elapsed = 0f;
            while (elapsed < flyDuration && item != null)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / Mathf.Max(.05f, flyDuration));
                float eased = flyEase != null ? Mathf.Clamp01(flyEase.Evaluate(t)) : t;
                Vector3 position = Vector3.Lerp(start, destination, eased);
                position += Vector3.up * (4f * arcHeight * t * (1f - t));
                moving.SetPositionAndRotation(position,
                    Quaternion.Slerp(startRotation, dustbinDropPoint != null
                        ? dustbinDropPoint.rotation : transform.rotation, eased));
                yield return null;
            }
            if (item == null) yield break;
            if (successSound != null) successSound.Play();
            if (successParticles != null)
            {
                successParticles.transform.position = destination;
                successParticles.Play();
            }
            if (economy != null && coinReward > 0 && !economy.TryGrantRecycleCoins(coinReward))
                Debug.LogWarning("[Recycle] Resistor disposed, but reward was not granted.", this);
            onAccepted?.Invoke();
            int id = item.GetInstanceID();
            item.gameObject.SetActive(false);
            Destroy(item.gameObject);
            processing.Remove(id);
        }

        /// <summary>
        /// Recycle a burned resistor discovered in the active scene, even when
        /// the prefab was not registered as a shelf inventory item.
        /// </summary>
        public bool TryRecycleBurnedSceneResistor(SparkResistor resistor)
        {
            Resolve();
            if (!isActive || !isActiveAndEnabled) return Reject("Recycle bin inactive.");
            if (resistor == null || !resistor.IsBurned)
                return Reject("Resistor is not burned.");
            if (circuitSystem == null) return Reject("Circuit system missing.");

            SparkShelfItem item = resistor.GetComponentInParent<SparkShelfItem>();
            if (item != null && item.IsMoving)
                return Reject("Shelf item is still moving.");

            GameObject disposalObject = item != null ? item.gameObject : resistor.gameObject;
            int instanceId = disposalObject.GetInstanceID();
            if (processing.Contains(instanceId)) return false;

            // Disconnect the resistor's own terminals before changing ownership.
            // This intentionally removes its wires. It does not clear the whole level.
            if (autoDisconnectBurnedResistor)
                circuitSystem.RemoveConnectionsForLayout(disposalObject.transform);

            foreach (SparkTerminal terminal in disposalObject.GetComponentsInChildren<SparkTerminal>(true))
            {
                if (terminal == null) continue;
                buffer.Clear();
                circuitSystem.GetConnections(terminal, buffer);
                foreach (SparkCircuitConnection connection in buffer)
                    if (connection != null && connection.IsValid)
                        return Reject("Disconnect resistor wires before recycling.");
            }

            // If inventory owns the item, unregister through the existing safe shelf API.
            // A level-prefab item may instead be unregistered: release only its own slot.
            if (item != null)
            {
                bool registered = false;
                if (shelf != null && shelf.RegisteredItems != null)
                    foreach (SparkShelfItem registeredItem in shelf.RegisteredItems)
                        if (registeredItem == item) { registered = true; break; }

                if (registered)
                {
                    if (!shelf.TryUnregisterBurnedItemForRecycling(item))
                        return Reject("Registered burned item could not be unregistered.");
                    if (levelShelfController != null)
                        levelShelfController.UnregisterPurchasedItem(item);
                }
                else
                {
                    SparkShelfSlot slot = item.CurrentSlot;
                    if (slot != null)
                    {
                        if (slot.Occupant != item)
                            return Reject("Workstation occupant does not match burned item.");
                        slot.Release(item);
                        item.CurrentSlot = null;
                    }
                }
            }

            RetireBurnedObjectBindings(disposalObject.transform);
            processing.Add(instanceId);
            disposalObject.transform.SetParent(null, true);
            StartCoroutine(FlySceneResistor(disposalObject, instanceId));
            return true;
        }

        private IEnumerator FlySceneResistor(GameObject obj, int instanceId)
        {
            if (obj == null) { processing.Remove(instanceId); yield break; }
            Transform moving = obj.transform;
            Vector3 start = moving.position;
            Quaternion rotation = moving.rotation;
            Vector3 end = dustbinDropPoint != null ? dustbinDropPoint.position : transform.position;
            Quaternion endRotation = dustbinDropPoint != null ? dustbinDropPoint.rotation : transform.rotation;
            foreach (Rigidbody rb in obj.GetComponentsInChildren<Rigidbody>(true))
            {
                if (rb == null) continue;
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
                rb.isKinematic = true;
                rb.useGravity = false;
            }
            foreach (Collider col in obj.GetComponentsInChildren<Collider>(true))
                if (col != null) col.enabled = false;

            float elapsed = 0f;
            while (elapsed < flyDuration && obj != null)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / Mathf.Max(.05f, flyDuration));
                float eased = flyEase != null ? Mathf.Clamp01(flyEase.Evaluate(t)) : t;
                Vector3 position = Vector3.Lerp(start, end, eased);
                position += Vector3.up * (4f * arcHeight * t * (1f - t));
                moving.SetPositionAndRotation(position, Quaternion.Slerp(rotation, endRotation, eased));
                yield return null;
            }
            if (successSound != null) successSound.Play();
            if (successParticles != null)
            {
                successParticles.transform.position = end;
                successParticles.Play();
            }
            if (economy != null && coinReward > 0 && !economy.TryGrantRecycleCoins(coinReward))
                Debug.LogWarning("[Recycle] Reward could not be granted.", this);
            onAccepted?.Invoke();
            if (obj != null) { obj.SetActive(false); Destroy(obj); }
            processing.Remove(instanceId);
        }

        // Retire scene references belonging to the burned object BEFORE it is
        // detached or destroyed. Uses the active layout, never a random global binding.
        private void RetireBurnedObjectBindings(Transform objectRoot)
        {
            if (objectRoot == null) return;
            SparkLevelLayoutLoader loader =
                FindFirstObjectByType<SparkLevelLayoutLoader>();
            if (loader == null || loader.ActiveLayout == null) return;
            SparkLevelLayout layout = loader.ActiveLayout;
            if (objectRoot != layout.transform &&
                !objectRoot.IsChildOf(layout.transform)) return;
            SparkLevelSceneBindings bindings = layout.SceneBindings;
            if (bindings != null)
                bindings.RetireRuntimeBindingsOwnedBy(objectRoot);
        }

        private bool Reject(string reason)
        {
            Debug.LogWarning("[Recycle] " + reason, this);
            onRejected?.Invoke();
            return false;
        }
    }
}
