using System;
using System.Collections.Generic;

namespace ProjectSpark.Gameplay.DeterministicGrid
{
    // The training timebase observes four timestamps. Frequency is estimated from
    // successive raw offsets so servo correction cannot hide an unstable source.
    internal sealed class SparkLevel118Timebase
    {
        internal const int Exchanges = 24;
        private double lastRaw, firstRaw, correction;
        internal int Count { get; private set; }
        internal double OffsetNs { get; private set; }
        internal double DriftPpm { get; private set; }
        internal double UncertaintyNs { get; private set; }
        internal bool Complete => Count == Exchanges;
        internal bool Locked => Complete && Math.Abs(OffsetNs) < 900 && Math.Abs(DriftPpm) < 5 && UncertaintyNs < 400;

        internal void Sample(bool power, double frequency)
        {
            if (!power || Complete) return;
            const double interval = 12000000;
            double t1 = Count * interval;
            double downstream = 2900 + 63 * Math.Sin(Count * 1.33);
            double upstream = 2970 + 59 * Math.Cos(Count * .77);
            double noise = 31 * Math.Sin(Count * .96);
            double Local(double master) => master + 265 + master * frequency * 1e-6 + noise;
            double t2 = Local(t1 + downstream);
            double t3 = Local(t1 + downstream + 2000);
            double t4 = t1 + downstream + 2000 + upstream;
            double raw = ((t2 - t1) - (t4 - t3)) * .5;
            if (Count == 0) firstRaw = lastRaw = raw;
            OffsetNs = raw - correction;
            if (Count > 0)
                DriftPpm = (raw - firstRaw) / (Count * interval) * 1e6;
            UncertaintyNs = 105 + Math.Abs(raw - lastRaw - DriftPpm * interval * 1e-6) + Math.Abs(upstream - downstream) * .5;
            lastRaw = raw;
            correction += Math.Clamp(OffsetNs * .35, -105, 105);
            Count++;
        }

        internal double HoldoverUncertainty(double seconds) =>
            UncertaintyNs + seconds * (Math.Abs(DriftPpm) + .30) * 1000;
    }

    internal sealed class SparkLevel118GateTable
    {
        private static readonly byte[] Reserved = { 0, 16, 22, 79, 104, 164, 170, 199, 207, 248 };
        private static readonly byte[] Shared = { 0, 42, 48, 71, 81, 175, 181, 197, 205, 248 };
        internal const int CycleUs = 250;
        private readonly byte[] bytes;
        internal uint Checksum { get; }
        internal bool Intact { get; }
        internal bool Guarded { get; }

        internal SparkLevel118GateTable(bool reserved, bool contact)
        {
            byte[] expected = reserved ? Reserved : Shared;
            bytes = (byte[])expected.Clone();
            if (!contact) bytes[3] ^= 0x01;
            Checksum = SparkLevel118Schedule.Crc(bytes);
            Intact = Checksum == SparkLevel118Schedule.Crc(expected);
            Guarded = bytes[1] + 4 <= bytes[2] && bytes[3] + 4 <= bytes[4] &&
                bytes[5] + 4 <= bytes[6] && bytes[7] + 4 <= bytes[8] && bytes[9] < CycleUs &&
                bytes[3] - bytes[2] >= 28 && bytes[7] - bytes[6] >= 28;
        }

        internal (int Start, int End, bool Protected) Window(int index) =>
            (bytes[index * 2], bytes[index * 2 + 1], index is 1 or 3);
    }

    internal sealed class SparkLevel118Schedule
    {
        internal const int Cycles = 96, CommandTotal = Cycles * 2;
        internal const int DeadlineUs = 500, JitterLimitUs = 50;
        private const int QueueLimit = 18;
        private const int LinkBitsPerUs = 100;
        private readonly SparkLevel118GateTable gates;
        private readonly Queue<Frame> commands = new(10);
        private readonly Queue<Frame> background = new(QueueLimit);
        private readonly bool[] received = new bool[CommandTotal];
        private readonly int[] perField = new int[4], minimum = { int.MaxValue, int.MaxValue, int.MaxValue, int.MaxValue }, maximum = new int[4];
        private readonly uint seed;

        internal int Cycle { get; private set; }
        internal int Sent { get; private set; }
        internal int Delivered { get; private set; }
        internal int Misses { get; private set; }
        internal int CrcErrors { get; private set; }
        internal int Suppressed { get; private set; }
        internal int BestEffortDrops { get; private set; }
        internal int BestEffortDelivered { get; private set; }
        internal int PeakQueue { get; private set; }
        internal int WorstUs { get; private set; }
        internal int JitterUs { get; private set; }
        internal int PathBOutages { get; private set; }
        internal int BFailovers { get; private set; }
        internal int AValid { get; private set; }
        internal int BValid { get; private set; }
        internal int LastDelayUs { get; private set; }
        internal int QueueDepth => background.Count;
        internal int LastReservedFinish { get; private set; }
        internal bool Complete => Cycle == Cycles;
        internal bool Qualified => Complete && Sent == CommandTotal && Delivered == CommandTotal &&
            Misses == 0 && CrcErrors == 0 && WorstUs <= DeadlineUs && JitterUs <= JitterLimitUs &&
            AValid == CommandTotal && BValid == CommandTotal && PathBOutages == 0 &&
            Suppressed == CommandTotal && BestEffortDelivered > 0 && BestEffortDrops > 0 &&
            Array.TrueForAll(perField, count => count == CommandTotal / 4);

        internal SparkLevel118Schedule(SparkLevel118GateTable table, uint seed)
        {
            gates = table;
            this.seed = seed;
        }

        internal int AtField(int field) => perField[field];

        internal void Step(bool pathA, bool pathB, bool[] sixPorts, int clockShiftUs, bool corruptPathB = false)
        {
            if (Complete) return;
            LastDelayUs = 0;
            if (!pathB) PathBOutages++;
            int begin = Cycle * SparkLevel118GateTable.CycleUs;
            for (int i = 0; i < 2; i++)
            {
                int id = Sent++;
                uint payload = Mix(seed ^ (uint)id);
                int field = id % 4;
                commands.Enqueue(new Frame(id, field, 320, begin + (i == 0 ? 10 : 155), payload, PayloadCrc(payload)));
            }
            for (int i = 0; i < 8; i++)
            {
                uint payload = Mix(seed ^ (uint)(Cycle * 8 + i + 0x2f19));
                if (background.Count == QueueLimit) { BestEffortDrops++; continue; }
                background.Enqueue(new Frame(-1, (Cycle + i) % 4, 410 + (int)(payload % 270), begin, payload, 0));
            }
            PeakQueue = Math.Max(PeakQueue, background.Count);
            int endOfLastTransmission = begin;
            for (int i = 0; i < 5; i++)
            {
                var window = gates.Window(i);
                int opens = begin + window.Start + clockShiftUs;
                int closes = begin + window.End + clockShiftUs;
                int now = Math.Max(opens, endOfLastTransmission);
                var queue = window.Protected ? commands : background;
                while (queue.Count > 0)
                {
                    var frame = queue.Peek();
                    int earliest = Math.Max(frame.Arrival, now);
                    int finish = earliest + ((frame.Bytes + 24) * 8 + LinkBitsPerUs - 1) / LinkBitsPerUs;
                    if (finish > closes) break;
                    queue.Dequeue();
                    now = endOfLastTransmission = finish;
                    if (!window.Protected)
                    {
                        if (sixPorts[frame.Field + 2] && (pathA || pathB)) BestEffortDelivered++;
                        continue;
                    }
                    LastReservedFinish = finish - begin;
                    Deliver(frame, finish, pathA && sixPorts[0], pathB && sixPorts[1], sixPorts[frame.Field + 2], corruptPathB && frame.Id % 2 == 1);
                }
            }
            int cycleEnd = begin + SparkLevel118GateTable.CycleUs;
            while (commands.Count > 0 && commands.Peek().Arrival + DeadlineUs < cycleEnd)
            {
                commands.Dequeue();
                Misses++;
            }
            Cycle++;
            if (Complete) Misses += commands.Count;
        }

        private void Deliver(Frame frame, int finish, bool pathA, bool pathB, bool fieldPresent, bool corruptB)
        {
            if (!fieldPresent || !pathA && !pathB) { Misses++; return; }
            int slack = (int)(frame.Seed % 5);
            bool preferB = frame.Id % 2 == 1;
            int delayA = 22 + slack + frame.Field * 2 + (preferB ? 12 : 0);
            int delayB = 24 + (4 - slack) + frame.Field + (preferB ? 0 : 12);
            if (pathA && !pathB && delayB < delayA) BFailovers++;
            if (pathA && (!pathB || delayA <= delayB))
            {
                Receive(frame, finish + delayA, true);
                if (pathB) Receive(frame, finish + delayB, false, corruptB);
            }
            else
            {
                Receive(frame, finish + delayB, false, corruptB);
                if (pathA) Receive(frame, finish + delayA, true);
            }
        }

        private void Receive(Frame frame, int at, bool isA, bool corrupt = false)
        {
            if (PayloadCrc(frame.Seed, corrupt) != frame.Crc) { CrcErrors++; return; }
            if (isA) AValid++;
            else BValid++;
            if (received[frame.Id]) { Suppressed++; return; }
            received[frame.Id] = true;
            Delivered++;
            int delay = at - frame.Arrival;
            LastDelayUs = Math.Max(LastDelayUs, delay);
            WorstUs = Math.Max(WorstUs, delay);
            if (delay > DeadlineUs) Misses++;
            int field = frame.Field;
            perField[field]++;
            minimum[field] = Math.Min(minimum[field], delay);
            maximum[field] = Math.Max(maximum[field], delay);
            JitterUs = Math.Max(JitterUs, maximum[field] - minimum[field]);
        }

        private readonly struct Frame
        {
            internal readonly int Id, Field, Bytes, Arrival;
            internal readonly uint Seed, Crc;
            internal Frame(int id, int field, int bytes, int arrival, uint seed, uint crc)
            { Id = id; Field = field; Bytes = bytes; Arrival = arrival; Seed = seed; Crc = crc; }
        }

        internal static uint Crc(byte[] data)
        {
            uint result = 0xffffffffu;
            foreach (byte value in data) result = Accumulate(result, value);
            return result ^ 0xffffffffu;
        }

        private static uint PayloadCrc(uint seed, bool corrupt = false)
        {
            uint result = 0xffffffffu;
            for (int i = 0; i < 320; i++)
            {
                seed = Mix(seed + (uint)i + 1);
                result = Accumulate(result, (byte)(seed ^ (corrupt && i == 160 ? 0x01u : 0u)));
            }
            return result ^ 0xffffffffu;
        }

        private static uint Mix(uint value)
        {
            value ^= value << 13;
            value ^= value >> 17;
            return value ^ (value << 5);
        }

        private static uint Accumulate(uint crc, byte value)
        {
            crc ^= value;
            for (int bit = 0; bit < 8; bit++)
                crc = (crc >> 1) ^ ((crc & 1u) != 0 ? 0xedb88320u : 0u);
            return crc;
        }
    }
}
