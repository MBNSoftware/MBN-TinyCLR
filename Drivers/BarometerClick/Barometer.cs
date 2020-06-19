/*
 * BarometerClick driver for TinyCLR 2.0
 * 
 * Initial version by Stephen Cardinale
 *  - Not implemented - Alert generation on over/under pressure.
 *                    - Setting Alert Threshold for Alert generation.
 *                    - Pressure Offset for One-point calibration i.e. - After soldering or re-flow (Registers RPDS_L (39h), RPDS_H (3Ah)).
 *                    - Pressure calculation based on Pressure Offset (Registers REF_P_XL (08h), REF_P_L (09h) and REF_P_H (0Ah).
 *                    - BDU (Block Data Update) (Temperature and Pressure out registers are not updated until both registers are read).
 *                    - Bypass mode, FIFO mode, Stream mode, Stream-to-FIFO mode, Bypass to-Stream mode and Bypass-to-FIFO mode.
 * 
 *  - Implemented - Reads Pressure (Uncompensated or Seal Level Compensated), Temperature and Altitude.
 *                - Common user configurations through the SetRecommendedMode method. 
 *                - FIFO Mean Mode only.
 *                - Pressure calculation based on Pressure Offset (Registers REF_P_XL (08h), REF_P_L (09h) and REF_P_H (0Ah).
 *                - Auto-Zero, can act as a Differential Pressure Sensor.
 *                - Everything else.    
 *  
 * References needed :
 *  Microsoft.SPOT.Hardware
 *  Microsoft.SPOT.Native
 *  MikroBusNet
 *  mscorlib
 *  
 * Copyright © 2020 Stephen Cardinale and Mikrobus.Net
 * Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License. You may obtain a copy of the License at
 * http://www.apache.org/licenses/LICENSE-2.0
 * Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, 
 * either express or implied. See the License for the specific language governing permissions and limitations under the License.
 * 
 */

using GHIElectronics.TinyCLR.Devices.I2c;

using System;
using System.Threading;

using Math = System.Math;

namespace MBN.Modules
{
    /// <summary>
    ///  Main class for the BarometerClick driver
    ///  <para><b>Pins used :</b> Scl, Sda, Int</para>
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
    ///
    ///         private static BarometerClick _sensor;
    ///
    ///         private static void Main()
    ///         {
    ///             _sensor = new BarometerClick(Hardware.SocketTwo, BarometerClick.I2CAddress.Address1);
    ///             _sensor.SetRecommendedMode(BarometerClick.RecommendedModes.WeatherMonitoring);
    ///
    ///             Debug.WriteLine("-----Barometer Click Demo-----");
    ///
    ///             while (true)
    ///             {
    ///                 Debug.WriteLine($"Temperature-----: {_sensor.ReadTemperature():F1} *C");
    ///                 Debug.WriteLine($"Pressure--------: {_sensor.ReadPressure(PressureCompensationModes.SeaLevelCompensated):F1} mBar");
    ///                 Debug.WriteLine($"Altitude--------: {_sensor.ReadAltitude():F0} meters\n");
    ///
    ///                 Thread.Sleep(1000);
    ///             }
    ///         }
    ///     }
    /// }
    /// </code>
    /// </example>
    public sealed class BarometerClick : ITemperature, IPressure
    {
        #region CTOR

        /// <summary>
        /// Initializes a new instance of the <see cref="BarometerClick" /> class.
        /// </summary>
        /// <param name="socket">The socket on which the BarometerClick module is plugged on MikroBus.Net board</param>
        /// <param name="slaveAddress">The slave address of the module.</param>
        public BarometerClick(Hardware.Socket socket, I2CAddress slaveAddress)
        {
            _sensor = I2cController.FromName(socket.I2cBus).GetDevice(new I2cConnectionSettings((Int32) slaveAddress, 400000));

            if (!Reset()) throw new ApplicationException("Unable to reset Barometer to Factory defaults");

            if (WhoAmI() != LPS25HB_DEVICE) throw new DeviceInitialisationException("Barometer Click not found on the I2C Bus.");

            SetRecommendedMode(RecommendedModes.WeatherMonitoringLowPower); // Set to lowest level of operating functionality.
        }

        #endregion

        #region Constants

        private const Byte REF_P_XL = 0x08;
        private const Byte REF_P_L = 0x09;
        private const Byte REF_P_H = 0x0A;
        private const Byte WHO_AM_I = 0x0F;
        private const Byte RES_CONF = 0x10;
        private const Byte CTRL_REG1 = 0x20;
        private const Byte CTRL_REG2 = 0x21;
        private const Byte STATUS_REG = 0x27;
        private const Byte PRESS_OUT_XL = 0x28;
        private const Byte PRESS_OUT_L = 0x29;
        private const Byte PRESS_OUT_H = 0x2A;
        private const Byte TEMP_OUT_L = 0x2B;
        private const Byte TEMP_OUT_H = 0x2C;
        private const Byte FIFO_CTRL = 0x2E;
        private const Byte LPS25HB_DEVICE = 0xBD;

        #endregion

        #region Public ENUMS

        /// <summary>I2C Bus Address selection enumeration</summary>
        public enum I2CAddress
        {
            /// <summary>Address 0 with the I2C Address Selection Jumper in position 0.</summary>
            Address0 = 0x5C,

            /// <summary>Address 1 with the I2C Address Selection Jumper in position 1 (factory default).</summary>
            Address1 = 0x5D
        }

        /// <summary>
        /// Various Manufacturer recommend modes of operation for the Barometer Click. See Application Note AN4672
        /// </summary>
        public enum RecommendedModes
        {
            /// <summary>
            /// One Shot - Lowest Power consumption
            /// <para>
            /// Operating Mode - Shutdown
            /// FIFO Coefficient Filter - Off
            /// Pressure Oversampling - 8
            /// Temperature Oversampling - 8
            /// ODR - Not Applicable.
            /// </para>
            /// </summary>
            OneShot = -1,

            /// <summary>
            /// Weather Monitoring Low Power and Low Resolution
            /// <para>
            /// Operating Mode - Continuous
            /// FIFO Coefficient Filter - Off
            /// Pressure Oversampling - 8
            /// Temperature Oversampling - 8
            /// ODR - 1 Hz
            /// RMS Noise - 0.15.
            /// </para>
            /// </summary>
            WeatherMonitoringLowPower = 0,

            /// <summary>
            /// Weather Monitoring Low resolution
            /// <para>
            /// Operating Mode - Continuous
            /// FIFO Coefficient Filter - Off
            /// Pressure Oversampling - 8
            /// Temperature Oversampling - 8
            /// ODR - 12.5 Hz
            /// RMS Noise - 0.15.
            /// </para>
            /// </summary>
            WeatherMonitoring = 1,

            /// <summary>
            /// Fall Detection / Standard Resolution
            /// <para>
            /// Operating Mode - Continuous
            /// FIFO Coefficient Filter - 2
            /// Pressure Oversampling - 512
            /// Temperature Oversampling - 64
            /// ODR - 25 Hz
            /// RMS Noise - 0.027.
            /// </para>
            /// </summary>
            FallDetection = 2,

            /// <summary>
            /// Elevator / Floor Change Detection / Standard resolution.
            /// <para>
            /// Operating Mode - Continuous
            /// FIFO Coefficient Filter - 4
            /// Pressure Oversampling - 512
            /// Temperature Oversampling - 64
            /// ODR - 25 Hz
            /// RMS Noise - 0.020.
            /// </para>
            /// </summary>
            ElevatorFloorDetectionChange = 3,

            /// <summary>
            /// Portable Device Low Power / High Resolution
            /// <para>
            /// Operating Mode - Continuous
            /// FIFO Coefficient Filter - 8
            /// Pressure Oversampling - 512
            /// Temperature Oversampling - 64
            /// ODR - 25 Hz
            /// and RMS Noise - 0.015.
            /// </para>
            /// </summary>
            PortableDeviceLowPower = 4,

            /// <summary>
            /// Portable Device Dynamic / Very High Resolution
            /// <para>
            /// Operating Mode - Continuous
            /// FIFO Coefficient Filter - 16
            /// Pressure Oversampling - 512
            /// Temperature Oversampling - 64
            /// ODR - 25 Hz
            /// RMS Noise - 0.011.
            /// </para>
            /// </summary>
            PortableDeviceDynamic = 5,

            /// <summary>
            /// Indoor Navigation / Ultra High resolution.
            /// <para>
            /// Operating Mode - Continuous
            /// FIFO Coefficient Filter - 32
            /// Pressure Oversampling - 512
            /// Temperature Oversampling - 64
            /// ODR - 25 Hz
            /// RMS Noise - 0.011.
            /// </para>
            /// </summary>
            IndoorNavigation = 6
        }

        #endregion

        #region Fields

        private readonly I2cDevice _sensor;
        private Byte _registerData;

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets or sets the auto zeroing capability of the Barometer Click.
        /// <p>This will copy the current pressure reading into the Reference Pressure registers causing the Barometer click to act a a differential pressure gauge.</p>
        /// </summary>
        /// <remarks>To use an offset other than the current pressure, you first have to enable the AutoZero property, then set the new offset with the
        /// <see cref="PressureOffset"/> method. To disable Auto-Zero, you simply have to change the offset to Zero (0) using the
        /// <see cref="PressureOffset"/> method. Or simply use the <see cref="SetDifferentialPressure(Single, Boolean)"/> method which wraps this up in one call.
        /// </remarks>
        /// <example>Example usage:
        /// <code language = "C#">
        /// _barometer.AutoZero = true;
        /// </code>
        /// </example>
        public Boolean AutoZero
        {
            get => Bits.IsBitSet(ReadRegister(CTRL_REG2)[0], 1);
            set
            {
                _registerData = ReadRegister(CTRL_REG1)[0];
                _registerData = Bits.Set(CTRL_REG1, 7, false);
                WriteRegister(CTRL_REG1, _registerData);

                _registerData = ReadRegister(CTRL_REG2)[0];
                _registerData = Bits.Set(_registerData, 1, value);
                WriteRegister(CTRL_REG2, _registerData);

                _registerData = ReadRegister(CTRL_REG1)[0];
                _registerData = Bits.Set(CTRL_REG1, 7, true);
                WriteRegister(CTRL_REG1, _registerData);

                Thread.Sleep(40);
            }
        }

        /// <summary>
        /// Gets or Sets the offset used to calculate differential pressure reading.
        /// </summary>
        /// <remarks>This property is to be used in conjunction with the <see cref="AutoZero"/> property.</remarks>
        /// <example>
        /// <code language = "C#">
        /// _barometer.AutoZero = true;
        /// _barometer.PressureOffset = 300;
        /// </code>
        /// </example>
        public Single PressureOffset
        {
            get
            {
                Byte ph = ReadRegister(REF_P_H)[0];
                Byte pl = ReadRegister(REF_P_L)[0];
                Byte pxl = ReadRegister(REF_P_XL)[0];
                Int32 rawPressureOffset = (ph << 16) | (pl << 8) | pxl;
                return ConvertTwosComplimentBytesToPressure(rawPressureOffset);
            }
            set
            {
                if (value < 0) throw new ApplicationException("Pressure Offset cannot be less than Zero (0) mBar.");
                value *=  4096F;
                WriteRegister(REF_P_XL, (Byte)((Int32)value & 0xFF));
                WriteRegister(REF_P_L, (Byte)(((Int32)value >> 8) & 0xFF));
                WriteRegister(REF_P_H, (Byte)((Int32)value >> 16));
            }
        }

        /// <summary>
        ///     Gets or sets the power mode.
        /// </summary>
        /// <example>
        ///     This sample shows how to use the PowerMode property.
        ///     <code language="C#">
        /// _barometer.PowerMode = PowerModes.Off;
        /// </code>
        /// </example>
        /// <value>
        ///     The current power mode of the module.
        /// </value>
        /// <exception cref="System.NotImplementedException"> A NotSupportedException will be thrown is attempting to set the PowerMode to <see cref="PowerModes.Low"/> as this module does not support this mode.</exception>
        public PowerModes PowerMode
        {
            get => Bits.IsBitSet(ReadRegister(CTRL_REG1)[0], 7) ? PowerModes.On : PowerModes.Off;
            set
            {
                if (value == PowerModes.Low)
                    throw new NotSupportedException(
                        "This module does not support PowerModes.Low, use PowerModes.On or PowerModes.Off instead.");

                _registerData = ReadRegister(CTRL_REG1)[0];
                _registerData = Bits.Set(_registerData, 7, value == PowerModes.On);
                WriteRegister(CTRL_REG1, _registerData);
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Gets the Manufacturer Chip ID of the LPS25HB IC.
        /// </summary>
        /// <returns>The chip id.</returns>
        /// <example>Example usage.
        /// <code language = "C#">
        /// Debug.Print("I am 0x" + _barometer.WhoAmI().ToString("x"));
        /// </code>
        /// </example>
        public Byte WhoAmI()
        {
            _registerData = ReadRegister(WHO_AM_I)[0];
            return _registerData;
        }

        /// <summary>
        /// Sets the configuration of the Barometer Click based on manufacturer recommended configurations..
        /// </summary>
        /// <param name="mode">The <see cref="RecommendedModes"/> to set the configuration to.</param>
        /// <returns>System.Void</returns>
        /// <example>
        /// <code language = "C#">
        /// _barometer.SetRecommendedMode(BarometerClick.RecommendedModes.WeatherMonitoringLowPower);
        /// </code>
        /// </example>
        public void SetRecommendedMode(RecommendedModes mode)
        {
            switch (mode)
            {
                case RecommendedModes.OneShot:
                {
                    WriteRegister(RES_CONF, 0x00); // Temp and Pressure resolution set to 8 internal averaging samples.
                    WriteRegister(CTRL_REG1, 0x80); // Set ODR to One-Shot and Power Control On.
                    WriteRegister(CTRL_REG2, 0x00); // Turn off FIFO Mode and FIFO Mean Mode
                    break;
                }
                case RecommendedModes.WeatherMonitoringLowPower:
                {
                    WriteRegister(RES_CONF, 0x00); // Temp and Pressure resolution set to 8 internal averaging samples.
                    WriteRegister(CTRL_REG1, 0x90); // Set ODR to 1 Hz Power Control On.
                    WriteRegister(CTRL_REG2, 0x00); // Turn off FIFO Mode and FIFO Mean Mode
                    break;
                }
                case RecommendedModes.WeatherMonitoring:
                {
                    WriteRegister(RES_CONF, 0x00); // Temp and Pressure resolution set to 8 internal averaging samples.
                    WriteRegister(CTRL_REG1, 0xB0); // Set ODR to 12.5 Hz Power Control On.
                    WriteRegister(CTRL_REG2, 0x00); // Turn off FFFO Mode and FIFO Mean Mode
                    break;
                }
                case RecommendedModes.FallDetection:
                {
                    WriteRegister(RES_CONF, 0x0F); // Set Temperature resolution to 64 internal averaging and Pressure resolution to 512 internal averaging..
                    WriteRegister(CTRL_REG1, 0xC0); // Set ODR to 25 Hz
                    WriteRegister(FIFO_CTRL, 0xC1); // Turn on FIFO Mean mode and set FIFO_MEAN_MODE sample size  to 2
                    _registerData = ReadRegister(CTRL_REG2)[0];
                    _registerData |= 0x50;
                    WriteRegister(CTRL_REG2, _registerData); // Turn on FIFO Mode and FIFO Mean Mode
                    break;
                }
                case RecommendedModes.ElevatorFloorDetectionChange:
                {
                    WriteRegister(RES_CONF, 0x0F); // Set Temperature resolution to 64 internal averaging and Pressure resolution to 512 internal averaging..
                    WriteRegister(CTRL_REG1, 0xC0); // Set ODR to 25 Hz
                    WriteRegister(FIFO_CTRL, 0xC3); // Turn on FIFO Mean mode and set FIFO_MEAN_MODE sample size  to 4
                    _registerData = ReadRegister(CTRL_REG2)[0];
                    _registerData |= 0x50;
                    WriteRegister(CTRL_REG2, _registerData); // Turn on FIFO Mode and FIFO Mean Mode
                    break;
                }
                case RecommendedModes.PortableDeviceLowPower:
                {
                    WriteRegister(RES_CONF, 0x0F); // Set Temperature resolution to 64 internal averaging and Pressure resolution to 512 internal averaging..
                    WriteRegister(CTRL_REG1, 0xC0); // Set ODR to 25 Hz
                    WriteRegister(FIFO_CTRL, 0xC7); // Turn on FIFO Mean mode and set FIFO_MEAN_MODE sample size  to 8
                    _registerData = ReadRegister(CTRL_REG2)[0];
                    _registerData |= 0x50;
                    WriteRegister(CTRL_REG2, _registerData); // Turn on FIFO Mode and FIFO Mean Mode
                    break;
                }
                case RecommendedModes.PortableDeviceDynamic:
                {
                    WriteRegister(RES_CONF, 0x0F); // Set Temperature resolution to 64 internal averaging and Pressure resolution to 512 internal averaging..
                    WriteRegister(CTRL_REG1, 0xC0); // Set ODR to 25 Hz
                    WriteRegister(FIFO_CTRL, 0xCF); // Turn on FIFO Mean mode and set FIFO_MEAN_MODE sample size  to 16
                    _registerData = ReadRegister(CTRL_REG2)[0];
                    _registerData |= 0x50;
                    WriteRegister(CTRL_REG2, _registerData); // Turn on FIFO Mode and FIFO Mean Mode
                    break;
                }
                case RecommendedModes.IndoorNavigation:
                {
                    WriteRegister(RES_CONF, 0x0F); // Set Temperature resolution to 64 internal averaging and Pressure resolution to 512 internal averaging..
                    WriteRegister(CTRL_REG1, 0xC0); // Set ODR to 25 Hz
                    WriteRegister(FIFO_CTRL, 0xDF); // Turn on FIFO Mean mode and set FIFO_MEAN_MODE sample size  to 32
                    _registerData = ReadRegister(CTRL_REG2)[0];
                    _registerData |= 0x50;
                    WriteRegister(CTRL_REG2, _registerData); // Turn on FIFO Mode and FIFO Mean Mode
                    break;
                }
                default:
                {
                    throw new ArgumentOutOfRangeException(nameof(mode));
                }
            }
        }

        /// <summary>
        /// Reads the altitude from the Barometer Click.
        /// </summary>
        /// <returns> Returns the altitude in meters based on well established formulas.</returns>
        /// <example>
        /// <code language = "C#">
        /// Debug.Print("Altitude is " + _barometer.ReadAltitude().ToString("F0") + " meters");
        /// </code>
        /// </example>
        public Single ReadAltitude()
        {
            Single pressure = ReadPressure(PressureCompensationModes.Uncompensated);
            return (Single) (44330.77*(1.0 - Math.Pow(pressure*100/CalculatePressureAsl(pressure*100), 0.190263)));
        }

        /// <summary>
        /// Reads the temperature from the Barometer click in One-Shot mode.
        /// </summary>
        /// <returns>The temperature as measured in °C. Or if the temperature is read while the Barometer Click is powered down, <see cref="Single.MinValue"/>.</returns>
        /// <example>
        /// <code language = "C#">
        /// Debug.Print("OS Temperature : " + _barometer.ReadTemperatureOneShot().ToString("F2") + " °C");
        /// </code>
        /// </example>
        /// <exception cref="ApplicationException">An ApplicationException will be thrown if attempting to read the temperature while the device is powered off or if attempting to read the temperature when configured for continuous operation.</exception>
        public Single ReadTemperatureOneShot()
        {
            if (ReadRegister(CTRL_REG1)[0] != 0x80) throw new ApplicationException("Cannot perform One-Shot temperature conversion when not in OneShot mode.");
            if (PowerMode == PowerModes.Off) return Single.MaxValue;

            _registerData = ReadRegister(CTRL_REG2)[0];
            _registerData |= 0x01;
            WriteRegister(CTRL_REG2, _registerData);

            do
            {
                _registerData = ReadRegister(CTRL_REG2)[0];
            } while ((_registerData & 0x01) == 0x01);

            return ConvertTwosComplimentBytesToTemperature((this as ITemperature).RawData);
        }

        /// <summary>
        /// Reads the pressure from the Barometer click in One-Shot mode.
        /// </summary>
        /// <param name="compensationMode"></param>
        /// <returns>The pressure as measured in millibars (hPa). Or if the pressure is read while the Barometer Click is powered down, <see cref="Single.MaxValue"/>.</returns>
        /// <example>
        /// <code language = "C#">
        /// Debug.Print("OS Pressure : " + _barometer.ReadPressureOneShot().ToString("F1") + " mBar");
        /// </code>
        /// </example>
        /// <exception cref="ApplicationException">An ApplicationException will be thrown if attempting to read the pressure while the device is powered off or if attempting to read the pressure when configured for continuous operation.</exception>
        public Single ReadPressureOneShot(PressureCompensationModes compensationMode = PressureCompensationModes.SeaLevelCompensated)
        {
            if (ReadRegister(CTRL_REG1)[0] != 0x80) throw new ApplicationException("Cannot perform One-Shot pressure conversion when not in OneShot mode.");
            if (PowerMode == PowerModes.Off) return Single.MaxValue;

            _registerData = ReadRegister(CTRL_REG2)[0];
            _registerData |= 0x01;
            WriteRegister(CTRL_REG2, _registerData);

            do
            {
                _registerData = ReadRegister(CTRL_REG2)[0];
            } while ((_registerData & 0x01) == 0x01);

            Single uncompensatedPressure = ConvertTwosComplimentBytesToPressure((this as IPressure).RawData);
            return compensationMode == PressureCompensationModes.SeaLevelCompensated ? CalculatePressureAsl(uncompensatedPressure * 100.0F) / 100  : uncompensatedPressure;
        }

        /// <summary>
        /// This method sets the pressure offset used for differential pressure measurements.
        /// </summary>
        /// <param name="offset">the offset used for differential pressure measurement. This value is subtracted internally by the Barometer Click.</param>
        /// <param name="enable">The <see cref="Boolean"/> value to enable or disable differential pressure measurements.</param>
        /// <example>Example usage:
        /// <code language = "C#">
        /// _barometer.SetDifferentialPressure(300, true);
        /// </code>
        /// </example>
        public void SetDifferentialPressure(Single offset, Boolean enable)
        {
            AutoZero = enable;
            PressureOffset = enable ? offset : 0;
        }

        /// <summary>
        ///     Resets the module
        /// </summary>
        /// <param name="resetMode">
        ///     The reset mode :
        ///     <para>SOFT reset : generally by sending a software command to the chip</para>
        ///     <para>HARD reset : generally by activating a special chip's pin</para>
        /// </param>
        /// <returns>True if successful or other wise false.</returns>
        /// <exception cref="System.NotSupportedException">A NotSupportedException will be thrown if attempting to perform a hard reset as this module does not support hard resets. Use soft reset instead.</exception>
        public Boolean Reset()
        {
            WriteRegister(CTRL_REG2, 0x04);

            Byte result;
            do
            {
                result = ReadRegister(CTRL_REG2)[0];
            } while (result != 0x00);
            Thread.Sleep(100);

            return result == 0x00;
        }

        #endregion

        #region Interface Implementations

        /// <summary>
        /// Reads the temperature from the Barometer click.
        /// </summary>
        /// <returns>The temperature as measured in °C. Or if the temperature is read while the Barometer Click is powered down, <see cref="Single.MaxValue"/></returns>
        /// <example>
        /// <code language = "C#">
        /// Debug.Print("Temperature : " + _barometer.ReadTemperature().ToString("F2") + " °C");
        /// </code>
        /// </example>
        /// <exception cref="ArgumentException">An ArgumentExceptin will be thrown if attempting to read the temperature as <see cref="TemperatureSources.Object"/> as reading object temperature is not supported by this module.</exception>
        /// <exception cref="ApplicationException">An ApplicationException will be thrown if attempting to read the temperature while the device is powered off or if attempting to read the temperature when configured for One-shot mode.</exception>
        public Single ReadTemperature(TemperatureSources source = TemperatureSources.Ambient)
        {
            if (source == TemperatureSources.Object) throw new ArgumentException(@"Reading Object temperature is not supported by this module, use TemperatureSources.Ambient instead", nameof(source));
            if (ReadRegister(CTRL_REG1)[0] == 0x80) throw new ApplicationException("Cannot perform continuous pressure conversion when in OneShot mode.");
            return PowerMode == PowerModes.Off ? Single.MaxValue : ConvertTwosComplimentBytesToTemperature((this as ITemperature).RawData);
        }

        /// <summary>
        /// Reads the pressure from the Barometer click.
        /// </summary>
        /// <param name="compensationMode">The <see cref="PressureCompensationModes"/> to return.</param>
        /// <returns>The pressure as measured in millibars (hPa). Or if the pressure is read while the Barometer Click is powered down, <see cref="Single.MaxValue"/></returns>
        /// <example>
        /// <code language = "C#">
        /// Debug.Print("Pressure : " + _barometer.ReadPressure().ToString("F1") + " mBar");
        /// </code>
        /// </example>
        /// <exception cref="NotSupportedException">A NotSupportedException will be thrown if attempting to read Seal Level Compensated pressure as this modules does not support Sea Level Compensation.</exception>
        public Single ReadPressure(PressureCompensationModes compensationMode = PressureCompensationModes.SeaLevelCompensated)
        {
            if (ReadRegister(CTRL_REG1)[0] == 0x80) throw new ApplicationException("Cannot perform continuous pressure conversion when in OneShot mode.");
            if (PowerMode == PowerModes.Off) return Single.MaxValue;
            Single uncompensatedPressure = ConvertTwosComplimentBytesToPressure((this as IPressure).RawData);
            return compensationMode == PressureCompensationModes.SeaLevelCompensated ? CalculatePressureAsl(uncompensatedPressure * 100.0F) / 100 : uncompensatedPressure;
        }

        /// <summary>
        /// Gets the raw data of the pressure value.
        /// </summary>
        /// <value>
        /// Raw data in the range depending on sensor's precision (8/10/12 bits, for example)
        /// </value>
        Int32 IPressure.RawData
        {
            get
            {
                do
                {
                    _registerData = ReadRegister(STATUS_REG)[0];
                    Thread.Sleep(1);
                } while ((_registerData & 0x02) == 0x00);

                Byte ph = ReadRegister(PRESS_OUT_H)[0];
                Byte pl = ReadRegister(PRESS_OUT_L)[0];
                Byte pxl = ReadRegister(PRESS_OUT_XL)[0];

                return (ph << 16) | (pl << 8) | pxl;
            }
        }

        /// <summary>
        /// Gets the raw data of the temperature value.
        /// </summary>
        /// <value>
        /// Raw data in the range depending on sensor's precision (8/10/12 bits, for example)
        /// </value>
        Int32 ITemperature.RawData
        {
            get
            {
                do
                {
                    _registerData = ReadRegister(STATUS_REG)[0];
                    Thread.Sleep(1);
                } while ((_registerData & 0x01) == 0x00);

                Byte tl = ReadRegister(TEMP_OUT_L)[0];
                Byte th = ReadRegister(TEMP_OUT_H)[0];

                return (th << 8) | tl;
            }
        }

        #endregion

        #region Private Methods

        private void WriteRegister(Byte registerAddress, Byte value)
        {
            Byte[] writeBuffer = new Byte[2];
            writeBuffer[0] = registerAddress;
            writeBuffer[1] = value;

            lock (Hardware.LockI2C)
            {
                _sensor.WritePartial(writeBuffer);
            }
        }

        private Byte[] ReadRegister(Byte registerAddres, Byte bytesToRead = 1)
        {
            Byte[] readBuffer = new Byte[bytesToRead];

            lock (Hardware.LockI2C)
            {
                _sensor.WriteRead(new[] { registerAddres}, readBuffer );
            }

            return readBuffer;
        }

        private static Single ConvertTwosComplimentBytesToTemperature(Int32 rawTemperature)
        {
            Single temperature;

            // Check for sign bit value
            if (((rawTemperature >> 8) & 0x80) != 0x00)
            {
                rawTemperature = ~rawTemperature & 0xFFFF;
                rawTemperature += 0x01;
                temperature = -rawTemperature / 480F + 42.5F;
            }
            else
            {
                temperature = rawTemperature / 480F + 42.5F;
            }
            return temperature;
        }

        private static Single ConvertTwosComplimentBytesToPressure(Int32 rawPressure)
        {
            Single pressure;

            // Check for sign bit value
            if (((rawPressure >> 16) & 0x80) != 0)
            {
                rawPressure = ~rawPressure & 0xFFFFFF;
                rawPressure += 0x01;
                pressure = -rawPressure / 4096F;
            }
            else
            {
                pressure = rawPressure / 4096F;
            }
            return pressure;
        }

        // This method expects the uncompensated pressure to be in Pascals (Pa) and not mBar.
        private static Single CalculatePressureAsl(Single uncompensatedPressure)
        {
            Single seaLevelCompensation = (Single)(101325 * Math.Pow((288 - 0.0065 * 143) / 288, 5.256));
            return 101325 + uncompensatedPressure - seaLevelCompensation;
        }

        #endregion
    }
}