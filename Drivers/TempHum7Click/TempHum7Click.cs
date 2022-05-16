/*
 * TempHum7 Click driver for TinyCLR 2.0
 * 
 * Initial revision coded by Stephen Cardinale
 * 
 * References needed :  (change according to your driver's needs)
 *  MikroBusNet
 *  mscorlib
 *  GHIElectronics.TinyCLR.Devices.I2c
 *  
 * Copyright 2020 Stephen Cardinale and MikroBUS.Net
 * Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License. You may obtain a copy of the License at
 * http://www.apache.org/licenses/LICENSE-2.0
 * Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, 
 * either express or implied. See the License for the specific language governing permissions and limitations under the License.
 * 
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
    ///     A Mikrobus.Net TinyCLR driver for the TempHum 7 Click.
    ///     <para>
    ///         <b>Pins used : Sda, Scl</b>
    ///     </para>
    /// </summary>
    /// <example>
    ///     <code language="C#">
    /// using System.Threading;
    /// using MBN;
    /// using MBN.Enums;
    /// using MBN.Exceptions;
    /// using MBN.Modules;
    /// using Microsoft.SPOT;
    /// 
    /// namespace Example
    /// {
    ///     public class Program
    ///     {
    ///         private static TempHum7Click _sensor;
    /// 
    ///         public static void Main()
    ///         {
    ///             try
    ///             {
    ///                 _sensor = new TempHum7Click(Hardware.SocketOne, ClockRatesI2C.Clock400KHz, 500)
    ///                 {
    ///                     HumidityMeasurementMode = TempHum7Click.MeasurementMode.Hold,
    ///                     TemperatureMeasurementMode = TempHum7Click.MeasurementMode.Hold,
    ///                     SensorResolution = TempHum7Click.Resolution.Medium
    ///                 };
    ///             }
    ///             catch (PinInUseException)
    ///             {
    ///                 Debug.WriteLine("Throughout human history we have been dependent on machines to survive. Fate it seems is not without a sense of irony.");
    ///             }
    /// 
    ///             while (true)
    ///             {
    ///                 Debug.WriteLine("Temperature.....: " + _sensor.ReadTemperature().ToString("F4") + " �C");
    ///                 Debug.WriteLine("Humidity........: " + _sensor.ReadHumidity().ToString("F4") + " %RH");
    ///                 Thread.Sleep(1000);
    ///             }
    ///         }
    ///     }
    /// }
    /// </code>
    /// </example>
    public sealed class TempHum7Click : ITemperature, IHumidity
    {
        #region .ctor

        /// <summary>
        /// Initializes a new instance of the TempHum 7 Click class
        /// </summary>
        /// <param name="socket">
        ///     The socket that TempHum 7 Click is plugged into.
        /// </param>
        public TempHum7Click(Hardware.Socket socket)
        {
            _socket = socket;
#if (NANOFRAMEWORK_1_0)
            _sensor = I2cDevice.Create(new I2cConnectionSettings(socket.I2cBus, 0x40, I2cBusSpeed.FastMode));
#else
			_sensor = I2cController.FromName(socket.I2cBus).GetDevice(new I2cConnectionSettings(0x40, 400000));
#endif

            Thread.Sleep(50); // Time from VDD >= 1.64V until ready for a full conversion.

            HumidityMeasurementMode = MeasurementMode.NoHold;
        }

        #endregion

        #region Public Methods

        /// <summary>
        ///     Resets the module
        /// </summary>
        /// <example>
        ///     <code language="C#">
        /// _sensor.Reset();
        /// </code>
        /// </example>
        public void Reset()
        {
            WriteRegister(SI7021_CMD_RESET);

            Byte registerData;

            do
            {
                registerData = ReadRegister(SI7021_CMD_READ_USER_REGISTER_1)[0];
                Thread.Sleep(5);
            } while (registerData != 0x3A);
        }

        #endregion

        #region Constants

        private const Byte SI7021_CMD_MEASURE_HUMIDITY_HOLD = 0xE5;
        private const Byte SI7021_CMD_MEASURE_HUMIDITY_NOHOLD = 0xF5;
        private const Byte SI7021_CMD_MEASURE_TEMPERATURE_HOLD = 0xE3;
        private const Byte SI7021_CMD_MEASURE_TEMPERATURE_NOHOLD = 0xF3;
        private const Byte SI7021_CMD_READ_TEMP_FROM_PREVIOUS = 0xE0;
        private const Byte SI7021_CMD_RESET = 0xFE;
        private const Byte SI7021_CMD_WRITE_USER_REGISTER_1 = 0xE6;
        private const Byte SI7021_CMD_READ_USER_REGISTER_1 = 0xE7;
        private const Byte SI7021_CMD_WRITE_HEATER_CONTROL_REGISTER = 0x51;
        private const Byte SI7021_CMD_READ_HEATER_CONTROL_REGISTER = 0x11;
        private const UInt16 SI7021_CMD_READ_ELECTRONIC_ID_FIRST_BYTE = 0xFA0F;
        private const UInt16 SI7021_CMD_READ_ELECTRONIC_ID_SECOND_BYTE = 0xFCC9;
        private const UInt16 SI7021_CMD_READ_FIRMWARE_REVISION = 0x84B8;

        #endregion

        #region Fields

        private I2cDevice _sensor;
        private MeasurementMode _humidityMeasurementMode;
        private readonly Hardware.Socket _socket;

        #endregion

        #region Public ENUMS

        /// <summary>
        ///     Possible resolutions supported by the Temp<![CDATA[&]]>>Hum 7 Click.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public enum Resolution
        {
            /// <summary>11 Bit Relative Humidity and 11 Bit Temperature resolution. </summary>
            Low = 0x03,

            /// <summary>8 Bit Relative Humidity and 12 Bit Temperature resolution.</summary>
            Medium = 0x01,

            /// <summary>10 Bit Relative Humidity and 13 Bit Temperature resolution.</summary>
            MediumHigh = 0x02,

            /// <summary>
            ///     12 Bit Relative Humidity and 14 Bit Temperature resolution.
            ///     <para>This is the power on default setting.</para>
            /// </summary>
            High = 0x00
        }

        /// <summary>I2C mode of operation when mesuring temperature or humidity.</summary>
        /// y>
        public enum MeasurementMode
        {
            /// <summary>SCL line is not held low during I2C reads. This is typically how NetMF is intended to work.</summary>
            Hold = 0,

            /// <summary>SCL line is held low during I2C reads. Works good but may interefere with other I2C operations.</summary>
            NoHold = 1,

            /// <summary>Intended to be applied for temperature measurement. Temperature value is taken from last humidity measurement.</summary>
            ReadFromPrevious
        }

        /// <summary>
        ///     Used to control heating output when the heat function is turned on. See
        ///     <see cref="TempHum7Click.HeaterEnable"> for additional information.</see>
        /// </summary>
        public enum HeaterControlSettings
        {
            /// <summary>Heating element is supplied with 3.09 mA of current. This is the power on default.</summary>
            MA_3_09 = 0x00,

            /// <summary>Heating element is supplied with 9.18 mA of current.</summary>
            MA_9_18 = 0x01,

            /// <summary>Heating element is supplied with 15.24 mA of current.</summary>
            MA_15_24 = 0x02,

            /// <summary>Heating element is supplied with 21.32 mA of current.</summary>
            MA_21_32 = 0x03,

            /// <summary>Heating element is supplied with 27.39 mA of current.</summary>
            MA_27_39 = 0x04,

            /// <summary>Heating element is supplied with 33.47 mA of current.</summary>
            MA_33_47 = 0x05,

            /// <summary>Heating element is supplied with 39.54 mA of current.</summary>
            MA_39_54 = 0x06,

            /// <summary>Heating element is supplied with 45.62 mA of current.</summary>
            MA_45_62 = 0x07,

            /// <summary>Heating element is supplied with 51.69 mA of current.</summary>
            MA_51_69 = 0x08,

            /// <summary>Heating element is supplied with 57.51 mA of current.</summary>
            MA_57_51 = 0x09,

            /// <summary>Heating element is supplied with 63.14 mA of current.</summary>
            MA_63_32 = 0x0A,

            /// <summary>Heating element is supplied with 69.14 mA of current.</summary>
            MA_69_14 = 0x0B,

            /// <summary>Heating element is supplied with 74.95 mA of current.</summary>
            MA_74_95 = 0x0C,

            /// <summary>Heating element is supplied with 80.77 mA of current.</summary>
            MA_80_77 = 0x0D,

            /// <summary>Heating element is supplied with 86.58 mA of current.</summary>
            MA_86_58 = 0x0E,

            /// <summary>Heating element is supplied with 94.20 mA of current.</summary>
            MA_94_20 = 0x0F
        }

        #endregion

        #region Public Properties

        /// <summary>
        ///     Sets or gets the sensor resolution for temperature and humidity measurements.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     An ArgumentOutOfRangeException will be thrown if passing an invalid value
        ///     to this property.
        /// </exception>
        /// <example>
        ///     <code language="C#">
        /// _sensor.SensorResolution = TempHum7Click.Resolution.High;
        /// </code>
        /// </example>
        public Resolution SensorResolution
        {
            get
            {
                Byte registerData = ReadRegister(SI7021_CMD_READ_USER_REGISTER_1)[0];
                Byte resolution = (Byte) ((registerData & 0x80) >> 6);
                resolution |= (Byte) (registerData & 0x01);
                return (Resolution) resolution;
            }
            set
            {
                Byte registerData = ReadRegister(SI7021_CMD_READ_USER_REGISTER_1)[0];

                switch (value)
                {
                    case Resolution.High:
                    {
                        Bits.Set(ref registerData, "0xxxxxx0");
                        break;
                    }

                    case Resolution.Low:
                    {
                        Bits.Set(ref registerData, "1xxxxxx1");
                        break;
                    }

                    case Resolution.Medium:
                    {
                        Bits.Set(ref registerData, "0xxxxxx1");
                        break;
                    }

                    case Resolution.MediumHigh:
                    {
                        Bits.Set(ref registerData, "1xxxxxx0");
                        break;
                    }

                    default:
                    {
                        throw new ArgumentOutOfRangeException(nameof(value));
                    }
                }

                WriteRegister(SI7021_CMD_WRITE_USER_REGISTER_1, registerData);
            }
        }

        /// <summary>
        ///     Sets or gets the temperature measurement mode.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     An ArgumentOutOfRangeException will be thrown if passing an invalid value
        ///     to this property.
        /// </exception>
        /// <example>
        ///     <code language="C#">
        /// _sensor.TemperatureMeasurementMode = TempHum7Click.MeasurementMode.ReadFromPrevious;
        /// </code>
        /// </example>
        public MeasurementMode TemperatureMeasurementMode { get; set; } = MeasurementMode.Hold;

        /// <summary>
        ///     Sets or gets the humidity measurement mode.
        /// </summary>
        /// <remarks>The default setting is <see cref="MeasurementMode.NoHold"></see></remarks>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     An ArgumentOutOfRangeException will be thrown if passing an invalid value
        ///     to this property.
        /// </exception>
        /// <exception cref="ApplicationException">
        ///     An Application exception will be thrown is attempting to assign
        ///     <see cref="MeasurementMode.ReadFromPrevious" /> to this property.
        /// </exception>
        /// <example>
        ///     <code language="C#">
        /// _sensor.HumidityMeasurementMode = TempHum7Click.MeasurementMode.NoHold;
        /// </code>
        /// </example>
        public MeasurementMode HumidityMeasurementMode
        {
            get => _humidityMeasurementMode;
            set
            {
                if (value == MeasurementMode.ReadFromPrevious) throw new ApplicationException("MeasurementMode.ReadFromPrevious is only to be used for temperature measurement.");
                if (value < 0 || (Byte) value > 1) throw new ArgumentOutOfRangeException(nameof(value));
                _humidityMeasurementMode = value;
            }
        }

        /// <summary>
        ///     Enables or disables the on-board heater.
        /// </summary>
        /// <remarks>The default setting is off.</remarks>
        /// <example>
        ///     <code language="C#">
        /// sensor.HeaterEnable = true;
        /// _sensor.HeaterSetttings = TempHum7Click.HeaterControlSettings..MA_94_20;
        /// </code>
        /// </example>
        public Boolean HeaterEnable
        {
            get
            {
                Byte[] registerData = ReadRegister(SI7021_CMD_READ_USER_REGISTER_1);
                return (registerData[0] &= 0x04) == 0x04;
            }
            set
            {
                Byte registerData = ReadRegister(SI7021_CMD_READ_USER_REGISTER_1)[0];
                registerData &= 0xFB;
                registerData |= (Byte) (value ? 0x04 : 0x00);
                WriteRegister(SI7021_CMD_WRITE_USER_REGISTER_1, registerData);
            }
        }

        /// <summary>
        ///     Gets or sets the on-board heating element setting.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     An ArgumentOutOfRangeException will be thrown if passing an invalid value
        ///     to this property.
        /// </exception>
        /// <example>
        ///     <code language="C#">
        /// sensor.HeaterEnable = true;
        /// _sensor.HeaterSetttings = TempHum7Click.HeaterControlSettings..MA_94_20;
        /// </code>
        /// </example>
        public HeaterControlSettings HeaterSetttings
        {
            get
            {
                Byte registerData = ReadRegister(SI7021_CMD_READ_HEATER_CONTROL_REGISTER)[0];
                return (HeaterControlSettings) (registerData & 0x0F);
            }
            set
            {
                if (value < 0 || (Byte) value > 15) throw new ArgumentOutOfRangeException(nameof(value));

                Byte registerData = ReadRegister(SI7021_CMD_READ_HEATER_CONTROL_REGISTER)[0];
                registerData &= 0xF0;
                WriteRegister(SI7021_CMD_WRITE_HEATER_CONTROL_REGISTER, (Byte) (registerData | (Byte) value));
            }
        }

        /// <summary>
        ///     Retrieves the firmware version of the Temp<![CDATA[&]]>Hum 7 Click.
        /// </summary>
        /// <example>
        ///     <code language="C#">
        /// Debug.WriteLine("Firmware Revision is 0x" + _sensor.FirmwareVersion.ToString("x2"));
        /// </code>
        /// </example>
        public Byte FirmwareVersion
        {
            get
            {
                Byte[] registerData = ReadRegister(SI7021_CMD_READ_FIRMWARE_REVISION);
                return registerData[0];
            }
        }

        /// <summary>
        ///     Retrieves the 64 Bit Serial number assigned to the Temp<![CDATA[&]]>Hum 7 Click.
        /// </summary>
        /// <example>
        ///     <code language="C#">
        /// Debug.WriteLine("Serial Number is " + _sensor.SerialNumber);
        /// </code>
        /// </example>
        public Int64 SerialNumber
        {
            get
            {
                Byte[] readBuffer = ReadRegister(SI7021_CMD_READ_ELECTRONIC_ID_FIRST_BYTE, 8);

                Int64 result = readBuffer[0];
                result <<= 8;
                result |= readBuffer[2];
                result <<= 8;
                result |= readBuffer[4];
                result <<= 8;
                result |= readBuffer[6];
                result <<= 8;


                readBuffer = ReadRegister(SI7021_CMD_READ_ELECTRONIC_ID_SECOND_BYTE, 6);

                result |= readBuffer[0];
                result <<= 8;
                result |= readBuffer[1];
                result <<= 8;
                result |= readBuffer[3];
                result <<= 8;
                result |= readBuffer[4];

                return result;
            }
        }

        /// <summary>
        ///     Gets or sets the unit of measure for reporting the temperature. Defaults to Degrees C.
        /// </summary>
        public TemperatureUnits TemperatureUnit { get; set; } = TemperatureUnits.Celsius;

        #endregion

        #region Private Methods

        private Byte[] ReadRegister(Byte register, Byte bytesToRead, Boolean clockStretch)
        {
            lock (_socket.LockI2c)
            {
                Byte[] readBuffer = new Byte[bytesToRead];

                _sensor.Write(new[] {register});

                if (!clockStretch)
                {
                    switch (register)
                    {
                        case SI7021_CMD_MEASURE_HUMIDITY_NOHOLD:
                        {
                            Thread.Sleep(30);
                            break;
                        }

                        case SI7021_CMD_MEASURE_TEMPERATURE_NOHOLD:
                        {
                            Thread.Sleep(30);
                            break;
                        }

                        default:
                        {
                            throw new ArgumentException(nameof(register));
                        }
                    }

                    _sensor.Read(readBuffer);
                }
                else
                {
                    _sensor.WriteRead(new[] {register}, readBuffer);
                }

                return readBuffer;
            }
        }

        private Byte[] ReadRegister(UInt16 registerAddress, Byte numberOfBytesToRead = 1)
        {
            lock (_socket.LockI2c)
            {
                Byte[] writeBuffer = {(Byte) (registerAddress >> 8), (Byte) (registerAddress & 0x00FF)};
                Byte[] registerData = new Byte[numberOfBytesToRead];
                _sensor.WriteRead(writeBuffer, registerData);
                return registerData;
            }
        }

        private void WriteRegister(Byte registerAdderss, Byte value)
        {
            lock (_socket.LockI2c)
            {
                _sensor.Write(new[] {registerAdderss, value});
            }
        }

        private void WriteRegister(Byte registerAddress)
        {
            lock (_socket.LockI2c)
            {
                _sensor.Write(new[] {registerAddress});
            }
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
        /// Debug.WriteLine("Raw Humidity Data........: " + (_sensor as IHumidity).RawData);
        /// </code>
        /// </example>
        Int32 IHumidity.RawData
        {
            get
            {
                Byte[] registerData = HumidityMeasurementMode == MeasurementMode.NoHold
                    ? ReadRegister(SI7021_CMD_MEASURE_HUMIDITY_NOHOLD, 2, false)
                    : ReadRegister(SI7021_CMD_MEASURE_HUMIDITY_HOLD, 2, true);

                return (registerData[0] << 8) | registerData[1];
            }
        }

        /// <inheritdoc cref="ITemperature" />
        /// <summary>
        ///     Gets the raw data of the temperature value.
        /// </summary>
        /// <value>
        ///     Raw data in the range depending on sensor's precision (8/10/12 bits, for example)
        /// </value>
        /// <example>
        ///     Example usage:
        ///     <code language="C#">
        /// Debug.WriteLine("Raw Temperature Data........: " + (_sensor as ITemperature).RawData);
        /// </code>
        /// </example>
        Int32 ITemperature.RawData
        {
            get
            {
                Byte[] registerData;

                switch (TemperatureMeasurementMode)
                {
                    case MeasurementMode.Hold:
                    {
                        registerData = ReadRegister(SI7021_CMD_MEASURE_TEMPERATURE_HOLD, 2, true);
                        break;
                    }

                    case MeasurementMode.NoHold:
                    {
                        registerData = ReadRegister(SI7021_CMD_MEASURE_TEMPERATURE_NOHOLD, 2, false);
                        break;
                    }

                    case MeasurementMode.ReadFromPrevious:
                    {
                        registerData = ReadRegister(SI7021_CMD_READ_TEMP_FROM_PREVIOUS, 2, true);
                        break;
                    }

                    default:
                    {
                        throw new ArgumentOutOfRangeException();
                    }
                }

                return (registerData[0] << 8) | registerData[1];
            }
        }

        /// <inheritdoc cref="ITemperature" />
        /// <summary>
        ///     Reads the temperature.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <returns>
        /// A single representing the temperature read from the source, degrees Celsius
        /// </returns>
        /// <example>
        /// Example usage:
        /// <code language = "C#">
        /// Debug.WriteLine("Temperature.....: " + _sensor.ReadTemperature().ToString("F4") + " �C");
        /// </code>
        /// </example>
        public Single ReadTemperature(TemperatureSources source = TemperatureSources.Ambient)
        {
            Int32 rawTemp = (this as ITemperature).RawData;
            return ScaleTemperature(175.72f * rawTemp / 65536.0f - 46.85f);
        }

        /// <inheritdoc cref="IHumidity" />
        /// <summary>
        ///     Reads the relative or absolute humidity value from the sensor.
        /// </summary>
        /// <returns>
        ///     A single representing the relative/absolute humidity as read from the sensor, in percentage (%) for relative
        ///     reading or value in case of absolute reading.
        /// </returns>
        /// <example>
        ///     Example usage:
        ///     <code language = "C#">
        /// Debug.WriteLine("Humidity........: {_sensor.ReadHumidity():F4} %RH");
        /// </code>
        /// </example>
        public Single ReadHumidity(HumidityMeasurementModes measurementMode = HumidityMeasurementModes.Relative)
        {
            Int32 rawRH = (this as IHumidity).RawData;

            Single relativeHumidity = 125.0f * rawRH / 65536.0f - 6.0f;

            if (relativeHumidity < 0.0) relativeHumidity = 0.0f;

            if (relativeHumidity > 100.0) relativeHumidity = 100.0f;

            return relativeHumidity;
        }

        #endregion
    }
}