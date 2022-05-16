/*
 * DS18B20 One Wire Temperature Sensor for TinyCLR 20
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
using GHIElectronics.TinyCLR.Devices.Onewire;
#endif

using System;
using System.Collections;

namespace MBN.Modules
{
    /// <summary>
    /// MikroBusNet Driver for a DS18B20 One-Wire Temperature Sensor
    /// <para><b>Pins used :</b> User selectable.</para>
    /// </summary>
    /// <example>
    /// <code language = "C#">
    ///using System;
    ///using MBN;
    ///using MBN.Modules;
    ///
    ///using System.Collections;
    ///using System.Diagnostics;
    ///using System.Threading;
    ///
    ///namespace Examples
    ///{
    ///    internal class Program
    ///    {
    ///        private static DS18B20 _ds18B20;
    ///
    ///         private static readonly Byte[] Device0 = new Byte[] { 40, 27, 213, 247, 4, 0, 0, 88 };
    ///         private static readonly Byte[] Device1 = new Byte[] { 40, 209, 177, 246, 4, 0, 0, 137 };
    ///         private static readonly Byte[] Device2 = new Byte[] { 40, 206, 219, 247, 4, 0, 0, 213 };
    ///
    ///             public static void Main()
    ///             {
    ///                 _ds18B20 = new DS18B20(Hardware.SC20100_2.AnPin);
    ///
    ///            // Run this first if you want to obtain a list of current DS18B20 Sensors on the OneWire Bus.
    ///            // To get your 64-Bit DeviceIds Uncomment the line DisplayDeviceIds()
    ///
    ///            // I am using three sensors for this Demo1, you will need to change the values of Device0, Device1 and Device2 to reflect your
    ///            // sensor id for the Demo1 Code to work. Or just Comment out "RunDemo1(); and Uncomment RunDemo2();
    ///
    ///            //DisplayDeviceIds();
    ///
    ///            //RunDemo1();
    ///
    ///            RunDemo2();
    ///
    ///            new Thread(Capture).Start();
    ///
    ///            Thread.Sleep(Timeout.Infinite);
    ///             }
    ///
    ///             private static void DisplayDeviceIds()
    ///             {
    ///                 Debug.WriteLine("Displaying DeviceIds");
    ///                 foreach (Byte[] device in _ds18B20.DeviceList)
    ///                 {
    ///                     Debug.WriteLine(GetDeviceId(device));
    ///                 }
    ///             }
    ///
    ///             private static void RunDemo1()
    ///             {
    ///                 Debug.WriteLine("*********DS18B20 Demo**********\n");
    ///                 Debug.WriteLine("Number of Devices: " + _ds18B20.NumberOfDevices() + "\n");
    ///
    ///                 Debug.WriteLine("Testing Setting Resolutions");
    ///                 _ds18B20.SetResolution(Device0, DS18B20.Resolution.Resolution9Bit);
    ///                 _ds18B20.SetResolution(Device1, DS18B20.Resolution.Resolution11Bit);
    ///                 _ds18B20.SetResolution(Device2, DS18B20.Resolution.Resolution12Bit);
    ///
    ///                 /* Alternate Method 1
    ///                 foreach (byte[] device in _ds18B20.DeviceList)
    ///                 {
    ///                     _ds18B20.SetResolution(device, DS18B20.Resolution.Resolution9Bit);
    ///                 }
    ///                 */
    ///
    ///                 /* Alternate Method 2
    ///                 _ds18B20.SetResolutionForAllDevices(DS18B20.Resolution.Resolution9Bit);
    ///                 */
    ///
    ///                 Debug.WriteLine("\nSetting Resolutions done, now reading back resolutions.");
    ///                 Debug.WriteLine("Current Resolution for Device 0: " + _ds18B20.GetResolutionString(_ds18B20.GetResolution(Device0)));
    ///                 Debug.WriteLine("Current Resolution for Device 1: " + _ds18B20.GetResolutionString(_ds18B20.GetResolution(Device1)));
    ///                 Debug.WriteLine("Current Resolution for Device 2: " + _ds18B20.GetResolutionString(_ds18B20.GetResolution(Device2)) + "\n");
    ///
    ///                 /* Alternate method
    ///                 foreach (byte[] device in _ds18B20.DeviceList)
    ///                 {
    ///                     Debug.WriteLine("Current Resolution for " + GetDeviceId(device) + " - "  + _ds18B20.GetResolutionString(_ds18B20.GetResolution(device)));
    ///                 }
    ///                 */
    ///
    ///                 Debug.WriteLine("Test - Checking for Parasitic Devices");
    ///                 Debug.WriteLine("Is Parasitic? -" + _ds18B20.IsParasitic(Device0));
    ///                 Debug.WriteLine("Is Parasitic? -" + _ds18B20.IsParasitic(Device1));
    ///                 Debug.WriteLine("Is Parasitic? -" + _ds18B20.IsParasitic(Device2) + "\n");
    ///
    ///                 /* Alternate method
    ///                 foreach (byte[] device in _ds18B20.DeviceList)
    ///                 {
    ///                     Debug.WriteLine("Is Parasitic? -" + _ds18B20.IsParasitic(device));
    ///                 }
    ///                 */
    ///
    ///                 Debug.WriteLine("Test - Resetting Alarms");
    ///                 Debug.WriteLine("Resetting Alarms 0 yields " + _ds18B20.ResetAlarmSettings(Device0));
    ///                 Debug.WriteLine("Resetting Alarms 1 yields " + _ds18B20.ResetAlarmSettings(Device1));
    ///                 Debug.WriteLine("Resetting Alarms 2 yields " + _ds18B20.ResetAlarmSettings(Device2) + "\n");
    ///
    ///                 /* ResetAlarmSettings - Alternate Method
    ///                 foreach (byte[] device in _ds18B20.DeviceList)
    ///                 {
    ///                     _ds18B20.ResetAlarmSettings(device);
    ///                 }
    ///                 */
    ///
    ///                 Debug.WriteLine("Test - Setting individual Alarms");
    ///                 _ds18B20.SetLowTempertureAlarm(Device0, -55);
    ///                 _ds18B20.SetHighTempertureAlarm(Device0, 100);
    ///                 _ds18B20.SetLowTempertureAlarm(Device1, 124);
    ///                 _ds18B20.SetHighTempertureAlarm(Device1, 125);
    ///                 _ds18B20.SetLowTempertureAlarm(Device2, -55);
    ///                 _ds18B20.SetHighTempertureAlarm(Device2, 125);
    ///
    ///                 /* Setting Alarms - alternate method 1
    ///                 _ds18B20.SetBothTemperatureAlarms(Device0, -55, 125);
    ///                 _ds18B20.SetBothTemperatureAlarms(Device0, 30, 125);
    ///                 _ds18B20.SetBothTemperatureAlarms(Device2, -55, 125);
    ///                 */
    ///
    ///                 /* Setting Alarms - Alternate method 2
    ///                 _ds18B20.SetAlarmsForAlllDevices(-55, 125);
    ///                 */
    ///
    ///                 /* Setting alarms - Alternate method 3
    ///                 foreach (byte[] device in _ds18B20.DeviceList)
    ///                 {
    ///                    _ds18B20.SetBothTemperatureAlarms(device, -55, 125);
    ///                 }
    ///                 */
    ///
    ///                 Debug.WriteLine("\nSetting Alarms done, now reading back alarm settings.");
    ///                 Debug.WriteLine("Low Temp Alarm for Device0 - " + _ds18B20.ReadLowTempAlarmSetting(Device0) + "°C");
    ///                 Debug.WriteLine("High Temp Alarm for Device0 - " + _ds18B20.ReadHighTempAlarmSetting(Device0) + "°C");
    ///                 Debug.WriteLine("Low Temp Alarm for Device1 - " + _ds18B20.ReadLowTempAlarmSetting(Device1) + "°C");
    ///                 Debug.WriteLine("High Temp Alarm for Device1 - " + _ds18B20.ReadHighTempAlarmSetting(Device1) + "°C");
    ///                 Debug.WriteLine("Low Temp Alarm for Device2 - " + _ds18B20.ReadLowTempAlarmSetting(Device2) + "°C");
    ///                 Debug.WriteLine("High Temp Alarm for Device2 - " + _ds18B20.ReadHighTempAlarmSetting(Device2) + "°C\n");
    ///
    ///                 /* Read Alarm Settings - Alternate method
    ///                 foreach (byte[] device in _ds18B20.DeviceList)
    ///                 {
    ///                     Debug.WriteLine("Low Temp Alarm for Device - " + GetDeviceId(device) + " - " + _ds18B20.ReadLowTempAlarmSetting(device));
    ///                     Debug.WriteLine("High Temp Alarm for Device - " + GetDeviceId(device) + " - " + _ds18B20.ReadHighTempAlarmSetting(device));
    ///                 }
    ///                 */
    ///
    ///                 Debug.WriteLine("Only Device 1 (" + GetDeviceId(Device1) + ") will have an alarm when reading the temperature.\n");
    ///             }
    ///
    ///             private static void RunDemo2()
    ///             {
    ///                 Debug.WriteLine("*********DS18B20 Demo**********\n");
    ///                 Debug.WriteLine("Number of Devices: " + _ds18B20.NumberOfDevices() + "\n");
    ///
    ///                 Debug.WriteLine("Testing Setting Resolutions");
    ///                 foreach (Byte[] device in _ds18B20.DeviceList)
    ///                 {
    ///                     _ds18B20.SetResolution(device, DS18B20.Resolution.Resolution9Bit);
    ///                 }
    ///
    ///                 Debug.WriteLine("Setting Resolutions done, now reading back resolutions.");
    ///                 foreach (Byte[] device in _ds18B20.DeviceList)
    ///                 {
    ///                     Debug.WriteLine("Current Resolution for " + GetDeviceId(device) + " - " + _ds18B20.GetResolutionString(_ds18B20.GetResolution(device)));
    ///                 }
    ///
    ///                 Debug.WriteLine("\nTest - Checking for Parasitic Devices");
    ///                 foreach (Byte[] device in _ds18B20.DeviceList)
    ///                 {
    ///                     Debug.WriteLine("Is Parasitic? -" + GetDeviceId(device) + " - " + _ds18B20.IsParasitic(device));
    ///                 }
    ///
    ///                 Debug.WriteLine("\nTest - Resetting Alarms");
    ///                 foreach (Byte[] device in _ds18B20.DeviceList)
    ///                 {
    ///                     Debug.WriteLine("Resetting Alarms for Device " + GetDeviceId(device) + " yields " + _ds18B20.ResetAlarmSettings(device));
    ///                 }
    ///
    ///                 Debug.WriteLine("\nTest - Setting all Alarms at once");
    ///                 foreach (Byte[] device in _ds18B20.DeviceList)
    ///                 {
    ///                     _ds18B20.SetBothTemperatureAlarms(device, 100, 125);
    ///                 }
    ///
    ///                 Debug.WriteLine("\nSetting Alarms done, now reading back alarm settings.");
    ///                 foreach (Byte[] device in _ds18B20.DeviceList)
    ///                 {
    ///                     Debug.WriteLine("Low Temp Alarm for Device - " + GetDeviceId(device) + " - " + _ds18B20.ReadLowTempAlarmSetting(device));
    ///                     Debug.WriteLine("High Temp Alarm for Device - " + GetDeviceId(device) + " - " + _ds18B20.ReadHighTempAlarmSetting(device));
    ///                 }
    ///
    ///                 Debug.WriteLine("\nAll Devices will have an alarm when reading the temperature.\n");
    ///
    ///             }
    ///
    ///             private static void Capture()
    ///             {
    ///                 while (true)
    ///                 {
    ///                     Debug.WriteLine("Reading temperatures by DeviceID");
    ///
    ///                     foreach (Byte[] id in _ds18B20.DeviceList)
    ///                     {
    ///                         Debug.WriteLine("By ID - Device Address - " + GetDeviceId(id) + ", Temperature - " + _ds18B20.ReadTemperatureByAddress(id) + "°C");
    ///                     }
    ///
    ///                     Debug.WriteLine("\nReading all temperatures using ReadTemperatureForAllDevices() method");
    ///
    ///                     Hashtable temperature = _ds18B20.ReadTemperatureForAllDevices();
    ///
    ///                     foreach (DictionaryEntry t in temperature)
    ///                     {
    ///                         Debug.WriteLine("Key - " + GetDeviceId((Byte[])t.Key) + " Value - " + t.Value);
    ///                     }
    ///
    ///                     Debug.WriteLine("\nReadTemperature (single device) - " + _ds18B20.ReadTemperature() + "°C");
    ///
    ///                     Debug.WriteLine("\nChecking for devices in alarm - Method 1");
    ///
    ///                     foreach (Byte[] alarmingDevice in _ds18B20.AlarmList())
    ///                     {
    ///                         Debug.WriteLine("Alarming Device Address - " + GetDeviceId(alarmingDevice));
    ///                     }
    ///
    ///                     Debug.WriteLine("\nChecking for devices in alarm - Method 2");
    ///
    ///                     foreach (Byte[] alarmingDevice in _ds18B20.AlarmList())
    ///                     {
    ///                         Debug.WriteLine("Devices in alarm:  " + _ds18B20.HasAlarm(alarmingDevice) + " - " + GetDeviceId(alarmingDevice));
    ///                     }
    ///
    ///                     Debug.WriteLine("\n***************************************************************************************************");
    ///                     Thread.Sleep(5000);
    ///                 }
    ///             }
    ///
    ///             private static String GetDeviceId(Byte[] id)
    ///             {
    ///                 return id[0] + "," + id[1] + "," + id[2] + "," + id[3] + "," + id[4] + "," + id[5] + "," + id[6] + "," + id[7];
    ///             }
    ///    }
    ///}    /// </code>
    /// </example>
    public class DS18B20 : ITemperature
    {
        #region CTOR/DTOR

        /// <summary>
        /// Initializes a new instance of the <see cref="DS18B20"/> class.
        /// </summary>
        /// <param name="oneWirePin">The data input/output pin for the 1-Wire Bus.</param>
        public DS18B20(Int32 oneWirePin)
        {
#if (NANOFRAMEWORK_1_0)
            _oneWire = new OneWireHost(); //TODO: is this on the right pin?
#else
            _oneWire = new OneWireController(oneWirePin);
#endif

            _deviceList = new ArrayList();
            _deviceList = GetDeviceList();

            if (_deviceList.Count == 0) throw new DeviceInitialisationException("No DS18B20 Sensors found on the 1-Wire Bus.");
        }

        #endregion

        #region Constants

        private const Byte TempLsb = 0;
        private const Byte TempMsb = 1;
        private const Byte HighTempAlarmBit = 2;
        private const Byte LowTempAlarmbit = 3;
        private const Byte ConfigurationBit = 4;

        #endregion

        #region Fields

        private static ArrayList _deviceList;
#if (NANOFRAMEWORK_1_0)
        private static OneWireHost _oneWire;
#else
        private static OneWireController _oneWire;
#endif

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
            ///     9-Bit with a resolution of 0.5°C.
            /// </summary>
            Resolution9Bit = 0x1F,
            /// <summary>
            ///     10-Bit with a resolution of 0.25°C.
            /// </summary>
            Resolution10Bit = 0x3F,
            /// <summary>
            ///     11-Bit with a resolution of 0.125°C.
            /// </summary>
            Resolution11Bit = 0x5F,
            /// <summary>
            ///     12-Bit with a resolution of 0.0625°C.
            /// </summary>
            Resolution12Bit = 0x7F
        }

#endregion

#region Public Properties

        /// <summary>
        ///     Returns an ArrayList of the unique 64-Bit Addresses of all DS18B20 sensors on the OneWire Bus.
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
        ///     Exposes the native NETMF OneWire interface object of the OneWire Bus.
        /// </summary>
#if (NANOFRAMEWORK_1_0)
        public OneWireHost Interface => _oneWire;
#else
        public OneWireController Interface => _oneWire;
#endif

        /// <summary>
        ///     Gets the number of DS18B20 devices on the OneWire Bus.
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
        /// _ds18B20.TemperatureUnit = TemperatureUnits.Farhenheit;
        /// </code>
        /// </example>
        public TemperatureUnits TemperatureUnit = TemperatureUnits.Celsius;

#endregion

#region Pubic Methods

#region Device Related Methods

        /// <summary>
        /// Returns an ArrayList of all DS18B20 sensors on the OneWire Bus.
        /// </summary>
        /// <returns>An <see cref="ArrayList"/> of all DS18B20 Sensors on the OneWire Bus.</returns>
        /// <example>Example usage:
        /// <code language="C#">
        /// foreach (byte[] device in _ds18B20.DeviceList)
        ///  {
        ///     Debug.WriteLine(device.GetHashCode().ToString());
        ///  }
        /// </code>
        /// </example>
        public static ArrayList GetDeviceList()
        {
            _deviceList.Clear();
            ArrayList tempList = _oneWire.FindAllDevices();
            foreach (Byte[] id in tempList)
            {
                if (id[0] == 0x28)
                {
                    _deviceList.Add(id);
                }
            }
            return _deviceList;
        }

        /// <summary>
        /// Determines if the DS18B20 device is using Parasitic Power.
        /// </summary>
        /// <param name="oneWireAddress">The address of the sensor to check.</param>
        /// <returns>True is the device is using Parasitic Power, otherwise false.</returns>
        /// <example>Example usage:
        /// <code language="C#">
        /// foreach (byte[] device in _ds18B20.DeviceList)
        /// {
        ///     Debug.WriteLine("Is Parasitic? -" + IsParasitic(device));
        /// }
        /// </code>
        /// </example>
        public Boolean IsParasitic(Byte[] oneWireAddress)
        {
            if (!SelectDevice(oneWireAddress)) return false;
            _oneWire.WriteByte((Byte) FunctionCommands.ReadPowerSupply);
            return _oneWire.ReadByte() == 0;
        }

        /// <summary>
        /// Determines if the DeviceId is valid.
        /// </summary>
        /// <param name="id">The 64-bit Device ID to verify.</param>
        /// <returns>True if the Device ID is valid, otherwise, false.</returns>
        /// <example>Example usage:
        /// <code language="C#">
        /// foreach (byte[] device in _ds18B20.DeviceList)
        /// {
        ///     Debug.WriteLine("Is Device Valid? -" + _ds18B20.IsValidId(device));
        /// }
        /// </code>
        /// </example>
        public Boolean IsValidId(Byte[] id)
        {
            IEnumerator enumerator = DeviceList.GetEnumerator();
            enumerator.Reset();

            while (enumerator.MoveNext())
            {
                Byte[] de = (Byte[]) enumerator.Current;
                return de != null && de.GetHashCode() == id.GetHashCode();
            }

            return false;
        }

#endregion

#region Device Resolution Methods

        /// <summary>
        ///     Returns the current resolution of the device, 9-12 Bit.
        /// </summary>
        /// <param name="oneWireAddress">The address of the DS18B20 sensor to check,.</param>
        /// <returns>The Resolution, see the <see cref="Resolution"/> for more information.</returns>
        /// <example>Example usage:
        /// <code language="C#">
        /// foreach (byte[] device in _ds18B20.DeviceList)
        /// {
        ///     Debug.WriteLine("Current Resolution for " + GetDeviceId(device) + " - "  + _ds18B20.GetResolutionString(_ds18B20.GetResolution(device)));
        /// }
        /// </code>
        /// </example>
        public Resolution GetResolution(Byte[] oneWireAddress)
        {
            GetDeviceList();
#if (NANOFRAMEWORK_1_0)
            if (!IsValidId(oneWireAddress) || _oneWire.TouchReset() == false)
#else
            if (!IsValidId(oneWireAddress) || _oneWire.TouchReset() <= 0)
#endif
            {
                return Resolution.Resolution12Bit;
            }

            _oneWire.WriteByte((Byte) RomCommands.MatchRom);
            _oneWire.WriteByte((Byte) FunctionCommands.ReadScratchPad);

            while (_oneWire.ReadByte() == 0)
            {
            } // wait while busy

            _oneWire.TouchReset();
            _oneWire.WriteByte((Byte) RomCommands.MatchRom);

            Byte[] scratchPad = ReadScratchPad(oneWireAddress);

            return (Resolution) scratchPad[ConfigurationBit];
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
        ///     Debug.WriteLine("Current Resolution - "  + GetResolutionString(GetResolution(device)));
        /// }
        /// </code>
        /// </example>
        public String GetResolutionString(Resolution resolution)
        {
            switch (resolution)
            {
                case Resolution.Resolution9Bit:
                {
                    return "Resolution9Bit";
                }

                case Resolution.Resolution10Bit:
                {
                    return "Resolution10Bit";
                }

                case Resolution.Resolution11Bit:
                {
                    return "Resolution11Bit";
                }

                case Resolution.Resolution12Bit:
                {
                    return "Resolution12Bit";
                }
            }
            return "Resolution Unknown";
        }

        /// <summary>
        ///     Sets the resolution for a specific device on the OneWire bus.
        /// </summary>
        /// <param name="oneWireAddress">The 1-Wire Address of the device to set the resolution.</param>
        /// <param name="resolution">The resolution to set all devices to. See <see cref="Resolution"/> for more information.</param>
        /// <param name="writeToEeprom">If true, writes the new resolution to EEPROM NVRAM</param>
        /// <example>Example usage:
        /// <code language="C#">
        /// foreach (byte[] device in _ds18B20.DeviceList)
        /// {
        ///     SetResolution(device, DS18B20.Resolution.Resolution9Bit);
        /// }
        /// </code>
        /// </example>
        /// <returns>True if successful or otherwise false.</returns>
        public Boolean SetResolution(Byte[] oneWireAddress, Resolution resolution, Boolean writeToEeprom = true)
        {
            GetDeviceList();

            if (!IsValidId(oneWireAddress)) return false;

            Byte[] scratchPad = ReadScratchPad(oneWireAddress);

            switch (resolution)
            {
                case Resolution.Resolution9Bit:
                {
                    scratchPad[ConfigurationBit] = (Byte) Resolution.Resolution9Bit;
                    break;
                }

                case Resolution.Resolution10Bit:
                {
                    scratchPad[ConfigurationBit] = (Byte) Resolution.Resolution10Bit;
                    break;
                }

                case Resolution.Resolution11Bit:
                {
                    scratchPad[ConfigurationBit] = (Byte) Resolution.Resolution11Bit;
                    break;
                }

                case Resolution.Resolution12Bit:
                {
                    scratchPad[ConfigurationBit] = (Byte) Resolution.Resolution12Bit;
                    break;
                }
            }

            return WriteScratchPad(oneWireAddress, scratchPad);
        }

        /// <summary>
        ///     Sets the resolution for all devices on the OneWire bus in one call. All devices will have the same resolution.
        /// </summary>
        /// <param name="resolution">The resolution to set all devices to. See <see cref="Resolution"/> for more information.</param>
        /// <param name="writeToEeprom">If true, writes the new resolution to EEPROM NVRAM</param>
        /// <returns>True if successful, otherwise false.</returns>
        /// <example>Example usage:
        /// <code language="C#">
        /// SetResolutionForAllDevices(DS18B20.Resolution.Resolution9Bit);
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
        ///     Reads the temperature of a single DS18B20 Temperature Sensor configuration or the first device found on the OneWire Bus in a multiple sensor configuration.
        /// </summary>
        /// <param name="source">The temperature source <see cref="TemperatureSources"/> for more information.</param>
        /// <remarks>
        ///     Calling this method in a multi sensor Bus will only return the temperature of the first sensor found on the OneWire Bus.
        ///     To read multiple sensors, use the <see cref="ReadTemperatureForAllDevices"/> method.
        /// </remarks>
        /// <returns>
        ///     A float representing the temperature read from the source in the °C.
        /// </returns>
        /// <exception cref="NotSupportedException">Throws a NotSupportedException if the sensor being read is powered in Parasitic Mode.</exception>
        /// <example>Example usage:
        /// <code language="C#">
        ///     Debug.WriteLine("ReadTemperature (deviceID) - " + ReadTemperature() + "°C"); // Where deviceID is the 64-Bit device identifier of your DS18B20.
        /// </code>
        /// </example>
        public Single ReadTemperature(TemperatureSources source = TemperatureSources.Ambient)
        {
            if (source == TemperatureSources.Object) throw new ArgumentException("TemperatureSources.Object not implemented for this module.");

            if (IsParasitic((Byte[]) DeviceList[0])) throw new NotSupportedException("Reading temperature in Parasitic Mode is not supported.");
#if (NANOFRAMEWORK_1_0)
            if (_oneWire.TouchReset() == false) return Single.MinValue;
#else
            if (_oneWire.TouchReset() <= 0) return Single.MinValue;
#endif

            _oneWire.WriteByte((Byte) RomCommands.SkipRom); // Skip ROM, we only have one device
            _oneWire.WriteByte((Byte) FunctionCommands.StartTemperatureConversion); // Start temperature conversion

            while (_oneWire.ReadByte() == 0)
            {
                _oneWire.TouchReset();
            }

            _oneWire.WriteByte((Byte) RomCommands.SkipRom);

            Byte[] device = (Byte[]) DeviceList[0];

            SelectDevice(device);

            Byte[] scratchPad = ReadScratchPad(device);

            Int32 temp = (scratchPad[TempMsb] << 8) | scratchPad[TempLsb];

            if ((scratchPad[1] & 0x80) != 0)
            {
                temp = (Int16) (scratchPad[0] | (scratchPad[1] << 8));
            }

            Single temperature = 0;

            switch (scratchPad[ConfigurationBit])
            {
                case (Byte) Resolution.Resolution12Bit:
                {
                    temperature =  (Single) (temp*0.0625);
                    break;
                }
                case (Byte) Resolution.Resolution11Bit:
                {
                    temperature = (Single) ((temp >> 1)*0.125);
                    break;
                }
                case (Byte) Resolution.Resolution10Bit:
                {
                    temperature =  (Single) ((temp >> 2)*0.25);
                    break;
                }
                case (Byte) Resolution.Resolution9Bit:
                {
                    temperature =  (Single) ((temp >> 3)*0.5);
                    break;
                }
            }
            return ConvertToScale(temperature);
        }

        /// <summary>
        ///     Reads the temperature of a DS18B20 Sensor by its unique 64-bit Address.
        /// </summary>
        /// <param name="oneWireAddress">The 64-bit address of the sensor to read.</param>
        /// <returns>The temperature of the sensor being passed to the method.</returns>
        /// <exception cref="NotSupportedException">Throws a NotSupportedException if the sensor being read is powered in Parasitic Mode.</exception>
        /// <example>Example usage:
        /// <code language="C#">
        /// foreach (byte[] id in _ds18B20.DeviceList)
        /// {
        ///     Debug.WriteLine("By ID - Device Address - " + GetDeviceId(id) + ", Temperature - " + _ds18B20.ReadTemperatureByAddress(id) + "°C");
        /// }
        /// </code>
        /// </example>
        public Single ReadTemperatureByAddress(Byte[] oneWireAddress)
        {
            GetDeviceList();

            if (IsParasitic(oneWireAddress)) throw new NotSupportedException("Reading temperature in Parasitic Mode is not supported.");

            if (!IsValidId(oneWireAddress)) return Single.MinValue;

            SelectDevice(oneWireAddress);

            _oneWire.WriteByte((Byte)FunctionCommands.StartTemperatureConversion);

            while (_oneWire.ReadByte() == 0)
            {
                _oneWire.TouchReset();
            }

            SelectDevice(oneWireAddress);

            Byte[] scratchPad = ReadScratchPad(oneWireAddress);

            Int32 temp = (scratchPad[TempMsb] << 8) | scratchPad[TempLsb];

            if ((scratchPad[1] & 0x80) != 0)
            {
                temp = (Int16)(scratchPad[0] | (scratchPad[1] << 8));
            }

            Single temperature = 0;

            switch (scratchPad[ConfigurationBit])
            {
                case (Byte)Resolution.Resolution12Bit:
                {
                    temperature = (Single)(temp * 0.0625);
                    break;
                }
                case (Byte)Resolution.Resolution11Bit:
                {
                    temperature = (Single)((temp >> 1) * 0.125);
                    break;
                }
                case (Byte)Resolution.Resolution10Bit:
                {
                    temperature = (Single)((temp >> 2) * 0.25);
                    break;
                }
                case (Byte)Resolution.Resolution9Bit:
                {
                    temperature = (Single)((temp >> 3) * 0.5);
                    break;
                }
            }
            return ConvertToScale(temperature);
        }

        /// <summary>
        ///     Reads all DS18B20 Temperature Sensors on the OneWire Bus.
        /// </summary>
        /// <returns>A HashTable containing a Key/Value Pair of the unique 64-Bit Address of the DS18B20 and the temperature in °C.</returns>
        /// <remarks>This method works in either a single sensor or multi sensor configuration.</remarks>
        /// <example>Example usage:
        /// <code language="C#">
        /// Hashtable temperature = _ds18B20.ReadTemperatureForAllDevices();
        /// foreach (DictionaryEntry t in temperature)
        /// {
        ///     Debug.WriteLine("Key - " + GetDeviceId((byte[]) t.Key) + " Value - " + t.Value);
        /// }
        /// </code>
        /// </example>
        public Hashtable ReadTemperatureForAllDevices()
        {
            GetDeviceList();
            Hashtable temperature = new Hashtable();
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
        ///     Gets an ArrayList of all DS18B20 sensors on the OneWire Bus that have the AlarmFlag set.
        /// </summary>
        /// <returns>An <see cref="ArrayList"/> of all devices in alarm.</returns>
        /// <example>Example usage:
        /// <code language="C#">
        /// foreach (byte[] alarmingDevice in _ds18B20.AlarmList())
        /// {
        ///     Debug.WriteLine("Alarming Device Address - " + alarmingDevice);
        /// }
        /// </code>
        /// </example>
        public ArrayList AlarmList()
        {
            ArrayList alarmList = new ArrayList();
#if (NANOFRAMEWORK_1_0)
            // find the first device (only devices alarming)
            bool rslt = _oneWire.FindFirstDevice(true, true);
            while (rslt == true)
            {
                // retrieve the serial number just found
                var sNum = _oneWire.SerialNumber;

                // save serial number
                alarmList.Add(sNum);

                // find the next alarming device
                rslt = _oneWire.FindNextDevice(true, true);
            }
#else
            // find the first device (only devices alarming)
            Int32 rslt = _oneWire.FindFirstDevice(true, true);
            while (rslt != 0)
            {
                Byte[] sNum = new Byte[8];

                // retrieve the serial number just found
                _oneWire.SerialNum(sNum, true);

                // save serial number
                alarmList.Add(sNum);

                // find the next alarming device
                rslt = _oneWire.FindNextDevice(true, true);
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
        ///     Debug.WriteLine("Devices in alarm:  " + HasAlarm(alarmingDevice) + " - " + GetDeviceId(alarmingDevice));
        /// }
        /// </code>
        /// </example>
        public Boolean HasAlarm(Byte[] oneWireAddress)
        {
            GetDeviceList(); // ToDo -  Should this be named UpdateDeviceList?

            ArrayList alarmArray = AlarmList();

            foreach (Byte[] device in alarmArray)
            {
                if (device.GetHashCode() == oneWireAddress.GetHashCode()) return true;
            }

            return false;
        }

        /// <summary>
        ///     Resets the Low and High Temperature Alarms Settings for the specified OneWire Device to -55°C and 125° (the range of the DS18B20 sensor).
        /// </summary>
        /// <param name="oneWireAddress">The OneWire device address.</param>
        /// <param name="writeToEeprom">If true, writes the new settings to the device EEPROM.</param>
        /// <returns>True if successful or otherwise false.</returns>
        /// <remarks>All temperature alarms settings are in the <see cref="TemperatureUnits.Celsius"/> unit only.</remarks>
        /// <example>Example usage:
        /// <code language="C#">
        /// foreach (byte[] device in _ds18B20.DeviceList)
        /// {
        ///     _ds18B20.ResetAlarmSettings(device, true);
        /// }
        /// </code>
        /// </example>
        public Boolean ResetAlarmSettings(Byte[] oneWireAddress, Boolean writeToEeprom = true)
        {
            GetDeviceList();

            if (!IsValidId(oneWireAddress)) return false;

            SelectDevice(oneWireAddress);

            SByte[] scratchPad = ReadScratchPadSigned(oneWireAddress);
            scratchPad[HighTempAlarmBit] = 125;
            scratchPad[LowTempAlarmbit] = -55;

            return WriteScratchPad(oneWireAddress, scratchPad, writeToEeprom);
        }

        /// <summary>
        ///     Returns the Low Temperature Alarm setting of the specified device in °C.
        /// </summary>
        /// <param name="oneWireAddress">The OneWire Address of the specified device.</param>
        /// <returns>The Low Temperature Alarm setting in °C</returns>
        /// <remarks>All temperature alarms settings are in the <see cref="TemperatureUnits.Celsius"/> unit only.</remarks>
        /// <example>Example usage:
        /// <code language="C#">
        /// foreach (byte[] device in DeviceList)
        /// {
        ///     Debug.WriteLine("Low Temp Alarm for Device - "  + ReadLowTempAlarmSetting(device));
        /// }
        /// </code>
        /// </example>
        public Int32 ReadLowTempAlarmSetting(Byte[] oneWireAddress)
        {
            GetDeviceList();

#if (NANOFRAMEWORK_1_0)
            if (!IsValidId(oneWireAddress) || _oneWire.TouchReset() == false) return Int32.MinValue;
#else
            if (!IsValidId(oneWireAddress) || _oneWire.TouchReset() <= 0) return Int32.MinValue;
#endif

            SelectDevice(oneWireAddress);

            SByte[] scratchPad = ReadScratchPadSigned(oneWireAddress);

            return scratchPad[LowTempAlarmbit];
        }

        /// <summary>
        ///     Returns the High Temperature Alarm setting of the specified device in °C.
        /// </summary>
        /// <param name="oneWireAddress">The OneWire Address of the specified device.</param>
        /// <returns>The High Temperature Alarm setting in °C</returns>
        /// <remarks>All temperature alarms settings are in the <see cref="TemperatureUnits.Celsius"/> unit only.</remarks>
        /// <example>Example usage:
        /// <code language="C#">
        /// foreach (byte[] device in DeviceList)
        /// {
        ///     Debug.WriteLine("High Temp Alarm for Device - " + ReadHighTempAlarmSetting(device));
        /// }
        /// </code>
        /// </example>
        public Int32 ReadHighTempAlarmSetting(Byte[] oneWireAddress)
        {
            GetDeviceList();

#if (NANOFRAMEWORK_1_0)
            if (!IsValidId(oneWireAddress) || _oneWire.TouchReset() == false) return Int32.MinValue;
#else
            if (!IsValidId(oneWireAddress) || _oneWire.TouchReset() <= 0) return Int32.MinValue;
#endif

            SelectDevice(oneWireAddress);

            SByte[] scratchPad = ReadScratchPadSigned(oneWireAddress);

            return scratchPad[HighTempAlarmBit];
        }

        /// <summary>
        /// Sets the low alarm temperature for a specific device in °C.
        /// </summary>
        /// <param name="oneWireAddress">The OneWire Address of the specified device.</param>
        /// <param name="lowTemperatureAlarm">Low Temperature Alarm set point. The valid range is -55°C - 125°C </param>
        /// <param name="writeToEeprom">If true, writes the new Low Temperature Alarm setting to NVRAM EEPROM.</param>
        /// <returns>True if successful or otherwise false.</returns>
        /// <exception cref="ArgumentException">Throws an ArgumentException if the <para>lowTemperatureAlarm</para> setting is higher than or equal to the <see cref="SetHighTempertureAlarm"/> method.</exception>
        /// <remarks>All temperature alarms settings are in the <see cref="TemperatureUnits.Celsius"/> unit only.</remarks>
        /// <example>Example usage:
        /// <code language="C#">
        ///  foreach (byte[] device in _ds18B20.DeviceList)
        /// {
        ///     SetLowTempertureAlarm(device, 20, true);
        /// }
        /// </code>
        /// </example>
        public Boolean SetLowTempertureAlarm(Byte[] oneWireAddress, SByte lowTemperatureAlarm, Boolean writeToEeprom = true)
        {
            GetDeviceList();

            if (!IsValidId(oneWireAddress)) return false;

            // Make sure the alarm setting is within the device's range (-55°C to 125°C)
            lowTemperatureAlarm = (SByte) (lowTemperatureAlarm < -55 ? -55 : lowTemperatureAlarm > 125 ? 125 : lowTemperatureAlarm);

            // Verify the Low Temperature alarm setting is not higher than the High Temperature Alarm setting in EEPROM, if so reset the values to default.
            if (lowTemperatureAlarm >= ReadHighTempAlarmSetting(oneWireAddress)) throw new ArgumentException("Low Temperature Alarm setting must not be higher than the High Temperature Alarm setting.", nameof(lowTemperatureAlarm));

            SByte[] scratchPad = ReadScratchPadSigned(oneWireAddress);
            scratchPad[LowTempAlarmbit] = lowTemperatureAlarm;

            return WriteScratchPad(oneWireAddress, scratchPad);
        }

        /// <summary>
        /// Sets the low alarm temperature for a specific device in °C.
        /// </summary>
        /// <param name="oneWireAddress">The OneWire Address of the specified device.</param>
        /// <param name="highTemperatureAlarm">High Temperature Alarm set point. The valid range is -55°C - 125°C </param>
        /// <param name="writeToEeprom">If true, writes the new Low Temperature Alarm setting to NVRAM EEPROM.</param>
        /// <returns>True if successful or otherwise false.</returns>
        /// <exception cref="ArgumentException">Throws an ArgumentException if the <para>lowTemperatureAlarm</para> setting is higher than or equal to the <see cref="SetHighTempertureAlarm"/> method.</exception>
        /// <remarks>All temperature alarms settings are in the <see cref="TemperatureUnits.Celsius"/> unit only.</remarks>
        /// <example>Example usage:
        /// <code language="C#">
        ///  foreach (byte[] device in _ds18B20.DeviceList)
        /// {
        ///     SetHighTempertureAlarm(device, 60, true);
        /// }
        /// </code>
        /// </example>
        public Boolean SetHighTempertureAlarm(Byte[] oneWireAddress, SByte highTemperatureAlarm, Boolean writeToEeprom = true)
        {
            GetDeviceList();

            if (!IsValidId(oneWireAddress)) throw new ArgumentException("Invalid OneWired Address", nameof(oneWireAddress));

            // Make sure the alarm setting is within the device's range (-55°C to 125°C)
            highTemperatureAlarm = (SByte) (highTemperatureAlarm < -55 ? -55 : highTemperatureAlarm > 125 ? 125 : highTemperatureAlarm);

            // Verify the Low Temperature alarm setting is not higher than the High Temperature Alarm setting in EEPROM, if so reset the values to default.
            if (highTemperatureAlarm <= ReadLowTempAlarmSetting(oneWireAddress)) throw new ArgumentException("High Temperature Alarm setting must not be lower than or equal to the Low Temperature Alarm setting.", nameof(highTemperatureAlarm));

            SByte[] scratchPad = ReadScratchPadSigned(oneWireAddress);
            scratchPad[HighTempAlarmBit] = highTemperatureAlarm;

            return WriteScratchPad(oneWireAddress, scratchPad, writeToEeprom);
        }

        /// <summary>
        /// Sets both the low  and hight temperature alarm at once for a specific device in °C.
        /// </summary>
        /// <param name="oneWireAddress">The OneWire Address of the specified device.</param>
        /// <param name="lowTemperatureAlarm">Low Temperature Alarm set point. The valid range is -55°C - 125°C </param>
        /// <param name="highTemperatureAlarm">High Temperature Alarm set point. The valid range is -55°C - 125°C </param>
        /// <param name="writeToEeprom">If true, writes the new Low Temperature Alarm setting to NVRAM EEPROM.</param>
        /// <returns>True if successful or otherwise false.</returns>
        /// <exception cref="ArgumentException">Throws an ArgumentException if the <para>lowTemperatureAlarm</para> setting is higher than or equal to the <see cref="SetHighTempertureAlarm"/> method or if the <para>highTemperatureAlarm</para> setting is lower than or equal to the <see cref="SetLowTempertureAlarm"/> method.</exception>
        /// <remarks>All temperature alarms settings are in the <see cref="TemperatureUnits.Celsius"/> unit only.</remarks>
        /// <example>Example usage:
        /// <code language="C#">
        /// foreach (byte[] device in _ds18B20.DeviceList)
        /// {
        ///     SetBothTemperatureAlarms(device,-55, 125);
        /// }
        /// </code>
        /// </example>
        public Boolean SetBothTemperatureAlarms(Byte[] oneWireAddress, SByte lowTemperatureAlarm, SByte highTemperatureAlarm, Boolean writeToEeprom = true)
        {
            GetDeviceList();

            if (!IsValidId(oneWireAddress)) return false;

            // Make sure the alarm setting is within the device's range (-55°C to 125°C)
            highTemperatureAlarm = (SByte)(highTemperatureAlarm < -55 ? -55 : highTemperatureAlarm > 125 ? 125 : highTemperatureAlarm);
            lowTemperatureAlarm = (SByte)(lowTemperatureAlarm < -55 ? -55 : lowTemperatureAlarm > 125 ? 125 : lowTemperatureAlarm);

            // Verify the Low Temperature alarm setting is not higher than the High Temperature Alarm setting in EEPROM, if so reset the values to default.
            if (highTemperatureAlarm <= lowTemperatureAlarm)
            {
                throw new ArgumentException("High Temperature Alarm setting must not be lower than or equal to the Low Temperature Alarm setting.", nameof(highTemperatureAlarm));
            }
            if (lowTemperatureAlarm >= highTemperatureAlarm)
            {
                throw new ArgumentException("Low Temperature Alarm setting must not be higher than or equal to the High Temperature Alarm setting.", nameof(lowTemperatureAlarm));
            }

            SByte[] scratchPad = ReadScratchPadSigned(oneWireAddress);
            scratchPad[HighTempAlarmBit] = highTemperatureAlarm;
            scratchPad[LowTempAlarmbit] = lowTemperatureAlarm;

            return WriteScratchPad(oneWireAddress, scratchPad, writeToEeprom);
        }

        /// <summary>
        /// Sets both the low  and hight temperature alarm at once for all devices on the One-Wire Bus device in °C.
        /// </summary>
        /// <param name="lowTemperatureAlarm">Low Temperature Alarm set point. The valid range is -55°C - 125°C </param>
        /// <param name="highTemperatureAlarm">High Temperature Alarm set point. The valid range is -55°C - 125°C </param>
        /// <param name="writeToEeprom">If true, writes the new Low Temperature Alarm setting to NVRAM EEPROM.</param>
        /// <returns>True if successful or otherwise false.</returns>
        /// <exception cref="ArgumentException">Throws an ArgumentException if the <para>lowTemperatureAlarm</para> setting is higher than or equal to the <see cref="SetHighTempertureAlarm"/> method or if the <para>highTemperatureAlarm</para> setting is lower than or equal to the <see cref="SetLowTempertureAlarm"/> method.</exception>
        /// <remarks>All temperature alarms settings are in the <see cref="TemperatureUnits.Celsius"/> unit only.</remarks>
        /// <example>
        /// <code language="C#">
        ///     SetAlarmsForAlllDevic(20, 60, true);
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

        private Byte[] ReadScratchPad(Byte[] oneWireAddress)
        {
            GetDeviceList();

            Byte[] scratchPad = new Byte[9];

            if (!IsValidId(oneWireAddress)) return scratchPad;

            SelectDevice(oneWireAddress);

            _oneWire.WriteByte((Byte) FunctionCommands.ReadScratchPad);

            for (UInt16 i = 0; i < 9; i++)
            {
                scratchPad[i] = (Byte) _oneWire.ReadByte();
            }
            return scratchPad;
        }

        private SByte[] ReadScratchPadSigned(Byte[] oneWireAddress)
        {
            GetDeviceList();

            SByte[] scratchPad = new SByte[9];

            if (!IsValidId(oneWireAddress)) return scratchPad;

            SelectDevice(oneWireAddress);

            _oneWire.WriteByte((Byte) FunctionCommands.ReadScratchPad);

            for (UInt16 i = 0; i < 9; i++)
            {
                scratchPad[i] = (SByte) _oneWire.ReadByte();
            }
            return scratchPad;
        }

        private static Boolean SelectDevice(Byte[] oneWireAddress)
        {
#if (NANOFRAMEWORK_1_0)
            if (_oneWire.TouchReset() == false) return false;
#else
            if (_oneWire.TouchReset() <= 0) return false;
#endif

            _oneWire.WriteByte((Byte) RomCommands.MatchRom);

            for (Int32 i = 0; i <= 7; i++)
            {
                _oneWire.WriteByte(oneWireAddress[i]);
            }
            return true;
        }

        private Boolean WriteScratchPad(Byte[] oneWireAddress, Byte[] scratchPad, Boolean writeToEeprom = true)
        {
            GetDeviceList();

            if (!IsValidId(oneWireAddress)) return false;

            SelectDevice(oneWireAddress);

            _oneWire.WriteByte((Byte) FunctionCommands.WriteScratchPad);
            _oneWire.WriteByte(scratchPad[HighTempAlarmBit]); // high alarm temp
            _oneWire.WriteByte(scratchPad[LowTempAlarmbit]); // low alarm temp
            _oneWire.WriteByte(scratchPad[ConfigurationBit]); // configuration


            SelectDevice(oneWireAddress);

            if (writeToEeprom)
            {
                SelectDevice(oneWireAddress);
                // save the newly written values to eeprom
                _oneWire.WriteByte((Byte) FunctionCommands.CopyScratchPad);
                while (_oneWire.ReadByte() == 0)
                {
                }
            }
            // Verify write to EEPROM
            _oneWire.TouchReset();
            _oneWire.WriteByte((Byte) FunctionCommands.RecallEe); // ToDo - Necessary?
            Byte[] scratchPad2 = ReadScratchPad(oneWireAddress);

            return scratchPad.GetHashCode() == scratchPad2.GetHashCode();
        }

        private Boolean WriteScratchPad(Byte[] oneWireAddress, SByte[] scratchPad, Boolean writeToEeprom = true)
        {
            GetDeviceList();

            if (!IsValidId(oneWireAddress)) return false;

            SelectDevice(oneWireAddress);

#if (NANOFRAMEWORK_1_0)
            _oneWire.WriteByte((byte)FunctionCommands.WriteScratchPad);
            _oneWire.WriteByte((byte)scratchPad[HighTempAlarmBit]); // high alarm temp
            _oneWire.WriteByte((byte)scratchPad[LowTempAlarmbit]); // low alarm temp
            _oneWire.WriteByte((byte)scratchPad[ConfigurationBit]); // configuration
#else
            _oneWire.WriteByte((Byte) FunctionCommands.WriteScratchPad);
            _oneWire.WriteByte(scratchPad[HighTempAlarmBit]); // high alarm temp
            _oneWire.WriteByte(scratchPad[LowTempAlarmbit]); // low alarm temp
            _oneWire.WriteByte(scratchPad[ConfigurationBit]); // configuration
#endif


            SelectDevice(oneWireAddress);

            if (writeToEeprom)
            {
                SelectDevice(oneWireAddress);
                // save the newly written values to EEPROM
                _oneWire.WriteByte((Byte) FunctionCommands.CopyScratchPad);
                while (_oneWire.ReadByte() == 0)
                {
                }
            }
            // Verify write to EEPROM
            _oneWire.TouchReset();
            _oneWire.WriteByte((Byte) FunctionCommands.RecallEe); // ToDo - Necessary?
            SByte[] scratchPad2 = ReadScratchPadSigned(oneWireAddress);

            return scratchPad.GetHashCode() == scratchPad2.GetHashCode();
        }

        private Single ConvertToScale(Single temperature)
        {
            switch (TemperatureUnit)
            {
                case TemperatureUnits.Celsius:
                {
                    return temperature;
                }

                case TemperatureUnits.Fahrenheit:
                {
                    return temperature * 9 / 5 + 32;
                }

                case TemperatureUnits.Kelvin:
                {
                    return temperature + 273.15F;
                }

                default:
                {
                    throw new ArgumentOutOfRangeException();
                }
            }
        }

#endregion
    }
}
