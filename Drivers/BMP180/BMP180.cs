/*
 * BMP180 Module Driver for TinyCLR 2.0
 * 
 * - Based on the Bosch BMP180 Absolute Pressure Sensor.
 * - This driver will also work for the Bosch BMP085 Absolute Pressure Sensor.
 * 
 * Version 1.0
 *  - Initial version
 *  
 * Copyright 2020 MikroBus.Net
 * Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License. You may obtain a copy of the License at
 * http://www.apache.org/licenses/LICENSE-2.0
 * Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, 
 * either express or implied. See the License for the specific language governing permissions and limitations under the License.
 */

#if (NANOFRAMEWORK_1_0)
using System.Device.I2c;
#else
using GHIElectronics.TinyCLR.Devices.I2c;
#endif

using System;
using System.Threading;

// ReSharper disable once CheckNamespace
namespace MBN.Modules
{
    /// <summary>
    /// A MikroBusNet Driver for a Bosch BMP180 module. This driver will also work for the Bosch BMP085 Sensor.
    /// <para><b>This module is an I2C Device</b></para>
    /// <para><b>Pins used :</b> Scl, Sda</para>
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
    ///         private static BMP180 _bmp180;
    ///
    ///         private static void Main()
    ///         {
    ///             _bmp180 = new BMP180(Hardware.SocketOne)
    ///             {
    ///                 OverSamplingSetting = BMP180.Oss.UltraHighResolution,
    ///                 TemperatureUnit = TemperatureUnits.Celsius
    ///             };
    ///
    ///             Debug.WriteLine("BMP180 Demo");
    ///             Debug.WriteLine("Is a BMP180 connected? " + _bmp180.IsConnected());
    ///             Debug.WriteLine("BMP180 Sensor OSS is - " + _bmp180.OverSamplingSetting + "\n");
    ///
    ///             while (true)
    ///             {
    ///                 Debug.WriteLine($"Temperature : {_bmp180.ReadTemperature():F2} °C");
    ///                 Debug.WriteLine($"   Pressure : {_bmp180.ReadPressure():F1} Pascals");
    ///                 Debug.WriteLine($"   Altitude : {_bmp180.ReadAltitude():F0} meters\n");
    ///                 Thread.Sleep(2000);
    ///             }
    ///         }
    ///     }
    /// }
    /// </code>
    /// </example>
    public sealed class BMP180 : ITemperature, IPressure
    {
        #region CTOR

        /// <summary>
        ///     Default Constructor
        /// </summary>
        /// <param name="socket">The socket that the BMP180 Module is connected into.</param>
        public BMP180(Hardware.Socket socket, Int32 address = 0x77)
        {
            _socket = socket;
            // Create the driver's I²C configuration
#if (NANOFRAMEWORK_1_0)
            _bmp180 = I2cDevice.Create(new I2cConnectionSettings(socket.I2cBus, address, I2cBusSpeed.StandardMode));
#else
            _bmp180 = I2cController.FromName(socket.I2cBus).GetDevice(new I2cConnectionSettings(address, 100000));
#endif

            Reset(ResetModes.Soft);

            // Is the BMP180 there?
            if (!IsConnected()) throw new DeviceInitialisationException("BMP180 not found.");

            // Get Calibration Data
            if (!GetCalibrationData()) throw new DeviceInitialisationException("BMP180 GetCalibrationData failed.");
        }

        #endregion

        #region Fields

        #region Calibration Data Fields

        private Int32 _ac1;
        private Int32 _ac2;
        private Int32 _ac3;
        private Int32 _ac4;
        private Int32 _ac5;
        private Int32 _ac6;
        private Int32 _b1;
        private Int32 _b2;
        private Int32 _mb;
        private Int32 _mc;
        private Int32 _md;

        #endregion
        private readonly Hardware.Socket _socket;
        private I2cDevice _bmp180; // I²C configuration
        private Oss _overSamplingSetting = Oss.Standard; // 0 = low precision & power to 3 = higher both

        #endregion

        #region ENUMS

        /// <summary>
        ///     Enumeration of the Over Sampling Setting (OSS) of the BMP180 Module.
        /// </summary>
        public enum Oss : Byte
        {
            /// <summary>
            ///     Least accurate and least power consumption.
            /// </summary>
            UltraLowPower = 0,

            /// <summary>
            ///     Standard mode
            /// </summary>
            Standard = 1,

            /// <summary>
            ///     High Resolution
            /// </summary>
            HighResolution = 2,

            /// <summary>
            ///     Most accurate and most power consumption.
            /// </summary>
            UltraHighResolution = 3
        }

        #endregion

        #region Private Methods

        private Single CalculatePressureAsl(Single pressure)
        {
            Single seaLevelCompensation = (Single)(101325 * Math.Pow((288 - 0.0065 * 143) / 288, 5.256));
            return 101325 + pressure - seaLevelCompensation;
        }

        private Single ScaleTemperature(Single temperatureData)
        {
            switch (TemperatureUnit)
            {
                case TemperatureUnits.Celsius:
                {
                    break;
                }
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
                default:
                {
                    throw new ArgumentOutOfRangeException();
                }
            }

            return temperatureData;
        }

        private Boolean GetCalibrationData()
        {
            Byte[] rawData = ReadRegister(0xAA, 22);

            _ac1 = (Int16)(rawData[0] << 8) + rawData[1];
            _ac2 = (Int16)(rawData[2] << 8) + rawData[3];
            _ac3 = (Int16)(rawData[4] << 8) + rawData[5];
            _ac4 = (UInt16)(rawData[6] << 8) + rawData[7];
            _ac5 = (UInt16)(rawData[8] << 8) + rawData[9];
            _ac6 = (UInt16)(rawData[10] << 8) + rawData[11];
            _b1 = (Int16)(rawData[12] << 8) + rawData[13];
            _b2 = (Int16)(rawData[14] << 8) + rawData[15];
            _mb = (Int16)(rawData[16] << 8) + rawData[17];
            _mc = (Int16)(rawData[18] << 8) + rawData[19];
            _md = (Int16)(rawData[20] << 8) + rawData[21];

            // If any of the 11 CalibrationData Words are either 0x00 or 0xFFFF the calibration data read has failed.
            return (_ac1 != 0 || _ac1 != 0xFFFF) && (_ac2 != 0 || _ac2 != 0xFFFF) && (_ac3 != 0) | (_ac3 != 0xFFFF) && (_ac4 != 0) | (_ac4 != 0xFFFF) && (_ac5 != 0 || _ac5 != 0xFFFF) && (_ac6 != 0 || _ac6 != 0xFFFF) && (_b1 != 0) | (_b1 != 0xFFFF) && (_b2 != 0 || _b2 != 0xFFFF) && (_mb != 0 || _mb != 0xFFFF) && (_mc != 0) | (_mc != 0xFFFF) && (_md != 0 || _md != 0xFFFF);
        }

        private Byte[] ReadRegister(Byte register, Byte bytesToRead)
        {
            Byte[] result = new Byte[bytesToRead];

            lock (_socket.LockI2c)
            {
                _bmp180.WriteRead(new[] { register }, result);
            }

            return result;
        }

        private void WriteByte(Byte register, Byte data)
        {
            lock (_socket.LockI2c)
            {
                _bmp180.Write(new[] { register, data });
            }
        }

        #endregion

        #region Public Properties

        /// <summary>
        ///     Sets or Gets the Over Sampling Setting (OSS) of the BMP180 Module see <see cref="Oss" />.
        /// </summary>
        /// <example>Example usage to set the Over Sampling Setting:
        /// <code language="C#">
        /// _bmp180.OverSamplingSetting = OverSamplingSetting.UltraHighResolution;
        /// </code>
        /// <code language="VB">
        /// _bmp180.OverSamplingSetting = OverSamplingSetting.UltraHighResolution
        /// </code>
        /// </example>
        /// <example>Example usage to get the Over Sampling Setting:
        /// <code language="C#">
        /// Debug.Print("BMP180 Module OSS is - " + _bmp180.OverSamplingSetting.ToString());
        /// </code>
        /// </example>
        public Oss OverSamplingSetting
        {
            get => _overSamplingSetting;
            set => _overSamplingSetting = value;
        }

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
        /// _bmp180.TemperatureUnit = TemperatureUnits.Farhenheit;
        /// </code>
        /// <code language="VB">
        /// ' Set temperature unit to Fahrenheit
        /// _bmp180.TemperatureUnit = TemperatureUnits.Farhenheit
        /// </code>
        /// </example>
        public TemperatureUnits TemperatureUnit { get; set; } = TemperatureUnits.Celsius;

        #endregion

        #region Public Methods

        /// <summary>
        /// This method determines if a BMP180 module is connected.
        /// </summary>
        /// <returns>True if a BMP180 module is connected to the I2C Bus or otherwise false.</returns>
        /// <example>
        /// <code language = "C#">
        /// Debug.Print("Is a BMP180 connected? " + _bmp180.IsConnected());
        /// </code>
        /// </example>
        public Boolean IsConnected() => ReadRegister(0xD0, 1)[0] == 0x55;

        /// <summary>
        /// The altitude as read from the BMP180 module.
        /// </summary>
        /// <returns>The altitude in meters.</returns>
        /// <remarks>The altitude reading is a calculated value based on well established mathematical formulas.</remarks>
        /// <example>Example usage to read the altitude:
        /// <code language = "C#">
        /// Debug.Print("Altitude - " + _bmp180.ReadAltitude());
        /// </code>
        /// </example>
        public Int32 ReadAltitude() => (Int32)Math.Round(44330 * (1.0 - Math.Pow(ReadPressure(PressureCompensationModes.Uncompensated) / ReadPressure(), 0.1903)));

        /// <summary>
        /// Return the pressure as read by the BMP180 module.
        /// </summary>
        /// <param name="compensationMode">The <see cref="PressureCompensationModes"/> of the BMP180 module.</param>
        /// <returns>The pressure as read from the BMP180 module, either uncompensated or Sea Level Compensated in Pascals (Pa)</returns>
        /// <remarks>One (1) Pascal (Pa) is equivalent to one (100) millibar (mBar).</remarks>
        /// <remarks>Standard meteorological reporting of Atmospheric pressure is Sea Level compensated.</remarks>
        /// <example>Example usage to read the Pressure:
        /// <code language = "C#">
        /// Debug.Print("Sea Level Compensated Pressure - " + _bmp180.ReadPressure() + " meters.");
        /// </code>
        /// </example>
        public Single ReadPressure(PressureCompensationModes compensationMode = PressureCompensationModes.SeaLevelCompensated)
        {
            Int32 x = (((this as ITemperature).RawData - _ac6)*_ac5) >> 15;
            Int32 y = (_mc << 11)/(x + _md);
            Int32 b5 = x + y;
            Int32 b6 = b5 - 4000;
            Int32 x1 = (_b2*((b6*b6) >> 12)) >> 11;
            Int32 x2 = (_ac2*b6) >> 11;
            Int32 x3 = x1 + x2;
            Int32 b3 = (((_ac1*4 + x3) << (Byte) _overSamplingSetting) + 2) >> 2;
            x1 = (_ac3*b6) >> 13;
            x2 = (_b1*((b6*b6) >> 12)) >> 16;
            x3 = (x1 + x2 + 2) >> 2;
            UInt32 b4 = (UInt32) ((_ac4*(x3 + 32768)) >> 15);
            UInt32 b7 = (UInt32) (((this as IPressure).RawData - b3)*(50000 >> (Byte) _overSamplingSetting));
            Int32 p = (Int32) (b7 < 0x80000000 ? b7*2/b4 : b7/b4*2);
            x1 = (p >> 8)*(p >> 8);
            x1 = (x1*3038) >> 16;
            x2 = (-7357*p) >> 16;
            Int32 pressure = p + ((x1 + x2 + 3791) >> 4);

            return compensationMode == PressureCompensationModes.Uncompensated ? pressure : CalculatePressureAsl(pressure);
        }

        /// <summary>
        /// Return the temperature as read by the BMP180 module.
        /// </summary>
        /// <param name="source">The <see cref="TemperatureSources.Ambient"/> to measure.</param>
        /// <returns>The temperature in the unit specified by the <see cref="TemperatureUnits"/> property.</returns>
        /// <exception cref="NotImplementedException">A <see cref="NotImplementedException"/> will be thrown the source parameter is <see cref="TemperatureSources.Object"/>. The BMP180 does not support Object temperature measurement.</exception>
        /// <example>Example usage to read the Temperature:
        /// <code language = "C#">
        /// Debug.Print("Temperature - " + _bmp180.ReadTemperature() + " °C");
        /// </code>
        /// </example>
        public Single ReadTemperature(TemperatureSources source = TemperatureSources.Ambient)
        {
            if (source == TemperatureSources.Object) throw new NotImplementedException("The BMP180 module does not provide Object temperature measurement. Use TemperatureSources.Ambient for Temperature measurement.");

            Int32 ut = (this as ITemperature).RawData;

            Int32 x1 = ((ut - _ac6) * _ac5) >> 15;
            Int32 x2 = (_mc << 11) / (x1 + _md);
            Int32 b5 = x1 + x2;

            Single temperature = (Single)((b5 + 8) >> 4) / 10;
            return ScaleTemperature(temperature);
        }

        /// <summary>
        /// The raw pressure data as read from the BMP180 module.
        /// </summary>
        /// <example>Example usage:
        /// <code language="C#">
        /// Debug.Print("ReadRaw (Pressure) - " + (_bmp180Sensor as IPressure).ReadRaw);
        /// </code>
        /// </example>
        /// <remarks>Not particularly useful but provided if one wants to calculate temperature by their own implementation.</remarks>
        Int32 IPressure.RawData
        {
            get
            {
                //while (_resetBusyPin.Read() == GpioPinValue.High) Thread.Sleep(10);

                WriteByte(0xF4, (Byte)(((Byte)_overSamplingSetting << 6) + 0x34));

                //Per data sheet wait 4.5 mSec for OSS=0, 7.5 mSec for OSS=1, 13.5 mSec for OSS=2 and 25.5 mSec for OSS=3
                Thread.Sleep(_overSamplingSetting == 0 ? 5 : (Byte) _overSamplingSetting == 1 ? 8 : (Byte) _overSamplingSetting == 2 ? 14 : 26);

                Byte[] upData = ReadRegister(0xF6, 3);

                return ((upData[0] << 16) + (upData[1] << 8) + upData[2]) >> (8 - (Byte) _overSamplingSetting);
            }
        }

        /// <summary>
        /// The raw temperature data as read from the BMP180 module.
        /// </summary>
        /// <example>Example usage:
        /// <code language="C#">
        /// Debug.Print("ReadRaw (Temperature) - " + (_bmp180Sensor as ITemperature).ReadRaw);
        /// </code>
        /// </example>
        /// <remarks>Not particularly useful but provided if one wants to calculate temperature by their own implementation.</remarks>
        Int32 ITemperature.RawData
        {
            get
            {
                //while (_resetBusyPin.Read() == GpioPinValue.High) Thread.Sleep(10);

                // Start the uncompensated temperature conversion (UT)
                WriteByte(0xF4, 0x2E);

                Thread.Sleep(5); // Wait 5 mSec to read the uncompensated temperature, minimum is 4.5 mSec per dataSheet.

                // Read Uncompensated Temperature
                Byte[] data = ReadRegister(0xF6, 2);

                return (data[0] << 8) + data[1];
            }
        }

        /// <summary>
        /// Resets the BMP180 module.
        /// </summary>
        /// <param name="resetMode">The reset mode, see <see cref="ResetModes"/> for more information.</param>
        /// <returns>True if the reset was successful  or otherwise false.</returns>
        /// <remarks>This module only supports <see cref="ResetModes.Soft"/> and will perform the same sequence as power on reset.</remarks>
        /// <exception cref="NotImplementedException">A <see cref="NotImplementedException"/> will be thrown is a <see cref="ResetModes.Hard"/> reset is attempted.</exception>
        /// <example>Example code to perform a Soft reset.
        /// <code language = "C#">
        /// _bmp180Sensor.Reset(ResetModes.Soft);
        /// </code>
        /// </example>
        public Boolean Reset(ResetModes resetMode)
        {
            if (resetMode == ResetModes.Hard) throw new NotImplementedException("Hard Reset is not available for this module. Only Soft Reset is implemented.");

            //_resetBusyPin.Write(GpioPinValue.High);

            //_isResetting = true; // We do not want to poll the sensor when it is resetting. Bad things could happen.

            WriteByte(0xE0, 0xB6);

            while (!IsConnected())
            {
                Thread.Sleep(5);
            }

            Boolean returnValue = GetCalibrationData();

            //_isResetting = false;

            //_resetBusyPin.Write(GpioPinValue.Low);

            return returnValue;
        }

        #endregion
    }
}