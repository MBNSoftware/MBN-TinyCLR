/*
 * TempHum6 Click driver for TinyCLR 2.0
 * 
 * Initial revision coded by Stephen Cardinale
 * 
 * Copyright 2020 Stephen Cardinale and MikroBUS.Net
 * Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License. You may obtain a copy of the License at
 * http://www.apache.org/licenses/LICENSE-2.0
 * Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, 
 * either express or implied. See the License for the specific language governing permissions and limitations under the License.
 * 
 */

using GHIElectronics.TinyCLR.Devices.I2c;

using System;
using System.Threading;

namespace MBN.Modules
{
    /// <inheritdoc cref="ITemperature" />
    /// <inheritdoc cref="IHumidity" />
    /// <summary>
    ///     Main class for the TempHum6Click driver
    ///     <para><b>Pins used :</b> Scl, Sda</para>
    ///     <para><b>This is an I2C device.</b></para>
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
    ///         private static TempHum6Click _sensor;
    ///
    ///         public static void Main()
    ///         {
    ///             _sensor = new TempHum6Click(Hardware.SocketTwo)
    ///             {
    ///                 TemperatureUnit = TemperatureUnits.Fahrenheit
    ///             };
    ///
    ///             Debug.WriteLine($"PartID is 0x{_sensor.PartID:x3}");
    ///             Debug.WriteLine($"UID is {_sensor.UniqueID}");
    ///
    ///             _sensor.PowerMode = PowerModes.Low;
    ///
    ///             TempHum6Click.SensorConfiguration configuration = new TempHum6Click.SensorConfiguration
    ///             {
    ///                 LowPower = false,
    ///                 EnableHumidityMeasurement = true,
    ///                 EnableTemperatureMeasurement = true,
    ///                 HumidityMeasurementMode = TempHum6Click.MeasurementMode.Continuous,
    ///                 TemperatureMeasurementMode = TempHum6Click.MeasurementMode.Continuous
    ///             };
    ///
    ///             _sensor.ConfigureSensor(configuration);
    ///
    ///             while (true)
    ///             {
    ///                 Debug.WriteLine($"Temperature .......: {_sensor.ReadTemperature():F2} °F");
    ///                 Debug.WriteLine($"Humidity...........: {_sensor.ReadHumidity():F2} %RH");
    ///                 Thread.Sleep(2000);
    ///             }
    ///         }
    ///     }
    /// }
    /// </code>
    /// </example>
    public sealed class TempHum6Click : ITemperature, IHumidity
    {
        #region .ctor

        /// <summary>
        ///     Initializes a new instance of the <see cref="TempHum6Click" /> class.
        /// </summary>
        /// <param name="socket">The socket on which the TempHum6Click module is plugged on MikroBus.Net board</param>
        public TempHum6Click(Hardware.Socket socket)
        {
            _sensor = I2cController.FromName(socket.I2cBus).GetDevice(new I2cConnectionSettings(0x43, 100000));

            // Reset device
            Reset(ResetModes.Soft);

            // Deactivate Low Power to be able to read the PartID and UID registers.
            PowerMode = PowerModes.On;

            PartID = BitConverter.ToUInt16(ReadRegister(ENS210_REG_PART_ID, 2), 0);

            if (PartID != 0x210) throw new DeviceInitialisationException("TempHum6 Click not found on I2C Bus.");
            UniqueID = BitConverter.ToInt64(ReadRegister(ENS210_REG_UID, 8), 0);

            ConfigureSensor(new SensorConfiguration(
                true,
                true,
                MeasurementMode.Continuous,
                MeasurementMode.Continuous,
                false));
        }

        #endregion

        #region Public ENUMS

        /// <summary>
        /// The mode of measurement for the Temp<![CDATA[&]]>Hum click.
        /// </summary>
        public enum MeasurementMode
        {
            /// <summary>
            /// Oneshot measurement mode.
            /// </summary>
            OneShot = 0,

            /// <summary>
            /// Continuous measurement.
            /// </summary>
            Continuous = 1
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Configures the Temp<![CDATA[&]]>Hum6 Click for measurement.
        /// </summary>
        /// <param name="configuration">The <see cref="SensorConfiguration"/> to use.</param>
        /// <example>
        /// <code language = "C#">
        /// TempHum6Click.SensorConfiguration configuration = new TempHum6Click.SensorConfiguration
        /// {
        ///     LowPower = false,
        ///     EnableHumidityMeasurement = true,
        ///     EnableTemperatureMeasurement = true,
        ///     HumidityMeasurementMode = TempHum6Click.MeasurementMode.Continuous,
        ///     TemperatureMeasurementMode = TempHum6Click.MeasurementMode.Continuous
        /// };
        /// _sensor.ConfigureSensor(configuration);
        /// </code>
        /// </example>
        public void ConfigureSensor(SensorConfiguration configuration)
        {
            PowerMode = configuration.LowPower ? PowerModes.Low : PowerModes.On;

            Byte registerData = 0x00;

            if (_currentConfiguration != null)
            {
                if (_currentConfiguration.HumidityMeasurementMode == MeasurementMode.Continuous
                    && configuration.HumidityMeasurementMode == MeasurementMode.OneShot)
                {
                    registerData = ENS210_HUM_STOP_CONTINUOUS_MEASUREMENT;
                }

                if (_currentConfiguration.TemperatureMeasurementMode == MeasurementMode.Continuous
                    && configuration.TemperatureMeasurementMode == MeasurementMode.OneShot)
                {
                    registerData |= ENS210_TEMP_STOP_CONTINUOUS_MEASUREMENT;
                }

                WriteRegister(ENS210_REG_SENS_STOP, registerData);
            }

            registerData = configuration.HumidityMeasurementMode == MeasurementMode.Continuous
                ? ENS210_HUM_RUN_CONTINUOUS_MODE
                : ENS210_HUM_RUN_SINGLE_SHOT_MODE;

            registerData |= configuration.TemperatureMeasurementMode == MeasurementMode.Continuous
                ? ENS210_TEMP_RUN_CONTINUOUS_MODE
                : ENS210_TEMP_RUN_SINGLE_SHOT_MODE;

            WriteRegister(ENS210_REG_SENS_RUN, registerData);

            registerData = configuration.EnableHumidityMeasurement
                ? ENS210_HUM_START_MEASUREMENT
                : ENS210_DISABLE_MEASUREMENT;

            registerData |= configuration.EnableTemperatureMeasurement
                ? ENS210_TEMP_START_MEASUREMENT
                : ENS210_DISABLE_MEASUREMENT;

            WriteRegister(ENS210_REG_SENS_START, registerData);

            _currentConfiguration = configuration;
        }

        #endregion

        #region Sensor Configuration Class

        /// <summary>
        /// The Sensor configuration class use to  configure the operating mode of the Temp<![CDATA[&]]>Hum6 Click.
        /// </summary>
        public class SensorConfiguration
        {
            /// <summary>
            /// Constructor specifying default configuration.
            /// </summary>
            /// <example>
            /// <code language = "C#">
            /// TempHum6Click.SensorConfiguration config = new TempHum6Click.SensorConfiguration();
            /// _sensor.ConfigureSensor(config);
            /// </code>
            /// </example>
            public SensorConfiguration()
            {
                EnableHumidityMeasurement = true;
                EnableTemperatureMeasurement = true;
                HumidityMeasurementMode = MeasurementMode.Continuous;
                TemperatureMeasurementMode = MeasurementMode.Continuous;
                LowPower = false;
            }

            /// <summary>
            /// Constructor specfying non-default configuration.
            /// </summary>
            /// <param name="enableHumidityMeasurement"></param>
            /// <param name="enableTemperatureMeasurement"></param>
            /// <param name="humidityMeasurementMode"></param>
            /// <param name="measurementMode"></param>
            /// <param name="lowPower"></param>
            /// <example>
            /// <code language = "C#">
            /// TempHum6Click.SensorConfiguration configuration = new TempHum6Click.SensorConfiguration();
            ///
            /// configuration.EnableHumidityMeasurement = true;
            /// configuration.EnableTemperatureMeasurement = true;
            /// configuration.HumidityMeasurementMode = TempHum6Click.MeasurementMode.OneShot;
            /// configuration.TemperatureMeasurementMode = TempHum6Click.MeasurementMode.OneShot;
            /// configuration.LowPower = true;
            ///
            /// _sensor.ConfigureSensor(configuration);
            /// </code>
            /// </example>
            public SensorConfiguration(Boolean enableHumidityMeasurement,
                Boolean enableTemperatureMeasurement,
                MeasurementMode humidityMeasurementMode,
                MeasurementMode measurementMode,
                Boolean lowPower)
            {
                EnableHumidityMeasurement = enableHumidityMeasurement;
                EnableTemperatureMeasurement = enableTemperatureMeasurement;
                HumidityMeasurementMode = humidityMeasurementMode;
                TemperatureMeasurementMode = measurementMode;
                LowPower = lowPower;
            }

            /// <summary>
            /// Enables or disables humidity measurement.
            /// </summary>
            /// <example>
            /// <code language = "C#">
            /// TempHum6Click.SensorConfiguration configuration = new TempHum6Click.SensorConfiguration();
            ///
            /// configuration.EnableHumidityMeasurement = true;
            /// configuration.EnableTemperatureMeasurement = true;
            /// configuration.HumidityMeasurementMode = TempHum6Click.MeasurementMode.OneShot;
            /// configuration.TemperatureMeasurementMode = TempHum6Click.MeasurementMode.OneShot;
            /// configuration.LowPower = true;
            ///
            /// _sensor.ConfigureSensor(configuration);
            /// </code>
            /// </example>
            public Boolean EnableHumidityMeasurement { get; set; }

            /// <summary>
            /// Enables or disables temperature measurement.
            /// </summary>
            /// <example>
            /// <code language = "C#">
            /// TempHum6Click.SensorConfiguration configuration = new TempHum6Click.SensorConfiguration();
            ///
            /// configuration.EnableHumidityMeasurement = true;
            /// configuration.EnableTemperatureMeasurement = true;
            /// configuration.HumidityMeasurementMode = TempHum6Click.MeasurementMode.OneShot;
            /// configuration.TemperatureMeasurementMode = TempHum6Click.MeasurementMode.OneShot;
            /// configuration.LowPower = true;
            ///
            /// _sensor.ConfigureSensor(configuration);
            /// </code>
            /// </example>
            public Boolean EnableTemperatureMeasurement { get; set; }

            /// <summary>
            /// Sets humidity measurement mode to be either continuous or oneshot.
            /// </summary>
            /// <example>
            /// <code language = "C#">
            /// TempHum6Click.SensorConfiguration configuration = new TempHum6Click.SensorConfiguration();
            ///
            /// configuration.EnableHumidityMeasurement = true;
            /// configuration.EnableTemperatureMeasurement = true;
            /// configuration.HumidityMeasurementMode = TempHum6Click.MeasurementMode.OneShot;
            /// configuration.TemperatureMeasurementMode = TempHum6Click.MeasurementMode.OneShot;
            /// configuration.LowPower = true;
            ///
            /// _sensor.ConfigureSensor(configuration);
            /// </code>
            /// </example>
            public MeasurementMode HumidityMeasurementMode { get; set; }

            /// <summary>
            /// Sets temperature measurement mode to be either continuous or oneshot.
            /// </summary>
            /// <example>
            /// <code language = "C#">
            /// TempHum6Click.SensorConfiguration configuration = new TempHum6Click.SensorConfiguration();
            ///
            /// configuration.EnableHumidityMeasurement = true;
            /// configuration.EnableTemperatureMeasurement = true;
            /// configuration.HumidityMeasurementMode = TempHum6Click.MeasurementMode.OneShot;
            /// configuration.TemperatureMeasurementMode = TempHum6Click.MeasurementMode.OneShot;
            /// configuration.LowPower = true;
            /// Sets humidity measurement mode to be either continuous or oneshot.
            /// _sensor.ConfigureSensor(configuration);
            /// </code>
            /// </example>
            public MeasurementMode TemperatureMeasurementMode { get; set; }

            /// <summary>
            /// Sets When enabled, the Temp<![CDATA[&]]>Hum6 Click returns to low power after a measurement is made. Otherwise, it will remain active.
            /// </summary>
            /// <example>
            /// <code language = "C#">
            /// TempHum6Click.SensorConfiguration configuration = new TempHum6Click.SensorConfiguration();
            ///
            /// configuration.EnableHumidityMeasurement = true;
            /// configuration.EnableTemperatureMeasurement = true;
            /// configuration.HumidityMeasurementMode = TempHum6Click.MeasurementMode.OneShot;
            /// configuration.TemperatureMeasurementMode = TempHum6Click.MeasurementMode.OneShot;
            /// configuration.LowPower = true;
            ///
            /// _sensor.ConfigureSensor(configuration);
            /// </code>
            /// </example>
            public Boolean LowPower { get; set; }
        }

        #endregion

        #region Private Fields

        private readonly I2cDevice _sensor;

        private SensorConfiguration _currentConfiguration;

        #endregion

        #region Constants

        private const Byte ENS210_REG_PART_ID = 0x00;
        private const Byte ENS210_REG_UID = 0x04;
        private const Byte ENS210_REG_SYS_CTRL = 0x10;
        private const Byte ENS210_REG_SYS_STAT = 0x11;
        private const Byte ENS210_REG_SENS_RUN = 0x21;
        private const Byte ENS210_REG_SENS_START = 0x22;
        private const Byte ENS210_REG_SENS_STOP = 0x23;
        private const Byte ENS210_REG_SENS_STAT = 0x24;
        private const Byte ENS210_REG_T_VAL = 0x30;
        private const Byte ENS210_REG_H_VAL = 0x33;

        private const Byte ENS210_STATUS_CRCERROR = 3;
        private const Byte ENS210_STATUS_INVALID = 2;
        private const Byte ENS210_STATUS_OK = 1;

        private const UInt16 ENS210_DELAY_BOOTING_TIME = 3;

        ////private const UInt16 CONVERSION_TIME_T_SS = 110;
        ////private const UInt16 CONVERSION_TIME_T_CONT = 109;
        ////private const UInt16 CONVERSION_TIME_T_H_SS = 130;
        private const UInt16 CONVERSION_TIME_T_H_CONT = 238;

        // Register configures the run modes
        private const Byte ENS210_HUM_RUN_SINGLE_SHOT_MODE = 0x00;
        private const Byte ENS210_TEMP_RUN_SINGLE_SHOT_MODE = 0x00;
        private const Byte ENS210_HUM_RUN_CONTINUOUS_MODE = 0x02;
        private const Byte ENS210_TEMP_RUN_CONTINUOUS_MODE = 0x01;

        // Starts a measurement for the sensors
        private const Byte ENS210_HUM_START_MEASUREMENT = 0x02;
        private const Byte ENS210_TEMP_START_MEASUREMENT = 0x01;
        private const Byte ENS210_DISABLE_MEASUREMENT = 0x00;

        // Stops a continuous measurement
        private const Byte ENS210_HUM_STOP_CONTINUOUS_MEASUREMENT = 0x02;
        private const Byte ENS210_TEMP_STOP_CONTINUOUS_MEASUREMENT = 0x01;

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets or sets the power mode.
        /// </summary>
        /// <example> None provided as this module does not support this feature.</example>
        /// <exception cref="T:System.NotSupportedException">A NotSupportedException will be thrown if attempting to set PowerMode to PowerModes.Off as this module does not support this feature.</exception>
        /// <example>Example usage:
        /// <code language = "C#">
        /// _sensor.PowerMode = PowerModes.Low;
        /// </code>
        /// </example>
        public PowerModes PowerMode
        {
            get => ReadRegister(ENS210_REG_SYS_CTRL)[0] == 0x01 ? PowerModes.Low : PowerModes.On;
            set
            {
                if (value == PowerModes.Off) throw new NotSupportedException("This module does not support PowerModes.Off");
                WriteRegister(ENS210_REG_SYS_CTRL, (Byte) (value == PowerModes.On ? 0x00 : 0x01));
                Thread.Sleep(ENS210_DELAY_BOOTING_TIME);
            }
        }

        /// <summary>
        /// Resets the module
        /// </summary>
        /// <exception cref="T:System.NotSupportedException">A NotSupportedException will be thrown if trying to use a Hard Reset as this module has no hard reset feature.
        /// </exception>
        /// <example>
        /// <code language = "C#">
        /// Reset(ResetModes.Soft);
        /// </code>
        /// </example>
        public Boolean Reset(ResetModes resetMode)
        {
            if (resetMode == ResetModes.Hard)
                throw new NotSupportedException("This module does not support a hard reset feature.");

            WriteRegister(ENS210_REG_SYS_CTRL, 0x80);

            do
            {
                Thread.Sleep(ENS210_DELAY_BOOTING_TIME);
            } while (ReadRegister(ENS210_REG_SYS_CTRL)[0] != 0x01);

            return ReadRegister(ENS210_REG_SYS_STAT)[0] == 0x01;
        }

        /// <summary>
        /// Returns the Part ID
        /// </summary>
        /// <example>
        /// <code language = "C#">
        /// Debug.Print("PartID is 0x" + _sensor.PartID.ToString("x3"));
        /// </code>
        /// </example>
        public UInt16 PartID { get; }

        /// <summary>
        /// Returns the 64-Bit unique ID number assigned to ENS210 IC hosted on the Temp<![CDATA[&]]>Hum6 Click.
        /// </summary>
        /// <example>
        /// <code language = "C#">
        /// Debug.Print("UID is " + _sensor.UniqueID);
        /// </code>
        /// </example>
        public Int64 UniqueID { get; }

        /// <summary>
        /// Sets or gets the Units used by the ITemperature interface.
        /// </summary>
        /// <example>
        /// <code language = "C#">
        /// _sensor.TemperatureUnit = TemperatureUnits.Celsius;
        /// </code>
        /// </example>
        public TemperatureUnits TemperatureUnit { get; set; } = TemperatureUnits.Kelvin;

        #endregion

        #region Private Methods

        private Byte[] ReadRegister(Byte registerAddress, Byte numberOfBytesToRead = 1)
        {
            Byte[] readBuffer = new Byte[numberOfBytesToRead];

            lock (Hardware.LockI2C)
            {
                _sensor.WriteRead(new[] { registerAddress}, readBuffer );
            }

            return readBuffer;
        }

        private void WriteRegister(Byte registerAddress, Byte value)
        {
            lock (Hardware.LockI2C)
            {
                _sensor.Write(new[] { registerAddress, value});
            }
        }

        private static Int32 CRC7(Int32 val)
        {
            const Int32 CRC7WIDTH = 7; // A 7 bits CRC has polynomial of 7th order, which has 8 terms
            const Int32 CRC7POLY = 0x89; // The 8 coefficients of the polynomial
            const Int32 CRC7IVEC = 0x7F; // Initial vector has all 7 bits high
            const Int32 DATA7WIDTH = 17;

            // Setup polynomial
            Int32 pol = CRC7POLY;

            // Align polynomial with dataOut
            pol <<= (DATA7WIDTH - CRC7WIDTH - 1);

            // Loop variable (indicates which bit to test, start with highest)
            UInt32 bit = 1U << (DATA7WIDTH - 1);

            // Make room for CRC value
            val <<= CRC7WIDTH;
            bit <<= CRC7WIDTH;
            pol <<= CRC7WIDTH;

            // Insert initial vector
            val |= CRC7IVEC;

            // Apply division until all bits done
            while ((bit & (((1U << DATA7WIDTH) - 1) << CRC7WIDTH)) != 0)
            {
                if ((bit & val) != 0)
                {
                    val ^= pol;
                }

                bit >>= 1;
                pol >>= 1;
            }

            return val;
        }

        private Boolean MeasuringTemperature()
        {
            return (ReadRegister(ENS210_REG_SENS_STAT)[0] & 0x01) == 0x01;
        }

        private Boolean MeasuringHumidity()
        {
            return (ReadRegister(ENS210_REG_SENS_STAT)[0] & 0x02) == 0x02;
        }

        // Extracts measurement `dataOut` and `status` from a `dataIn` obtained from `read`.
        // Upon entry, 'dataIn' is the 24 bits read from T_VAL or H_VAL.
        // Upon exit, 'dataOut' is the T_DATA or H_DATA, and 'status' one of ENS210_STATUS_XXX.
        private static void Extract(Int32 dataIn, out Int32 dataOut, out Int32 status)
        {
            // Destruct 'dataIn'
            dataOut = (dataIn >> 0) & 0xFFFF;
            Boolean valid = ((dataIn >> 16) & 0x01) == 0x01;
            Int32 crc = (dataIn >> 17) & 0x7F;
            Int32 payload = (dataIn >> 0) & 0x1FFFF;
            Boolean crc_ok = CRC7(payload) == crc;

            // Check CRC and valid bit
            if (!crc_ok) status = ENS210_STATUS_CRCERROR;
            else if (!valid) status = ENS210_STATUS_INVALID;
            else status = ENS210_STATUS_OK;
        }

        #endregion

        #region Interface Implentations

        /// <inheritdoc />
        /// <summary>
        /// Reads the ambient temperature.
        /// </summary>
        /// <param name="source"></param>
        /// <returns>A <see cref="System.Single"/> representing the temperature in the temperature units set in the <see cref="TemperatureUnit"/> property. Defaults to °K.</returns>
        /// <example>Example usage ot the ReadTemperature method.
        /// <code language = "C#">
        /// while (true)
        /// {
        ///     Debug.Print("Temeperature .......: " + _sensor.ReadTemperature().ToString("F2") + " °F");
        ///     Debug.Print("Humidity............: " + _sensor.ReadHumidity().ToString("F2") + " %RH");
        ///     Thread.Sleep(2000);
        /// }
        /// </code>
        /// </example>
        /// <remarks>If attempting to read temperature when temperature measurement is not enabled, this method will return <see cref="System.Single.MaxValue"/>.</remarks>
        /// <remarks>This method will return <see cref="System.Single.MaxValue"/> if there is a CRC mismatch or the ENS210 reports an invalid mesurement.</remarks>
        public Single ReadTemperature(TemperatureSources source = TemperatureSources.Ambient)
        {
            if (source == TemperatureSources.Object)
                throw new NotSupportedException(
                    "This module does not support reading object temerature. Plese use TemperatureSources.Ambient instead.");

            if (!_currentConfiguration.EnableTemperatureMeasurement) return Single.MaxValue;

            if (_currentConfiguration.TemperatureMeasurementMode == MeasurementMode.OneShot)
            {
                WriteRegister(ENS210_REG_SENS_START, ENS210_TEMP_START_MEASUREMENT);

                do
                {
                    Thread.Sleep(1);
                } while (MeasuringTemperature());
            }
            else
            {
                Thread.Sleep(CONVERSION_TIME_T_H_CONT);
            }

            Extract((this as ITemperature).RawData, out Int32 rawTemperature, out Int32 status);

            if (status == ENS210_STATUS_CRCERROR || status == ENS210_STATUS_INVALID) return Single.MaxValue;

            switch (TemperatureUnit)
            {
                case TemperatureUnits.Fahrenheit:
                {
                    return (rawTemperature / 64.0F - 273.15F) * 1.8F + 32.0F;
                }
                case TemperatureUnits.Kelvin:
                {
                    return rawTemperature / 64.0F;
                }
                case TemperatureUnits.Celsius:
                {
                    return rawTemperature / 64.0F - 273.15F;
                }
                default: throw new ArgumentException("Invalid Temperature Unit Selected");
            }
        }

        /// <inheritdoc />
        /// <summary>
        /// Reads the relative humidity.
        /// </summary>
        /// <param name="measurementMode"></param>
        /// <returns></returns>
        /// <example>Example usage ot the ReadHumidity method.
        /// <code language = "C#">
        /// while (true)
        /// {
        ///     Debug.Print("Temeperature .......: " + _sensor.ReadTemperature().ToString("F2") + " °F");
        ///     Debug.Print("Humidity............: " + _sensor.ReadHumidity().ToString("F2") + " %RH");
        ///     Thread.Sleep(2000);
        /// }
        /// </code>
        /// </example>
        /// <remarks>If attempting to read humidity when humidity measurement is not enabled, this method will return <see cref="System.Single.MaxValue"/>.</remarks>
        /// <remarks>This method will return <see cref="System.Single.MaxValue"/> if there is a CRC mismatch or the ENS210 reports an invalid mesurement.</remarks>
        public Single ReadHumidity(HumidityMeasurementModes measurementMode = HumidityMeasurementModes.Relative)
        {
            if (measurementMode == HumidityMeasurementModes.Absolute)
                throw new NotSupportedException(
                    "Absolute measurement is not supported by this module. Use HumidityMeasurementModes.Relative instead.");

            if (!_currentConfiguration.EnableHumidityMeasurement) return Single.MaxValue;

            if (_currentConfiguration.HumidityMeasurementMode == MeasurementMode.OneShot)
            {
                WriteRegister(ENS210_REG_SENS_START, ENS210_HUM_START_MEASUREMENT);

                do
                {
                    Thread.Sleep(1);
                } while (MeasuringHumidity());
            }
            else
            {
                Thread.Sleep(CONVERSION_TIME_T_H_CONT);
            }

            Extract((this as IHumidity).RawData, out Int32 rawHumidity, out Int32 status);

            if (status == ENS210_STATUS_CRCERROR || status == ENS210_STATUS_INVALID) return Single.MaxValue;

            return rawHumidity / 512.0F;
        }

        /// <inheritdoc cref="IHumidity"/>
        /// <summary>Gets the raw data of the humidity value.</summary>
        /// <value>
        /// Raw data in the range depending on sensor's precision (8/10/12 bits, for example)
        /// </value>
        /// <example>Example usage:
        /// <code language = "C#">
        /// Debug.Print("RawTemperature value is " + (_sensor as IHumidity).RawData);
        /// </code>
        /// </example>
        Int32 IHumidity.RawData
        {
            get
            {
                Byte[] registerData = ReadRegister(ENS210_REG_H_VAL, 3);
                return (registerData[2] << 16) | (registerData[1] << 8) | registerData[0];
            }
        }

        /// <inheritdoc cref="ITemperature"/>
        /// <summary>Gets the raw data of the temperature value.</summary>
        /// <value>
        /// Raw data in the range depending on sensor's precision (8/10/12 bits, for example)
        /// </value>
        /// <example>Example usage:
        /// <code language = "C#">
        /// Debug.Print("RawTemperature value is " + (_sensor as ITemperature).RawData);
        /// </code>
        /// </example>
        Int32 ITemperature.RawData
        {
            get
            {
                Byte[] registerData = ReadRegister(ENS210_REG_T_VAL, 3);
                return (registerData[2] << 16) | (registerData[1] << 8) | registerData[0];
            }
        }

        #endregion
    }
}