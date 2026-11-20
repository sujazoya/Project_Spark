using System;

namespace ProjectSpark.Gameplay
{
    // Bounded teaching peripherals. The level supplies solved electrical line qualification
    // before invoking packet exchanges. Transaction time is independent of render frames.
    internal sealed class SparkLevel100Protocols
    {
        private readonly byte[] spiRam = new byte[32];
        private readonly byte[] deviceAddresses = { 0x48, 0x50, 0x68 };
        private readonly byte[] registers = { 0x1B, 0x00, 0x25 };

        public bool ExchangeI2c(out string result)
        {
            result = "";
            var found = new byte[3];
            int foundCount = 0;
            for (int address = 0x08; address <= 0x77; address++)
            {
                if (AddressI2c((byte)(address << 1)) < 0) continue;
                if (foundCount == found.Length) return false;
                found[foundCount++] = (byte)address;
            }

            const byte witness = 0x5A;
            if (foundCount != found.Length || ProbeI2c(0x49, false, 0, out _) ||
                !ProbeI2c(0x50, false, witness, out _) ||
                !ProbeI2c(0x50, true, 0, out byte eeprom) || eeprom != witness ||
                !ProbeI2c(0x48, true, 0, out byte thermal) || thermal != 0x1B ||
                !ProbeI2c(0x68, true, 0, out byte clock) || clock != 0x25) return false;
            result = $"I²C ACK 0x{found[0]:X2} / 0x{found[1]:X2} / 0x{found[2]:X2} · EEPROM {eeprom:X2}";
            return true;
        }

        private int AddressI2c(byte frame) => Array.IndexOf(deviceAddresses, (byte)(frame >> 1));

        private bool ProbeI2c(byte address, bool read, byte data, out byte answer)
        {
            answer = 0;
            byte frame = OpenDrainByte((byte)((address << 1) | (read ? 1 : 0)));
            int device = AddressI2c(frame);
            if (device < 0) return false;
            if ((frame & 1) != 0)
            {
                answer = OpenDrainByte(registers[device]);
                return true;
            }
            if (device != 1) return false;
            registers[device] = OpenDrainByte(data);
            return true;
        }

        private static byte OpenDrainByte(byte driver)
        {
            int sampled = 0;
            for (int bit = 7; bit >= 0; bit--)
            {
                bool drivesLow = (driver & 1 << bit) == 0;
                sampled = (sampled << 1) | (drivesLow ? 0 : 1);
            }
            return (byte)sampled;
        }

        public bool ExchangeSpi(out string result)
        {
            result = "";
            const byte marker = 0xD6;
            if (SpiTransfer(false, false, 0, 0x9F, 0, 0, out _) ||
                SpiTransfer(true, true, 0, 0x9F, 0, 0, out _) ||
                SpiTransfer(false, true, 0, 0xA1, 0, 0, out _) ||
                !SpiTransfer(true, false, 0, 0x9F, 0, 0, out byte identity) || identity != 0xD8 ||
                !SpiTransfer(true, false, 0, 0x02, 7, marker, out _) ||
                !SpiTransfer(true, false, 0, 0x03, 7, 0, out byte readback) || readback != marker ||
                !SpiTransfer(false, true, 3, 0xA1, 0, 0, out byte sample) || sample != 0x96) return false;

            result = $"SPI CS0 RAM {identity:X2} / {readback:X2} · CS1 ADC {sample:X2}";
            return true;
        }

        private bool SpiTransfer(bool selectMemory, bool selectAdc, int mode, byte opcode, byte address, byte data, out byte response)
        {
            response = 0;
            if (selectMemory == selectAdc || mode != (selectMemory ? 0 : 3)) return false;
            if (selectAdc)
            {
                if (opcode != 0xA1) return false;
                response = 0x96;
                return true;
            }
            if (address >= spiRam.Length) return false;
            switch (opcode)
            {
                case 0x9F: response = 0xD8; return true;
                case 0x02: spiRam[address] = data; return true;
                case 0x03: response = spiRam[address]; return true;
                default: return false;
            }
        }

        public bool ExchangeUart(int controllerBaud, int endpointBaud, out string result)
        {
            const byte request = 0xA5, reply = 0x3C;
            bool outbound = UartFrame(request, controllerBaud, endpointBaud, out byte remote);
            bool inbound = UartFrame(reply, endpointBaud, controllerBaud, out byte host);
            if (!outbound || !inbound || remote != request || host != reply)
            {
                result = $"UART frame error · U1 {controllerBaud} / U8 {endpointBaud} baud";
                return false;
            }
            result = $"UART {controllerBaud} 8N1 · TX {remote:X2} / RX {host:X2}";
            return true;
        }

        private static bool UartFrame(byte transmit, int txBaud, int rxBaud, out byte receive)
        {
            receive = 0;
            if (SampleUart(transmit, txBaud, rxBaud, 0) || !SampleUart(transmit, txBaud, rxBaud, 9)) return false;
            for (int bit = 0; bit < 8; bit++)
                if (SampleUart(transmit, txBaud, rxBaud, bit + 1)) receive |= (byte)(1 << bit);
            return true;
        }

        private static bool SampleUart(byte frame, int txBaud, int rxBaud, int sample)
        {
            int slot = (int)((sample + .5) * txBaud / rxBaud);
            return slot >= 9 || slot > 0 && (frame & (1 << (slot - 1))) != 0;
        }
    }
}
