using System;
using System.Security.Cryptography;
using UnityEngine;

namespace ProjectSpark.Gameplay
{
    internal sealed class SparkLevel99Firmware
    {
        internal enum Image { Diagnostic, Legacy, Corrupt }
        private static readonly byte[] Approved = BuildImage();
        private static readonly string ApprovedDigest = Digest(Approved);
        private byte[] installed, pending;
        private float progress;
        internal bool Busy => pending != null;
        internal bool IsVerified => installed != null;
        internal bool VerifyReadback() => installed != null && Digest(installed) == ApprovedDigest;
        internal string Identity => IsVerified ? "DIAGNOSTIC 1.0 / " + ApprovedDigest.Substring(0, 8) : "EMPTY";
        internal int Percent => Mathf.Clamp(Mathf.RoundToInt(progress / .7f * 100f), 0, 100);

        internal bool Begin(Image image, out string reason)
        {
            reason = "";
            if (Busy) { reason = "SWD is already writing."; return false; }
            var candidate = (byte[])Approved.Clone();
            if (image == Image.Legacy) candidate[0] = 0x71;
            if (image == Image.Corrupt) candidate[24] ^= 0x80;
            if (candidate[0] != 0x99) { reason = "Image target is incompatible with the SBC-7 training MCU."; return false; }
            if (Digest(candidate) != ApprovedDigest) { reason = "Image digest failed verification."; return false; }
            pending = candidate;
            progress = 0;
            return true;
        }

        internal bool Tick(float delta)
        {
            if (!Busy) return false;
            progress += delta;
            if (progress < .7f) return false;
            installed = pending;
            pending = null;
            return IsVerified;
        }

        internal void Abort() => pending = null;
        internal string Export() => installed == null ? "" : Convert.ToBase64String(installed);

        internal bool Restore(string encoded)
        {
            if (string.IsNullOrEmpty(encoded)) { installed = null; return true; }
            byte[] parsed;
            try { parsed = Convert.FromBase64String(encoded); }
            catch (FormatException) { return false; }
            if (parsed.Length != Approved.Length || Digest(parsed) != ApprovedDigest) return false;
            installed = parsed;
            return true;
        }

        private static byte[] BuildImage()
        {
            var bytes = new byte[64];
            bytes[0] = 0x99;
            bytes[1] = 0x07;
            bytes[2] = 0x10;
            for (int i = 3; i < bytes.Length; i++) bytes[i] = (byte)((i * 53 + (i >> 2) + 17) & 255);
            return bytes;
        }

        private static string Digest(byte[] bytes)
        {
            using var hash = SHA256.Create();
            return BitConverter.ToString(hash.ComputeHash(bytes)).Replace("-", "");
        }
    }
}
