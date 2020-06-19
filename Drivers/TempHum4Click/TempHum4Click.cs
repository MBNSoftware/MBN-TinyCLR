/*
 * TempHum4 Click TinyCLR driver for TinyCLR 2.0
 * 
 * Initial revision coded by Stephen Cardinale
 *  
 * Copyright 2020 Stephen Cardinale and MikroBUS.Net
 * Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License. You may obtain a copy of the License at
 * http://www.apache.org/licenses/LICENSE-2.0
 * Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, 
 * either express or implied. See the License for the specific language governing permissions and limitations under the License.
 */

using GHIElectronics.TinyCLR.Devices.Gpio;
using GHIElectronics.TinyCLR.Devices.I2c;

using System;
using System.Threading;

namespace MBN.Modules
{
    /// <summary>
    /// Main class for the TempHum 4 click
    /// <para><b>Pins used: Sda, Scl, RST, Cs and Int</b></para>
    /// <para><b>This is an I2C Device.</b></para>
    /// </summary>
    /// <example>
    /// <code language = "C#">
    /// using MBN;
    /// using MBN.Modules;
    ///
    /// using System;
    /// using System.Diagnostics;
    /// using System.Threading;
    ///
    /// namespace Examples
    /// {
    ///     public class Program
    ///     {
    ///         private static TempHum4Click _sensor;
    ///
    ///         private static Single temperature, humidity;
    ///
    ///         public static void Main()
    ///         {
    ///             _sensor = new TempHum4Click(Hardware.SocketOne, TempHum4Click.I2CAddress.AddressOne) {TemperatureUnit = TemperatureUnits.Fahrenheit};
    ///
    ///             _sensor.ConfigureSensor(TempHum4Click.AcquisitionModes.Sequential, TempHum4Click.TemperatureResolutions.ElevenBit, TempHum4Click.HumidityResolutions.EightBit, TempHum4Click.HeaterModes.Disabled);
    ///
    ///             Debug.WriteLine("Manufacturer ID - 0x" + _sensor.GetManufacturerId().ToString("X"));
    ///             Debug.WriteLine("Device ID - 0x" + _sensor.GetDeviceId().ToString("X"));
    ///             Debug.WriteLine("SN - " + _sensor.GetSerialNumber());
    ///
    ///             Thread.Sleep(5000);
    ///
    ///             while (true)
    ///             {
    ///                 /* For Sequential Acquisition Mode use the following code to obtain temperature and humidity data
    ///                    You will need to change the Acquisition Mode to Sequential.*/
    ///                 _sensor.ReadSensor(out temperature, out humidity);
    ///                 Debug.WriteLine($"Temperature....: {temperature:F2} °F");
    ///                 Debug.WriteLine($"Humidity.......: {humidity:F2} %RH");
    ///
    ///                 /* For independent measurement mode use the following code to obtain temperature and humidity data
    ///                    You will need to change the Acquisition Mode to Independant.*/
    ///                 //Debug.WriteLine($"Temperature....: {_sensor.ReadTemperature():F2} °F");
    ///                 //Debug.WriteLine($"Humidity.......: {_sensor.ReadHumidity():F2} %RH");
    ///
    ///                 Thread.Sleep(2000);
    ///             }
    ///         }
    ///     }
    /// }
    /// </code>
    /// </example>
    public sealed class TempHum4Click :  ITemperature, IHumidity
    {
        #region .ctor

        /// <summary>
        ///     Initializes a new instance of the <see cref="TempHum4Click" /> class.
        /// </summary>
        /// <param name="socket">The socket on which the TempHum4Click module is plugged on MikroBus.Net board</param>
        /// <param name="slaveAddress"></param>
        public TempHum4Click(Hardware.Socket socket, I2CAddress slaveAddress)
        {
            _addressPin0 = GpioController.GetDefault().OpenPin(socket.Rst);
            _addressPin0.SetDriveMode(GpioPinDriveMode.Output);
            _addressPin0.Write(GpioPinValue.Low);

            _addressPin1 = GpioController.GetDefault().OpenPin(socket.Cs);
            _addressPin1.SetDriveMode(GpioPinDriveMode.Output);
            _addressPin1.Write(GpioPinValue.Low);

            switch (slaveAddress)
            {
                case I2CAddress.AddessZero:
                {
                    _addressPin0.Write(GpioPinValue.Low);
                    _addressPin1.Write(GpioPinValue.Low);
                    break;
                }

                case I2CAddress.AddressOne:
                {
                    _addressPin0.Write(GpioPinValue.High);
                    _addressPin1.Write(GpioPinValue.Low);
                    break;
                }

                case I2CAddress.AddressTwo:
                {
                    _addressPin0.Write(GpioPinValue.Low);
                    _addressPin1.Write(GpioPinValue.High);
                    break;
                }

                case I2CAddress.AddressThree:
                {
                    _addressPin0.Write(GpioPinValue.High);
                    _addressPin1.Write(GpioPinValue.High);
                    break;
                }

                default:
                {
                    throw new ArgumentOutOfRangeException(nameof(slaveAddress));
                }
            }

            _dataReadyPin = GpioController.GetDefault().OpenPin(socket.Int);
            _dataReadyPin.SetDriveMode(GpioPinDriveMode.InputPullUp);

            _sensor = I2cController.FromName(socket.I2cBus).GetDevice(new I2cConnectionSettings((Int32) slaveAddress, 100000));

            Reset();

            if (GetManufacturerId() != 0x5449 && GetDeviceId() != 0x1000)
            {
                throw new DeviceInitialisationException("TempHum4 click not found on I2C Bus");
            }

            // Set default configuration for Sequential acquisition, 14-bit Temperature, 14 Bit Humidity and HeaterMode Off.
            ConfigureSensor(AcquisitionModes.Sequential,
                TemperatureResolutions.FourteenBit,
                HumidityResolutions.FourteenBit,
                HeaterModes.Disabled);
        }

        #endregion

        #region Private Fields

        private readonly I2cDevice _sensor;

        private readonly GpioPin _addressPin0;
        private readonly GpioPin _addressPin1;
        private readonly GpioPin _dataReadyPin;

        #endregion

        #region Constants

        private const Byte TempRegister = 0x00;
        private const Byte HumidityRegister = 0x01;
        private const Byte ConfigRegister = 0x02;
        private const Byte ManufacturerIdRegister = 0xFE;
        private const Byte DeviceIdRegister = 0xFF;

        private const Byte Hdc1010Serial1 = 0xFB;
        private const Byte Hdc1010Serial2 = 0xFC;
        private const Byte Hdc1010Serial3 = 0xFD;

        #endregion

        #region Public Enums

        /// <summary>
        /// Available I2C Address of the TempHum 4 Click
        /// </summary>
        public enum I2CAddress
        {
            /// <summary>
            /// Address is AddressZero (0x40).
            /// </summary>
            AddessZero = 0x40,

            /// <summary>
            /// Address is AddressOne (0x41).
            /// </summary>
            AddressOne = 0x41,

            /// <summary>
            /// Address is AddressTwo (0x42).
            /// </summary>
            AddressTwo = 0x42,

            /// <summary>
            /// Address is AddressThree (0x43).
            /// </summary>
            AddressThree = 0x43
        }

        /// <summary>
        ///     Measurement acquisition mode.
        /// </summary>
        public enum AcquisitionModes
        {
            /// <summary>
            ///     Independent, measures temperature or humidity independently.
            /// </summary>
            Independant = 0x00,

            /// <summary>
            ///     Sequential, measures temperature first and then humidity sequentially in one read.
            /// </summary>
            Sequential = 0x01
        }

        /// <summary>
        ///     Temperature Measurement Resolution
        /// </summary>
        public enum TemperatureResolutions
        {
            /// <summary>
            ///     Eleven (11) Bit resolution with a conversion time of 3.65 milliseconds.
            /// </summary>
            FourteenBit = 0x00,

            /// <summary>
            ///     Fourteen (14) Bit resolution with a conversion time of 6.35 milliseconds.
            /// </summary>
            ElevenBit = 0x01
        }

        /// <summary>
        ///     Humidity Measurement Resolution
        /// </summary>
        public enum HumidityResolutions
        {
            /// <summary>
            ///     Eight (8) Bit resolution with a conversion time of 2.5 milliseconds.
            /// </summary>
            EightBit = 0x02,

            /// <summary>
            ///     Eleven (11) Bit resolution with a conversion time of 3.85 milliseconds.
            /// </summary>
            ElevenBit = 0x01,

            /// <summary>
            ///     Fourteen (14) Bit resolution with a conversion time of 6.5 milliseconds.
            /// </summary>
            FourteenBit = 0x00
        }

        /// <summary>
        ///     The on-chip Heater functionality enumeration.
        /// </summary>
        public enum HeaterModes
        {
            /// <summary>
            ///     The heater is enabled. The heater helps in reducing the accumulated offset after long exposure at high humidity
            ///     conditions.
            ///     Once enabled the heater is turned on only in the measurement mode. To have a reasonable increase of the temperature
            ///     it is suggested to increase the measurement data rate.
            /// </summary>
            Enabled = 0x01,

            /// <summary>
            ///     The HeaterMode is disabled.
            /// </summary>
            Disabled = 0x00
        }

        #endregion

        #region Public Properties

        public AcquisitionModes AcquisitionMode
        {
            get
            {
                Byte[] registerData = ReadRegister(ConfigRegister, 2);
                return (AcquisitionModes) ((registerData[0] & 0x10) >> 4);
            }
            set
            {
                Byte[] registerData = ReadRegister(ConfigRegister, 2);
                registerData[0] ^= 0x10;
                registerData[0] |= (Byte)((Byte)value << 4);
                WriteRegister(ConfigRegister,  new[] { registerData[0], registerData[1]});
            }
        }

        public TemperatureResolutions TemperatureResolution
        {
            get
            {
                Byte[] registerData = ReadRegister(ConfigRegister, 2);
                return (TemperatureResolutions) ((registerData[0] & 0x04) >> 2);
            }
            set
            {
                Byte[] registerData = ReadRegister(ConfigRegister, 2);
                registerData[0] ^= 0x04;
                registerData[0] |= (Byte)((Byte)value << 2);
                WriteRegister(ConfigRegister,  new[] { registerData[0], registerData[1]});
            }
        }

        public HumidityResolutions HumidityResolution
        {
            get
            {
                Byte[] registerData = ReadRegister(ConfigRegister, 2);
                return (HumidityResolutions) (registerData[0] & 0x03);
            }
            set
            {
                Byte[] registerData = ReadRegister(ConfigRegister, 2);
                registerData[0] ^= 0x03;
                registerData[0] |= (Byte) value;
                WriteRegister(ConfigRegister,  new[] { registerData[0], registerData[1]});
            }
        }

        public HeaterModes HeaterMode
        {
            get
            {
                Byte[] registerData = ReadRegister(ConfigRegister, 2);
                return (HeaterModes) ((registerData[0] & 0x20)>> 5);
            }
            set
            {
                Byte[] registerData = ReadRegister(ConfigRegister, 2);
                registerData[0] ^= 0x20;
                registerData[0] |= (Byte)((Byte)value << 5);
                WriteRegister(ConfigRegister,  new[] { registerData[0], registerData[1]});
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Configures the TempHum4 Click for temperature, humidity acquisition.
        /// </summary>
        /// <param name="acquisitionMode">The <see cref="AcquisitionModes" /> for measurement.</param>
        /// <param name="temperatureResolution">The <see cref="TemperatureResolution" /> for reading temperature data.</param>
        /// <param name="humidityResolution">The <see cref="HumidityResolution" /> for reading humidity data.</param>
        /// <param name="heaterModes">The <see cref="HeaterModes" /> for enabling or disabling the on-board heater.</param>
        /// <example>
        /// Example usage:
        /// <code language="C#">
        /// _sensor.Configure(TempHum4Click.AcquisitionMode.Sequential, TempHum4Click.TemperatureResolution.FourteenBit, TempHum4Click.HumidityResolution.FourteenBit, TempHum4Click.HeaterMode.Disabled);
        /// </code>
        /// </example>
        public void ConfigureSensor(AcquisitionModes acquisitionMode, TemperatureResolutions temperatureResolution, HumidityResolutions humidityResolution, HeaterModes heaterModes)
        {
            Int32 value = (Byte)acquisitionMode << 4;
            value |= (Byte)heaterModes << 5;
            value |= (Byte)temperatureResolution << 2;
            value |= (Byte) humidityResolution;

            lock (Hardware.LockI2C)
            {
                _sensor.Write(new Byte []{ ConfigRegister, (Byte)value, 0x00});
            }

            Thread.Sleep(15);
        }

        /// <summary>
        ///     Gets the Device ID of the TempHum4 Click.
        /// </summary>
        /// <returns>The Device ID</returns>
        /// <example>
        ///     Example usage:
        ///     <code language="C#">
        /// Debug.WriteLine("Device ID - 0x" + _sensor.GetDeviceId().ToString("X"));
        /// </code>
        /// </example>
        public Int32 GetDeviceId()
        {
            Byte[] id = ReadRegister(DeviceIdRegister, 2);
            return id[1] | (id[0] << 8);
        }

        /// <summary>
        ///     Gets the Manufacturer ID of the TempHum4 Click
        /// </summary>
        /// <returns>The Manufacturer ID</returns>
        /// <example>
        ///     Example usage:
        ///     <code language="C#">
        /// Debug.WriteLine("Manufacturer ID - 0x" + _sensor.GetManufacturerId().ToString("X"));
        /// </code>
        /// </example>
        public Int32 GetManufacturerId()
        {
            Byte[] id = ReadRegister(ManufacturerIdRegister, 2);
            return id[1] | (id[0] << 8);
        }

        /// <summary>
        ///     Gets the unique Serial Number of the TempHum4 Click
        /// </summary>
        /// <returns>The Serial Number</returns>
        /// <example>
        ///     Example usage:
        ///     <code language="C#">
        /// Debug.WriteLine("SN - " + _sensor.GetSerialNumber());
        /// </code>
        /// </example>
        public UInt64 GetSerialNumber()
        {
            Byte[] snValue = new Byte[5];

            Byte[] readBuffer1 = ReadRegister(Hdc1010Serial1, 2);
            Byte[] readBuffer2 = ReadRegister(Hdc1010Serial2, 2);
            Byte[] readBuffer3 = ReadRegister(Hdc1010Serial3, 2);

            snValue[4] = readBuffer1[1];
            snValue[3] = readBuffer1[0];
            snValue[2] = readBuffer2[1];
            snValue[1] = readBuffer2[0];
            snValue[0] = readBuffer3[1];

            return BitConverter.ToUInt32(snValue, 0);
        }

        /// <summary>
        ///     Reads both Temperature and Humidity from the TempHum4 click sequentially in
        ///     <see cref="AcquisitionModes.Sequential" /> mode.
        /// </summary>
        /// <param name="temperature">The Temperature value to be passed by reference</param>
        /// <param name="humidity">The Humidity value to be passed by reference</param>
        /// <exception cref="InvalidOperationException">
        ///     Calling this method while the TempHum4 click is configured for
        ///     <see cref="AcquisitionModes.Independant" /> will throw this exception. You must obtain temperature and humidity
        ///     measurements independently using the <see cref="ReadTemperature" /> and <see cref="ReadHumidity" /> methods.
        /// </exception>
        /// <example>
        ///     Example usage:
        ///     <code language="C#">
        /// float temperature;
        /// float humidity;
        /// tempHumidity.ReadSensor(out temperature, out humidity);
        /// Debug.WriteLine("Temperature - " + _sensor.ReadTemperature().ToString("F2") + " �C");
        /// Debug.WriteLine("Humidity - " + humidity.ToString("F2") + " %RH");
        /// </code>
        /// </example>
        public void ReadSensor(out Single temperature, out Single humidity)
        {
            if (AcquisitionMode == AcquisitionModes.Independant)
                throw new InvalidOperationException(
                    "You cannot read both Temperature and Humidity measurements while the TempHum11 Click is configured for Independent Measurement.");

            WriteByte(TempRegister);

            Thread.Sleep(15);

            Byte[] readBuffer = ReadBytes(4);

            Single tempTemp = (readBuffer[0] << 8) / 65536.0F * 165.0F - 40.0F;

            temperature = ConvertToScale(tempTemp);
            humidity = (Single)((readBuffer[2] << 8) / 65536.0 * 100.0);
        }

        /// <summary>
        ///     Resets the TempHum4 Click to default values for Temperature (14-Bit), Humidity (14-Bit), Acquisition Mode
        ///     (Sequential) and disables the on-board heater.
        /// </summary>
        /// <returns>True if successful, otherwise false.</returns>
        /// <example>
        ///     Example usage:
        ///     <code language="C#">
        ///  Debug.WriteLine("Reset " + _sensor.Reset(ResetModes.Soft));
        /// </code>
        /// </example>
        public void Reset()
        {
            WriteRegister(ConfigRegister, new Byte [] { 0x80, 0x00});

            while (ReadRegister(ConfigRegister, 2)[0] != 0x80)
            {
                Thread.Sleep(1);
                if (ReadRegister(ConfigRegister, 1)[0] != 0x80) break;
            }
        }

        #endregion

        #region Private Methods

        private Byte[] ReadRegister(Byte registerAddress, Byte numberOfBytesToRead)
        {
            Byte[] readBuffer = new Byte[numberOfBytesToRead];

            lock (Hardware.LockI2C)
            {
                _sensor.WriteRead(new[] { registerAddress}, readBuffer);
            }

            return readBuffer;
        }

        private Byte[] ReadBytes(Byte numberOfBytesToRead)
        {
            Byte[] readBuffer = new Byte[numberOfBytesToRead];

            lock (Hardware.LockI2C)
            {
                _sensor.Read(readBuffer);
            }

            return readBuffer;
        }

        private void WriteRegister(Byte registerAddress, Byte[] value)
        {
            Byte[] writeBuffer = {registerAddress, value[0], value[1]};

            lock (Hardware.LockI2C)
            {
                _sensor.Write(writeBuffer);
            }
        }

        private void WriteByte(Byte value)
        {
            lock (Hardware.LockI2C)
            {
                _sensor.Write(new[] { value});
            }
        }

        private Single ConvertToScale(Single temperature)
        {
            switch (TemperatureUnit)
            {
                case TemperatureUnits.Celsius:
                    {
                        return temperature;
                    }

                case TemperatureUnits.Fahrenheit:
                    {
                        return temperature * 9 / 5 + 32;
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

        #endregion

        #region Interface Implementations

        /// <summary>
        ///     Reads the temperature from the TempHum4 Click.
        /// </summary>
        /// <param name="source">The <see cref="TemperatureSources" /> to read the Temperature from.</param>
        /// <returns>The temperature measured in the unit specified in the TemperatureUnits property. Defaults to °C.</returns>
        /// <exception cref="InvalidOperationException">
        ///     an InvalidOperationException will be thrown if attempting to read
        ///     <see cref="TemperatureSources.Object" /> as this module does not support object measurement.
        /// </exception>
        /// <example>
        ///     Example usage:
        ///     <code language="C#">
        /// Debug.WriteLine("Temperature - " + _sensor.ReadTemperature());
        /// </code>
        /// </example>
        public Single ReadTemperature(TemperatureSources source = TemperatureSources.Ambient)
        {
            if (source == TemperatureSources.Object)
                throw new InvalidOperationException(
                    "Object temperature measurement not supported by the TempHum11 Click");

            if (AcquisitionMode == AcquisitionModes.Sequential)
                throw new ApplicationException(
                    "You cannot read temperature independantly while Acquisition Mode is set for sequential access. Use the ReadSensor method instead.");

            WriteByte(TempRegister);

            while(_dataReadyPin.Read() == GpioPinValue.High) { }

            Byte[] readBuffer = ReadBytes(2);

            Single temperature = (Single)((readBuffer[0] << 8) / 65536.0 * 165.0 - 40.0);

            return ConvertToScale(temperature);
        }

        /// <summary>
        ///     Reads the humidity from the TempHum4 Click.
        /// </summary>
        /// <param name="measurementMode">The <see cref="HumidityMeasurementModes" /> to read the Humidity from.</param>
        /// <returns>The humidity measured in %RH.</returns>
        /// <exception cref="InvalidOperationException">
        ///     an InvalidOperationException will be thrown if attempting to read
        ///     <see cref="HumidityMeasurementModes.Absolute" /> as this module does not support Absolute humidity measurement.
        /// </exception>
        /// <example>
        ///     Example usage:
        ///     <code language="C#">
        ///  Debug.WriteLine("Humidity - " + _sensor.ReadHumidity());
        /// </code>
        /// </example>
        public Single ReadHumidity(HumidityMeasurementModes measurementMode = HumidityMeasurementModes.Relative)
        {
            if (measurementMode == HumidityMeasurementModes.Absolute)
                throw new InvalidOperationException(
                    "Absolute humidity measurement not supported by the TempHum11 Click");

            if (AcquisitionMode == AcquisitionModes.Sequential)
                throw new ApplicationException(
                    "You cannot read temperature independantly while Acquisition Mode is set for sequential access. Use the ReadSensor method instead.");

            WriteByte(HumidityRegister);

            while(_dataReadyPin.Read() == GpioPinValue.High) { }

            Byte[] readBuffer = ReadBytes(2);

            return (Single)((readBuffer[0] << 8) / 65536.0 * 100.0);
        }

        /// <summary>
        ///     Returns the raw data as read from the HDC1010 Click.
        ///     <para>Not supported by this driver.</para>
        /// </summary>
        /// <exception cref="NotSupportedException">
        ///     A NotSpportedException will be thrown if calling this method as this module
        ///     does not support reading raw data.
        /// </exception>
        public Int32 RawData => throw new NotSupportedException("Reading RawData is not supported by this module.");

        /// <summary>
        ///     Gets or sets the temperature unit for the <seealso cref="ReadTemperature" /> method.
        ///     <remarks>
        ///         <seealso cref="TemperatureUnits" />
        ///     </remarks>
        /// </summary>
        /// <value>
        ///     The temperature unit used.
        /// </value>
        /// <example>
        ///     <code language="C#">
        /// // Set temperature unit to Fahrenheit
        /// _sensor.TemperatureUnit = TemperatureUnits.Farhenheit;
        /// </code>
        /// </example>
        public TemperatureUnits TemperatureUnit { get; set; } = TemperatureUnits.Celsius;

        #endregion

    }
}
