/*
 * TempHum11  driver for the TinyCLR 2.0
 * 
 * Initial revision coded by Stephen Cardinale
 * 
 * Copyright 2020 Stephen Cardinale and MikroBUS.Net
 * Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License. You may obtain a copy of the License at
 * http://www.apache.org/licenses/LICENSE-2.0
 * Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, 
 * either express or implied. See the License for the specific language governing permissions and limitations under the License.
 */

#region Usings

using System;
using System.Threading;
#if (NANOFRAMEWORK_1_0)
using System.Device.I2c;
#else
using GHIElectronics.TinyCLR.Devices.I2c;
#endif

#endregion

namespace MBN.Modules
{
    /// <summary>
    ///     Main class for the TempHum11 Click driver
    ///     <para><b>Pins used :</b> Scl, Sda</para>
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
    ///     internal class Program
    ///     {
    ///         private static TempHum11Click _sensor;
    ///
    ///         private static void Main()
    ///         {
    ///             _sensor = new TempHum11Click(Hardware.SocketTwo);
    ///
    ///             _sensor.Configure(TempHum11Click.AcquisitionMode.Independant, TempHum11Click.TemperatureResolution.FourteenBit, TempHum11Click.HumidityResolution.FourteenBit, TempHum11Click.HeaterMode.Disabled);
    ///
    ///             Debug.WriteLine($"Manufacturer ID 0x{_sensor.GetManufacturerId():x}");
    ///             Debug.WriteLine($"Device ID - 0x{_sensor.GetDeviceId():X}");
    ///             Debug.WriteLine($"SN - {_sensor.GetSerialNumber()}");
    ///
    ///             while (true)
    ///             {
    ///                 /* For Sequential Acquisition Mode use the following code to obtain temperature and humidity data
    ///                    You will need to change the Acquisition Mode to Independent.*/
    ///                 //_tempHumidity.ReadSensor(out Single temperature, out Single humidity);
    ///                 //Debug.WriteLine($"Temperature : {temperature:F2} °C");
    ///                 //Debug.WriteLine($"   Humidity : {humidity:F2} %RH\n");
    ///
    ///                 /* For independent measurement mode use the following code to obtain temperature and humidity data
    ///                    You will need to change the Acquisition Mode to Independent.*/
    ///                 Debug.WriteLine($"Temperature : {_sensor.ReadTemperature():F2} *C");
    ///                 Debug.WriteLine($"   Humidity : {_sensor.ReadHumidity():F2} %RH\n");
    ///
    ///                 Thread.Sleep(2000);
    ///             }
    ///         }
    ///     }
    /// }
    /// </code>
    /// </example>
    public sealed class TempHum11Click : ITemperature, IHumidity
    {
        #region .ctor

        /// <summary>
        ///     Initializes a new instance of the <see cref="TempHum11Click" /> class.
        /// </summary>
        /// <param name="socket">The socket on which the HDC1000Click module is plugged on MikroBus.Net board</param>
        public TempHum11Click(Hardware.Socket socket)
        {
            _socket = socket;
#if (NANOFRAMEWORK_1_0)
            _sensor = I2cDevice.Create(new I2cConnectionSettings(socket.I2cBus, 0x40, I2cBusSpeed.StandardMode));
#else
			_sensor = I2cController.FromName(socket.I2cBus).GetDevice(new I2cConnectionSettings(0x40, 100000));
#endif

            if (GetManufacturerId() != 0x5449 && GetDeviceId() != 0x1000)
            {
                throw new DeviceInitialisationException("TempHum11 click not found on I2C Bus");
            }

            // Set default configuration for Sequential acquisition, 14-bit Temperature, 14 Bit Humidity and HeaterMode Off.
            Configure(AcquisitionMode.Sequential,
                TemperatureResolution.FourteenBit,
                HumidityResolution.FourteenBit,
                HeaterMode.Disabled);
        }

        #endregion

        #region Private Fields

        private readonly I2cDevice _sensor;
        private readonly Hardware.Socket _socket;
        private AcquisitionMode _acquisitionMode = AcquisitionMode.Sequential;

        #endregion

        #region Constants

        private const Byte TempRegister = 0x00;
        private const Byte HumidityRegister = 0x01;
        private const Byte ConfigRegister = 0x02;
        private const Byte DeviceIdRegister = 0xFF;
        private const Byte ManufacturerIdRegister = 0xFE;

        private const Byte Hdc1080Serial1 = 0xFB;
        private const Byte Hdc1080Serial2 = 0xFC;
        private const Byte Hdc1080Serial3 = 0xFD;

        #endregion

        #region Public Enums

        /// <summary>
        ///     Measurement acquisition mode.
        /// </summary>
        public enum AcquisitionMode
        {
            /// <summary>
            ///     Independent, measures temperature or humidity independently.
            /// </summary>
            Independant = 0x00,

            /// <summary>
            ///     Sequential, measures temperature first and then humidity sequentially in one read.
            /// </summary>
            Sequential = 0x10
        }

        /// <summary>
        ///     Temperature Measurement Resolution
        /// </summary>
        public enum TemperatureResolution
        {
            /// <summary>
            ///     Eleven (11) Bit resolution with a conversion time of 3.65 milliseconds.
            /// </summary>
            FourteenBit = 0x00,

            /// <summary>
            ///     Fourteen (14) Bit resolution with a conversion time of 6.35 milliseconds.
            /// </summary>
            ElevenBit = 0x04
        }

        /// <summary>
        ///     Humidity Measurement Resolution
        /// </summary>
        public enum HumidityResolution
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
        public enum HeaterMode
        {
            /// <summary>
            ///     The heater is enabled. The heater helps in reducing the accumulated offset after long exposure at high humidity
            ///     conditions.
            ///     Once enabled the heater is turned on only in the measurement mode. To have a reasonable increase of the temperature
            ///     it is suggested to increase the measurement data rate.
            /// </summary>
            Enabled = 0x05,

            /// <summary>
            ///     The HeaterMode is disabled.
            /// </summary>
            Disabled = 0x00
        }

        #endregion

        #region Public Methods

        /// <summary>
        ///     Configures the HDC1000 Click for temperature, humidity acquisition.
        /// </summary>
        /// <param name="acquisitionMode">The <see cref="AcquisitionMode" /> for measurement.</param>
        /// <param name="temperatureResolution">The <see cref="TemperatureResolution" /> for reading temperature data.</param>
        /// <param name="humidityResolution">The <see cref="HumidityResolution" /> for reading humidity data.</param>
        /// <param name="heaterMode">The <see cref="HeaterMode" /> for enabling or disabling the on-board heater.</param>
        /// <example>
        ///     Example usage:
        ///     <code language="C#">
        /// _tempHumidity.Configure(Hdc1000Click.AcquisitionMode.Sequential, Hdc1000Click.TemperatureResolution.FourteenBit, Hdc1000Click.HumidityResolution.FourteenBit, Hdc1000Click.HeaterMode.Disabled);
        /// </code>
        /// </example>
        public void Configure(AcquisitionMode acquisitionMode, TemperatureResolution temperatureResolution, HumidityResolution humidityResolution, HeaterMode heaterMode)
        {
            _acquisitionMode = acquisitionMode;

            Byte value = (Byte) ((Byte) acquisitionMode | (Byte) temperatureResolution | (Byte) humidityResolution |
                                 (Byte) heaterMode);

            WriteRegister(ConfigRegister, value);

            Thread.Sleep(15);
        }

        /// <summary>
        ///     Gets the Device ID of the HDC1000 Click.
        /// </summary>
        /// <returns>The Device ID</returns>
        /// <example>
        ///     Example usage:
        ///     <code language="C#">
        /// Debug.WriteLine("Device ID - 0x" + _tempHumidity.GetDeviceId().ToString("X"));
        /// </code>
        /// </example>
        public Int32 GetDeviceId()
        {
            Byte[] id = ReadRegister(DeviceIdRegister, 2);
            return id[1] | (id[0] << 8);
        }

        /// <summary>
        ///     Gets the Manufacturer ID of the HDC1000 Click
        /// </summary>
        /// <returns>The Manufacturer ID</returns>
        /// <example>
        ///     Example usage:
        ///     <code language="C#">
        /// Debug.WriteLine("Manufacturer ID - 0x" + _tempHumidity.GetManufacturerId().ToString("X"));
        /// </code>
        /// </example>
        public Int32 GetManufacturerId()
        {
            Byte[] id = ReadRegister(ManufacturerIdRegister, 2);
            return id[1] | (id[0] << 8);
        }

        /// <summary>
        ///     Gets the unique Serial Number of the HDC1000 Click
        /// </summary>
        /// <returns>The Serial Number</returns>
        /// <example>
        ///     Example usage:
        ///     <code language="C#">
        /// Debug.WriteLine("SN - " + _tempHumidity.GetSerialNumber());
        /// </code>
        /// </example>
        public UInt64 GetSerialNumber()
        {
            Byte[] snValue = new Byte[5];

            Byte[] readBuffer1 = ReadRegister(Hdc1080Serial1, 2);
            Byte[] readBuffer2 = ReadRegister(Hdc1080Serial2, 2);
            Byte[] readBuffer3 = ReadRegister(Hdc1080Serial3, 2);

            snValue[4] = readBuffer1[1];
            snValue[3] = readBuffer1[0];
            snValue[2] = readBuffer2[1];
            snValue[1] = readBuffer2[0];
            snValue[0] = readBuffer3[1];

            return System.BitConverter.ToUInt32(snValue, 0);
        }

        /// <summary>
        ///     Resets the TempHum11 Click to default values for Temperature (14-Bit), Humidity (14-Bit), Acquisition Mode
        ///     (Sequential) and disables the on-board heater.
        /// </summary>
        /// <returns>True if successful, otherwise false.</returns>
        /// <example>
        ///     Example usage:
        ///     <code language="C#">
        ///  Debug.WriteLine("Reset " + _tempHumidity.Reset());
        /// </code>
        /// </example>
        public Boolean Reset()
        {
            WriteRegister(ConfigRegister, 0x10);
            return ReadRegister(ConfigRegister, 1)[0] == 0x10;
        }

        /// <summary>
        ///     Reads both Temperature and Humidity from the HDC1000 click sequentially in
        ///     <see cref="AcquisitionMode.Sequential" /> mode.
        /// </summary>
        /// <param name="temperature">The Temperature value to be passed by reference</param>
        /// <param name="humidity">The Humidity value to be passed by reference</param>
        /// <exception cref="InvalidOperationException">
        ///     Calling this method while the HDC1000 click is configured for
        ///     <see cref="AcquisitionMode.Independant" /> will throw this exception. You must obtain temperature and humidity
        ///     measurements independently using the <see cref="ReadTemperature" /> and <see cref="ReadHumidity" /> methods.
        /// </exception>
        /// <example>
        ///     Example usage:
        ///     <code language="C#">
        /// float temperature;
        /// float humidity;
        /// tempHumidity.ReadSensor(out temperature, out humidity);
        /// Debug.WriteLine("Temperature - " + _tempHumidity.ReadTemperature().ToString("F2") + " �C");
        /// Debug.WriteLine("Humidity - " + humidity.ToString("F2") + " %RH");
        /// </code>
        /// </example>
        public void ReadSensor(out Single temperature, out Single humidity)
        {
            if (_acquisitionMode == AcquisitionMode.Independant)
                throw new InvalidOperationException(
                    "You cannot read both Temperature and Humidity measurements while the TempHum11 Click is configured for Independent Measurement.");

            WriteByte(TempRegister);

            Thread.Sleep(15);

            Byte[] readBuffer = ReadBytes(4);

            Single tempTemp = (readBuffer[0] << 8) / 65536.0F * 165.0F - 40.0F;

            temperature = ConvertToScale(tempTemp);
            humidity = (Single) ((readBuffer[2] << 8) / 65536.0 * 100.0);
        }

        #endregion

        #region Private Methods

        private Byte[] ReadBytes(Byte numberOfBytesToRead)
        {
            Byte[] readBuffer = new Byte[numberOfBytesToRead];

            lock (_socket.LockI2c)
            {
                _sensor.Read(readBuffer);
            }

            return readBuffer;
        }

        private Byte[] ReadRegister(Byte registerAddress, Byte numberOfBytesToRead)
        {
            Byte[] readBuffer = new Byte[numberOfBytesToRead];

            lock (_socket.LockI2c)
            {
                _sensor.WriteRead(new[] { registerAddress}, readBuffer);
            }

            return readBuffer;
        }

        private void WriteByte(Byte value)
        {
            lock (_socket.LockI2c)
            {
                _sensor.Write(new[] { value});
            }
        }

        private void WriteRegister(Byte registerAddress, Byte value)
        {
            Byte[] writeBuffer = {registerAddress, value};

            lock (_socket.LockI2c)
            {
                _sensor.Write(writeBuffer);
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
        ///     Reads the temperature from the TempHum11 Click.
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
        /// Debug.WriteLine("Temperature - " + _tempHumidity.ReadTemperature());
        /// </code>
        /// </example>
        public Single ReadTemperature(TemperatureSources source = TemperatureSources.Ambient)
        {
            if (source == TemperatureSources.Object)
                throw new InvalidOperationException(
                    "Object temperature measurement not supported by the TempHum11 Click");

            if (_acquisitionMode == AcquisitionMode.Sequential)
                throw new ApplicationException(
                    "You cannot read temperature independantly while Acquisition Mode is set for sequential access.");

            WriteByte(TempRegister);

            Thread.Sleep(15);

            Byte[] readBuffer = ReadBytes(2);

            Single temperature = (Single) ((readBuffer[0] << 8) / 65536.0 * 165.0 - 40.0);

            return ConvertToScale(temperature);
        }

        /// <summary>
        ///     Reads the humidity from the TempHum11 Click.
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
        ///  Debug.WriteLine("Humidity - " + _tempHumidity.ReadHumidity());
        /// </code>
        /// </example>
        public Single ReadHumidity(HumidityMeasurementModes measurementMode = HumidityMeasurementModes.Relative)
        {
            if (measurementMode == HumidityMeasurementModes.Absolute)
                throw new InvalidOperationException(
                    "Absolute humidity measurement not supported by the TempHum11 Click");

            if (_acquisitionMode == AcquisitionMode.Sequential)
                throw new ApplicationException(
                    "You cannot read temperature independantly while Acquisition Mode is set for sequential access.");

            WriteByte(HumidityRegister);

            Thread.Sleep(15);

            Byte[] readBuffer = ReadBytes(2);

            return (Single) ((readBuffer[0] << 8) / 65536.0 * 100.0);
        }

        /// <summary>
        ///     Returns the raw data as read from the HDC1000 Click.
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
        /// _tempHumidity.TemperatureUnit = TemperatureUnits.Farhenheit;
        /// </code>
        /// </example>
        public TemperatureUnits TemperatureUnit { get; set; } = TemperatureUnits.Celsius;

        #endregion
    }
}