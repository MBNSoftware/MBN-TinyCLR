/*
 * TempHum9 Click driver for TinyCLR 2.0
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

#if (NANOFRAMEWORK_1_0)
using System.Device.I2c;
#else
using GHIElectronics.TinyCLR.Devices.I2c;
#endif

using System;
using System.Threading;

namespace MBN.Modules
{
    /// <inheritdoc cref="ITemperature" />
    /// <inheritdoc cref="IHumidity" />
    /// <summary>
    /// Main class for the TempHum9 Click driver
    /// <para><b>Pins used :</b> Scl, Sda</para>
    /// </summary>
    /// <example>
    /// <code language = "C#">
    /// using MBN;
    /// using MBN.Modules;
    /// using System.Threading;
    ///
    /// namespace Example
    /// {
    ///     public class Program
    ///     {
    ///         private static TempHum9Click _sensor;
    ///
    ///         public static void Main()
    ///         {
    ///              _sensor = new TempHum9Click(Hardware.SC20100_1)
    ///              
    ///                 PowerMode = PowerModes.On,
    ///                 ClockStretch = true
    ///              };
    ///
    ///             Debug.WriteLine("ID is " + _sensor.ReadID());
    ///
    ///             while (true)
    ///             {
    ///                 Debug.WriteLine("----------Temp<![CDATA[&]]>Hum 9 Click----------");
    ///                 Debug.WriteLine("Temperature...............: " + _sensor.ReadTemperature());
    ///                 Debug.WriteLine("Humidity..................: " + _sensor.ReadHumidity());
    ///                 Debug.WriteLine("Temperature RawData is... : " + (_sensor as ITemperature).RawData);
    ///                 Debug.WriteLine("Humidity RawData is...... : " + (_sensor as IHumidity).RawData);
    ///                 Thread.Sleep(2000);
    ///             }
    ///         }
    ///     }
    /// }
    /// </code>
    /// </example>
    public sealed class TempHum9Click : ITemperature, IHumidity
    {
        #region .ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="TempHum9Click"/> class.
        /// </summary>
        /// <param name="socket">The socket on which the Temp_Hum9Click module is plugged on MikroBus.Net board</param>
        public TempHum9Click(Hardware.Socket socket)
        {
            _socket = socket;
#if (NANOFRAMEWORK_1_0)
            _sensor = I2cDevice.Create(new I2cConnectionSettings(socket.I2cBus, 0x70, I2cBusSpeed.StandardMode));
#else
			_sensor = I2cController.FromName(socket.I2cBus).GetDevice(new I2cConnectionSettings(0x70, 100000));
#endif

            Thread.Sleep(DURATION_POWER_UP); //Time between VDD reaching VPU and sensor entering the idle state typically 240 µSec

            //Reset();

            ClockStretch = false;

            _powerMode = PowerModes.Low;
        }

        #endregion

        #region Private Fields

        private readonly I2cDevice _sensor;
        private PowerModes _powerMode;
        private readonly Hardware.Socket _socket;

        #endregion

        #region Private Constants

        private const Byte Duration_Normal_Mode = 13;
        private const Byte Duration_Low_Power_Mode = 1;
        private const Byte DURATION_POWER_UP = 1;
        private const Byte DURATION_SOFT_RESET = 1;

        private const UInt16 SHTC3_CMD_ID = 0xEFC8;
        private const UInt16 SHTC3_CMD_SoftReset = 0x805D;
        private const UInt16 SHTC3_CMD_Sleep = 0xB098;
        private const UInt16 SHTC3_CMD_WakeUp = 0x3517;
        private const UInt16 SHTC3_CMD_MEASURE_NORMALPOWER_CLOCKSTRETCH_TEMPERATURE_FIRST = 0x7CA2;
        private const UInt16 SHTC3_CMD_MEASURE_NORMALPOWER_CLOCKSTRETCH_HUMIDITY_FIRST = 0x5C24;
        private const UInt16 SHTC3_CMD_MEASURE_NORMALPOWER_POLLING_TEMPERATURE_FIRST = 0x7866;
        private const UInt16 SHTC3_CMD_MEASURE_NORMALPOWER_POLLING_HUMIDITY_FIRST = 0x58E0;
        private const UInt16 SHTC3_CMD_MEASURE_LOWPOWER_CLOCKSTRETCH_TEMPERATURE_FIRST = 0x6458;
        private const UInt16 SHTC3_CMD_MEASURE_LOWPOWER_CLOCKSTRETCH_HUMIDITY_FIRST = 0x44DE;
        private const UInt16 SHTC3_CMD_MEASURE_LOWPOWER_POLLING_TEMPERATURE_FIRST = 0x609C;
        private const UInt16 SHTC3_CMD_MEASURE_LOWPOWER_POLLING_HUMIDITY_FIRST = 0x401A;

        #endregion

        #region Public Properties

        /// <summary>
        /// When enabled the SHTC-3 on the Temp<![CDATA[&]]>Hum9 Click will pull the SCL line low throughout any measurement.
        /// When not enabled, the driver uses a polling method by waiting the prescribed time between Writes and Reads on the I2C Bus.
        /// </summary>
        /// <example>
        /// <code language = "C#">
        /// _sensor.ClockStretch = true;
        /// </code>
        /// <code language = "VB">
        /// _sensor.ClockStretch = true
        /// </code>
        /// </example>
        public Boolean ClockStretch { get; set; }

        /// <summary>
        ///     Gets or sets the unit of measure for reporting the temperature. Defaults to Degrees C.
        /// </summary>
        public TemperatureUnits TemperatureUnit { get; set; }

        /// <summary>
        /// Gets or sets the power mode.
        /// </summary>
        /// <example> This sample shows how to use the PowerMode property.
        /// <code language="C#">
        /// _tempHum.PowerMode = PowerModes.Low;
        /// Debug.WriteLine("PowerMode is : + _tempHum.PowerMode;
        /// </code>
        /// <code language = "VB">
        /// _tempHum.PowerMode = PowerModes.Low
        /// Debug.WriteLine("PowerMode is : <![CDATA[&]]> _tempHum.PowerMode
        /// </code>
        /// </example>
        /// <value>
        /// The current power mode of the module.
        /// </value>
        /// <exception cref="T:System.NotImplementedException">Thrown if the property is set and the module doesn't support power modes.</exception>
        public PowerModes PowerMode
        {
            get => _powerMode;
            set
            {
                switch (value)
                {
                    case PowerModes.Off:
                    {
                        throw new ApplicationException("This Modules does not support PowerModes.Off" );
                    }
                    case PowerModes.Low:
                    {
                        Sleep();
                        break;
                    }
                    case PowerModes.On:
                    {
                        Wake();
                        break;
                    }
                    default:
                    {
                        throw new ArgumentOutOfRangeException(nameof(value));
                    }
                }

                _powerMode = value;
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Reads the Unique ID of this SHT-C3 IC hosted on the Temp<![CDATA[&]]>Hum9 click
        /// </summary>
        /// <returns></returns>
        /// <example>
        /// <code language = "C#">
        /// Debug.WriteLine("ID is " + _sensor.ReadID());
        /// </code>
        /// <code language = "VB">
        /// Debug.WriteLine("ID is " <![CDATA[&]]> _sensor.ReadID().ToString())
        /// </code>
        /// </example>
        public UInt16 ReadID()
        {
            if (PowerMode == PowerModes.Low)
            {
                Wake();
                Thread.Sleep(10);
            }

            Byte[] registerData = ReadRegister(SHTC3_CMD_ID, 3);

            Byte crc = ComputeCRC8(new[] { registerData[0], registerData[1] });
            if (crc != registerData[2]) throw new ApplicationException("Checksum failure");

            if (PowerMode != PowerModes.Low) return (UInt16) ((registerData[0] << 8) | registerData[1]);

            Sleep();

            Thread.Sleep(10);

            return (UInt16)((registerData[0] << 8) | registerData[1]);

        }

        /// <summary>
        /// Resets the module
        /// </summary>
        /// <code language = "C#">
        /// _tempHum.Reset(ResetModes.Soft);
        /// </code>
        /// <code language = "VB">
        /// _tempHum.Reset(ResetModes.Soft)
        /// </code>
        /// <exception cref="T:System.NotSupportedException">A NotSupportedException will be thrown as this module does not support a hard reset mechanism.</exception>
        public void Reset()
        {
            WriteRegister(SHTC3_CMD_SoftReset);

            Thread.Sleep(DURATION_SOFT_RESET); // Time between ACK of soft reset command and sensor entering the idle state. Typically 240 µSec.
        }

        #endregion

        #region Private Methods

        private void Sleep()
        {
            WriteRegister(SHTC3_CMD_Sleep);
        }

        private void Wake()
        {
            WriteRegister(SHTC3_CMD_WakeUp);
        }

        private void WriteRegister(UInt16 command)
        {
            Byte[] writeBuffer = new Byte[2];
            writeBuffer[0] = (Byte)(command >> 8);
            writeBuffer[1] = (Byte)(command & 0xFF);

            lock (_socket.LockI2c)
            {
                _sensor.Write(writeBuffer);
            }
        }

        private Byte[] ReadRegister(Byte numberOfBytesToRead)
        {
            Byte[] readBuffer = new Byte[numberOfBytesToRead];

            lock (_socket.LockI2c)
            {
                _sensor.Read(readBuffer);
            }

            return readBuffer;
        }

        private Byte[] ReadRegister(UInt16 registerAddress, Byte bytesToRead)
        {
            Byte[] writeBuffer = new Byte[2];
            writeBuffer[0] = (Byte)(registerAddress >> 8);
            writeBuffer[1] = (Byte)(registerAddress & 0xFF);

            Byte[] readBuffer = new Byte[bytesToRead];


            lock (_socket.LockI2c)
            {
                _sensor.WriteRead(writeBuffer, readBuffer);
            }

            return readBuffer;
        }

        // Tested with 0xBEEF = 0x92 per data-sheet.
        private static Byte ComputeCRC8(Byte[] data)
        {
            const Int16 polynomial = 0x131; // P(x) = 2^8 + 2^5 + 2^4 + 1
            Byte crc = 0xFF; // Initialize the CRC to 0xFF

            // Iterate through each byte in the passed data
            for (Byte byteCounter = 0; byteCounter < data.Length; byteCounter++)
            {
                crc ^= data[byteCounter];

                for (Byte bit = 0; bit <= 7; bit++) // Once for each bit in the Byte
                {
                    crc = (crc & 0x80) == 0x80 ? (Byte)((crc << 1) ^ polynomial) : (Byte)(crc << 1);
                }
            }

            return crc;
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

        #region Interface Implementation

        /// <inheritdoc />
        /// <summary>
        /// Reads the temperature.
        /// </summary>
        /// <param name="source">The temperature source to read. In this case only <see cref="TemperatureSources.Ambient"/> is supported.</param>
        /// <returns>A <see cref="System.Single"/> representing the ambient temperature.</returns>
        /// <exception cref="NotSupportedException">A <see cref="NotSupportedException"/> will be thrown if attempting to read Object temperature as this module does not support reading of object temperature.</exception>
        /// <remarks>Reading temperature while in low power mode is automatically handled by this method. The SHTC3 is woken up and returned to sleep after the temperature is measured.</remarks>
        /// <example>
        /// <code language = "C#">
        /// Debug.WriteLine("Temperature...............: " + _sensor.ReadTemperature());
        /// </code>
        /// <code language = "VB">
        /// Debug.WriteLine("Temperature...............: " <![CDATA[&]]> _sensor.ReadTemperature())
        /// </code>
        /// </example>
        public Single ReadTemperature(TemperatureSources source = TemperatureSources.Ambient)
        {
            if (source == TemperatureSources.Object) throw new NotSupportedException("This module does not support reading Object temperature. Please use TemperatureSources.Ambient instead.");
            return ScaleTemperature(175.0f * (this as ITemperature).RawData / 65535.0f - 45.0f);
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
        /// Debug.WriteLine("Humidity..................: " + _sensor.ReadHumidity());
        /// </code>
        /// <code language = "VB">
        /// Debug.WriteLine("Humidity..................: " <![CDATA[&]]> _sensor.ReadHumidity())
        /// </code>
        /// </example>
        public Single ReadHumidity(HumidityMeasurementModes measurementMode = HumidityMeasurementModes.Relative)
        {
            if (measurementMode == HumidityMeasurementModes.Absolute) throw new NotSupportedException("This module does not support reading absolute humidity.");
            return 100.0f * ((this as IHumidity).RawData / 65535.0f);
        }

        /// <inheritdoc />
        /// <summary>Gets the raw data of the humidity value.</summary>
        /// <value>
        /// Raw data in the range depending on sensor's precision (8/10/12 bits, for example)
        /// </value>
        /// <example>
        /// <code language = "C#">
        /// Debug.WriteLine("Humidity RawData is... : " + (_sensor as IHumidity).RawData);
        /// </code>
        /// <code language = "VB">
        /// Debug.WriteLine("Humidity RawData is... : " <![CDATA[&]]> (TryCast(_sensor, IHumidity)).RawData)
        /// </code>
        /// </example>
        Int32 IHumidity.RawData
        {
            get
            {
                UInt16 command;
                Byte[] registerData = { };

                switch (ClockStretch)
                {
                    case true:
                    {
                        switch (PowerMode)
                        {
                            case PowerModes.Low:
                                {
                                    command = SHTC3_CMD_MEASURE_LOWPOWER_CLOCKSTRETCH_HUMIDITY_FIRST;
                                    Wake();
                                    Thread.Sleep(Duration_Low_Power_Mode);
                                    registerData = ReadRegister(command, 3);
                                    Sleep();
                                    break;
                                }
                            case PowerModes.On:
                                {
                                    command = SHTC3_CMD_MEASURE_NORMALPOWER_CLOCKSTRETCH_HUMIDITY_FIRST;
                                    registerData = ReadRegister(command, 3);
                                    break;
                                }

                            case PowerModes.Off:
                            {
                                break;
                            }

                            default:
                            {
                                throw new ArgumentOutOfRangeException(nameof(PowerMode));
                            }
                        }

                        break;
                    }
                    case false:
                        {
                            switch (PowerMode)
                            {
                                case PowerModes.Low:
                                {
                                    command = SHTC3_CMD_MEASURE_LOWPOWER_POLLING_HUMIDITY_FIRST;
                                    Wake();
                                    Thread.Sleep(Duration_Normal_Mode);
                                    WriteRegister(command);
                                    Thread.Sleep(Duration_Normal_Mode);
                                    registerData = ReadRegister(3);
                                    Sleep();
                                    break;
                                }
                                case PowerModes.On:
                                {
                                    command = SHTC3_CMD_MEASURE_NORMALPOWER_POLLING_HUMIDITY_FIRST;
                                    WriteRegister(command);
                                    Thread.Sleep(Duration_Normal_Mode);
                                    registerData = ReadRegister(3);
                                    break;
                                }

                                case PowerModes.Off:
                                {
                                    break;
                                }

                                default:
                                {
                                    throw new ArgumentOutOfRangeException(nameof(PowerMode));
                                }
                            }

                            break;
                        }
                }

                if (ComputeCRC8(new[] { registerData[0], registerData[1] }) != registerData[2]) return Int32.MaxValue;

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
        /// Debug.WriteLine("Temperature RawData is... : " + (_sensor as ITemperature).RawData);
        /// </code>
        /// <code language = "VB">
        /// Debug.WriteLine("Temperature RawData is... : " <![CDATA[&]]> (TryCast(_sensor, ITemperature)).RawData)
        /// </code>
        /// </example>
        Int32 ITemperature.RawData
        {
            get
            {
                UInt16 command;
                Byte[] registerData = { };

                switch (ClockStretch)
                {
                    case true:
                    {
                        switch (PowerMode)
                        {
                            case PowerModes.Low:
                            {
                                command = SHTC3_CMD_MEASURE_LOWPOWER_CLOCKSTRETCH_TEMPERATURE_FIRST;
                                Wake();
                                Thread.Sleep(Duration_Low_Power_Mode);
                                registerData = ReadRegister(command, 3);
                                Sleep();
                                break;
                            }
                            case PowerModes.On:
                            {
                                command = SHTC3_CMD_MEASURE_NORMALPOWER_CLOCKSTRETCH_TEMPERATURE_FIRST;
                                registerData = ReadRegister(command, 3);
                                break;
                            }

                            case PowerModes.Off:
                            {
                                break;
                            }

                            default:
                            {
                                throw new ArgumentOutOfRangeException(nameof(PowerMode));
                            }
                        }

                        break;
                    }
                    case false:
                    {
                        switch (PowerMode)
                        {
                            case PowerModes.Low:
                            {
                                command = SHTC3_CMD_MEASURE_LOWPOWER_POLLING_TEMPERATURE_FIRST;
                                Wake();
                                Thread.Sleep(Duration_Normal_Mode);
                                WriteRegister(command);
                                Thread.Sleep(Duration_Normal_Mode);
                                registerData = ReadRegister(3);
                                Sleep();
                                break;
                            }
                            case PowerModes.On:
                            {
                                command = SHTC3_CMD_MEASURE_NORMALPOWER_POLLING_TEMPERATURE_FIRST;
                                WriteRegister(command);
                                Thread.Sleep(Duration_Normal_Mode);
                                registerData = ReadRegister(3);
                                break;
                            }

                            case PowerModes.Off:
                                break;
                            default:
                                throw new ArgumentOutOfRangeException(nameof(PowerMode));
                        }

                        break;
                    }
                }

                if (ComputeCRC8(new[] { registerData[0], registerData[1] }) != registerData[2]) return Int32.MaxValue;

                return (registerData[0] << 8) | registerData[1];
            }
        }

        #endregion
    }
}

