/*
 * Qspi memory driver for TinyCLR 2.0
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
// This is currently incompatible with nanoFramework due to not supporting managed external storage.
#else
using GHIElectronics.TinyCLR.Devices.Storage;
using GHIElectronics.TinyCLR.Devices.Storage.Provider;
using GHIElectronics.TinyCLR.Pins;
using System;
using GHIElectronics.TinyCLR.Native;

namespace MBN.Modules
{
    /// <summary>
    /// Main class for the Qspi memory driver
    /// </summary>
    /// <example>
    /// <code language="C#">
    /// public class Program
    /// {
    ///     private static Storage _qspi;
    ///
    ///     public static void Main()
    ///     {
    ///        _qspi = new QspiMemory();
    ///
    ///        Debug.Print("Address 231 before : " + _qspi.ReadByte(231));
    ///        _qspi.WriteByte(231, 123);
    ///        Debug.Print("Address 231 after : " + _qspi.ReadByte(231));
    ///
    ///        _qspi.WriteData(400, new Byte[] { 100, 101, 102 }, 0, 3);
    ///        _qspi.ReadData(400, bArray, 0, 3);
    ///        Debug.Print("Read 3 bytes starting @400 (should be 100, 101, 102) : " + bArray[0] + ", " + bArray[1] + ", " + bArray[2]);
    ///     }
    /// </code>
    /// </example>
    public sealed class QspiMemory : Storage
    {
        private IStorageControllerProvider qspi;

        public override Int32 Capacity => Flash.IsEnabledExternalFlash() ? 0x00800000: 0x01000000;
        public override Int32 PageSize => 0x100;
        public override Int32 SectorSize => 0x1000;
        public override Int32 BlockSize => 0x10000;

        /// <summary>
        /// Initializes a new instance of the <see cref="QspiMemory"/> class.
        /// </summary>
        public QspiMemory()
        {
            qspi = StorageController.FromName(SC20260.StorageController.QuadSpi).Provider;
            qspi.Open();
        }

        /// <summary>
        /// Completely erases the memory.
        /// </summary>
        /// <remarks>
        /// This method is mainly used by Flash memory chips, because of their internal behaviour. It can be safely ignored with other memory types.
        /// </remarks>
        /// <example>
        ///   <code language="C#">
        /// public class Program
        /// {
        ///     private static Storage _storage;
        /// 
        ///     public static void Main()
        ///     {
        ///         _storage = new QspiMemory();
        ///         _storage.EraseChip();
        ///     }
        /// </code>
        /// </example>
        public override void EraseChip()
        {
            var sectorCount = Capacity / SectorSize;
            qspi.Erase(0, sectorCount, TimeSpan.FromSeconds(100));
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
        ///     private static Storage _storage;
        /// 
        ///     public static void Main()
        ///     {
        ///         _storage = new QspiMemory();
        ///         _storage.EraseSector(10,1);
        ///     }
        /// </code>
        /// </example>
        public override void EraseSector(Int32 sector, Int32 count)
        {
            qspi.Erase(sector * SectorSize, count, TimeSpan.FromSeconds(1));
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
        ///     private static Storage _storage;
        /// 
        ///     public static void Main()
        ///     {
        ///         _storage = new QspiMemory();
        ///         _storage.EraseBlock(10,1);
        ///     }
        /// </code>
        /// </example>
        public override void EraseBlock(Int32 block, Int32 count)
        {
            qspi.Erase(block * BlockSize, count * BlockSize / SectorSize, TimeSpan.FromSeconds(1));
        }

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
        ///     private static Storage _storage;
        ///     public static void Main()
        ///     {
        ///         _storage = new QspiMemory();
        ///         _storage.WriteData(400, new Byte[] { 100, 101, 102 }, 0, 3);
        ///         _storage.ReadData(400, bArray, 0, 3);
        ///         Debug.Print("Read 3 bytes starting @400 (should be 100, 101, 102) : " + bArray[0] + ", " + bArray[1] + ", " + bArray[2]);
        ///     }
        /// </code>
        /// </example>
        public override void WriteData(Int32 address, Byte[] data, Int32 index, Int32 count)
        {
            qspi.Write(address, count, data, index, TimeSpan.FromSeconds(1));
        }

        /// <summary>
        /// Reads data from Qspi memory.
        /// </summary>
        /// <param name="address">The address.</param>
        /// <param name="data">The array.</param>
        /// <param name="index">The index.</param>
        /// <param name="count">The count.</param>
        public override void ReadData(Int32 address, Byte[] data, Int32 index, Int32 count)
        {
            qspi.Read(address, count, data, index, TimeSpan.FromSeconds(1));
        }
    }
}
#endif
