/*
 * Pressure9 Click driver for TinyCLR 2.0
 * 
 * Initial version coded by Stephen Cardinale
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
using System.Diagnostics;
using System.Threading;

namespace MBN.Modules
{
    /// <summary>
    /// Main class for the Pressure9Click driver
    /// <para><b>Pins used :</b> Scl, Sda</para>
    /// </summary>
    /// <example>
    /// <code language = "C#">
    /// using System;
    /// using System.Diagnostics;
    /// using System.Threading;
    /// using MBN;
    /// using MBN.Modules;
    ///
    /// namespace Examples
    /// {
    ///     internal class Program
    ///     {
    ///         private static Pressure9Click _sensor;
    ///
    ///         private static void Main()
    ///         {
    ///             _sensor = new Pressure9Click(Hardware.SocketOne, Pressure9Click.I2cAddresses.Address0)
    ///             {
    ///                 PressureCompensation = PressureCompensationModes.SeaLevelCompensated,
    ///                 TemperatureUnit = TemperatureUnits.Fahrenheit
    ///             };
    ///
    ///             Pressure9Click.SensorConfiguration configuration = new Pressure9Click.SensorConfiguration
    ///             {
    ///                 TemperatureMeasurementRate = Pressure9Click.TemperatureMeasurementRates.Hertz_8,
    ///                 TemperatureSamplingRate = Pressure9Click.OverSamplingRates.OSR_8X,
    ///                 PressureMeasurementRate = Pressure9Click.PressureMeasurementRates.Hertz_8,
    ///                 PressureSamplingRate = Pressure9Click.OverSamplingRates.OSR_8X,
    ///                 SensorMeasurementControl = Pressure9Click.MeasurementControl.ContinuousPressureandTemperatureMeasurement,
    ///                 FIFOEnable = true,
    ///                 FIFOBehavior = Pressure9Click.FIFOBehaviors.Streaming
    ///             };
    ///
    ///             _sensor.SetConfiguration(configuration);
    ///
    ///             Debug.WriteLine($"Product ID is 0x{_sensor.ProductID:x2}");
    ///             Debug.WriteLine($"Silicon Revision 0x{_sensor.SiliconRevision:x2}");
    ///
    ///             while (true)
    ///             {
    ///                 while (_sensor.ReadIFOFillLevel() <![CDATA[<]]> 32) Thread.Sleep(375); // Don't poll any faster than 375 ms per data-sheet.
    ///
    ///                 Single[] data = _sensor.ReadFIFO();
    ///
    ///                 Debug.WriteLine("----------Pressure 9 Click----------");
    ///                 Debug.WriteLine($"Temperature.......: {data[0]:F2} *F");
    ///                 Debug.WriteLine($"Pressure..........: {data[1]:F2} Pa");
    ///                 Debug.WriteLine($"Altitude..........: {_sensor.Altitude:F1} meters\n");
    ///             }
    ///         }
    ///     }
    /// }
    /// </code>
    /// </example>
    public sealed class Pressure9Click : IPressure, ITemperature
    {
        #region .ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="Pressure9Click"/> class.
        /// </summary>
        /// <param name="socket">The socket on which the Pressure9Click module is plugged on MikroBus.Net board</param>
        /// <param name="slaveAddress">The address of the module.</param>
        public Pressure9Click(Hardware.Socket socket, I2cAddresses slaveAddress)
        {
            _socket = socket;
#if (NANOFRAMEWORK_1_0)
            _sensor = I2cDevice.Create(new I2cConnectionSettings(socket.I2cBus, (int)slaveAddress, I2cBusSpeed.StandardMode));
#else
			_sensor = I2cController.FromName(socket.I2cBus).GetDevice(new I2cConnectionSettings((Int32) slaveAddress, 100000));
#endif

            ResetSensor(ResetTypes.FullReset, true);

            // Almost do not need to check for initialization complete as it is very quick.
            Byte[] registerData;
            do
            {
                registerData = ReadRegister(DPS422_REG_MEAS_CFG, 1);
            } while ((registerData[0] & 0x80) == 0x00);

            ReadCoefficients();
        }

        #endregion

        #region Public ENUMS

        /// <summary>
        /// The I2C address the Pressure 9 Click supports
        /// </summary>
        public enum I2cAddresses
        {
            /// <summary>
            /// I2C Slave address is 0x76 with the Address Select jumper soldered in the zero (0) position.
            /// </summary>
            Address0 = 0x76,

            /// <summary>
            /// I2C Slave address is 0x77 with the Address Select jumper soldered in the one (1) position.
            /// </summary>
            Address1 = 0x77
        }

        /// <summary>
        /// Temperature measurement rate.
        /// </summary>
        public enum TemperatureMeasurementRates
        {
            /// <summary>
            /// Measurement rate is 1 hertz or one measurement per second.
            /// </summary>
            Hertz_1 = 0x01,

            /// <summary>
            /// Measurement rate is 2 hertz or 2 measurements per second.
            /// </summary>
            Hertz_2 = 0x01,

            /// <summary>
            /// Measurement rate is 4 hertz or 4 measurements per second.
            /// </summary>
            Hertz_4 = 0x02,

            /// <summary>
            /// Measurement rate is 8 hertz or 8 measurements per second.
            /// </summary>
            Hertz_8 = 0x03,

            /// <summary>
            /// Measurement rate is 16 hertz or 16 measurements per second.
            /// </summary>
            Hertz_16 = 0x04,

            /// <summary>
            /// Measurement rate is 32 hertz or 32 measurements per second.
            /// </summary>
            Hertz_32 = 0x05,

            /// <summary>
            /// Measurement rate is 64 hertz or 64 measurements per second.
            /// </summary>
            Hertz_64 = 0x06
        }

        /// <summary>
        /// Pressure measurement rates.
        /// </summary>
        public enum PressureMeasurementRates
        {
            /// <summary>
            /// Measurement rate is 1 hertz or one measurement per second.
            /// </summary>
            Hertz_1 = 0x01,

            /// <summary>
            /// Measurement rate is 2 hertz or 2 measurements per second.
            /// </summary>
            Hertz_2 = 0x01,

            /// <summary>
            /// Measurement rate is 4 hertz or 4 measurements per second.
            /// </summary>
            Hertz_4 = 0x02,

            /// <summary>
            /// Measurement rate is 8 hertz or 8 measurements per second.
            /// </summary>
            Hertz_8 = 0x03,

            /// <summary>
            /// Measurement rate is 16 hertz or 16 measurements per second.
            /// </summary>
            Hertz_16 = 0x04,

            /// <summary>
            /// Measurement rate is 32 hertz or 32 measurements per second.
            /// </summary>
            Hertz_32 = 0x05,

            /// <summary>
            /// Measurement rate is 64 hertz or 64 measurements per second.
            /// </summary>
            Hertz_64 = 0x06,
            /// <summary>
            /// Measurement rate is 128 hertz or 128 measurements per second.
            /// </summary>
            Hertz_128 = 0x07
        }

        /// <summary>
        /// Oversampling rates or the number of samples to take to reduce electronic noise influences.
        /// </summary>
        public enum OverSamplingRates
        {
            /// <summary>
            /// Oversampling Rate is 1x decimation or 256 samples taken.
            /// <para>Pressure precision is 5 Pa and Pressure measurement time is 3.6 ms</para>
            /// <para>Temperature measurement time is 5.2 ms</para>
            /// </summary>
            OSR_1X = 0x00,

            /// <summary>
            /// Oversampling Rate is 2x decimation or 512 samples taken.
            /// <para>Pressure precision is 5 Pa and Pressure measurement time is 5.2 ms</para>
            /// <para>Temperature measurement time is 8.4 ms</para>
            /// </summary>
            OSR_2X = 0x01,

            /// <summary>
            /// Oversampling Rate is 4x decimation or 1024 samples taken.
            /// <para>Pressure precision is 2.5 Pa and measurement time is 8.4 ms</para>
            /// <para>Temperature measurement time is 14.8 ms</para>
            /// </summary>
            OSR_4X = 0x02,

            /// <summary>
            /// Oversampling Rate is 8x decimation or 2048 samples taken.
            /// <para>Pressure precision is 2.5 Pa and Pressure measurement time is 14.8 ms</para>
            /// <para>Temperature measurement time is 27.6 ms</para>
            /// </summary>
            OSR_8X = 0x03,

            /// <summary>
            /// Oversampling Rate is 16x decimation or 4096 samples taken.
            /// <para>Pressure precision is 1.2 Pa and Pressure measurement time is 27.6 ms</para>
            /// <para>Temperature measurement time is 53.2 ms</para>
            /// </summary>
            OSR_16X = 0x04,

            /// <summary>
            /// Oversampling Rate is 32x decimation or 8192 samples taken.
            /// <para>Pressure precision is 0.9 Pa and Pressure measurement time is 53.2 ms</para>
            /// <para>Temperature measurement time is 104.4 ms</para>
            /// </summary>
            OSR_32X = 0x05,

            /// <summary>
            /// Oversampling Rate is 64x decimation or 16384 samples taken.
            /// <para>Pressure precision is 0.5 Pa and Pressure measurement time is 104.4 ms</para>
            /// <para>Temperature measurement time is 2068 ms</para>
            /// </summary>
            OSR_64X = 0x06,

            /// <summary>
            /// Oversampling Rate is 128x decimation or 32768 samples taken.
            /// <para>Pressure precision is 0.5 Pa and Pressure measurement time is 206.8 ms</para>
            /// <para>Temperature measurement time is not defined</para>
            /// </summary>
            OSR_128X = 0x07
        }

        /// <summary>
        /// Supported Measurement Modes.
        /// </summary>
        public enum MeasurementControl
        {
            /// <summary>
            /// Module is idle. All registers accessible. This is the lowest power setting. When sensor is idle, you can read the temperature or pressure using the one-shot methods only.
            /// </summary>
            Idle = 0x00,

            /// <summary>
            /// Pressure measurement only is enabled. Once pressure conversion is complete, you can read the pressure using the <see cref="Pressure9Click.ReadPressure"/> method.
            /// </summary>
            /// <remarks>In this configuration, the pressure measurement will not re-trigger and the measurement mode will fall back to Idle. In order to read pressure again, the configuration must be set again to PressureMeasurementOnly.</remarks>
            PressureMeasurementOnly = 0x01,

            /// <summary>
            /// Temperature measurement only is enabled. Once temperature conversion is complete, you can read the pressure using the <see cref="Pressure9Click.ReadTemperature"/> method.
            /// </summary>
            /// <remarks>In this configuration, the temperature measurement will not re-trigger and the measurement mode will fall back to Idle. In order to read temperature again, the configuration must be set again to TemperatureMeasurementOnly.</remarks>
            TemperatureMeasurementOnly = 0x02,

            /// <summary>
            /// Both Pressure and Temperature are enabled. This is preferred configuration for accurate pressure reading as the pressure conversion is based on accurate temperature measurement.
            /// </summary>
            PressureAndTemperatureOneShot = 0x03,

            /// <summary>
            /// This configuration will stop any continuous conversions and revert back to Idle power mode.
            /// </summary>
            StopBackground = 0x04,

            /// <summary>
            /// Only continuous Pressure measurement is enabled. You can read pressure measurements using the <see cref="Pressure9Click.ReadPressure"/> or <see cref="Pressure9Click.ReadFIFO"/> methods.
            /// </summary>
            ContinuousPressureMeasurement = 0x05,

            /// <summary>
            /// Only continuous Temperature measurement is enabled. You can read temperature measurements using the <see cref="Pressure9Click.ReadTemperature"/> or <see cref="Pressure9Click.ReadFIFO"/> methods.
            /// </summary>
            ContinuousTemperatureMeasurement = 0x06,

            /// <summary>
            /// Continuous Pressure and Temperature measurement is enabled. You can read pressure and temperature measurements using the <see cref="Pressure9Click.ReadTemperature"/> and <see cref="Pressure9Click.ReadTemperature"/> or <see cref="Pressure9Click.ReadFIFO"/> methods.
            /// This is preferred configuration for accurate pressure reading as the pressure conversion is based on accurate temperature measurement.
            /// </summary>
            ContinuousPressureandTemperatureMeasurement = 0x07
        }

        /// <summary>
        /// The behavior of the FiFo 32 byte buffer when the FiFo is enabled.
        /// </summary>
        public enum FIFOBehaviors
        {
            /// <summary>
            /// Data is continually streamed to the FiFo buffer with the oldest data removed to make room for new data.
            /// </summary>
            Streaming = 0x00,

            /// <summary>
            /// Data is streamed to the FiFo buffer but all measurements are stopped when the buffer is full. Measurements resume when the FiFo is read.
            /// </summary>
            StopOnFull = 0x01
        }

        /// <summary>
        /// One of the two soft reset types supported by the Pressure 9 Click.
        /// </summary>
        public enum ResetTypes
        {
            /// <summary>
            /// Partial reset of configuration registers without refreshing the temperature and pressure coefficients.
            /// </summary>
            PartialReset = 0x08,

            /// <summary>
            /// Full reset. similar to a Power-on-reset. All registers are reset and temperature and pressure coefficients are refreshed.
            /// </summary>
            FullReset = 0x09
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets the Product ID of the DPS422 IC hosted in the Pressure 9 Click.
        /// </summary>
        /// <returns>The Product ID set by Infineon. Should be 0x0A.
        /// </returns>
        /// <example>Example usage:
        /// <code language = "C#">
        /// Debug.WriteLine("Product ID is 0x" + _sensor.ProductID.ToString("x2"));
        /// </code>
        /// <code language = "VB">
        /// Debug.WriteLine("Product ID is 0x" <![CDATA[&]]> _sensor.ProductID.ToString("x2"));
        /// </code>
        /// </example>
        public Byte ProductID
        {
            get
            {
                Byte[] registerData = ReadRegister(DPS422_REG_PROD_ID, 1);
                return (Byte) (registerData[0] & 0x0F);
            }
        }

        /// <summary>
        /// Gets the Silicon Revision of the DPS422 IC hosted on the Pressure 9 Click.
        /// </summary>
        /// <returns>
        /// </returns>
        /// <example>Example usage:
        /// <code language = "C#">
        /// Debug.WriteLine("Silicon Revision 0x" + _sensor.SiliconRevision.ToString("x2"));
        /// </code>
        /// <code language = "VB">
        /// Debug.WriteLine("Silicon Revision 0x" <![CDATA[&]]> _sensor.SiliconRevision.ToString("x2"))
        /// </code>
        /// </example>
        public Byte SiliconRevision
        {
            get
            {
                Byte[] registerData = ReadRegister(DPS422_REG_PROD_ID, 1);
                return (Byte) (registerData[0] >> 4);
            }
        }

        /// <summary>
        /// Gets or sets the pressure compensation mode for one-shot pressure measurements.
        /// </summary>
        /// <example>Example usage:
        /// <code language = "C#">
        /// _sensor.PressureCompensation = PressureCompensationModes.SeaLevelCompensated;
        /// </code>
        /// <code language = "VB">
        /// _sensor.PressureCompensation = PressureCompensationModes.SeaLevelCompensated
        /// </code>
        /// </example>
        public PressureCompensationModes PressureCompensation { get; set; } = PressureCompensationModes.SeaLevelCompensated;

        /// <summary>
        /// Gets or sets the <see cref="TemperatureUnits"/> used for temperature measurements.
        /// </summary>
        /// <example>
        /// Example usage:
        /// <code language="C#">
        /// _sensor.TemperatureUnits = TemperatureUnits.Kelvin;
        /// </code>
        /// <code language="VB">
        /// _sensor.TemperatureUnits = TemperatureUnits.Kelvin
        /// </code>
        /// </example>
        public TemperatureUnits TemperatureUnit { get; set; } = TemperatureUnits.Celsius;

        /// <summary>
        /// Gets the altitude in meters that was sampled during the last pressure conversion.
        /// </summary>
        /// <remarks>The Altitude property is updated after each Pressure measurement.</remarks>
        /// <example>Example usage:
        /// <code language="C#">
        /// Debug.WriteLine("Pressure..........: " + _sensor.ReadPressure().ToString("F2") + " Pa");
        /// Debug.WriteLine("Altitude..........: " + _sensor.Altitude.ToString("F1") + " meters");
        /// </code>
        /// <code language = "VB">
        /// Debug.WriteLine("Pressure..........: " <![CDATA[&]]> _sensor.ReadPressure().ToString("F2") <![CDATA[&]]> " Pa");
        /// Debug.WriteLine("Altitude..........: " <![CDATA[&]]> _sensor.Altitude.ToString("F1") <![CDATA[&]]> " meters");
        /// </code>
        /// </example>
        public Single Altitude { get; private set; }

        #endregion

        #region Public Methods

        /// <summary>
        /// Resets the Pressure 9 Click.
        /// </summary>
        /// <param name="resetType">The reset type to implement. <see cref="ResetTypes"/> for a detailed explanation.</param>
        /// <param name="clearFIFO">Set to true to clear the FiFo buffer.</param>
        /// <example>Example usage:
        /// <code language = "C#">
        /// _sensor.ResetSensor(Pressure9Click.ResetTypes.PartialReset, true);
        /// </code>
        /// </example>
        public void ResetSensor(ResetTypes resetType, Boolean clearFIFO)
        {
            Byte registerData = (Byte)resetType;
            if (clearFIFO) registerData |= 0x80;

            WriteRegister(DPS422_REG_RESET, registerData);

            Thread.Sleep(resetType == ResetTypes.FullReset ? 3 : 1);
        }

        /// <summary>
        /// Sets the sensor configuration to the values in the configuration parameter.
        /// </summary>
        /// <param name="configuration">The sensor configuration to use. See <see cref="SensorConfiguration"/> class for more information.</param>
        /// <example>Example usage:
        /// <code language = "C#">
        /// Pressure9Click.SensorConfiguration configuration = new Pressure9Click.SensorConfiguration
        /// {
        ///     TemperatureMeasurementRate = Pressure9Click.TemperatureMeasurementRates.Hertz_8,
        ///     TemperatureSamplingRate = Pressure9Click.OverSamplingRates.OSR_16X,
        ///     PressureMeasurementRate = Pressure9Click.PressureMeasurementRates.Hertz_8,
        ///     PressureSamplingRate = Pressure9Click.OverSamplingRates.OSR_16X,
        ///     SensorMeasurementControl = Pressure9Click.MeasurementControl.ContinuousPressureandTemperatureMeasurement,
        ///     FIFOEnable = false,
        ///     FIFOBehavior = Pressure9Click.FIFOBehaviors.Streaming
        /// };
        ///
        /// _sensor.SetConfiguration(configuration);
        /// </code>
        /// </example>
        public void SetConfiguration(SensorConfiguration configuration)
        {
            Byte registerData = (Byte)((Byte)configuration.PressureSamplingRate | (Byte)((Byte)configuration.PressureMeasurementRate << 4));
            WriteRegister(DPS422_REG_PSR_CFG, registerData);

            registerData = (Byte)((Byte)configuration.TemperatureSamplingRate | (Byte)((Byte)configuration.TemperatureMeasurementRate << 4) | 0x80);
            WriteRegister(DPS422_REG_TEMP_CFG, registerData);

            registerData = ReadRegister(DPS422_REG_MEAS_CFG, 1)[0];
            registerData |= (Byte)configuration.SensorMeasurementControl;
            WriteRegister(DPS422_REG_MEAS_CFG, registerData);

            registerData = (Byte) (((Int32) configuration.FIFOBehavior << 2) | ((configuration.FIFOEnable ? 1 : 0) << 1));
            WriteRegister(DPS422_REG_CFG_REG, registerData);
        }

        /// <summary>
        /// Performs a one shot temperature measurement. The configuration must be Idle to use this method.
        /// </summary>
        /// <returns>
        /// </returns>
        /// <exception cref="ApplicationException">An ApplicationExcepttion will be thrown if attempting to use this method when the Measurement Control is not set to Idle.</exception>
        /// <example>Example usage:
        /// <code language = "C#">
        /// while (true)
        /// {
        ///     Debug.WriteLine("Temperature..........: " + _sensor.ReadTemperatureOneShot().ToString("F2") + " °C");
        ///     Debug.WriteLine("Pressure.............: " + _sensor.ReadPressureOneShot().ToString("F2") + " Pa");
        ///     Thread.Sleep(1000);
        /// }
        /// </code>
        /// </example>
        public Single ReadTemperatureOneShot()
        {
            Byte registerData = ReadRegister(DPS422_REG_MEAS_CFG, 1)[0];

            MeasurementControl currentMode = (MeasurementControl)(Byte) (registerData & 0x07);

            if (!CheckForIdleMode(currentMode))
                throw new ApplicationException(
                    "You cannot use this method to read one-shot temperature measurements while not in idle, background or OneShot Pressure and Temperature modes...");

            if (currentMode != MeasurementControl.PressureAndTemperatureOneShot)
            {
                registerData |= (Byte)MeasurementControl.TemperatureMeasurementOnly;
                WriteRegister(DPS422_REG_MEAS_CFG, registerData);
            }

            Byte readyFlag;

            do
            {
                readyFlag = ReadRegister(DPS422_REG_MEAS_CFG, 1)[0];
                Thread.Sleep(5);
            } while ((readyFlag & 0x20) != 0x20);

            Int32 temp = (this as ITemperature).RawData;
            GetTwosComplement(ref temp, 24);

            WriteRegister(DPS422_REG_MEAS_CFG, (Byte) currentMode);

            return ScaleTemperature(temp);
        }

        /// <summary>
        /// Performs a one shot pressure measurement. The configuration must be Idle to use this method.
        /// </summary>
        /// <returns>
        /// </returns>
        /// <exception cref="ApplicationException">An ApplicationExcepttion will be thrown if attempting to use this method when the Measurement Control is not set to Idle.</exception>
        /// <example>Example usage:
        /// <code language = "C#">
        /// while (true)
        /// {
        ///     Debug.WriteLine("Temperature..........: " + _sensor.ReadTemperatureOneShot().ToString("F2") + " °C");
        ///     Debug.WriteLine("Pressure.............: " + _sensor.ReadPressureOneShot().ToString("F2") + " Pa");
        ///     Thread.Sleep(1000);
        /// }
        /// </code>
        /// </example>
        public Single ReadPressureOneShot()
        {
            Byte registerData = ReadRegister(DPS422_REG_MEAS_CFG, 1)[0];

            MeasurementControl currentMode = (MeasurementControl)(Byte) (registerData & 0x07);

            if (!CheckForIdleMode(currentMode))
                throw new ApplicationException(
                    "You cannot use this method to read one-shot temperature measurements while not in idle or background mode...");

            if (currentMode != MeasurementControl.PressureAndTemperatureOneShot)
            {
                registerData |= (Byte)MeasurementControl.PressureMeasurementOnly;
                WriteRegister(DPS422_REG_MEAS_CFG, registerData);
            }

            Byte readyFlag;

            do
            {
                readyFlag = ReadRegister(DPS422_REG_MEAS_CFG, 1)[0];
                Thread.Sleep(5);
            } while ((readyFlag & 0x10) != 0x10);

            Int32 rawData = (this as IPressure).RawData;
            GetTwosComplement(ref rawData, 24);

            WriteRegister(DPS422_REG_MEAS_CFG, (Byte) currentMode);

            Single pressureData = CalculatePressure(rawData);

            Altitude = CalcualteAltitude(pressureData);

            switch (PressureCompensation)
            {
                case PressureCompensationModes.SeaLevelCompensated:
                {
                    return CalculatePressureAsl(pressureData);
                }
                case PressureCompensationModes.Uncompensated:
                {
                    return pressureData;
                }
                default:
                {
                    throw new ArgumentOutOfRangeException();
                }
            }
        }

        /// <summary>
        /// Reads the FiFo buffer when the FiFo is enabled.
        /// </summary>
        /// <returns>
        /// An Single array containing the average of the temperature readings as the first Single value and the average of the pressure readings as the second Single value.
        /// </returns>
        /// <example>Example usage:
        /// <code language = "C#">
        /// while (true)
        /// {
        ///     while (_sensor.ReadIFOFillLevel() <![CDATA[<]]> 32) Thread.Sleep(375);
        ///
        ///     Single[] data = _sensor.ReadFIFO();
        ///
        ///     Debug.WriteLine("Temperature.......: " + data[0].ToString("F2") + " °C");
        ///     Debug.WriteLine("Pressure..........: " + data[1].ToString("F2") + " Pa");
        /// }
        /// </code>
        /// </example>
        public Single[] ReadFIFO()
        {
            Byte fillLevel = ReadIFOFillLevel();
            Single temperatureData = 0;
            Byte temperatureCount = 0;
            Single pressureData = 0;
            Byte pressureCount = 0;

            if (ReadIFOFillLevel() <= 0)
            {
                return new[] {Single.MinValue, Single.MinValue};
            }

            for (Byte x = 0; x < fillLevel; x++)
            {
                Int32 tempData = (this as IPressure).RawData;

                if ((tempData & 0x01) == 0x00)
                {
                    temperatureData += tempData;
                    temperatureCount++;
                }
                else
                {
                    pressureData += tempData;
                    pressureCount++;
                }
            }

            Int32 rawTemperatDataAverage = (Int32) (temperatureCount > 0 ? temperatureData / temperatureCount : 0.0F);
            Int32 rawPressureDataAverage = (Int32) (pressureCount > 0 ? pressureData / pressureCount : 0.0F);

            Single temperatureResult;
            Single pressureResult;

            if (temperatureCount > 0)
            {
                GetTwosComplement(ref rawTemperatDataAverage, 24);
                temperatureResult = CalculateTemperature(rawTemperatDataAverage);
            }
            else
            {
                temperatureResult = Single.MinValue;
            }

            if (pressureCount > 0)
            {
                if ((ReadRegister(DPS422_REG_MEAS_CFG, 1)[0] & 0x07) == 0x05)
                {
                    _lastTempScale = 0.08716583251F;
                }

                GetTwosComplement(ref rawPressureDataAverage, 24);
                pressureResult = CalculatePressure(rawPressureDataAverage);
                Altitude = CalcualteAltitude(pressureResult);
            }
            else
            {
                pressureResult = Single.MinValue;
            }

            switch (PressureCompensation)
            {
                case PressureCompensationModes.SeaLevelCompensated:
                {
                    pressureResult = CalculatePressureAsl(pressureResult);
                    break;
                }
                case PressureCompensationModes.Uncompensated:
                {
                    break;
                }
                default:
                {
                    throw new ArgumentOutOfRangeException();
                }
            }

            return new[] {ScaleTemperature(temperatureResult), pressureResult};
        }

        /// <summary>
        /// Returns the number of entries currently in the FiFo buffer.
        /// </summary>
        /// <returns>The number of entries.</returns>
        /// <example>Example usage:
        /// <code language = "C#">
        /// while (true)
        /// {
        ///     while (_sensor.ReadIFOFillLevel() <![CDATA[<]]> 32) Thread.Sleep(375);
        ///
        ///     Single[] data = _sensor.ReadFIFO();
        ///
        ///     Debug.WriteLine("Temperature.......: " + data[0].ToString("F2") + " °C");
        ///     Debug.WriteLine("Pressure..........: " + data[1].ToString("F2") + " Pa");
        /// }
        /// </code>
        /// </example>
        public Byte ReadIFOFillLevel()
        {
            //return (Byte) (ReadRegister(DPS422_REG_FIFO_STS, 1)[0] >> 2);
            Byte level =  (Byte) (ReadRegister(DPS422_REG_FIFO_STS, 1)[0] >> 2);
            Debug.WriteLine($"ReadFifoFillLevel reports {level}");
            return level;
        }

        #endregion

        #region Private Fields

        private readonly I2cDevice _sensor;
        private readonly Hardware.Socket _socket;

        // Compensation coefficients
        private Int32 C00;
        private Int32 C01;
        private Int32 C02;
        private Int32 C10;
        private Int32 C11;
        private Int32 C12;
        private Int32 C20;
        private Int32 C21;
        private Int32 C30;
        private Single a_prime;
        private Single b_prime;

        // Last measured scaled temperature (necessary for pressure compensation)
        //private Single _lastTempScale = 0.08716583251F; // In case temperature reading disabled, the default raw temperature value corresponds the reference temperature of 27 degrees.
        private Single _lastTempScale;

        // Scale factor for Pressure measurement based upon Pressure OSR.
        private readonly Int32[] ScaleFactor = {524288, 1572864, 3670016, 7864320, 253952, 516096, 1040384, 2088960};

        #endregion

        #region Constants

        // Registers
        private const Byte DPS422_REG_PSR_B2 = 0x00;
        private const Byte DPS422_REG_TMP_B2 = 0x03;
        private const Byte DPS422_REG_PSR_CFG = 0x06;
        private const Byte DPS422_REG_TEMP_CFG = 0x07;
        private const Byte DPS422_REG_MEAS_CFG = 0x08;
        private const Byte DPS422_REG_CFG_REG = 0x09;
        private const Byte DPS422_REG_FIFO_STS = 0x0C;
        private const Byte DPS422_REG_RESET = 0x0D;
        private const Byte DPS422_REG_PROD_ID = 0x1D;

        // Constants used for temperature calculation
        private const Byte DPS422_T_REF = 27;
        private const Single DPS422_V_BE_TARGET = 0.687027F;
        private const Single DPS422_ALPHA = 9.45f;
        private const Single DPS422_T_C_VBE = -0.001735F;
        private const Single DPS422_K_PTAT_CORNER = -0.8F;
        private const Single DPS422_K_PTAT_CURVATURE = 0.039F;
        private const UInt16 DPS422_A_0 = 5030;

        #endregion

        #region Private Methods

        private static void GetTwosComplement(ref Int32 raw, Byte length)
        {
            if ((raw & (1 << (length - 1))) == 1 << (length - 1))
            {
                raw -= 1 << length;
            }
        }

        private void ReadCoefficients()
        {
            Byte[] buffer_temp = ReadRegister(0x20, 3);
            Byte[] buffer_prs = ReadRegister(0x26, 20);

            // Refer to data sheet
            // 1. Read T_Vbe, T_dVbe and T_gain
            Int32 t_gain = buffer_temp[0];
            Int32 t_dVbe = buffer_temp[1] >> 1;
            Int32 t_Vbe = (buffer_temp[1] & 0x01) | (buffer_temp[2] << 1);

            GetTwosComplement(ref t_gain, 8);
            GetTwosComplement(ref t_dVbe, 7);
            GetTwosComplement(ref t_Vbe, 9);

            // 2. Vbe, dVbe and Aadc
            Single Vbe = t_Vbe * 0.000105031F + 0.463232422F;
            Single dVbe = t_dVbe * 0.0000125885F + 0.04027621F;
            Single Aadc = t_gain * 0.000084375F + 0.675F;

            // 3. Vbe_cal and dVbe_cal
            Single Vbe_cal = Vbe / Aadc;
            Single dVbe_cal = dVbe / Aadc;

            // 4. T_calib
            Single T_calib = DPS422_A_0 * dVbe_cal - 273.15F;

            // 5. Vbe_cal(T_ref): Vbe value at reference temperature
            Single Vbe_cal_tref = Vbe_cal - (T_calib - DPS422_T_REF) * DPS422_T_C_VBE;

            // 6. Calculate PTAT correction coefficient
            Single k_ptat = (DPS422_V_BE_TARGET - Vbe_cal_tref) * DPS422_K_PTAT_CORNER + DPS422_K_PTAT_CURVATURE;

            // 7. calculate A' and B'
            a_prime = DPS422_A_0 * (Vbe_cal + DPS422_ALPHA * dVbe_cal) * (1 + k_ptat);
            b_prime = -273.15F * (1 + k_ptat) - k_ptat * T_calib;

            // c00, c01, c02, c10 : 20 bits
            // c11, c12: 17 bits
            // c20: 15 bits; c21: 14 bits; c30 12 bits
            C00 = (buffer_prs[0] << 12) | (buffer_prs[1] << 4) | ((buffer_prs[2] & 0xF0) >> 4);
            C10 = ((buffer_prs[2] & 0x0F) << 16) | (buffer_prs[3] << 8) | buffer_prs[4];
            C01 = (buffer_prs[5] << 12) | (buffer_prs[6] << 4) | ((buffer_prs[7] & 0xF0) >> 4);
            C02 = ((buffer_prs[7] & 0x0F) << 16) | (buffer_prs[8] << 8) | buffer_prs[9];
            C20 = ((buffer_prs[10] & 0x7F) << 8) | buffer_prs[11];
            C30 = ((buffer_prs[12] & 0x0F) << 8) | buffer_prs[13];
            C11 = (buffer_prs[14] << 9) | (buffer_prs[15] << 1) | ((buffer_prs[16] & 0x80) >> 7);
            C12 = ((buffer_prs[16] & 0x7F) << 10) | (buffer_prs[17] << 2) | ((buffer_prs[18] & 0xC0) >> 6);
            C21 = ((buffer_prs[18] & 0x3F) << 8) | buffer_prs[19];

            GetTwosComplement(ref C00, 20);
            GetTwosComplement(ref C01, 20);
            GetTwosComplement(ref C02, 20);
            GetTwosComplement(ref C10, 20);
            GetTwosComplement(ref C11, 17);
            GetTwosComplement(ref C12, 17);
            GetTwosComplement(ref C20, 15);
            GetTwosComplement(ref C21, 14);
            GetTwosComplement(ref C30, 12);
        }

        private Byte[] ReadRegister(Byte registerAddress, Byte numberOfBytesToRead)
        {
            Byte[] readBuffer = new Byte[numberOfBytesToRead];

            lock (_socket.LockI2c)
            {
                _sensor.WriteRead(new[] { registerAddress}, readBuffer);
            }

            return readBuffer;
        }

        private void WriteRegister(Byte registerAddress, Byte data)
        {
            lock (_socket.LockI2c)
            {
                _sensor.Write(new[] { registerAddress, data});
            }
        }

        private Single CalculateTemperature(Int32 rawTemperatureData)
        {
            _lastTempScale = rawTemperatureData / 1048576F;
            Single u = _lastTempScale / (1 + DPS422_ALPHA * _lastTempScale);
            return a_prime * u + b_prime;
        }

        private Single CalculatePressure(Int32 rawPressureData)
        {
            Single prs = rawPressureData;
            prs /= ScaleFactor[ReadRegister(DPS422_REG_PSR_CFG, 1)[0] & 0x07];

            Single temp = 8.5F * _lastTempScale / (1.0F + 8.8F * _lastTempScale);

            return C00 + C10 * prs + C01 * temp + C20 * prs * prs + C02 * temp * temp +
                  C30 * prs * prs * prs + C11 * temp * prs + C12 * prs * temp * temp + C21 * prs * prs * temp;
        }

        private static Boolean CheckForIdleMode(MeasurementControl currentMode)
        {
            return currentMode == MeasurementControl.Idle || currentMode == MeasurementControl.StopBackground || currentMode == MeasurementControl.PressureAndTemperatureOneShot;
        }

        private static Single CalcualteAltitude(Single uncompensatedPressure)
        {
            Single pressASL = CalculatePressureAsl(uncompensatedPressure);
            return (Single) (44330 * (1.0 - Math.Pow(uncompensatedPressure / pressASL, 0.1903)));
        }

        private static Single CalculatePressureAsl(Single uncompensatedPressure)
        {
            Single seaLevelCompensation = (Single)(101325 * Math.Pow((288 - 0.0065 * 143) / 288, 5.256));
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

        #region Interface Implementations

        /// <inheritdoc cref = "ITemperature" />
        /// <summary>
        /// Reads the Temperature in the <see cref="TemperatureUnit"/> as set in the <see cref="TemperatureUnit"/> property.
        /// </summary>
        /// <remarks>This method is intended to be used when the configuration is in continuous mode only.</remarks>
        /// <remarks>Reading the temperature is recommended as the Pressure readings uses the temperature to calculate the pressure. If not read, the driver automatically uses a standard 27°C value for pressure calculation.</remarks>
        /// <returns>
        /// A <see cref="System.Single"/> representing the temperature reading in the unit specified in the <see cref="TemperatureUnit"/> property.
        /// </returns>
        /// <exception cref="NotSupportedException">A NotSupoportedException will be thrown if attempting to read Object temperature as this module does not support reading Object temperature. Use <see cref="TemperatureSources.Ambient"/> instead.</exception>
        /// <exception cref="NotSupportedException">A NotSupoportedException will be thrown if attempting to use this method to read Pressure when not in Continuous mode of operation.</exception>
        /// <example>Example usage:
        /// <code language="C#">
        /// while (true)
        /// {
        ///     Debug.WriteLine("Temperature.......: " + _sensor.ReadTemperature().ToString("F2") + " °C");
        ///     Debug.WriteLine("Pressure..........: " + _sensor.ReadPressure().ToString("F2") + " Pa");
        ///     Thread.Sleep(1000);
        /// }
        /// </code>
        /// </example>
        public Single ReadTemperature(TemperatureSources source = TemperatureSources.Ambient)
        {
            if (source == TemperatureSources.Object) throw new NotSupportedException("This module does not support reading of object temperature. Use TemperatureSources.Ambient instead.");

            Byte readyFlag = ReadRegister(DPS422_REG_MEAS_CFG, 1)[0];

            if ((readyFlag & 0x40) == 0x00) throw new NotSupportedException("This method only supports reading of temperature in continuous mode. For one-shot temperature measurement, use the ReadTemperatureOneshot method.");

            do
            {
                readyFlag = ReadRegister(DPS422_REG_MEAS_CFG, 1)[0];
                Thread.Sleep(5);
            } while ((readyFlag & 0x20) != 0x20);

            Int32 temp = (this as ITemperature).RawData;
            GetTwosComplement(ref temp, 24);

            return ScaleTemperature(temp);
        }

        /// <inheritdoc cref = "IPressure" />
        /// <summary>
        /// Reads the Pressure in Pascals (Pa) from the Pressure 9 Click.
        /// </summary>
        /// <remarks>This method is intended to be used when the configuration is in continuous mode only.</remarks>
        /// <returns>
        /// A <see cref="System.Single"/> representing the Pressure in Pascals (Pa).
        /// </returns>
        /// <param name="compensationMode">The <see cref="PressureCompensationModes"/> to return. Either <see cref="PressureCompensationModes.SeaLevelCompensated"/> or <see cref="PressureCompensationModes.Uncompensated"/>.</param>
        /// <exception cref="NotSupportedException">A NotSupoportedException will be thrown if attempting to use this method to read Pressure when not in Continuous mode of operation.</exception>
        /// <example>Example usage:
        /// <code language="C#">
        /// while (true)
        /// {
        ///     Debug.WriteLine("Temperature.......: " + _sensor.ReadTemperature().ToString("F2") + " °C");
        ///     Debug.WriteLine("Pressure..........: " + _sensor.ReadPressure().ToString("F2") + " Pa");
        ///     Thread.Sleep(1000);
        /// }
        /// </code>
        /// </example>
        public Single ReadPressure(PressureCompensationModes compensationMode = PressureCompensationModes.SeaLevelCompensated)
        {
            Byte readyFlag = ReadRegister(DPS422_REG_MEAS_CFG, 1)[0];

            if ((readyFlag & 0x40) == 0x00) throw new NotSupportedException("This method only supports reading of pressure in continuous mode. For one-shot pressure measurement, use the ReadPressureOneshot method.");

            do
            {
                readyFlag = ReadRegister(DPS422_REG_MEAS_CFG, 1)[0];
                Thread.Sleep(5);
            } while ((readyFlag & 0x10) != 0x10);

            if ((readyFlag & 0x07) != 0x07) _lastTempScale = 0.08716583251F;

            Int32 pres = (this as IPressure).RawData;
            GetTwosComplement(ref pres, 24);

            Single pressureData = CalculatePressure(pres);

            Altitude = CalcualteAltitude(pressureData);

            return compensationMode == PressureCompensationModes.Uncompensated ? pressureData : CalculatePressureAsl(pressureData);
        }

        /// <inheritdoc cref = "ITemperature" />
        /// <summary>
        /// Reads the raw 24 bit temperature data.
        /// </summary>
        /// <returns>
        /// A <see cref="System.Int32"/> of the raw temperature data before conversion.
        /// </returns>
        /// <example>Example usage:
        /// <code language="C#">
        /// Debug.WriteLine("Raw Temperature Data...: " + (_sensor as ITemperature).RawData);
        /// </code>
        /// </example>
        Int32 ITemperature.RawData
        {
            get
            {
                Byte[] registerData = ReadRegister(DPS422_REG_TMP_B2, 3);
                return (registerData[0] << 16) | (registerData[1] << 8) | registerData[2];
            }
        }

        /// <inheritdoc cref = "IPressure" />
        /// <summary>
        /// Reads the raw 24 bit pressure data.
        /// </summary>
        /// <returns>
        /// A <see cref="System.Int32"/> of the raw pressure data before conversion.
        /// </returns>
        /// <example>Example usage:
        /// <code language="C#">
        /// Debug.WriteLine("Raw Temperature Data...: " + (_sensor as IPressure).RawData);
        /// </code>
        /// </example>
        Int32 IPressure.RawData
        {
            get
            {
                Byte[] registerData = ReadRegister(DPS422_REG_PSR_B2, 3);
                return (registerData[0] << 16) | (registerData[1] << 8) | registerData[2];
            }
        }

        #endregion

        #region Sensor Configuration Class

        /// <summary>
        /// A configuration class to hold the configuration parameters of the Pressure 9 click.
        /// </summary>
        /// <example>Example usage:
        /// <code language = "C#">
        /// Pressure9Click.SensorConfiguration _configuration = new Pressure9Click.SensorConfiguration
        /// {
        ///     TemperatureMeasurementRate = Pressure9Click.TemperatureMeasurementRates.Hertz_8,
        ///     TemperatureSamplingRate = Pressure9Click.OverSamplingRates.OSR_16X,
        ///     PressureMeasurementRate = Pressure9Click.PressureMeasurementRates.Hertz_8,
        ///     PressureSamplingRate = Pressure9Click.OverSamplingRates.OSR_16X,
        ///     SensorMeasurementControl = Pressure9Click.MeasurementControl.Idle,
        ///     FIFOEnable = false,
        ///     FIFOBehavior = Pressure9Click.FIFOBehaviors.Streaming
        /// };
        ///
        /// _sensor.SetConfiguration(_configuration);
        /// </code>
        /// </example>
        public class SensorConfiguration
        {
            /// <summary>
            /// Gets or Sets the Sensor Measurement Control of the DPS422 IC hosted on the Pressure 9 Click. See <see cref="SensorMeasurementControl"/> for more information.</summary>
            /// <returns>
            /// </returns>
            /// <example>Example usage:
            /// <code language = "C#">
            /// Pressure9Click.SensorConfiguration _configuration = new Pressure9Click.SensorConfiguration
            /// {
            ///     TemperatureMeasurementRate = Pressure9Click.TemperatureMeasurementRates.Hertz_8,
            ///     TemperatureSamplingRate = Pressure9Click.OverSamplingRates.OSR_16X,
            ///     PressureMeasurementRate = Pressure9Click.PressureMeasurementRates.Hertz_8,
            ///     PressureSamplingRate = Pressure9Click.OverSamplingRates.OSR_16X,
            ///     SensorMeasurementControl = Pressure9Click.MeasurementControl.Idle,
            ///     FIFOEnable = false,
            ///     FIFOBehavior = Pressure9Click.FIFOBehaviors.Streaming
            /// };
            ///
            /// _sensor.SetConfiguration(_configuration);
            /// </code>
            /// </example>
            public MeasurementControl SensorMeasurementControl { get; set; }

            /// <summary>
            /// Gets or Sets the Temperature measurement rate (Hertz) of the DPS422 IC hosted on the Pressure 9 Click. See <see cref="TemperatureMeasurementRates"/> for more information.
            /// </summary>
            /// <returns>
            /// </returns>
            /// <example>Example usage:
            /// <code language = "C#">
            /// Pressure9Click.SensorConfiguration _configuration = new Pressure9Click.SensorConfiguration
            /// {
            ///     TemperatureMeasurementRate = Pressure9Click.TemperatureMeasurementRates.Hertz_8,
            ///     TemperatureSamplingRate = Pressure9Click.OverSamplingRates.OSR_16X,
            ///     PressureMeasurementRate = Pressure9Click.PressureMeasurementRates.Hertz_8,
            ///     PressureSamplingRate = Pressure9Click.OverSamplingRates.OSR_16X,
            ///     SensorMeasurementControl = Pressure9Click.MeasurementControl.Idle,
            ///     FIFOEnable = false,
            ///     FIFOBehavior = Pressure9Click.FIFOBehaviors.Streaming
            /// };
            ///
            /// _sensor.SetConfiguration(_configuration);
            /// </code>
            /// </example>
            public TemperatureMeasurementRates TemperatureMeasurementRate { get; set; }

            /// <summary>
            /// Gets or Sets the Pressure measurement rate (Hertz) of the DPS422 IC hosted on the Pressure 9 Click. See <see cref="PressureMeasurementRates"/> for more information.
            /// </summary>
            /// <returns>
            /// </returns>
            /// <example>Example usage:
            /// <code language = "C#">
            /// Pressure9Click.SensorConfiguration _configuration = new Pressure9Click.SensorConfiguration
            /// {
            ///     TemperatureMeasurementRate = Pressure9Click.TemperatureMeasurementRates.Hertz_8,
            ///     TemperatureSamplingRate = Pressure9Click.OverSamplingRates.OSR_16X,
            ///     PressureMeasurementRate = Pressure9Click.PressureMeasurementRates.Hertz_8,
            ///     PressureSamplingRate = Pressure9Click.OverSamplingRates.OSR_16X,
            ///     SensorMeasurementControl = Pressure9Click.MeasurementControl.Idle,
            ///     FIFOEnable = false,
            ///     FIFOBehavior = Pressure9Click.FIFOBehaviors.Streaming
            /// };
            ///
            /// _sensor.SetConfiguration(_configuration);
            /// </code>
            /// </example>
            public PressureMeasurementRates PressureMeasurementRate { get; set; }

            /// <summary>
            /// Gets or sets the Temperature Oversampling Rate or the number of samples taken per measurement. See <see cref="OverSamplingRates"/> for more information.
            /// </summary>
            /// <example>Example usage:
            /// <code language = "C#">
            /// Pressure9Click.SensorConfiguration _configuration = new Pressure9Click.SensorConfiguration
            /// {
            ///     TemperatureMeasurementRate = Pressure9Click.TemperatureMeasurementRates.Hertz_8,
            ///     TemperatureSamplingRate = Pressure9Click.OverSamplingRates.OSR_16X,
            ///     PressureMeasurementRate = Pressure9Click.PressureMeasurementRates.Hertz_8,
            ///     PressureSamplingRate = Pressure9Click.OverSamplingRates.OSR_16X,
            ///     SensorMeasurementControl = Pressure9Click.MeasurementControl.Idle,
            ///     FIFOEnable = false,
            ///     FIFOBehavior = Pressure9Click.FIFOBehaviors.Streaming
            /// };
            ///
            /// _sensor.SetConfiguration(_configuration);
            /// </code>
            /// </example>
            public OverSamplingRates TemperatureSamplingRate { get; set; }

            /// <summary>
            /// Gets or sets the Pressure Oversampling Rate or the number of samples taken per measurement. See <see cref="OverSamplingRates"/> for more information.
            /// </summary>
            /// <example>Example usage:
            /// <code language = "C#">
            /// Pressure9Click.SensorConfiguration _configuration = new Pressure9Click.SensorConfiguration
            /// {
            ///     TemperatureMeasurementRate = Pressure9Click.TemperatureMeasurementRates.Hertz_8,
            ///     TemperatureSamplingRate = Pressure9Click.OverSamplingRates.OSR_16X,
            ///     PressureMeasurementRate = Pressure9Click.PressureMeasurementRates.Hertz_8,
            ///     PressureSamplingRate = Pressure9Click.OverSamplingRates.OSR_16X,
            ///     SensorMeasurementControl = Pressure9Click.MeasurementControl.Idle,
            ///     FIFOEnable = false,
            ///     FIFOBehavior = Pressure9Click.FIFOBehaviors.Streaming
            /// };
            ///
            /// _sensor.SetConfiguration(_configuration);
            /// </code>
            /// </example>
            public OverSamplingRates PressureSamplingRate { get; set; }

            /// <summary>
            /// Gets or sets the functionality of the FiFo functionality. See <see cref="FIFOBehaviors"/> for more information.
            /// </summary>
            /// <example>Example usage:
            /// <code language = "C#">
            /// Pressure9Click.SensorConfiguration _configuration = new Pressure9Click.SensorConfiguration();
            /// FIFOBehavior = Pressure9Click.FIFOBehaviors.Streaming
            /// _sensor.SetConfiguration(_configuration);
            /// </code>
            /// </example>
            public FIFOBehaviors FIFOBehavior { get; set; }

            /// <summary>
            /// Enables or disables the FIFO functionality of the Pressure 9 Click.
            /// <para>The FIFO can hold up to 32 of the last pressure or temperature values.</para></summary>
            /// <para>Enable the FIFO and use in conjunction with the <see cref="Pressure9Click.ReadFIFO"/> method.</para>
            /// <example>Example usage:
            /// <code language = "C#">
            /// Pressure9Click.SensorConfiguration _configuration = new Pressure9Click.SensorConfiguration();
            /// configuration.FIFOEnable = false,
            /// _sensor.SetConfiguration(_configuration);
            /// </code>
            /// </example>
            public Boolean FIFOEnable { get; set; }

            /// <summary>
            /// Creates an instance with the default configuration.
            /// </summary>
            /// <example>Example usage:
            /// <code language = "C#">
            /// Pressure9Click.SensorConfiguration configuration = new Pressure9Click.SensorConfiguration();
            /// _sensor.SetConfiguration(configuration);
            /// </code>
            /// </example>
            public SensorConfiguration()
            {
                SensorMeasurementControl = MeasurementControl.Idle;
                TemperatureMeasurementRate = TemperatureMeasurementRates.Hertz_1;
                TemperatureSamplingRate = OverSamplingRates.OSR_1X;
                PressureMeasurementRate = PressureMeasurementRates.Hertz_1;
                PressureSamplingRate = OverSamplingRates.OSR_1X;
                FIFOBehavior = FIFOBehaviors.Streaming;
                FIFOEnable = false;
            }

            /// <summary>
            /// Creates an instance with user defined parameters.
            /// </summary>
            /// <param name="sensorMeasurementControl">The <see cref="MeasurementControl"/> to use.</param>
            /// <param name="temperatureMeasurementRate">The <see cref="TemperatureMeasurementRate"/> to use.</param>
            /// <param name="temperatureSamplingRate">The temperature <see cref="OverSamplingRates"/> to use.</param>
            /// <param name="pressureMeasurementRate">The pressure <see cref="PressureMeasurementRate"/> to use.</param>
            /// <param name="pressureSamplingRate">The pressure <see cref="OverSamplingRates"/> to use.</param>
            /// <param name="fifoEnable">Set to true to enable the FiFo.</param>
            /// <param name="fifoBehavior">The behavior of the FiFo. See <see cref="FIFOBehavior"/></param>
            /// <example>Example usage:
            /// <code language = "C#">
            /// Pressure9Click.SensorConfiguration configuration = new Pressure9Click.SensorConfiguration(
            ///    Pressure9Click.MeasurementControl.Idle, Pressure9Click.TemperatureMeasurementRates.Hertz_1,
            ///    Pressure9Click.OverSamplingRates.OSR_1X, Pressure9Click.PressureMeasurementRates.Hertz_1,
            ///    Pressure9Click.OverSamplingRates.OSR_1X, false, Pressure9Click.FIFOBehaviors.StopOnFull);
            /// 
            /// _sensor.SetConfiguration(configuration);
            /// </code>
            /// </example>
            public SensorConfiguration(MeasurementControl sensorMeasurementControl,
                TemperatureMeasurementRates temperatureMeasurementRate,
                OverSamplingRates temperatureSamplingRate,
                PressureMeasurementRates pressureMeasurementRate,
                OverSamplingRates pressureSamplingRate,
                Boolean fifoEnable,
                FIFOBehaviors fifoBehavior)
            {
                SensorMeasurementControl = sensorMeasurementControl;
                TemperatureMeasurementRate = temperatureMeasurementRate;
                TemperatureSamplingRate = temperatureSamplingRate;
                PressureMeasurementRate = pressureMeasurementRate;
                PressureSamplingRate = pressureSamplingRate;
                FIFOEnable = fifoEnable;
                FIFOBehavior = fifoBehavior;
            }
        }

        #endregion
    }
}
