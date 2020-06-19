/*
 * TempHum10 Click driver for TinyCLR 2.0.
 * 
 * Initial version coded by Stephen Cardinale
 * 
 * Copyright 2020 Stephen Cardinale and MikroBUS.Net
 * Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License. You may obtain a copy of the License at
 * http://www.apache.org/licenses/LICENSE-2.0
 * Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, 
 * either express or implied. See the License for the specific language governing permissions and limitations under the License.
 */

#region Usings

using GHIElectronics.TinyCLR.Devices.Gpio;
using GHIElectronics.TinyCLR.Devices.I2c;

using System;
using System.Threading;

#endregion

namespace MBN.Modules
{
    /// <summary>
    ///     Main class for the TempHum10 driver
    ///     <para><b>Pins used :</b> Scl, Sda and Cs</para>
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
    ///     internal class Program
    ///     {
    ///         private static TempHum10Click _sensor;
    ///
    ///         private static void Main()
    ///         {
    ///             _sensor = new TempHum10Click(Hardware.SocketOne)
    ///             {
    ///                 TemperatureUnits = TemperatureUnits.Kelvin,
    ///                 HumiditySamplingRate = TempHum10Click.HumiditySampling.EightTimes,
    ///                 TemperatureSamplingRate = TempHum10Click.TemperatureSampling.SixteenTimes
    ///             };
    ///
    ///             for (;;)
    ///             {
    ///                 Debug.WriteLine($"Temperature.....: {_sensor.ReadTemperature():F2} °K");
    ///                 Debug.WriteLine($"Humidity........: {_sensor.ReadHumidity():F2} %RH");
    ///                 Thread.Sleep(2000);
    ///             }
    ///         }
    ///     }
    /// }
    /// </code>
    /// </example>
    public sealed class TempHum10Click : ITemperature, IHumidity
    {
        #region .ctor

        /// <summary>
        ///     Initializes a new instance of the <see cref="TempHum10Click" /> class.
        /// </summary>
        /// <param name="socket">The socket on which the TempHum10 module is plugged on MikroBus.Net board</param>
        public TempHum10Click(Hardware.Socket socket)
        {
            _sensor = I2cController.FromName(socket.I2cBus).GetDevice(new I2cConnectionSettings(0x7F, 100000));

            _chipEnable = GpioController.GetDefault().OpenPin(socket.Cs);
            _chipEnable.SetDriveMode(GpioPinDriveMode.Output);
            _chipEnable.Write(GpioPinValue.Low);

            PowerMode = PowerModes.On;

            Thread.Sleep(1); // Need some time to wake up sensor.

            Reset();
        }

        #endregion

        #region Public Methods

        /// <summary>
        ///     Resets the module
        /// </summary>
        /// <example>
        ///     Example usage:
        ///     <code language="C#">
        /// 
        /// </code>
        /// </example>
        public void Reset()
        {
            WriteRegister(BPS230_REG_DEVICE_RESET, 0x01);

            Byte registerData;
            do
            {
                registerData = ReadRegister(BPS230_REG_DEVICE_RESET)[0];
            } while ((registerData & 0x01) == 0x01);
        }

        #endregion

        #region Private Fields

        private readonly I2cDevice _sensor;
        private readonly GpioPin _chipEnable;

        #endregion

        #region Constants

        private const Byte BPS230_REG_DEVICE_RESET = 0x00;
        private const Byte BPS230_REG_CONFIG = 0x01;
        private const Byte BPS_REG_ERR = 0x03;
        private const Byte BPS230_REG_HUMIDITY_LSB = 0x04;
        private const Byte BPS230_REG_TEMPERATURE_LSB = 0x06;

        #endregion

        #region Public ENUMS

        /// <summary>
        ///     The number of samples taken for humidity measurement.
        /// </summary>
        public enum HumiditySampling
        {
            /// <summary>
            ///     No additional samples taken, humidity reported is for  single sample.
            /// </summary>
            NoAveraging = 0x00,

            /// <summary>
            ///     Two (2) samples are taken for humidity measurement.
            /// </summary>
            TwoTimes = 0x01,

            /// <summary>
            ///     Four (4) samples are taken for humidity measurement.
            /// </summary>
            FourTimes = 0x02,

            /// <summary>
            ///     Eight (8) samples are taken for humidity measurement.
            /// </summary>
            EightTimes = 0x04
        }

        /// <summary>
        ///     The number of samples taken for temperature measurement.
        /// </summary>
        public enum TemperatureSampling
        {
            /// <summary>
            ///     Eight (8) samples are taken for averaging temperature measurement.
            /// </summary>
            EightTimes = 0x00,

            /// <summary>
            ///     Sixteen (16) samples are taken for averaging temperature measurement.
            /// </summary>
            SixteenTimes = 0x01
        }

        #endregion

        #region Public Properties

        /// <summary>
        ///     Gets or sets the power mode.
        /// </summary>
        /// <example>
        ///     This sample shows how to use the PowerMode property.
        ///     <code language="C#">
        /// _sensor.PowerMode = PowerModes.Low;
        /// </code>
        /// </example>
        /// <value>
        ///     The current power mode of the module.
        /// </value>
        public PowerModes PowerMode
        {
            get => _chipEnable.Read() == GpioPinValue.High ? PowerModes.On : PowerModes.Low;
            set
            {
                if (value == PowerModes.Off) throw new NotSupportedException("This module does not support PowerModes.Off");
                _chipEnable.Write(value == PowerModes.On ? GpioPinValue.High : GpioPinValue.Low);
            }
        }

        /// <summary>
        ///     Gets or sets the number of samples used for averaging humidity measurements.
        /// </summary>
        /// <example>
        ///     Example usage:
        ///     <code language="C#">
        /// _sensor.HumiditySamplingRate = TempHum10.HumiditySampling.EightTimes;
        /// Console.WriteLine("Humidity SamplingRate is " + _sensor.HumiditySamplingRate);
        /// </code>
        /// </example>
        public HumiditySampling HumiditySamplingRate
        {
            get
            {
                Byte[] registerData = ReadRegister(BPS230_REG_CONFIG);
                return (HumiditySampling) (registerData[0] >> 3);
            }
            set
            {
                Byte[] registerData = ReadRegister(BPS230_REG_CONFIG);
                registerData[0] &= 0xC7;
                registerData[0] |= (Byte) ((Byte) value << 3);
                WriteRegister(BPS230_REG_CONFIG, registerData[0]);
            }
        }

        /// <summary>
        ///     Gets or sets the number of samples used for averaging temperature measurements.
        /// </summary>
        /// <example>
        ///     Example usage:
        ///     <code language="C#">
        /// _sensor.TemperatureSamplingRate = TempHum10.TemperatureSampling.SixteenTimes;
        /// Console.WriteLine("Temperature SamplingRate is " + _sensor.TemperatureSamplingRate);
        /// </code>
        /// </example>
        public TemperatureSampling TemperatureSamplingRate
        {
            get
            {
                Byte[] registerData = ReadRegister(BPS230_REG_CONFIG);
                return (TemperatureSampling) (registerData[0] >> 2);
            }
            set
            {
                Byte[] registerData = ReadRegister(BPS230_REG_CONFIG);
                registerData[0] &= 0xFB;
                registerData[0] |= (Byte) ((Byte) value << 2);
                WriteRegister(BPS230_REG_CONFIG, registerData[0]);
            }
        }

        /// <summary>
        ///     Returns whether the last measurement erred or was successful.
        /// </summary>
        /// <example>
        ///     Example usage:
        ///     <code language="C#">
        /// Console.WriteLine("Measurement error? " + _sensor.HasError);
        /// </code>
        /// </example>
        public Boolean HasError
        {
            get
            {
                Byte registerData = ReadRegister(BPS_REG_ERR)[0];
                return (registerData & 0x01) == 0x01;
            }
        }

        /// <summary>
        ///     Gets or sets the <see cref="TemperatureUnits" /> used for temperature measurements.
        /// </summary>
        /// <example>
        ///     Example usage:
        ///     <code language="C#">
        /// _sensor.TemperatureUnits = TemperatureUnits.Kelvin;
        /// </code>
        /// </example>
        public TemperatureUnits TemperatureUnits { get; set; }

        #endregion

        #region Private Methods

        private void WriteRegister(Byte registerAddress, Byte data)
        {
            lock (Hardware.LockI2C)
            {
                _sensor.Write(new[] { registerAddress, data });
            }
        }

        private Byte[] ReadRegister(Byte registerAddress, Byte numberOfBytesToRead = 1)
        {
            Byte[] readBuffer = new Byte[numberOfBytesToRead];

            lock (Hardware.LockI2C)
            {
                _sensor.WriteRead(new[] { registerAddress }, readBuffer);
            }

            return readBuffer;
        }

        private void TriggerMeasurement()
        {
            Byte[] registerData = ReadRegister(BPS230_REG_CONFIG);

            registerData[0] |= 0x01;

            WriteRegister(BPS230_REG_CONFIG, registerData[0]);
        }

        private Boolean Measuring()
        {
            return (ReadRegister(BPS230_REG_CONFIG)[0] & 0x01) == 0x01;
        }

        #endregion

        #region Interface Implementations

        /// <inheritdoc cref="IHumidity" />
        /// <summary>
        ///     Reads the relative humidity value from the sensor.
        /// </summary>
        /// <returns>
        ///     A single representing the relative humidity as read from the sensor expressed as in percentage (%) relative
        ///     humidity.
        /// </returns>
        /// <example>
        ///     Example usage:
        ///     <code language="C#">
        /// Console.WriteLine("Humidity........: " + _sensor.ReadHumidity().ToString("F2") + " %RH");
        /// </code>
        /// </example>
        public Single ReadHumidity(HumidityMeasurementModes measurementMode = HumidityMeasurementModes.Relative)
        {
            if (measurementMode == HumidityMeasurementModes.Absolute)
                throw new ArgumentException("This module does not support reading object temperature.",
                    nameof(measurementMode));

            if (PowerMode == PowerModes.Low) return Single.MinValue;

            return 100.0F / 1024.0F * (this as IHumidity).RawData;
        }

        /// <inheritdoc cref="ITemperature" />
        /// <summary>Reads the temperature.</summary>
        /// <param name="source">The source.</param>
        /// <returns>A single representing the temperature read from the source, degrees Celsius</returns>
        /// <example>
        ///     Example usage:
        ///     <code language="C#">
        /// Console.WriteLine("Temperature.....: " + _sensor.ReadTemperature().ToString("F2") + " °K");
        /// </code>
        /// </example>
        public Single ReadTemperature(TemperatureSources source = TemperatureSources.Ambient)
        {
            if (source == TemperatureSources.Object)
                throw new ArgumentException("This module does not support reading object temperature.", nameof(source));

            if (PowerMode == PowerModes.Low) return Single.MinValue;

            Single temperature = ((this as ITemperature).RawData - 774.0F) * 0.1F;

            switch (TemperatureUnits)
            {
                case TemperatureUnits.Celsius:
                {
                    return temperature;
                }

                case TemperatureUnits.Fahrenheit:
                {
                    return temperature * 9f / 5f + 32f;
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

        public TemperatureUnits TemperatureUnit { get; set; } = TemperatureUnits.Celsius;

        /// <inheritdoc cref="IHumidity" />
        /// <summary>
        ///     Gets the raw data of the humidity value.
        /// </summary>
        /// <value>
        ///     Raw data in the range depending on sensor's precision (8/10/12 bits, for example)
        /// </value>
        /// <example>
        ///     Example usage:
        ///     <code language="C#">
        /// Console.WriteLine("Raw Humidity Data is " + (_sensor as IHumidity).RawData);
        /// </code>
        /// </example>
        Int32 IHumidity.RawData
        {
            get
            {
                if (PowerMode == PowerModes.Low) return Int32.MinValue;

                TriggerMeasurement();
                while (Measuring()) { Thread.Sleep(5); }
                Byte[] registerData = ReadRegister(BPS230_REG_HUMIDITY_LSB, 2);
                return (registerData[1] << 8) | registerData[0];
            }
        }

        /// <inheritdoc cref="ITemperature" />
        /// <summary>Gets the raw data of the temperature value.</summary>
        /// <value>
        ///     Raw data in the range depending on sensor's precision (8/10/12 bits, for example)
        /// </value>
        /// <example>
        ///     Example usage:
        ///     <code language="C#">
        /// Console.WriteLine("Raw Humidity Data is " + (_sensor as ITemperature).RawData);
        /// </code>
        /// </example>
        Int32 ITemperature.RawData
        {
            get
            {
                if (PowerMode == PowerModes.Low) return Int32.MinValue;

                TriggerMeasurement();
                while (Measuring()) { Thread.Sleep(5);}
                Byte[] registerData = ReadRegister(BPS230_REG_TEMPERATURE_LSB, 2);
                return (registerData[1] << 8) | registerData[0];
            }
        }

        #endregion
    }
}