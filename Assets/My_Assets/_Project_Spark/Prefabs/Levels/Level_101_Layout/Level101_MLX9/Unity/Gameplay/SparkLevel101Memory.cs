using System;
using UnityEngine;

namespace ProjectSpark.Gameplay.HiddenLayers
{
    internal sealed class SparkLevel101Memory
    {
        internal enum Image { Diagnostic, Legacy, Corrupt }
        private static readonly byte[] Approved = CreateImage();
        private static readonly uint Expected = Crc(Approved);
        private byte[] flash, pending;
        private float elapsed;
        public bool Busy => pending != null;
        public bool HasImage => flash != null && Crc(flash) == Expected;
        public int Progress => Mathf.Clamp(Mathf.RoundToInt(elapsed / .65f * 100f), 0, 100);

        public bool Begin(Image image, out string error)
        {
            error = "";
            var candidate = (byte[])Approved.Clone();
            if (image == Image.Legacy) candidate[0] = 0x95;
            if (image == Image.Corrupt) candidate[19] ^= 0x80;
            if (candidate[0] != 0xA1) { error = "Image target does not match MLX-9."; return false; }
            if (Crc(candidate) != Expected) { error = "Image CRC does not match the approved descriptor."; return false; }
            pending = candidate;
            elapsed = 0f;
            return true;
        }

        public bool Tick(float delta)
        {
            if (!Busy) return false;
            elapsed += delta;
            if (elapsed < .65f) return false;
            flash = (byte[])pending.Clone();
            pending = null;
            return HasImage;
        }

        public bool BootAndVerify(out string proof)
        {
            proof = "";
            if (!HasImage) return false;
            var readback = (byte[])flash.Clone();
            if (Crc(readback) != Expected || readback[1] != 9 || readback[2] != 1) return false;
            var ram = new byte[64];
            for (int bit = 0; bit < 8; bit++)
            {
                byte pattern = (byte)(1 << bit);
                for (int i = 0; i < ram.Length; i++) ram[i] = (i & 1) == 0 ? pattern : (byte)~pattern;
                for (int i = 0; i < ram.Length; i++)
                    if (ram[i] != ((i & 1) == 0 ? pattern : (byte)~pattern)) return false;
            }
            proof = $"FLASH CRC {Expected:X8} · RAM 512 readbacks";
            return true;
        }

        public void Abort() => pending = null;
        public string Export() => HasImage ? Convert.ToBase64String(flash) : "";
        public bool Restore(string encoded)
        {
            if (string.IsNullOrEmpty(encoded)) { flash = null; return true; }
            byte[] candidate;
            try { candidate = Convert.FromBase64String(encoded); }
            catch (FormatException) { return false; }
            if (candidate.Length != Approved.Length || Crc(candidate) != Expected) return false;
            flash = candidate;
            return true;
        }

        private static byte[] CreateImage()
        {
            var bytes = new byte[96];
            bytes[0] = 0xA1;
            bytes[1] = 9;
            bytes[2] = 1;
            for (int i = 3; i < bytes.Length; i++) bytes[i] = (byte)(i * 73 + (i >> 1) + 11);
            return bytes;
        }

        private static uint Crc(byte[] bytes)
        {
            uint crc = uint.MaxValue;
            foreach (var value in bytes)
            {
                crc ^= value;
                for (int bit = 0; bit < 8; bit++) crc = (crc >> 1) ^ ((crc & 1) != 0 ? 0xEDB88320u : 0u);
            }
            return ~crc;
        }
    }
}
