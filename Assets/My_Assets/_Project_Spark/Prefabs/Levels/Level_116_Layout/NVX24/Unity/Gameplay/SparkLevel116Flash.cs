using System;
using UnityEngine;

namespace ProjectSpark.Gameplay.DataPhoenix
{
    // The training store has 8 channels, 4 logical pages per channel and
    // two alternating physical pages per address. Only journal publication
    // changes the durable map. Completion is issued in the following step.
    internal sealed class SparkLevel116Flash
    {
        internal const int PageCount = 32;
        internal enum WriteStep { Intent, Program, Verify, Commit, Complete }
        internal enum ReadResult { Missing, Clean, Corrected, Uncorrectable }
        private static readonly float[] StepTime = { .016f, .075f, .022f, .069f, .006f };

        [Serializable]
        internal struct Page
        {
            public int sequence, address, erases;
            public long code, crc;
        }

        [Serializable]
        internal sealed class Journal
        {
            public int sequence;
            public int[] map;
            public long marker;
            public bool programmed, retired;
        }

        [Serializable]
        internal sealed class State
        {
            public Page[] pages;
            public Journal[] journals;
            public int[] acknowledged;
            public int sequence, completions;
        }

        internal sealed class Pending
        {
            public int address, sequence, slot;
            public uint data;
            public WriteStep step;
        }

        private State state = NewState();
        private int activeJournal;
        internal Pending Current { get; private set; }
        internal int Acknowledgements => state.completions;
        internal int DurableGeneration => activeJournal < 0 ? -1 : state.journals[activeJournal].sequence;
        internal int RecoveredPages { get; private set; }
        internal int RetiredMetadataBlocks => Array.FindAll(state.journals, journal => journal.retired).Length;

        private static State NewState()
        {
            var journals = new Journal[3];
            for (int i = 0; i < journals.Length; i++)
            {
                var map = new int[PageCount];
                Array.Fill(map, -1);
                journals[i] = new Journal { map = map };
            }
            var state = new State { journals = journals, pages = new Page[PageCount * 2], acknowledged = new int[PageCount] };
            journals[0].programmed = true;
            journals[0].marker = Encode(MapCrc(journals[0].map, 0));
            return state;
        }

        internal State Snapshot() => state;

        internal bool Restore(State loaded)
        {
            if (loaded?.pages?.Length != PageCount * 2 || loaded.acknowledged?.Length != PageCount ||
                loaded.journals?.Length != 3 || loaded.sequence is < 0 or > 10000000 || loaded.completions < 0 ||
                Array.Exists(loaded.journals, j => j?.map?.Length != PageCount))
                return false;
            state = loaded;
            Current = null;
            Replay();
            return true;
        }

        internal bool Begin(int address, uint data)
        {
            if (Current != null || activeJournal < 0 || address < 0 || address >= PageCount ||
                !HealthyJournal(state.journals[activeJournal])) return false;
            int prior = state.journals[activeJournal].map[address];
            int slot = prior == 2 * address ? prior + 1 : 2 * address;
            Current = new Pending { address = address, data = data, sequence = ++state.sequence, slot = slot };
            return true;
        }

        internal bool Advance()
        {
            var write = Current;
            if (write == null) return false;
            switch (write.step)
            {
                case WriteStep.Intent:
                    break;
                case WriteStep.Program:
                    var old = state.pages[write.slot];
                    state.pages[write.slot] = new Page
                    {
                        sequence = write.sequence, address = write.address, erases = old.erases + (old.sequence > 0 ? 1 : 0),
                        code = Encode(write.data), crc = PageCrc(write.data, write.address, write.sequence)
                    };
                    break;
                case WriteStep.Verify:
                    if (ReadSlot(write.slot, write.address, 0, out uint checkedData) != ReadResult.Clean ||
                        checkedData != write.data) { Current = null; return false; }
                    break;
                case WriteStep.Commit:
                    int target = WritableJournal();
                    if (target < 0) { Current = null; return false; }
                    Array.Copy(state.journals[activeJournal].map, state.journals[target].map, PageCount);
                    state.journals[target].map[write.address] = write.slot;
                    Publish(target, write.sequence);
                    break;
                case WriteStep.Complete:
                    state.acknowledged[write.address] = write.sequence;
                    state.completions++;
                    Current = null;
                    return true;
            }
            write.step++;
            return true;
        }

        internal bool Write(uint data, int address)
        {
            if (!Begin(address, data)) return false;
            while (Current != null)
                if (!Advance()) return false;
            return true;
        }

        internal int PowerLoss(float holdSeconds, bool corruptMetadata = false)
        {
            int before = Acknowledgements;
            while (Current != null && holdSeconds >= StepTime[(int)Current.step])
            {
                holdSeconds -= StepTime[(int)Current.step];
                if (!Advance()) break;
            }
            if (corruptMetadata) CorruptPublishedMetadata();
            Current = null;
            activeJournal = -1;
            return Acknowledgements - before;
        }

        internal bool Replay()
        {
            activeJournal = -1;
            RecoveredPages = 0;
            int best = -1;
            for (int i = 0; i < state.journals.Length; i++)
            {
                var journal = state.journals[i];
                if (journal.sequence <= best || !HealthyJournal(journal)) continue;
                activeJournal = i;
                best = journal.sequence;
            }
            if (activeJournal < 0) return false;
            bool passed = true;
            var current = state.journals[activeJournal];
            for (int address = 0; address < PageCount; address++)
            {
                int slot = current.map[address];
                int ack = state.acknowledged[address];
                if (slot < 0) { passed &= ack == 0; continue; }
                var page = state.pages[slot];
                bool valid = ReadSlot(slot, address, 0, out _) is ReadResult.Clean or ReadResult.Corrected;
                if (valid) RecoveredPages++;
                // A later durable commit may legitimately replace the most
                // recently acknowledged data even if its completion was lost.
                passed &= valid && page.sequence >= ack;
            }
            return passed;
        }

        internal bool CorruptPublishedMetadata()
        {
            if (activeJournal < 0 || !HealthyJournal(state.journals[activeJournal])) return false;
            state.journals[activeJournal].marker ^= 3;
            return true;
        }

        internal bool RemapMetadata()
        {
            int bad = Array.FindIndex(state.journals, journal => journal.programmed && !journal.retired && !HealthyJournal(journal));
            if (bad < 0) return false;
            int target = WritableJournal(bad);
            if (target < 0) return false;
            var newMap = new int[PageCount];
            Array.Fill(newMap, -1);
            for (int address = 0; address < PageCount; address++)
            {
                int sequence = state.acknowledged[address];
                if (sequence == 0) continue;
                int slot = 2 * address;
                if (state.pages[slot].sequence != sequence) slot++;
                if (state.pages[slot].sequence != sequence ||
                    ReadSlot(slot, address, 0, out _) is not (ReadResult.Clean or ReadResult.Corrected)) return false;
                newMap[address] = slot;
            }
            Array.Copy(newMap, state.journals[target].map, PageCount);
            state.journals[bad].retired = true;
            Publish(target, ++state.sequence);
            return Replay();
        }

        private int WritableJournal(int excluded = -1)
        {
            int target = -1;
            for (int i = 0; i < state.journals.Length; i++)
                if (i != activeJournal && i != excluded && !state.journals[i].retired &&
                    (target < 0 || state.journals[i].sequence < state.journals[target].sequence))
                    target = i;
            return target;
        }

        private void Publish(int target, int sequence)
        {
            var journal = state.journals[target];
            journal.sequence = sequence;
            journal.marker = Encode(MapCrc(journal.map, sequence));
            journal.programmed = true;
            activeJournal = target;
        }

        private bool HealthyJournal(Journal journal)
        {
            if (!journal.programmed || journal.retired ||
                Decode(journal.marker, out uint marker) == ReadResult.Uncorrectable ||
                marker != MapCrc(journal.map, journal.sequence)) return false;
            for (int i = 0; i < PageCount; i++)
                if (journal.map[i] != -1 && journal.map[i] != i * 2 && journal.map[i] != i * 2 + 1) return false;
            return true;
        }

        internal ReadResult Read(int address, long flippedBits, out uint data)
        {
            data = 0;
            if (activeJournal < 0 || address < 0 || address >= PageCount) return ReadResult.Missing;
            return ReadSlot(state.journals[activeJournal].map[address], address, flippedBits, out data);
        }

        private ReadResult ReadSlot(int slot, int address, long flippedBits, out uint data)
        {
            data = 0;
            if (slot < 0) return ReadResult.Missing;
            var page = state.pages[slot];
            if (page.sequence == 0 || page.address != address) return ReadResult.Missing;
            ReadResult result = Decode(page.code ^ flippedBits, out data);
            return result != ReadResult.Uncorrectable && page.crc == PageCrc(data, address, page.sequence)
                ? result : ReadResult.Uncorrectable;
        }

        private static bool IsParity(int position) => (position & (position - 1)) == 0;

        private static long Encode(uint data)
        {
            long word = 0;
            int parity = 0, bit = 0;
            for (int pos = 1; pos <= 38; pos++)
            {
                if (IsParity(pos)) continue;
                if ((data & (1U << bit)) != 0) { word |= 1L << (pos - 1); parity ^= pos; }
                bit++;
            }
            for (int i = 0; i < 6; i++)
                if ((parity & 1 << i) != 0) word |= 1L << ((1 << i) - 1);
            if (Odd(word)) word |= 1L << 38;
            return word;
        }

        private static ReadResult Decode(long word, out uint data)
        {
            data = 0;
            int syndrome = 0;
            for (int pos = 1; pos <= 38; pos++)
                if ((word & 1L << (pos - 1)) != 0) syndrome ^= pos;
            bool odd = Odd(word);
            if (syndrome > 38 || syndrome != 0 && !odd) return ReadResult.Uncorrectable;
            if (syndrome != 0) word ^= 1L << (syndrome - 1);
            int bit = 0;
            for (int pos = 1; pos <= 38; pos++)
            {
                if (IsParity(pos)) continue;
                if ((word & 1L << (pos - 1)) != 0) data |= 1U << bit;
                bit++;
            }
            return odd ? ReadResult.Corrected : ReadResult.Clean;
        }

        private static bool Odd(long word)
        {
            bool result = false;
            while (word != 0) { word &= word - 1; result = !result; }
            return result;
        }

        private static uint PageCrc(uint value, int address, int sequence) =>
            Crc(Crc(Crc(0xffffffffU, value), (uint)address), (uint)sequence) ^ 0xffffffffU;

        private static uint MapCrc(int[] map, int sequence)
        {
            uint crc = Crc(0xffffffffU, (uint)sequence);
            foreach (int slot in map) crc = Crc(crc, unchecked((uint)slot));
            return crc ^ 0xffffffffU;
        }

        private static uint Crc(uint crc, uint value)
        {
            crc ^= value;
            for (int i = 0; i < 32; i++) crc = (crc >> 1) ^ ((crc & 1) == 0 ? 0 : 0xedb88320U);
            return crc;
        }
    }

    internal static class SparkLevel116Reserve
    {
        internal static float HoldSeconds(float chargeVolts, float esr, float boardPower)
        {
            const float efficiency = .86f, capacitance = .28f, minimum = 2.63f, step = .001f;
            float power = boardPower / efficiency;
            float voltage = chargeVolts;
            for (int i = 0; i < 1400; i++)
            {
                float discriminant = voltage * voltage - 4f * esr * power;
                if (discriminant <= 0f) return i * step;
                float loaded = (voltage + Mathf.Sqrt(discriminant)) * .5f;
                if (loaded < minimum) return i * step;
                voltage -= power / loaded * step / capacitance;
            }
            return 1.4f;
        }
    }
}
