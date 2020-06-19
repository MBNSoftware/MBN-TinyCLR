/*
 * Altitude 2 Click TinyCLR driver for MikroBUS.Net
 * 
 * Initial revision coded by Stephen Cardinale
 * 
 * Copyright 2020 Stephen Cardinale and MikroBUS.Net
 *
 * Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License. You may obtain a copy of the License at
 * http://www.apache.org/licenses/LICENSE-2.0
 * Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, 
 * either express or implied. See the License for the specific language governing permissions and limitations under the License.
 * 
 */

#region Usings

using GHIElectronics.TinyCLR.Devices.I2c;

using System;
using System.Threading;

using Math = System.Math;

#endregion

namespace MBN.Modules
{
    /// <inheritdoc cref="IPressure"/>
    /// <inheritdoc cref="ITemperature"/>
    /// <summary>
    ///     Main class for the Altitude2Click driver
    ///     <para><b>Pins used :</b> Scl, Sda</para>
    ///     <para><b>This is an I2C modue.</b></para>
    /// </summary>
    /// <example>
    /// <code language = "C#">
    /// #region Usings
    ///
    /// using MBN;
    /// using MBN.Modules;
    ///
    /// using System;
    /// using System.Diagnostics;
    /// using System.Threading;
    /// 
    /// #endregion
    ///
    /// namespace Examples
    /// {
    ///     public static class Program
    ///     {
    ///         private static Altitude2Click _sensor;
    ///         private static Single temperature;
    ///         private static Single pressure;
    ///         private static Single altitude;
    ///
    ///         public static void Main()
    ///         {
    ///             _sensor = new Altitude2Click(Hardware.SocketOne, Altitude2Click.I2CAddress.AddressTwo, ClockRatesI2C.Clock100KHz, 1000)
    ///             {
    ///                 TemperatureUnit = TemperatureUnits.Celsius,
    ///                 PressureCompensationMode = PressureCompensationModes.Uncompensated,
    ///                 PressureOverSamplingRate = Altitude2Click.OverSamplingRate.ADC4096,
    ///                 TemperatureOverSamplingRate = Altitude2Click.OverSamplingRate.ADC4096
    ///             };
    ///
    ///             for (;;)
    ///             {
    ///                 _sensor.ReadSensor(out temperature, out pressure, out altitude);
    ///                 Debug.WriteLine("Temperature.........: " + temperature.ToString("F2") + " °C");
    ///                 Debug.WriteLine("Pressure............: " + pressure.ToString("F2") + " Pa");
    ///                 Debug.WriteLine("Altitude............: " + altitude.ToString("F2") + " meters");
    ///                 Debug.WriteLine("-----------------------------------");
    ///                 Thread.Sleep(2000);
    ///             }
    ///         }
    ///     }
    /// }
    /// </code>
    /// </example>
    public sealed class Altitude2Click : IPressure, ITemperature
    {
        #region .ctor

        /// <summary>
        ///     Initializes a new instance of the <see cref="Altitude2Click" /> class.
        /// </summary>
        /// <param name="socket">The socket on which the Altitude2Click module is plugged on MikroBus.Net board</param>
        /// <param name="address">The slave address of the module.</param>
        public Altitude2Click(Hardware.Socket socket, I2CAddress address)
        {
            _sensor = I2cController.FromName(socket.I2cBus).GetDevice(new I2cConnectionSettings((Int32) address, 100000));

            Reset();

            ReadCalibrationData();

            PressureOverSamplingRate = OverSamplingRate.ADC256;
            TemperatureOverSamplingRate = OverSamplingRate.ADC256;
            TemperatureUnit = TemperatureUnits.Celsius;
            PressureCompensationMode = PressureCompensationModes.SeaLevelCompensated;
        }

        #endregion

        #region Private Fields

        private readonly I2cDevice _sensor;
        private readonly UInt32[] CalibrationData = new UInt32[8];

        #endregion

        #region Public ENUMS

        /// <summary>
        ///     Possible I2C Slave Addresses for the Altitude 2 Click
        /// </summary>
        public enum I2CAddress
        {
            /// <summary>
            ///     Address is 0x76 with I2C Address Jumper soldered in the 1 position.
            /// </summary>
            AddressOne = 0x76,

            /// <summary>
            ///     Address is 0x77 with I2C Address Jumper soldered in the 0 position.
            /// </summary>
            AddressTwo = 0x77
        }

        /// <summary>
        /// Resolution RMS
        /// </summary>
        public enum OverSamplingRate
        {
            /// <summary>
            /// OSR is 256 samples with a Pressure resolution of 13.00 Pa, Temperature resolution of 0.012 °C and a Conversion time of .06 milliseconds.
            /// </summary>
            ADC256 = MS5607_CMD_ADC_256,

            /// <summary>
            /// OSR is 512 samples with a Pressure resolution of 8.40 Pa, Temperature resolution of 0.008 °C and a Conversion time of 1.17 milliseconds.
            /// </summary>
            ADC512 = MS5607_CMD_ADC_512,

            /// <summary>
            /// OSR is 1024 samples with a Pressure resolution of 5.40 PA, Temperature resolution of 0.005 °C and a Conversion time of 2.28 milliseconds.
            /// </summary>
            ADC1024 = MS5607_CMD_ADC_1024,

            /// <summary>
            /// OSR is 2048 samples with a Pressure resolution of 3.60 Pa, Temperature resolution of 0.003 °C and a Conversion time of 4.54 milliseconds.
            /// </summary>
            ADC2048 = MS5607_CMD_ADC_2048,

            /// <summary>
            /// OSR is 4096 samples with a Pressure resolution of 2.40 Pa, Temperature resolution of 0.002 °C and a Conversion time of 9.04 milliseconds.
            /// </summary>
            ADC4096 = MS5607_CMD_ADC_4096
        }

        #endregion

        #region Constants

        private const Byte MS5607_CMD_RESET = 0x1E; // Reset
        private const Byte MS5607_CMD_ADC_READ = 0x00; // Initiate read sequence
        private const Byte MS5607_CMD_ADC_CONV = 0x40; // Start conversion
        private const Byte MS5607_CMD_ADC_D1 = 0x00; // Read ADC 1
        private const Byte MS5607_CMD_ADC_D2 = 0x10; // Read ADC 2
        private const Byte MS5607_CMD_ADC_256 = 0x00; // ADC oversampling ratio to 256
        private const Byte MS5607_CMD_ADC_512 = 0x02; // ADC oversampling ratio to 512
        private const Byte MS5607_CMD_ADC_1024 = 0x04; // ADC oversampling ratio to 1024
        private const Byte MS5607_CMD_ADC_2048 = 0x06; // ADC oversampling ratio to 2048
        private const Byte MS5607_CMD_ADC_4096 = 0x08; // ADC oversampling ratio to 4096
        private const Byte MS5607_CMD_PROM_RD = 0xA0; // Read PROM registers

        #endregion

        #region Public Properties

        /// <summary>
        /// Sets or gets the oversampling rate or resolution for pressure conversion.
        /// </summary>
        /// <example>
        /// <code language = "C#">
        /// _sensor.PressureOverSamplingRate = Altitude2Click.OverSamplingRate.ADC2048;
        /// </code>
        /// <code language = "VB">
        /// _sensor.PressureOverSamplingRate = Altitude2Click.OverSamplingRate.ADC2048
        /// </code>
        /// </example>
        public OverSamplingRate PressureOverSamplingRate { get; set; }

        /// <summary>
        /// Sets or gets the oversampling rate or resolution for temperature conversion.
        /// </summary>
        /// <example>
        /// <code language = "C#">
        /// _sensor.TemperatureOverSamplingRate = Altitude2Click.OverSamplingRate.ADC2048;
        /// </code>
        /// <code language = "VB">
        /// _sensor.TemperatureOverSamplingRate = Altitude2Click.OverSamplingRate.ADC2048
        /// </code>
        /// </example>
        public OverSamplingRate TemperatureOverSamplingRate { get; set; }

        /// <summary>
        /// Seltects between <see cref="PressureCompensationModes.Uncompensated"/> or <see cref="PressureCompensationModes.SeaLevelCompensated"/>.
        /// </summary>
        /// <example>
        /// <code language = "C#">
        /// _sensor.PressureCompensationMode = PressureCompensationModes.SeaLevelCompensated;
        /// </code>
        /// <code language = "VB">
        /// _sensor.PressureCompensationMode = PressureCompensationModes.SeaLevelCompensated
        /// </code>
        /// </example>
        public PressureCompensationModes PressureCompensationMode { get; set; }

        /// <summary>
        /// The <see cref="TemperatureUnits"/> in which to report temperature.
        /// </summary>
        /// <example>
        /// <code language = "C#">
        /// _sensor.TemperatureUnit = TemperatureUnits.Kelvin;
        /// </code>
        /// <code language = "VB">
        ///  _sensor.TemperatureUnit = TemperatureUnits.Kelvin
        /// </code>
        /// </example>
        public TemperatureUnits TemperatureUnit { get; set; }

        #endregion

        #region Private Methods

        private void WriteByte(Byte registerAddress)
        {
            lock (Hardware.LockI2C)
            {
                _sensor.Write(new[] { registerAddress });
            }
        }

        private Byte[] ReadRegister(Byte registerAddress, Byte numberOfBytesToRead)
        {
            Byte[] readBuffer = new Byte[numberOfBytesToRead];

            lock(Hardware.LockI2C)
            {
                _sensor.WriteRead(new[] { registerAddress}, readBuffer );
            }

            return readBuffer;
        }

        private void ReadCalibrationData()
        {
            for (Byte i = 0; i < 8; i++)
            {
                CalibrationData[i] = ReadPROM(i);
            }
        }

        private UInt16 ReadPROM(Byte coefficientNumber)
        {
            Byte[] registerData = ReadRegister((Byte) (MS5607_CMD_PROM_RD + coefficientNumber * 2), 2);
            return (UInt16) ((registerData[0] << 8) | registerData[1]);
        }

        private UInt32 ReadADC(Byte command, OverSamplingRate oss)
        {
            WriteByte((Byte) (MS5607_CMD_ADC_CONV | command | (Byte) oss));

            switch (oss)
            {
                case OverSamplingRate.ADC256:
                {
                    Thread.Sleep(1);
                    break;
                }
                case OverSamplingRate.ADC512:
                {
                    Thread.Sleep(3);
                    break;
                }
                case OverSamplingRate.ADC1024:
                {
                    Thread.Sleep(4);
                    break;
                }
                case OverSamplingRate.ADC2048:
                {
                    Thread.Sleep(6);
                    break;
                }
                case OverSamplingRate.ADC4096:
                {
                    Thread.Sleep(10);
                    break;
                }
                default:
                {
                    throw new ArgumentException("command");
                }
            }

            Byte[] registerData = ReadRegister(MS5607_CMD_ADC_READ, 3);

            UInt32 value = (UInt32) ((registerData[0] << 16) | (registerData[1] << 8) | registerData[2]);
            return value;
        }

        private static Single CalculatePressureAsl(Single uncompensatedPressure)
        {
            Single seaLevelCompensation = (Single) (101325 * Math.Pow((288 - 0.0065 * 143) / 288, 5.256));
            return 101325 + uncompensatedPressure - seaLevelCompensation;
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

        #region Public Methods

        /// <summary>
        /// Trigger a pressure and temperature conversion.
        /// </summary>
        /// <param name="temperature">The referenced temperature parameter is reported based upon user selected <see cref="TemperatureUnit"/> property.</param>
        /// <param name="pressure">The referenced pressure parameter is reported in Pascal units (1 Pa = 0.01 mBar or 1HectoPascals) either as <see cref="PressureCompensationModes.SeaLevelCompensated"/> or <see cref="PressureCompensationModes.Uncompensated"/>.</param>
        /// <param name="altitude">The referenced  altitude parameter is reported in meters.</param>
        /// <remarks>The altitude reading is a calculated value, based on well established mathematical formulas.</remarks>
        /// <example>Example usage:
        /// <code language = "C#">
        /// for (;;)
        /// {
        ///     _sensor.ReadSensor(out temperature, out pressure, out altitude);
        ///     Debug.Print("Temperature.........: " + temperature.ToString("F2") + " °C");
        ///     Debug.Print("Pressure............: " + pressure.ToString("F2") + " Pascals");
        ///     Debug.Print("Altitude............: " + altitude.ToString("F2") + " meters");
        ///     Debug.Print("-----------------------------------");
        ///     Thread.Sleep(2000);
        /// }
        /// </code>
        /// <code language = "VB">
        /// While True
        ///     _sensor.ReadSensor(temperature, pressure, altitude)
        ///     Debug.Print("Temperature.........: " <![CDATA[&]]> temperature.ToString("F2") <![CDATA[&]]> " °C")
        ///     Debug.Print("Pressure............: " <![CDATA[&]]> pressure.ToString("F2") <![CDATA[&]]> " Pascals")
        ///     Debug.Print("Altitude............: " <![CDATA[&]]> altitude.ToString("F2") <![CDATA[&]]> " meters")
        ///     Debug.Print("-----------------------------------")
        ///     Thread.Sleep(2000)
        /// End While
        /// </code>
        /// </example>
        public void ReadSensor(out Single temperature, out Single pressure, out Single altitude)
        {
            Single D2 = ReadADC(MS5607_CMD_ADC_D2, PressureOverSamplingRate);
            Single D1 = ReadADC(MS5607_CMD_ADC_D1, TemperatureOverSamplingRate);

            // Calcualte 1st order pressure and temperature compensation
            Single dT = (Single) (D2 - CalibrationData[5] * Math.Pow(2, 8));
            Single OFF = (Single) (CalibrationData[2] * Math.Pow(2, 17) + dT * CalibrationData[4] / Math.Pow(2, 6));
            Single SENS = (Single) (CalibrationData[1] * Math.Pow(2, 16) + dT * CalibrationData[3] / Math.Pow(2, 7));

            temperature = (Single) (2000 + dT * CalibrationData[6] / Math.Pow(2, 23)) / 100;
            pressure = (Single) ((D1 * SENS / Math.Pow(2, 21) - OFF) / Math.Pow(2, 15));
            altitude = (Single) (44330.77 * (1.0 - Math.Pow(pressure / CalculatePressureAsl(pressure), 0.1902663538687809)));

            if (PressureCompensationMode == PressureCompensationModes.SeaLevelCompensated)
            {
                pressure = CalculatePressureAsl(pressure);
            }

            if (!(temperature * 100 < 2000))
            {
                temperature = ScaleTemperature(temperature);
                return;
            }

            // Calculate 2nd order pressure and temperature compensation
            Single T2 = (Single) (dT * dT / Math.Pow(2, 31));
            Single OFF2 = (Single) (61 * (temperature - 2000) * (temperature - 2000) / Math.Pow(2, 4));
            Single SENS2 = 2 * (temperature - 2000) * (temperature - 2000);

            if (temperature < -1500)
            {
                OFF2 += 15 * (temperature + 1500) * (temperature + 1500);
                SENS2 += 8 * (temperature + 1500) * (temperature + 1500);
            }

            OFF -= OFF2;
            SENS -= SENS2;

            temperature -= T2;
            temperature = ScaleTemperature(temperature);

            pressure = (Single) ((D1 * SENS / Math.Pow(2, 21) - OFF) / Math.Pow(2, 15));

            if (PressureCompensationMode == PressureCompensationModes.SeaLevelCompensated)
            {
                pressure = CalculatePressureAsl(pressure);
            }
        }

        /// <summary>
        ///     Resets the module
        /// </summary>
        public Boolean Reset()
        {
            WriteByte(MS5607_CMD_RESET);
            Thread.Sleep(3);
            return true;
        }

        #endregion

        #region Interface Implementations

        /// <inheritdoc />
        /// <summary>Reads the temperature.</summary>
        /// <param name="source">The source.</param>
        /// <returns>A single representing the temperature read from the source, degrees Celsius</returns>
        /// <example>
        ///     Example usage
        ///     <code language="C#">// None provided as this module does not support reading raw data.</code>
        ///     <code language="VB">'None provided as this module does not support reading raw data.</code>
        ///     None provided as this module does not support reading raw data.
        /// </example>
        /// <exception cref="NotSupportedException">
        ///     A NotSupported Exception will be thrown when attempting to read Temperature as
        ///     this module does not support direct reading of teperature.
        /// </exception>
        public Single ReadTemperature(TemperatureSources source = TemperatureSources.Ambient)
        {
            throw new NotSupportedException("Direct reading of temperature is not supported by this driver. Please ");
        }

        public Single ReadPressure(PressureCompensationModes compensationMode = PressureCompensationModes.SeaLevelCompensated)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc cref="IPressure" />
        /// <inheritdoc cref="ITemperature" />
        /// <summary>Gets the raw data of the pressure value.</summary>
        /// <value>
        ///     Raw data in the range depending on sensor's precision (8/10/12 bits, for example)
        /// </value>
        /// <example>
        ///     Example usage
        ///     <code language="C#">// None provided as this module does not support reading raw data.</code>
        ///     <code language="VB">'None provided as this module does not support reading raw data.</code>
        ///     None provided as this module does not support reading raw data.
        /// </example>
        public Int32 RawData =>
            throw new NotSupportedException(
                "Reading raw data is not supported by this module or implemented in this driver.");

        #endregion
    }
}