

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using ProjectSpark.Gameplay.Shelf;

namespace ProjectSpark.Gameplay
{
    /// <summary>
    /// Project Spark - Universal Throw System
    ///
    /// Supports:
    /// - Resistors, LEDs, capacitors and other shelf items
    /// - Original and purchased components
    /// - Storage and workstation recycling
    /// - Smart slot runtime binding refresh
    /// - Electrical safety through SparkObjectShelf
    /// - Accepted/rejected feedback
    /// - Audio and particle effects
    /// - UnityEvents for UI and animations
    ///
    /// The shelf owns slot release and registration.
    /// The throw system owns recycling feedback and
    /// destruction after successful unregistration.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class SparkThrowSystem : MonoBehaviour
    {
        [Serializable]
        public sealed class ItemThrowEvent :
            UnityEvent<SparkShelfItem> { }

        // ============================================
        // REFERENCES
        // ============================================

        [Header("References")]
        [SerializeField]
        private SparkObjectShelf shelf;

        [Header("Recycling Settings")]
        [SerializeField]
        private bool allowThrowing = true;

        [SerializeField]
        private bool allowStorageItems = true;

        [SerializeField]
        private bool allowWorkstationItems = true;

        // ============================================
        // VISUAL FEEDBACK
        // ============================================

        [Header("Visual Effects")]
        [SerializeField]
        private ParticleSystem recycleParticles;

        [SerializeField]
        private ParticleSystem rejectParticles;

        [Header("Audio")]
        [SerializeField]
        private AudioSource audioSource;

        [SerializeField]
        private AudioClip acceptedSound;

        [SerializeField]
        private AudioClip rejectedSound;

        [SerializeField, Range(0f, 1f)]
        private float soundVolume = 1f;

        // ============================================
        // EVENTS
        // ============================================

        [Header("Unity Events")]
        [SerializeField]
        private ItemThrowEvent onItemThrown =
            new ItemThrowEvent();

        [SerializeField]
        private UnityEvent onThrowRejected =
            new UnityEvent();

        public event Action<SparkShelfItem> ItemThrown;

        public event Action<SparkShelfItem, string>
            ThrowRejected;

        // ============================================
        // DEBUG
        // ============================================

        [Header("Debug")]
        [SerializeField]
        private bool logResults = true;

        private readonly HashSet<int> processingItems =
            new HashSet<int>();

        public bool AllowThrowing => allowThrowing;

        public SparkObjectShelf Shelf => shelf;

        // ============================================
        // UNITY
        // ============================================

        private void Awake()
        {
            Resolve();
        }

        private void OnDisable()
        {
            processingItems.Clear();
        }

        private void Resolve()
        {
            if (shelf == null)
            {
                shelf =
                    FindFirstObjectByType<SparkObjectShelf>();
            }

            if (audioSource == null)
            {
                audioSource =
                    GetComponent<AudioSource>();
            }
        }

        // ============================================
        // VALIDATION
        // ============================================

        public bool CanThrow(
            SparkShelfItem item,
            SparkRecycleBinTarget bin,
            out string reason)
        {
            reason = null;

            if (!allowThrowing)
            {
                reason = "Recycling is disabled.";
                return false;
            }

            if (item == null)
            {
                reason = "No component selected.";
                return false;
            }

            if (bin == null)
            {
                reason = "Recycle bin missing.";
                return false;
            }

            if (!bin.CanAccept(item))
            {
                reason =
                    "Recycle bin cannot accept this item.";
                return false;
            }

            if (!item.isActiveAndEnabled ||
                !item.gameObject.activeInHierarchy)
            {
                reason = "Component is inactive.";
                return false;
            }

            if (item.IsMoving)
            {
                reason = "Component is moving.";
                return false;
            }

            if (item.CurrentSlot == null)
            {
                reason =
                    "Component has no registered slot.";
                return false;
            }

            if (item.CurrentSlot.Occupant != item)
            {
                reason =
                    "Component slot ownership mismatch.";
                return false;
            }

            if (item.IsInStorage &&
                !allowStorageItems)
            {
                reason =
                    "Recycling storage items is disabled.";
                return false;
            }

            if (item.IsAtWorkbench &&
                !allowWorkstationItems)
            {
                reason =
                    "Recycling workstation items is disabled.";
                return false;
            }

            if (processingItems.Contains(
                item.GetInstanceID()))
            {
                reason =
                    "Component is already being recycled.";
                return false;
            }

            Resolve();

            if (shelf == null)
            {
                reason = "SparkObjectShelf missing.";
                return false;
            }

            return true;
        }

        // ============================================
        // UNIVERSAL THROW
        // ============================================

        public bool TryThrow(
            SparkShelfItem item,
            SparkRecycleBinTarget bin)
        {
            if (!CanThrow(item, bin, out string reason))
            {
                Reject(item, reason);
                return false;
            }

            int instanceId = item.GetInstanceID();

            processingItems.Add(instanceId);

            try
            {
                // Shelf owns electrical safety,
                // slot release and item unregistration.
                bool success =
                    shelf.TryRecycleItem(
                        item,
                        out reason);

                if (!success)
                {
                    Reject(item, reason);
                    return false;
                }

                // At this point the item is no longer
                // registered in the shelf.
                // Its workstation slot has been released.

                PlayAcceptedEffects(item, bin);

                if (logResults)
                {
                    Debug.Log(
                        "[ThrowSystem] Recycled: " +
                        item.ItemId,
                        this);
                }

                // Notify other systems before destruction.
                ItemThrown?.Invoke(item);
                onItemThrown?.Invoke(item);

                // Destroy only after successful removal
                // from shelf bookkeeping.
                Destroy(item.gameObject);

                return true;
            }
            finally
            {
                processingItems.Remove(instanceId);
            }
        }

        // ============================================
        // ACCEPTED FEEDBACK
        // ============================================

        private void PlayAcceptedEffects(
            SparkShelfItem item,
            SparkRecycleBinTarget bin)
        {
            if (bin != null)
                bin.PlayAcceptedEffect();

            if (recycleParticles != null)
            {
                recycleParticles.transform.position =
                    bin.transform.position;

                recycleParticles.Play();
            }

            PlaySound(acceptedSound);
        }

        // ============================================
        // REJECTED FEEDBACK
        // ============================================

        private void Reject(
            SparkShelfItem item,
            string reason)
        {
            if (string.IsNullOrWhiteSpace(reason))
                reason = "Recycling refused.";

            if (logResults)
            {
                Debug.LogWarning(
                    "[ThrowSystem] Rejected: " +
                    reason,
                    this);
            }

            if (rejectParticles != null)
                rejectParticles.Play();

            PlaySound(rejectedSound);

            ThrowRejected?.Invoke(item, reason);
            onThrowRejected?.Invoke();
        }

        // ============================================
        // AUDIO
        // ============================================

        private void PlaySound(AudioClip clip)
        {
            if (clip == null || audioSource == null)
                return;

            audioSource.PlayOneShot(
                clip,
                soundVolume);
        }

        // ============================================
        // PUBLIC CONTROLS
        // ============================================

        public void SetThrowingEnabled(bool enabled)
        {
            allowThrowing = enabled;
        }

        public void SetShelf(SparkObjectShelf newShelf)
        {
            shelf = newShelf;
        }
    }
}

