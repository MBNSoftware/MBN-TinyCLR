using MBN;
using MBN.Modules;

using System;
using System.Diagnostics;
using System.Text;
using System.Threading;

namespace Examples
{
    internal class Program
    {
        private static void Main()
        {
            _macAddressClick = new MACAddressClick(Hardware.SC20260_2, MACAddressClick.I2CAddresses.Address0);

            Debug.WriteLine($"MACAddress-EIU48 is {MACAddressToString(_macAddressClick.MACAddress_EIU48)}");
            Debug.WriteLine($"MACAddress-EIU64 is {MACAddressToString(_macAddressClick.MACAddress_EIU64)}");

            ClearUserEEPROM();
            WriteEEPROMTest();
            ReadEEPROMTest();

            Thread.Sleep(-1);
        }

        // Make sure that you do not write past the end of user EEPROM which is 0x80.
        // Page size is limited to 16 Byte Writes. If more than 16 bytes are written, it will roll back to
        // the starting address and overwrite the previously written data.
        private static void WriteEEPROMTest()
        {
            ClearUserEEPROM();

            String[] testString = new String[4];

            testString[0] = "MBN Software MAC";
            testString[1] = " Address Click E";
            testString[2] = "EPROM Write Test";
            testString[3] = " on TinyCLR 2.0 ";

            Byte[] txBuffer = new Byte[16 + 1];

            for (Int32 x = 0; x < 4; x++)
            {
                txBuffer[0] = (Byte)(16 * x);
                Byte[] testStringBytes = Encoding.UTF8.GetBytes(testString[x]);
                testStringBytes.CopyTo(txBuffer, 1);
                _macAddressClick.Device.Write(txBuffer);

                Thread.Sleep(100);
            }
        }

        private static void ClearUserEEPROM()
        {
            Byte[] txBuffer = new Byte[17];

            for (Int32 x = 0; x < 6; x++)
            {
                txBuffer[0] = (Byte)(16 * x);
                String clearString = new String(' ', 16);
                Byte[] clearStringBytes = Encoding.UTF8.GetBytes(clearString);
                clearStringBytes.CopyTo(txBuffer, 1);
                _macAddressClick.Device.Write(txBuffer);

                Thread.Sleep(100);
            }
        }

        private static void ReadEEPROMTest()
        {
            Byte[] readBuffer = new Byte[63]; // This is the length of the test string written to EEPROM.
            const Byte startAddress = 0; // This is the start address where we wrote the data to the EEPROM.

            _macAddressClick.Device.WriteRead(new[] { startAddress }, readBuffer);

            Debug.WriteLine($"Read from EEPROM contents : {Encoding.UTF8.GetString(readBuffer)}");
        }

        private static String MACAddressToString(Byte[] macAddress)
        {
            if (macAddress == null) throw new ArgumentNullException(nameof(macAddress));

            StringBuilder sb = new StringBuilder();
            sb.Append(macAddress[0].ToString("x2"));
            sb.Append(":");
            sb.Append(macAddress[1].ToString("x2"));
            sb.Append(":");
            sb.Append(macAddress[2].ToString("x2"));
            sb.Append(":");
            sb.Append(macAddress[3].ToString("x2"));
            sb.Append(":");
            sb.Append(macAddress[4].ToString("x2"));
            sb.Append(":");
            sb.Append(macAddress[5].ToString("x2"));

            if (macAddress.Length == 6) return sb.ToString();

            sb.Append(macAddress[6].ToString("x2"));
            sb.Append(":");
            sb.Append(macAddress[7].ToString("x2"));

            return sb.ToString();
        }

        private static MACAddressClick _macAddressClick;
    }
}
