/*
 * Pressure 4 driver for TinyCLR 2.0
 *
 * Version 1.0 by Stephen Cardinale 
 *
 * Copyright 2020 Stephen Cardinale and MikroBus.Net
 * Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License. You may obtain a copy of the License at
 * http://www.apache.org/licenses/LICENSE-2.0
 * Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, 
 * either express or implied. See the License for the specific language governing permissions and limitations under the License.
 * 
 */

#region Usings

#if (NANOFRAMEWORK_1_0)
using System.Device.Gpio;
using System.Device.I2c;
using System.Device.Spi;
#else
using GHIElectronics.TinyCLR.Devices.Gpio;
using GHIElectronics.TinyCLR.Devices.I2c;
using GHIElectronics.TinyCLR.Devices.Spi;
#endif

using System;
using System.Threading;

#endregion

namespace MBN.Modules
{
    /// <inheritdoc cref="IPressure" />
    /// <inheritdoc cref="ITemperature" />
    /// <summary>
    ///     Main class for the Pressure4Click driver
    ///     <para><b>Pins used :</b> SPI - Mosi, Miso, Sck and Cs or for I2C - Scl, Sda</para>
    ///     <para><b>This is a SPI/I2C device and driver.</b></para>
    /// </summary>
    /// <example>Example usage:
    ///<code language="C#">
    /// using System;
    /// using System.Diagnostics;
    /// using System.Threading;
    /// using MBN;
    /// using MBN.Modules;
    ///
    /// namespace Examples
    /// {
    ///     public static class Program
    ///     {
    ///         private static Pressure4Click _sensor;
    ///
    ///         public static void Main()
    ///         {
    ///             Debug.WriteLine("Program started");
    ///
    ///             // Using the SPI Interface
    ///             //_sensor = new Pressure4Click(Hardware.SC20100_1)
    ///             //{
    ///             //    PressureCompensation = PressureCompensationModes.SeaLevelCompensated,
    ///             //    TemperatureUnit = TemperatureUnits.Fahrenheit
    ///             //};
    ///
    ///             // Using the I2C Interface
    ///             _sensor = new Pressure4Click(Hardware.SC20100_1, Pressure4Click.I2CAddress.I2CAddress0)
    ///             {
    ///                 PressureCompensation = PressureCompensationModes.SeaLevelCompensated,
    ///                 TemperatureUnit = TemperatureUnits.Fahrenheit
    ///             };
    ///
    ///             Debug.WriteLine($"Device ID is {_sensor.DeviceId}");
    ///
    ///             // Set recommended mode using SetRecemmondedMode method.
    ///             _sensor.SetRecommendedMode(Pressure4Click.RecommendedModes.WeatherMonitoring);
    ///
    ///             // Or set individual parameters as in below. Note: You must call EnableSettings() method to enforce the user settings.
    ///             //_sensor.PressureSamplingRate = Pressure4Click.OversamplingRates.Osr1;
    ///             //_sensor.TemperatureSamplingRate = Pressure4Click.OversamplingRates.Osr1;
    ///             //_sensor.Filter = Pressure4Click.FilterCoefficient.IIROff;
    ///             //_sensor.StandbyDuration = Pressure4Click.StandbyDurations.MS_0_5;
    ///             //_sensor.OperatingMode = Pressure4Click.Mode.Sleep;
    ///             //_sensor.EnableSettings();
    ///
    ///             while (true)
    ///             {
    ///                 _sensor.ReadSensor(out Single pressure, out Single temperature, out Single altitude);
    ///
    ///                 Debug.WriteLine($"Pressure.......: {pressure:F1} hPa");
    ///                 Debug.WriteLine($"Temperature....: {temperature:F2} °F");
    ///                 Debug.WriteLine($"Altitude.......: {altitude:F0} meters\n");
    ///
    ///                 Thread.Sleep(2000);
    ///             }
    ///         }
    ///     }
    /// }
    ///  </code>
    /// </example>
    public sealed class Pressure4Click : IPressure, ITemperature
    {
        #region CTOR

        /// <summary>
        ///     Initializes a new instance of the <see cref="Pressure4Click" /> class using the SPI interface.
        /// </summary>
        /// <param name="socket">The virtual socket on which the Pressure4Click module is connected to on the MikroBus.Net board</param>
        public Pressure4Click(Hardware.Socket socket)
        {
            _socket = socket;
            _interface = Interface.SPI;

#if (NANOFRAMEWORK_1_0)
            _sensorSPI = SpiDevice.Create(new SpiConnectionSettings(socket.SpiBus, socket.Cs)
            {
                Mode = SpiMode.Mode0,
                ClockFrequency = 8 * 1000 * 1000
            });
#else
            _sensorSPI = SpiController.FromName(socket.SpiBus).GetDevice(new SpiConnectionSettings()
            {
                ChipSelectType = SpiChipSelectType.Gpio,
                ChipSelectLine = GpioController.GetDefault().OpenPin(socket.Cs),
                Mode = SpiMode.Mode0,
                ClockFrequency = 8 * 1000 * 1000
            });
#endif

            Initialize();
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="Pressure4Click" /> class using the I2C interface.
        /// </summary>
        /// <param name="socket">The virtual socket on which the Pressure4Click module is connected to on the MikroBus.Net board</param>
        /// <param name="slaveAddress">The I2C Slave Address <see cref="I2CAddress"/> to use.</param>
        public Pressure4Click(Hardware.Socket socket, I2CAddress slaveAddress)
        {
            _interface = Interface.I2C;

#if (NANOFRAMEWORK_1_0)
            _sensorI2C = I2cDevice.Create(new I2cConnectionSettings(socket.I2cBus, (int)slaveAddress, I2cBusSpeed.FastMode));
#else
			_sensorI2C = I2cController.FromName(socket.I2cBus).GetDevice(new I2cConnectionSettings((Int32) slaveAddress, 400000));
#endif

            Initialize();
        }

        #endregion

        #region Constants

        private const Byte REG_ID = 0xD0;
        private const Byte REG_RESET = 0xE0;
        private const Byte REG_STATUS = 0xF3;
        private const Byte REG_CTRL_MEAS = 0xF4;
        private const Byte REG_CONFIG = 0xF5;
        private const Byte REG_READ_PRESSURE = 0xF7;
        private const Byte REG_READ_TEMPERATURE = 0xFA;
        private const Byte CALIBRATION_REG_START = 0x88;

        #endregion

        #region ENUMS

        /// <summary>
        ///     Oversampling rates
        /// </summary>
        public enum OversamplingRates : byte
        {
            /// <summary>
            ///     No oversampling, output is set to 0x8000
            /// </summary>
            Skipped = 0,

            /// <summary>
            ///     Oversampling x 1 with 16 bit temperature resolution (0.0050 °C) and 16 bit pressure resolution (2.62 Pa)
            /// </summary>
            Osr1 = 1,

            /// <summary>
            ///     Oversampling x 2 with 17 bit temperature resolution (0.0025 °C) and 17 bit pressure resolution (1.31 Pa)
            /// </summary>
            Osr2 = 2,

            /// <summary>
            ///     Oversampling x 4 with 18 bit temperature resolution (0.0012 °C) and 18 bit pressure resolution (0.66 Pa)
            /// </summary>
            Osr4 = 3,

            /// <summary>
            ///     Oversampling x 8 with 19 bit temperature resolution (0.0006 °C) and 19 bit pressure resolution (0.33 Pa)
            /// </summary>
            Osr8 = 4,

            /// <summary>
            ///     Oversampling x 16 with 20 bit temperature resolution (0.0003 °C) and 20 bit pressure resolution (0.16 Pa)
            /// </summary>
            Osr16 = 5
        }

        /// <summary>
        ///     Infinite Impulse Response Filter Coefficient.
        /// </summary>
        public enum FilterCoefficient : byte
        {
            /// <summary>
            ///     IIR Filter is off.
            /// </summary>
            IIROff = 0x00,

            /// <summary>
            ///     IIR Filter is 2.
            /// </summary>
            IIR2 = 0x01,

            /// <summary>
            ///     IIR Filter is 4.
            /// </summary>
            IIR4 = 0x02,

            /// <summary>
            ///     IIR Filter is 8.
            /// </summary>
            IIR8 = 0x03,

            /// <summary>
            ///     IIR Filter is 16.
            /// </summary>
            IIR16 = 0x04
        }

        /// <summary>
        ///     Inactive duration in normal mode
        /// </summary>
        public enum StandbyDurations : byte
        {
            /// <summary>
            ///     Standby time of 0.5 milliseconds between readings.
            /// </summary>
            MS_0_5 = 0x00,

            /// <summary>
            ///     Standby time of 62.5 milliseconds between readings.
            /// </summary>
            MS_62_5 = 0x01,

            /// <summary>
            ///     Standby time of 125 milliseconds between readings.
            /// </summary>
            MS_125 = 0x02,

            /// <summary>
            ///     Standby time of 250 milliseconds between readings.
            /// </summary>
            MS_250 = 0x03,

            /// <summary>
            ///     Standby time of 500 milliseconds between readings.
            /// </summary>
            MS_500 = 0x04,

            /// <summary>
            ///     Standby time of 1000 milliseconds between readings.
            /// </summary>
            MS_1000 = 0x05,

            /// <summary>
            ///     Standby time of 2000 milliseconds between readings.
            /// </summary>
            MS_2000 = 0x06,

            /// <summary>
            ///     Standby time of 4000 milliseconds between readings.
            /// </summary>
            MS_4000 = 0x07
        }

        /// <summary>
        ///     Preconfigured operating modes, as recommended by Bosch.
        /// </summary>
        public enum RecommendedModes
        {
            /// <summary>
            ///     Only a very low data rate is needed. Power consumption is minimal. Noise of pressure values is of no concern.
            ///     Humidity, pressure and temperature are monitored.
            ///     <para>
            ///         Current consumption 0.16 µA
            ///         RMS Noise 3.3 Pa / 30 cm, 0.07 %RH
            ///         Data output rate 1/60 Hz
            ///     </para>
            /// </summary>
            HandheldDeviceLowPower,

            /// <summary>
            ///     A low data rate is needed. Power consumption is minimal. Forced mode is used to minimize power consumption and to
            ///     synchronize readout, but using normal mode would also be possible.
            ///     <para>
            ///         Current consumption 2.9 µA
            ///         RMS Noise 0.07 %RH
            ///         Data output rate 1/60 Hz
            ///     </para>
            /// </summary>
            HandheldDeviceDynamic,

            /// <summary>
            ///     Lowest possible altitude noise is needed. A very low bandwidth is preferred. Increased power consumption  is
            ///     tolerated.  Humidity  is  measured  to  help  detect  room  changes.
            ///     <para>
            ///         Current consumption  633 µA
            ///         RMS Noise  0.2 Pa / 1.7 cm
            ///         Data output rate  25Hz
            ///         Filter bandwidth  0.53 Hz
            ///         Response time (75%)  0.9 s
            ///     </para>
            /// </summary>
            WeatherMonitoring,

            /// <summary>
            ///     Low altitude noise is needed. The required bandwidth is ~2 Hz in order to respond quickly to altitude  changes
            ///     (e.g.  be  able  to  dodge  a  flying  monster  in  a  game).
            ///     Increased  power consumption is tolerated. Humidity sensor is disabled.
            ///     <para>
            ///         Current consumption  581 µA
            ///         RMS Noise  0.3 Pa / 2.5 cm
            ///         Data output rate  83 Hz
            ///         Filter bandwidth  1.75 Hz
            ///         Response time (75%)  0.3 s
            ///     </para>
            /// </summary>
            FloorChangeDetection,

            /// <summary>
            ///     Maximal power consumption
            /// </summary>
            DropDetection,

            /// <summary>
            /// </summary>
            IndoorNavigation
        }

        /// <summary>
        /// The operating or power modes that the Pressure 4 Click supports.
        /// </summary>
        public enum Mode
        {
            /// <summary>
            /// In sleep mode, no measurements are performed and power consumption is at a minimum. All registers are accessible, Chip-ID and compensation coefficients can be read.
            /// </summary>
            Sleep = 0x00,

            /// <summary>
            /// In forced mode, a single measurement is performed according to selected measurement and filter options. When the measurement is finished, the sensor returns to sleep mode and the measurement results can be obtained from the data registers. For a next measurement, forced mode needs to be selected again.
            /// </summary>
            Forced = 0x01,

            /// <summary>
            /// Actually, reserved mode is a valid setting and corresponds to forced mode.
            /// </summary>
            Reserved = 0x02,

            /// <summary>
            /// Normal mode, continuously cycles between an (active) measurement period and an (inactive) standby period, whose time is defined by standby. The current in the standby period (IDDSB) is slightly higher than in sleep mode. After setting the mode,measurement and filter options, the last measurement results can be obtained from the data registers without the need of further write accesses.
            /// </summary>
            Normal = 0x03
        }

        /// <summary>
        /// The slave address to use when using the I2C communication bus.
        /// </summary>
        public enum I2CAddress
        {
            /// <summary>
            /// Address is 0x77 with the I2C Address Selector Jumper soldered in the 1 position.
            /// </summary>
            I2CAddress0 = 0x77,

            /// <summary>
            /// Address is 0x76 with the I2C Address Selector Jumper soldered in the 0 position.
            /// </summary>
            I2CAddress1 = 0x76
        }

        private enum Interface
        {
            SPI = 0,
            I2C = 1
        }

        #endregion

        #region Fields

        private readonly SpiDevice _sensorSPI;
        private readonly I2cDevice _sensorI2C;
        private readonly Interface _interface;
        private readonly Hardware.Socket _socket;
        private StandbyDurations _standbyDuration;
        private FilterCoefficient _filter;
        private Mode _powerMode;

        #region Calibration Fields

        private UInt16 _digT1;
        private Int16 _digT2, _digT3;
        private UInt16 _digP1;
        private Int16 _digP2, _digP3, _digP4, _digP5, _digP6, _digP7, _digP8, _digP9;

        #endregion

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets or sets the Operating Mode or Power Mode of the Pressure 4 Click.
        /// </summary>
        /// <example>Example usage:
        /// <code language = "C#">
        /// _sensor.PressureSamplingRate = Pressure4Click.OversamplingRates.Osr1;
        /// _sensor.TemperatureSamplingRate = Pressure4Click.OversamplingRates.Osr1;
        /// _sensor.Filter = Pressure4Click.FilterCoefficient.IIROff;
        /// _sensor.StandbyDuration = Pressure4Click.StandbyDurations.MS_0_5;
        /// _sensor.OperatingMode = Pressure4Click.Mode.Sleep;
        /// _sensor.EnableSettings();
        /// </code>
        /// </example>
        /// <remarks>It is necessary to call the <see cref="EnableSettings"/> method after setting any of the <see cref="OperatingMode"/>, <see cref="PressureSamplingRate"/>,
        /// <see cref="TemperatureSamplingRate"/>, <see cref="Filter"/> or <see cref="StandbyDuration"/> properties manually.</remarks>
        public Mode OperatingMode
        {
            get => _powerMode;
            set
            {
                if ((Byte) value > 3) value = Mode.Normal;
                _powerMode = value;
            }
        }

        /// <summary>
        /// Controls the time constant of the IIR filter.
        /// <p>
        /// Although this property is exposed, it is recommended against setting this property directly. Use the
        /// <see cref="SetRecommendedMode" /> method to avoid improper settings.
        /// </p>
        /// </summary>
        /// <value>
        /// The Filter Coefficient. See data sheet for the values associated to this coefficient.
        /// </value>
        /// <example>Example usage:
        /// <code language = "C#">
        /// _sensor.PressureSamplingRate = Pressure4Click.OversamplingRates.Osr1;
        /// _sensor.TemperatureSamplingRate = Pressure4Click.OversamplingRates.Osr1;
        /// _sensor.Filter = Pressure4Click.FilterCoefficient.IIROff;
        /// _sensor.StandbyDuration = Pressure4Click.StandbyDurations.MS_0_5;
        /// _sensor.OperatingMode = Pressure4Click.Mode.Sleep;
        /// _sensor.EnableSettings();
        /// </code>
        /// </example>
        /// <remarks>It is necessary to call the <see cref="EnableSettings"/> method after setting any of the <see cref="OperatingMode"/>, <see cref="PressureSamplingRate"/>,
        /// <see cref="TemperatureSamplingRate"/>, <see cref="Filter"/> or <see cref="StandbyDuration"/> properties manually.</remarks>
        public FilterCoefficient Filter
        {
            get => _filter;
            set
            {
                if ((Byte) value > 4) value = FilterCoefficient.IIR16;
                _filter = value;
            }
        }

        /// <summary>
        /// Gets or sets the inactive duration in normal mode.
        /// <p>Although this property is exposed, it is recommended against setting this property directly. Use the <see cref="SetRecommendedMode" /> method to avoid improper settings.</p>
        /// </summary>
        /// <value>The duration. See data sheet for the values (in ms) associated to this parameter</value>
        /// <example>Example usage:
        /// <code language = "C#">
        /// _sensor.PressureSamplingRate = Pressure4Click.OversamplingRates.Osr1;
        /// _sensor.TemperatureSamplingRate = Pressure4Click.OversamplingRates.Osr1;
        /// _sensor.Filter = Pressure4Click.FilterCoefficient.IIROff;
        /// _sensor.StandbyDuration = Pressure4Click.StandbyDurations.MS_0_5;
        /// _sensor.OperatingMode = Pressure4Click.Mode.Sleep;
        /// _sensor.EnableSettings();
        /// </code>
        /// </example>
        /// <remarks>It is necessary to call the <see cref="EnableSettings"/> method after setting any of the <see cref="OperatingMode"/>, <see cref="PressureSamplingRate"/>,
        /// <see cref="TemperatureSamplingRate"/>, <see cref="Filter"/> or <see cref="StandbyDuration"/> properties manually.</remarks>
        public StandbyDurations StandbyDuration
        {
            get => _standbyDuration;
            set
            {
                if ((Byte) value > 7) value = StandbyDurations.MS_4000;
                _standbyDuration = value;
            }
        }

        /// <summary>
        /// Gets or sets the temperature sampling rate.
        /// <p>Although this property is exposed, it is recommended against setting this property directly. Use the <see cref="SetRecommendedMode" /> method to avoid improper settings.</p>
        /// </summary>
        /// <value>
        /// The temperature sampling rate. See the <seealso cref="OversamplingRates" /> for oversampling rates.
        /// </value>
        /// <example>Example usage:
        /// <code language = "C#">
        /// _sensor.PressureSamplingRate = Pressure4Click.OversamplingRates.Osr1;
        /// _sensor.TemperatureSamplingRate = Pressure4Click.OversamplingRates.Osr1;
        /// _sensor.Filter = Pressure4Click.FilterCoefficient.IIROff;
        /// _sensor.StandbyDuration = Pressure4Click.StandbyDurations.MS_0_5;
        /// _sensor.OperatingMode = Pressure4Click.Mode.Sleep;
        /// _sensor.EnableSettings();
        /// </code>
        /// </example>
        /// <remarks>It is necessary to call the <see cref="EnableSettings"/> method after setting any of the <see cref="OperatingMode"/>, <see cref="PressureSamplingRate"/>,
        /// <see cref="TemperatureSamplingRate"/>, <see cref="Filter"/> or <see cref="StandbyDuration"/> properties manually.</remarks>
        public OversamplingRates TemperatureSamplingRate { get; set; } = OversamplingRates.Osr1;

        /// <summary>
        /// Gets or sets the pressure sampling rate.
        /// <p>Although this property is exposed, it is recommended against setting this property directly. Use the <see cref="SetRecommendedMode" /> method to avoid improper settings.</p>
        /// </summary>
        /// <value>The pressure sampling rate. See the <seealso cref="OversamplingRates" /> for oversampling rates.</value>
        /// <example>Example usage:
        /// <code language = "C#">
        /// _sensor.PressureSamplingRate = Pressure4Click.OversamplingRates.Osr1;
        /// _sensor.TemperatureSamplingRate = Pressure4Click.OversamplingRates.Osr1;
        /// _sensor.Filter = Pressure4Click.FilterCoefficient.IIROff;
        /// _sensor.StandbyDuration = Pressure4Click.StandbyDurations.MS_0_5;
        /// _sensor.OperatingMode = Pressure4Click.Mode.Sleep;
        /// _sensor.EnableSettings();
        /// </code>
        /// </example>
        /// <remarks>It is necessary to call the <see cref="EnableSettings"/> method after setting any of the <see cref="OperatingMode"/>, <see cref="PressureSamplingRate"/>,
        /// <see cref="TemperatureSamplingRate"/>, <see cref="Filter"/> or <see cref="StandbyDuration"/> properties manually.</remarks>
        public OversamplingRates PressureSamplingRate { get; set; } = OversamplingRates.Osr1;

        /// <summary>
        ///     Gets the identifier of the chip.
        /// </summary>
        /// <value>
        ///     Should be 0x58. Other value means error.
        /// </value>
        /// <example>
        ///     Example usage:
        ///     <code language="C#">
        /// Debug.Print("Device ID is " + _sensor.DeviceId);
        /// </code>
        /// </example>
        public Byte DeviceId => ReadRegister(REG_ID)[0];

        /// <summary>
        /// Gets or sets the pressure compensation mode for one-shot pressure measurements.
        /// </summary>
        /// <example>Example usage:
        /// <code language = "C#">
        /// _sensor.PressureCompensation = PressureCompensationModes.SeaLevelCompensated;
        /// </code>
        /// </example>
        public PressureCompensationModes PressureCompensation { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="TemperatureUnits"/> used for temperature measurements.
        /// </summary>
        /// <example>
        /// Example usage:
        /// <code language="C#">
        /// _sensor.TemperatureUnits = TemperatureUnits.Kelvin;
        /// </code>
        /// </example>
        public TemperatureUnits TemperatureUnit { get; set; }

        #endregion

        #region Public Methods

        /// <summary>
        ///     Sets operating mode according to Bosch's recommended modes of operation.
        /// </summary>
        /// <param name="mode">The desired mode.</param>
        /// <example>
        ///     Example usage:
        ///     <code language="C#">
        /// _sensor.SetRecommendedMode(Pressure4Click.RecommendedModes.WeatherMonitoring);
        /// </code>
        /// </example>
        public void SetRecommendedMode(RecommendedModes mode)
        {
            switch (mode)
            {
                case RecommendedModes.HandheldDeviceLowPower:
                {
                    Filter = FilterCoefficient.IIR4;
                    StandbyDuration = StandbyDurations.MS_62_5;
                    PressureSamplingRate = OversamplingRates.Osr16;
                    TemperatureSamplingRate = OversamplingRates.Osr2;
                    OperatingMode = Mode.Normal;
                    break;
                }
                case RecommendedModes.HandheldDeviceDynamic:
                {
                    Filter = FilterCoefficient.IIR16;
                    StandbyDuration = StandbyDurations.MS_0_5;
                    PressureSamplingRate = OversamplingRates.Osr4;
                    TemperatureSamplingRate = OversamplingRates.Osr1;
                    OperatingMode = Mode.Normal;
                    break;
                }
                case RecommendedModes.WeatherMonitoring:
                {
                    Filter = FilterCoefficient.IIROff;
                    StandbyDuration = StandbyDurations.MS_0_5;
                    PressureSamplingRate = OversamplingRates.Osr1;
                    TemperatureSamplingRate = OversamplingRates.Osr1;
                    OperatingMode = Mode.Sleep;
                    break;
                }
                case RecommendedModes.FloorChangeDetection:
                {
                    Filter = FilterCoefficient.IIR4;
                    StandbyDuration = StandbyDurations.MS_125;
                    PressureSamplingRate = OversamplingRates.Osr4;
                    TemperatureSamplingRate = OversamplingRates.Osr1;
                    OperatingMode = Mode.Normal;
                    break;
                }
                case RecommendedModes.DropDetection:
                {
                    Filter = FilterCoefficient.IIROff;
                    StandbyDuration = StandbyDurations.MS_0_5;
                    PressureSamplingRate = OversamplingRates.Osr2;
                    TemperatureSamplingRate = OversamplingRates.Osr1;
                    OperatingMode = Mode.Normal;
                    break;
                }
                case RecommendedModes.IndoorNavigation:
                {
                    Filter = FilterCoefficient.IIR16;
                    StandbyDuration = StandbyDurations.MS_0_5;
                    PressureSamplingRate = OversamplingRates.Osr16;
                    TemperatureSamplingRate = OversamplingRates.Osr2;
                    OperatingMode = Mode.Normal;
                    break;
                }

                default:
                {
                    throw new ArgumentOutOfRangeException(nameof(mode));
                }
            }

            SetCTRL_MEAS();
        }

        /// <summary>
        /// Enables the user defined settings for <see cref="TemperatureSamplingRate"/>, <see cref="PressureSamplingRate"/>, <see cref="Filter"/>, <see cref="StandbyDuration"/> and <see cref="OperatingMode"/> when not using the <see cref="SetRecommendedMode"/> method.
        /// </summary>
        /// <example>Example usage:
        /// <code language = "C#">
        /// _sensor.PressureSamplingRate = Pressure4Click.OversamplingRates.Osr1;
        /// _sensor.TemperatureSamplingRate = Pressure4Click.OversamplingRates.Osr1;
        /// _sensor.Filter = Pressure4Click.FilterCoefficient.IIROff;
        /// _sensor.StandbyDuration = Pressure4Click.StandbyDurations.MS_0_5;
        /// _sensor.OperatingMode = Pressure4Click.Mode.Sleep;
        /// _sensor.EnableSettings();
        /// </code>
        /// </example>
        public void EnableSettings()
        {
            SetCTRL_MEAS();
        }

        /// <summary>
        /// Reads the Temperature and Pressure from the Pressure 4 Click. Additionally, provides calculated Altitude.
        /// </summary>
        /// <remarks>As pressure measurement is dependent on temperature, this method reads both temperature and pressure data on the same conversion to ensure data integrity.</remarks>
        /// <param name="pressure">The referenced <c>out</c> parameter that will hold the pressure value.</param>
        /// <param name="temperature">The referenced <c>out</c> parameter that will hold the temperature value.</param>
        /// <param name="altitude">The referenced <c>out</c> parameter that will hold the altitude value.</param>
        /// <example>Example usage:
        /// <code language = "C#">
        /// _sensor.SetRecommendedMode(Pressure4Click.RecommendedModes.HandheldDeviceLowPower);
        /// 
        /// while (true)
        /// {
        ///     Single pressure, temperature, altitude;
        ///
        ///    _sensor.ReadSensor(out pressure, out temperature, out altitude);
        ///
        ///     Debug.Print("Pressure.......: " + pressure.ToString("F1") + " hPa");
        ///     Debug.Print("Temperature....: " + temperature.ToString("F2") + " °K");
        ///     Debug.Print("Altitude.......: " + altitude.ToString("F0") + " meters");
        ///
        ///     Thread.Sleep(5000);
        /// }
        /// </code>
        /// </example>
        public void ReadSensor(out Single pressure, out Single temperature, out Single altitude)
        {
            if (TemperatureSamplingRate == OversamplingRates.Skipped || PressureSamplingRate == OversamplingRates.Skipped)
            {
                pressure = Single.MinValue;
                temperature = Single.MinValue;
                altitude = Single.MinValue;
            }

            if (_powerMode == Mode.Sleep) EnableForcedMeasurement();

            Int32 adc = (this as ITemperature).RawData;
            Int32 var1 = (((adc >> 3) - (_digT1 << 1)) * _digT2) >> 11;
            Int32 var2 = (((((adc >> 4) - _digT1) * ((adc >> 4) - _digT1)) >> 12) * _digT3) >> 14;
            Int32 tFine = var1 + var2;

            Single temp = ((tFine * 5 + 128) >> 8) / 100.0F;

            switch (TemperatureUnit)
            {
                case TemperatureUnits.Celsius:
                {
                    temperature = temp;
                    break;
                }
                case TemperatureUnits.Fahrenheit:
                {
                    temperature = temp * 1.8F + 32;
                    break;
                }
                case TemperatureUnits.Kelvin:
                {
                    temperature = temp + 273.15F;
                    break;
                }
                default:
                {
                    throw new ArgumentOutOfRangeException();
                }
            }

            adc = (this as IPressure).RawData;
            Int64 var3 = (Int64) tFine - 128000;
            Int64 var4 = var3 * var3 * _digP6;
            var4 += (var3 * _digP5) << 17;
            var4 += (Int64) _digP4 << 35;
            var3 = ((var3 * var3 * _digP3) >> 8) + ((var3 * _digP2) << 12);
            var3 = ((((Int64) 1 << 47) + var3) * _digP1) >> 33;

            if (var3 == 0)
            {
                pressure =  Single.MinValue; // avoid exception caused by division by zero
                altitude = Single.MinValue;
                goto end;
            }

            Int64 uncompensatedPressure = 1048576 - adc;
            uncompensatedPressure = ((uncompensatedPressure << 31) - var4) * 3125 / var3;
            var3 = (_digP9 * (uncompensatedPressure >> 13) * (uncompensatedPressure >> 13)) >> 25;
            var4 = (_digP8 * uncompensatedPressure) >> 19;
            uncompensatedPressure = ((uncompensatedPressure + var3 + var4) >> 8) + ((Int64) _digP7 << 4);
            uncompensatedPressure /= 256;

            Single compensatedPressure = CalculatePressureAsl(uncompensatedPressure);

            pressure = PressureCompensation == PressureCompensationModes.Uncompensated
                ? uncompensatedPressure / 100.0F
                : compensatedPressure / 100.0F;

            altitude = (Int32) Math.Round(44330 * (1.0 - Math.Pow(uncompensatedPressure / compensatedPressure, 0.1903)));

            end:;
        }

        /// <summary>
        ///     Resets the module
        /// </summary>
        /// <example>
        ///     Example usage:
        ///     <code language="C#">
        /// _sensor.Reset(ResetModes.Soft);
        /// </code>
        /// </example>
        public Boolean Reset()
        {
            WriteByte(REG_RESET, 0xB6);

            do
            {
                Thread.Sleep(10);
            } while (ReadRegister(REG_CONFIG)[0] != 0);

            return ReadRegister(REG_CONFIG)[0] == 0;
        }

        #endregion

        #region Private Methods

        private Boolean Measuring => (ReadRegister(REG_STATUS)[0] & 0x08) == 0x08;

        private Boolean UpdatingNvm => (ReadRegister(REG_STATUS)[0] & 0x01) == 0x01;

        private void ReadCalibrationData()
        {
            Byte[] rawData = ReadRegister(CALIBRATION_REG_START, 24); // Temperature & Pressure

            // Temperature
            _digT1 = (UInt16) ((rawData[1] << 8) | rawData[0]);
            _digT2 = (Int16) ((rawData[3] << 8) | rawData[2]);
            _digT3 = (Int16) ((rawData[5] << 8) | rawData[4]);

            // Pressure
            _digP1 = (UInt16) ((rawData[7] << 8) | rawData[6]);
            _digP2 = (Int16) ((rawData[9] << 8) | rawData[8]);
            _digP3 = (Int16) ((rawData[11] << 8) | rawData[10]);
            _digP4 = (Int16) ((rawData[13] << 8) | rawData[12]);
            _digP5 = (Int16) ((rawData[15] << 8) | rawData[14]);
            _digP6 = (Int16) ((rawData[17] << 8) | rawData[16]);
            _digP7 = (Int16) ((rawData[19] << 8) | rawData[18]);
            _digP8 = (Int16) ((rawData[21] << 8) | rawData[20]);
            _digP9 = (Int16) ((rawData[23] << 8) | rawData[22]);
        }

        private static Single CalculatePressureAsl(Single uncompensatedPressure)
        {
            Single seaLevelCompensation = (Single) (101325F * Math.Pow((288 - 0.0065 * 143) / 288, 5.256));
            return 101325 + uncompensatedPressure - seaLevelCompensation;
        }

        private void SetCTRL_MEAS()
        {
            Byte registerData = (Byte) (((Byte) _standbyDuration << 5) | ((Byte) _filter << 2));
            WriteByte(REG_CONFIG, registerData);

            registerData = (Byte) (((Byte) TemperatureSamplingRate << 5) | ((Byte) PressureSamplingRate << 2) | (Byte) _powerMode);
            WriteByte(REG_CTRL_MEAS, registerData);
        }

        private Byte[] ReadRegister(Byte registerAddress, Byte bytesToRead = 1)
        {
            Byte[] result = new Byte[bytesToRead];

            if (_interface== Interface.SPI)
            {
                lock (_socket.LockSpi)
                {
#if (NANOFRAMEWORK_1_0)
                    _sensorSPI.WriteByte(registerAddress);
                    _sensorSPI.Read(result);
                    //_sensorSPI.TransferSequential(new[] { registerAddress }, result); //TODO: this might need reverting if the lib ever supports it!
#else
                    _sensorSPI.TransferSequential(new[] { registerAddress }, result);
#endif
                }
            }
            else
            {
                lock (_socket.LockI2c)
                {
                    _sensorI2C.WriteRead(new[] {registerAddress}, result);
                }
            }
            return result;
        }

        private void WriteByte(Byte registerAddress, Byte data)
        {
            if (_interface == Interface.SPI)
            {
                lock (_socket.LockSpi)
                {
                    _sensorSPI.Write(new[] { (Byte)(registerAddress & 0x7F), data });
                }
            }
            else
            {
                lock (_socket.LockI2c)
                {
                    _sensorI2C.Write(new[] { registerAddress, data });
                }
            }
        }

        private void Initialize()
        {
            /* Maximum number of tries before timeout */
            Byte tryCount = 20;

            while (tryCount > 0)
            {
                Byte result = ReadRegister(REG_ID)[0];

                /* Check for chip id validity */
                if (result == 0x58)
                {
                    Boolean reset = Reset();

                    if (reset)
                    {
                        ReadCalibrationData();
                    }

                    break;
                }

                /* Wait for 10 ms */
                Thread.Sleep(10);

                tryCount--;
            }

            /* Chip id check failed and/or timed out */
            if (tryCount == 0)
            {
                throw new DeviceInitialisationException("Error initializing the Pressure 4 Click.");
            }

            PressureCompensation = PressureCompensationModes.SeaLevelCompensated;
            TemperatureUnit = TemperatureUnits.Celsius;
            TemperatureSamplingRate = OversamplingRates.Skipped;
            PressureSamplingRate = OversamplingRates.Skipped;
            OperatingMode = Mode.Sleep;
            Filter = FilterCoefficient.IIROff;
            StandbyDuration = StandbyDurations.MS_0_5;
            SetCTRL_MEAS();
        }

        private void EnableForcedMeasurement()
        {
            Byte registerData = ReadRegister(REG_CTRL_MEAS)[0];
            registerData &= 0xFC;
            registerData |= 0x02;
            WriteByte(REG_CTRL_MEAS, registerData);
        }

        #endregion

        #region Interface Implementations

        /// <inheritdoc cref = "IPressure"/>
        /// <summary>
        ///     Reads the pressure from the sensor.
        /// </summary>
        /// <param name="compensationMode">
        ///     Indicates if the pressure reading returned by the sensor is see-level compensated or
        ///     not.
        /// </param>
        /// <returns>
        ///     A <see cref="T:System.Single" /> representing the pressure read from the source, in hPa (hectoPascal)
        /// </returns>
        /// <example>
        ///     Example usage:
        ///     <code language="C#">
        /// Debug.Print("Pressure is " + _sensor.ReadPressure(PressureCompensationModes.Uncompensated).ToString("F1") + " mBar");
        /// </code>
        /// </example>
        public Single ReadPressure(PressureCompensationModes compensationMode = PressureCompensationModes.SeaLevelCompensated)
        {
            if (TemperatureSamplingRate == OversamplingRates.Skipped || PressureSamplingRate == OversamplingRates.Skipped) return Single.MaxValue;

            if (_powerMode == Mode.Sleep) EnableForcedMeasurement();

            Int32 adc = (this as ITemperature).RawData;
            Int32 var1 = (((adc >> 3) - (_digT1 << 1)) * _digT2) >> 11;
            Int32 var2 = (((((adc >> 4) - _digT1) * ((adc >> 4) - _digT1)) >> 12) * _digT3) >> 14;
            Int32 tFine = var1 + var2;

            adc = (this as IPressure).RawData;
            Int64 var3 = (Int64) tFine - 128000;
            Int64 var4 = var3 * var3 * _digP6;
            var4 += (var3 * _digP5) << 17;
            var4 += (Int64) _digP4 << 35;
            var3 = ((var3 * var3 * _digP3) >> 8) + ((var3 * _digP2) << 12);
            var3 = ((((Int64) 1 << 47) + var3) * _digP1) >> 33;

            if (var3 == 0) return 0; // avoid exception caused by division by zero

            Int64 p = 1048576 - adc;
            p = ((p << 31) - var4) * 3125 / var3;
            var3 = (_digP9 * (p >> 13) * (p >> 13)) >> 25;
            var4 = (_digP8 * p) >> 19;
            p = ((p + var3 + var4) >> 8) + ((Int64) _digP7 << 4);
            p /= 256;

            return compensationMode == PressureCompensationModes.Uncompensated
                ? p / 100.0F
                : CalculatePressureAsl(p) / 100.0F;
        }

        /// <inheritdoc cref = "ITemperature"/>
        /// <summary>
        ///     Reads the temperature.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        /// <returns>
        ///     A single representing the temperature read from the source, degrees Celsius
        /// </returns>
        /// <example>
        ///     Example usage:
        ///     <code language="C#">
        /// Debug.Print("Temperature is " + _sensor.ReadTemperature(TemperatureSources.Ambient).ToString("F2") + " °C");
        /// </code>
        /// </example>
        public Single ReadTemperature(TemperatureSources source = TemperatureSources.Ambient)
        {
            if (TemperatureSamplingRate == OversamplingRates.Skipped) return Single.MaxValue;

            if (_powerMode == Mode.Sleep) EnableForcedMeasurement();

            Int32 adc = (this as ITemperature).RawData;
            Int32 var1 = (((adc >> 3) - (_digT1 << 1)) * _digT2) >> 11;
            Int32 var2 = (((((adc >> 4) - _digT1) * ((adc >> 4) - _digT1)) >> 12) * _digT3) >> 14;
            Int32 tFine = var1 + var2;
            Single temp = ((tFine * 5 + 128) >> 8) / 100.0F;

            switch (TemperatureUnit)
            {
                case TemperatureUnits.Celsius:
                {
                    return temp;
                }
                case TemperatureUnits.Fahrenheit:
                {
                    return temp * 1.8F + 32;
                }
                case TemperatureUnits.Kelvin:
                {
                    return temp + 273.15F;
                }
                default:
                {
                    throw new ArgumentOutOfRangeException();
                }
            }
        }

        /// <inheritdoc cref = "ITemperature" />
        /// <summary>Gets the raw data of the temperature value.</summary>
        /// <value>
        /// Raw data in the range depending on sensor's precision (8/10/12 bits, for example)
        /// </value>
        /// <example>Example usage:
        /// <code language = "C#">
        /// Debug.Print("Raw Temperature Data is " + (_sensor as ITemperature).RawData);
        /// </code>
        /// </example>
        Int32 ITemperature.RawData
        {
            get
            {
                while (Measuring || UpdatingNvm)
                {
                    Thread.Sleep(10);
                }

                Byte[] registerData = ReadRegister(REG_READ_TEMPERATURE, 3);

                return ((registerData[0] << 16) | (registerData[1] << 8) | registerData[2]) >> 4;
            }
        }

        /// <inheritdoc cref = "IPressure" />
        /// <summary>Gets the raw data of the pressure value.</summary>
        /// <value>
        /// Raw data in the range depending on sensor's precision (8/10/12 bits, for example)
        /// </value>
        /// <example>Example usage:
        /// <code language = "C#">
        /// Debug.Print("Raw Pressure Data is " + (_sensor as IPressure).RawData);
        /// </code>
        /// </example>
        Int32 IPressure.RawData
        {
            get
            {
                while (Measuring || UpdatingNvm)
                {
                    Thread.Sleep(10);
                }

                Byte[] registerData =ReadRegister(REG_READ_PRESSURE, 3);

                return ((registerData[0] << 16) + (registerData[1] << 8) + registerData[2]) >> 4;
            }
        }

        #endregion
    }
}