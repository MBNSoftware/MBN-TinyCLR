/*
 * Thermo 4 Click driver for TinyCLR 2.0
 * 
 * Initial revision coded by Stephen Cardinale
 * 
 * Copyright 2020 Stephen Cardinale
 * Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License. You may obtain a copy of the License at
 * http://www.apache.org/licenses/LICENSE-2.0
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

#endregion
namespace MBN.Modules
{
    /// <summary>
    /// Main class for the Thermo 4 Click driver
    /// <para><b>Pins used :</b> Scl, Sda, Int</para>
    /// <para><b>This is an I2C Device</b></para>
    /// </summary>
    /// <example>
    /// <code language = "C#">
    /// using System.Threading;
    /// using MBN;
    /// using MBN.Enums;
    /// using MBN.Modules;
    /// using Microsoft.SPOT;
    /// 
    /// namespace Example2
    /// {
    ///     public class Program
    ///     {
    ///         private static Thermo4Click _sensor;
    /// 
    ///         public static void Main()
    ///         {
    ///             Debug.Print(Resources.GetString(Resources.StringResources.String1));
    ///             Debug.EnableGCMessages(false);
    /// 
    ///             _sensor = new Thermo4Click(Hardware.SocketOne, Thermo4Click.I2cAddress.AddressOne, ClockRatesI2C.Clock400KHz) {TemperatureUnit = TemperatureUnits.Fahrenheit};
    /// 
    ///             _sensor.SetConfiguration(Thermo4Click.OSMode.Comparator, Thermo4Click.OSPolarity.ActiveLow, Thermo4Click.OSFaultQueue.OneFault);
    ///             _sensor.PowerMode = PowerModes.On;
    /// 
    ///             while (true)
    ///             {
    ///                 Debug.Print("Temperature : " + _sensor.ReadTemperature().ToString("F1") + " �F");
    ///                 Thread.Sleep(2000);
    ///             }
    ///         }
    ///     }
    /// }
    /// </code>
    /// </example>
    public class Thermo4Click : ITemperature
    {
        #region .ctor

        /// <summary>
        /// 
        /// </summary>
        /// <param name="socket"></param>
        /// <param name="slaveAddress"></param>
        public Thermo4Click(Hardware.Socket socket, I2cAddress slaveAddress)
        {
            _socket = socket;
#if (NANOFRAMEWORK_1_0)
            _sensor = I2cDevice.Create(new I2cConnectionSettings(socket.I2cBus, (int)slaveAddress, I2cBusSpeed.StandardMode));
#else
			_sensor = I2cController.FromName(socket.I2cBus).GetDevice(new I2cConnectionSettings((Int32) slaveAddress, 100000));
#endif
        }

        #endregion

        #region Private Fields

        private I2cDevice _sensor;
        private readonly Hardware.Socket _socket;
        #endregion

        #region Private Constants

        private const Byte LM75A_REG_TEMP = 0x00;
        private const Byte LM75A_REG_CONF = 0x01;
        private const Byte LM57A_REG_THYST = 0x02;
        private const Byte LM57A_REG_TOS = 0x03;

        #endregion

        #region Public ENUMS

        /// <summary>
        /// Available I2C Addresses for the Thermo 4 Click
        /// </summary>
        public enum I2cAddress
        {
            /// <summary>
            /// I2C Address is 0x48 with Jumper A0, A1 and A2 soldered in the 0 position.
            /// </summary>
            AddressOne = 0x48,

            /// <summary>
            /// I2C Address is 0x49 with Jumper A0 soldered in the 1 position and Jumper A1 and A2 soldered in the 0 position.
            /// </summary>
            AddressTwo = 0x49,

            /// <summary>
            /// I2C Address is 0x4A with Jumper A0 and A2 soldered in the 0 position and Jumper A1 soldered in the 1 position.
            /// </summary>
            AddressThree = 0x4A,

            /// <summary>
            /// I2C Address is 0x4B with Jumper A0 and A1 soldered in the 1 position and Jumper A2 soldered in the 0 position.
            /// </summary>
            AddressFour = 0x4B,

            /// <summary>
            /// I2C Address is 0x4c with Jumper A0 and A1 soldered in the 0 position and Jumper A2 soldered in the 1 position.
            /// </summary>
            AddressFive = 0x4C,

            /// <summary>
            /// I2C Address is 0x4D with Jumper A0 and A2 soldered in the 1 position and Jumper A1 soldered in the 0 position.
            /// </summary>
            AddressSix = 0x4D,

            /// <summary>
            /// I2C Address is 0x4E with Jumper A0 soldered in the 0 position and Jumper A1 and A2 soldered in the 1 position.
            /// </summary>
            AddressSeven = 0x4E,

            /// <summary>
            /// I2C Address is 0x4F with Pins A0, A1 and A2 soldered in the 1 position.
            /// </summary>
            AddressEight = 0x4F
        }

        /// <summary>
        /// Fault queue is defined as the number of faults that must occur consecutively to activate the Over Temperature Shutdown output.
        /// </summary>
        public enum OSFaultQueue
        {
            /// <summary>
            /// One fault (POR default value).
            /// </summary>
            OneFault = 0x00,

            /// <summary>
            /// Two consecutive faults to trigger Over Temperature Shutdown output.
            /// </summary>
            TwoFaults = 0x01,

            /// <summary>
            /// Four consecutive faults to trigger Over Temperature Shutdown output.
            /// </summary>
            FourFaults = 0x02,

            /// <summary>
            /// Six consecutive faults to trigger Over Temperature Shutdown output.
            /// </summary>
            SixFaults = 0x03
        }

        /// <summary>
        /// Polarity conffiguration for the Overtemperature Shutdown Pin (socket.Int)
        /// </summary>
        public enum OSPolarity
        {
            /// <summary>
            /// Overtemperature Shutdown Pin output is Active Low. (POR default value)
            /// </summary>
            ActiveLow = 0x00,

            /// <summary>
            /// Overtemperature Shutdown Pin output is Active High.
            /// </summary>
            ActiveHigh = 0x01
        }

        /// <summary>
        /// The LM74A IC on Thermo 4 Click can operate in two modes of operation. Comparator (thermostat) or Interrupt.
        /// </summary>
        public enum OSMode
        {
            /// <summary>
            /// In OS comparator mode, the OS output behaves like a thermostat.
            /// It becomes active when the Temp exceeds the Tos,
            /// and is reset when the Temp drops below the Thyst.
            /// </summary>
            Comparator = 0x00,

            /// <summary>
            /// In OS interrupt mode, the OS output is used for thermal interruption.
            /// The OS output is first activated only when the Temp exceeds the <see cref="OvertemperatureShutdownThreshold"/> (Tos);
            /// then it remains active indefinitely until being reset by a read of any register.
            /// Once the OS output has been activated by crossing Tos and then reset,
            /// it can be activated again only when the Temp drops below the <see cref="HysterisisTemperatureThreshold"/> (Thyst).
            /// </summary>
            Interrupt = 0x01
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// The Hysterisis (Thyst) or lower boundry threshold temperature.
        /// In Comparator mode, the Overtemperature Shutdown will reset when the Temp drops below the Thyst value.
        /// </summary>
        public Single HysterisisTemperatureThreshold
        {
            get
            {
                Byte[] registerData = ReadRegister(LM57A_REG_THYST, 2);
                return ConvertTwosComplementByteArrayToTemperature(registerData, 7);
            }
            set
            {
                if (value < -55) value = -55f;
                if (value > 127) value = 125f;
                WriteRegister(LM57A_REG_THYST, ConvertTemperatureToTwosComplimentByteArray(value, 7));
            }
        }

        /// <summary>
        /// Overtemperature Shutdown threshold (Tos) is the temperature at which the Overtemperature Shutdown will become active
        /// </summary>
        public Single OvertemperatureShutdownThreshold
        {
            get
            {
                Byte[] registerData = ReadRegister(LM57A_REG_TOS, 2);
                return ConvertTwosComplementByteArrayToTemperature(registerData, 7);
            }
            set
            {
                if (value < -55) value = -55f;
                if (value > 127) value = 125f;
                WriteRegister(LM57A_REG_TOS, ConvertTemperatureToTwosComplimentByteArray(value, 7));
            }
        }

        /// <summary>
        /// Gets or sets the number of consecutive faults to trigger a Overtemperature Shudtown condition.
        /// </summary>
        public OSFaultQueue FaultQueue
        {
            get
            {
                Byte registerData = ReadRegister(LM75A_REG_CONF, 1)[0];
                return (OSFaultQueue) ((registerData & 0x18) >> 3);
            }
            set
            {
                Byte registerData = ReadRegister(LM75A_REG_CONF, 1)[0];

                switch (value)
                {
                    case OSFaultQueue.OneFault:
                    {
                        Bits.Set(ref registerData, "xxx00xxx");
                        break;
                    }

                    case OSFaultQueue.TwoFaults:
                    {
                        Bits.Set(ref registerData, "xxx01xxx");
                        break;
                    }

                    case OSFaultQueue.FourFaults:
                    {
                        Bits.Set(ref registerData, "xxx10xxx");
                        break;
                    }

                    case OSFaultQueue.SixFaults:
                    {
                        Bits.Set(ref registerData, "xxx11xxx");
                        break;
                    }

                    default:
                    {
                        throw new ArgumentOutOfRangeException(nameof(value));
                    }
                }

                WriteByte(LM75A_REG_CONF, registerData);
            }
        }

        /// <summary>
        /// Gets or sets the mode of operation of the LM75A IC on the Thermo 4 Click.
        /// </summary>
        public OSMode Mode
        {
            get
            {
                Byte registerData = ReadRegister(LM75A_REG_CONF, 1)[0];
                return (OSMode) ((registerData & 0x02) >> 1);
            }
            set
            {
                Byte registerData = ReadRegister(LM75A_REG_CONF, 1)[0];
                registerData ^= 0x02;
                registerData |= (Byte) ((Byte) value << 1);
                WriteByte(LM75A_REG_CONF, registerData);
            }
        }

        /// <summary>
        /// The polarity of the Overtemperature Shutdown pin (socket.Int). Either Active Low or Active High.
        /// </summary>
        public OSPolarity Polarity
        {
            get
            {
                Byte registerData = ReadRegister(LM75A_REG_CONF, 1)[0];
                return (OSPolarity) ((registerData & 0x04) >> 2);
            }
            set
            {
                Byte registerData = ReadRegister(LM75A_REG_CONF, 1)[0];
                registerData ^= 0x04;
                registerData |= (Byte) ((Byte) value << 2);
                WriteByte(LM75A_REG_CONF, registerData);
            }
        }

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
        public TemperatureUnits TemperatureUnit { get; set; } = TemperatureUnits.Celsius;

        #endregion

        #region Public Methods

        /// <summary>
        /// Sets the configuration of the LM75A IC on the Thermo4 Click.
        /// </summary>
        /// <param name="mode">The <see cref="Mode"/> to use.</param>
        /// <param name="polarity">The polarity of the OverTemperature Shudtown pin in a fault condition.</param>
        /// <param name="faltqueue">The number of consecutive faults needed to trigger a fault condition.</param>
        public void SetConfiguration(OSMode mode, OSPolarity polarity, OSFaultQueue faltqueue)
        {
            Byte configuration = (Byte) (((Byte) faltqueue << 3) | ((Byte) polarity << 2) | ((Byte) mode << 1));
            WriteByte(LM75A_REG_CONF, configuration);
        }

        #endregion

        #region Private Methods

        private void WriteByte(Byte registerAddress, Byte value)
        {
            lock (_socket.LockI2c)
            {
                _sensor.Write(new [] {registerAddress, value });
            }
        }

        private void WriteRegister(Byte registerAddress, Byte[] data)
        {
            lock (_socket.LockI2c)
            {
                _sensor.Write(new [] {registerAddress, data[0], data[1] });
            }
        }

        private Byte[] ReadRegister(Byte registerAddress, Byte numberOfBytesToRead)
        {
            Byte[] registerData = new Byte[numberOfBytesToRead];

            lock (_socket.LockI2c)
            {
                _sensor.WriteRead(new[] {registerAddress}, registerData);
            }
            return registerData;
        }

        private Single ConvertTwosComplementByteArrayToTemperature(Byte[] data, Byte bitLength)
        {
            Int32 integralPortion = data[0].TwoComplement();
            Single fractionalPortion =
                (data[1].TwoComplement() >> (bitLength == 9 ? 5 : 7)) * (bitLength == 9 ? 0.125f : 0.5f);
            return integralPortion + fractionalPortion;
        }

        private Byte[] ConvertTemperatureToTwosComplimentByteArray(Double temperature, Byte bitLength)
        {
            Int32 integralPortion = (Int32) temperature;

            Double fractionalPortion = temperature % 1;

            Byte[] result = new Byte[2];

            result[0] = integralPortion.TwoComplement();

            result[1] = (Byte) ((Byte) (fractionalPortion / (bitLength == 9 ? 0.125 : 0.5)) << (16 - bitLength));

            return result;
        }

        private Single ScaleTemperature(Single temperatureData)
        {
            switch (TemperatureUnit)
            {
                case TemperatureUnits.Fahrenheit:
                {
                    temperatureData *= 1.8F;
                    temperatureData += 32;
                    break;
                }

                case TemperatureUnits.Kelvin:
                {
                    temperatureData += 273.15F;
                    break;
                }

                case TemperatureUnits.Celsius:
                {
                    break;
                }

                default:
                {
                    return temperatureData;
                }
            }

            return temperatureData;
        }

        #endregion

        #region Interface Implementations

        /// <summary>Gets or sets the power mode.</summary>
        /// <value>The current power mode of the module.</value>
        /// <remarks>If the module has no power modes, then GET should always return PowerModes.ON while SET should throw a NotImplementedException.</remarks>
        public PowerModes PowerMode
        {
            get => (PowerModes) ((ReadRegister(LM75A_REG_CONF, 1)[0] & 0x01) + 1);
            set
            {
                if (value == PowerModes.Low) throw new ArgumentException("This module does not support PowerModes.Low.");
                var registerData = ReadRegister(LM75A_REG_CONF, 1);
                if (value == PowerModes.Off)
                    registerData[0] &= 0b11111110;
                else
                    registerData[0] |= 0b00000001;
                WriteByte(LM75A_REG_CONF, registerData[0]);
            }
        }

        /// <summary>Reads the temperature.</summary>
        /// <param name="source">The source.</param>
        /// <returns>A single representing the temperature read from the source, degrees Celsius</returns>
        public Single ReadTemperature(TemperatureSources source = TemperatureSources.Ambient)
        {
            if (source == TemperatureSources.Object) throw new NotSupportedException("This module does not support reading of Object temperature. Please use Ambient instead.");
            Byte[] registerData = ReadRegister(LM75A_REG_TEMP, 2);
            return ScaleTemperature(ConvertTwosComplementByteArrayToTemperature(registerData, 9));
        }

        /// <summary>Gets the raw data of the temperature value.</summary>
        /// <value>
        /// Raw data in the range depending on sensor's precision (8/10/12 bits, for example)
        /// </value>
        public Int32 RawData
        {
            get
            {
                Byte[] registerData = ReadRegister(LM75A_REG_TEMP, 2);
                return (registerData[0] << 8) | registerData[1];
            }
        }

        #endregion
    }
}