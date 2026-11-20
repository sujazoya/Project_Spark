using System;
using System.Security.Cryptography;
using UnityEngine;

namespace ProjectSpark.Gameplay
{
    // Deterministic training images with a preloaded trusted digest. The recovery bank exposes read access only.
    internal sealed class SparkPfx15Flash
    {
        private const int PageSize = 64;
        private readonly byte[] recovery = new byte[8 * PageSize];
        private readonly byte[] primary = new byte[8 * PageSize];
        private readonly byte[] trustedDigest;
        private float elapsed;
        private int writtenPages;

        public SparkPfx15Flash()
        {
            for (int i = 0; i < recovery.Length; i++) recovery[i] = (byte)((i * 73 + ((i >> 2) ^ 0x51)) & 0xff);
            recovery[0] = 0x15;
            recovery[1] = 0x04;
            trustedDigest = Hash(recovery);
            Array.Copy(recovery, primary, primary.Length);
            Corrupt();
        }

        public bool Busy { get; private set; }
        public bool Erasing => Busy && elapsed < .26f;
        public int PagesWritten => writtenPages;
        public int Pages => primary.Length / PageSize;
        public string Digest => BitConverter.ToString(trustedDigest, 0, 4).Replace("-", "");
        public bool PrimaryValid => !Busy && Matches(primary);
        public bool RecoveryValid => Matches(recovery);

        public void Corrupt() => primary[PageSize * 2 + 13] ^= 0x7f;
        public bool Begin()
        {
            if (Busy || !RecoveryValid) return false;
            elapsed = 0;
            writtenPages = 0;
            Busy = true;
            return true;
        }

        public void Tick(float delta)
        {
            if (!Busy) return;
            float before = elapsed;
            elapsed += delta;
            if (elapsed < .26f) return;
            if (before < .26f)
                for (int i = 0; i < primary.Length; i++) primary[i] = 0xff;
            int pages = Mathf.Min(Pages, Mathf.FloorToInt((elapsed - .26f) / .16f) + 1);
            while (writtenPages < pages)
            {
                int at = writtenPages * PageSize;
                Array.Copy(recovery, at, primary, at, PageSize);
                writtenPages++;
            }
            if (writtenPages == Pages) Busy = false;
        }

        public bool Interrupt()
        {
            if (!Busy) return false;
            if (Erasing)
                for (int i = primary.Length / 2; i < primary.Length; i++) primary[i] = 0xff;
            Busy = false;
            return !PrimaryValid;
        }

        public string ExportPrimary() => Convert.ToBase64String(primary);

        public bool RestorePrimary(string record)
        {
            if (string.IsNullOrEmpty(record)) return false;
            byte[] content;
            try { content = Convert.FromBase64String(record); }
            catch (FormatException) { return false; }
            if (content.Length != primary.Length) return false;
            Busy = false;
            Array.Copy(content, primary, primary.Length);
            writtenPages = 0;
            elapsed = 0;
            return true;
        }

        private bool Matches(byte[] image)
        {
            if (image[0] != 0x15 || image[1] != 0x04) return false;
            byte[] digest = Hash(image);
            for (int i = 0; i < digest.Length; i++)
                if (digest[i] != trustedDigest[i]) return false;
            return true;
        }

        private static byte[] Hash(byte[] image)
        {
            using var sha = SHA256.Create();
            return sha.ComputeHash(image);
        }
    }
}
