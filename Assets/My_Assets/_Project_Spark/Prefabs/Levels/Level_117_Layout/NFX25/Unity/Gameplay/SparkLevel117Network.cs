using System;
using System.Collections.Generic;

namespace ProjectSpark.Gameplay.PacketCitadel
{
    internal readonly struct SparkLevel117PortSample
    {
        internal readonly float TransmitDbm, ReceiveDbm, Eye;
        internal readonly bool OpticalReady;
        internal bool Link => OpticalReady && Eye >= .52f;

        internal SparkLevel117PortSample(int port, bool present, bool powered, float bias, float lane, bool clocked)
        {
            bool optical = present && powered && bias > 2.85f && clocked;
            TransmitDbm = optical ? -2.15f - port * .18f : -34f;
            ReceiveDbm = TransmitDbm - (3.15f + port * .24f);
            OpticalReady = optical && ReceiveDbm > -12f;
            Eye = powered && clocked && lane > 4.62f ? .83f - port * .046f : .19f;
        }
    }

    // Four timestamp observations of the same exchange. The local oscillator
    // remains a separate, drifting clock; following exchanges observe the servo's
    // bounded corrections and the bias from asymmetric optical transit.
    internal sealed class SparkLevel117Clock
    {
        internal const int Exchanges = 32;
        private double elapsed, phase = .96, residual;
        internal int Count { get; private set; }
        internal double Offset { get; private set; }
        internal double Path { get; private set; }
        internal double PeakResidual { get; private set; }
        internal bool Complete => Count == Exchanges;
        internal bool Locked => Complete && Math.Abs(Offset) < 1.2 && PeakResidual < 1.5;

        internal void Observe(bool disciplined)
        {
            if (Complete) return;
            const double period = .026;
            elapsed += period;
            double ppm = disciplined ? 1.35 : 34.5;
            double send = elapsed * 1e6;
            double fwd = 4.55 + .13 * Math.Sin(Count * .77);
            double rev = 5.08 + .14 * Math.Cos(Count * .71);
            double jitter = .071 * Math.Sin(Count * 2.18);
            double Local(double master) => master + phase + (master * ppm * 1e-6) + jitter;
            double t1 = send;
            double t2 = Local(send + fwd);
            double t3 = Local(send + fwd + 3.8);
            double t4 = send + fwd + 3.8 + rev;
            Offset = ((t2 - t1) - (t4 - t3)) * .5;
            Path = ((t2 - t1) + (t4 - t3)) * .5;
            phase -= Math.Max(-.20, Math.Min(.20, Offset * .28));
            Count++;
            if (Count >= Exchanges / 2)
            {
                residual = Math.Abs(Offset);
                PeakResidual = Math.Max(PeakResidual, residual);
            }
        }
    }

    internal sealed class SparkLevel117PacketRun
    {
        internal const int SlotCount = 64, FrameCount = SlotCount * 8;
        private const double SlotUs = 6.0, LineBitsPerUs = 10000.0;
        private const int QueueCap = 12;

        private readonly struct Frame
        {
            internal readonly int Id, Source, Destination, Length;
            internal readonly bool Priority;
            internal readonly double Ingress;
            internal readonly uint Seed, Crc;

            internal Frame(int id, int source, int destination, int length, bool priority, double ingress, uint seed)
            {
                Id = id;
                Source = source;
                Destination = destination;
                Length = length;
                Priority = priority;
                Ingress = ingress;
                Seed = seed;
                Crc = ComputeCrc(length, seed, false);
            }
        }

        private readonly Queue<Frame>[] priority = MakeQueues(), regular = MakeQueues();
        private readonly double[] latency = new double[FrameCount];
        private readonly bool[] accepted = new bool[FrameCount];
        private readonly int[] deliveries = new int[4];
        private readonly uint seed;
        private double priorityDuration, regularDuration;
        private int priorityCount, regularCount;

        internal SparkLevel117PacketRun(uint seed) => this.seed = seed;
        internal int Slot { get; private set; }
        internal int Sent { get; private set; }
        internal int Received { get; private set; }
        internal int Dropped { get; private set; }
        internal int BadCrc { get; private set; }
        internal int Duplicates { get; private set; }
        internal int PeakQueue { get; private set; }
        internal double P95 { get; private set; }
        internal double PriorityMean => priorityCount == 0 ? 0 : priorityDuration / priorityCount;
        internal double RegularMean => regularCount == 0 ? 0 : regularDuration / regularCount;
        internal bool Complete => Slot == SlotCount;
        internal bool Qualified => Complete && Sent == FrameCount && Received == FrameCount &&
            Dropped == 0 && BadCrc == 0 && Duplicates == 0 && P95 < 6.4 && PriorityMean < RegularMean &&
            Array.TrueForAll(deliveries, count => count == FrameCount / 4);
        internal int DeliveredAt(int port) => deliveries[port];

        private static Queue<Frame>[] MakeQueues() =>
            new[] { new Queue<Frame>(QueueCap), new Queue<Frame>(QueueCap), new Queue<Frame>(QueueCap), new Queue<Frame>(QueueCap) };

        internal void Step(SparkLevel117PortSample[] ports)
        {
            if (Complete) return;
            double start = Slot * SlotUs;
            for (int p = 0; p < 4; p++)
            {
                int destination = (p + 1 + Slot % 3) & 3;
                uint payload = seed ^ unchecked((uint)(Slot * 4 + p + 1) * 0x9e3779b9u);
                int length = 256 + (int)(payload % 768u);
                Enqueue(new Frame(Sent, p, destination, length, false, start, payload));
                Enqueue(new Frame(Sent, p, destination, length, true, start + .095, payload ^ 0xcee5413du));
            }
            for (int port = 0; port < 4; port++)
            {
                double now = start + .22;
                while (priority[port].Count + regular[port].Count > 0)
                {
                    var queue = priority[port].Count > 0 ? priority[port] : regular[port];
                    var frame = queue.Peek();
                    double end = now + (frame.Length + 24) * 8 / LineBitsPerUs;
                    if (end > start + SlotUs) break;
                    queue.Dequeue();
                    now = end;
                    Deliver(frame, now, ports);
                }
            }
            Slot++;
            if (!Complete) return;
            for (int port = 0; port < 4; port++)
                Dropped += priority[port].Count + regular[port].Count;
            if (Received > 0)
            {
                Array.Sort(latency, 0, Received);
                P95 = latency[(int)Math.Ceiling(Received * .95) - 1];
            }
        }

        private void Enqueue(Frame frame)
        {
            Sent++;
            var queue = frame.Priority ? priority[frame.Destination] : regular[frame.Destination];
            if (queue.Count >= QueueCap) { Dropped++; return; }
            queue.Enqueue(frame);
            PeakQueue = Math.Max(PeakQueue, priority[frame.Destination].Count + regular[frame.Destination].Count);
        }

        private void Deliver(Frame frame, double at, SparkLevel117PortSample[] ports)
        {
            var input = ports[frame.Source];
            var output = ports[frame.Destination];
            if (!input.OpticalReady || !output.OpticalReady) { Dropped++; return; }
            bool weak = input.Eye < .52f || output.Eye < .52f;
            if (weak && frame.Id % 3 == 0) { Dropped++; return; }
            if (ComputeCrc(frame.Length, frame.Seed, weak) != frame.Crc) { BadCrc++; return; }
            if (accepted[frame.Id]) { Duplicates++; return; }
            accepted[frame.Id] = true;
            deliveries[frame.Destination]++;
            double duration = at - frame.Ingress + 3.40 + frame.Destination * .07;
            latency[Received++] = duration;
            if (frame.Priority) { priorityDuration += duration; priorityCount++; }
            else { regularDuration += duration; regularCount++; }
        }

        private static uint ComputeCrc(int length, uint state, bool corrupt)
        {
            uint crc = 0xffffffffu;
            for (int i = 0; i < length; i++)
            {
                state ^= state << 13;
                state ^= state >> 17;
                state ^= state << 5;
                uint octet = state & 255u;
                if (corrupt && i == length / 2) octet ^= 0x10u;
                crc ^= octet;
                for (int bit = 0; bit < 8; bit++) crc = (crc >> 1) ^ ((crc & 1u) != 0 ? 0xedb88320u : 0u);
            }
            return crc ^ 0xffffffffu;
        }
    }
}
