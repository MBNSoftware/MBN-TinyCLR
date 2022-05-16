/*
 * Altitude3 Click driver for TinyCLR
 * 
 * Version 1.0 :
 *  - Initial revision coded by Stephen Cardinale
 *  
 * Copyright 2020 Stephen Cardinale and MikroBus.Net
 *
 * Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License. You may obtain a copy of the License at
 * http:///www.apache.org/licenses/LICENSE-2.0
 * Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, 
 * either express or implied. See the License for the specific language governing permissions and limitations under the License.
 */

#region Usings

#if (NANOFRAMEWORK_1_0)
using System.Device.I2c;
#else
using GHIElectronics.TinyCLR.Devices.I2c;
#endif

using System;
using System.Threading;

#endregion

namespace MBN.Modules
{
    /// <inheritdoc cref="ITemperature" />
    /// <inheritdoc cref="IPressure" />
    /// <summary>
    /// Main class for the Altitude3 Click driver
    /// <para><b>Pins used :</b> Scl, Sda</para>
    /// <para><b>This is an I2C Module.</b></para>
    /// </summary>
    /// <example>
    /// Example usage:
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
    ///     internal static class Program
    ///     {
    ///         private static Altitude3Click _sensor;
    ///
    ///         private static void Main()
    ///         {
    ///             _sensor = new Altitude3Click(Hardware.SocketOne)
    ///             {
    ///                 TemperatureUnit = TemperatureUnits.Fahrenheit,
    ///                 PressureCompensation = PressureCompensationModes.SeaLevelCompensated,
    ///                 MeasurementMode = Altitude3Click.Mode.UltraLowNoise,
    ///             };
    ///
    ///             Debug.WriteLine("Device ID is " + _sensor.DeviceID);
    ///
    ///             while (true)
    ///             {
    ///                 _sensor.ReadSensor(out Single temperature, out Single pressure, out Single altitude);
    ///
    ///                 Debug.WriteLine("---------------------------------");
    ///                 Debug.WriteLine($"Temperature.......: {temperature:F2} �F");
    ///                 Debug.WriteLine($"Pressure..........: {pressure:F0} Pascals");
    ///                 Debug.WriteLine($"Altitude..........: {altitude:F0} meters");
    ///
    ///                 Thread.Sleep(1000);
    ///             }
    ///         }
    ///     }
    /// }
    /// </code>
    /// </example>
    public sealed class Altitude3Click : ITemperature, IPressure
    {
        #region .ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="Altitude3Click"/> class.
        /// </summary>
        /// <param name="socket">The <see cref="Hardware.Socket"/> that the Altitude3 Click is inserted into.</param>
        /// <exception cref="DeviceInitialisationException">A DeviceInitialisationException will be thrown if the Altitude3 Click does not complete its initialization properly.</exception>
        public Altitude3Click(Hardware.Socket socket, Int32 address = 0x63)
        {
            _socket = socket;
#if (NANOFRAMEWORK_1_0)
            _sensor = I2cDevice.Create(new I2cConnectionSettings(socket.I2cBus, address, I2cBusSpeed.StandardMode));
#else
            _sensor = I2cController.FromName(socket.I2cBus).GetDevice(new I2cConnectionSettings(address, 100000));
#endif

            Reset();

            if (DeviceID != 0x08)
                throw new DeviceInitialisationException(
                    "Altitude3 Click not found on the I2C Bus. Please check your hardware setup.");

            ReadCalibrationData();

            MeasurementMode = Mode.Normal;
            PressureCompensation = PressureCompensationModes.SeaLevelCompensated;
            TemperatureUnit = TemperatureUnits.Celsius;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Reads the temperature, pressure and altitude
        /// </summary>
        /// <param name="temperature">The referenced temperature parameter passed to this method.</param>
        /// <param name="pressure">The referenced pressure parameter passed to this method.</param>
        /// <param name="altitude">The referenced altitude parameter passed to this method.</param>
        /// <returns>Returns the temperature in the unit set in the <see cref="TemperatureUnit"/> property,
        /// pressure in Pascals reported as Uncompensated or Se Level Compensated as set in the
        /// <see cref="PressureCompensation"/> property and sea level compensated altitude in meters.</returns>
        /// <example>
        /// Example usage:
        /// <code language="C#">
        /// Single pressure;
        /// Single temperature;
        /// Single altitude;
        ///
        /// while (true)
        /// {
        ///     _sensor.ReadSensor(out temperature, out pressure, out altitude);
        ///
        ///     Debug.Print("---------------------------------");
        ///     Debug.Print("Temperature.......: " + temperature +
        ///                 (_sensor.TemperatureUnit == TemperatureUnits.Celsius ? " �C" :
        ///                    _sensor.TemperatureUnit == TemperatureUnits.Fahrenheit ? " �F" : " �K"));
        ///     Debug.Print("Pressure..........: " + pressure + " Pa");
        ///     Debug.Print("Altitude..........: " + altitude + " meters");
        ///
        ///     Thread.Sleep(1000);
        /// }
        /// </code>
        /// <code language="VB">
        /// Dim temperature As Single
        /// Dim pressure As Single
        /// Dim altitude As Single
        ///
        /// While True
        ///     _sensor.ReadSensor(temperature, pressure, altitude)
        ///     Debug.Print("---------------------------------")
        ///     Debug.Print(
        ///        "Temperature.......: " <![CDATA[&]]> temperature <![CDATA[&]]>
        ///         (If _
        ///             (_sensor.TemperatureUnit = TemperatureUnits.Celsius, " �C",
        ///             If(_sensor.TemperatureUnit = TemperatureUnits.Fahrenheit, " �F", " �K"))))
        ///     Debug.Print("Pressure..........: " <![CDATA[&]]> pressure <![CDATA[&]]> " Pa")
        ///     Debug.Print("Altitude..........: " <![CDATA[&]]> altitude <![CDATA[&]]> " meters")
        ///     Thread.Sleep(1000)
        /// End While
        /// </code>
        /// </example>
        public void ReadSensor(out Single temperature, out Single pressure, out Single altitude)
        {
            Byte[] command;
            Byte conversionTime;
            Int32 _raw_t;
            Int32 _raw_p;

            switch (MeasurementMode)
            {
                case Mode.LowPower:
                {
                    command = ICP10100_CMD_MEAS_LP_T_FIRST;
                    conversionTime = 3;
                    break;
                }
                case Mode.Normal:
                {
                    command = ICP10100_CMD_MEAS_N_T_FIRST;
                    conversionTime = 7;
                    break;
                }
                case Mode.LowNoise:
                {
                    command = ICP10100_CMD_MEAS_LN_T_FIRST;
                    conversionTime = 24;
                    break;
                }
                case Mode.UltraLowNoise:
                {
                    command = ICP10100_CMD_MEAS_ULN_T_FIRST;
                    conversionTime = 95;
                    break;
                }
                default:
                {
                    throw new ArgumentOutOfRangeException();
                }
            }

            Byte[] registerData = ReadRegister(command, conversionTime, 9);

            if (CalculateCRC8(new[] {registerData[0], registerData[1]}) != registerData[2])
            {
                _raw_t = Int32.MaxValue;
            }
            else
            {
                _raw_t = (registerData[0] << 8) | registerData[1];
            }

            if (CalculateCRC8(new[] {registerData[3], registerData[4]}) != registerData[5])
            {
                _raw_p = Int32.MaxValue;
            }
            else if (CalculateCRC8(new[] {registerData[6], registerData[7]}) != registerData[8])
            {
                _raw_p = Int32.MaxValue;
            }
            else
            {
                _raw_p = (registerData[3] << 16) | (registerData[4] << 8) | registerData[6];
            }

            // calculate and scale temperature
            temperature = ScaleTemperature(-45.0f + 175.0f / 65536.0f * _raw_t);

            // calculate pressure
            Single t = _raw_t - 32768F;
            Single s1 = _lut_lower + _calibrationData[0] * t * t * _quadr_factor;
            Single s2 = _offst_factor * _calibrationData[3] + _calibrationData[1] * t * t * _quadr_factor;
            Single s3 = _lut_upper + _calibrationData[2] * t * t * _quadr_factor;
            Single c = (s1 * s2 * (_pcal[0] - _pcal[1]) +
                        s2 * s3 * (_pcal[1] - _pcal[2]) +
                        s3 * s1 * (_pcal[2] - _pcal[0])) /
                       (s3 * (_pcal[0] - _pcal[1]) +
                        s1 * (_pcal[1] - _pcal[2]) +
                        s2 * (_pcal[2] - _pcal[0]));
            Single a = (_pcal[0] * s1 - _pcal[1] * s2 - (_pcal[1] - _pcal[0]) * c) / (s1 - s2);
            Single b = (_pcal[0] - a) * (s1 + c);
            Single rawPressure = a + b / (c + _raw_p);

            if (PressureCompensation == PressureCompensationModes.Uncompensated)
            {
                pressure = rawPressure;
            }
            else
            {
                pressure = (Single) CalculatePressureAsl(rawPressure);
            }

            Double seaLevelCompensation = 101325 * Math.Pow((288 - 0.0065 * 143) / 288, 5.256);
            Double tempValue = 101325 + rawPressure - seaLevelCompensation;
            altitude = (Single) (44330 * (1.0 - Math.Pow(tempValue / CalculatePressureAsl(tempValue), 0.1903)));
        }

        /// <summary>
        /// Resets the Altitude 3 Click and reloads the calibration data into OTP memory.
        /// </summary>
        /// <example>
        /// Example usage:
        /// <code language="C#">
        /// // Reset the Altitude 3 Click
        /// _sensor.Reset;
        /// </code>
        /// <code language="VB">
        /// ' Reset the Altitude 3 Click
        /// _sensor.Reset
        /// </code>
        /// </example>
        public Boolean Reset()
        {
            WriteComand(ICP10100_CMD_RESET);
            ReadCalibrationData();
            return true;
        }

        #endregion

        #region Public ENUMS

        /// <summary>
        /// One of four different measurement modes supported by the Altitude 3 click.
        /// </summary>
        public enum Mode
        {
            /// <summary>
            /// Low Power mode. Conversion time is 1.8 ms, current consumption is 1.3 �A and Pressure RMS noise is +/- 3.3 Pascals.
            /// </summary>
            LowPower,

            /// <summary>
            /// Normal Power mode. Conversion time is 6.3 ms, current consumption is 2.6 �A and Pressure RMS noise is +/- 1.6 Pascals.
            /// </summary>
            Normal,

            /// <summary>
            /// Low Noise mode. Conversion time is 23.8 ms, current consumption is 5.2 �A and Pressure RMS noise is +/- 0.8 Pascals.
            /// </summary>
            LowNoise,

            /// <summary>
            /// Ultra Low Noise mode. Conversion time is 94.5 ms, current consumption is 10.4 �A and Pressure RMS noise is +/- 0.4 Pascals.
            /// </summary>
            UltraLowNoise
        }

        #endregion

        #region Constants (sort of)

        // Constants used for pressure calculation
        private readonly Single[] _pcal = {45000.0F, 80000.0F, 105000.0F};
        private const Single _lut_lower = 3.5F * (1 << 20);
        private const Single _lut_upper = 11.5F * (1 << 20);
        private const Single _quadr_factor = 1F / 16777216.0F;
        private const Single _offst_factor = 2048.0F;

        // Register Addresses and Commands
        private readonly Byte[] ICP10100_CMD_READ_ID = {0xEF, 0xC8};
        private readonly Byte[] ICP10100_CMD_SET_ADDRESS = {0xC5, 0x95};
        private readonly Byte[] ICP10100_CMD_SET_OTP = {0x00, 0x66};
        private readonly Byte[] ICP10100_CMD_READ_OTP = {0xC7, 0xF7};
        private readonly Byte[] ICP10100_CMD_RESET = {0x80, 0x5D};

        // 16 bit register address for initiating a conversion
        private readonly Byte[] ICP10100_CMD_MEAS_LP_T_FIRST = {0x60, 0x9C};
        private readonly Byte[] ICP10100_CMD_MEAS_N_T_FIRST = {0x68, 0x25};
        private readonly Byte[] ICP10100_CMD_MEAS_LN_T_FIRST = {0x70, 0xDF};
        private readonly Byte[] ICP10100_CMD_MEAS_ULN_T_FIRST = {0x78, 0x66};

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets or sets the Measurement mode for the Altitude 3 click
        /// </summary>
        /// <example>
        /// Example usage:
        /// <code language="C#">
        /// // Set the measurement mode to low noise
        /// _sensor.MeasurementMode = Altitude3Click.Mode.LowNoise;
        /// </code>
        /// <code language="VB">
        /// ' Set the measurement mode to low noise
        /// _sensor.MeasurementMode = Altitude3Click.Mode.LowNoise
        /// </code>
        /// </example>
        public Mode MeasurementMode { get; set; }

        /// <summary>
        /// Gets or sets the compensation mode for pressure readings.
        /// </summary>
        /// <example>
        /// Example usage:
        /// <code language="C#">
        /// // Set the compensation mode to Sea Level compensated.
        /// _sensor.PressureCompensation = PressureCompensationModes.SeaLevelCompensated;
        /// </code>
        /// <code language="VB">
        /// ' Set the compensation mode to Sea Level compensated.
        /// _sensor.PressureCompensation = PressureCompensationModes.SeaLevelCompensated
        /// </code>
        /// </example>
        public PressureCompensationModes PressureCompensation { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="TemperatureUnits"/> that the temperature is reported in.
        /// </summary>
        /// <remarks><seealso cref="TemperatureUnits"/></remarks>
        /// <value>
        /// The temperature unit used.
        /// </value>
        /// <example>
        /// <code language="C#">
        /// // Set temperature unit to Fahrenheit
        /// _sensor.TemperatureUnit = TemperatureUnits.Farhenheit;
        /// </code>
        /// <code language="VB">
        /// ' Set temperature unit to Fahrenheit
        /// _sensor.TemperatureUnit = TemperatureUnits.Farhenheit
        /// </code>
        /// </example>
        public TemperatureUnits TemperatureUnit { get; set; }

        /// <summary>
        /// Reads the Device ID from the Altitude3 Click IC
        /// </summary>
        /// <example>
        /// <code language = "C#">
        /// Debug.Print("Device ID is 0x" + _sensor.DeviceID().ToString("x2"));
        /// </code>
        /// <code language = "VB">
        /// Debug.Print("Device ID is 0x" <![CDATA[&]]> _sensor.DeviceID().ToString("x2"))
        /// </code>
        /// </example>
        public UInt16 DeviceID
        {
            get
            {
                Byte[] registerData = ReadRegister(ICP10100_CMD_READ_ID, 0, 3);

                if (CalculateCRC8(new[] {registerData[0], registerData[1]}) != registerData[2])
                {
                    throw new ApplicationException("Error during reading DeviceID register.");
                }

                return (UInt16) (((registerData[0] << 8) | registerData[1]) & 0x3F);
            }
        }

        #endregion

        #region Interface Implementations

        /// <inheritdoc cref="IPressure" />
        /// <summary>
        /// Reads the pressure from the altitude3 Click.
        /// </summary>
        /// <param name="compensationMode">The <see cref="PressureCompensationModes"/> to report temperature measurements.</param>
        /// <exception cref="NotImplementedException">A NotImplementedException will be thrown if attempting to read the temperature as this driver does not implement direct reading of temperature. Use the ReadSensor method instead.</exception>
        /// <example>
        /// Example usage:
        /// <code language="C#">
        /// // None provided as this driver does not implement direct reading of pressure. Use the <exception cref="ReadSensor"> method instead.</exception>
        /// </code>
        /// <code language="VB">
        /// ' None provided as this driver does not implement direct reading of pressure. Use the <exception cref="ReadSensor"> method instead.</exception>
        /// </code>
        /// </example>
        public Single ReadPressure(PressureCompensationModes compensationMode = PressureCompensationModes.SeaLevelCompensated)
        {
            throw new NotImplementedException("Reading of pressure is not supported in this driver. Use ReadSensor method instead.");
        }

        /// <inheritdoc cref="ITemperature" />
        /// <summary>
        /// Reads the temperature from the altitude3 Click.
        /// </summary>
        /// <param name="source">The <see cref="TemperatureSources"/> of which to measure.</param>
        /// <exception cref="NotImplementedException">A NotImplementedException will be thrown if attempting to read the temperature as this driver does not implement direct reading of temperature. Use the ReadSensor method instead.</exception>
        /// <example>
        /// Example usage:
        /// <code language="C#">
        /// // None provided as this driver does not implement direct reading of temperature. Use the <exception cref="ReadSensor"> method instead.</exception>
        /// </code>
        /// <code language="VB">
        /// ' None provided as this driver does not implement direct reading of temperature. Use the <exception cref="ReadSensor"> method instead.</exception>
        /// </code>
        /// </example>
        public Single ReadTemperature(TemperatureSources source = TemperatureSources.Ambient)
        {
            throw new NotImplementedException("Reading of temperature is not supported in this driver. Use ReadSensor method instead.");
        }

        /// <inheritdoc cref="ITemperature" />
        /// <inheritdoc cref="IPressure" />
        /// <summary>
        /// Reads raw temperature or pressure form the Altitude3 Click.
        /// </summary>
        /// <exception cref="NotImplementedException">A NotImplementedException will e thrown if attempting to read RawData as this driver does not implement reading of RawData.</exception>
        /// <example>
        /// Example usage:
        /// <code language="C#">
        /// // None provided as this module does not support direct reading of Temperature or Pressure.
        /// </code>
        /// <code language="VB">
        /// ' None provided as this module does not support direct reading of Temperature or Pressure.
        /// </code>
        /// </example>
        public Int32 RawData => throw new NotImplementedException("Reading of Raw Data is not implemented in this driver.");

        #endregion

        #region Private Fields

        private readonly Single[] _calibrationData = new Single[4];
        private readonly I2cDevice _sensor;
        private readonly Hardware.Socket _socket;

        #endregion

        #region Private Methods

        private void ReadCalibrationData()
        {
            Byte[] otpCommand =
            {
                ICP10100_CMD_SET_ADDRESS[0],
                ICP10100_CMD_SET_ADDRESS[1],
                ICP10100_CMD_SET_OTP[0],
                ICP10100_CMD_SET_OTP[1],
                CalculateCRC8(ICP10100_CMD_SET_OTP)
            };

            WriteComand(otpCommand);

            for (Int32 i = 0; i < 4; i++)
            {
                Byte[] registerData = ReadRegister(ICP10100_CMD_READ_OTP, 0, 3);

                if (CalculateCRC8(new[] {registerData[0], registerData[1]}) != registerData[2])
                    throw new DeviceInitialisationException("Error during reading OTP memory calibration data.");

                _calibrationData[i] = (registerData[0] << 8) | registerData[1];
            }
        }

        // Tested with 0xBEEF = 0x92 per data sheet.
        private Byte CalculateCRC8(Byte[] data)
        {
            const Int16 polynomial = 0x131; // P(x) = 2^8 + 2^5 + 2^4 + 1
            Byte crc = 0xFF; // Initialize the CRC to 0xFF

            // Iterate through each byte in the passed data
            for (Byte byteCounter = 0; byteCounter < data.Length; byteCounter++)
            {
                crc ^= data[byteCounter];

                for (Byte bit = 0; bit <= 7; bit++) // Once for each bit in the Byte
                {
                    crc = (crc & 0x80) == 0x80 ? (Byte) ((crc << 1) ^ polynomial) : (Byte) (crc << 1);
                }
            }

            return crc;
        }

        private Byte[] ReadRegister(Byte[] command, Byte readDelay, Byte bytesToRead)
        {
            Byte[] readBuffer = new Byte[bytesToRead];

            lock (_socket.LockI2c)
            {
                _sensor.Write(command);

                Thread.Sleep(readDelay);

                _sensor.Read(readBuffer);
            }

            return readBuffer;
        }

        private void WriteComand(Byte[] command)
        {
            lock (_socket.LockI2c)
            {
                _sensor.Write(command);
            }
        }

        private Double CalculatePressureAsl(Double pressure)
        {
            Double seaLevelCompensation = 101325 * Math.Pow((288 - 0.0065 * 143) / 288, 5.256);
            return 101325 + pressure - seaLevelCompensation;
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

        #endregion
    }
}