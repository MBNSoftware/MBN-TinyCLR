/*
 * UniqueID Click Driver for TinyCLR 2.0
 * 
 * Version 1.0 :
 *  - Initial revision coded by Stephen Cardinale
 *  
 * Copyright © 2020 MikroBus.Net
 * Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License. You may obtain a copy of the License at
 * http://www.apache.org/licenses/LICENSE-2.0
 * Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, 
 * either express or implied. See the License for the specific language governing permissions and limitations under the License.
 */
#if (NANOFRAMEWORK_1_0)
using nanoFramework.Device.OneWire;
#else
using GHIElectronics.TinyCLR.Devices.OneWire;
#endif

using System;
using System.Collections;

namespace MBN.Modules
{
    /// <summary>
    /// MikroBusNet TinyCLR Driver for the MikroE UniqueID Click.
    /// <para><b>Pins used :</b>User selectable by solder jumper -  An or Pwm</para>
    /// </summary>
    /// <example>
    /// <code language="C#">
    /// using MBN;
    ///
    /// using System;
    /// using System.Collections;
    /// using System.Diagnostics;
    /// using System.Threading;
    /// using MBN.Modules;
    ///
    /// namespace Examples
    /// {
    ///     public class Program
    ///     {
    ///         private static UniqueIDClick _uniqueIdClick;
    ///
    ///         public static void Main()
    ///         {
    ///             _uniqueIdClick = new UniqueIDClick(Hardware.SC20100_2, UniqueIDClick.GpioSelect.GP0);
    ///
    ///             ArrayList devices = new ArrayList();
    ///
    ///             foreach (var device in _uniqueIdClick.DeviceList)
    ///             {
    ///                 devices.Add(_uniqueIdClick.GetSerialNumber((Byte[])device));
    ///             }
    ///
    ///             foreach (var i in devices)
    ///             {
    ///                 Debug.WriteLine($"Device with Serial Number {i} has One-Wire Address of {GetDeviceId(_uniqueIdClick.SerialNumberToOneWireAddress((Int64)i))}");
    ///             }
    ///
    ///             Thread.Sleep(Timeout.Infinite);
    ///         }
    ///
    ///         private static String GetDeviceId(Byte[] id)
    ///         {
    ///             return $"[0x{id[0]:x2}, 0x{id[1]:x2}, 0x{id[2]:x2}, 0x{id[3]:x2}, 0x{id[4]:x2}, 0x{id[5]:x2}, 0x{id[6]:x2}, 0x{id[7]:x2}]";
    ///         }
    ///     }
    /// }
    /// </code>
    /// </example>
    public class UniqueIDClick
    {
        #region CTOR/DTOR

        /// <summary>
        ///     Initializes a new instance of the <see cref="UniqueIDClick" /> class.
        /// </summary>
        /// <param name="socket">The <see cref="Hardware.Socket" /> that the sensor is plugged in to.</param>
        /// <param name="gpio">The pin to use as the One-wire port. </param>
        public UniqueIDClick(Hardware.Socket socket, GpioSelect gpio)
        {
#if (NANOFRAMEWORK_1_0)
            _interface = new OneWireHost();
#else
            _interface = new OneWireController(gpio == GpioSelect.GP0 ? socket.PwmPin : socket.AnPin);
#endif

            _deviceList = new ArrayList();
            _deviceList = GetDeviceList();

            if (_deviceList.Count == 0) throw new DeviceInitialisationException("UniqueID Click not found on the OneWire Bus.");
        }

#endregion

#region Fields

        private static ArrayList _deviceList;
#if (NANOFRAMEWORK_1_0)
        private static OneWireHost _interface;
#else
        private static OneWireController _interface;
#endif

#endregion

#region Constants

        private const Byte DeviceFamilyCode = 0x01;

#endregion

#region Public ENUMS

        /// <summary>
        ///     Jumper position on JP1 for GPIO Pin selection for the One-Wire bus.
        /// </summary>
        public enum GpioSelect
        {
            /// <summary>
            ///     Use GPIO position 0 for the PWM Pin on the MikroBus Socket. (Factory default)
            /// </summary>
            GP0,

            /// <summary>
            ///     Use GPIO position 1 for the An Pin on the MikroBus Socket.
            /// </summary>
            GP1
        }

#endregion

#region Private Methods

        private static Byte CalculateCrc(Byte[] data, Int32 length)
        {
            Byte crc = 0;

            for (Byte i = 0; i < length; i++)
            {
                var inputByte = data[i];
                for (Byte j = 0; j < 8; j++)
                {
                    var mix = (Byte) ((crc ^ inputByte) & 0x01);
                    crc >>= 1;
                    if (mix != 0) crc ^= 0x8C;

                    inputByte >>= 1;
                }
            }
            return crc;
        }

        #endregion

        #region Public Properties

        /// <summary>
        ///     Exposes the native TinyCLR 1-Wire Bus.
        /// </summary>
#if (NANOFRAMEWORK_1_0)
        public OneWireHost Interface => _interface;
#else
        public OneWireController Interface => _interface;
#endif

        /// <summary>
        ///     Returns an ArrayList of the unique 64-Bit Addresses of all UniqueID Clicks on the One-Wire Bus.
        /// </summary>
        /// <example>
        /// <code language = "C#" >
        /// foreach (var device in _uniqueIdClick.GetDeviceList())
        /// {
        ///     Debug.WriteLine("Device found with ID of " + BitConverter.ToInt64((byte[]) device, 0));
        /// }
        /// </code>
        /// </example>
        public ArrayList DeviceList
        {
            get
            {
                GetDeviceList();
                return _deviceList;
            }
        }

        /// <summary>
        ///     Gets the number of UniqueID Clicks on the One-Wire Bus.
        /// </summary>
        /// <returns>The count of devices.</returns>
        /// <example>
        /// <code language = "C#" >
        /// Debug.WriteLine("Number of devices on One-Wire Bus is " + _uniqueIdClick.NumberOfDevices());
        /// </code>
        /// </example >
        public Int32 NumberOfDevices()
        {
            GetDeviceList();
            return _deviceList.Count;
        }

#endregion

#region Pubic Methods

        /// <summary>
        ///     Returns an ArrayList of all DS2401 sensors on the OneWire Bus.
        /// </summary>
        /// <returns>An <see cref="ArrayList" /> of all DS2401 Sensors on the OneWire Bus.</returns>
        /// <example>
        ///     Example usage:
        ///     <code language="C#">
        /// foreach (byte[] device in _uniqueID.DeviceList)
        ///  {
        ///     Debug.WriteLine(BitConverter.ToInt64(device, 0).ToString());
        ///  }
        /// </code>
        /// </example>
        public ArrayList GetDeviceList()
        {
            _deviceList.Clear();

            ArrayList tempList = Interface.FindAllDevices();

            foreach (Byte[] id in tempList)
            {
                if (id[0] == DeviceFamilyCode)
                {
                    _deviceList.Add(id);
                }
            }

            return _deviceList;
        }

        /// <summary>
        ///     Determines if the DeviceId is valid and on the One-wire Bus.
        /// </summary>
        /// <param name="id">The 64-bit Device ID to verify.</param>
        /// <returns>True if the Device ID is valid, otherwise, false.</returns>
        /// <example>
        ///     Example usage:
        ///     <code language="C#">
        ///  var fakeDevice = new byte[] {0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08};
        ///  Debug.WriteLine("Is Device Valid? - " + _uniqueIdClick.IsValidId(fakeDevice));
        /// 
        ///  foreach (byte[] device in _uniqueIdClick.DeviceList)
        ///  {
        ///      Debug.WriteLine("Is Device Valid? - " + _uniqueIdClick.IsValidId(device));
        ///  }
        ///  </code>
        /// </example>
        public Boolean IsValidId(Byte[] id)
        {
            // Refresh the DeviceList.
            GetDeviceList();

            var enumerator = DeviceList.GetEnumerator();
            enumerator.Reset();

            while (enumerator.MoveNext())
            {
                Byte[] de = (Byte[]) enumerator.Current;
                if (de != null && System.BitConverter.ToInt64(de, 0) == System.BitConverter.ToInt64(id, 0)) return true;
            }

            return false;
        }

        /// <summary>
        ///     Returns the laser etched 64-Bit Serial Number of the of the UniqueID Click.
        /// </summary>
        /// <param name="oneWireAddress">The 64-bit One-Wire Address of the UniqueID click on the One-wire Bus.</param>
        /// <remarks>The Serial Number returned does not include the One-wireDevice Family code and the checksum</remarks>
        /// <returns>The Serial Number of the UniqueID Click.</returns>
        /// <example>
        ///     <code language="C#">
        ///  var devices = new ArrayList();
        /// 
        ///  foreach (var device in _uniqueIdClick.DeviceList)
        ///  {
        ///      devices.Add(_uniqueIdClick.GetSerialNumber((byte[])device));
        ///  }
        ///     
        ///  foreach (var i in devices)
        ///  {
        ///      Debug.WriteLine("Device with Serial Number " + i + " has One-Wire Address of " + GetDeviceId(_uniqueIdClick.SerialNumberToOneWireAddress((long)i)));
        ///  }
        ///  </code>
        /// </example>
        public Int64 GetSerialNumber(Byte[] oneWireAddress)
        {
            return System.BitConverter.ToInt32(new[] {oneWireAddress[1], oneWireAddress[2], oneWireAddress[3], oneWireAddress[4], oneWireAddress[5]}, 0);
        }

        /// <summary>
        ///     Returns the laser etched 64-Bit One-Wire Address of the UniqueID Click.
        /// </summary>
        /// <param name="serialNumber">The serial number of the UniqueID click on the One-wire Bus.</param>
        /// <remarks>The One-wire Address returned includes the One-wireDevice Family code and the checksum</remarks>
        /// <returns>The 64-Bit One-Wire Address of the UniqueID Click.</returns>
        /// <example>
        ///     <code language="C#">
        ///  var devices = new ArrayList();
        /// 
        ///  foreach (var device in _uniqueIdClick.DeviceList)
        ///  {
        ///      devices.Add(_uniqueIdClick.GetSerialNumber((byte[])device));
        ///  }
        ///     
        ///  foreach (var i in devices)
        ///  {
        ///      Debug.WriteLine("Device with Serial Number " + i + " has One-Wire Address of " + GetDeviceId(_uniqueIdClick.SerialNumberToOneWireAddress((long)i)));
        ///  }
        ///  </code>
        /// </example>
        public Byte[] SerialNumberToOneWireAddress(Int64 serialNumber)
        {
            var sn = Utility.CombineArrays(new[] {DeviceFamilyCode}, Utility.ExtractRangeFromArray(System.BitConverter.GetBytes(serialNumber), 0, 6));
            return Utility.CombineArrays(sn, new[] {CalculateCrc(sn, 7)});
        }

#endregion
    }

#if (NANOFRAMEWORK_1_0)
    //This class is already in Thermo2Click, so we will just use that (should really split it out to generic location.)
#else
    internal class Utility
    {
       internal static Byte[] CombineArrays(Byte[] firstByteArray, Byte[] secondByteArray)
       {
           Byte[] combinedArray = new Byte[secondByteArray.Length + firstByteArray.Length];
           Array.Copy(firstByteArray, 0, combinedArray, 0, firstByteArray.Length);
           Array.Copy(secondByteArray, 0, combinedArray, firstByteArray.Length, secondByteArray.Length);
           return combinedArray;
       }

       internal static Byte[] CombineArrays(Byte[] firstByteArray, Int32 offset1, Int32 count1, Byte[] secondByteArray, Int32 offset2, Int32 count2)
       {
           Byte[] combinedArray = new Byte[count1 + count2];
           for (Int32 i = 0; i < count1; i++)
           {
               combinedArray[i] = firstByteArray[offset1 + i];
           }

           for (Int32 i = 0; i < count2; i++)
           {
               combinedArray[count1 + i] = secondByteArray[offset2 + i];
           }

           return combinedArray;
       }

       internal static Byte[] ExtractRangeFromArray(Byte[] source, Int32 offset, Int32 length)
       {
           Byte[] result = new Byte[length];
           Array.Copy(source, offset, result, 0, length);
           return result;
       }
    }
#endif

}