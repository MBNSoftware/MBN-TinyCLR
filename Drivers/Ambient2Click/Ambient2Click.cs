/*
 * Ambient2 Click driver for TinyCLR 2.0
 * 
 * Initial version by Stephen Cardinale
 *  - Not implemented:
 *    - Interrupt Generation
 *    - Interrupt Latch Operation - Requires SM Bus to clear latch
 *    - Interrupt Polarity - Not particularly useful without Interrupt Operation    
 * 
 * Copyright 2020 Stephen Cardinale and MikroBus.Net
 *
 * Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License. You may obtain a copy of the License at
 * http://www.apache.org/licenses/LICENSE-2.0
 * Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, 
 * either express or implied. See the License for the specific language governing permissions and limitations under the License.
 * 
 */

using GHIElectronics.TinyCLR.Devices.I2c;

using System;

namespace MBN.Modules
{
    /// <summary>
    /// Main class for the Ambient2 Click driver
    /// <para><b>Pins used :</b> Scl, Sda, Int</para>
    /// <para><b>This is an I2C Device.</b></para>
    /// </summary>
    /// <example>Example usage:
    /// <code language="C#">
    /// using MBN;
    /// using MBN.Modules;
    ///
    /// using System;
    /// using System.Diagnostics;
    /// using System.Threading;
    ///
    /// namespace Examples
    /// {
    ///     public static class Program
    ///     {
    ///         private static Ambient2Click _light;
    ///
    ///         public static void Main()
    ///         {
    ///                 _light = new Ambient2Click(Hardware.SocketOne, Ambient2Click.I2CAddresses.Address0)
    ///                 {
    ///                     OperatingMode = Ambient2Click.OperatingModes.Continuous,
    ///                     ConversionTime = Ambient2Click.ConversionTimes.Ms800,
    ///                     ConsecutiveFaults = Ambient2Click.Faults.OneFault,
    ///                     DetectionRange = Ambient2Click.Range.AutoFullScale,
    ///                 };
    ///
    ///             _light.SetAlertLimits(10, 50);
    ///
    ///             _light.DetectionRange = Ambient2Click.Range.AutoFullScale;
    ///
    ///             while (true)
    ///             {
    ///                 _light.ReadSensor(out Int32 luxValue, out Boolean hasAlert, out Ambient2Click.AlertType type);
    ///                 Debug.WriteLine($"LUX is {luxValue} has an alert? {hasAlert} of type {type}");
    ///                 Thread.Sleep(1000);
    ///             }
    ///         }
    ///     }
    /// }
    /// </code>
    /// </example>
    public sealed class Ambient2Click
    {
        #region CTOR

        /// <summary>
        /// Initializes a new instance of the <see cref="Ambient2Click"/> class.
        /// </summary>
        /// <param name="socket">The socket on which the Ambient2Click module is plugged on MikroBus.Net board</param>
        /// <param name="slaveAddress">The address of the module.</param>
        public Ambient2Click(Hardware.Socket socket, I2CAddresses slaveAddress)
        {
            _sensor = I2cController.FromName(socket.I2cBus).GetDevice(new I2cConnectionSettings((Int32) slaveAddress, 100000));

            if (GetDeviceId() != 0x3001) throw new DeviceInitialisationException("Ambient2 Click not found on the I2C Bus.");

            WriteRegister(ConfigurationRegister, new Byte[] { 0xC8, 0x10}); // Reset to Factory Default.
        }

        #endregion

        #region Constants

        private const Byte ResultRegister = 0x00;
        private const Byte ConfigurationRegister = 0x01;
        private const Byte LowLimitRegister = 0x02;
        private const Byte HighLimitRegister = 0x03;
        private const Byte ManufacturerIdRegister = 0x7E;
        private const Byte DeviceIdRegister = 0x7F;

        #endregion

        #region Public ENUMS

        /// <summary>
        /// The OPT3001 can utilize four (4) different I2C addresses.
        /// </summary>
        public enum I2CAddresses
        {
            /// <summary>
            /// Primary I2C Address with the Zero (0) Ohm resistor soldered in the GND position.
            /// </summary>
            Address0 = 0x44,
            /// <summary>
            /// Secondary I2C Address with the Zero (0) Ohm resistor soldered in the SDA position.
            /// </summary>
            Address1 = 0x45,
            /// <summary>
            /// Tertiary I2C Address with the Zero (0) Ohm resistor soldered in the SCL position.
            /// </summary>
            Address2 = 0x46,
            /// <summary>
            /// Quaternary I2C Address with the Zero (0) Ohm resistor soldered in the VCC position.
            /// </summary>
            Address3 = 0x47
        }

        /// <summary>
        /// The OPT3001 supports either manual range detection or Automatic Full Scale range detection.
        /// <p>Currently only AutoFullScale is supported by the OPT3001.</p>
        /// <p>In AutoFullScale, the OPT3001 will automatically adjust the range if the measured value is above the current setting. Sort of like an Auto ranging IC.</p>
        /// </summary>
        public enum Range
        {
            /// <summary>
            /// Automatic full-scale-range setting mode. This is the default value.
            /// </summary>
            AutoFullScale = 0,
            /// <summary>
            /// Reserved for future use. Do not use.
            /// </summary>
            Reserved1 = 1,
            /// <summary>
            /// Reserved for future use. Do not use.
            /// </summary>
            Reserved2 = 2,
            /// <summary>
            /// Reserved for future use. Do not use.
            /// </summary>
            Reserved3 = 3
        }

        /// <summary>
        /// The amount of time it takes to complete a light to digital conversion.
        /// </summary>
        public enum ConversionTimes
        {
            /// <summary>
            /// 100 milliseconds.
            /// </summary>
            Ms100 = 0,
            /// <summary>
            /// 800 milliseconds. This is he default value.
            /// </summary>
            Ms800 = 1
        }

        /// <summary>
        /// The OPT3001 operates in two modes. Continuous conversion (full power) or in shutdown mode.
        /// </summary>
        public enum OperatingModes
        {
            // Default
            /// <summary>
            /// Low power shutdown mode.
            /// </summary>
            Shutdown = 0,
            /// <summary>
            /// Continuous conversion or full power.
            /// </summary>
            Continuous = 1
        }

        /// <summary>
        /// The number of consecutive readings required before the FH or FL fields are triggered indicating a fault condition.
        /// </summary>
        public enum Faults
        {
            /// <summary>
            /// One fault
            /// </summary>
            OneFault = 0,
            /// <summary>
            /// Two faults
            /// </summary>
            TwoFaults = 1,
            /// <summary>
            /// Four faults
            /// </summary>
            FourFaults = 2,
            /// <summary>
            /// Eight faults
            /// </summary>
            EightFaults = 3
        }

        /// <summary>
        /// The alert types that are returned from the <see cref="ReadSensor(out int, out bool, out AlertType)"></see> method.
        /// </summary>
        public enum AlertType
        {
            /// <summary>
            /// No alert present.
            /// </summary>
            None = 0,
            /// <summary>
            /// LUX reading is below the <seealso cref="LowLimitAlert"/> setting.
            /// </summary>
            Low = 1,
            /// <summary>
            /// LUX reading is above the <seealso cref="HighLimitAlert"/> setting.
            /// </summary>
            High = 2
        }

        #endregion

        #region Fields

        private static I2cDevice _sensor;

        private static OperatingModes _operatingMode = OperatingModes.Shutdown;
        private static Byte[] _registerData = new Byte[2];
        private static Byte[] _temperatureData = new Byte[2];

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets or sets the Operating Mode of the OPT3001.
        /// <p>The Operating Mode controls whether the device is operating in continuous conversion, or low-power shutdown mode.</p>
        /// <p>The default is shutdown mode.</p>
        /// </summary>
        /// <example>Example usage:
        /// <code language="C#">
        /// _light.OperatingMode = Ambient2Click.OperatingModes.Shutdown;
        /// Debug.Print("Operating Mode is " + (_light.OperatingMode == Ambient2Click.OperatingModes.Continuous ? "Continuous" : "Shutdown"));
        /// </code>
        /// <code language = "VB">
        /// _light.OperatingMode = Ambient2Click.OperatingModes.Shutdown
        /// Debug.Print("Operating Mode is " <![CDATA[&]]> (If(_light.OperatingMode = Ambient2Click.OperatingModes.Continuous, "Continuous", "Shutdown")))
        /// </code>
        /// </example>
        /// <value>The value of the OperatingMode property.</value>
        public OperatingModes OperatingMode
        {
            get
            {
                _registerData = ReadRegister(ConfigurationRegister, 2);
                return (OperatingModes) ((_registerData[0] & 0x06) >> 1);
            }
            set
            {
                _registerData = ReadRegister(ConfigurationRegister, 2);
                _registerData[0] |= (Byte)((Byte) value << 1);
                WriteRegister(ConfigurationRegister, _registerData);
                _operatingMode = value;
            }
        }

        /// <summary>
        /// Gets or sets the ConversionTime of the OPT3001
        /// <p>This property determines the length of the light to digital conversion process. The choices are 100 ms and 800 ms. A longer integration time allows for a lower noise measurement.</p>
        /// <p>The default is 800 ms.</p>
        /// </summary>
        /// <example>Example usage:
        /// <code language="C#">
        /// _light.ConversionTime = Ambient2Click.ConversionTimes.Ms100;
        /// Debug.Print("Conversion Time is set for" + (_light.ConversionTime == Ambient2Click.ConversionTimes.Ms800 ? "800 ms" : "100 ms"));
        /// </code>
        /// <code language = "VB">
        /// _light.ConversionTime = Ambient2Click.ConversionTimes.Ms100
        /// Debug.Print("Conversion Time is set for" <![CDATA[&]]> (If(_light.ConversionTime = Ambient2Click.ConversionTimes.Ms800, "800 ms", "100 ms")))
        /// </code>
        /// </example>
        /// <value>The value of the ConversionTime property.</value>
        public ConversionTimes ConversionTime
        {
            get => Bits.IsBitSet(ReadRegister(ConfigurationRegister, 2)[0], 3) ? ConversionTimes.Ms800 : ConversionTimes.Ms100;
            set
            {
                _registerData = ReadRegister(ConfigurationRegister, 2);

                Bits.Set(ref _registerData[0], 3, value == ConversionTimes.Ms800);
                WriteRegister(ConfigurationRegister, _registerData);
            }
        }

        /// <summary>
        /// Gets or sets the DetectionRange of the device.
        /// <p><b>Currently only <see cref="Range.AutoFullScale"/> is supported by this OPT3001 IC. Other modes are reserved for future use.</b></p>
        /// <p>Setting this property to values other than <see cref="Range.AutoFullScale"/> will have no affect.</p>
        /// </summary>
        /// <example>Example usage:
        /// <code language="C#">
        /// _light.DetectionRange = Ambient2Click.Range.AutoFullScale;
        /// Debug.Print("DetectionRange setting is " + (_light.DetectionRange == Ambient2Click.Range.AutoFullScale ? "Auto Full Scale" : "Reserved"));
        /// </code>
        /// <code language = "VB">
        /// _light.OperatingMode = Ambient2Click.OperatingModes.Shutdown
        /// Debug.Print("DetectionRange setting is " <![CDATA[&]]> (If(_light.DetectionRange = Ambient2Click.Range.AutoFullScale, "Auto Full Scale", "Reserved")))
        /// </code>
        /// </example>
        /// <value>The value of the DetectionRange property.</value>
        public Range DetectionRange
        {
            get
            {
                _registerData = ReadRegister(ConfigurationRegister, 2);
                Int32 range = _registerData[0] >> 4;
                switch (range)
                {
                    case 0x0C: return Range.AutoFullScale;
                    case 0x0D: return Range.Reserved1;
                    case 0x0E: return Range.Reserved2;
                    case 0x0F: return Range.Reserved3;
                    default: throw new ApplicationException("value");
                }
            }
            set
            {
                _registerData = ReadRegister(ConfigurationRegister, 2);

                switch (value)
                {
                    case Range.Reserved1:
                    case Range.Reserved2:
                    case Range.Reserved3:
                    {
                        break;
                    }
                    case Range.AutoFullScale:
                    {
                        _registerData[0] |= 0xF0;
                        break;
                    }
                    default:
                    {
                        throw new ArgumentException(nameof(value));
                    }
                }

                WriteRegister(ConfigurationRegister, _registerData);
            }
        }

        /// <summary>
        /// Gets the LUX reading for the LowAlert setting.
        /// </summary>
        /// <example>Example usage:
        /// <code language="C#">
        /// Debug.Print("Low alert setting is " + _light.LowLimitAlert);
        /// </code>
        /// <code language = "VB">
        /// Debug.Print("Low alert setting is " <![CDATA[&]]> _light.LowLimitAlert)
        /// </code>
        /// </example>
        /// <value>The value of the LowLimitAlert property.</value>
        public Double LowLimitAlert
        {
            get
            {
                Byte[] alertValue = ReadRegister(LowLimitRegister, 2);
                return BytesToLux(alertValue);
            }
            set => WriteRegister(LowLimitRegister, LuxToBytes(value));
        }

        /// <summary>
        /// Gets the LUX reading for the HighAlert setting.
        /// </summary>
        /// <example>Example usage:
        /// <code language="C#">
        /// Debug.Print("High alert setting is " + _light.HighLimitAlert);
        /// </code>
        /// <code language = "VB">
        /// Debug.Print("High alert setting is " <![CDATA[&]]> _light.HighLimitAlert)
        /// </code>
        /// </example>
        /// <value>The value of the HighLimitAlert property.</value>
        public Double HighLimitAlert
        {
            get
            {
                Byte[] alertValue = ReadRegister(HighLimitRegister, 2);
                return BytesToLux(alertValue);
            }
            set => WriteRegister(HighLimitRegister, LuxToBytes(value));
        }

        /// <summary>
        /// Gets or sets the ConsecutiveFault Property.
        /// <p>This property instructs the OPT3001 as to how many consecutive fault events are required to trigger the flag high field (FH), and flag low field (FL)</p>
        /// </summary>
        /// <example>Example usage:
        /// <code language="C#">
        /// _light.ConsecutiveFaults = Ambient2Click.Faults.TwoFaults;
        /// Debug.Print("Consecutive fault setting is " + (_light.ConsecutiveFaults == Ambient2Click.Faults.EightFaults ? "Eight faults" : _light.ConsecutiveFaults == Ambient2Click.Faults.FourFaults ? "Four faults" : _light.ConsecutiveFaults == Ambient2Click.Faults.TwoFaults ? "Two faults" : "One fault"));
        /// </code>
        /// <code language = "VB">
        /// _light.ConsecutiveFaults = Ambient2Click.Faults.TwoFaults
        /// Debug.Print("Consecutive fault setting is " <![CDATA[&]]> (If(_light.ConsecutiveFaults = Ambient2Click.Faults.EightFaults, "Eight faults", If(_light.ConsecutiveFaults = Ambient2Click.Faults.FourFaults, "Four faults", If(_light.ConsecutiveFaults = Ambient2Click.Faults.TwoFaults, "Two faults", "One fault")))))
        /// </code>
        /// </example>
        /// <value>The value of the ConsecutiveFaults property.</value>
        public Faults ConsecutiveFaults
        {
            get
            {
                _registerData = ReadRegister(ConfigurationRegister,2);
                return (Faults) (_registerData[1] & 0x03);
            }
            set
            {
                _registerData = ReadRegister(ConfigurationRegister, 2);
                _registerData[1] &= (Byte) value;
                WriteRegister(ConfigurationRegister, _registerData);
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Resets the module
        /// </summary>
        /// <param name="resetMode">The reset mode :
        /// <para>SOFT reset : generally by sending a software command to the chip</para><para>HARD reset : generally by activating a special chip's pin</para></param>
        /// <returns>True if successful or otherwise false.</returns>
        /// <exception cref="System.NotSupportedException"> a NotSupportedException will be thrown if attempting to perform a <see cref="ResetModes.Hard"/> as this module does not support Hard resets.</exception>
        public Boolean Reset(ResetModes resetMode)
        {
            if (resetMode == ResetModes.Hard) throw new NotSupportedException("This module does not support hard resets, use ResetModes.Soft instead");
            WriteRegister(ConfigurationRegister, new []{ (Byte) 0xC8, (Byte) 0x10});
            _registerData = ReadRegister(ConfigurationRegister, 2);
            return _registerData[0] == 0xC8 && _registerData[1] == 0x10;
        }

        /// <summary>
        /// Retrieves the Device ID from the OPT3001 IC on the Ambient2 Click.
        /// </summary>
        /// <returns>A <see cref="int"/> representing the Device ID.</returns>
        /// <example>Example usage:
        /// <code language="C#">
        /// Debug.Print("Device ID is 0x" + _light.GetDeviceId().ToString("X"));
        /// </code>
        /// <code language = "VB">
        /// Debug.Print("Device ID is 0x" <![CDATA[&]]> _light.GetDeviceId().ToString("X"))
        /// </code>
        /// </example>
        public Int32 GetDeviceId()
        {
            _registerData = ReadRegister(DeviceIdRegister, 2);
            return (_registerData[0] << 8) | _registerData[1];
        }

        /// <summary>
        /// Retrieves the Manufacturer ID from the OPT3001 IC on the Ambient2 Click.
        /// </summary>
        /// <returns>A <see cref="int"/> representing the Manufacturer ID.</returns>
        /// <example>Example usage:
        /// <code language="C#">
        /// Debug.Print("Manufacturer ID is 0x" + _light.GetManufacturerId().ToString("X"));
        /// </code>
        /// <code language = "VB">
        /// Debug.Print("Manufacturer ID is 0x" <![CDATA[&]]> _light.GetManufacturerId().ToString("X"))
        /// </code>
        /// </example>
        public Int32 GetManufacturerId()
        {
            _registerData = ReadRegister(ManufacturerIdRegister, 2);
            return (_registerData[0] << 8) | _registerData[1];
        }

        /// <summary>
        /// Reads the ambient light from the Ambient2 Click.
        /// </summary>
        /// <remarks>
        /// This method will read the ambient light from the Ambient2 Click in either operating modes.
        /// If <see cref="OperatingMode"/> is set to <see cref="OperatingModes.Shutdown"/> this method will initiate a One-shot conversion automatically.
        /// </remarks>
        /// <returns>The ambient light in LUX.</returns>
        /// <example>Example usage:
        /// <code language="C#">
        /// Debug.Print("LUX is " + _light.ReadSensor());
        /// </code>
        /// <code language = "VB">
        /// Debug.Print("LUX is " <![CDATA[&]]> _light.ReadSensor())
        /// </code>
        /// </example>
        public Int32 ReadSensor()
        {
            _registerData = ReadRegister(ConfigurationRegister, 2);

            if (OperatingMode == OperatingModes.Shutdown)
            {
                //_registerData[0] = Bits.Set(_registerData[0], 1, true);
                //_registerData[0] = Bits.Set(_registerData[0], 2, false);
                Bits.Set(ref _registerData[0], "xxxxx01x");
                WriteRegister(ConfigurationRegister, _registerData);
                while (Bits.IsBitSet(ReadRegister(ConfigurationRegister,2)[0], 1)) {}
            }

            _temperatureData = ReadRegister(ResultRegister, 2);

            return BytesToLux(_temperatureData);
        }

        /// <summary>
        /// Reads the ambient light from the Ambient2 Click.
        /// </summary>
        /// <remarks>
        /// This method will read the ambient light from the Ambient2 Click in either operating modes.
        /// If <see cref="OperatingMode"/> is set to <see cref="OperatingModes.Shutdown"/> this method will initiate a One-shot conversion automatically.
        /// </remarks>
        /// <returns>The ambient light in LUX.</returns>
        /// <param name="lux">The <see cref="int"/> variable that will hold the value of the LUX parameter.</param>
        /// <param name="hasAlert">The <see cref="Boolean"/> variable that will hold the value of the HasAlarm parameter.</param>
        /// <param name="type">The <see cref="AlertType"/> variable that will hold </param>
        /// <example>Example usage:
        /// <code language="C#">
        /// while (true)
        /// {
        ///     int luxValue;
        ///     bool hasAlert;
        ///     Ambient2Click.AlertType type;
        ///     _light.ReadSensor(out luxValue, out hasAlert, out type);
        ///     Debug.Print("LUX is " + luxValue + " has an alert? " + hasAlert + " of type " + (type == Ambient2Click.AlertType.None ? "No Alert" : type == Ambient2Click.AlertType.Low ? "Low" : "High"));
        ///     Thread.Sleep(1000);
        /// }
        /// </code>
        /// <code language = "VB">
        /// While True
        ///     Dim luxValue As Integer
        ///     Dim hasAlert As Boolean
        ///     Dim type As Ambient2Click.AlertType
        ///     _light.ReadSensor(luxValue, hasAlert, type)
        ///     Debug.Print("LUX is " <![CDATA[&]]> luxValue <![CDATA[&]]> " has an alert? " <![CDATA[&]]> hasAlert <![CDATA[&]]> " of type " <![CDATA[&]]> (If(type = Ambient2Click.AlertType.None, "No Alert", If(type = Ambient2Click.AlertType.Low, "Low", "High"))))
        ///     Thread.Sleep(1000)
        /// End While
        /// </code>
        /// </example>
        public void ReadSensor(out Int32 lux, out Boolean hasAlert, out AlertType type)
        {
            type = AlertType.None;

            if (_operatingMode == OperatingModes.Shutdown)
            {
                //_registerData[0] = Bits.Set(_registerData[0], 1, true);
                //_registerData[0] = Bits.Set(_registerData[0], 2, false);
                Bits.Set(ref _registerData[0], "xxxxx01x");
                WriteRegister(ConfigurationRegister, _registerData);
                while (Bits.IsBitSet(ReadRegister(ConfigurationRegister, 2)[0], 1)) { }
            }

            _temperatureData = ReadRegister(ResultRegister, 2);
            lux = BytesToLux(_temperatureData);

            if (_operatingMode == OperatingModes.Shutdown) // The FH and FL bits are not updated in Shutdown Mode/One-shot Mode so we have to determine the AlertType the hard way.
            {
                hasAlert = lux < LowLimitAlert || lux > HighLimitAlert;
                type = lux < LowLimitAlert ? AlertType.Low : lux > HighLimitAlert ? AlertType.High : AlertType.None;
            }
            else // or we can read it directly from the FH and FL bits of the ConfigurationRegister.
            {
                _registerData = ReadRegister(ConfigurationRegister, 2);
                if (Bits.IsBitSet(_registerData[1], 5)) type = AlertType.Low;
                else if (Bits.IsBitSet(_registerData[1], 6)) type = AlertType.High;
                hasAlert = type != AlertType.None;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="lowLuxAlert"></param>
        /// <param name="highLuxAlert"></param>
        /// <example>Example usage:
        /// <code language="C#">
        /// _light.SetAlertLimits(10, 50); // Sets the LowLimit Alert to 10 LUX and the HighLimit Alert to 50 LUX.
        /// Debug.Print("LA - " + _light.LowLimitAlert);
        /// Debug.Print("HA - " + _light.HighLimitAlert);
        /// </code>
        /// <code language = "VB">
        /// _light.SetAlertLimits(10, 50) ' Sets the LowLimit Alert to 10 LUX and the HighLimit Alert to 50 LUX.
        /// Debug.Print("LA - " <![CDATA[&]]> _light.LowLimitAlert)
        /// Debug.Print("HA - " <![CDATA[&]]> _light.HighLimitAlert)
        /// </code>
        /// </example>
        public void SetAlertLimits(UInt32 lowLuxAlert, UInt32 highLuxAlert)
        {
            if (lowLuxAlert >= highLuxAlert) throw new ArgumentException("lowLuxAlert cannot be greater than or equal to the highLuxAlert value");
            WriteRegister(HighLimitRegister, LuxToBytes(highLuxAlert));
            WriteRegister(LowLimitRegister, LuxToBytes(lowLuxAlert));
        }

        #endregion

        #region Private Methods

        private static Int32 BytesToLux(Byte[] registerData)
        {
            Int32 dataSum = (registerData[0] << 8) | registerData[1];
            Int32 exponent = dataSum >> 12;
            Int32 mantissa = dataSum & 0xFFF;

            return (Int32) (0.01 * Math.Pow(2, exponent) * mantissa);
        }

        private static Byte[] LuxToBytes(Double lux)
        {
            Int32 exponent = 0;

            if (lux.IsBetween(0, 49.94, true)) exponent = 0x01;
            if (lux.IsBetween(49.95, 81.89, true)) exponent = 0x02;
            if (lux.IsBetween(81.90, 163.79, true)) exponent = 0x04;
            if (lux.IsBetween(163.80, 327.59, true)) exponent = 0x08;
            if (lux.IsBetween(327.60, 655.19, true)) exponent = 0x10;
            if (lux.IsBetween(655.20, 1310.39, true)) exponent = 0x20;
            if (lux.IsBetween(1310.40, 2620.79, true)) exponent = 0x40;
            if (lux.IsBetween(2620.80, 5241.59, true)) exponent = 0x80;
            if (lux.IsBetween(5241.60, 10483.19, true)) exponent = 0x100;
            if (lux.IsBetween(10483.20, 20966.39, true)) exponent = 0x200;
            if (lux.IsBetween(20966.40, 41932.79, true)) exponent = 0x400;
            if (lux.IsBetween(41932.80, 83865.60, true)) exponent = 0x800;
            if (lux > 83865.61) exponent = 2048; // This is the maximum value that the module can read. I wouldn't expect to see a value above this, but just in case.

            Int32 mantissa = (Int32) (lux * 100 / exponent);

            _registerData[0] = (Byte) ((exponent.Log(2) << 4) | ((mantissa & 0xFF00) >> 8));
            _registerData[1] = (Byte) (mantissa & 0xFF);

            return _registerData;
        }

        // Note data returned is MSB first.
        private static Byte[] ReadRegister(Byte registerAddress, Byte numberOfBytesToRead)
        {
            Byte[] data = new Byte[numberOfBytesToRead];

            lock (Hardware.LockI2C)
            {
                _sensor.WriteRead(new[] {registerAddress}, data);
            }

            return data;
        }

        // Note: Date is written MSB first.
        private static void WriteRegister(Byte registerAddress, Byte[] data)
        {
            Byte[] writeBuffer = new Byte[3];
            writeBuffer[0] = registerAddress;
            writeBuffer[1] = data[0];
            writeBuffer[2] = data[1];

            lock (Hardware.LockI2C)
            {
                _sensor.Write(writeBuffer);
            }
        }

        #endregion
    }

    /// <summary>
    ///     An extension class for the between operation
    ///     name pattern IsBetweenXX where X = I -> Inclusive, X = E -> Exclusive
    /// </summary>
    public static class BetweenExtensions
    {
        /// <summary>
        ///     Between check <![CDATA[lowerBounds <= value <= upperBounds]]>
        /// </summary>
        /// <param name="value">the value to check</param>
        /// <param name="lowerBounds">The minimum bounds for comparison</param>
        /// <param name="upperBounds">The maximum bounds for comparison</param>
        /// <param name="inclusive">Set to true to include bounds values in or false to exclude bounds values.</param>
        /// <returns>Return true if the value is between the min and max else false</returns>
        public static Boolean IsBetween(this Double value, Double lowerBounds, Double upperBounds,
            Boolean inclusive = false)
        {
            return inclusive
                ? lowerBounds <= value && value <= upperBounds
                : lowerBounds < value && value < upperBounds;
        }
    }

    /// <summary>
    ///     An extension class to extend System.Math Log method.
    /// </summary>
    public static class IntegerExtensions
    {
        /// <summary>
        ///     Calculate logarithmic value from value with given base
        /// </summary>
        /// <param name="x">The Number</param>
        /// <param name="base">The base to use</param>
        /// <returns>Logarithmic of x</returns>
        public static Int32 Log(this Int32 x, Int32 @base)
        {
            Single partial = 0.5F;
            Single integer = 0F;
            Single fractional = 0.0F;

            if (Math.Abs(x) < 0.0) throw new ArgumentOutOfRangeException(nameof(x));
            if ((x < 1.0F) & (@base < 1.0F)) throw new ArgumentOutOfRangeException(nameof(x));

            while (x < 1.0F)
            {
                integer -= 1F;
                x *= @base;
            }

            while (x >= @base)
            {
                integer += 1F;
                x /= @base;
            }

            x *= x;

            while (partial >= 2.22045e-16F)
            {
                if (x >= @base)
                {
                    fractional += partial;
                    x /= @base;
                }

                partial *= 0.5F;
                x *= x;
            }

            return (Int32) (integer + fractional);
        }
    }

}

