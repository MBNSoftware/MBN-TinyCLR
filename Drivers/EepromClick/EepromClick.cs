/*
 * Eeprom Click driver for TinyCLR 2.0
 * 
 * Version 1.0 :
 * - Initial revision
 * 
 * Copyright 2018 MikroBus.Net
 * Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License. You may obtain a copy of the License at
 * http://www.apache.org/licenses/LICENSE-2.0
 * Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, 
 * either express or implied. See the License for the specific language governing permissions and limitations under the License.
 */

#if (NANOFRAMEWORK_1_0)
using gpio = System.Device.Gpio;
using System.Device.I2c;
#else
using GHIElectronics.TinyCLR.Devices.I2c;
using gpio = GHIElectronics.TinyCLR.Devices.Gpio;
#endif

using System;
using System.Threading;

namespace MBN.Modules
{
    /// <summary>
    /// Main class for the EEpromClick driver
    /// <para><b>Pins used :</b> Scl, Sda, Pwm</para>
    /// </summary>
    /// <example>
    /// <code language="C#">
    /// public class Program
    /// {
    ///     private static Storage _eeprom;
    ///
    ///     public static void Main()
    ///     {
    ///        _eeprom = new EEpromClick(Hardware.SocketOne, memorySize: 256);  // Here, the original 8KB chip has been replaced by a 256KB one
    ///
    ///        Debug.Print("Address 231 before : " + _eeprom.ReadByte(231));
    ///        _eeprom.WriteByte(231, 123);
    ///        Debug.Print("Address 231 after : " + _eeprom.ReadByte(231));
    ///
    ///        _eeprom.WriteData(400, new Byte[] { 100, 101, 102 }, 0, 3);
    ///        _eeprom.ReadData(400, bArray, 0, 3);
    ///        Debug.Print("Read 3 bytes starting @400 (should be 100, 101, 102) : " + bArray[0] + ", " + bArray[1] + ", " + bArray[2]);
    ///     }
    /// </code>
    /// </example>
    public sealed class EEpromClick : Storage
    {
        /// <summary>
        /// Gets the memory capacity.
        /// </summary>
        /// <value>
        /// The maximum capacity, in KBytes. Ex: 256 for 256KB
        /// </value>
        /// <remarks>
        /// 1MB = 1024KB, so 8MB will be : 8*1024 = 8192KB
        /// </remarks>
        public override Int32 Capacity { get { return _memorysize; } }
        /// <summary>
        /// Gets the size of a page in memory.
        /// </summary>
        /// <value>
        /// The size of a page in bytes
        /// </value>
        public override Int32 PageSize { get { return 64; } }
        /// <summary>
        /// Gets the size of a sector.
        /// </summary>
        /// <value>
        /// The size of a sector in bytes.
        /// </value>
        public override Int32 SectorSize { get { return 256; } }

        public override Int32 BlockSize
        {
            get { throw new NotImplementedException(); }
        }

        /// <summary>
        /// Gets the number of pages per cluster.
        /// </summary>
        /// <value>
        /// The number of pages per cluster.
        /// </value>
        public override Int32 PagesPerCluster { get { return 1; } }

        private readonly I2cDevice _eeprom;
        private readonly Int32 _memorysize;
        private readonly Hardware.Socket _socket;

        /// <summary>
        /// Initializes a new instance of the <see cref="EEpromClick"/> class.
        /// </summary>
        /// <param name="socket">The socket on which the EEpromClick module is plugged on MikroBus.Net board</param>
        /// <param name="address">The address of the module.</param>
        /// <param name="memorySize">Optionnal size of the memory chip, in KB. Default to 8KB, as sold by MikroElektronika.</param>
        public EEpromClick(Hardware.Socket socket, Byte address, Int32 memorySize = 8)
        {
            _socket = socket;
            // Create the driver's I²C configuration
#if (NANOFRAMEWORK_1_0)
            _eeprom = I2cDevice.Create(new I2cConnectionSettings(socket.I2cBus, address, I2cBusSpeed.StandardMode));
            _memorysize = memorySize * 1024;
            gpio.GpioPin _wp = new gpio.GpioController().OpenPin(socket.PwmPin);
            _wp.SetPinMode(gpio.PinMode.Output);
            _wp.Write(gpio.PinValue.Low);
#else
            _eeprom = I2cController.FromName(socket.I2cBus).GetDevice(new I2cConnectionSettings(address, 100000));
            _memorysize = memorySize * 1024;
            gpio.GpioPin _wp = gpio.GpioController.GetDefault().OpenPin(socket.PwmPin);
            _wp.SetDriveMode(gpio.GpioPinDriveMode.Output);
            _wp.Write(gpio.GpioPinValue.Low);
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
        /// _storage = new OnboardFlash();
        /// _storage.EraseChip();
        /// }
        /// </code>
        /// </example>
        public override void EraseChip() { }

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
        /// _storage = new OnboardFlash();
        /// _storage.EraseSector(10,1);
        /// }
        /// </code>
        /// </example>
        public override void EraseSector(Int32 sector, Int32 count)
        {
            if ((sector + count) * SectorSize > Capacity) throw new ArgumentException("Invalid sector + count");
            var address = sector * SectorSize;
            var data3 = new Byte[SectorSize + 2];
            for (var i = 0; i < count; i++)
            {
                data3[0] = (Byte)(address >> 8);
                data3[1] = (Byte)(address >> 0);
                lock (_socket.LockI2c)
                {
                    _eeprom.Write(data3);
                }
                Thread.Sleep(5); // Mandatory after each Write transaction !!!
                address += SectorSize;
            }
        }

        public override void EraseBlock(Int32 block, Int32 count) => throw new NotImplementedException();

        /// <summary>
        /// Writes data to a memory location.
        /// </summary>
        /// <param name="address">The address to write to.</param>
        /// <param name="data">The data to write.</param>
        /// <param name="index">The starting index in the data array.</param>
        /// <param name="count">The count of bytes to write to memory.</param>
        /// <example>
        ///   <code language="C#">
        /// public class Program
        /// {
        /// private static Storage _storage;
        /// public static void Main()
        /// {
        /// _storage = new EEpromClick(Hardware.Socket2);
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
            var buffer = new Byte[data.Length + 2];
            buffer[0] = (Byte)(address >> 8);
            buffer[1] = (Byte)(address & 0xFF);
            Array.Copy(data, index, buffer, 2, count);
            lock (_socket.LockI2c)
            {
                _eeprom.Write(buffer);
            }
            Thread.Sleep(5); // Mandatory after each Write transaction !!!
        }

        /// <summary>
        /// Reads data at a specific address.
        /// </summary>
        /// <param name="address">The address to read from.</param>
        /// <param name="data">An array of bytes containing data read back</param>
        /// <param name="index">The starting index to read in the array.</param>
        /// <param name="count">The count of bytes to read.</param>
        /// <example>
        ///   <code language="C#">
        /// public class Program
        /// {
        /// private static Storage _storage;
        /// public static void Main()
        /// {
        /// _storage = new FlashClick(Hardware.Socket1);
        /// _storage.WriteData(400, new Byte[] { 100, 101, 102 }, 0, 3);
        /// _storage.ReadData(400, bArray, 0, 3);
        /// Debug.Print("Read 3 bytes starting @400 (should be 100, 101, 102) : " + bArray[0] + ", " + bArray[1] + ", " + bArray[2]);
        /// }
        /// </code>
        /// </example>
        public override void ReadData(Int32 address, Byte[] data, Int32 index, Int32 count)
        {
            lock (_socket.LockI2c)
            {
                _eeprom.Write(new[] { (Byte)(address >> 8), (Byte)(address & 0xFF) });
            }
            Thread.Sleep(5);   // Mandatory after each Write transaction !!!
            var buffer = new Byte[count];
            lock (_socket.LockI2c)
            {
                _eeprom.Read(buffer);
            }
            Array.Copy(buffer, 0, data, index, count);
        }
    }
}
