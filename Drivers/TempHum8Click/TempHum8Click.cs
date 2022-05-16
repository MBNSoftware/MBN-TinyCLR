/*
 * Temp&Hum 8 Click driver for TinyCLR 2.0
 * 
 * Initial version coded by Stephen Cardinale
 * 
 * References needed :
 * MBN;
 * MBN.Enums;
 * Meadow.Hardware;
 * System;
 *  
 * Copyright 2020 Stephen Cardinale and MikroBUS.Net
 * Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License. You may obtain a copy of the License at
 * http://www.apache.org/licenses/LICENSE-2.0
 * Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, 
 * either express or implied. See the License for the specific language governing permissions and limitations under the License.
 */

#if (NANOFRAMEWORK_1_0)
using System.Device.I2c;
#else
using GHIElectronics.TinyCLR.Devices.I2c;
#endif

using System;
using System.Threading;

namespace MBN.Modules
{
    /// <summary>
    /// Main class for the DCMotor Click board driver
    /// <para><b>Pins used :</b> An, Rst, Cs, Pwm, Int</para>
    /// </summary>
    /// <example>
    /// <code language = "C#">
    /// using MBN;
    /// using MBN.Modules;
    ///
    /// using System.Diagnostics;
    /// using System.Threading;
    ///
    /// namespace Examples
    /// {
    ///     public class Program
    ///     {
    ///         private static TempHum8Click _sensor;
    ///
    ///         public static void Main()
    ///         {
    ///             _sensor = new TempHum8Click(Hardware.SocketOne);
    ///
    ///             Debug.WriteLine($"Heater enabled ? {_sensor.HeaterState}");
    ///             _sensor.HeaterState = true;
    ///             Debug.WriteLine($"Heater enabled ? {_sensor.HeaterState}");
    ///             _sensor.HeaterState = false;
    ///             Debug.WriteLine($"Heater enabled ? {_sensor.HeaterState}");
    ///
    ///             Debug.WriteLine($"Disable OTP Reload ? {_sensor.DisableOTPReload}");
    ///             _sensor.DisableOTPReload = false;
    ///             Debug.WriteLine($"Disable OTP Reload ? {_sensor.DisableOTPReload}");
    ///             _sensor.DisableOTPReload = true;
    ///             Debug.WriteLine($"Disable OTP Reload ? {_sensor.DisableOTPReload}");
    ///
    ///             Debug.WriteLine($"End Of Battery ? {_sensor.EndOfBattery}");
    ///
    ///             Debug.WriteLine($"Resolution is {_sensor.Resolution}");
    ///             _sensor.Resolution = TempHum8Click.SensorResolution.Resolution_11RH_11T;
    ///             Debug.WriteLine($"Resolution is {_sensor.Resolution}");
    ///
    ///             _sensor.ClockStretch = false;
    ///             _sensor.DisableOTPReload = false;
    ///             _sensor.TemperatureUnit = TemperatureUnits.Fahrenheit;
    ///
    ///
    ///             while (true)
    ///             {
    ///                 Debug.WriteLine($"Temperature is {_sensor.ReadTemperature():F1} �F");
    ///                 Debug.WriteLine($"   Humidity is {_sensor.ReadHumidity():F1} %RH\n");
    ///                 Thread.Sleep(1000);
    ///             }
    ///         }
    ///     }
    /// }
    /// </code>
    /// </example>
    public sealed class TempHum8Click : ITemperature, IHumidity
    {
        #region .ctor

        /// <summary>
        ///  A TinyCLR driver for the MikroE TempHum 8 Click.
        /// </summary>
        /// <param name="socket">
        ///     The socket that TempHum 8 Click is plugged into.
        /// </param>
        public TempHum8Click(Hardware.Socket socket)
        {
            _socket = socket;
#if (NANOFRAMEWORK_1_0)
            _sensor = I2cDevice.Create(new I2cConnectionSettings(socket.I2cBus, 0x40, I2cBusSpeed.StandardMode));
#else
			_sensor = I2cController.FromName(socket.I2cBus).GetDevice(new I2cConnectionSettings(0x40, 100000));
#endif

            Reset();

            Resolution = SensorResolution.Resolution_12RH_14T;
        }

        #endregion

        #region Private Fields

        private I2cDevice _sensor;
        private SensorResolution _resolution;
        private readonly Hardware.Socket _socket;

        #endregion

        #region Private Methods

        private void WriteByte(Byte value)
        {
            lock (_socket.LockI2c)
            {
                _sensor.Write(new[] { value });
            }
        }

        private void WriteRegister(Byte registerAddress, Byte value)
        {
            lock (_socket.LockI2c)
            {
                Byte[] writeBuffer = {registerAddress, value};
                _sensor.Write(writeBuffer);
            }
        }

        private Byte[] ReadRegister(Byte registerAddress, Byte numberOfBytesToRead)
        {
            Byte[] readBuffer = new Byte[numberOfBytesToRead];

            lock (_socket.LockI2c)
            {
                _sensor.WriteRead(new[] { registerAddress }, readBuffer);
            }

            return readBuffer;
        }

        private Byte[] ReadBytes(Byte numberOfBytesToRead)
        {
            Byte[] readBuffer = new Byte[numberOfBytesToRead];

            lock (_socket.LockI2c)
            {
                _sensor.Read(readBuffer);
            }

            return readBuffer;
        }

        private Single ScaleTemperature(Single temperature)
        {
            switch (TemperatureUnit)
            {
                case TemperatureUnits.Celsius:
                {
                    return temperature;
                }

                case TemperatureUnits.Fahrenheit:
                {
                    return temperature * 1.8F + 32F;
                }

                case TemperatureUnits.Kelvin:
                {
                    return temperature + 273.15F;
                }

                default:
                {
                    return temperature;
                }
            }
        }

        private void MeasureDelay(SensorResolution resolution)
        {
            Int32 delay;

            switch (resolution)
            {
                case SensorResolution.Resolution_10RH_13T:
                {
                    delay = 52;
                    break;
                }

                case SensorResolution.Resolution_11RH_11T:
                {
                    delay = 26;
                    break;
                }

                case SensorResolution.Resolution_8RH_12T:
                {
                    delay = 26;
                    break;
                }

                case SensorResolution.Resolution_12RH_14T:
                {
                    delay = 114;
                    break;
                }

                default:
                {
                    delay = 114;
                    break;
                }
            }

            Thread.Sleep(delay);
        }

        #endregion

        #region Constants

        private const Byte SHT_REG_CMD_RESET = 0xFE;
        private const Byte SHT_REG_CMD_TRIGGER_T_MEAS_HOLD = 0xE3;
        private const Byte SHT_REG_CMD_TRIGGER_RH_MEAS_HOLD = 0xE5;
        private const Byte SHT_REG_CMD_TRIGGER_T_MEAS_NOHOLD = 0xF3;
        private const Byte SHT_REG_CMD_TRIGGER_RH_MEAS_NOHOLD = 0xF5;
        private const Byte SHT_REG_CMD_WRITE_USER_REG = 0xE6;
        private const Byte SHT_REG_CMD_READ_USER_REG = 0xE7;

        #endregion

        #region Public ENUMS

        /// <summary>
        /// Possible resolution combinations that the TempHum 8 supports.
        /// </summary>
        public enum SensorResolution
        {
            /// <summary>
            /// Humidity resolution is 12 bit and Temperture resolution is 14 bit.
            /// This is the POR/Reset setting.
            /// </summary>
            Resolution_12RH_14T = 0x00,

            /// <summary>
            /// Humidity resolution is 8 bit and Temperture resolution is 12 bit
            /// </summary>
            Resolution_8RH_12T = 0x01,

            /// <summary>
            /// Humidity resolution is 10 bit and Temperture resolution is 13 bit
            /// </summary>
            Resolution_10RH_13T = 0x02,

            /// <summary>
            /// Humidity resolution is 12 bit and Temperture resolution is 14 bit
            /// </summary>
            Resolution_11RH_11T = 0x03
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// When enabled the SHTC-21 on the TempHum8 Click will pull the SCL line low throughout any measurement.
        /// When not enabled, the driver uses a polling method by waiting the prescribed time between Writes and Reads on the I2C Bus.
        /// </summary>
        /// <example>
        /// <code language = "C#">
        /// _sensor.ClockStretch = true;
        /// </code>
        /// </example>
        public Boolean ClockStretch { get; set; }

        /// <summary>
        /// ReadOnly property which contains the status of the Battery or Voltage to the 3.3VDC Line.
        /// </summary>
        /// <returns>False is the voltage level is greater than 2.4 VDC or true if bvoltage is less than 2.4 VDC.</returns>
        /// <remarks>Not very practical if we are running on a Quail MainBoard.</remarks>
        public Boolean EndOfBattery
        {
            get
            {
                Byte registerData = ReadRegister(SHT_REG_CMD_READ_USER_REG, 1)[0];
                return Bits.IsBitSet(registerData, 6);
            }
        }

        /// <summary>
        /// Gets or sets the resolution for measuring Temperature and Humidity. Default value is <see cref="SensorResolution.Resolution_12RH_14T"/>
        /// </summary>
        public SensorResolution Resolution
        {
            get => _resolution;
            set
            {
                Byte registerData = ReadRegister(SHT_REG_CMD_READ_USER_REG, 1)[0];

                switch (value)
                {
                    case SensorResolution.Resolution_8RH_12T:
                    {
                        Bits.Set(ref registerData, "0xxxxxx1");
                        break;
                    }

                    case SensorResolution.Resolution_10RH_13T:
                    {
                        Bits.Set(ref registerData, "1xxxxxx0");
                        break;
                    }

                    case SensorResolution.Resolution_11RH_11T:
                    {
                        Bits.Set(ref registerData, "1xxxxxx1");
                        break;
                    }

                    case SensorResolution.Resolution_12RH_14T:
                    {
                        Bits.Set(ref registerData, "0xxxxxx0");
                        break;
                    }

                    default: //SensorResolution.Resolution_12RH_14T:
                    {
                        Bits.Set(ref registerData, "0xxxxxx0");
                        break;
                    }
                }

                WriteRegister(SHT_REG_CMD_WRITE_USER_REG, registerData);
                _resolution = value;
            }
        }

        /// <summary>
        /// Enables or disables the on board heater of the IC.
        /// </summary>
        public Boolean Heater
        {
            get
            {
                Byte registerData = ReadRegister(SHT_REG_CMD_READ_USER_REG, 1)[0];
                return Bits.IsBitSet(registerData, 2);
            }
            set
            {
                Byte registerData = ReadRegister(SHT_REG_CMD_READ_USER_REG, 1)[0];
                Bits.Set(ref registerData, 2, value);
                WriteRegister(SHT_REG_CMD_WRITE_USER_REG, registerData);
            }
        }

        /// <summary>
        /// Sets or Gets the value of the DisableOTPReload setting in the User Configuration.
        /// If set to false, the OTP Memory is refreshed after every meaurement. If set to true, the OTP is not refreshed for each measurement.
        /// </summary>
        public Boolean DisableOTPReload
        {
            get
            {
                Byte registerData = ReadRegister(SHT_REG_CMD_READ_USER_REG, 1)[0];
                return Bits.IsBitSet(registerData, 1);
            }
            set
            {
                Byte registerData = ReadRegister(SHT_REG_CMD_READ_USER_REG, 1)[0];
                Bits.Set(ref registerData, 1, value);
                WriteRegister(SHT_REG_CMD_WRITE_USER_REG, registerData);
            }
        }

        /// <summary>
        /// The unit of which to return temperature readings in.
        /// </summary>
        public TemperatureUnits TemperatureUnit { get; set; } = TemperatureUnits.Celsius;

        #endregion

        #region Public Methods

        /// <summary>
        /// Performs a soft resets the module
        /// </summary>
        /// <code language = "C#">
        /// _sensor.Reset();
        /// </code>
        /// <exception cref="T:System.NotSupportedException">A NotSupportedException will be thrown as this module does not support a hard reset mechanism.</exception>
        public void Reset()
        {
            WriteByte(SHT_REG_CMD_RESET);
            Thread.Sleep(20); // Time between ACK of soft reset command and sensor entering the idle state. Typically 15 mSec.
        }

        #endregion

        #region Interface Implementation

        /// <inheritdoc />
        /// <summary>
        /// Reads the temperature in �C.
        /// </summary>
        /// <param name="source">The temperature source to read. In this case only <see cref="TemperatureSources.Ambient"/> is supported.</param>
        /// <returns>A <see cref="System.Single"/> representing the ambient temperature.</returns>
        /// <exception cref="NotSupportedException">A <see cref="NotSupportedException"/> will be thrown if attempting to read Object temperature as this module does not support reading of object temperature.</exception>
        /// <remarks>Reading temperature while in low power mode is automatically handled by this method. The SHTC3 is woken up and returned to sleep after the temperature is measured.</remarks>
        /// <example>
        /// <code language = "C#">
        /// Debug.Print("Temperature...............: " + _sensor.ReadTemperature());
        /// </code>
        /// </example>
        public Single ReadTemperature(TemperatureSources source = TemperatureSources.Ambient)
        {
            if (source == TemperatureSources.Object)
                throw new NotSupportedException(
                    "This module does not support reading Object temperature. Please use TemperatureSources.Ambient instead.");
            Int32 result = (this as ITemperature).RawData;
            result &= 0xFFFC; // clear bits [1..0] (status bits)
            return ScaleTemperature(-46.85f + 175.72f / 65536f * result);
        }

        /// <inheritdoc />
        /// <summary>
        /// Reads the humidity as %RH.
        /// </summary>
        /// <param name="measurementMode">The measurement mode to read. In this case only <see cref="HumidityMeasurementModes.Relative"/> is supported.</param>
        /// <returns>A <see cref="System.Single"/> representing the relative humidity.</returns>
        /// <exception cref="NotSupportedException">A <see cref="NotSupportedException"/> will be thrown if attempting to read absolute humidity as this module does not support reading of absolute humidity.</exception>
        /// <remarks>Reading the relative humidity while in low power mode is automatically handled by this method. The SHTC3 is woken up and returned to sleep after the humidity is measured.</remarks>
        /// <example>
        /// <code language = "C#">
        /// Debug.Print("Humidity..................: " + _sensor.ReadHumidity());
        /// </code>
        /// </example>
        public Single ReadHumidity(HumidityMeasurementModes measurementMode = HumidityMeasurementModes.Relative)
        {
            if (measurementMode == HumidityMeasurementModes.Absolute)
                throw new NotSupportedException("This module does not support reading absolute humidity.");
            Int32 result = (this as IHumidity).RawData;
            result &= 0xFFFC; // clear bits [1..0] (status bits)
            return -6.0F + 125.0F / 65536F * result;
        }

        /// <inheritdoc />
        /// <summary>Gets the raw data of the humidity value.</summary>
        /// <value>
        /// Raw data in the range depending on sensor's precision (8/10/12 bits, for example)
        /// </value>
        /// <example>
        /// <code language = "C#">
        /// Debug.Print("Humidity RawData is... : " + (_sensor as IHumidity).RawData);
        /// </code>
        /// </example>
        Int32 IHumidity.RawData
        {
            get
            {
                Byte[] registerData = new Byte[3];

                switch (ClockStretch)
                {
                    case true:
                    {
                        registerData = ReadRegister(SHT_REG_CMD_TRIGGER_RH_MEAS_HOLD, 3);
                        break;
                    }

                    case false:
                    {
                        WriteByte(SHT_REG_CMD_TRIGGER_RH_MEAS_NOHOLD);
                        MeasureDelay(Resolution);
                        registerData = ReadBytes(3);
                        break;
                    }
                }

                return (registerData[0] << 8) | registerData[1];
            }
        }

        /// <inheritdoc />
        /// <summary>Gets the raw data of the temperature value.</summary>
        /// <value>
        /// Raw data in the range depending on sensor's precision (8/10/12 bits, for example)
        /// </value>
        /// <example>
        /// <code language = "C#">
        /// Debug.Print("Temperature RawData is... : " + (_sensor as ITemperature).RawData);
        /// </code>
        /// </example>
        Int32 ITemperature.RawData
        {
            get
            {
                Byte[] registerData = new Byte[3];

                switch (ClockStretch)
                {
                    case true:
                    {
                        registerData = ReadRegister(SHT_REG_CMD_TRIGGER_T_MEAS_HOLD, 3);
                        break;
                    }

                    case false:
                    {
                        WriteByte(SHT_REG_CMD_TRIGGER_T_MEAS_NOHOLD);
                        MeasureDelay(Resolution);
                        registerData = ReadBytes(3);
                        break;
                    }
                }

                return (registerData[0] << 8) | registerData[1];
            }
        }

        #endregion
    }
}