/*
 * Thermo2 Click Driver for TinyCLR 2.0
 * 
 * Version 1.0 :
 *  - Initial revision coded by Stephen Cardinale
 *    
 * Copyright � 2020 MikroBus.Net
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
    /// MikroBusNet Driver for a Thermo2 Click Temperature Sensor
    /// <para><b>This module is a Generic Device</b></para>
    /// <para><b>Pins used :</b>User selectable by solder jumper -  An or Pwm</para>
    /// </summary>
    /// <example>
    /// <code language = "C#">
    /// using MBN;
    /// using MBN.Modules;
    ///
    /// using System;
    /// using System.Diagnostics;
    ///  using System.Threading;
    /// 
    /// namespace Examples
    /// {
    ///     public class Program
    ///     {
    ///         // Here i am using two (2) Thermo2 clicks in a stacked arrangement.
    ///         // Code will work as is for a single device.
    ///         private static Thermo2Click _thermo2;
    /// 
    ///         public static void Main()
    ///         {
    ///             // We only need to instantiate one (1) as both Thremo2 clicks are sharing the same One-Wire Bus.
    ///             // The code will work with only one (1) Thermo2 Click as well.
    ///             _thermo2 = new Thermo2Click(Hardware.SC20100_2, Thermo2Click.GpioSelect.GP0)
    ///             {
    ///                 TemperatureUnit = TemperatureUnits.Celsius
    ///             };
    /// 
    ///             _thermo2.SetResolutionForAllDevices(Thermo2Click.Resolution.Resolution12Bit, false);
    /// 
    ///             foreach (Byte[] device in _thermo2.DeviceList)
    ///             {
    ///                 Debug.WriteLine($"Device ID - {GetDeviceId(device)}, Is Parasitic? - {_thermo2.IsParasitic(device)}");
    ///             }
    /// 
    ///             new Thread(TempThread).Start();
    /// 
    ///             Thread.Sleep(-1);
    /// 
    ///         }
    /// 
    ///         private static void TempThread()
    ///         {
    ///             while (true)
    ///             {
    ///                 foreach (Byte[] id in _thermo2.DeviceList)
    ///                 {
    ///                     Debug.WriteLine($"Device Address - {GetDeviceId(id)} , S/N - {_thermo2.GetSerialNumber(id)}, Temperature - {_thermo2.ReadTemperatureByAddress(id)}");
    ///                 }
    /// 
    ///                 /* For a single device, use the following code */
    ///                 //Debug.Print("Temperature - " + _thermo2.ReadTemperature());
    /// 
    ///                 Thread.Sleep(1000);
    ///             }
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
    public class Thermo2Click : ITemperature
    {
        #region .ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="Thermo2Click"/> class.
        /// </summary>
        /// <param name="socket">The <see cref="Hardware.Socket"/> that the sensor is plugged in to.</param>
        /// <param name="gpio">The pin used for the One-Wire Bus.</param>
        public Thermo2Click(Hardware.Socket socket, GpioSelect gpio = GpioSelect.GP0)
        {
#if (NANOFRAMEWORK_1_0)
            Interface = new OneWireHost();
#else
            Interface = new OneWireController(gpio == GpioSelect.GP0 ? socket.PwmPin : socket.AnPin);
#endif
            _deviceList = new ArrayList();
            _deviceList = GetDeviceList();

            if (_deviceList.Count == 0) throw new DeviceInitialisationException("No Thermo2 Clicks found on the OneWire Bus.");
        }

#endregion

#region Constants

        private const Byte TempLsb = 0;
        private const Byte TempMsb = 1;

        private const Byte HighTempAlarmBit = 2;
        private const Byte LowTempAlarmbit = 3;
        private const Byte ConfigurationBit = 4;

        private const Byte DeviceFamilyCode = 0x3B;

#endregion

#region Fields

        private readonly ArrayList _deviceList;

#endregion

#region ENUMS

        internal enum RomCommands
        {
            MatchRom = 0x55,
            SkipRom = 0xCC,
            SearchRom = 0xF0,
            AlarmSearch = 0xEC,
            ReadRom = 0x33
        }

        internal enum FunctionCommands
        {
            ReadScratchPad = 0xBE,
            WriteScratchPad = 0x4E,
            StartTemperatureConversion = 0x44,
            CopyScratchPad = 0x48,
            RecallEe = 0xB8,
            ReadPowerSupply = 0xB4
        }

        /// <summary>
        ///     Device Resolution Enumeration 9-12 Bit.
        /// </summary>
        public enum Resolution
        {
            /// <summary>
            ///     9-Bit with a resolution of 0.5�C.
            /// </summary>
            Resolution9Bit = 0,

            /// <summary>
            ///     10-Bit with a resolution of 0.25�C.
            /// </summary>
            Resolution10Bit = 1,

            /// <summary>
            ///     11-Bit with a resolution of 0.125�C.
            /// </summary>
            Resolution11Bit = 2,

            /// <summary>
            ///     12-Bit with a resolution of 0.0625�C.
            /// </summary>
            Resolution12Bit = 3
        }

        /// <summary>
        /// The GPIO Enumeration to use for the One-Wire Bus. Selectable by the GPIO Solder Pad on the Thermo2 Click.
        /// </summary>
        public enum GpioSelect
        {
            /// <summary>
            /// GPIO 0 uses the Pwm pin on the MBN Socket. To use this, the solder pad must have the Zero Ohm resistor soldered in the GPIO 0 position.
            /// </summary>
            GP0 = 0,
            /// <summary>
            /// GPIO 1 uses the An pin on the MBN Socket. To use this, the solder pad must have the Zero Ohm resistor soldered in the GPIO 1 position.
            /// </summary>
            GP1 = 1
        }

#endregion

#region Public Properties

        /// <summary>
        ///     Returns an ArrayList of the unique 64-Bit Addresses of all Thermo2 Clicks on the OneWire Bus.
        /// </summary>
        public ArrayList DeviceList
        {
            get
            {
                GetDeviceList();
                return _deviceList;
            }
        }

        /// <summary>
        ///     Exposes the native TinyCLR 1-Wire Bus.
        /// </summary>
#if (NANOFRAMEWORK_1_0)
        public OneWireHost Interface { get; }
#else
        public OneWireController Interface { get; }
#endif

        /// <summary>
        ///     Gets the number of Thermo2 Clicks on the OneWire Bus.
        /// </summary>
        /// <returns>The count of devices.</returns>
        public Int32 NumberOfDevices()
        {
            GetDeviceList();
            return _deviceList.Count;
        }

        /// <summary>
        ///     Gets the raw data of the temperature value.
        /// </summary>
        /// <value>
        ///     Raw data in the range depending on sensor's precision (8/10/12 bits, for example)
        /// </value>
        /// <remarks>Not Implemented.</remarks>
        /// <exception cref="NotImplementedException"></exception>
        public Int32 RawData => throw new NotImplementedException("RawData not implemented for this sensor");

        /// <summary>
        /// Gets or sets the temperature unit for the <seealso cref="ReadTemperature"/> method.
        /// <remarks><seealso cref="TemperatureUnits"/></remarks>
        /// </summary>
        /// <value>
        /// The temperature unit used.
        /// </value>
        /// <example>
        /// <code language="C#">
        /// // Set temperature unit to Fahrenheit
        /// _thermo.TemperatureUnit = TemperatureUnits.Farhenheit;
        /// </code>
        /// <code language="VB">
        /// ' Set temperature unit to Fahrenheit
        /// _thermo.TemperatureUnit = TemperatureUnits.Farhenheit
        /// </code>
        /// </example>
        public TemperatureUnits TemperatureUnit { get; set; } = TemperatureUnits.Celsius;

#endregion

#region Pubic Methods

#region Device Related Methods

        /// <summary>
        /// Returns an ArrayList of all Thermo2 Clicks on the OneWire Bus.
        /// </summary>
        /// <returns>An <see cref="ArrayList"/> of all Thermo2 Clicks on the OneWire Bus.</returns>
        /// <example>Example usage:
        /// <code language="C#">
        /// foreach (byte[] device in _thermo.DeviceList)
        ///  {
        ///     Debug.Print(device.GetHashCode().ToString());
        ///  }
        /// </code>
        /// </example>
        public ArrayList GetDeviceList()
        {
            _deviceList.Clear();
            var tempList = Interface.FindAllDevices();
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
        /// Determines if the Thermo2 Click is using Parasitic Power.
        /// </summary>
        /// <param name="oneWireAddress">The address of the sensor to check.</param>
        /// <returns>True is the device is using Parasitic Power, otherwise false.</returns>
        /// <example>Example usage:
        /// <code language="C#">
        /// foreach (byte[] device in _thermo.DeviceList)
        /// {
        ///     Debug.Print("Is Parasitic? -" + IsParasitic(device));
        /// }
        /// </code>
        /// </example>
        public Boolean IsParasitic(Byte[] oneWireAddress)
        {
            if (!SelectDevice(oneWireAddress)) return false;
            Interface.WriteByte((Byte) FunctionCommands.ReadPowerSupply);
            return Interface.ReadByte() == 0;
        }

        /// <summary>
        /// Determines if the DeviceId is valid.
        /// </summary>
        /// <param name="id">The 64-bit Device ID to verify.</param>
        /// <returns>True if the Device ID is valid, otherwise, false.</returns>
        /// <example>Example usage:
        /// <code language="C#">
        /// foreach (byte[] device in _thermo.DeviceList)
        /// {
        ///     Debug.Print("Is Device Valid? -" + _thermo.IsValidId(device));
        /// }
        /// </code>
        /// </example>
        public Boolean IsValidId(Byte[] id)
        {
            var enumerator = DeviceList.GetEnumerator();
            enumerator.Reset();

            while (enumerator.MoveNext())
            {
                Byte[] de = (Byte[]) enumerator.Current;
                if (de == null) throw new ArgumentNullException(nameof(de));
                return de.GetHashCode() == id.GetHashCode();
            }

            return false;
        }

        /// <summary>
        ///     Returns the laser etched 64-Bit Serial Number of the of the Thermo2 Click.
        /// </summary>
        /// <param name="oneWireAddress">The 64-bit One-Wire Address of the UniqueID click on the One-wire Bus.</param>
        /// <remarks>The Serial Number returned does not include the One-wireDevice Family code and the checksum</remarks>
        /// <returns>The Serial Number of the UniqueID Click.</returns>
        /// <example>
        /// <code language="C#">
        ///  var devices = new ArrayList();
        /// 
        ///  foreach (var device in _thermo2.DeviceList)
        ///  {
        ///      devices.Add(_thermo2.GetSerialNumber((byte[])device));
        ///  }
        ///     
        ///  foreach (var i in devices)
        ///  {
        ///      Debug.Print("Device with Serial Number " + i + " has One-Wire Address of " + GetDeviceId(_thermo2.SerialNumberToOneWireAddress((long)i)));
        ///  }
        ///  </code>
        /// </example>
        public UInt64 GetSerialNumber(Byte[] oneWireAddress)
        {
            return
                System.BitConverter.ToUInt64(
                    new[] { oneWireAddress[1], oneWireAddress[2], oneWireAddress[3], oneWireAddress[4], oneWireAddress[5], (Byte)0x00, (Byte) 0x00, (Byte) 0x00}, 0);
        }

        /// <summary>
        ///     Returns the laser etched 64-Bit One-Wire Address of the Thermo2 Click.
        /// </summary>
        /// <param name="serialNumber">The serial number of the UniqueID click on the One-wire Bus.</param>
        /// <remarks>The One-wire Address returned includes the One-wireDevice Family code and the checksum</remarks>
        /// <returns>The 64-Bit One-Wire Address of the UniqueID Click.</returns>
        /// <example>
        /// <code language="C#">
        ///  var devices = new ArrayList();
        /// 
        ///  foreach (var device in _thermo2.DeviceList)
        ///  {
        ///      devices.Add(_thermo2.GetSerialNumber((byte[])device));
        ///  }
        ///     
        ///  foreach (var i in devices)
        ///  {
        ///      Debug.Print("Device with Serial Number " + i + " has One-Wire Address of " + GetDeviceId(_thermo2.SerialNumberToOneWireAddress((long)i)));
        ///  }
        ///  </code>
        /// </example>
        public Byte[] SerialNumberToOneWireAddress(Int64 serialNumber)
        {
            var sn = Utility.CombineArrays(new[] { DeviceFamilyCode }, Utility.ExtractRangeFromArray(System.BitConverter.GetBytes(serialNumber), 0, 6));
            return Utility.CombineArrays(sn, new[] { CalculateCrc(sn, 7) });
        }

#endregion

#region Device Resolution Methods

        /// <summary>
        ///     Returns the current resolution of the device, 9-12 Bit.
        /// </summary>
        /// <param name="oneWireAddress">The address of the Thermo2 Click to check,.</param>
        /// <returns>The Resolution, see the <see cref="Resolution"/> for more information.</returns>
        /// <example>Example usage:
        /// <code language="C#">
        /// foreach (byte[] device in _thermo.DeviceList)
        /// {
        ///     Debug.Print("Current Resolution for " + GetDeviceId(device) + " - "  + _thermo.GetResolutionString(_thermo.GetResolution(device)));
        /// }
        /// </code>
        /// <code language="VB">
        /// For Each device As Byte() In _thermo.DeviceList
        ///	    Debug.Print("Current Resolution for " <![CDATA[&]]> GetDeviceId(device) <![CDATA[&]]> " - " <![CDATA[&]]> _thermo.GetResolutionString(_thermo.GetResolution(device)))
        /// Next
        /// </code>
        /// </example>
        public Resolution GetResolution(Byte[] oneWireAddress)
        {
            GetDeviceList();
#if (NANOFRAMEWORK_1_0)
            if (!IsValidId(oneWireAddress) || Interface.TouchReset() == false)
#else
            if (!IsValidId(oneWireAddress) || Interface.TouchReset() <= 0)
#endif
            {
                return Resolution.Resolution12Bit;
            }

            Interface.WriteByte((Byte) RomCommands.MatchRom);
            Interface.WriteByte((Byte) FunctionCommands.ReadScratchPad);

            while (Interface.ReadByte() == 0)
            {
            }

            Interface.TouchReset();
            Interface.WriteByte((Byte) RomCommands.MatchRom);

            var scratchPad = ReadScratchPad(oneWireAddress);
            var resolutionBits = scratchPad[ConfigurationBit];

            if (!Bits.IsBitSet(resolutionBits, 5) && !Bits.IsBitSet(resolutionBits, 6)) return Resolution.Resolution9Bit;
            if (Bits.IsBitSet(resolutionBits, 5) && !Bits.IsBitSet(resolutionBits, 6)) return Resolution.Resolution10Bit;
            if (!Bits.IsBitSet(resolutionBits, 5) && Bits.IsBitSet(resolutionBits, 6)) return Resolution.Resolution11Bit;
            return Resolution.Resolution12Bit;
        }

        /// <summary>
        ///     Returns the string representation of the device, 9-12
        /// </summary>
        /// <param name="resolution">The <see cref="Resolution"/> to return the string representation of.</param>
        /// <returns>The string representation of the Resolution, see the <see cref="Resolution"/> for more information.</returns>
        /// <example>Example usage:
        /// <code language="C#">
        /// foreach (byte[] device in DeviceList)
        /// {
        ///     Debug.Print("Current Resolution - "  + GetResolutionString(GetResolution(device)));
        /// }
        /// </code>
        /// <code language="VB">
        /// For Each device As Byte() In DeviceList
        ///     Debug.Print("Current Resolution - " <![CDATA[&]]> GetResolutionString(GetResolution(device)))
        /// Next
        /// </code>
        /// </example>
        public String GetResolutionString(Resolution resolution)
        {
            switch (resolution)
            {
                case Resolution.Resolution9Bit: return "Resolution9Bit";
                case Resolution.Resolution10Bit: return "Resolution10Bit";
                case Resolution.Resolution11Bit: return "Resolution11Bit";
                case Resolution.Resolution12Bit: return "Resolution12Bit";
            }
            return "Resolution Unknown";
        }

        /// <summary>
        ///     Sets the resolution for a specific device on the OneWire bus.
        /// </summary>
        /// <param name="oneWireAddress">The Onewire Address of the device to set the resolution.</param>
        /// <param name="resolution">The resolution to set all devices to. See <see cref="Resolution"/> for more information.</param>
        /// <param name="writeToEeprom">If true, writes the new resolution to EEPROM NVRAM</param>
        /// <example>Example usage:
        /// <code language="C#">
        /// foreach (byte[] device in _thermo.DeviceList)
        /// {
        ///     SetResolution(device, Thermo2Click.Resolution.Resolution9Bit);
        /// }
        /// </code>
        /// <code language="VB">
        /// For Each device As Byte() In _thermo.DeviceList
        ///	    SetResolution(device, Thermo2Click.Resolution.Resolution9Bit)
        /// Next
        /// </code>
        /// </example>
        /// <returns>True if successful or otherwise false.</returns>
        public Boolean SetResolution(Byte[] oneWireAddress, Resolution resolution, Boolean writeToEeprom = true)
        {
            GetDeviceList();

            if (!IsValidId(oneWireAddress)) return false;

            var scratchPad = ReadScratchPad(oneWireAddress);
            var resolutionBits = scratchPad[ConfigurationBit];

            switch (resolution)
            {
                case Resolution.Resolution9Bit:
                {
                    resolutionBits = Bits.Set(resolutionBits, 5, false);
                    resolutionBits = Bits.Set(resolutionBits, 6, false);
                    break;
                }
                case Resolution.Resolution10Bit:
                {
                    resolutionBits = Bits.Set(resolutionBits, 5, true);
                    resolutionBits = Bits.Set(resolutionBits, 6, false);
                    break;
                }
                case Resolution.Resolution11Bit:
                {
                    resolutionBits = Bits.Set(resolutionBits, 5, false);
                    resolutionBits = Bits.Set(resolutionBits, 6, true);
                    break;
                }
                case Resolution.Resolution12Bit:
                {
                    resolutionBits = Bits.Set(resolutionBits, 5, true);
                    resolutionBits = Bits.Set(resolutionBits, 6, true);
                    break;
                }
                default:
                {
                   resolutionBits = Bits.Set(resolutionBits, 5, true);
                   resolutionBits = Bits.Set(resolutionBits, 6, true);
                   break;
                }
            }
            scratchPad[ConfigurationBit] = resolutionBits;
            return WriteScratchPad(oneWireAddress, scratchPad, writeToEeprom);
        }

        /// <summary>
        ///     Sets the resolution for all devices on the OneWire bus in one call. All devices will have the same resolution.
        /// </summary>
        /// <param name="resolution">The resolution to set all devices to. See <see cref="Resolution"/> for more information.</param>
        /// <param name="writeToEeprom">If true, writes the new resolution to EEPROM NVRAM</param>
        /// <returns>True if successful, otherwise false.</returns>
        /// <example>Example usage:
        /// <code language="C#">
        /// SetResolutionForAllDevices(Thermo2Click.Resolution.Resolution9Bit);
        /// </code>
        /// <code language="VB">
        /// SetResolutionForAllDevices(Thermo2Click.Resolution.Resolution9Bit);
        /// </code>
        /// </example>
        public void SetResolutionForAllDevices(Resolution resolution, Boolean writeToEeprom = true)
        {
            foreach (Byte[] device in DeviceList)
            {
                SetResolution(device, resolution, writeToEeprom);
            }
        }

#endregion

#region Temperature Measurement Related Methods

        /// <summary>
        ///     Reads the temperature of a single Thermo2 Click configuration or the first device found on the OneWire Bus in a multiple sensor configuration.
        /// </summary>
        /// <param name="source">The temperature source <see cref="TemperatureSources"/> for more information.</param>
        /// <remarks>
        ///     Calling this method in a multi sensor Bus will only return the temperature of the first sensor found on the OneWire Bus.
        ///     To read multiple sensors, use the <see cref="ReadTemperatureForAllDevices"/> method.
        /// </remarks>
        /// <returns>
        ///     A float representing the temperature read from the source in the temperature scale set in the <see cref="TemperatureUnit"/> property.
        /// </returns>
        /// <exception cref="NotSupportedException">Throws a NotSupportedException if the sensor being read is powered in Parasitic Mode.</exception>
        /// <example>Example usage:
        /// <code language="C#">
        ///     Debug.Print("Temperature - " + ReadTemperature());
        /// </code>
        /// <code language="VB">
        ///     Debug.Print("Temperature - " <![CDATA[&]]> ReadTemperature())
        /// </code>
        /// </example>
        public Single ReadTemperature(TemperatureSources source = TemperatureSources.Ambient)
        {
            if (source == TemperatureSources.Object) throw new ArgumentException("TemperatureSources.Object not implemented for this module.");

            if (IsParasitic((Byte[]) DeviceList[0])) throw new NotSupportedException("Reading temperature in Parasitic Mode is not supported.");

#if (NANOFRAMEWORK_1_0)
            if (Interface.TouchReset() == false) return Single.MinValue;
#else
            if (Interface.TouchReset() <= 0) return Single.MinValue;
#endif

            Interface.WriteByte((Byte) RomCommands.SkipRom); // Skip ROM, we only have one device
            Interface.WriteByte((Byte) FunctionCommands.StartTemperatureConversion); // Start temperature conversion

            while (Interface.ReadByte() == 0)
            {
            }

            Interface.TouchReset();

            Interface.WriteByte((Byte) RomCommands.SkipRom);

            var device = (Byte[]) DeviceList[0];

            SelectDevice(device);

            var scratchPad = ReadScratchPad(device);

            var temp = (scratchPad[TempMsb] << 8) | scratchPad[TempLsb];

            if ((scratchPad[1] & 0x80) != 0)
            {
                temp = (Int16)(scratchPad[0] | (scratchPad[1] << 8));
            }

            Double temperature;

            switch (GetResolution((Byte[])DeviceList[0]))
            {
                case Resolution.Resolution12Bit:
                {
                    temperature = temp * 0.0625;
                    break;
                }
                case Resolution.Resolution11Bit:
                {
                    temperature = (temp >> 1) * 0.1250;
                    break;
                }
                case Resolution.Resolution10Bit:
                {
                    temperature = (temp >> 2) * 0.2500;
                    break;
                }
                case Resolution.Resolution9Bit:
                {
                    temperature = (temp >> 3) * 0.500;
                break;
                }
                default:
                {
                    temperature = (temp >> 3) * 0.5000;
                    break;
                }
            }

            return (Single)ConvertToScale(temperature);
        }

        /// <summary>
        ///     Reads the temperature of a Thermo2 Click by its unique 64-bit Address.
        /// </summary>
        /// <param name="oneWireAddress">The 64-bit address of the sensor to read.</param>
        /// <returns>The temperature of the sensor being passed to the method.</returns>
        /// <exception cref="NotSupportedException">Throws a NotSupportedException if the sensor being read is powered in Parasitic Mode.</exception>
        /// <example>Example usage:
        /// <code language="C#">
        /// foreach (byte[] id in _thermo.DeviceList)
        /// {
        ///     Debug.Print("By ID - Device Address - " + GetDeviceId(id) + ", Temperature - " + _thermo.ReadTemperatureByAddress(id));
        /// }
        /// </code>
        /// <code language="VB">
        /// For Each id As Byte() In _thermo.DeviceList
        ///	    Debug.Print("By ID - Device Address - " <![CDATA[&]]> GetDeviceId(id) <![CDATA[&]]> ", Temperature - " <![CDATA[&]]> _thermo.ReadTemperatureByAddress(id))
        /// Next
        /// </code>
        /// </example>
        public Double ReadTemperatureByAddress(Byte[] oneWireAddress)
        {
            GetDeviceList();

            if (IsParasitic(oneWireAddress)) throw new NotSupportedException("Reading temperature in Parasitic Mode is not supported.");

            if (!IsValidId(oneWireAddress)) return Single.MinValue;

#if (NANOFRAMEWORK_1_0)
            if (Interface.TouchReset() == false) return Single.MinValue;
#else
            if (Interface.TouchReset() <= 0) return Single.MinValue;
#endif

            SelectDevice(oneWireAddress);

            Interface.WriteByte((Byte) FunctionCommands.StartTemperatureConversion);

            while (Interface.ReadByte() == 0)
            {
            }

            Interface.TouchReset();

            SelectDevice(oneWireAddress);

            var scratchPad = ReadScratchPad(oneWireAddress);

            var temp = (scratchPad[TempMsb] << 8) | scratchPad[TempLsb];

            if (Bits.IsBitSet(scratchPad[1], 7))
            {
                temp = -(Int16)((scratchPad[1] << 8) | scratchPad[0]);
            }

            Single rawTemperature;

            var resolution = GetResolution(oneWireAddress);

            switch (resolution)
            {
                case Resolution.Resolution12Bit:
                    {
                        rawTemperature = temp * 0.0625F;
                        break;
                    }
                case Resolution.Resolution11Bit:
                    {
                        rawTemperature = (temp >> 1) * 0.1250F;
                        break;
                    }
                case Resolution.Resolution10Bit:
                    {
                        rawTemperature = (temp >> 2) * 0.2500F;
                        break;
                    }
                case Resolution.Resolution9Bit:
                    {
                        rawTemperature = (temp >> 3) * 0.5000F;
                        break;
                    }
                default:
                    {
                        rawTemperature = (temp >> 3) * 0.5000F;
                        break;
                    }
            }

            return ConvertToScale(rawTemperature);
        }

        /// <summary>
        ///     Reads all Thermo2 Click on the OneWire Bus.
        /// </summary>
        /// <returns>A HashTable containing a Key/Value Pair of the unique 64-Bit Address of the Thermo2 Click and the temperature in �C.</returns>
        /// <remarks>This method works in either a single sensor or multi sensor configuration.</remarks>
        /// <example>Example usage:
        /// <code language="C#">
        /// Hashtable temperature = _thermo2.ReadTemperatureForAllDevices();
        /// foreach (DictionaryEntry t in temperature)
        /// {
        ///     Debug.Print("Key - " + GetDeviceId((byte[]) t.Key) + " Value - " + t.Value);
        /// }
        /// </code>
        /// <code language="VB">
        /// Dim temperature As Hashtable = _thermo2.ReadTemperatureForAllDevices()
        /// For Each t As DictionaryEntry In temperature
        ///	    Debug.Print(("Key - " <![CDATA[&]]> GetDeviceId(DirectCast(t.Key, Byte())) <![CDATA[&]]> " Value - ") + t.Value)
        /// Next
        /// </code>
        /// </example>
        public Hashtable ReadTemperatureForAllDevices()
        {
            GetDeviceList();
            var temperature = new Hashtable();
            lock (_deviceList)
            {
                lock (temperature.SyncRoot)
                {
                    foreach (Byte[] device in DeviceList)
                    {
                        temperature.Add(device, NumberOfDevices() >= 1 ? ReadTemperatureByAddress(device) : ReadTemperature());
                    }
                    return temperature;
                }
            }
        }

#endregion

#region Alarm Related Methods

        /// <summary>
        ///     Gets an ArrayList of all Thermo2 Clicks on the OneWire Bus that have the AlarmFlag set.
        /// </summary>
        /// <returns>An <see cref="ArrayList"/> of all devices in alarm.</returns>
        /// <example>Example usage:
        /// <code language="C#">
        /// foreach (byte[] alarmingDevice in _thermo.AlarmList())
        /// {
        ///     Debug.Print("Alarming Device Address - " + alarmingDevice);
        /// }
        /// </code>
        /// <code language="VB">
        /// For Each alarmingDevice As Byte() In _thermo.AlarmList()
        ///	    Debug.Print("Alarming Device Address - " <![CDATA[&]]> Convert.ToString(alarmingDevice))
        /// Next
        /// </code>
        /// </example>
        public ArrayList AlarmList()
        {
            var alarmList = new ArrayList();

            // find the first device (only devices alarming)
#if (NANOFRAMEWORK_1_0)
            bool result = Interface.FindFirstDevice(true, true);
            while (result != true)
            {
                var sNum = new Byte[8];

                // retrieve the serial number just found
                sNum = Interface.SerialNumber;

                // save serial number
                alarmList.Add(sNum);

                // find the next alarming device
                result = Interface.FindNextDevice(false, true);
            }
#else
            Int32 result = Interface.FindFirstDevice(true, true);
            while (result != 0)
            {
                var sNum = new Byte[8];

                // retrieve the serial number just found
                Interface.SerialNum(sNum, true);

                // save serial number
                alarmList.Add(sNum);

                // find the next alarming device
                result = Interface.FindNextDevice(false, true);
            }
#endif
            return alarmList;
        }

        /// <summary>
        ///     Checks whether a specific device has the Alarm flag set.
        /// </summary>
        /// <param name="oneWireAddress">The Onewire Address to check.</param>
        /// <returns>True is the specified device is in alarm.</returns>
        /// <remarks>
        /// The device Alarm Flag will not be cleared until a subsequent temperature conversion is done.
        /// If the device has an alarm, best practice is to read the temperature again, if the Alarm Flag is still present, it is an actual alarm.
        /// If the device temperature changes and the device no longer is in alarm, the alarm flag will be cleared by doing a subsequent temperature reading.
        /// </remarks>
        /// <example>Example usage:
        /// <code language="C#">
        /// foreach (byte[] alarmingDevice in AlarmList())
        /// {
        ///     Debug.Print("Devices in alarm:  " + HasAlarm(alarmingDevice) + " - " + GetDeviceId(alarmingDevice));
        /// }
        /// </code>
        /// <code language="VB">
        /// For Each alarmingDevice As Byte() In AlarmList()
        ///     Debug.Print("Devices in alarm:  " <![CDATA[&]]> HasAlarm(alarmingDevice) <![CDATA[&]]> " - " <![CDATA[&]]> GetDeviceId(alarmingDevice))
        /// Next
        /// </code>
        /// </example>
        public Boolean HasAlarm(Byte[] oneWireAddress)
        {
            GetDeviceList();

            var alarmArray = AlarmList();

            foreach (Byte[] device in alarmArray)
            {
                if (System.BitConverter.ToInt64(oneWireAddress, 0) == System.BitConverter.ToInt64(device, 0)) return true;
            }

            return false;
        }

        /// <summary>
        ///     Resets the Low and High Temperature Alarms Settings for the specified OneWire Device to -55�C and 125� (the range of the Thermo2 Click).
        /// </summary>
        /// <param name="oneWireAddress">The OneWire device address.</param>
        /// <param name="writeToEeprom">If true, writes the new settings to the device EEPROM.</param>
        /// <returns>True if successful or otherwise false.</returns>
        /// <remarks>All temperature alarms settings are in the <see cref="TemperatureUnits.Celsius"/> unit only.</remarks>
        /// <example>Example usage:
        /// <code language="C#">
        /// foreach (byte[] device in _thermo.DeviceList)
        /// {
        ///     _thermo.ResetAlarmSettings(device, true);
        /// }
        /// </code>
        /// <code language="VB">
        /// For Each device As Byte() In _thermo.DeviceList
        ///	    _thermo.ResetAlarmSettings(device, True)
        /// Next
        /// </code>
        /// </example>
        public Boolean ResetAlarmSettings(Byte[] oneWireAddress, Boolean writeToEeprom = true)
        {
            GetDeviceList();

            if (!IsValidId(oneWireAddress)) return false;

            SelectDevice(oneWireAddress);

            var scratchPad = ReadScratchPadSigned(oneWireAddress);
            scratchPad[HighTempAlarmBit] = 125;
            scratchPad[LowTempAlarmbit] = -55;

            return WriteScratchPad(oneWireAddress, scratchPad, writeToEeprom);
        }

        /// <summary>
        ///     Returns the Low Temperature Alarm setting of the specified device in �C.
        /// </summary>
        /// <param name="oneWireAddress">The OneWire Address of the specified device.</param>
        /// <returns>The Low Temperature Alarm setting in �C</returns>
        /// <remarks>All temperature alarms settings are in the <see cref="TemperatureUnits.Celsius"/> unit only.</remarks>
        /// <example>Example usage:
        /// <code language="C#">
        /// foreach (byte[] device in DeviceList)
        /// {
        ///     Debug.Print("Low Temp Alarm for Device - "  + ReadLowTempAlarmSetting(device));
        /// }
        /// </code>
        /// <code language="VB">
        /// For Each device As Byte() In DeviceList
        ///	    Debug.Print("Low Temp Alarm for Device - " <![CDATA[&]]> ReadLowTempAlarmSetting(device))
        /// Next
        /// </code>
        /// </example>
        public Int32 ReadLowTempAlarmSetting(Byte[] oneWireAddress)
        {
            GetDeviceList();

#if (NANOFRAMEWORK_1_0)
            if (!IsValidId(oneWireAddress) || Interface.TouchReset() == false) return Int32.MinValue;
#else
            if (!IsValidId(oneWireAddress) || Interface.TouchReset() <= 0) return Int32.MinValue;
#endif

            SelectDevice(oneWireAddress);

            var scratchPad = ReadScratchPadSigned(oneWireAddress);

            return scratchPad[LowTempAlarmbit];
        }

        /// <summary>
        ///     Returns the High Temperature Alarm setting of the specified device in �C.
        /// </summary>
        /// <param name="oneWireAddress">The OneWire Address of the specified device.</param>
        /// <returns>The High Temperature Alarm setting in �C</returns>
        /// <remarks>All temperature alarms settings are in the <see cref="TemperatureUnits.Celsius"/> unit only.</remarks>
        /// <example>Example usage:
        /// <code language="C#">
        /// foreach (byte[] device in DeviceList)
        /// {
        ///     Debug.Print("High Temp Alarm for Device - " + ReadHighTempAlarmSetting(device));
        /// }
        /// </code>
        /// <code language="VB">
        /// For Each device As Byte() In DeviceList
        ///     Debug.Print("High Temp Alarm for Device - " <![CDATA[&]]> ReadHighTempAlarmSetting(device))
        /// Next
        /// </code>
        /// </example>
        public Int32 ReadHighTempAlarmSetting(Byte[] oneWireAddress)
        {
            GetDeviceList();

#if (NANOFRAMEWORK_1_0)
            if (!IsValidId(oneWireAddress) || Interface.TouchReset() == false) return Int32.MinValue;
#else
            if (!IsValidId(oneWireAddress) || Interface.TouchReset() <= 0) return Int32.MinValue;
#endif

            SelectDevice(oneWireAddress);

            var scratchPad = ReadScratchPadSigned(oneWireAddress);

            return scratchPad[HighTempAlarmBit];
        }

        /// <summary>
        /// Sets the low alarm temperature for a specific device in �C.
        /// </summary>
        /// <param name="oneWireAddress">The OneWire Address of the specified device.</param>
        /// <param name="lowTemperatureAlarm">Low Temperature Alarm set point. The valid range is -55�C - 125�C </param>
        /// <param name="writeToEeprom">If true, writes the new Low Temperature Alarm setting to NVRAM EEPROM.</param>
        /// <returns>True if successful or otherwise false.</returns>
        /// <exception cref="ArgumentException">Throws an ArgumentException if the <para>lowTemperatureAlarm</para> setting is higher than or equal to the <see cref="SetHighTempertureAlarm"/> method.</exception>
        /// <remarks>All temperature alarms settings are in the <see cref="TemperatureUnits.Celsius"/> unit only.</remarks>
        /// <example>Example usage:
        /// <code language="C#">
        ///  foreach (byte[] device in _thermo.DeviceList)
        /// {
        ///     SetLowTempertureAlarm(device, 20, true);
        /// }
        /// </code>
        /// <code language="VB">
        /// For Each device As Byte() In _thermo.DeviceList
        ///	    SetLowTempertureAlarm(device, 20, True)
        /// Next
        /// </code>
        /// </example>
        public Boolean SetLowTempertureAlarm(Byte[] oneWireAddress, SByte lowTemperatureAlarm, Boolean writeToEeprom = true)
        {
            GetDeviceList();

            if (!IsValidId(oneWireAddress)) return false;

            // Make sure the alarm setting is within the device's range (-55�C to 125�C)
            lowTemperatureAlarm = (SByte) (lowTemperatureAlarm < -55 ? -55 : lowTemperatureAlarm > 125 ? 125 : lowTemperatureAlarm);

            // Verify the Low Temperature alarm setting is not higher than the High Temperature Alarm setting in EEPROM, if so reset the values to default.
            if (lowTemperatureAlarm >= ReadHighTempAlarmSetting(oneWireAddress))
                throw new ArgumentException("Low Temperature Alarm setting must not be higher than the High Temperature Alarm setting.", nameof(lowTemperatureAlarm));

            var scratchPad = ReadScratchPadSigned(oneWireAddress);
            scratchPad[LowTempAlarmbit] = lowTemperatureAlarm;

            return WriteScratchPad(oneWireAddress, scratchPad, writeToEeprom);
        }

        /// <summary>
        /// Sets the low alarm temperature for a specific device in �C.
        /// </summary>
        /// <param name="oneWireAddress">The OneWire Address of the specified device.</param>
        /// <param name="highTemperatureAlarm">High Temperature Alarm set point. The valid range is -55�C - 125�C </param>
        /// <param name="writeToEeprom">If true, writes the new Low Temperature Alarm setting to NVRAM EEPROM.</param>
        /// <returns>True if successful or otherwise false.</returns>
        /// <exception cref="ArgumentException">Throws an ArgumentException if the <para>lowTemperatureAlarm</para> setting is higher than or equal to the <see cref="SetHighTempertureAlarm"/> method.</exception>
        /// <remarks>All temperature alarms settings are in the <see cref="TemperatureUnits.Celsius"/> unit only.</remarks>
        /// <example>Example usage:
        /// <code language="C#">
        ///  foreach (byte[] device in _thermo.DeviceList)
        /// {
        ///     SetHighTempertureAlarm(device, 60, true);
        /// }
        /// </code>
        /// <code language="VB">
        /// For Each device As Byte() In _thermo.DeviceList
        ///	    SetHighTempertureAlarm(device, 60, True)
        /// Next
        /// </code>
        /// </example>
        public Boolean SetHighTempertureAlarm(Byte[] oneWireAddress, SByte highTemperatureAlarm, Boolean writeToEeprom = true)
        {
            GetDeviceList();

            if (!IsValidId(oneWireAddress)) throw new ArgumentException("Invalid OneWired Address", nameof(oneWireAddress));

            // Make sure the alarm setting is within the device's range (-55�C to 125�C)
            highTemperatureAlarm = (SByte) (highTemperatureAlarm < -55 ? -55 : highTemperatureAlarm > 125 ? 125 : highTemperatureAlarm);

            // Verify the Low Temperature alarm setting is not higher than the High Temperature Alarm setting in EEPROM, if so reset the values to default.
            if (highTemperatureAlarm <= ReadLowTempAlarmSetting(oneWireAddress))
                throw new ArgumentException("High Temperature Alarm setting must not be lower than or equal to the Low Temperature Alarm setting.", nameof(highTemperatureAlarm));

            var scratchPad = ReadScratchPadSigned(oneWireAddress);
            scratchPad[HighTempAlarmBit] = highTemperatureAlarm;

            return WriteScratchPad(oneWireAddress, scratchPad, writeToEeprom);
        }

        /// <summary>
        /// Sets both the low  and hight temperature alarm at once for a specific device in �C.
        /// </summary>
        /// <param name="oneWireAddress">The OneWire Address of the specified device.</param>
        /// <param name="lowTemperatureAlarm">Low Temperature Alarm set point. The valid range is -55�C - 125�C </param>
        /// <param name="highTemperatureAlarm">High Temperature Alarm set point. The valid range is -55�C - 125�C </param>
        /// <param name="writeToEeprom">If true, writes the new Low Temperature Alarm setting to NVRAM EEPROM.</param>
        /// <returns>True if successful or otherwise false.</returns>
        /// <exception cref="ArgumentException">Throws an ArgumentException if the <para>lowTemperatureAlarm</para> setting is higher than or equal to the <see cref="SetHighTempertureAlarm"/> method or if the <para>highTemperatureAlarm</para> setting is lower than or equal to the <see cref="SetLowTempertureAlarm"/> method.</exception>
        /// <remarks>All temperature alarms settings are in the <see cref="TemperatureUnits.Celsius"/> unit only.</remarks>
        /// <example>Example usage:
        /// <code language="C#">
        /// foreach (byte[] device in _thermo.DeviceList)
        /// {
        ///     SetBothTemperatureAlarms(device,-55, 125);
        /// }
        /// </code>
        /// <code language="VB">
        ///  For Each device as Byte() In _thermo.DeviceList)
        ///     SetBothTemperatureAlarms(device,-55, 125)
        /// Next
        /// </code>
        /// </example>
        public Boolean SetBothTemperatureAlarms(Byte[] oneWireAddress, SByte lowTemperatureAlarm, SByte highTemperatureAlarm, Boolean writeToEeprom = true)
        {
            GetDeviceList();

            if (!IsValidId(oneWireAddress)) return false;

            // Make sure the alarm setting is within the device's range (-55�C to 125�C)
            highTemperatureAlarm = (SByte) (highTemperatureAlarm < -55 ? -55 : highTemperatureAlarm > 125 ? 125 : highTemperatureAlarm);
            lowTemperatureAlarm = (SByte) (lowTemperatureAlarm < -55 ? -55 : lowTemperatureAlarm > 125 ? 125 : lowTemperatureAlarm);

            // Verify the Low Temperature alarm setting is not higher than the High Temperature Alarm setting in EEPROM, if so reset the values to default.
            if (highTemperatureAlarm <= lowTemperatureAlarm)
            {
                throw new ArgumentException("High Temperature Alarm setting must not be lower than or equal to the Low Temperature Alarm setting.", nameof(highTemperatureAlarm));
            }
            if (lowTemperatureAlarm >= highTemperatureAlarm)
            {
                throw new ArgumentException("Low Temperature Alarm setting must not be higher than or equal to the High Temperature Alarm setting.", nameof(lowTemperatureAlarm));
            }

            var scratchPad = ReadScratchPadSigned(oneWireAddress);
            scratchPad[HighTempAlarmBit] = highTemperatureAlarm;
            scratchPad[LowTempAlarmbit] = lowTemperatureAlarm;

            return WriteScratchPad(oneWireAddress, scratchPad, writeToEeprom);
        }

        /// <summary>
        /// Sets both the low  and hight temperature alarm at once for all devices on the One-Wire Bus device in �C.
        /// </summary>
        /// <param name="lowTemperatureAlarm">Low Temperature Alarm set point. The valid range is -55�C - 125�C </param>
        /// <param name="highTemperatureAlarm">High Temperature Alarm set point. The valid range is -55�C - 125�C </param>
        /// <param name="writeToEeprom">If true, writes the new Low Temperature Alarm setting to NVRAM EEPROM.</param>
        /// <returns>True if successful or otherwise false.</returns>
        /// <exception cref="ArgumentException">Throws an ArgumentException if the <para>lowTemperatureAlarm</para> setting is higher than or equal to the <see cref="SetHighTempertureAlarm"/> method or if the <para>highTemperatureAlarm</para> setting is lower than or equal to the <see cref="SetLowTempertureAlarm"/> method.</exception>
        /// <remarks>All temperature alarms settings are in the <see cref="TemperatureUnits.Celsius"/> unit only.</remarks>
        /// <example>
        /// <code language="C#">
        ///     SetAlarmsForAlllDevic(20, 60, true);
        /// </code>
        /// <code language="VB">
        ///	    SetHighTempertureAlarm(20, 60, True)
        /// </code>
        /// </example>
        public void SetAlarmsForAlllDevices(SByte lowTemperatureAlarm, SByte highTemperatureAlarm, Boolean writeToEeprom = true)
        {
            foreach (Byte[] device in DeviceList)
            {
                SetBothTemperatureAlarms(device, lowTemperatureAlarm, highTemperatureAlarm, writeToEeprom);
            }
        }

#endregion

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
                    var mix = (Byte)((crc ^ inputByte) & 0x01);
                    crc >>= 1;
                    if (mix != 0) crc ^= 0x8C;

                    inputByte >>= 1;
                }
            }
            return crc;
        }

        private Byte[] ReadScratchPad(Byte[] oneWireAddress)
        {
            GetDeviceList();

            var scratchPad = new Byte[9];

            if (!IsValidId(oneWireAddress)) return scratchPad;

            SelectDevice(oneWireAddress);

            Interface.WriteByte((Byte) FunctionCommands.ReadScratchPad);

            for (UInt16 i = 0; i < 9; i++)
            {
                scratchPad[i] = (Byte) Interface.ReadByte();
            }
            return scratchPad;
        }

        private SByte[] ReadScratchPadSigned(Byte[] oneWireAddress)
        {
            GetDeviceList();

            var scratchPad = new SByte[9];

            if (!IsValidId(oneWireAddress)) return scratchPad;

            SelectDevice(oneWireAddress);

            Interface.WriteByte((Byte) FunctionCommands.ReadScratchPad);

            for (UInt16 i = 0; i < 9; i++)
            {
                scratchPad[i] = (SByte) Interface.ReadByte();
            }
            return scratchPad;
        }

        private Boolean SelectDevice(Byte[] oneWireAddress)
        {
#if (NANOFRAMEWORK_1_0)
            if (Interface.TouchReset() == false) return false;
#else
            if (Interface.TouchReset() <= 0) return false;
#endif

            Interface.WriteByte((Byte) RomCommands.MatchRom);

            for (Int32 i = 0; i <= 7; i++)
            {
                Interface.WriteByte(oneWireAddress[i]);
            }
            return true;
        }

        private Boolean WriteScratchPad(Byte[] oneWireAddress, Byte[] scratchPad, Boolean writeToEeprom = true)
        {
            GetDeviceList();

            if (!IsValidId(oneWireAddress)) return false;

            SelectDevice(oneWireAddress);

            Interface.WriteByte((Byte) FunctionCommands.WriteScratchPad);
            Interface.WriteByte(scratchPad[HighTempAlarmBit]); // high alarm temp
            Interface.WriteByte(scratchPad[LowTempAlarmbit]); // low alarm temp
            Interface.WriteByte(scratchPad[ConfigurationBit]); // configuration


            SelectDevice(oneWireAddress);

            if (writeToEeprom)
            {
                SelectDevice(oneWireAddress);
                // save the newly written values to eeprom
                Interface.WriteByte((Byte) FunctionCommands.CopyScratchPad);
                while (Interface.ReadByte() == 0)
                {
                }
            }
            // Verify write to EEPROM
            Interface.TouchReset();
            Interface.WriteByte((Byte) FunctionCommands.RecallEe);
            var scratchPad2 = ReadScratchPad(oneWireAddress);

            return scratchPad.GetHashCode() == scratchPad2.GetHashCode();
        }

        private Boolean WriteScratchPad(Byte[] oneWireAddress, SByte[] scratchPad, Boolean writeToEeprom = true)
        {
            GetDeviceList();

            if (!IsValidId(oneWireAddress)) return false;

            SelectDevice(oneWireAddress);

            Interface.WriteByte((Byte) FunctionCommands.WriteScratchPad);
#if (NANOFRAMEWORK_1_0)
            Interface.WriteByte((byte)scratchPad[HighTempAlarmBit]); // high alarm temp
            Interface.WriteByte((byte)scratchPad[LowTempAlarmbit]); // low alarm temp
            Interface.WriteByte((byte)scratchPad[ConfigurationBit]); // configuration
#else
            Interface.WriteByte(scratchPad[HighTempAlarmBit]); // high alarm temp
            Interface.WriteByte(scratchPad[LowTempAlarmbit]); // low alarm temp
            Interface.WriteByte(scratchPad[ConfigurationBit]); // configuration
#endif


            SelectDevice(oneWireAddress);

            if (writeToEeprom)
            {
                SelectDevice(oneWireAddress);
                // save the newly written values to EEPROM
                Interface.WriteByte((Byte) FunctionCommands.CopyScratchPad);
                while (Interface.ReadByte() == 0)
                {
                }
            }
            // Verify write to EEPROM
            Interface.TouchReset();
            Interface.WriteByte((Byte) FunctionCommands.RecallEe); // ToDo - Necessary?
            var scratchPad2 = ReadScratchPadSigned(oneWireAddress);

            return scratchPad.GetHashCode() == scratchPad2.GetHashCode();
        }

        private Double ConvertToScale(Double temperature)
        {
            switch (TemperatureUnit)
            {
                case TemperatureUnits.Celsius:
                {
                    return temperature;
                }
                case TemperatureUnits.Fahrenheit:
                {
                    return temperature*9/5 + 32;
                }
                case TemperatureUnits.Kelvin:
                {
                    return temperature + 273.15F;
                }
                default:
                {
                    return Double.NaN;
                }
            }
        }

#endregion
    }

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
}