/*
 * Pressure 11 Click driver for TinyCLR 2.0
 * 
 * Initial revision coded by Stephen Cardinale
 * 
 * Copyright 2020 Stephen Cardinale and MikroBUS.Net
 *
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
using System.Threading;

using Math = System.Math;

#endregion

namespace MBN.Modules
{
    /// <inheritdoc cref="IPressure" />
    /// <inheritdoc cref="ITemperature" />
    /// <summary>
    /// Main class for the Pressure11Click driver
    /// <para><b>Pins used :</b> Scl, Sda, Int</para>
    /// <para><b>This is an I2C Device</b></para>
    /// </summary>
    /// <example>Example usage:
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
    ///     public static class Program
    ///     {
    ///         private static Pressure11Click _sensor;
    ///         private static Pressure11Click.SensorConfiguration configuration;
    ///
    ///         public static void Main()
    ///         {
    ///             _sensor = new Pressure11Click(Hardware.SocketOne, Pressure11Click.I2CAddress.AddressOne)
    ///             {
    ///                 TemperatureUnit = TemperatureUnits.Fahrenheit,
    ///                 PressureCompensation = PressureCompensationModes.Uncompensated
    ///             };
    ///
    ///             Pressure11Click.SensorConfiguration config = new Pressure11Click.SensorConfiguration
    ///             {
    ///                 OutputDataRate = Pressure11Click.ODR.OneHertz,
    ///                 FiFoEnabled = true,
    ///                 StopOnFiFoWatermark = true,
    ///                 FiFoWatermarkLevel = 16
    ///             };
    ///
    ///             _sensor.ConfigureSensor(config);
    ///
    ///             /* Choose one of the following demos to run. */
    ///             //FiFoDemo();
    ///             //FiFoAverageDemo();
    ///             OneShotDemo();
    ///             //ContinuousMeasurementNoFiFoDemo();
    ///         }
    ///
    ///         private static void FiFoAverageDemo()
    ///         {
    ///             configuration = new Pressure11Click.SensorConfiguration
    ///             {
    ///                 OutputDataRate = Pressure11Click.ODR.TenHertz,
    ///                 LowPassFilterConfiguration = Pressure11Click.LowPassFillter.Disabled,
    ///                 BlockDataUpdate = Pressure11Click.BDU.Continuous,
    ///
    ///                 FiFoEnabled = true,
    ///                 StopOnFiFoWatermark = false,
    ///                 AutoIncrementRegisterAddress = false,
    ///
    ///                 FiFoWatermarkLevel = 32,
    ///
    ///                 PowerModeConfiguration = Pressure11Click.PowerModes.LowNoise
    ///             };
    ///
    ///             _sensor.ConfigureSensor(configuration);
    ///
    ///             while (true)
    ///             {
    ///                 while (_sensor.GetFiFoFillLevel() <![CDATA[<]]> 32)
    ///                 {
    ///                     Thread.Sleep(5);
    ///                 }
    ///
    ///                 Single[] fifoBuffer = _sensor.ReadFiFoAverage();
    ///                 Debug.WriteLine("---Pressure 9 Click FiFo Averaging Demo---");
    ///                 Debug.WriteLine($"Pressure.......................: {fifoBuffer[0]:F2} mBar");
    ///                 Debug.WriteLine($"Temperature....................: {fifoBuffer[1]:F2} °F");
    ///                 Debug.WriteLine($"Altitude is....................: {_sensor.Altitude:F1} meters");
    ///                 Debug.WriteLine($"Reference Pressure is..........: {_sensor.ReferencePressure:F2}");
    ///
    ///                 _sensor.ClearFiFoAndRestart();
    ///             }
    ///         }
    ///
    ///         private static void FiFoDemo()
    ///         {
    ///             configuration = new Pressure11Click.SensorConfiguration
    ///             {
    ///                 OutputDataRate = Pressure11Click.ODR.TenHertz,
    ///                 LowPassFilterConfiguration = Pressure11Click.LowPassFillter.High,
    ///                 BlockDataUpdate = Pressure11Click.BDU.Continuous,
    ///
    ///                 FiFoEnabled = true,
    ///                 StopOnFiFoWatermark = true,
    ///                 AutoIncrementRegisterAddress = true,
    ///
    ///                 FiFoWatermarkLevel = 31,
    ///
    ///                 PowerModeConfiguration = Pressure11Click.PowerModes.LowNoise
    ///             };
    ///
    ///             _sensor.ConfigureSensor(configuration);
    ///
    ///             while (true)
    ///             {
    ///                 while (_sensor.GetFiFoFillLevel() <![CDATA[<]]>= configuration.FiFoWatermarkLevel) Thread.Sleep(5);
    ///                 {
    ///                     Single[][] fifoBuffer = _sensor.ReadFiFo();
    ///
    ///                     for (Int32 x = 0; x <![CDATA[<]]> fifoBuffer.Length; x++)
    ///                     {
    ///                         Debug.WriteLine("-----Pressure 9 Click FiFo Demo-----");
    ///                         Debug.WriteLine($"Pressure.......................: {fifoBuffer[0]:F2} mBar");
    ///                         Debug.WriteLine($"Temperature....................: {fifoBuffer[1]:F2} °F");
    ///                         Debug.WriteLine($"Altitude is....................: {_sensor.Altitude:F1} meters");
    ///                         Debug.WriteLine($"Reference Pressure is..........: {_sensor.ReferencePressure:F2}");
    ///                     }
    ///
    ///                     _sensor.ClearFiFoAndRestart();
    ///                 }
    ///             }
    ///         }
    ///
    ///         private static void OneShotDemo()
    ///         {
    ///             configuration = new Pressure11Click.SensorConfiguration
    ///             {
    ///                 OutputDataRate = Pressure11Click.ODR.Powerdown,
    ///                 LowPassFilterConfiguration = Pressure11Click.LowPassFillter.Disabled,
    ///                 BlockDataUpdate = Pressure11Click.BDU.Quiescent,
    ///
    ///                 FiFoEnabled = false,
    ///                 StopOnFiFoWatermark = false,
    ///                 FiFoWatermarkLevel = 0,
    ///                 AutoIncrementRegisterAddress = true,
    ///
    ///                 PowerModeConfiguration = Pressure11Click.PowerModes.LowNoise
    ///             };
    ///
    ///             _sensor.ConfigureSensor(configuration);
    ///
    ///             while (true)
    ///             {
    ///                 Debug.WriteLine("---Pressure 9 Click One-shot Demo---");
    ///                 Debug.WriteLine($"Pressure..............: {_sensor.ReadPressureOneshot():F2} mBar");
    ///                 Debug.WriteLine($"Temperature...........: {_sensor.ReadTemperatureOneshot():F2} °F");
    ///                 Debug.WriteLine($"Altitude is...........: {_sensor.Altitude:F1} meters");
    ///                 Thread.Sleep(5000);
    ///             }
    ///         }
    ///
    ///         private static void ContinuousMeasurementNoFiFoDemo()
    ///         {
    ///             configuration = new Pressure11Click.SensorConfiguration
    ///             {
    ///                 OutputDataRate = Pressure11Click.ODR.OneHertz,
    ///                 LowPassFilterConfiguration = Pressure11Click.LowPassFillter.Disabled,
    ///                 BlockDataUpdate = Pressure11Click.BDU.Quiescent,
    ///
    ///                 FiFoEnabled = false,
    ///                 StopOnFiFoWatermark = false,
    ///                 FiFoWatermarkLevel = 0,
    ///                 AutoIncrementRegisterAddress = true,
    ///
    ///                 PowerModeConfiguration = Pressure11Click.PowerModes.LowNoise
    ///             };
    ///
    ///             _sensor.ConfigureSensor(configuration);
    ///
    ///             while (true)
    ///             {
    ///                 Debug.WriteLine("---Pressure 9 Click Continuous Measurement Demo---");
    ///                 Debug.WriteLine($"Pressure.................: {_sensor.ReadPressure():F2} mBar");
    ///                 Debug.WriteLine($"Temperature..............: {_sensor.ReadTemperature():F2} °F");
    ///                 Debug.WriteLine($"Altitude is..............: {_sensor.Altitude:F1} meters");
    ///             }
    ///         }
    ///     }
    /// }
    /// </code>
    /// </example>
    public sealed class Pressure11Click : IPressure, ITemperature
    {
        #region .ctor

        /// <summary>
        ///     Initializes a new instance of the <see cref="Pressure11Click" /> class.
        /// </summary>
        /// <param name="socket">The socket on which the Pressure11Click module is plugged on MikroBus.Net board</param>
        /// <param name="slaveAddress">The address of the module.</param>
        public Pressure11Click(Hardware.Socket socket, I2CAddress slaveAddress)
        {
            _socket = socket;
#if (NANOFRAMEWORK_1_0)
            _sensor = I2cDevice.Create(new I2cConnectionSettings(socket.I2cBus, (int)slaveAddress, I2cBusSpeed.StandardMode));
#else
			_sensor = I2cController.FromName(socket.I2cBus).GetDevice(new I2cConnectionSettings((Int32) slaveAddress, 100000));
#endif

            RebootDevice();

            if (!WhoAmI()) throw new DeviceInitialisationException("Pressure 11 Click not found on the I2C Bus. Please check your hardware configuration.");
        }

        #endregion

        #region Public ENUMS

        /// <summary>
        /// Possible I2C 7 bit slave addresses based on the position of the Add Sel jumper.
        /// </summary>
        public enum I2CAddress
        {
            /// <summary>
            /// Address one (0x5C) with the Add Sel jumper soldered to the 0 position.
            /// </summary>
            AddressOne = 0x5C,

            /// <summary>
            /// Address one (0x5D) with the Add Sel jumper soldered to the 1 position.
            /// </summary>
            AddressTwo = 0x5D
        }

        /// <summary>
        /// Output data rate for both pressure and temperature measurement
        /// </summary>
        public enum ODR : byte
        {
            /// <summary>
            /// Power-down / One-shot mode enabled. This is the POR (Power On Reset) default setting.
            /// </summary>
            Powerdown = 0x00,

            /// <summary>
            /// Output data rate is 1 Hz or one sample per second.
            /// </summary>
            OneHertz = 0x01,

            /// <summary>
            /// Output data rate is 10 Hz or ten samples per second.
            /// </summary>
            TenHertz = 0x02,

            /// <summary>
            /// Output data rate is 25 Hz or twenty-five samples per second.
            /// </summary>
            TwentyFiveHertz = 0x03,

            /// <summary>
            /// Output data rate is 50 Hz or fifty samples per second.
            /// </summary>
            FiftyHertz = 0x04,

            /// <summary>
            /// Output data rate is 75 Hz or seventy-five samples per second.
            /// </summary>
            SeventyFiveHertz = 0x05
        }

        /// <summary>
        /// Block Data Update for pressure and temperature conversions.
        /// </summary>
        public enum BDU
        {
            /// <summary>
            /// Pressure output registers are continuously updated when operating in continuous mode. This is the POR (Power On Reset) default setting.
            /// </summary>
            Continuous = 0,

            /// <summary>
            /// Quiescent mode where the output registers not updated until MSB, LSB and XLSB registers have been read. Use this mode to guarantee all registers are read on the same ODR.
            /// </summary>
            Quiescent = 1
        }

        /// <summary>
        /// Low-pass Filter Configuration
        /// </summary>
        public enum LowPassFillter
        {
            /// <summary>
            /// Low-pass Filter is disabled. Device bandwith is ODR/2.
            /// </summary>
            Disabled = 0,

            /// <summary>
            /// Low-pass Filter is enabled. Device bandwith is ODR/9.
            /// </summary>
            Low = 2,

            /// <summary>
            /// Low-pass Filter is enabled. Device bandwith is ODR/20.
            /// </summary>
            High = 3
        }

        /// <summary>
        /// The two (2) FiFo modes supported by the this driver. Thera re seven (7) but I have only implemented 2.
        /// </summary>
        public enum FiFoMode
        {
            /// <summary>
            /// In Bypass mode, the FIFO is not operational and it remains empty. Bypass mode is also used to reset or clear the FIFO when in FIFO mode.
            /// </summary>
            BypassMode = 0,

            /// <summary>
            /// In FIFO mode data from the output registers are stored in the FIFO until it is overwritten. To reset FIFO content, set the FIFO mode to Bypass mode. After this reset command it is possible to restart FIFO mode by setting the FIFO mode back to FiFO mode.
            /// </summary>
            FiFoMode = 1
        }

        /// <summary>
        /// Low-power mode configuration
        /// </summary>
        public enum PowerModes
        {
            /// <summary>
            /// Low-noise mode. This is the default POR setting.
            /// </summary>
            LowNoise = 0,

            /// <summary>
            /// Low-current mode.
            /// </summary>
            LowCurrent = 1
        }

        #endregion

        #region Private Fields

        private readonly I2cDevice _sensor;
        private Boolean _autoZero;
        private readonly Hardware.Socket _socket;

        #endregion

        #region Private Constants

        // BAROMETER REGISTERS
        private const Byte LPS33HW_REG_INTERRUPT_CFG = 0x0B;    // Interrupt configuration register
        private const Byte LPS33HW_REG_WHO_AM_I = 0x0F;         // Device Identification Register
        private const Byte LPS33HW_REG_CTRL_REG1 = 0x10;        // Control Register 1
        private const Byte LPS33HW_REG_CTRL_REG2 = 0x11;        // Control Register 2
        private const Byte LPS33HW_REG_FIFO_CTRL = 0x14;        // FIFO Control Register
        private const Byte LPS33HW_REG_REF_P_XL = 0x15;         // Reference pressure Low register
        private const Byte LPS33HW_REG_REF_P_L = 0x16;          // Reference pressure Mid register
        private const Byte LPS33HW_REG_REF_P_H = 0x17;          // Reference pressure High register
        private const Byte LPS33HW_REG_RPDS_L = 0x18;           // Pressure offset Low register
        private const Byte LPS33HW_REG_RPDS_H = 0x19;           // Pressure offset High registers
        private const Byte LPS33HW_REG_RES_CONFIG = 0x1A;       // Resolution Register
        private const Byte LPS33HW_REG_FIFO_STATUS = 0x26;      // FIFO Status Register
        private const Byte LPS33HW_REG_STATUS = 0x27;           // Status Register
        private const Byte LPS33HW_REG_PRESS_OUT_XL = 0x28;     // Pressure Output Value Low Register
        private const Byte LPS33HW_REG_PRESS_OUT_L = 0x29;      // Pressure Output Value Mid Register
        private const Byte LPS33HW_REG_PRESS_OUT_H = 0x2A;      // Pressure Output Value High Register
        private const Byte LPS33HW_REG_TEMP_OUT_L = 0x2B;       // Temperature Output Value Low Register
        private const Byte LPS33HW_REG_TEMP_OUT_H = 0x2C;       // Temperature Output Value High Register
        private const Byte LPS33HW_REG_LPFP_RES = 0x33;         // LowPassFilterConfiguration Reset Register

        private const Byte LPS33HW_PRESS_READY_BIT = 0x01;
        private const Byte LPS33HW_TEMP_READY_BIT = 0x02;
        private const Byte LPS33HW_ONESHOT_BIT = 0x01;

        #endregion

        #region Private Methods

        private Boolean WhoAmI()
        {
            return ReadRegister(LPS33HW_REG_WHO_AM_I, 1)[0] == 0xB1;
        }

        private static Int32 ConvertTwosComplementToInteger(Int32 raw, Byte length)
        {
            if ((raw & (1 << (length - 1))) == 1 << (length - 1))
            {
                raw -= 1 << length;
            }

            return raw;
        }

        private static Byte[] ConvertIntegerToTwosComplement(Int32 pressure)
        {
            if (pressure >> 23 != 1)
            {
                return new[] {(Byte) (pressure & 0xFF), (Byte) ((pressure >> 8) & 0xFF), (Byte) (pressure >> 16)};
            }

            Int32 foo = ~pressure + 1;

            Byte[] result = new Byte[3];

            result[0] = (Byte) (foo & 0xFF);
            result[1] = (Byte) ((foo >> 8) & 0xFF);
            result[2] = (Byte) (foo >> 16);
            return result;
        }

        private static Single CalculateAltitude(Single uncompensatedPressure)
        {
            Single pressASL = CalculatePressureAsl(uncompensatedPressure);
            return (Single) (44330 * (1.0 - Math.Pow(uncompensatedPressure / pressASL, 0.1903)));
        }

        private static Single CalculatePressureAsl(Single uncompensatedPressure)
        {
            Single seaLevelCompensation = (Single) (101325 * Math.Pow((288 - 0.0065D * 143F) / 288F, 5.256F));
            return 101325 + uncompensatedPressure - seaLevelCompensation;
        }

        private Single ScalePressure(Single pressureData)
        {
            switch (PressureCompensation)
            {
                case PressureCompensationModes.SeaLevelCompensated:
                {
                    pressureData = CalculatePressureAsl(pressureData * 100) / 100;
                    break;
                }
                case PressureCompensationModes.Uncompensated:
                {
                    break;
                }
                default:
                {
                    throw new ArgumentOutOfRangeException();
                }
            }

            return pressureData;
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

        private Byte[] ReadRegister(Byte registerAddress, Byte numberOfBytesToRead)
        {
            lock (_socket.LockI2c)
            {
                Byte[] writeBuffer = {registerAddress};
                Byte[] readBuffer = new Byte[numberOfBytesToRead];
                _sensor.WriteRead(writeBuffer, readBuffer);
                return readBuffer;
            }
        }

        private void WriteRegister(Byte registerAdderss, Byte value)
        {
            lock (_socket.LockI2c)
            {
                _sensor.Write(new[] {registerAdderss, value});
            }
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// If AUTOZERO is enabled, the measured pressure is used as a reference on the Pressure Out registers.
        /// From that point on, the output pressure registers are updated : PRESS_OUT = measured pressure - reference pressure.
        /// To return back to normal mode, set AutoZero to false. This also sets the <see cref="ReferencePressure"/> back to zero (0).
        /// </summary>
        /// <example>Example usage:
        /// <code language = "C#">
        /// _sensor.AutoZero = true;
        /// </code>
        /// <code language = "VB">
        ///_sensor.AutoZero = True
        /// </code>
        /// </example>
        public Boolean AutoZero
        {
            get => _autoZero;
            set
            {
                _autoZero = value;

                Byte registerData = ReadRegister(LPS33HW_REG_INTERRUPT_CFG, 1)[0];

                registerData &= 0xDF;

                registerData |= value ? (Byte) 0x20 : (Byte) 0x10;

                WriteRegister(LPS33HW_REG_INTERRUPT_CFG, registerData);
            }
        }

        /// <summary>
        /// All pressure readings are based on this reference pressure value. Pressure readings wil be reported either as a negative or positive value compared to the reference pressure.
        /// <para>You can either set the value of the Reference Pressure or enable <see cref="AutoZero"/> property to have the refreence pressure populated based on the first pressure reading.</para>
        /// </summary>
        /// <example>Example usage:
        /// <code language = "C#">
        /// _sensor.ReferencePressure = 1000.0F
        /// </code>
        /// <code language = "VB">
        /// _sensor.ReferencePressure = 1000.0
        /// </code>
        /// </example>
        public Single ReferencePressure
        {
            get
            {
                Int32 registerData = ReadRegister(LPS33HW_REG_REF_P_XL, 1)[0];
                registerData |= ReadRegister(LPS33HW_REG_REF_P_L, 1)[0] << 8;
                registerData |= ReadRegister(LPS33HW_REG_REF_P_H, 1)[0] << 16;

                return ConvertTwosComplementToInteger(registerData, 24) / 4096.0F;
            }
            set
            {
                if (value > 1260) value = 1260;

                value *= 4096;

                Byte[] data = ConvertIntegerToTwosComplement((Int32) value);

                WriteRegister(LPS33HW_REG_REF_P_XL, data[0]);
                WriteRegister(LPS33HW_REG_REF_P_L, data[1]);
                WriteRegister(LPS33HW_REG_REF_P_H, data[2]);
            }
        }

        /// <summary>
        /// The pressure offset os used to remove any residual offset after manufacture with a one-point calibration.
        /// This value is automatically subtracted from the pressure output registers.
        /// </summary>
        /// <example>Example usage:
        /// <code language = "C#">
        /// _sensor.PressureOffset = 15.7F;
        /// </code>
        /// <code language = "VB">
        /// _sensor.PressureOffset = 15.7
        /// </code>
        /// </example>
        public Single PressureOffset
        {
            get
            {
                Int32 result = ReadRegister(LPS33HW_REG_RPDS_L, 1)[0];
                result |= ReadRegister(LPS33HW_REG_RPDS_H, 1)[0] << 8;
                return result / 16F;
            }
            set
            {
                if (value > 1260) value = 1260;
                if (value < 0) value = 0;

                value *= 16F;

                Byte registerData = (Byte) ((Byte) value & 0xFF);
                WriteRegister(LPS33HW_REG_RPDS_L, registerData);

                registerData = (Byte) (((Int32)value >> 8) & 0xFF);
                WriteRegister(LPS33HW_REG_RPDS_H, registerData);
            }
        }

        /// <summary>
        /// Gets the altitude reading.
        /// <para>It must be noted that the altitude reading is a calculated value, based on well established mathematical formulas.</para>
        /// <para>The altitude value is updated upon each pressure reading, therefore, you must take a pressure reading to obtain the latest altitude reading.</para>
        /// </summary>
        /// <example>Example usage:
        /// <code language = "C#">
        /// Debug.Print("Pressure............: " + _sensor.ReadPressure().ToString("F2") + " mBar");
        /// Debug.Print("Temperature..........: " + _sensor.ReadTemperature().ToString("F2") + " °F");
        /// Debug.Print("Altitude is..........: " + _sensor.Altitude.ToString("F1") + " meters");
        /// </code>
        /// <code language = "VB">
        /// Debug.Print("Pressure............: " <![CDATA[&]]> _sensor.ReadPressure().ToString("F2") <![CDATA[&]]> " mBar")
        /// Debug.Print("Temperature..........: " <![CDATA[&]]> _sensor.ReadTemperature().ToString("F2") <![CDATA[&]]> " °F")
        /// Debug.Print("Altitude is..........: " <![CDATA[&]]> _sensor.Altitude.ToString("F1") <![CDATA[&]]> " meters")
        /// </code>
        /// </example>
        public Single Altitude { get; private set; }

        /// <summary>
        /// Seltects between <see cref="PressureCompensationModes.Uncompensated"/> or <see cref="PressureCompensationModes.SeaLevelCompensated"/> when reporting pressure readings.
        /// </summary>
        /// <example>
        /// <code language = "C#">
        /// _sensor.PressureCompensationMode = PressureCompensationModes.SeaLevelCompensated;
        /// </code>
        /// <code language = "VB">
        /// _sensor.PressureCompensationMode = PressureCompensationModes.SeaLevelCompensated
        /// </code>
        /// </example>
        public PressureCompensationModes PressureCompensation { get; set; }

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
        public TemperatureUnits TemperatureUnit { get; set; }

        #endregion

        #region Public Methods

        /// <summary>
        /// The method is used to refresh the content of the internal registers stored in the Flash memory block.
        /// At device power-up the content of the Flash memory block is transferred to the internal registers related
        /// to the trimming functions to allow correct behavior of the device itself.
        /// If for any reason the content of the trimming registers is modified, it is sufficient to use this method to restore the correct values.
        /// </summary>
        /// <example>Example usage:
        /// <code language = "C#">
        ///  _sensor.RebootDevice();
        /// </code>
        /// </example>
        public void RebootDevice()
        {
            Byte registerData = ReadRegister(LPS33HW_REG_CTRL_REG2, 1)[0];
            registerData |= 0x80;
            WriteRegister(LPS33HW_REG_CTRL_REG2, registerData);

            do
            {
                registerData = ReadRegister(LPS33HW_REG_CTRL_REG2, 1)[0];
                Thread.Sleep(5);
            } while ((registerData & 0x80) == 0x80);
        }

        /// <summary>
        /// Performs a one-shot pressure measurement.
        /// </summary>
        /// <returns>A <see cref="System.Single"/> of the one-shot pressure measurement.</returns>
        /// <remarks>This method is intended to be used when the Output Data Rate of the SensorConfiguration <see cref="SensorConfiguration.OutputDataRate"/> is set to PowerDown.</remarks>
        /// <exception cref="ApplicationException">An ApplicationException will be thrown when attempting to take a one-shot measurement when not in PowerDown mode.</exception>
        /// <example>Example usage:
        /// <code language = "C#">
        /// configuration = new Pressure11Click.SensorConfiguration
        /// {
        ///     OutputDataRate = Pressure11Click.ODR.Powerdown,
        ///     LowPassFilterConfiguration = Pressure11Click.LowPassFillter.High,
        ///     BlockDataUpdate = Pressure11Click.BlockDataUpdate.Quiescent,
        ///     FiFoEnabled = false,
        ///     StopOnFiFoWatermark = false,
        ///     AutoIncrementRegisterAddress = true,
        ///     FiFoWatermarkLevel = 0,
        ///     OperatingMode = Pressure11Click.OperatingMode.LowCurrent,
        /// };
        ///
        /// _sensor.ConfigureSensor(configuration);
        ///
        /// Debug.Print("Pressure..............: " + _sensor.ReadPressureOneshot().ToString("F2") + " mBar");
        /// </code>
        /// </example>
        public Single ReadPressureOneshot()
        {
            Byte registerData = ReadRegister(LPS33HW_REG_CTRL_REG1, 1)[0];

            if ((registerData & 0x70) != 0x00) throw new ApplicationException("You cannot read one-shot pressure when the mode is not set to Powerdown/One-shot mode enabled.");

            registerData = ReadRegister(LPS33HW_REG_CTRL_REG2, 1)[0];

            WriteRegister(LPS33HW_REG_CTRL_REG2, (Byte) (registerData | LPS33HW_ONESHOT_BIT));

            Byte dataReady;

            do
            {
                dataReady = ReadRegister(LPS33HW_REG_CTRL_REG2, 1)[0];
                Thread.Sleep(5);
            } while ((dataReady & LPS33HW_ONESHOT_BIT) == LPS33HW_ONESHOT_BIT);

            Single pressureData = ConvertTwosComplementToInteger((this as IPressure).RawData, 24) / 4096.0F;

            Altitude = CalculateAltitude(pressureData * 100);

            return ScalePressure(pressureData);
        }

        /// <summary>
        /// Performs a one-shot temperature measurement.
        /// </summary>
        /// <returns>A <see cref="System.Single"/> of the one-shot temperature measurement.</returns>
        /// <remarks>This method is intended to be used when the Output Data Rate of the SensorConfiguration <see cref="SensorConfiguration.OutputDataRate"/> is set to PowerDown.</remarks>
        /// <exception cref="ApplicationException">An ApplicationException will be thrown when attempting to take a one-shot measurement when not in PowerDown mode.</exception>
        /// <example>Example usage:
        /// <code language = "C#">
        /// configuration = new Pressure11Click.SensorConfiguration
        /// {
        ///     OutputDataRate = Pressure11Click.ODR.Powerdown,
        ///     LowPassFilterConfiguration = Pressure11Click.LowPassFillter.High,
        ///     BlockDataUpdate = Pressure11Click.BlockDataUpdate.Quiescent,
        ///     FiFoEnabled = false,
        ///     StopOnFiFoWatermark = false,
        ///     AutoIncrementRegisterAddress = true,
        ///     FiFoWatermarkLevel = 0,
        ///     OperatingMode = Pressure11Click.OperatingMode.LowCurrent,
        /// };
        ///
        /// _sensor.ConfigureSensor(configuration);
        ///
        /// Debug.Print("Pressure..............: " + _sensor.ReadTemperatureOneshot().ToString("F2") + " mBar");
        /// </code>
        /// </example>
        public Single ReadTemperatureOneshot()
        {
            Byte registerData = ReadRegister(LPS33HW_REG_CTRL_REG1, 1)[0];

            if ((registerData & 0x70) != 0x00) throw new ApplicationException("You cannot read one-shot pressure when the mode is not set to PowerDown/One-shot mode enabled.");

            registerData = ReadRegister(LPS33HW_REG_CTRL_REG2, 1)[0];

            WriteRegister(LPS33HW_REG_CTRL_REG2, (Byte) (registerData | LPS33HW_ONESHOT_BIT));

            Byte dataReady;

            do
            {
                dataReady = ReadRegister(LPS33HW_REG_CTRL_REG2, 1)[0];
                Thread.Sleep(5);
            } while ((dataReady & LPS33HW_ONESHOT_BIT) == LPS33HW_ONESHOT_BIT);

            return ScaleTemperature(ConvertTwosComplementToInteger((this as ITemperature).RawData, 16) / 100.0F);
        }

        /// <summary>
        /// Configures the LPS33HW IC on the Pressure11 Click.
        /// </summary>
        /// <param name="configuration">The <see cref="SensorConfiguration"/> to use.</param>
        /// <example>Example usage:
        /// <code language = "C#">
        /// configuration = new Pressure11Click.SensorConfiguration
        /// {
        ///     OutputDataRate = Pressure11Click.ODR.Powerdown,
        ///     LowPassFilterConfiguration = Pressure11Click.LowPassFillter.High,
        ///     BlockDataUpdate = Pressure11Click.BlockDataUpdate.Quiescent,
        ///     FiFoEnabled = false,
        ///     StopOnFiFoWatermark = false,
        ///     AutoIncrementRegisterAddress = true,
        ///     FiFoWatermarkLevel = 0,
        ///     OperatingMode = Pressure11Click.OperatingMode.LowCurrent,
        /// };
        ///
        /// _sensor.ConfigureSensor(configuration);
        ///
        /// Debug.Print("Pressure..............: " + _sensor.ReadPressureOneshot().ToString("F2") + " mBar");
        /// </code>
        /// </example>
        public void ConfigureSensor(SensorConfiguration configuration)
        {
            // RES_CONFIG register must be set while device is in Idle mode.
            WriteRegister(LPS33HW_REG_CTRL_REG1, 0x00);
            // Read LPS33HW_REG_LPFP_RES register to clear the existing low pass filter setting.
            ReadRegister(LPS33HW_REG_LPFP_RES, 1);
            //Write new low pass filter setting.
            Int32 registerData = (Int32) configuration.PowerModeConfiguration;
            WriteRegister(LPS33HW_REG_RES_CONFIG, (Byte) registerData);

            registerData = (Int32) configuration.OutputDataRate << 4;
            registerData |= (Int32) configuration.LowPassFilterConfiguration << 2;
            registerData |= (Int32) configuration.BlockDataUpdate << 1;
            WriteRegister(LPS33HW_REG_CTRL_REG1, (Byte) registerData);

            registerData = configuration.FiFoEnabled ? 0x40 : 0x00;
            registerData |= configuration.StopOnFiFoWatermark ? 0x20 : 0x00;
            registerData |= configuration.AutoIncrementRegisterAddress ? 0x10 : 0x00;
            WriteRegister(LPS33HW_REG_CTRL_REG2, (Byte) registerData);

            registerData = configuration.FiFoEnabled ? 0x20 : 0x00;
            if (configuration.FiFoWatermarkLevel > 31) configuration.FiFoWatermarkLevel = 31;
            registerData |= configuration.FiFoWatermarkLevel;
            WriteRegister(LPS33HW_REG_FIFO_CTRL, (Byte) registerData);
        }

        /// <summary>
        /// Returs the current numbers of samples stored in the FiFo buffer.
        /// </summary>
        /// <example>Example usage:
        /// <code language = "C#">
        ///  Debug.Print("Number of stored readings is " + _sensor.GetFiFoFillLevel());
        /// </code>
        /// </example>
        public Byte GetFiFoFillLevel()
        {
            Byte registerData = ReadRegister(LPS33HW_REG_FIFO_STATUS, 1)[0];
            return (Byte) (registerData & 0x3F);
        }

        /// <summary>
        /// Returns an array containing the average of all pressure and temperature readings in the FiFo buffer.
        /// </summary>
        /// <example>Example usage:
        /// <code language = "C#">
        /// configuration = new Pressure11Click.SensorConfiguration
        /// {
        ///     OutputDataRate = Pressure11Click.ODR.Powerdown,
        ///     LowPassFilterConfiguration = Pressure11Click.LowPassFillter.High,
        ///     BlockDataUpdate = Pressure11Click.BlockDataUpdate.Quiescent,
        ///     FiFoEnabled = false,
        ///     StopOnFiFoWatermark = false,
        ///     AutoIncrementRegisterAddress = true,
        ///     FiFoWatermarkLevel = 0,
        ///     OperatingMode = Pressure11Click.OperatingMode.LowCurrent,
        /// };
        ///
        /// _sensor.ConfigureSensor(configuration);
        ///
        /// while (true)
        /// {
        ///     while (_sensor.GetFiFoFillLevel() <![CDATA[<]]> 32) Thread.Sleep(5);
        ///
        ///     Single[] fifoBuffer = _sensor.ReadFiFoAverage();
        ///     Debug.Print("---Pressure 9 Click FiFo Averaging Demo---");
        ///     Debug.Print("Pressure.......................: " + fifoBuffer[0].ToString("F2") + " mBar");
        ///     Debug.Print("Temperature....................: " + fifoBuffer[1].ToString("F2") + " °F");
        ///     Debug.Print("Altitude is....................: " + _sensor.Altitude.ToString("F1") + " meters");
        ///     Debug.Print("Reference Pressure is..........: " + _sensor.ReferencePressure.ToString("F2"));
        ///
        ///     _sensor.ClearFiFoAndRestart();
        /// }
        /// </code>
        /// </example>
        public Single[] ReadFiFoAverage()
        {
            // Make sure that register address is automatically incremented.
            Byte registerData = ReadRegister(LPS33HW_REG_CTRL_REG2, 1)[0];
            WriteRegister(LPS33HW_REG_CTRL_REG2, (Byte) (registerData | 0x10));

            Single fillLevel = GetFiFoFillLevel();
            Single pressureData = 0;
            Single temperatureData = 0;

            Byte[] rawFiFoData = ReadRegister(LPS33HW_REG_PRESS_OUT_XL, (Byte) (fillLevel * 5));

            for (Int32 x = 0; x < fillLevel * 5; x += 5)
            {
                pressureData += ConvertTwosComplementToInteger(rawFiFoData[x] | (rawFiFoData[x + 1] << 8) | (rawFiFoData[x + 2] << 16), 24) / 4096.0F;
                temperatureData += ConvertTwosComplementToInteger(rawFiFoData[x + 3] | (rawFiFoData[x + 4] << 8), 16) / 100.0F;
            }

            pressureData /= fillLevel;
            temperatureData /= fillLevel;

            Altitude = CalculateAltitude(pressureData * 100);

            // Write previous contents of LPS33HW_REG_CTRL_REG2.
            WriteRegister(LPS33HW_REG_CTRL_REG2, registerData);

            return new[] {ScalePressure(pressureData), ScaleTemperature(temperatureData)};
        }

        /// <summary>
        /// Returns an jagged containing all pressure and temperature readings in the FiFo buffer.
        /// </summary>
        /// <returns></returns>
        /// <example>Example usage:
        /// <code language = "C#">
        /// configuration = new Pressure11Click.SensorConfiguration
        /// {
        ///     OutputDataRate = Pressure11Click.ODR.TenHertz,
        ///     LowPassFilterConfiguration = Pressure11Click.LowPassFillter.High,
        ///     BlockDataUpdate = Pressure11Click.BlockDataUpdate.Continuous,
        ///     FiFoEnabled = true,
        ///     StopOnFiFoWatermark = true,
        ///     AutoIncrementRegisterAddress = true,
        ///     FiFoWatermarkLevel = 31,
        ///     OperatingMode = Pressure11Click.OperatingMode.LowNoise
        /// };
        ///
        /// _sensor.ConfigureSensor(configuration);
        /// 
        /// while (true)
        /// {
        ///     while (_sensor.GetFiFoFillLevel() <![CDATA[<]]>= configuration.FiFoWatermarkLevel) Thread.Sleep(5);
        ///     {
        ///         Single[][] fifoBuffer = _sensor.ReadFiFo();
        /// 
        ///         for (Int32 x = 0; x <![CDATA[<]]> fifoBuffer.Length; x++)
        ///         {
        ///             Debug.Print("-----Pressure 9 Click FiFo Demo-----");
        ///             Debug.Print("Pressure reading " +  x + " ......: " + fifoBuffer[x][0].ToString("F2") + " mBar");
        ///             Debug.Print("Temperature reading " + x + " ...: " + fifoBuffer[x][1].ToString("F2") + " °F");
        ///             Debug.Print("Altitude is...............: " + _sensor.Altitude.ToString("F1") + " meters");
        ///             Debug.Print("Reference Pressure is..........: " + _sensor.ReferencePressure.ToString("F2"));
        ///         }
        /// 
        ///         _sensor.ClearFiFoAndRestart();
        ///     }
        /// }
        /// </code>
        /// </example>
        public Single[][] ReadFiFo()
        {
            // Make sure that register address is automatically incremented.
            Byte registerData = ReadRegister(LPS33HW_REG_CTRL_REG2, 1)[0];
            WriteRegister(LPS33HW_REG_CTRL_REG2, (Byte) (registerData | 0x10));

            Int32 numberOfReadings = ReadRegister(LPS33HW_REG_FIFO_STATUS, 1)[0] & 0x3F;
            Single[][] result = new Single[numberOfReadings][];

            for (Byte x = 0; x < numberOfReadings; x++)
            {
                result[x] = new Single[2];
            }

            Byte[] rawFiFoData = ReadRegister(LPS33HW_REG_PRESS_OUT_XL, (Byte) (numberOfReadings * 5));

            for (Int32 x = 0; x < numberOfReadings * 5; x += 5)
            {
                result[x / 5][0] = ScalePressure(ConvertTwosComplementToInteger(rawFiFoData[x] | (rawFiFoData[x + 1] << 8) | (rawFiFoData[x + 2] << 16), 24) / 4096.0F);
                result[x / 5][1] = ScaleTemperature(ConvertTwosComplementToInteger(rawFiFoData[x + 3] | (rawFiFoData[x + 4] << 8), 16) / 100.0F);
            }

            Altitude = CalculateAltitude(result[0][0] * 100);

            // Write previous contents of LPS33HW_REG_CTRL_REG2.
            WriteRegister(LPS33HW_REG_CTRL_REG2, registerData);

            return result;
        }

        /// <summary>
        /// Clears and restarts the FiFo buffer when operating in <see cref="SensorConfiguration.FiFoEnabled"/> is set to true.
        /// <remarks>This method needs to be called when the FiFo buffer is equal to the <see cref="SensorConfiguration.FiFoWatermarkLevel"/> or when the FiFo buffer has reached its maximum capacity of 32 samples.</remarks>
        /// </summary>
        /// <example>
        /// <code language = "C#">
        /// _sensor.ClearFiFoAndRestart();
        /// </code>
        /// </example>
        public void ClearFiFoAndRestart()
        {
            Byte registerData = ReadRegister(LPS33HW_REG_FIFO_CTRL, 1)[0];

            if ((registerData & 0xE0) == 0x00) return;

            registerData &= 0x1F;
            WriteRegister(LPS33HW_REG_FIFO_CTRL, registerData);

            registerData |= 0x20;
            WriteRegister(LPS33HW_REG_FIFO_CTRL, registerData);
        }

        #endregion

        #region Interface Implementations

        /// <summary>
        /// Gets or sets the power mode.
        /// </summary>
        /// <example> This sample shows how to use the PowerMode property.
        /// <code language="C#">
        /// _thermo8.PowerMode = PowerModes.Low;
        /// </code>
        /// </example>
        /// <value>
        /// The current power mode of the module.
        /// </value>
        /// <exception cref="T:System.NotImplementedException">A System.NotImplementedException will be thrown if the property is set as it does not support direct setting of the power mode. Use the <see cref="SensorConfiguration"/> class and <see cref="ConfigureSensor"/> method instead.</exception>
        public PowerModes PowerMode
        {
            get
            {
                Byte registerData = ReadRegister(LPS33HW_REG_CTRL_REG2, 1)[0];
                return (registerData & 0x70) == 0x00 ? PowerModes.LowNoise : PowerModes.LowCurrent;
            }
            set =>
                throw new NotImplementedException(
                    "Direct setting of PowerMode is not supported by this driver. Use the SensorConfiguration class and ConfigureSensor method to adjust the Output Data Rate property of the SensorConfiguration class.");
        }

        /// <summary>
        /// Resets the module
        /// </summary>
        /// <example>Example usage:
        /// <code language = "C#">
        /// _sensor.Reset();
        /// </code>
        /// </example>
        public Boolean Reset( )
        {
            Byte registerData = ReadRegister(LPS33HW_REG_CTRL_REG2, 1)[0];
            registerData |= 0x04;
            WriteRegister(LPS33HW_REG_CTRL_REG2, registerData);

            do
            {
                registerData = ReadRegister(LPS33HW_REG_CTRL_REG2, 1)[0];
                Thread.Sleep(5);
            } while ((registerData & 0x04) == 0x04);

            return true;
        }

        /// <inheritdoc cref = "IPressure"/>
        /// <summary>
        /// Reads the pressure from the Pressure 11 Click.
        /// </summary>
        /// <param name="compensationMode">The <see cref="PressureCompensationModes"/> which you want to return.</param>
        /// <exception cref="ApplicationException">An ApplicationException will be thrown if attempting to use this method when the <see cref="SensorConfiguration.OutputDataRate"/> is not configured for cotinuous measurement.</exception> >
        /// <example>Example usage:
        /// <code language="C#">
        /// configuration = new Pressure11Click.SensorConfiguration
        /// {
        ///     OutputDataRate = Pressure11Click.ODR.OneHertz,
        ///     LowPassFilterConfiguration = Pressure11Click.LowPassFillter.Disabled,
        ///     BlockDataUpdate = Pressure11Click.BlockDataUpdate.Quiescent,
        ///
        ///     FiFoEnabled = false,
        ///     StopOnFiFoWatermark = false,
        ///     FiFoWatermarkLevel = 0,
        ///     AutoIncrementRegisterAddress = true,
        ///
        ///     OperatingMode = Pressure11Click.OperatingMode.LowNoise
        /// };
        ///
        /// _sensor.ConfigureSensor(configuration);
        ///
        /// while (true)
        /// {
        ///     Debug.Print("---Pressure 9 Click Continuous Measurement Demo---");
        ///     Debug.Print("Pressure.................: " + _sensor.ReadPressure().ToString("F2") + " mBar");
        ///     Debug.Print("Temperature..............: " + _sensor.ReadTemperature().ToString("F2") + " °F");
        ///     Debug.Print("Altitude is..............: " + _sensor.Altitude.ToString("F1") + " meters");
        /// }
        /// </code>
        /// </example>
        public Single ReadPressure(PressureCompensationModes compensationMode = PressureCompensationModes.SeaLevelCompensated)
        {
            if (compensationMode == PressureCompensationModes.Uncompensated) throw new NotSupportedException("This module does not support reading of uncompensated pressure. Use SeaLevelCompensated instead.");
            Byte registerData = ReadRegister(LPS33HW_REG_CTRL_REG1, 1)[0];

            if ((registerData & 0x70) == 0x00) throw new ApplicationException("You cannot perform a continuous pressure measurement when the Output Data Rate is set to Powerdown/One-shot mode enabled. Use the ReadPressureOneshot() method instead.");

            Single pressureData = ConvertTwosComplementToInteger((this as IPressure).RawData, 24) / 4096.0F;

            Altitude = CalculateAltitude(pressureData * 100);

            return ScalePressure(pressureData);
        }

        /// <inheritdoc cref = "ITemperature"/>
        /// <summary>
        /// 
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        /// <example>Example usage:
        /// <code language="C#">
        /// configuration = new Pressure11Click.SensorConfiguration
        /// {
        ///     OutputDataRate = Pressure11Click.ODR.OneHertz,
        ///     LowPassFilterConfiguration = Pressure11Click.LowPassFillter.Disabled,
        ///     BlockDataUpdate = Pressure11Click.BlockDataUpdate.Quiescent,
        ///
        ///     FiFoEnabled = false,
        ///     StopOnFiFoWatermark = false,
        ///     FiFoWatermarkLevel = 0,
        ///     AutoIncrementRegisterAddress = true,
        ///
        ///     OperatingMode = Pressure11Click.OperatingMode.LowNoise
        /// };
        ///
        /// _sensor.ConfigureSensor(configuration);
        ///
        /// while (true)
        /// {
        ///     Debug.Print("---Pressure 9 Click Continuous Measurement Demo---");
        ///     Debug.Print("Pressure.................: " + _sensor.ReadPressure().ToString("F2") + " mBar");
        ///     Debug.Print("Temperature..............: " + _sensor.ReadTemperature().ToString("F2") + " °F");
        ///     Debug.Print("Altitude is..............: " + _sensor.Altitude.ToString("F1") + " meters");
        /// }
        /// </code>
        /// </example>
        public Single ReadTemperature(TemperatureSources source = TemperatureSources.Ambient)
        {
            Byte registerData = ReadRegister(LPS33HW_REG_CTRL_REG1, 1)[0];

            if ((registerData & 0x70) == 0x00)
                throw new ApplicationException(
                    "You cannot perform a continuous pressure measurement when the Output Data Rate is set to Powerdown/One-shot mode enabled. Use the ReadTemperatureOneshot() method instead.");

            return ScaleTemperature(ConvertTwosComplementToInteger((this as ITemperature).RawData, 16) / 100.0F);
        }

        /// <inheritdoc cref = "ITemperature"/>
        /// <summary>
        /// <summary>Gets the raw temperature value.</summary>
        /// </summary>
        /// <example>Example usage:
        /// <code language="C#">
        /// Debug.Print("Pressure.....: " + (_sensor as ITemperature).RawData.ToString("F2") + " mBar");
        /// </code>
        /// </example>
        Int32 ITemperature.RawData
        {
            get
            {
                Byte registerData;

                do
                {
                    registerData = ReadRegister(LPS33HW_REG_STATUS, 1)[0];
                    Thread.Sleep(5);
                } while ((registerData & LPS33HW_TEMP_READY_BIT) == 0x00);

                if ((ReadRegister(LPS33HW_REG_CTRL_REG2, 1)[0] & 0x10) == 0x10)
                {
                    Byte[] rawBytes = ReadRegister(LPS33HW_REG_TEMP_OUT_L, 2);
                    return (rawBytes[1] << 8) | rawBytes[0];
                }

                Int32 rawData = ReadRegister(LPS33HW_REG_TEMP_OUT_L, 1)[0];
                rawData |= ReadRegister(LPS33HW_REG_TEMP_OUT_H, 1)[0] << 8;
                return rawData;
            }
        }

        /// <inheritdoc cref = "IPressure"/>
        /// <summary>Gets the raw pressure value.</summary>
        /// <value>
        /// Raw data in the range depending on sensor's precision (8/10/12 bits, for example)
        /// </value>
        /// <example>Example usage:
        /// <code language="C#">
        /// Debug.Print("Pressure.....: " + (_sensor as IPressure).RawData.ToString("F2") + " mBar");
        /// </code>
        /// </example>
        Int32 IPressure.RawData
        {
            get
            {
                Byte registerData;

                do
                {
                    registerData = ReadRegister(LPS33HW_REG_STATUS, 1)[0];
                    Thread.Sleep(5);
                } while ((registerData & LPS33HW_PRESS_READY_BIT) == 0x00);

                if ((ReadRegister(LPS33HW_REG_CTRL_REG2, 1)[0] & 0x10) == 0x10)
                {
                    Byte[] rawBytes = ReadRegister(LPS33HW_REG_PRESS_OUT_XL, 3);
                    return (rawBytes[2] << 16) | (rawBytes[1] << 8) | rawBytes[0];
                }

                Int32 rawData = ReadRegister(LPS33HW_REG_PRESS_OUT_XL, 1)[0];
                rawData |= ReadRegister(LPS33HW_REG_PRESS_OUT_L, 1)[0] << 8;
                rawData |= ReadRegister(LPS33HW_REG_PRESS_OUT_H, 1)[0] << 16;
                return rawData;
            }
        }

        #endregion

        #region Sensor Configuration Class

        /// <summary>
        /// A configuration class to hold the configuration parameters of the Pressure 11 Click.
        /// </summary>
        public class SensorConfiguration
        {
            /// <summary>
            /// The default constructor of the Configuration Class.
            /// </summary>
            /// <example>Example usage:
            /// <code language = "C#">
            /// configuration = new Pressure11Click.SensorConfiguration
            /// {
            ///     ODR = Pressure11Click.ODR.TenHertz,
            ///     LowPassFilterConfiguration = Pressure11Click.LowPassFillter.Disabled,
            ///     BlockDataUpdate = Pressure11Click.BlockDataUpdate.Continuous,
            ///     FiFoEnabled = true,
            ///     StopOnFiFoWatermark = false,
            ///     AutoIncrementRegisterAddress = false,
            ///     FiFoWatermarkLevel = 32,
            ///     OperatingMode = Pressure11Click.OperatingMode.LowNoise
            /// };
            /// _sensor.ConfigureSensor(configuration);
            /// </code>
            /// </example>
            public SensorConfiguration()
            {
                OutputDataRate = ODR.Powerdown;
                LowPassFilterConfiguration = LowPassFillter.Disabled;
                BlockDataUpdate = BDU.Continuous;
                AutoIncrementRegisterAddress = true;
                PowerModeConfiguration = PowerModes.LowNoise;
                FiFoEnabled = false;
                StopOnFiFoWatermark = false;
                FiFoWatermarkLevel = 0;
            }

            /// <summary>
            /// The frequency or data rate used for continuous pressure and temperature measurements. The default Power-On-Reset (POR) value is <see cref="ODR.Powerdown"/>
            /// </summary>
            /// <example>Example usage:
            /// <code language = "C#">
            /// Pressure11Click.SensorConfiguration config = new Pressure11Click.SensorConfiguration();
            /// config.ODR = Pressure11Click.ODR.OneHertz;
            /// _sensor.ConfigureSensor(configuration);
            /// </code>
            /// </example>
            public ODR OutputDataRate { get; set; }

            /// <summary>
            /// Sets the low pass filter for pressure conversions. The default Power-On-Reset (POR) value is <see cref="LowPassFillter.Disabled"/>
            /// </summary>
            /// <example>Example usage:
            /// <code language = "C#">
            /// Pressure11Click.SensorConfiguration config = new Pressure11Click.SensorConfiguration();
            /// config.LowPassFilterConfiguration = Pressure11Click.LowPassFillter.High;
            /// _sensor.ConfigureSensor(configuration);
            /// </code>
            /// </example>
            public LowPassFillter LowPassFilterConfiguration { get; set; }

            /// <summary>
            /// Used to inhibit pressure out registers from being updated until MSB and LSB registers have been read. The default Power-On-Reset (POR) value is <see cref="BDU.Continuous"/>
            /// When the value is set to <see cref="BDU.Quiescent"/> the content of the output registers is not updated until the pressure registers are read, avoiding the reading of values related to different samples.
            /// </summary>
            /// <example>Example usage:
            /// <code language = "C#">
            /// Pressure11Click.SensorConfiguration config = new Pressure11Click.SensorConfiguration();
            /// config.BlockDataUpdate = Pressure11Click.BlockDataUpdate.Continuous;
            /// _sensor.ConfigureSensor(configuration);
            /// </code>
            /// </example>
            public BDU BlockDataUpdate { get; set; }

            /// <summary>
            /// Enables or disables pointer register address automatically incremented during a multiple byte access.The default Power-On-Reset (POR) value is enabled (True).
            /// </summary>
            /// <remarks>This driver automatically handles reading with both cases.</remarks>
            /// <example>Example usage:
            /// <code language = "C#">
            /// Pressure11Click.SensorConfiguration config = new Pressure11Click.SensorConfiguration();
            /// config.AutoIncrementRegisterAddress = true;
            /// _sensor.ConfigureSensor(config);
            /// </code>
            /// </example>
            public Boolean AutoIncrementRegisterAddress { get; set; }

            /// <summary>
            /// Sets or gets the low power configuration. The default Power-On-Reset (POR) value is <see cref="PowerModes.LowNoise"/>.
            /// </summary>
            /// <example>Example usage:
            /// <code language = "C#">
            /// Pressure11Click.SensorConfiguration config = new Pressure11Click.SensorConfiguration();
            /// config.OperatingMode = Pressure11Click.OperatingMode.LowCurrent;
            /// _sensor.ConfigureSensor(config);
            /// </code>
            /// </example>
            public PowerModes PowerModeConfiguration { get; set; }

            /// <summary>
            /// Enables or disables the use of the FiFo. The default Power-On-Reset (POR) value is FiFo disabled.
            /// </summary>
            /// <example>Example usage:
            /// <code language = "C#">
            /// Pressure11Click.SensorConfiguration config = new Pressure11Click.SensorConfiguration();
            /// config.OutputDataRate = Pressure11Click.ODR.OneHertz;
            /// config.FiFoEnabled = true;
            /// config.StopOnFiFoWatermark = true;
            /// config.FiFoWatermarkLevel = 16;
            /// _sensor.ConfigureSensor(config);
            /// </code>
            /// </example>
            public Boolean FiFoEnabled { get; set; }

            /// <summary>
            /// Gets or sets the watermark level of the FiFo. When set to true, the sensor will stop measurements when the FiFo buffer contains the number of samples set in <see cref="FiFoWatermarkLevel"/>.
            /// The default Power-On-Reset (POR) value is 0.
            /// </summary>
            /// <example>Example usage:
            /// <code language = "C#">
            /// Pressure11Click.SensorConfiguration config = new Pressure11Click.SensorConfiguration();
            /// config.OutputDataRate = Pressure11Click.ODR.OneHertz;
            /// config.FiFoEnabled = true;
            /// config.StopOnFiFoWatermark = true;
            /// config.FiFoWatermarkLevel = 16;
            /// _sensor.ConfigureSensor(config);
            /// </code>
            /// </example>
            public Boolean StopOnFiFoWatermark { get; set; }

            /// <summary>
            /// Gets or sets the number of samples in the FiFo buffer that the sensor will stop taking measurements. The default Power-On/Reset (POR) is zero (0).
            /// </summary>
            /// <example>Example usage:
            /// <code language = "C#">
            /// Pressure11Click.SensorConfiguration config = new Pressure11Click.SensorConfiguration();
            /// config.OutputDataRate = Pressure11Click.ODR.OneHertz;
            /// config.FiFoEnabled = true;
            /// config.StopOnFiFoWatermark = true;
            /// config.FiFoWatermarkLevel = 16;
            /// _sensor.ConfigureSensor(config);
            /// </code>
            /// </example>
            public Byte FiFoWatermarkLevel { get; set; }
        }

        #endregion
    }
}