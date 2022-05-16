/*
 * Thermostat 2 click driver for TinyCLR 2.0
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

#region Usings

#if (NANOFRAMEWORK_1_0)
using System.Device.Gpio;
using nanoFramework.Device.OneWire;
#else
using GHIElectronics.TinyCLR.Devices.Gpio;
using GHIElectronics.TinyCLR.Devices.Onewire;
#endif

using System;
using System.Collections;

#endregion

namespace MBN.Modules
{
    /// <inheritdoc cref="ITemperature" />
    /// <summary>
    /// MikroBusNet Driver for the MikroElektronika Thermostat2 Click
    /// <para><b>Pins used :</b> Pwm and Cs</para>
    /// </summary>
    /// <example>
    /// <code language="C#">
    /// using MBN;
    /// using MBN.Modules;
    ///
    /// using System;
    /// using System.Diagnostics;
    /// using System.Threading;
    ///
    /// namespace Examples
    /// {
    ///     public class Program
    ///     {
    ///         private static Thermostat2Click _thermostat;
    ///
    ///         public static void Main()
    ///         {
    ///             _thermostat = new Thermostat2Click(Hardware.SC20100_1) {TemperatureUnit = TemperatureUnits.Celsius};
    ///
    ///             DisplayDeviceIds();
    ///
    ///             _thermostat.SetResolutionForAllDevices(Thermostat2Click.Resolution.Resolution12Bit, false);
    ///
    ///             _thermostat.SetAlarmsForAlllDevices(25, 29, false);
    ///
    ///             while (true)
    ///             {
    ///                 foreach (Byte[] sensor in _thermostat.DeviceList)
    ///                 {
    ///                     Single result = _thermostat.ReadTemperatureByAddress(sensor);
    ///
    ///                     if (_thermostat.HasAlarm(sensor))
    ///                     {
    ///                         TurnOnRelay();
    ///                     }
    ///                     else
    ///                     {
    ///                         TurnOffRelay();
    ///                     }
    ///
    ///                     Debug.WriteLine($"Device with ID of {GetDeviceId(sensor)} has a temperature of {result:f2} °C");
    ///
    ///                     Thread.Sleep(1000);
    ///                 }
    ///             }
    ///         }
    ///
    ///         private static void TurnOnRelay()
    ///         {
    ///             _thermostat.RelayState = true;
    ///         }
    ///
    ///         private static void TurnOffRelay()
    ///         {
    ///             _thermostat.RelayState = false;
    ///         }
    ///
    ///         private static void DisplayDeviceIds()
    ///         {
    ///             foreach (Byte[] device in _thermostat.DeviceList)
    ///             {
    ///                 Debug.WriteLine(GetDeviceId(device));
    ///             }
    ///         }
    ///
    ///         private static String GetDeviceId(Byte[] id)
    ///         {
    ///             return id[0] + ":" + id[1] + ":" + id[2] + ":" + id[3] + ":" + id[4] + ":" + id[5] + ":" + id[6] + ":" + id[7];
    ///         }
    ///     }
    /// }
    /// </code>
    /// </example>
    public class Thermostat2Click : ITemperature
    {
        #region .ctor

        /// <summary>
        ///     Initializes a new instance of the <see cref="Thermostat2Click" /> class.
        /// </summary>
        /// <param name="socket">The <see cref="Hardware.Socket" /> that the sensor is plugged in to.</param>
        public Thermostat2Click(Hardware.Socket socket)
        {
#if (NANOFRAMEWORK_1_0)
            Interface = new OneWireHost();

            _relayOutput = new GpioController().OpenPin(socket.Cs);
            _relayOutput.SetPinMode(PinMode.Output);
            _relayOutput.Write(PinValue.Low);
#else
            Interface = new OneWireController(socket.PwmPin);

            _relayOutput = GpioController.GetDefault().OpenPin(socket.Cs);
            _relayOutput.SetDriveMode(GpioPinDriveMode.Output);
            _relayOutput.Write(GpioPinValue.Low);
#endif

            _deviceList = new ArrayList();
            _deviceList = GetDeviceList();

            if (_deviceList.Count == 0) throw new DeviceInitialisationException("Thermostat2 Click not found on the 1-Wire Bus.");
        }

#endregion

#region Constants

        private const Byte TEMP_LSB_BIT = 0;
        private const Byte TEMP_MSB_BIT = 1;
        private const Byte HIGH_TEMP_ALARM_BIT = 2;
        private const Byte LOW_TEMP_ALARM_BIT = 3;
        private const Byte CONFIGURATION_BIT = 4;

#endregion

#region Fields

        private static ArrayList _deviceList;
        private static GpioPin _relayOutput;

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
        /// Gets or Sets the state of the on-board relay.
        /// </summary>
        /// <example>Example usage:
        /// <code language = "C#">
        /// _thermostat.RelayState = true;
        /// </code>
        /// </example>
        public Boolean RelayState
        {
#if (NANOFRAMEWORK_1_0)
            get => _relayOutput.Read() == PinValue.High;
            set => _relayOutput.Write(value ? PinValue.High : PinValue.Low);
#else
            get => _relayOutput.Read() == GpioPinValue.High;
            set => _relayOutput.Write(value ? GpioPinValue.High : GpioPinValue.Low);
#endif
        }

        /// <summary>
        ///     Returns an ArrayList of the unique 64-Bit Addresses of all DS18B20 sensors on the 1-Wire Bus.
        /// </summary>
        /// <example>Example usage:
        /// <code language = "C#">
        /// foreach (Byte[] sensor in _thermostat.DeviceList)
        /// {
        ///      Single result = _thermostat.ReadTemperatureByAddress(sensor);
        ///      Debug.WriteLine($"Device with ID of {GetDeviceId(sensor)} has a temperature of {result:f2} °C");
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
        ///     Exposes the native 1-Wire Bus.
        /// </summary>
        /// <example>Example usage:
        /// <code language = "C#">
        /// OneWireController _ow = _thermostat.Interface;
        /// _ow.Dispose();
        /// </code>
        /// </example>
#if (NANOFRAMEWORK_1_0)
        public OneWireHost Interface { get; }
#else
        public OneWireController Interface { get; }
#endif

        /// <summary>
        ///     Gets the number of DS18B20 devices on the OneWire Bus.
        /// </summary>
        /// <returns>The count of devices.</returns>
        /// <example>Example usage:
        /// <code language = "C#">
        /// Debug.WriteLine($"Number of devices on the 1-Wire bus is {_thermostat.NumberOfDevices}");
        /// </code>
        /// </example>
        public Int32 NumberOfDevices
        {
            get
            {
                GetDeviceList();
                return _deviceList.Count;
            }
        }

        /// <summary>
        /// Gets or sets the temperature unit for the <seealso cref="ReadTemperature" /> method.
        /// <remarks>
        /// <seealso cref="TemperatureUnits" />
        /// </remarks>
        /// </summary>
        /// <value>
        /// The temperature unit used.
        /// </value>
        /// <example>
        /// <code language="C#">
        /// // Set temperature unit to Fahrenheit
        /// _thermostat.TemperatureUnit = TemperatureUnits.Fahrenheit;
        /// </code>
        /// </example>
        public TemperatureUnits TemperatureUnit { get; set; } = TemperatureUnits.Celsius;

#endregion

#region Pubic Methods

#region Device Related Methods

        /// <summary>
        /// Returns an ArrayList of all DS18B20 sensors on the OneWire Bus.
        /// </summary>
        /// <returns>An <see cref="ArrayList"/> of all DS18B20 Sensors on the 1-Wire Bus.</returns>
        /// <example>
        /// Example usage:
        /// <code language="C#">
        /// foreach (byte[] device in _ds18B20.DeviceList)
        /// {
        ///    Debug.WriteLine(device.GetHashCode().ToString());
        /// }
        /// </code>
        /// </example>
        public ArrayList GetDeviceList()
        {
            _deviceList.Clear();
            ArrayList tempList = Interface.FindAllDevices();
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
        /// <example>
        /// Example usage:
        /// <code language="C#">
        /// foreach (byte[] device in _Thermostat.DeviceList)
        /// {
        ///     Debug.WriteLine("Is Parasitic? -" + IsParasitic(device));
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
        /// <example>
        /// Example usage:
        /// <code language="C#">
        /// foreach (byte[] device in _thermostat.DeviceList)
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
        /// Returns the current resolution of the device, 9-12 Bit.
        /// </summary>
        /// <param name="oneWireAddress">The address of the DS18B20 sensor to check,.</param>
        /// <returns>The Resolution, see the <see cref="Resolution" /> for more information.</returns>
        /// <example>
        /// Example usage:
        /// <code language="C#">
        /// foreach (byte[] device in _thermostat.DeviceList)
        /// {
        ///     Debug.WriteLine("Current Resolution for " + GetDeviceId(device) + " - "  + _ds18B20.GetResolutionString(_ds18B20.GetResolution(device)));
        /// }
        /// </code>
        /// </example>
        public Resolution GetResolution(Byte[] oneWireAddress)
        {
            GetDeviceList();

#if (NANOFRAMEWORK_1_0)
            if (!IsValidId(oneWireAddress) || Interface.TouchReset() != true)
#else
            if (!IsValidId(oneWireAddress) || Interface.TouchReset() <= 0)
#endif
            {
                return Resolution.Resolution12Bit;
            }

            Interface.WriteByte((Byte) RomCommands.MatchRom);
            Interface.WriteByte((Byte) FunctionCommands.ReadScratchPad);

            while (Interface.ReadByte() == 0) { } // wait while busy

            Interface.TouchReset();
            Interface.WriteByte((Byte) RomCommands.MatchRom);

            Byte[] scratchPad = ReadScratchPad(oneWireAddress);

            return (Resolution) scratchPad[CONFIGURATION_BIT];
        }

        /// <summary>
        /// Sets the resolution for a specific device on the OneWire bus.
        /// </summary>
        /// <param name="oneWireAddress">The 1-Wire Address of the device to set the resolution.</param>
        /// <param name="resolution">The resolution to set all devices to. See <see cref="Resolution" /> for more information.</param>
        /// <param name="writeToEeprom">If true, writes the new resolution to EEPROM NVRAM</param>
        /// <example>
        /// Example usage:
        /// <code language="C#">
        /// foreach (byte[] device in _thermostat.DeviceList)
        /// {
        ///     SetResolution(device, Thermostat2Click.Resolution.Resolution9Bit);
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
                    scratchPad[CONFIGURATION_BIT] = (Byte) Resolution.Resolution9Bit;
                    break;
                }
                case Resolution.Resolution10Bit:
                {
                    scratchPad[CONFIGURATION_BIT] = (Byte) Resolution.Resolution10Bit;
                    break;
                }
                case Resolution.Resolution11Bit:
                {
                    scratchPad[CONFIGURATION_BIT] = (Byte) Resolution.Resolution11Bit;
                    break;
                }
                case Resolution.Resolution12Bit:
                {
                    scratchPad[CONFIGURATION_BIT] = (Byte) Resolution.Resolution12Bit;
                    break;
                }
            }

            return WriteScratchPad(oneWireAddress, scratchPad);
        }

        /// <summary>
        /// Sets the resolution for all devices on the OneWire bus in one call. All devices will have the same resolution.
        /// </summary>
        /// <param name="resolution">The resolution to set all devices to. See <see cref="Resolution" /> for more information.</param>
        /// <param name="writeToEeprom">If true, writes the new resolution to EEPROM NVRAM</param>
        /// <returns>True if successful, otherwise false.</returns>
        /// <example>
        /// Example usage:
        /// <code language="C#">
        /// SetResolutionForAllDevices(Thermostat2Click.Resolution.Resolution9Bit);
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
        /// Reads the temperature of a DS18B20 Sensor by its unique 64-bit Address.
        /// </summary>
        /// <param name="oneWireAddress">The 64-bit address of the sensor to read.</param>
        /// <returns>The temperature of the sensor being passed to the method.</returns>
        /// <exception cref="NotSupportedException">
        /// Throws a NotSupportedException if the sensor being read is powered in Parasitic Mode.
        /// </exception>
        /// <example>
        /// Example usage:
        /// <code language="C#">
        /// foreach (byte[] id in _thermostat.DeviceList)
        /// {
        ///     Debug.WriteLine("By ID - Device Address - " + GetDeviceId(id) + ", Temperature - " + _thermostat.ReadTemperatureByAddress(id) + "°C");
        /// }
        /// </code>
        /// </example>
        public Single ReadTemperatureByAddress(Byte[] oneWireAddress)
        {
            GetDeviceList();

            if (IsParasitic(oneWireAddress))
                throw new NotSupportedException("Reading temperature in Parasitic Mode is not supported.");

            if (!IsValidId(oneWireAddress)) return Single.MinValue;

            SelectDevice(oneWireAddress);

            Interface.WriteByte((Byte) FunctionCommands.StartTemperatureConversion);

            while (Interface.ReadByte() == 0)
            {
                Interface.TouchReset();
            }

            SelectDevice(oneWireAddress);

            Byte[] scratchPad = ReadScratchPad(oneWireAddress);

            Int32 temp = (scratchPad[TEMP_MSB_BIT] << 8) | scratchPad[TEMP_LSB_BIT];

            if ((scratchPad[1] & 0x80) != 0)
            {
                temp = (Int16) ((scratchPad[0] + scratchPad[1]) << 8);
            }

            Single temperature = 0;

            switch (scratchPad[CONFIGURATION_BIT])
            {
                case (Byte) Resolution.Resolution12Bit:
                {
                    temperature = (Single) (temp * 0.0625);
                    break;
                }
                case (Byte) Resolution.Resolution11Bit:
                {
                    temperature = (Single) ((temp >> 1) * 0.125);
                    break;
                }
                case (Byte) Resolution.Resolution10Bit:
                {
                    temperature = (Single) ((temp >> 2) * 0.25);
                    break;
                }
                case (Byte) Resolution.Resolution9Bit:
                {
                    temperature = (Single) ((temp >> 3) * 0.5);
                    break;
                }
            }

            return ConvertToScale(temperature);
        }

        /// <summary>
        /// Reads all DS18B20 Temperature Sensors on the OneWire Bus.
        /// </summary>
        /// <returns>A HashTable containing a Key/Value Pair of the unique 64-Bit Address of the DS18B20 and the temperature in °C.</returns>
        /// <remarks>This method works in either a single sensor or multi sensor configuration.</remarks>
        /// <example>
        /// Example usage:
        /// <code language="C#">
        /// Hashtable temperature = _thermostat.ReadTemperatureForAllDevices();
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
                        temperature.Add(device, NumberOfDevices >= 1 ? ConvertToScale(ReadTemperatureByAddress(device)) : ConvertToScale(ReadTemperature()));
                    }

                    return temperature;
                }
            }
        }

#endregion

#region Alarm Related Methods

        /// <summary>
        /// Gets an ArrayList of all DS18B20 sensors on the OneWire Bus that have the AlarmFlag set.
        /// </summary>
        /// <returns>An <see cref="ArrayList" /> of all devices in alarm.</returns>
        /// <example>
        /// Example usage:
        /// <code language="C#">
        /// foreach (byte[] alarmingDevice in _thermostat.AlarmList())
        /// {
        ///     Debug.WriteLine("Alarming Device Address - " + alarmingDevice);
        /// }
        /// </code>
        /// </example>
        public ArrayList AlarmList()
        {
            ArrayList alarmList = new ArrayList();

            // find the first device (only devices alarming)
#if (NANOFRAMEWORK_1_0)
            bool rslt = Interface.FindFirstDevice(true, true);
            while (rslt != true)
            {
                Byte[] sNum = new Byte[8];

                // retrieve the serial number just found
                sNum = Interface.SerialNumber;

                // save serial number
                alarmList.Add(sNum);

                // find the next alarming device
                rslt = Interface.FindNextDevice(true, true);
            }
#else
            Int32 rslt = Interface.FindFirstDevice(true, true);
            while (rslt != 0)
            {
                Byte[] sNum = new Byte[8];

                // retrieve the serial number just found
                Interface.SerialNum(sNum, true);

                // save serial number
                alarmList.Add(sNum);

                // find the next alarming device
                rslt = Interface.FindNextDevice(true, true);
            }
#endif

            return alarmList;
        }

        /// <summary>
        /// Checks whether a specific device has the Alarm flag set.
        /// </summary>
        /// <param name="oneWireAddress">The 1-Wire Address to check.</param>
        /// <returns>True is the specified device is in alarm.</returns>
        /// <remarks>
        ///     The device Alarm Flag will not be cleared until a subsequent temperature conversion is done.
        ///     If the device has an alarm, best practice is to read the temperature again, if the Alarm Flag is still present, it
        ///     is an actual alarm.
        ///     If the device temperature changes and the device no longer is in alarm, the alarm flag will be cleared by doing a
        ///     subsequent temperature reading.
        /// </remarks>
        /// <example>
        /// Example usage:
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
        /// Resets the Low and High Temperature Alarms Settings for the specified OneWire Device to -55°C and 125° (the range of the DS18B20 sensor).
        /// </summary>
        /// <param name="oneWireAddress">The OneWire device address.</param>
        /// <param name="writeToEeprom">If true, writes the new settings to the device EEPROM.</param>
        /// <returns>True if successful or otherwise false.</returns>
        /// <remarks>All temperature alarms settings are in the <see cref="TemperatureUnits.Celsius" /> unit only.</remarks>
        /// <example>
        /// Example usage:
        /// <code language="C#">
        /// foreach (byte[] device in _thermostat.DeviceList)
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
            scratchPad[HIGH_TEMP_ALARM_BIT] = 125;
            scratchPad[LOW_TEMP_ALARM_BIT] = -55;

            return WriteScratchPad(oneWireAddress, scratchPad, writeToEeprom);
        }

        /// <summary>
        /// Returns the Low Temperature Alarm setting of the specified device in °C.
        /// </summary>
        /// <param name="oneWireAddress">The OneWire Address of the specified device.</param>
        /// <returns>The Low Temperature Alarm setting in °C</returns>
        /// <remarks>All temperature alarms settings are in the <see cref="TemperatureUnits.Celsius" /> unit only.</remarks>
        /// <example>
        /// Example usage:
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
            if (!IsValidId(oneWireAddress) || Interface.TouchReset() != true) return Int32.MinValue;
#else
            if (!IsValidId(oneWireAddress) || Interface.TouchReset() <= 0) return Int32.MinValue;
#endif

            SelectDevice(oneWireAddress);

            SByte[] scratchPad = ReadScratchPadSigned(oneWireAddress);

            return scratchPad[LOW_TEMP_ALARM_BIT];
        }

        /// <summary>
        /// Returns the High Temperature Alarm setting of the specified device in °C.
        /// </summary>
        /// <param name="oneWireAddress">The OneWire Address of the specified device.</param>
        /// <returns>The High Temperature Alarm setting in °C</returns>
        /// <remarks>All temperature alarms settings are in the <see cref="TemperatureUnits.Celsius" /> unit only.</remarks>
        /// <example>
        /// Example usage:
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
            if (!IsValidId(oneWireAddress) || Interface.TouchReset() != true) return Int32.MinValue;
#else
            if (!IsValidId(oneWireAddress) || Interface.TouchReset() <= 0) return Int32.MinValue;
#endif

            SelectDevice(oneWireAddress);

            SByte[] scratchPad = ReadScratchPadSigned(oneWireAddress);

            return scratchPad[HIGH_TEMP_ALARM_BIT];
        }

        /// <summary>
        /// Sets the low alarm temperature for a specific device in °C.
        /// </summary>
        /// <param name="oneWireAddress">The OneWire Address of the specified device.</param>
        /// <param name="lowTemperatureAlarm">Low Temperature Alarm set point. The valid range is -55°C - 125°C </param>
        /// <param name="writeToEeprom">If true, writes the new Low Temperature Alarm setting to NVRAM EEPROM.</param>
        /// <returns>True if successful or otherwise false.</returns>
        /// <exception cref="ArgumentException">Throws an ArgumentException if the lowTemperatureAlarm setting is higher than or equal to the <see cref="SetHighTempertureAlarm" /> method.
        /// </exception>
        /// <remarks>All temperature alarms settings are in the <see cref="TemperatureUnits.Celsius" /> unit only.</remarks>
        /// <example>
        /// Example usage:
        /// <code language="C#">
        /// foreach (byte[] device in _thermostat.DeviceList)
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
            lowTemperatureAlarm =
                (SByte) (lowTemperatureAlarm < -55 ? -55 : lowTemperatureAlarm > 125 ? 125 : lowTemperatureAlarm);

            // Verify the Low Temperature alarm setting is not higher than the High Temperature Alarm setting in EEPROM, if so reset the values to default.
            if (lowTemperatureAlarm >= ReadHighTempAlarmSetting(oneWireAddress))
                throw new ArgumentException(
                    "Low Temperature Alarm setting must not be higher than the High Temperature Alarm setting.",
                    nameof(lowTemperatureAlarm));

            SByte[] scratchPad = ReadScratchPadSigned(oneWireAddress);
            scratchPad[LOW_TEMP_ALARM_BIT] = lowTemperatureAlarm;

            return WriteScratchPad(oneWireAddress, scratchPad);
        }

        /// <summary>
        /// Sets the low alarm temperature for a specific device in °C.
        /// </summary>
        /// <param name="oneWireAddress">The OneWire Address of the specified device.</param>
        /// <param name="highTemperatureAlarm">High Temperature Alarm set point. The valid range is -55°C - 125°C </param>
        /// <param name="writeToEeprom">If true, writes the new Low Temperature Alarm setting to NVRAM EEPROM.</param>
        /// <returns>True if successful or otherwise false.</returns>
        /// <exception cref="ArgumentException">Throws an ArgumentException if the lowTemperatureAlarm setting is higher than or equal to the <see cref="SetHighTempertureAlarm" /> method.</exception>
        /// <remarks>All temperature alarms settings are in the <see cref="TemperatureUnits.Celsius" /> unit only.</remarks>
        /// <example>
        /// Example usage:
        /// <code language="C#">
        /// foreach (byte[] device in _thermostat.DeviceList)
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
            highTemperatureAlarm = (SByte) (highTemperatureAlarm < -55 ? -55 :
                highTemperatureAlarm > 125 ? 125 : highTemperatureAlarm);

            // Verify the Low Temperature alarm setting is not higher than the High Temperature Alarm setting in EEPROM, if so reset the values to default.
            if (highTemperatureAlarm <= ReadLowTempAlarmSetting(oneWireAddress))
                throw new ArgumentException(
                    "High Temperature Alarm setting must not be lower than or equal to the Low Temperature Alarm setting.",
                    nameof(highTemperatureAlarm));

            SByte[] scratchPad = ReadScratchPadSigned(oneWireAddress);
            scratchPad[HIGH_TEMP_ALARM_BIT] = highTemperatureAlarm;

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
        /// <exception cref="ArgumentException">Throws an ArgumentException if the lowTemperatureAlarm setting is higher than or equal to the <see cref="SetHighTempertureAlarm" /> method or if the highTemperatureAlarm setting is lower than or equal to the <see cref="SetLowTempertureAlarm" /> method.</exception>
        /// <remarks>All temperature alarms settings are in the <see cref="TemperatureUnits.Celsius" /> unit only.</remarks>
        /// <example>
        /// Example usage:
        /// <code language="C#">
        /// foreach (byte[] device in _thermostat.DeviceList)
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
            highTemperatureAlarm = (SByte) (highTemperatureAlarm < -55 ? -55 :
                highTemperatureAlarm > 125 ? 125 : highTemperatureAlarm);
            lowTemperatureAlarm =
                (SByte) (lowTemperatureAlarm < -55 ? -55 : lowTemperatureAlarm > 125 ? 125 : lowTemperatureAlarm);

            // Verify the Low Temperature alarm setting is not higher than the High Temperature Alarm setting in EEPROM, if so reset the values to default.
            if (highTemperatureAlarm <= lowTemperatureAlarm)
            {
                throw new ArgumentException(
                    "High Temperature Alarm setting must not be lower than or equal to the Low Temperature Alarm setting.",
                    nameof(highTemperatureAlarm));
            }

            if (lowTemperatureAlarm >= highTemperatureAlarm)
            {
                throw new ArgumentException(
                    "Low Temperature Alarm setting must not be higher than or equal to the High Temperature Alarm setting.",
                    nameof(lowTemperatureAlarm));
            }

            SByte[] scratchPad = ReadScratchPadSigned(oneWireAddress);
            scratchPad[HIGH_TEMP_ALARM_BIT] = highTemperatureAlarm;
            scratchPad[LOW_TEMP_ALARM_BIT] = lowTemperatureAlarm;

            return WriteScratchPad(oneWireAddress, scratchPad, writeToEeprom);
        }

        /// <summary>
        /// Sets both the low  and hight temperature alarm at once for all devices on the One-Wire Bus device in °C.
        /// </summary>
        /// <param name="lowTemperatureAlarm">Low Temperature Alarm set point. The valid range is -55°C - 125°C </param>
        /// <param name="highTemperatureAlarm">High Temperature Alarm set point. The valid range is -55°C - 125°C </param>
        /// <param name="writeToEeprom">If true, writes the new Low Temperature Alarm setting to NVRAM EEPROM.</param>
        /// <returns>True if successful or otherwise false.</returns>
        /// <exception cref="ArgumentException">Throws an ArgumentException if the lowTemperatureAlarm setting is higher than or equal to the <see cref="SetHighTempertureAlarm" /> method or if the highTemperatureAlarm setting is lower than or equal to the <see cref="SetLowTempertureAlarm" /> method.</exception>
        /// <remarks>All temperature alarms settings are in the <see cref="TemperatureUnits.Celsius" /> unit only.</remarks>
        /// <example>
        /// <code language="C#">
        ///     _thermostat.SetAlarmsForAlllDevic(20, 60, true);
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

            SByte[] scratchPad = new SByte[9];

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
            Interface.WriteByte(scratchPad[HIGH_TEMP_ALARM_BIT]); // high alarm temp
            Interface.WriteByte(scratchPad[LOW_TEMP_ALARM_BIT]); // low alarm temp
            Interface.WriteByte(scratchPad[CONFIGURATION_BIT]); // configuration

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
            Interface.WriteByte((Byte) FunctionCommands.RecallEe); // ToDo - Necessary?
            Byte[] scratchPad2 = ReadScratchPad(oneWireAddress);

            return scratchPad.GetHashCode() == scratchPad2.GetHashCode();
        }

        private Boolean WriteScratchPad(Byte[] oneWireAddress, SByte[] scratchPad, Boolean writeToEeprom = true)
        {
            GetDeviceList();

            if (!IsValidId(oneWireAddress)) return false;

            SelectDevice(oneWireAddress);

            Interface.WriteByte((Byte) FunctionCommands.WriteScratchPad);
#if (NANOFRAMEWORK_1_0)
            Interface.WriteByte((byte)scratchPad[HIGH_TEMP_ALARM_BIT]); // high alarm temp
            Interface.WriteByte((byte)scratchPad[LOW_TEMP_ALARM_BIT]); // low alarm temp
            Interface.WriteByte((byte)scratchPad[CONFIGURATION_BIT]); // configuration
#else
            Interface.WriteByte(scratchPad[HIGH_TEMP_ALARM_BIT]); // high alarm temp
            Interface.WriteByte(scratchPad[LOW_TEMP_ALARM_BIT]); // low alarm temp
            Interface.WriteByte(scratchPad[CONFIGURATION_BIT]); // configuration
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
                    return temperature*9/5 + 32;
                }
                case TemperatureUnits.Kelvin:
                {
                    return temperature + 273.15F;
                }
                default:
                {
                    return Single.MinValue;
                }
            }
        }

#endregion

#region Interface Implementations

        /// <inheritdoc cref="ITemperature" />
        /// <summary>
        /// Reads the temperature of a single DS18B20 Temperature Sensor configuration or the first device found on the 1-Wire Bus in a multiple sensor configuration.
        /// </summary>
        /// <param name="source">The temperature source <see cref="T:MBN.Enums.TemperatureSources" /> for more information.</param>
        /// <remarks>
        ///     Calling this method in a multi sensor Bus will only return the temperature of the first sensor found on the OneWire
        ///     Bus.
        ///     To read multiple sensors, use the <see cref="M:MBN.Modules.DS18B20.ReadTemperatureForAllDevices" /> method.
        /// </remarks>
        /// <returns>
        ///     A float representing the temperature read from the source in the °C.
        /// </returns>
        /// <exception cref="T:System.NotSupportedException">
        ///     Throws a NotSupportedException if the sensor being read is powered in
        ///     Parasitic Mode.
        /// </exception>
        /// <example>
        ///     Example usage:
        ///     <code language="C#">
        ///     Debug.WriteLine("ReadTemperature (deviceID) - " + _thermostat.ReadTemperature() + "°C"); // Where deviceID is the 64-Bit device identifier of your DS18B20.
        /// </code>
        /// </example>
        public Single ReadTemperature(TemperatureSources source = TemperatureSources.Ambient)
        {
            if (source == TemperatureSources.Object)
                throw new ArgumentException("TemperatureSources.Object not implemented for this module.");

            if (IsParasitic((Byte[]) DeviceList[0]))
                throw new NotSupportedException("Reading temperature in Parasitic Mode is not supported.");

#if (NANOFRAMEWORK_1_0)
            if (Interface.TouchReset() == false) return Single.MinValue;
#else
            if (Interface.TouchReset() <= 0) return Single.MinValue;
#endif

            Interface.WriteByte((Byte) RomCommands.SkipRom); // Skip ROM, we only have one device
            Interface.WriteByte((Byte) FunctionCommands.StartTemperatureConversion); // Start temperature conversion

            while (Interface.ReadByte() == 0) { }

            Interface.TouchReset();

            Interface.WriteByte((Byte) RomCommands.SkipRom);

            Byte[] device = (Byte[]) DeviceList[0];

            SelectDevice(device);

            Byte[] scratchPad = ReadScratchPad(device);

            Int32 temp = (scratchPad[TEMP_MSB_BIT] << 8) | scratchPad[TEMP_LSB_BIT];

            if ((scratchPad[1] & 0x80) != 0)
            {
                temp = (Int16) ((scratchPad[0] + scratchPad[1]) << 8);
            }

            Single temperature = 0;

            switch (scratchPad[CONFIGURATION_BIT])
            {
                case (Byte) Resolution.Resolution12Bit:
                {
                    temperature = (Single) (temp * 0.0625);
                    break;
                }
                case (Byte) Resolution.Resolution11Bit:
                {
                    temperature = (Single) ((temp >> 1) * 0.125);
                    break;
                }
                case (Byte) Resolution.Resolution10Bit:
                {
                    temperature = (Single) ((temp >> 2) * 0.25);
                    break;
                }
                case (Byte) Resolution.Resolution9Bit:
                {
                    temperature = (Single) ((temp >> 3) * 0.5);
                    break;
                }
            }

            return ConvertToScale(temperature);
        }

        /// <summary>
        ///     Gets the raw data of the temperature value.
        /// </summary>
        /// <value>
        ///     Raw data in the range depending on sensor's precision (8/10/12 bits, for example)
        /// </value>
        /// <remarks>Not Implemented.</remarks>
        /// <exception cref="T:System.NotImplementedException"></exception>
        public Int32 RawData => throw new NotImplementedException("RawData is not implemented for this module");

#endregion
    }
}