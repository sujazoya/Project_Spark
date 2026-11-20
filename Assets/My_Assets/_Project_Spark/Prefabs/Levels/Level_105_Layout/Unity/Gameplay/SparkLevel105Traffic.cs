namespace ProjectSpark.Gameplay.ExpansionNexus
{
    internal sealed class SparkLevel105Traffic
    {
        public const int Bursts = 16;
        private const int FrameBytes = 32;
        private readonly byte[] payload = new byte[FrameBytes], received = new byte[FrameBytes];

        public int Burst { get; private set; }
        public int Frames { get; private set; }
        public int Bytes { get; private set; }
        public int Errors { get; private set; }
        public int FirstLane { get; private set; } = -1;
        public string FirstDirection { get; private set; }

        public void Reset()
        {
            Burst = Frames = Bytes = Errors = 0;
            FirstLane = -1;
            FirstDirection = "";
        }

        public void CompareBurst(int validDirections)
        {
            for (int path = 0; path < 8; path++)
            {
                int sequence = Frames;
                uint state = 0xC4EC105u ^ (uint)(sequence + 1) * 747796405u;
                payload[0] = (byte)sequence;
                payload[1] = (byte)path;
                for (int i = 2; i < FrameBytes; i++)
                {
                    state ^= state << 13;
                    state ^= state >> 17;
                    state ^= state << 5;
                    payload[i] = (byte)state;
                }
                System.Array.Copy(payload, received, FrameBytes);
                if ((validDirections & (1 << path)) == 0)
                    received[2 + sequence % (FrameBytes - 2)] ^= (byte)(1 << (sequence % 8));
                if (received[0] != sequence || received[1] != path || Crc(payload) != Crc(received))
                {
                    Errors++;
                    if (FirstLane < 0)
                    {
                        FirstLane = path / 2;
                        FirstDirection = path % 2 == 0 ? "TX" : "RX";
                    }
                }
                Frames++;
                Bytes += FrameBytes;
            }
            Burst++;
        }

        private static ushort Crc(byte[] data)
        {
            int remainder = 0xffff;
            foreach (byte value in data)
            {
                remainder ^= value << 8;
                for (int bit = 0; bit < 8; bit++)
                    remainder = (remainder & 0x8000) != 0 ? ((remainder << 1) ^ 0x1021) & 0xffff : (remainder << 1) & 0xffff;
            }
            return (ushort)remainder;
        }
    }
}
