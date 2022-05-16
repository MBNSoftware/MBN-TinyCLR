/*
 * HDC1000 Click Driver for TinyCLR 2.0
 * 
 * Version 1.0 :
 *  - Initial revision coded by Stephen Cardinale
 *  
 * Copyright � 2020 Stephen Cardinale and MikroBUS.Net
 * Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License. You may obtain a copy of the License at
 * http://www.apache.org/licenses/LICENSE-2.0
 * Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, 
 * either express or implied. See the License for the specific language governing permissions and limitations under the License.
 */

#if (NANOFRAMEWORK_1_0)
using System.Device.Gpio;
using System.Device.I2c;
#else
using GHIElectronics.TinyCLR.Devices.Gpio;
using GHIElectronics.TinyCLR.Devices.I2c;
#endif

using System;
using System.Threading;

namespace MBN.Modules
{
    /// <summary>
    /// A MikroBusNet Driver for the MikroE HDC1000 Click.
    /// <para><b>This module is an I2C Device</b></para>
    /// <para><b>Pins used :</b> Scl, Sda and Int</para>
    /// <para><b>References required:</b> MikroBus.Net, Microsoft.SPOT.Native, Microsoft.Spot.Hardware, mscorlib</para>
    /// </summary>
    /// <example>
    /// <code language = "C#">
    /// using MBN;
    /// using MBN.Enums;
    /// using Microsoft.SPOT;
    /// using MBN.Modules;
    /// using System.Threading;
    ///
    /// namespace Examples
    /// {
    ///     public class Program
    ///     {
    ///         private static Hdc1000Click _tempHumidity;
    ///
    ///         public static void Main()
    ///         {
    ///             _tempHumidity = new Hdc1000Click(Hardware.SocketFour, Hdc1000Click.I2CAddress.I2CAddressOne, ClockRatesI2C.Clock400KHz, 1000);
    ///             _tempHumidity.Configure(Hdc1000Click.AcquisitionMode.Sequential, Hdc1000Click.TemperatureResolution.FourteenBit, Hdc1000Click.HumidityResolution.FourteenBit, Hdc1000Click.HeaterMode.Disabled);
    ///
    ///             Debug.Print("Manufacturer ID - 0x" + _tempHumidity.GetManufacturerId().ToString("X"));
    ///             Debug.Print("Device ID - 0x" + _tempHumidity.GetDeviceId().ToString("X"));
    ///             Debug.Print("SN - " + _tempHumidity.GetSerialNumber());
    ///
    ///             while (true)
    ///             {
    ///
    ///                 /* For Sequential Acquisition Mode use the following code to obtain temperature and humidity data */
    ///                 float temperature;
    ///                 float humidity;
    ///
    ///                 _tempHumidity.ReadSensor(out temperature, out humidity);
    ///                 Debug.Print("Temperature - " + _tempHumidity.ReadTemperature().ToString("F2") + " �C");
    ///                 Debug.Print("Humidity - " + humidity.ToString("F2") + " %RH");
    ///
    ///                 /* For independent measurement mode use the following code to obtain temperature and humidity data */
    ///                 //Debug.Print("Temperature - " + _tempHumidity.ReadTemperature());
    ///                 //Debug.Print("Humidity - " + _tempHumidity.ReadHumidity());
    ///
    ///                 Thread.Sleep(2000);
    ///             }
    ///         }
    ///     }
    /// }
    /// </code>
    /// </example>
    public sealed class Hdc1000Click : ITemperature, IHumidity
    {
        #region CTOR

        /// <summary>
        /// Creates a new instance of the HDC1000 click class.
        /// </summary>
        /// <param name="socket">The socket that the HDC1000 is plugged in to.</param>
        /// <param name="ic2Address">The <see cref="I2CAddress"/> used for the I2C Bus.</param>
        public Hdc1000Click(Hardware.Socket socket, I2CAddress ic2Address = I2CAddress.I2CAddressOne)
        {
            _socket = socket;
#if (NANOFRAMEWORK_1_0)
            _sensor = I2cDevice.Create(new I2cConnectionSettings(socket.I2cBus, (int)ic2Address, I2cBusSpeed.StandardMode));
            _dataReady = new GpioController().OpenPin(socket.Int);
            _dataReady.SetPinMode(PinMode.InputPullUp);
            //_dataReady.ValueChangedEdge = PinEventTypes.Falling;
#else
            _sensor = I2cController.FromName(socket.I2cBus).GetDevice(new I2cConnectionSettings((Int32)ic2Address, 100000));
            _dataReady = GpioController.GetDefault().OpenPin(socket.Int);
            _dataReady.SetDriveMode(GpioPinDriveMode.InputPullUp);
            _dataReady.ValueChangedEdge = GpioPinEdge.FallingEdge;
#endif

            _dataReady.ValueChanged += DataReady_ValueChanged;

            if (GetManufacturerId() != 0x5449 && GetDeviceId() != 0x1000) throw new DeviceInitialisationException("HDC1000 click not found on I2C Bus");

            // Set default configuration for 14-bit Temperature, 14 Bit Humidity and HeaterMode Off.
            Configure(AcquisitionMode.Sequential, TemperatureResolution.FourteenBit, HumidityResolution.FourteenBit, HeaterMode.Disabled);
        }

        #endregion

        #region Constants

        private const Byte TempRegister = 0x00;
        private const Byte HumidityRegister = 0x01;
        private const Byte ConfigRegister = 0x02;
        private const Byte DeviceIdRegister = 0xFF;
        private const Byte ManufacturerIdRegister = 0xFE;

        private const Byte Hdc1000Serial1 = 0xFB;
        private const Byte Hdc1000Serial2 = 0xFC;
        private const Byte Hdc1000Serial3 = 0xFD;

        #endregion

        #region Fields

        private readonly I2cDevice _sensor;
        private AcquisitionMode _acquisitionMode = AcquisitionMode.Sequential;

        private readonly GpioPin _dataReady;
        private Boolean _dataAvailable;
        private readonly Hardware.Socket _socket;

        #endregion

        #region Public Enums

        /// <summary>
        /// Enumeration of possible I2C Address supported by the HDC1000 Click.
        /// </summary>
        public enum I2CAddress
        {
            /// <summary>
            /// I2C Address Jumper Positions ADR0 in position 0 and ADR1 in position 0.
            /// </summary>
            I2CAddressOne = 0x40,

            /// <summary>
            /// I2C Address Jumper Positions ADR0 in position 1 and ADR1 in position 0.
            /// </summary>
            I2CAddressTwo = 0x41,

            /// <summary>
            /// I2C Address Jumper Positions ADR0 in position 0 and ADR1 in position 1.
            /// </summary>
            I2CAddressThree = 0x42,

            /// <summary>
            /// I2C Address Jumper Positions ADR0 in position 1 and ADR1 in position 1.
            /// </summary>
            I2CAddressFour = 0x43
        }

        /// <summary>
        /// Measurement acquisition mode.
        /// </summary>
        public enum AcquisitionMode
        {
            /// <summary>
            /// Independent, measures temperature or humidity independently.
            /// </summary>
            Independant = 0x00,

            /// <summary>
            /// Sequential, measures temperature first and then humidity sequentially in one read.
            /// </summary>
            Sequential = 0x10
        }

        /// <summary>
        /// Temperature Measurement Resolution
        /// </summary>
        public enum TemperatureResolution
        {
            /// <summary>
            /// Eleven (11) Bit resolution with a conversion time of 3.65 milliseconds.
            /// </summary>
            FourteenBit = 0x00,

            /// <summary>
            /// Fourteen (14) Bit resolution with a conversion time of 6.35 milliseconds.
            /// </summary>
            ElevenBit = 0x04
        }

        /// <summary>
        /// Humidity Measurement Resolution
        /// </summary>
        public enum HumidityResolution
        {
            /// <summary>
            /// Eight (8) Bit resolution with a conversion time of 2.5 milliseconds.
            /// </summary>
            EightBit = 0x02,

            /// <summary>
            /// Eleven (11) Bit resolution with a conversion time of 3.85 milliseconds.
            /// </summary>
            ElevenBit = 0x01,

            /// <summary>
            /// Fourteen (14) Bit resolution with a conversion time of 6.5 milliseconds.
            /// </summary>
            FourteenBit = 0x00
        }

        /// <summary>
        /// The on-chip Heater functionality enumeration.
        /// </summary>
        public enum HeaterMode
        {
            /// <summary>
            /// The heater is enabled. The heater helps in reducing the accumulated offset after long exposure at high humidity conditions.
            /// Once enabled the heater is turned on only in the measurement mode. To have a reasonable increase of the temperature it is suggested to increase the measurement data rate.
            /// </summary>
            Enabled = 0x05,

            /// <summary>
            /// The HeaterMode is disabled.
            /// </summary>
            Disabled = 0x00
        }

        #endregion

        #region Private Methods

#if (NANOFRAMEWORK_1_0)
        private void DataReady_ValueChanged(object sender, PinValueChangedEventArgs e)
        {
            if (e.ChangeType == PinEventTypes.Falling)
#else
        private void DataReady_ValueChanged(GpioPin sender, GpioPinValueChangedEventArgs e)
        {
            if (e.Edge== GpioPinEdge.FallingEdge)
#endif
            {
                _dataAvailable = true;
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
                    return temperature*9/5 + 32;
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

        private Byte[] ReadRegister(Byte registerAddress, Byte[] readBuffer)
        {
            Byte[] writeBuffer = {registerAddress};

            lock (_socket.LockI2c)
            {
                _sensor.WriteRead(writeBuffer, readBuffer);
            }

            return readBuffer;
        }

        private Byte[] ReadRegister(Byte bytesToRead)
        {
            Byte[] readBuffer = new Byte[bytesToRead];

            lock (_socket.LockI2c)
            {
                _sensor.Read(readBuffer);
            }

            return readBuffer;
        }

        private void WriteRegister(Byte registerAddress, Byte value)
        {
            Byte[] writeBuffer = new Byte[3];
            writeBuffer[0] = registerAddress;
            writeBuffer[1] = value;
            writeBuffer[2] = 0x00; // Second 8 bits of the Configuration register. Always 0x00.

            lock (_socket.LockI2c)
            {
                _sensor.Write(writeBuffer);
            }
        }

        private void WriteRegister(Byte registerAddress)
        {
            lock (_socket.LockI2c)
            {
                _sensor.Write(new[] { registerAddress });
            }
        }

#endregion

#region Public Properties

        /// <summary>
        /// Returns the raw data as read from the HDC1000 Click.
        /// <para>Not supported by this driver.</para>
        /// </summary>
        /// <exception cref="NotSupportedException">A NotSpportedException will be thrown if calling this method as this module does not support reading raw data.</exception>
        public Int32 RawData => throw new NotSupportedException("Reading RawData is not supported by this module.");

        /// <summary>
        /// Gets or sets the temperature unit for the <seealso cref="ReadTemperature"/> method.
        /// <remarks><seealso cref="TemperatureUnits"/></remarks>
        /// </summary>
        /// <value>
        /// The temperature unit used.
        /// </value>
        /// <example>
        /// <code language="C#">
        /// // Set temperature unit to Fahrenheit
        /// _tempHumidity.TemperatureUnit = TemperatureUnits.Farhenheit;
        /// </code>
        /// </example>
        public TemperatureUnits TemperatureUnit { get; set; } = TemperatureUnits.Celsius;

#endregion

#region Public Methods

        /// <summary>
        /// Configures the HDC1000 Click for temperature, humidity acquisition.
        /// </summary>
        /// <param name="acquisitionMode">The <see cref="AcquisitionMode"/> for measurement.</param>
        /// <param name="temperatureResolution">The <see cref="TemperatureResolution"/> for reading temperature data.</param>
        /// <param name="humidityResolution">The <see cref="HumidityResolution"/> for reading humidity data.</param>
        /// <param name="heatMode">The <see cref="HeaterMode"/> for enabling or disabling the on-board heater.</param>
        /// <example>Example usage:
        /// <code language = "C#">
        /// _tempHumidity.Configure(Hdc1000Click.AcquisitionMode.Sequential, Hdc1000Click.TemperatureResolution.FourteenBit, Hdc1000Click.HumidityResolution.FourteenBit, Hdc1000Click.HeaterMode.Disabled);
        /// </code>
        /// </example>
        public void Configure(AcquisitionMode acquisitionMode, TemperatureResolution temperatureResolution, HumidityResolution humidityResolution, HeaterMode heatMode)
        {
            _acquisitionMode = acquisitionMode;

            Byte value = (Byte) ((Byte) acquisitionMode | (Byte) temperatureResolution | (Byte) humidityResolution | (Byte) heatMode);

            WriteRegister(ConfigRegister, value);

            Thread.Sleep(15);
        }

        /// <summary>
        /// Gets the Device ID of the HDC1000 Click.
        /// </summary>
        /// <returns>The Device ID</returns>
        /// <example>Example usage:
        /// <code language = "C#">
        /// Debug.Print("Device ID - 0x" + _tempHumidity.GetDeviceId().ToString("X"));
        /// </code>
        /// </example>
        public Int32 GetDeviceId()
        {
            Byte[] id = ReadRegister(DeviceIdRegister, new Byte[2]);
            return id[1] | (id[0] << 8);
        }

        /// <summary>
        /// Gets the Manufacturer ID of the HDC1000 Click
        /// </summary>
        /// <returns>The Manufacturer ID</returns>
        /// <example>Example usage:
        /// <code language = "C#">
        /// Debug.Print("Manufacturer ID - 0x" + _tempHumidity.GetManufacturerId().ToString("X"));
        /// </code>
        /// </example>
        public Int32 GetManufacturerId()
        {
            Byte[] id = ReadRegister(ManufacturerIdRegister, new Byte[2]);
            return id[1] | (id[0] << 8);
        }

        /// <summary>
        /// Gets the unique Serial Number of the HDC1000 Click
        /// </summary>
        /// <returns>The Serial Number</returns>
        /// <example>Example usage:
        /// <code language = "C#">
        /// Debug.Print("SN - " + _tempHumidity.GetSerialNumber());
        /// </code>
        /// </example>
        public UInt64 GetSerialNumber()
        {
            Byte[] readBuffer1 = new Byte[2];
            Byte[] readBuffer2 = new Byte[2];
            Byte[] readBuffer3 = new Byte[2];

            Byte[] snValue = new Byte[5];

            ReadRegister(Hdc1000Serial1, readBuffer1);
            ReadRegister(Hdc1000Serial2, readBuffer2);
            ReadRegister(Hdc1000Serial3, readBuffer3);

            snValue[4] = readBuffer1[1];
            snValue[3] = readBuffer1[0];
            snValue[2] = readBuffer2[1];
            snValue[1] = readBuffer2[0];
            snValue[0] = readBuffer3[1];

            return System.BitConverter.ToUInt32(snValue, 0); //TODO: requires "System" as "MBN"  overrides it!
        }

        /// <summary>
        /// Reads the humidity from the HDC1000 Click.
        /// </summary>
        /// <param name="measurementMode">The <see cref="HumidityMeasurementModes"/> to read the Humidity from.</param>
        /// <returns>The humidity measured in %RH.</returns>
        /// <exception cref="InvalidOperationException"> an InvalidOperationException will be thrown if attempting to read <see cref="HumidityMeasurementModes.Absolute"/> as this module does not support Absolute humidity measurement.</exception>
        /// <example>Example usage:
        /// <code language = "C#">
        ///  Debug.Print("Humidity - " + _tempHumidity.ReadHumidity());
        /// </code>
        /// </example>
        public Single ReadHumidity(HumidityMeasurementModes measurementMode = HumidityMeasurementModes.Relative)
        {
            if (measurementMode == HumidityMeasurementModes.Absolute) throw new InvalidOperationException("Absolute humidity measurement not supported by the HDC1000 Click");

            _dataAvailable = false;

            WriteRegister(HumidityRegister);

            while (_dataAvailable == false)  {}

            Byte[] readBuffer = ReadRegister(2);

            return (readBuffer[0] << 8) / 65536.0f * 100.0f;
        }

        /// <summary>
        /// Reads both Temperature and Humidity from the HDC1000 click sequentially in <see cref="AcquisitionMode.Sequential"/> mode.
        /// </summary>
        /// <param name="temperature">The Temperature value to be passed by reference</param>
        /// <param name="humidity">The Humidity value to be passed by reference</param>
        /// <exception cref="InvalidOperationException">Calling this method while the HDC1000 click is configured for <see cref="AcquisitionMode.Independant"/> will throw this exception. You must obtain temperature and humidity measurements independently using the <see cref="ReadTemperature"/> and <see cref="ReadHumidity"/> methods.</exception>
        /// <example>Example usage:
        /// <code language = "C#">
        /// float temperature;
        /// float humidity;
        /// tempHumidity.ReadSensor(out temperature, out humidity);
        /// Debug.Print("Temperature - " + _tempHumidity.ReadTemperature().ToString("F2") + " �C");
        /// Debug.Print("Humidity - " + humidity.ToString("F2") + " %RH");
        /// </code>
        /// </example>
        public void ReadSensor(out Single temperature, out Single humidity)
        {
            if (_acquisitionMode == AcquisitionMode.Independant) throw new InvalidOperationException("You cannot read both Temperature and Humidity measurements while the HDC1000 Click is configured for Independent Measurement.");

            _dataAvailable = false;

            WriteRegister(TempRegister);

            while (_dataAvailable == false){}

            Byte[] readBuffer = ReadRegister(4);

            Single tempTemp = (readBuffer[0] << 8) / 65536.0f * 165.0f - 40.0f;

            temperature = ConvertToScale(tempTemp);
            humidity = (readBuffer[2] << 8) / 65536.0f * 100.0f;

            _dataAvailable = false;
        }

        /// <summary>
        /// Reads the temperature from the HDC1000 Click.
        /// </summary>
        /// <param name="source">The <see cref="TemperatureSources"/> to read the Temperature from.</param>
        /// <returns>The temperature measured in �C.</returns>
        /// <exception cref="InvalidOperationException"> an InvalidOperationException will be thrown if attempting to read <see cref="TemperatureSources.Object"/> as this module does not support object measurement.</exception>
        /// <example>Example usage:
        /// <code language = "C#">
        /// Debug.Print("Temperature - " + _tempHumidity.ReadTemperature());
        /// </code>
        /// </example>
        public Single ReadTemperature(TemperatureSources source = TemperatureSources.Ambient)
        {
            if (source == TemperatureSources.Object) throw new InvalidOperationException("Object temperature measurement not supported by the HDC1000 Click");

            _dataAvailable = false;

            WriteRegister(TempRegister);

            while (_dataAvailable == false){}

            Byte[] readBuffer = ReadRegister(2);

            Single temperature = (readBuffer[0] << 8) / 65536.0f * 165.0f - 40.0f;

            return ConvertToScale(temperature);
        }

        /// <summary>
        /// Resets the HDC1000 Click to default values for Temperature (14-Bit), Humidity (14-Bit), Acquisition Mode (Sequential) and disables the on-board heater.
        /// </summary>
        /// <param name="resetMode">The <see cref="ResetModes"/> used for the reset.</param>
        /// <returns>True if successful, otherwise false.</returns>
        /// <exception cref="NotSupportedException">A NotSupportedException will be thrown if this method is called with <see cref="ResetModes.Hard"/> as this module does not support hard resets.</exception>
        /// <example>Example usage:
        /// <code language = "C#">
        ///  Debug.Print("Reset " + _tempHumidity.Reset(ResetModes.Soft));
        /// </code>
        /// </example>
        public Boolean Reset(ResetModes resetMode)
        {
            if (resetMode == ResetModes.Hard) throw new NotSupportedException("Hard resets are not supported by this module.");

            WriteRegister(ConfigRegister, 0x10);
            return ReadRegister(ConfigRegister)[0] == 0x10;
        }

#endregion
    }
}
