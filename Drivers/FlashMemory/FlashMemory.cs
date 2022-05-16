/*
 * Flash Click driver for TinyCLR 2.0
 * 
 * Version 1.0 :
 * - Initial revision
 * 
 * Copyright 2020 MikroBus.Net
 * Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License. You may obtain a copy of the License at
 * http://www.apache.org/licenses/LICENSE-2.0
 * Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, 
 * either express or implied. See the License for the specific language governing permissions and limitations under the License.
 */

#if (NANOFRAMEWORK_1_0)
using System.Device.Gpio;
using System.Device.Spi;
#else
using GHIElectronics.TinyCLR.Devices.Gpio;
using GHIElectronics.TinyCLR.Devices.Spi;
#endif

using System;
using System.Threading;

namespace MBN.Modules
{
    /// <summary>
    /// Main class for the FlashClick driver
    /// <para><b>Pins used :</b> Miso, Mosi, Cs, Sck, Pwm, Rst</para>
    /// </summary>
    /// <example>
    /// <code language="C#">
    /// public class Program
    /// {
    ///     private static Storage _flash;
    ///
    ///     public static void Main()
    ///     {
    ///        _flash = new FlashMemory(Hardware.SocketOne);
    ///
    ///        Debug.Print("Address 231 before : " + _flash.ReadByte(231));
    ///        _flash.WriteByte(231, 123);
    ///        Debug.Print("Address 231 after : " + _flash.ReadByte(231));
    ///
    ///        _flash.WriteData(400, new Byte[] { 100, 101, 102 }, 0, 3);
    ///        _flash.ReadData(400, bArray, 0, 3);
    ///        Debug.Print("Read 3 bytes starting @400 (should be 100, 101, 102) : " + bArray[0] + ", " + bArray[1] + ", " + bArray[2]);
    ///     }
    /// </code>
    /// </example>
    public sealed class FlashMemory : Storage
    {
        private readonly SpiDevice _flash;
        private readonly Int32 _PageSize = 0x100;

        // Fill in those values manually if they are not detected in the DetectParameters() method
        private Int32 _SectorSize = 0x1000, _BlockSize = 0x10000, _Capacity = 0x800000;
        private Byte _SectorEraseInstruction = 0x20;
        private Byte _BlockEraseInstruction = 0xD8;
        private readonly Hardware.Socket _socket;

        public override Int32 Capacity => _Capacity;
        public override Int32 PageSize => _PageSize;
        public override Int32 SectorSize => _SectorSize;
        public override Int32 BlockSize => _BlockSize;

        public GpioPin LedIndicator { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="FlashMemory"/> class.
        /// </summary>
        /// <param name="socket">The socket on the MBN mainboard.</param>
        public FlashMemory(Hardware.Socket socket, Boolean detectParameters = true, Int32 hold = -1, Int32 wp = -1)
        {
            _socket = socket;
#if (NANOFRAMEWORK_1_0)
            _flash = SpiDevice.Create(new SpiConnectionSettings(socket.SpiBus, socket.Cs)
            {
                Mode = SpiMode.Mode0,
                ClockFrequency = 10000000
            });

            if (wp != -1)
            {
                GpioPin _wp = new GpioController().OpenPin(wp, PinMode.Output);
                _wp.Write(PinValue.High);
            }

            if (hold != -1)
            {
                GpioPin _hold = new GpioController().OpenPin(hold, PinMode.Output);
                _hold.Write(PinValue.High);
            }
#else
            _flash = SpiController.FromName(socket.SpiBus).GetDevice(new SpiConnectionSettings()
            {
                ChipSelectType = SpiChipSelectType.Gpio,
                ChipSelectLine = GpioController.GetDefault().OpenPin(socket.Cs),
                Mode = SpiMode.Mode0,
                ClockFrequency = 10000000
            });

            if (wp != -1)
            {
                GpioPin _wp = GpioController.GetDefault().OpenPin(wp);
                _wp.SetDriveMode(GpioPinDriveMode.Output);
                _wp.Write(GpioPinValue.High);
            }

            if (hold != -1)
            {
                GpioPin _hold = GpioController.GetDefault().OpenPin(hold);
                _hold.SetDriveMode(GpioPinDriveMode.Output);
                _hold.Write(GpioPinValue.High);
            }
#endif

            if (detectParameters) DetectParameters();
        }

        private Boolean WriteEnabled()
        {
            var data2 = new Byte[2];

            lock (_socket.LockSpi)
            {
                _flash.Write(new Byte[] { 0x06 });
                _flash.TransferFullDuplex(new Byte[] { 0x05, 0x00 }, data2);
            }

            return (data2[1] & 0x02) != 0;
        }

        private Boolean WriteInProgress()
        {
            var data2 = new Byte[2];

            lock (_socket.LockSpi)
            {
                _flash.TransferFullDuplex(new Byte[] { 0x05, 0x00 }, data2);
            }

            return (data2[1] & 0x01) != 0x00;
        }

        /// <summary>
        /// Erases "count" blocks starting at "block".
        /// </summary>
        /// <param name="block">The starting block.</param>
        /// <param name="count">The count of blocks to erase.</param>
        /// <exception cref="ArgumentException">Invalid block + count</exception>
        /// <example>
        ///   <code language="C#">
        /// public class Program
        /// {
        /// private static Storage _storage;
        /// public static void Main()
        /// {
        /// _storage = new FlashMemory();
        /// _storage.EraseBlock(10,1);
        /// }
        /// </code>
        /// </example>
        public override void EraseBlock(Int32 block, Int32 count)
        {
            if ((block + count) * BlockSize > Capacity) throw new ArgumentException("Invalid block + count");
#if (NANOFRAMEWORK_1_0)
            if (LedIndicator != null) LedIndicator.Write(PinValue.High);
#else
            if (LedIndicator != null) LedIndicator.Write(GpioPinValue.High);
#endif

            var data4 = new Byte[4];
            var address = block * BlockSize;

            for (var i = 0; i < count; i++)
            {
                data4[0] = _BlockEraseInstruction;
                data4[1] = (Byte)(address >> 16);
                data4[2] = (Byte)(address >> 8);
                data4[3] = (Byte)(address >> 0);
                while (!WriteEnabled()) { }
                lock (_socket.LockSpi)
                {
                    _flash.Write(data4);
                }
                while (WriteInProgress()) { }
                address += BlockSize;
            }
#if (NANOFRAMEWORK_1_0)
            if (LedIndicator != null) LedIndicator.Write(PinValue.Low);
#else
            if (LedIndicator != null) LedIndicator.Write(GpioPinValue.Low);
#endif
        }

        /// <summary>
        /// Completely erases the chip.
        /// </summary>
        /// <remarks>
        /// This method is mainly used by Flash memory chips, because of their internal behaviour. It can be safely ignored with other memory types.
        /// </remarks>
        /// <example>
        ///   <code language="C#">
        /// public class Program
        /// {
        /// private static Storage _storage;
        /// public static void Main()
        /// {
        /// _storage = new FlashMemory();
        /// _storage.EraseChip();
        /// }
        /// </code>
        /// </example>
        public override void EraseChip()
        {
#if (NANOFRAMEWORK_1_0)
            if (LedIndicator != null) LedIndicator.Write(PinValue.High);
#else
            if (LedIndicator != null) LedIndicator.Write(GpioPinValue.High);
#endif
            while (!WriteEnabled()) { }
            lock (_socket.LockSpi)
            {
                _flash.Write(new Byte[] { 0xC7 });
            }
            while (WriteInProgress()) { Thread.Sleep(20); }
#if (NANOFRAMEWORK_1_0)
            if (LedIndicator != null) LedIndicator.Write(PinValue.Low);
#else
            if (LedIndicator != null) LedIndicator.Write(GpioPinValue.Low);
#endif
        }

        /// <summary>
        /// Erases "count" sectors starting at "sector".
        /// </summary>
        /// <param name="sector">The starting sector.</param>
        /// <param name="count">The count of sectors to erase.</param>
        /// <exception cref="ArgumentException">Invalid sector + count</exception>
        /// <example>
        ///   <code language="C#">
        /// public class Program
        /// {
        /// private static Storage _storage;
        /// public static void Main()
        /// {
        /// _storage = new FlashMemory();
        /// _storage.EraseSector(10,1);
        /// }
        /// </code>
        /// </example>
        public override void EraseSector(Int32 sector, Int32 count)
        {
            if ((sector + count) * SectorSize > Capacity) throw new ArgumentException("Invalid sector + count");
#if (NANOFRAMEWORK_1_0)
            if (LedIndicator != null) LedIndicator.Write(PinValue.High);
#else
            if (LedIndicator != null) LedIndicator.Write(GpioPinValue.High);
#endif

            var data4 = new Byte[4];
            var address = sector * SectorSize;

            for (var i = 0; i < count; i++)
            {
                data4[0] = _SectorEraseInstruction;
                data4[1] = (Byte)(address >> 16);
                data4[2] = (Byte)(address >> 8);
                data4[3] = (Byte)(address >> 0);
                while (!WriteEnabled()) { }
                lock (_socket.LockSpi)
                {
                    _flash.Write(data4);
                }
                while (WriteInProgress()) { }
                address += SectorSize;
            }

#if (NANOFRAMEWORK_1_0)
            if (LedIndicator != null) LedIndicator.Write(PinValue.Low);
#else
            if (LedIndicator != null) LedIndicator.Write(GpioPinValue.Low);
#endif
        }

        /// <summary>
        /// Reads data from Flash.
        /// </summary>
        /// <param name="address">The address.</param>
        /// <param name="array">The array.</param>
        /// <param name="index">The index.</param>
        /// <param name="count">The count.</param>
        /// <exception cref="ArgumentException">
        /// Invalid index + count
        /// or
        /// Invalid address + count
        /// </exception>
        public override void ReadData(Int32 address, Byte[] data, Int32 index, Int32 count)
        {
#if (NANOFRAMEWORK_1_0)
            if (LedIndicator != null) LedIndicator.Write(PinValue.High);
#else
            if (LedIndicator != null) LedIndicator.Write(GpioPinValue.High);
#endif

            var data4 = new Byte[4 + data.Length];
            data4[0] = 0x03;
            data4[1] = (Byte)(address >> 16);
            data4[2] = (Byte)(address >> 8);
            data4[3] = (Byte)(address >> 0);

            while (!WriteEnabled()) { }
            lock (_socket.LockSpi)
            {
                _flash.TransferFullDuplex(data4, data4);
            }
            while (WriteInProgress()) { }
            Array.Copy(data4, 4, data, index, count);

#if (NANOFRAMEWORK_1_0)
            if (LedIndicator != null) LedIndicator.Write(PinValue.Low);
#else
            if (LedIndicator != null) LedIndicator.Write(GpioPinValue.Low);
#endif
        }

        /// <summary>
        /// Writes data to a memory location.
        /// </summary>
        /// <param name="address">The address to write to.</param>
        /// <param name="array">The data to write.</param>
        /// <param name="index">The starting index in the data array.</param>
        /// <param name="count">The count of bytes to write to memory.</param>
        /// <example>
        ///   <code language="C#">
        /// public class Program
        /// {
        /// private static Storage _storage;
        /// public static void Main()
        /// {
        /// _storage = new FlashMemory();
        /// _storage.WriteData(400, new Byte[] { 100, 101, 102 }, 0, 3);
        /// _storage.ReadData(400, bArray, 0, 3);
        /// Debug.Print("Read 3 bytes starting @400 (should be 100, 101, 102) : " + bArray[0] + ", " + bArray[1] + ", " + bArray[2]);
        /// }
        /// </code>
        /// </example>
        /// <exception cref="System.ArgumentException">
        /// Invalid index + count
        /// or
        /// Invalid address + count
        /// </exception>
        public override void WriteData(Int32 address, Byte[] data, Int32 index, Int32 count)
        {
#if (NANOFRAMEWORK_1_0)
            if (LedIndicator != null) LedIndicator.Write(PinValue.High);
#else
            if (LedIndicator != null) LedIndicator.Write(GpioPinValue.High);
#endif

            var len = _PageSize - (address & 0xFF); // remaining of first page
            while (count > 0)
            {
                if (len > count) len = count;
                var wr_cmd = new Byte[len + 4];
                wr_cmd[0] = 0x02;
                wr_cmd[1] = (Byte)(address >> 16);
                wr_cmd[2] = (Byte)(address >> 8);
                wr_cmd[3] = (Byte)address;
                Array.Copy(data, index, wr_cmd, 4, len);
                while (!WriteEnabled()) { }
                lock (_socket.LockSpi)
                {
                    _flash.Write(wr_cmd);
                }
                while (WriteInProgress()) { }
                address += len;
                count -= len;
                index += len;
                len = _PageSize;
            }

#if (NANOFRAMEWORK_1_0)
            if (LedIndicator != null) LedIndicator.Write(PinValue.Low);
#else
            if (LedIndicator != null) LedIndicator.Write(GpioPinValue.Low);
#endif
        }

        /// <summary>
        /// Detects memory chip features by reading JEDEC SFDP information.
        /// </summary>
        public void DetectParameters()
        {
#if (NANOFRAMEWORK_1_0)
            if (LedIndicator != null) LedIndicator.Write(PinValue.High);
#else
            if (LedIndicator != null) LedIndicator.Write(GpioPinValue.High);
#endif

            var sfdp = new Byte[165];
            while (!WriteEnabled()) { }
            sfdp[0] = 0x5A;
            lock (_socket.LockSpi)
            {
                _flash.TransferFullDuplex(sfdp, sfdp);    // Try to read SFDP information
            }
            while (WriteInProgress()) { }
            Array.Copy(sfdp, 5, sfdp, 0, sfdp.Length - 5);
            var i = 0;
            if (sfdp[i] == 0x53 && sfdp[i + 1] == 0x46 && sfdp[i + 2] == 0x44 && sfdp[i + 3] == 0x50)   // JEDEC "SFDP" signature detected
            {
                var Offset = sfdp[0x0C];
                _SectorSize = 1 << sfdp[Offset + 0x1C];
                _SectorEraseInstruction = sfdp[Offset + 0x1D];
                _BlockSize = 1 << sfdp[Offset + 0x1E];
                _BlockEraseInstruction = sfdp[Offset + 0x1F];
                _Capacity = (((sfdp[Offset + 0x07] << 24) +
                              (sfdp[Offset + 0x06] << 16) +
                              (sfdp[Offset + 0x05] << 8) +
                               sfdp[Offset + 0x04]) >> 3) + 1;
            }
#if (NANOFRAMEWORK_1_0)
            if (LedIndicator != null) LedIndicator.Write(PinValue.Low);
#else
            if (LedIndicator != null) LedIndicator.Write(GpioPinValue.Low);
#endif
        }
    }
}
