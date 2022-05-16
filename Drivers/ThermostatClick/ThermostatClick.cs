/*
 * ThermostatClick driver for TinyCLR 2.0
 * 
 * Initial revision coded by Stephen Cardinale
 * 
 * Copyright 2020 Stephen Cardinale
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
#else
using GHIElectronics.TinyCLR.Devices.Gpio;
using GHIElectronics.TinyCLR.Devices.I2c;
#endif

using System;
using System.Threading;

using Math = System.Math;

#endregion

namespace MBN.Modules
{
    /// <inheritdoc cref="ITemperature" />
    /// <summary>
    /// Main class for the ThermostatClick driver
    /// <para><b>Pins used :</b> Scl, Sda, Rst, Cs</para>
    /// <para><b>This is an I2C Device</b></para>
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
    ///     public class Program
    ///     {
    ///         private static ThermostatClick _sensor;
    ///
    ///         public static void Main()
    ///         {
    ///             _sensor = new ThermostatClick(Hardware.SC20100_2, ThermostatClick.I2CAddress.AddressOne)
    ///             {
    ///                 TemperatureThreshold = 28.5f,
    ///                 TemperatureHysteresis = 26.5f,
    ///                 OperatingMode = ThermostatClick.OperatingModes.Comparator,
    ///                 Polarity = ThermostatClick.InterruptPolarity.ActiveHigh,
    ///                 TemperatureUnit = TemperatureUnits.Celsius
    ///             };
    ///
    ///             while (true)
    ///             {
    ///                 Debug.WriteLine($"Temperature................: {_sensor.ReadTemperature():F1} °C");
    ///                 Thread.Sleep(1000);
    ///             }
    ///         }
    ///     }
    /// }
    /// </code>
    /// </example>
    public class ThermostatClick : ITemperature
    {
        #region .ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="ThermostatClick"/> class.
        /// </summary>
        /// <param name="socket">The socket on which the ThermostatClick module is plugged on MikroBus.Net board</param>
        /// <param name="address">The <see cref="I2CAddress"/> of the module.</param>
        public ThermostatClick(Hardware.Socket socket, I2CAddress address)
        {
            _socket = socket;
#if (NANOFRAMEWORK_1_0)
            _sensor = I2cDevice.Create(new I2cConnectionSettings(socket.I2cBus, (int)address, I2cBusSpeed.StandardMode));

            _resetPin = new GpioController().OpenPin(socket.Rst);
            _resetPin.SetPinMode(PinMode.Output);
            _resetPin.Write(PinValue.Low);
            Thread.Sleep(1);
            _resetPin.Write(PinValue.High);

            GpioPin enablePin = new GpioController().OpenPin(socket.Cs);
            enablePin.SetPinMode(PinMode.Output);
            enablePin.Write(PinValue.High);
#else
			_sensor = I2cController.FromName(socket.I2cBus).GetDevice(new I2cConnectionSettings((UInt16) address, 100000));
            
            _resetPin = GpioController.GetDefault().OpenPin(socket.Rst);
            _resetPin.SetDriveMode(GpioPinDriveMode.Output);
            _resetPin.Write(GpioPinValue.Low);
            Thread.Sleep(1);
            _resetPin.Write(GpioPinValue.High);

            GpioPin enablePin = GpioController.GetDefault().OpenPin(socket.Cs);
            enablePin.SetDriveMode(GpioPinDriveMode.Output);
            enablePin.Write(GpioPinValue.High);
#endif
        }

        #endregion

        #region Private Constants

        private const Byte MAX7502_REG_TEMPERATURE = 0X00;
        private const Byte MAX7502_REG_CONFIGURATION = 0X01;
        private const Byte MAX7502_REG_THYST = 0X02;
        private const Byte MAX7502_REG_TOS = 0X03;

        #endregion

        #region Private Fields

        private readonly I2cDevice _sensor;
        private readonly Hardware.Socket _socket;
        private readonly GpioPin _resetPin;

        #endregion

        #region Public Enumerations

        /// <summary>
        /// Possible I2C 7 bit addresses based on the position of the Address Select Jumpers A0 and A1.
        /// </summary>
        public enum I2CAddress
        {
            /// <summary>
            /// Address one (0x48) with the A0 Address Select Jumper soldered to the 0 position and the A1 Address Select Jumper soldered in the 0 position.
            /// </summary>
            AddressOne = 0x48,
            /// <summary>
            /// Address two (0x49) with the A0 Address Select Jumper soldered to the 1 position and the A1 Address Select Jumper soldered in the 0 position.
            /// </summary>
            AddressTwo = 0x49,
            /// <summary>
            /// Address three (0x4A) with the A0 Address Select Jumper soldered to the 0 position and the A1 Address Select Jumper soldered in the 1 position.
            /// </summary>
            AddressThree = 0x4A,
            /// <summary>
            /// Address four (0x4B) with the A0 Address Select Jumper soldered to the 1 position and the A1 Address Select Jumper soldered in the 1 position.
            /// </summary>
            AddressFour = 0x4B
        }

        /// <summary>
        /// The operating mode for the Over-temperature (OT) status bit of the config register.
        /// </summary>
        public enum OperatingModes
        {
            /// <summary>
            /// Comparator mode of operation.
            /// <para><b>This is the default POR setting.</b></para>
            /// </summary>
            /// <remarks>The OT status bit has a value of 1 when the temperature rises above the Temperature Threshold (TOS) value (subject to the Fault Queue selection). See <see cref="ThermostatClick.TemperatureThreshold"/> and <see cref="ThermostatClick.ConsecutiveFaults"/>.</remarks>
            Comparator = 0,
            /// <summary>
            /// Interrupt mode of operation.
            /// </summary>
            ///<remarks>The OT status bit has a value of 1 when exceeding Temperature Threshold (TOS). OT Status remains set to 1 until a read operation is performed on any of the registers, at which point it returns to 0. Once OT Status has been set to 1 due to crossing above TOS and is then reset, it is set to 1 again only when the temperature drops below THYST. The output then remains asserted until it is reset by a read. It is then set again if the temperature rises above TOS , and so on.</remarks>
            Interrupt = 1
        }

        /// <summary>
        /// Selects how many consecutive over-temperature faults must occur before an over-temperature fault is indicated in the Over-temperature Status bit of the configuration register.
        /// </summary>
        /// <remarks>
        /// The fault queue selection applies to both Comparator and Interrupt modes. The Fault Queue does not apply to de-asserting the over-temperature status when the measured temperature drops below <see cref="ThermostatClick.TemperatureHysteresis"/> (THYST).
        /// </remarks>
        public enum FaultQueue
        {
            /// <summary>
            /// OverTemperature status bit (bit 15) of the configuration register is asserted after one (1) fault.
            /// <para><b>This is the default POR setting.</b></para>
            /// </summary>
            OneFault = 0,
            /// <summary>
            /// OverTemperature status bit (bit 15) of the configuration register is asserted after two (2) consecutive faults.
            /// </summary>
            TwoFaults = 1,
            /// <summary>
            /// OverTemperature status bit (bit 15) of the configuration register is asserted after four (4) consecutive faults.
            /// </summary>
            FourFaults = 2,
            /// <summary>
            /// OverTemperature status bit (bit 15) of the configuration register is asserted after six (6) consecutive faults.
            /// </summary>
            SixFaults = 3
        }

        /// <summary>
        /// Polarity of the OS pin of the MAX7052 IC.
        /// </summary>
        public enum InterruptPolarity
        {
            /// <summary>
            /// Relay de-asserts (opens) when temperature exceeds <see cref="ThermostatClick.TemperatureThreshold"/> value and re-asserts when temperature drops below <see cref="ThermostatClick.TemperatureHysteresis"/> value;
            /// </summary>
            ActiveLow = 0,
            /// <summary>
            /// Relay asserts (closes) when temperature exceeds <see cref="ThermostatClick.TemperatureThreshold"/> value and de-asserts when temperature drops below <see cref="ThermostatClick.TemperatureHysteresis"/> value;
            /// </summary>
            ActiveHigh = 1
        }

        #endregion

        #region Private Methods

        private static Int32 ConvertTwosComplementToDecimal(Byte twosComplementByte)
        {
            if ((twosComplementByte & 0x80) != 0x80) return twosComplementByte;
            Int32 result = ~twosComplementByte & 0xFF;
            result += 1;
            result *= -1;
            return result;
        }

        private static Byte[] ConvertTemperatureToTwosComplimentByteArray(Double temperature)
        {
            Int32 integralPortion = (Int32) Math.Truncate(temperature);
            Double fractionalPortion = temperature % 1;

            Byte[] result = new Byte[2];

            result[0] = integralPortion.TwoComplement();

            if ((result[0] & 0x80) == 0x80) result[0] -= 1;
            result[1] = (Byte) ((Byte)(fractionalPortion / 0.5) << 7);

            return result;
        }

        private void WriteRegister(Byte register, Byte[] value)
        {
            Byte[] writeBuffer = new Byte[3];
            writeBuffer[0] = register;
            writeBuffer[1] = value[0];
            writeBuffer[2] = value[1];

            lock (_socket.LockI2c)
            {
                _sensor.Write(writeBuffer);
            }
        }

        private void WriteByte(Byte register, Byte value)
        {
            Byte[] writeBuffer = new Byte[2];
            writeBuffer[0] = register;
            writeBuffer[1] = value;

            lock (_socket.LockI2c)
            {
                _sensor.Write(writeBuffer);
            }
        }

        private Byte[] ReadRegister(Byte registerAddress, Byte bytesToRead)
        {
            Byte[] readBuffer = new Byte[bytesToRead];

            lock (_socket.LockI2c)
            {
                _sensor.WriteRead(new[] {registerAddress}, readBuffer);
            }

            return readBuffer;
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Sets or Gets the number of consecutive over-temperature faults must occur before an over-temperature fault is indicated.
        /// <para>The default POR is 1 fault.</para>
        /// </summary>
        /// <example>Example usage:
        /// <code language="C#">
        /// _sensor.ConsecutiveFaults = ThermostatClick.FaultQueue.TwoFaults;
        /// </code>
        /// </example>
        public FaultQueue ConsecutiveFaults
        {
            get
            {
                Byte registerData = ReadRegister(MAX7502_REG_CONFIGURATION, 1)[0];
                return (FaultQueue) ((registerData >> 3) & 0x03);
            }
            set
            {
                Byte registerData = ReadRegister(MAX7502_REG_CONFIGURATION, 1)[0];

                switch (value)
                {
                    case FaultQueue.OneFault:
                    {
                            Bits.Set(ref registerData, "xxx0xxx");
                        break;
                    }
                    case FaultQueue.TwoFaults:
                    {
                            Bits.Set(ref registerData, "xxx01xxx");
                        break;
                    }
                    case FaultQueue.FourFaults:
                    {
                            Bits.Set(ref registerData, "xxx10xxx");
                        break;
                    }
                    case FaultQueue.SixFaults:
                    {
                            Bits.Set(ref registerData, "xxx11xxx");
                        break;
                    }
                    default:
                    {
                        throw new ArgumentOutOfRangeException(nameof(value));
                    }
                }

                WriteByte(MAX7502_REG_CONFIGURATION, registerData);
            }
        }

        ///<summary>
        /// Gets or sets the behavior of the onboard relay output. Default POR value is Active Low.
        /// </summary>
        /// <example>Example usage:
        /// <code language = "C#">
        /// _sensor.Polarity = ThermostatClick.InterruptPolarity.ActiveHigh;
        /// </code>
        /// </example>
        public InterruptPolarity Polarity
        {
            get
            {
                Byte registerData = ReadRegister(MAX7502_REG_CONFIGURATION, 1)[0];
                return (InterruptPolarity) (registerData >> 2);
            }
            set
            {
                Byte registerData = ReadRegister(MAX7502_REG_CONFIGURATION, 1)[0];
                Bits.Set(ref registerData, 2, value == InterruptPolarity.ActiveHigh);
                WriteByte(MAX7502_REG_CONFIGURATION, registerData);
            }
        }

        ///<summary>
        /// Gets or sets the behavior of the onboard relay. Default POR is Comparator mode.
        /// <para>
        /// In comparator mode, behavior is like a thermostat. The output asserts when the temperature rises above the limit set in the <see cref="TemperatureThreshold"/> property.
        /// The output de-asserts when the temperature falls below the limit set in the <see cref="TemperatureHysteresis"/>THYST property.
        /// In comparator mode when used in conjunction with the <see cref="Polarity"/>, the relay output can be used to turn on or off a cooling fan or heating element.
        /// </para>
        /// <para>
        /// In interrupt mode, exceeding <see cref="TemperatureThreshold"/> also asserts the relay output. The relay remains asserted until a read operation is performed on any of the registers.
        /// Once OS has asserted due to crossing above <see cref="TemperatureThreshold"/> and is then reset, it is asserted again only when the temperature drops below <see cref="TemperatureHysteresis"/>.
        /// The output remains asserted until it is reset by a read.
        /// </para>
        /// </summary>
        /// <example>Example usage:
        /// <code language = "C#">
        /// _sensor.OperatingMode = ThermostatClick.OperatingModes.Interrupt;
        /// Debug.Print("OperatingMode is " + (_sensor.OperatingMode == ThermostatClick.OperatingModes.Interrupt ? "Interrupt Mode" : "Comparator Mode"));
        /// </code>
        /// </example>
        public OperatingModes OperatingMode
        {
            get
            {
                Byte registerData = ReadRegister(MAX7502_REG_CONFIGURATION, 1)[0];
                return (OperatingModes) (registerData >> 1);
            }
            set
            {
                Byte registerData = ReadRegister(MAX7502_REG_CONFIGURATION, 1)[0];
                Bits.Set(ref registerData, 1, value == OperatingModes.Interrupt);
                WriteByte(MAX7502_REG_CONFIGURATION, registerData);

            }
        }

        /// <summary>
        /// The <see cref="TemperatureUnits"/> in which to report temperature measurements with.
        /// </summary>
        /// <para>Default POR value is °C.</para>
        /// <example>Example usage:
        /// <code language="C#">
        /// _sensor.TemperatureUnit = TemperatureUnits.Fahrenheit;
        /// </code>
        /// </example>
        public TemperatureUnits TemperatureUnit { get; set; } = TemperatureUnits.Celsius;

        /// <summary>
        /// The temperature hysteresis (low setting) used for OS signaling . See data-sheet for information on usage of the Temperature Threshold Registers (TOS and THyst).
        /// </summary>
        /// <remarks>Note that TemperatureHysteresis (THYST) is intended to have values less than or equal to TemperatureThreshold (TOS). Therefore, a THYST value greater than TOS will be interpreted as being equal to TOS.</remarks>
        /// <example>Example usage:
        /// <code language="C#">
        ///  _sensor.TemperatureHysteresis = -20.25f;
        /// </code>
        /// </example>
        public Single TemperatureHysteresis
        {
            get
            {
                Byte[] registerData = ReadRegister(MAX7502_REG_THYST, 2);

                Int32 integerPortion = ConvertTwosComplementToDecimal(registerData[0]);
                Single fractionalPortion = (registerData[1] >> 7) * 0.5f;
                return integerPortion + fractionalPortion;
            }
            set
            {
                if (value > 125) value = 125f;
                if (value < -55) value = -55;

                WriteRegister(MAX7502_REG_THYST, ConvertTemperatureToTwosComplimentByteArray(value));
            }
        }

        /// <summary>
        /// The temperature threshold (high setting) used for OS signaling . See data-sheet for information on usage of the Temperature Threshold Registers (TOS and THyst).
        /// </summary>
        /// <remarks>Note that TemperatureThreshold (TOS) is intended to have values greater than or equal to TemperatureHysteresis (THYST). Therefore, a TOS value greater than TOS will be interpreted as being equal to THYST.</remarks>
        /// <example>Example usage:
        /// <code language="C#">
        /// _sensor.TemperatureThreshold = 50.25F;
        /// </code>
        /// </example>
        public Single TemperatureThreshold
        {
            get
            {
                Byte[] registerData = ReadRegister(MAX7502_REG_TOS, 2);

                Int32 integerPortion = ConvertTwosComplementToDecimal(registerData[0]);
                Single fractionalPortion = (registerData[1] >> 7) * 0.5f;
                return integerPortion + fractionalPortion;
            }
            set
            {
                if (value > 125) value = 125f;
                if (value < -55) value = -55;

                WriteRegister(MAX7502_REG_TOS, ConvertTemperatureToTwosComplimentByteArray(value));
            }
        }

        /// <summary>
        /// Gets or sets the power mode.
        /// </summary>
        /// <remarks>When power mode is set to <see cref="PowerModes.Low"/>, the temperature read will be the last temperature measured when power mode was set to <see cref="PowerModes.On"/>.</remarks>
        /// <example> This sample shows how to use the PowerMode property.
        /// <code language="C#">
        ///  _sensor.PowerMode = PowerModes.Low;
        /// </code>
        /// </example>
        /// <value>
        /// The current power mode of the module.
        /// </value>
        /// <exception cref="T:System.NotSupportedException">A NotSupportedException will be thrown if attempting to set this property to PowerModes.Off as this module does not support PowerModes.Off.</exception>
        public PowerModes PowerMode
        {
            get
            {
                Byte registerData = ReadRegister(MAX7502_REG_CONFIGURATION, 1)[0];
                return (registerData & 0x01) == 0x01 ? PowerModes.Low : PowerModes.On;
            }
            set
            {
                if (value == PowerModes.Off) throw new NotSupportedException("This module does not support PowerModes.Off. Use PowerModes.Low instead.");
                Byte registerData = ReadRegister(MAX7502_REG_CONFIGURATION, 1)[0];
                Bits.Set(ref registerData, 0, value == PowerModes.Low);
                WriteByte(MAX7502_REG_CONFIGURATION, registerData);
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Resets the module
        /// </summary>
        /// <example>
        /// <code language = "C#">
        /// _sensor.Reset(ResetModes.Hard);
        /// </code>
        /// </example>
        /// <exception cref="T:System.NotSupportedException">A NotSupportedException will be thrown as this module has no soft reset feature.</exception>
        public Boolean Reset(ResetModes resetMode)
        {
            if (resetMode == ResetModes.Soft) throw new NotSupportedException("This module does not have a soft reset feature. Use a hard reset instead.");
#if (NANOFRAMEWORK_1_0)
            _resetPin.Write(PinValue.Low);
            Thread.Sleep(1);
            _resetPin.Write(PinValue.High);
#else
            _resetPin.Write(GpioPinValue.Low);
            Thread.Sleep(1);
            _resetPin.Write(GpioPinValue.High);
#endif

            return true;
        }

#endregion

#region Interface Implementations

        /// <inheritdoc cref="ITemperature" />
        /// <summary>
        ///     Reads the temperature from the Thermo 6 Click.
        /// </summary>
        /// <param name="source">The source temperature to read. In this case, only reading of ambient temperature is supported.</param>
        /// <returns>A <see cref="System.Single" /> precision number representing the temperature.</returns>
        /// <exception cref="NotSupportedException">
        ///     A NotSupportedException will be thrown if attempting to read object temperature
        ///     as this module does not support reading of object temperature.
        /// </exception>
        /// <remarks>
        ///     If attempting to read temperature while in Low Power mode, will result in the temperature not getting updated.
        /// </remarks>
        /// <example>
        /// <code language="C#">
        /// while (true)
        /// {
        ///     Debug.Print("Temperature........: " + _sensor.ReadTemperature().ToString("F4") + " °F");
        ///     Thread.Sleep(1000);
        /// }
        /// </code>
        /// </example>
        public Single ReadTemperature(TemperatureSources source = TemperatureSources.Ambient)
        {
            if (source == TemperatureSources.Object)
                throw new NotSupportedException("This module does not support reading of object temperatures.");

            Int32 rawData = (this as ITemperature).RawData;

            Int32 integralPortion = ConvertTwosComplementToDecimal((Byte) (rawData >> 8));
            Single fractionalPortion = ((rawData & 0xFF) >> 7) * 0.5F;

            switch (TemperatureUnit)
            {
                case TemperatureUnits.Celsius:
                {
                    return integralPortion + fractionalPortion;
                }
                case TemperatureUnits.Kelvin:
                {
                    return integralPortion + fractionalPortion + 273.13f;
                }
                case TemperatureUnits.Fahrenheit:
                {
                    return (integralPortion + fractionalPortion) * 1.80f + 32F;
                }
                default:
                {
                    throw new ArgumentOutOfRangeException();
                }
            }
        }

        /// <inheritdoc cref="ITemperature" />
        /// <summary>Gets the raw data of the temperature value.</summary>
        /// <value>
        /// Raw data in the range depending on sensor's precision (8/10/12 bits, for example)
        /// </value>
        /// <example>
        /// <code language = "C#">
        /// Int32 rawData = (_sensor as ITemperature).RawData;
        /// Debug.Print(rawData.ToString());
        /// </code>
        /// </example>
        public Int32 RawData
        {
            get
            {
                Byte[] registerData = ReadRegister(MAX7502_REG_TEMPERATURE, 2);
                return (registerData[0] << 8) | registerData[1];
            }
        }

#endregion
    }
}

