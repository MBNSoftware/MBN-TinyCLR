/*
 * MACAddress Click driver for TinyCLR 2.0
 * 
 * Initial revision coded by Stephen Cardinale
 * 
 * Copyright 2021 Stephen Cardinale and MikroBUS.Net
 * Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License. You may obtain a copy of the License at
 * http://www.apache.org/licenses/LICENSE-2.0
 * Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, 
 * either express or implied. See the License for the specific language governing permissions and limitations under the License.
 */

using GHIElectronics.TinyCLR.Devices.I2c;

using System;

namespace MBN.Modules
{
    /// <summary>
    /// Main class for the MACAddress Click driver.
    /// <para><b>Pins used :</b> Scl and Sda</para>
    /// <para>This is an I2C Device.</para>
    /// </summary>
    /// <example>
    /// <code language = "C#">
    /// using MBN;
    /// using MBN.Modules;
    ///
    /// using System;
    /// using System.Diagnostics;
    /// using System.Text;
    /// using System.Threading;
    ///
    /// namespace Examples
    /// {
    ///     internal class Program
    ///     {
    ///         private static void Main()
    ///         {
    ///             _macAddressClick = new MACAddressClick(Hardware.SC20260_2, MACAddressClick.I2CAddresses.Address0);
    ///
    ///             Debug.WriteLine($"MACAddress-EIU48 is {MACAddressToString(_macAddressClick.MACAddress_EIU48)}");
    ///             Debug.WriteLine($"MACAddress-EIU64 is {MACAddressToString(_macAddressClick.MACAddress_EIU64)}");
    ///
    ///             ClearUserEEPROM();
    ///             WriteEEPROMTest();
    ///             ReadEEPROMTest();
    ///
    ///             Thread.Sleep(-1);
    ///         }
    ///
    ///         // Make sure that you do not write past the end of user EEPROM which is 0x80.
    ///         // Page size is limited to 16 Byte Writes. If more than 16 bytes are written, it will roll back to
    ///         // the starting address and overwrite the previously written data.
    ///         private static void WriteEEPROMTest()
    ///         {
    ///             ClearUserEEPROM();
    ///
    ///             String[] testString = new String[4];
    ///
    ///             testString[0] = "MBN Software MAC";
    ///             testString[1] = " Address Click E";
    ///             testString[2] = "EPROM Write Test";
    ///             testString[3] = " on TinyCLR 2.0 ";
    ///
    ///             Byte[] txBuffer = new Byte[16 + 1];
    ///
    ///             for(Int32 x = 0; x <![CDATA[<]]> 4; x++)
    ///             {
    ///                 txBuffer[0] = (Byte) (16 * x);
    ///                 Byte[] testStringBytes = Encoding.UTF8.GetBytes(testString[x]);
    ///                 testStringBytes.CopyTo(txBuffer, 1);
    ///                 _macAddressClick.Device.Write(txBuffer);
    ///
    ///                 Thread.Sleep(100);
    ///             }
    ///         }
    ///
    ///         private static void ClearUserEEPROM()
    ///         {
    ///             Byte[] txBuffer = new Byte[17];
    ///
    ///             for(Int32 x = 0; x <![CDATA[<]]> 6; x++)
    ///             {
    ///                 txBuffer[0] = (Byte) (16 * x);
    ///                 String clearString = new String(' ', 16);
    ///                 Byte[] clearStringBytes = Encoding.UTF8.GetBytes(clearString);
    ///                 clearStringBytes.CopyTo(txBuffer, 1);
    ///                 _macAddressClick.Device.Write(txBuffer);
    ///
    ///                 Thread.Sleep(100);
    ///             }
    ///         }
    ///
    ///         private static void ReadEEPROMTest()
    ///         {
    ///             Byte[] readBuffer = new Byte[63]; // This is the length of the test string written to EEPROM.
    ///             Byte startAddress = 0; // This is the start address where we wrote the data to the EEPROM.
    ///
    ///             _macAddressClick.Device.WriteRead(new Byte[] { startAddress }, readBuffer);
    ///
    ///             Debug.WriteLine($"Read from EEPROM contents : {Encoding.UTF8.GetString(readBuffer)}");
    ///         }
    ///
    ///         private static String MACAddressToString(Byte[] macAddress)
    ///         {
    ///             if (macAddress == null) throw new ArgumentNullException(nameof(macAddress));
    ///
    ///             StringBuilder sb = new StringBuilder();
    ///              sb.Append(macAddress[0].ToString("x2"));
    ///             sb.Append(":");
    ///             sb.Append(macAddress[1].ToString("x2"));
    ///             sb.Append(":");
    ///             sb.Append(macAddress[2].ToString("x2"));
    ///             sb.Append(":");
    ///             sb.Append(macAddress[3].ToString("x2"));
    ///             sb.Append(":");
    ///             sb.Append(macAddress[4].ToString("x2"));
    ///             sb.Append(":");
    ///             sb.Append(macAddress[5].ToString("x2"));
    ///
    ///             if (macAddress.Length == 6) return sb.ToString();
    ///
    ///             sb.Append(macAddress[6].ToString("x2"));
    ///             sb.Append(":");
    ///             sb.Append(macAddress[7].ToString("x2"));
    ///
    ///             return sb.ToString();
    ///         }
    ///
    ///         private static MACAddressClick _macAddressClick;
    ///     }
    /// }
    /// </code>
    /// </example>
    public class MACAddressClick
    {
        #region .ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="MACAddressClick"/> class using a board with a MikroBUS socket.
        /// </summary>
        /// <param name="socket">The socket that the Thermo15 click is inserted into.</param>
        /// <param name="slaveAddress">The Slave Address of the MACAddress Click.</param>
        public MACAddressClick(Hardware.Socket socket, I2CAddresses slaveAddress)
        {
            _macAdress = I2cController.FromName(socket.I2cBus).GetDevice(new I2cConnectionSettings((Int32)slaveAddress, 400000));

            _lockObject = socket.LockI2c;

            MACAddress_EIU48 = ReadRegister(MAC_ADDRESS_START, 6);
            MACAddress_EIU64 = ReadRegister(MAC_ADDRESS_START, 8);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MACAddressClick"/> class using a board without a MikroBUS socket.
        /// </summary>
        /// <param name="i2cBus">the I2C Bus to communicate with.</param>
        /// <param name="slaveAddress">The Slave Address of the MACAddress Click.</param>
        public MACAddressClick(String i2cBus, I2CAddresses slaveAddress)
        {
            _macAdress = I2cController.FromName(i2cBus).GetDevice(new I2cConnectionSettings((Int32)slaveAddress, 400000));
            _lockObject = Hardware.LockI2C;

            MACAddress_EIU48 = ReadRegister(MAC_ADDRESS_START, 6);
            MACAddress_EIU64 = ReadRegister(MAC_ADDRESS_START, 8);
        }

        #endregion

        #region Public Enumerations

        /// <summary>
        /// The available I2C Slave addresses for the MAC Address Click.
        /// </summary>
        public enum I2CAddresses
        {
            /// <summary>
            /// Adress0 with pad A0 and A1 soldered in the Zero (0) position.
            /// </summary>
            Address0 = 0x50,
            /// <summary>
            /// Adress1 with pad A0 soldered in the Zero (0) position and A1 soldered in the One (1) Position.
            /// </summary>
            Address1 = 0x51,
            /// <summary>
            /// Adress2 with pad A0 soldered in the One (1) position and A1 soldered in the Zero (0) Position.
            /// </summary>
            Address2 = 0x52,
            /// <summary>
            /// Adress3 with pad A0 and A1 soldered in the one (1) Position.
            /// </summary>
            Address3 = 0x53
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Returns the EIU-48 6-bit MAC Address from the MACAddress Click.
        /// </summary>
        /// <example>
        /// <code language = "C#">
        /// Byte[] macAddress = _macAddressClick.MACAddress_EIU48;
        ///
        /// for (int x = 0; x <![CDATA[<]]> 6; x++)
        /// {
        ///     Debug.WriteLine($"Mac Address EIU-48 Byte [{x}] is {macAddress[x]}h");
        /// }
        /// </code>
        /// </example>
        public Byte[] MACAddress_EIU48 { get; }

        /// <summary>
        /// <code language = "C#">
        /// Byte[] macAddress = _macAddressClick.MACAddress_EIU64;
        ///
        /// for (int x = 0; x <![CDATA[<]]> 6; x++)
        /// {
        ///     Debug.WriteLine($"Mac Address EIU-648 Byte [{x}] is {macAddress[x]}h");
        /// }
        /// </code>
        /// </summary>
        public Byte[] MACAddress_EIU64 { get; }

        /// <summary>
        /// Exposes the underlying I2c Device to be used if you wish to write to the User EEProm area of the MACAddress Click.
        /// </summary>
        /// <example>Example usage:
        /// <code>
        /// Byte[] readBuffer = new Byte[63]; // This is the length of the test string written to EEPROM.
        /// Byte startAddress = 0; // This is the start address where we wrote the data to the EEPROM.
        ///
        /// _macAddressClick.Device.WriteRead(new Byte[] { startAddress }, readBuffer);
        /// 
        /// Debug.WriteLine($"Read from EEPROM contents : {Encoding.UTF8.GetString(readBuffer)}");
        /// </code>
        /// </example>
        public I2cDevice Device
        {
            get
            {
                lock (_lockObject)
                {
                    return _macAdress;
                }
            }
        }

        #endregion

        #region Private Fields

        private readonly I2cDevice _macAdress;
        private readonly Object _lockObject;

        #endregion

        #region Constants

        private const Byte MAC_ADDRESS_START = 0xF8;

        /// <summary>
        /// The end address of user eeprom.
        /// <remarks>Do not attempt to write past the end of user eeprom.</remarks>
        /// </summary>
        public const Byte EEPROM_END_ADDRESS = 0x80;

        #endregion

        #region Private Methods

        private Byte[] ReadRegister(Byte registerAddress, Byte numberOfBytesToRead)
        {
            Byte[] readBuffer = new Byte[numberOfBytesToRead];

            lock (_lockObject)
            {
                _macAdress.WriteRead(new[] {registerAddress}, readBuffer);
            }

            return readBuffer;
        }

        #endregion
    }
}
