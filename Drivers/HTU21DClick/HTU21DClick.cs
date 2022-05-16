/*
 * HTU21DClick driver for TinyCLR 2.0
 *  Version 1.0
 *  - Initial version
 *  
 * Copyright 2020 Stephen Cardinale
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
    /// <summary>
    /// Main class for the HTU21DClick driver
    /// <para>This module is an I2C Device. <b>Pins used :</b> Scl, Sda</para>
    /// </summary>
    /// <example>Example usage:
    /// <code language = "C#">
    /// using MBN;
    /// using MBN.Modules;
    /// using System.Diagnostics;
    /// using System.Threading;
    /// 
    /// namespace Examples
    /// {
    ///     class Program
    ///     {
    ///         private static HTU21DClick _sensor;
    /// 
    ///         public static void Main()
    ///         {
    ///             _sensor = new HTU21DClick(Hardware.SocketTwo)
    ///             {
    ///                 MeasurementMode = HTU21DClick.ReadMode.Hold,
    ///                 Resolution = HTU21DClick.DeviceResolution.UltraHigh
    ///             };
    /// 
    ///             while (true)
    ///             {
    ///                 Debug.WriteLine("Humidity    " + _sensor.ReadHumidity(HumidityMeasurementModes.Relative).ToString("n2") + " %RH");
    ///                 Debug.WriteLine("Temperature " + _sensor.ReadTemperature(TemperatureSources.Ambient).ToString("n2") + " °C");
    ///                 Thread.Sleep(100);
    ///             }
    ///         }
    ///     }
    /// }
    /// </code>
    /// </example>
    public sealed class HTU21DClick : ITemperature, IHumidity
    {
        #region CTOR

        /// <summary>
        /// Initializes a new instance of the <see cref="HTU21DClick"/> class.
        /// </summary>
        /// <param name="socket">The socket on which the HTU21DClick module is plugged on MikroBus.Net board</param>
        public HTU21DClick(Hardware.Socket socket)
        {
            _socket = socket;
#if (NANOFRAMEWORK_1_0)
            _htu = I2cDevice.Create(new I2cConnectionSettings(socket.I2cBus, HTDU21D_WRITE_ADDRESS, I2cBusSpeed.FastMode));
#else
            _htu = I2cController.FromName(socket.I2cBus)
                .GetDevice(new I2cConnectionSettings(HTDU21D_WRITE_ADDRESS, 400000));
#endif

            Reset(ResetModes.Soft);
        }

        #endregion

        #region Constants

        private const Byte HTDU21D_WRITE_ADDRESS = 0x40; //0x80 >> 1;
        private const Byte TRIGGER_TEMP_MEASURE_HOLD = 0xE3;
        private const Byte TRIGGER_HUMD_MEASURE_HOLD = 0xE5;
        private const Byte TRIGGER_TEMP_MEASURE_NOHOLD = 0xF3;
        private const Byte TRIGGER_HUMD_MEASURE_NOHOLD = 0xF5;
        private const Byte WRITE_USER_REG = 0xE6;
        private const Byte READ_USER_REG = 0xE7;
        private const Byte SOFT_RESET = 0xFE;
        private const UInt32 SHIFTED_DIVISOR = 0x988000; // Used for CRC Checksum calculation.

        #endregion

        #region Fields

        private readonly I2cDevice _htu;
        private readonly Hardware.Socket _socket;

        #endregion

        #region ENUMS

        /// <summary>
        /// Enumeration for the measurement resolution settings for the HTU21DClick.
        /// </summary>
        public enum DeviceResolution : Byte
        {
            /// <summary>
            /// 8-bit RH and 12-bit temperature. Measurement times are between 2 - 3 ms for Humidity and between 6 - 7 ms for Temperature.
            /// </summary>
            Low = 0,

            /// <summary>
            /// 10-bit RH and 13-bit temperature. Measurement times are between 4 - 5 ms for Humidity and between 11 - 13 ms for Temperature.
            /// </summary>
            Standard = 1,

            /// <summary>
            ///  11-bit RH and 11-bit temperature. Measurement times are between 7 - 8 ms for Humidity and between 22 - 25 ms for Temperature.
            /// </summary>
            High = 2,

            /// <summary>
            ///  12-bit RH and 14-bit temperature. Measurement times are between 14 - 16 ms for Humidity and between 44 - 50 ms for Temperature.
            /// <para>This is the default DeviceResolution upon power up and after reset.</para>
            /// </summary>
            UltraHigh = 3
        }

        /// <summary>
        /// There are two different operation modes to communicate with the HTU21DClick, Hold Master mode and No Hold Master mode.
        /// </summary>
        /// <remarks>In the NoHold mode allows for processing other I²C communication tasks on a bus while the HTU21D sensor is measuring.
        /// In the Hold mode, the HTU21D(F) pulls down the SCK line while measuring to force the master into a wait state. By releasing the SCK line, the HTU21D Click indicates that internal processing is completed and that transmission may be continued.</remarks>
        public enum ReadMode : Byte
        {
            /// <summary>
            /// Hold Master mode, the HTU21D(F) pulls down the SCK line while measuring to force the master into a wait state. By releasing the SCK line, the HTU21D Click indicates that internal processing is completed and that transmission may be continued.
            /// </summary>
            Hold = 0,

            /// <summary>
            /// No Hold Master mode allows for processing other I²C communication tasks on a bus while the HTU21D(F) sensor is measuring.
            /// </summary>
            NoHold = 1
        }

        internal enum MeasurementType
        {
            Humidity = 0,
            Temperature = 1
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets or Sets the reading resolution of the HUD212D click. See <see cref="DeviceResolution"/> for additional information.
        /// </summary>
        /// <example>Example usage:
        /// <code language = "C#">
        /// sensor.Resolution = HTU21DClick.DeviceResolution.Standard;
        /// </code>
        /// </example>
        public DeviceResolution Resolution
        {
            get
            {
                Byte userRegister = ReadUserRegister();

                if (!Bits.IsBitSet(userRegister, 7) && Bits.IsBitSet(userRegister, 0))
                {
                    return DeviceResolution.Low;
                }

                if (Bits.IsBitSet(userRegister, 7) && !Bits.IsBitSet(userRegister, 0))
                {
                    return DeviceResolution.Standard;
                }

                if (Bits.IsBitSet(userRegister, 7) && Bits.IsBitSet(userRegister, 0))
                {
                    return DeviceResolution.High;
                }

                if (!Bits.IsBitSet(userRegister, 7) && !Bits.IsBitSet(userRegister, 0))
                {
                    return DeviceResolution.UltraHigh;
                }

                return DeviceResolution.Standard;
            }
            set
            {
                var userRegister = ReadUserRegister();

                switch (value)
                {
                    case DeviceResolution.UltraHigh:
                    {
                        userRegister = Bits.Set(userRegister, 7, false);
                        userRegister = Bits.Set(userRegister, 0, false);
                        break;
                    }

                    case DeviceResolution.High:
                    {
                        userRegister = Bits.Set(userRegister, 7, true);
                        userRegister = Bits.Set(userRegister, 0, true);
                        break;
                    }

                    case DeviceResolution.Standard:
                    {
                        userRegister = Bits.Set(userRegister, 7, true);
                        userRegister = Bits.Set(userRegister, 0, false);

                        break;
                    }

                    case DeviceResolution.Low:
                    {
                        userRegister = Bits.Set(userRegister, 7, false);
                        userRegister = Bits.Set(userRegister, 0, true);
                        break;
                    }
                }

                var command = new Byte[2];
                command[0] = WRITE_USER_REG;
                command[1] = userRegister;

                WriteRegister(command);
            }
        }

        /// <summary>
        /// Gets the End of Battery (low voltage detection) status of the HTD21D Click. Detects and notifies of VDD voltages below 2.25V. Accuracy is ±0.1V.
        /// </summary>
        /// <returns>True is VDD is below 2.25, otherwise false.</returns>
        /// <remarks>The EndOfBattery Bit in the UserRegister is only updated after an measurement is complete. This method is included for completeness of the driver. Usage is not practical as the HUD21DClick is powered by the MBN main board and will be supplied a constant 3.3V VDD. If the HTU21D is powered remotely, this might come in handy.</remarks>
        /// <example>Example usage:
        /// <code language="C#">
        /// Debug.WriteLine("End Of Battery : " + sensor.EndOfBattery);
        /// </code>
        /// </example>
        public Boolean EndOfBattery
        {
            get
            {
                var userRegister = ReadUserRegister();
                return Bits.IsBitSet(userRegister, 6);
            }
        }

        /// <summary>
        /// Enable or disables the on-chip heater of the HTU21D Click.
        /// </summary>
        /// <remarks>The heater is intended to be used for functionality diagnosis: relative humidity drops upon rising temperature. The heater consumes about 5.5mW and provides a temperature increase of about 0.5-1.5°C.</remarks>
        /// <returns>True if the on-chip heater is enabled or otherwise false.</returns>
        /// <example>Example usage:
        /// <code language="C#">
        /// sensor.Heater = true;
        /// Debug.WriteLine("Heater is -  " + sensor.Heater ? "On" : "Off");
        /// sensor.Heater = false;
        /// Debug.WriteLine("Heater is -  " + sensor.Heater ? "On" : "Off");
        /// </code>
        /// </example>
        public Boolean Heater
        {
            get
            {
                var userRegister = ReadUserRegister();
                return Bits.IsBitSet(userRegister, 2);
            }
            set
            {
                var userRegister = ReadUserRegister();
                userRegister = Bits.Set(userRegister, 2, value);

                var command = new Byte[2];
                command[0] = WRITE_USER_REG;
                command[1] = userRegister;

                WriteRegister(command);
            }
        }

        /// <summary>
        /// Sets the OTPReload (One-time Programmable Memory) setting.
        /// </summary>
        /// <value>If set to false, deactivates reloading the calibration data before each measurement.</value>
        /// <remarks>OTP reload is a safety feature and load the entire OTP settings to the register, with the exception of the heater bit, before every measurement. This feature is disabled per default and it is not recommended for use. Please use soft reset instead as it contains OTP reload.</remarks>
        /// <example>Example usage:
        /// <code language="C#">
        /// sensor.OTPReload = true;
        /// Debug.WriteLine("OTPReload : " + sensor.OTPReload);
        /// </code>
        /// </example>
        public Boolean OTPReload
        {
            get
            {
                var userRegister = ReadUserRegister();
                return Bits.IsBitSet(userRegister, 1);
            }
            set
            {
                var userRegister = ReadUserRegister();
                userRegister = Bits.Set(userRegister, 1, value);

                var command = new Byte[2];
                command[0] = WRITE_USER_REG;
                command[1] = userRegister;

                WriteRegister(command);
            }
        }

        /// <summary>
        /// Sets or gets the communication mode with the HTU21D click. See <see cref="ReadMode"/> for additional information.
        /// </summary>
        /// <example>Example usage:
        /// <code language = "C#">
        /// sensor.MeasurementMode = HTU21DClick.ReadMode.Hold;
        /// </code>
        /// </example>
        public ReadMode MeasurementMode { get; set; } = ReadMode.NoHold;

        /// <summary>
        /// Gets the raw data of the humidity value.
        /// </summary>
        /// <value>
        /// Raw data in the range depending on sensor's precision (8/10/12 bits, for example)
        /// </value>
        /// <exception cref="System.NotImplementedException">A NotImplementedException will be thrown if the Get accessor for this property is attempted.</exception>
        /// <example>Example usage:
        /// <code language="C#">
        ///  // None provided as this module does not support PowerModes.
        /// </code>
        /// </example>
        Int32 IHumidity.RawData => throw new NotImplementedException("RawData is not supported by this module.");

        /// <summary>
        /// Gets the raw data of the temperature value.
        /// </summary>
        /// <value>
        /// Raw data in the range depending on sensor's precision (8/10/12 bits, for example)
        /// </value>
        /// <exception cref="System.NotImplementedException">A NotImplementedException will be thrown if the Get accessor for this property is attempted.</exception>
        /// <example>Example usage:
        /// <code language="C#">
        ///  // None provided as this module does not support PowerModes.
        /// </code>
        /// </example>
        Int32 ITemperature.RawData => throw new NotImplementedException("RawData is not supported by this module.");

        #endregion

        #region Public Methods

        /// <summary>
        /// Returns the temperature as read from the HTU21D Click in degrees Celsius.
        /// </summary>
        /// <param name="source">The measurement source. See <see cref="TemperatureSources"/> for more information.</param>
        /// <returns>Temperature in degrees Celsius</returns>
        /// <exception cref="NotImplementedException">A NotImplementedException will be thrown is attempting to read <see cref="TemperatureSources.Object"/></exception>
        /// <example>Example usage:
        /// <code language = "C#">
        /// Debug.WriteLine("Temperature " + sensor.ReadTemperature(TemperatureSources.Ambient).ToString("n2") + " °C");
        /// </code>
        /// </example>
        public Single ReadTemperature(TemperatureSources source = TemperatureSources.Ambient)
        {
            if (source == TemperatureSources.Object)
                throw new NotImplementedException(
                    "Object temperature measurement not supported. Use TemperatureSources.Ambient for temperature measurement.");

            return ReadSensor(MeasurementMode, MeasurementType.Temperature);
        }

        /// <summary>
        /// Returns the humidity as read from the HTU21D Click in degrees % RH.
        /// </summary>
        /// <param name="measurementMode">The measurement mode. See <see cref="HumidityMeasurementModes"/> for more information.</param>
        /// <returns>The humidity as read from the HTU21D click in %RH.</returns>
        /// <exception cref="NotImplementedException">A NotImplementedException will be thrown is attempting to read <see cref="HumidityMeasurementModes.Absolute"/></exception>
        /// <example>Example usage:
        /// <code language = "C#">
        /// Debug.WriteLine("Humidity - " + sensor.ReadHumidity(HumidityMeasurementModes.Relative).ToString("n2") + " %RH");
        /// </code>
        /// </example>
        public Single ReadHumidity(HumidityMeasurementModes measurementMode = HumidityMeasurementModes.Relative)
        {
            if (measurementMode == HumidityMeasurementModes.Absolute)
                throw new NotImplementedException(
                    "Absolute Humidity measurement not supported. Use HumidityMeasurementModes.Absolute for humidity measurement.");

            return ReadSensor(MeasurementMode, MeasurementType.Humidity);
        }

        /// <summary>
        /// Resets the HTU21D Click.
        /// </summary>
        /// <param name="resetMode">The reset mode :
        /// <para>SOFT reset : generally by sending a software command to the chip</para><para>HARD reset : generally by activating a special chip's pin.</para></param>
        /// <returns>True if the reset was successful, or otherwise false.</returns>
        /// <exception cref="System.NotImplementedException">A NotImplementedException will be thrown if a <see cref="ResetModes.Hard"/> is attempted. Use <see cref="ResetModes.Soft"/> to reset this module.</exception>
        /// <example>Example usage:
        /// <code language = "C#">
        /// sensor.Reset(ResetModes.Soft);
        /// </code>
        /// </example>
        public Boolean Reset(ResetModes resetMode)
        {
            if (resetMode == ResetModes.Hard)
                throw new NotImplementedException(
                    "Hard resets are not supported. Use ResetModes.Soft to reset this module");

            var buffer = new Byte[1];
            buffer[0] = SOFT_RESET;
            var returnvalue = WriteRegister(buffer);
            Thread.Sleep(15);

            return returnvalue;
        }

        #endregion

        #region Private Methods

        private void ReadWait(Byte[] readBuffer)
        {
            lock (_socket.LockI2c)
            {
                _htu.Read(readBuffer);
            }
        }

        private Boolean WriteRegister(Byte[] writeBuffer)
        {
            lock (_socket.LockI2c)
            {
                _htu.Write(writeBuffer);
            }

            return true;
        }

        private Byte[] WriteReadRegister(Byte[] writeBuffer, Byte[] readBuffer)
        {
            lock (_socket.LockI2c)
            {
                _htu.WriteRead(writeBuffer, readBuffer);
            }

            return readBuffer;
        }

        private Single ReadSensor(ReadMode mode, MeasurementType type)
        {
            var writeBuffer = new Byte[1];
            var readBuffer = new Byte[3];

            switch (mode)
            {
                case ReadMode.Hold:
                {
                    //writeBuffer[0] = TRIGGER_HUMD_MEASURE_HOLD;
                    writeBuffer[0] = type == MeasurementType.Temperature
                        ? TRIGGER_TEMP_MEASURE_HOLD
                        : TRIGGER_HUMD_MEASURE_HOLD;
                    readBuffer = WriteReadRegister(writeBuffer, readBuffer);
                    break;
                }

                case ReadMode.NoHold:
                {
                    //writeBuffer[0] = TRIGGER_HUMD_MEASURE_NOHOLD;
                    writeBuffer[0] = type == MeasurementType.Temperature
                        ? TRIGGER_TEMP_MEASURE_NOHOLD
                        : TRIGGER_HUMD_MEASURE_NOHOLD;
                    WriteRegister(writeBuffer);
                    ReadWait(readBuffer);
                    break;
                }
            }

            var raw = ((UInt32) readBuffer[0] << 8) | readBuffer[1];

            if (CheckCrc(raw, readBuffer[2]) != 0) return Single.MinValue; // Checksum fail.

            raw &= 0xFFFC;

            var tempValue = raw / (Single) 65536;

            return type == MeasurementType.Temperature ? (Single) (-46.85 + 175.72 * tempValue) : -6 + 125 * tempValue;
        }

        private static Byte CheckCrc(UInt32 value, UInt32 checksum)
        {
            var remainder = value << 8;
            remainder &= checksum;

            var divsor = SHIFTED_DIVISOR;

            for (var i = 0; i < 16; i++)
            {
                if ((remainder & ((UInt32) 1 << (23 - i))) == 1) remainder ^= divsor;
                divsor >>= 1;
            }

            return (Byte) remainder;
        }

        private Byte ReadUserRegister()
        {
            var userRegister = new Byte[1];
            var writeRegister = new Byte[1];
            writeRegister[0] = READ_USER_REG;
            WriteReadRegister(writeRegister, userRegister);
            return userRegister[0];
        }

        #endregion
    }
}