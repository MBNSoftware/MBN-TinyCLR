/* 
* MikroBus.Net main assembly for TinyCLR 2.0
* 
* Version 1.0 : 
* - Initial revision
* 
* Copyright 2020 MikroBus.Net 
* Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License. You may obtain a copy of the License at 
* http://www.apache.org/licenses/LICENSE-2.0 
* Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, 
* either express or implied. See the License for the specific language governing permissions and limitations under the License. 
*/
using GHIElectronics.TinyCLR.Devices.Gpio;
using GHIElectronics.TinyCLR.Pins;
using System;
using System.Collections;

namespace MBN
{
    #region Enums
    /// <summary>
    /// Units used by the ITemperature interface
    /// </summary>
    public enum TemperatureUnits
    {
        /// <summary>
        /// Celsius unit
        /// </summary>
        Celsius,
        /// <summary>
        /// Fahrenheit unit
        /// </summary>
        Fahrenheit,
        /// <summary>
        /// Kelvin unit
        /// </summary>
        Kelvin
    }

    /// <summary>
    /// Temperature sources used by the ITemperature interface.
    /// </summary>
    public enum TemperatureSources
    {
        /// <summary>
        /// Measures the ambient (room) temperature.
        /// </summary>
        Ambient,
        /// <summary>
        /// Measures an object temperature, either via external sensor or IR sensor, for example.
        /// </summary>
        Object
    }

    /// <summary>
    /// Measurement modes used by the IHumidity interface.
    /// </summary>
    public enum HumidityMeasurementModes
    {
        /// <summary>
        /// Relative humidity measurement mode
        /// </summary>
        Relative,
        /// <summary>
        /// Absolute humidity measurement mode
        /// </summary>
        Absolute
    }

    /// <summary>
    /// Compensation modes for pressure sensors
    /// </summary>
    public enum PressureCompensationModes
    {
        /// <summary>
        /// Sea level compensated
        /// </summary>
        SeaLevelCompensated,
        /// <summary>
        /// Raw uncompensated
        /// </summary>
        Uncompensated
    }

    /// <summary>
    /// Power modes that may be applicable to a module
    /// </summary>
    public enum PowerModes : Byte
    {
        /// <summary>
        /// Module is turned off, meaning it generally can't perform measures or operate
        /// </summary>
        Off,
        /// <summary>
        /// Module is either in hibernate mode or low power mode (depending on the module)
        /// </summary>
        Low,
        /// <summary>
        /// Module is turned on, at full power, meaning it is fully functionnal
        /// </summary>
        On
    }

    /// <summary>
    /// Reset modes that may be applicable to a module
    /// </summary>
    public enum ResetModes : Byte
    {
        /// <summary>
        /// Software reset, which usually consists in a command sent to the device.
        /// </summary>
        Soft,
        /// <summary>
        /// Hardware reset, which usually consists in toggling a IO pin connected to the device.
        /// </summary>
        Hard
    }

    /// <summary>
    /// Interface used for drivers using humidity sensors
    /// </summary>
    public interface IHumidity
    {
        /// <summary>
        /// Reads the relative or absolute humidity value from the sensor.
        /// </summary>
        /// <returns>A single representing the relative/absolute humidity as read from the sensor, in percentage (%) for relative reading or value in case of absolute reading.</returns>
        Single ReadHumidity(HumidityMeasurementModes measurementMode = HumidityMeasurementModes.Relative);

        /// <summary>
        /// Gets the raw data of the humidity value.
        /// </summary>
        /// <value>
        /// Raw data in the range depending on sensor's precision (8/10/12 bits, for example)
        /// </value>
        Int32 RawData { get; }
    }
    #endregion

    #region Interfaces
    /// <summary>
    /// Interface used for drivers using pressure sensors
    /// </summary>
    public interface IPressure
    {
        /// <summary>
        /// Reads the pressure from the sensor.
        /// </summary>
        /// <param name="compensationMode">Indicates if the pressure reading returned by the sensor is see-level compensated or not.</param>
        /// <returns>A single representing the pressure read from the source, in hPa (hectoPascal)</returns>
        Single ReadPressure(PressureCompensationModes compensationMode = PressureCompensationModes.SeaLevelCompensated);

        /// <summary>
        /// Gets the raw data of the pressure value.
        /// </summary>
        /// <value>
        /// Raw data in the range depending on sensor's precision (8/10/12 bits, for example)
        /// </value>
        Int32 RawData { get; }
    }

    /// <summary>
    /// Interface used for drivers using temperature sensors
    /// </summary>
    public interface ITemperature
    {
        /// <summary>
        /// Reads the temperature.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <returns>A single representing the temperature read from the source, degrees Celsius</returns>
        Single ReadTemperature(TemperatureSources source = TemperatureSources.Ambient);

        /// <summary>
        /// Gets the raw data of the temperature value.
        /// </summary>
        /// <value>
        /// Raw data in the range depending on sensor's precision (8/10/12 bits, for example)
        /// </value>
        Int32 RawData { get; }
    }
    #endregion

    #region Hardware Class
    /// <summary>
    /// Main Hardware class for MikroBus.Net board
    /// </summary>
    public static class Hardware
    {
        #region Socket Class
        /// <summary>
        /// MikroBus socket description
        /// </summary>
        public class Socket
        {
            /// <summary>
            /// Analog pin
            /// </summary>
            public Int32 AnPin;

            /// <summary>
            /// Analog Channel
            /// </summary>
            public Int32 AdcChannel;

            /// <summary>
            /// ADC Controller
            /// </summary>
            public String AdcController;

            /// <summary>
            /// UART COM port
            /// </summary>
            public String ComPort;

            /// <summary>
            /// Chip Select pin for SPI
            /// </summary>
            public Int32 Cs;

            /// <summary>
            /// Interrupt pin
            /// </summary>
            public Int32 Int;

            /// <summary>
            /// SPI Master IN Slave OUT pin
            /// </summary>
            public Int32 Miso;

            /// <summary>
            /// SPI Master OUT Slave IN pin
            /// </summary>
            public Int32 Mosi;

            /// <summary>
            /// Socket's name
            /// </summary>
            public String Name;

            /// <summary>
            /// PWM Controller
            /// </summary>
            public String PwmController;

            /// <summary>
            /// PWM channel
            /// </summary>
            public Int32 PwmChannel;

            /// <summary>
            /// PWM pin
            /// </summary>
            public Int32 PwmPin;

            /// <summary>
            /// Reset pin
            /// </summary>
            public Int32 Rst;

            /// <summary>
            /// UART Receive pin
            /// </summary>
            public Int32 Rx;

            /// <summary>
            /// SPI Clock line
            /// </summary>
            public Int32 Sck;

            /// <summary>
            /// I²C Serial Clock
            /// </summary>
            public Int32 Scl;

            /// <summary>
            /// I²C Serial Data
            /// </summary>
            public Int32 Sda;

            /// <summary>
            /// SPI module
            /// </summary>
            public String SpiBus;

            /// <summary>
            /// I2C Bus
            /// </summary>
            public String I2cBus;

            /// <summary>
            /// UART Transmit pin
            /// </summary>
            public Int32 Tx;
        }
        #endregion

        #region MBN sockets definitions

        #region Ram board definitions
        public static class RamBoard
        {
            /// <summary>ADC channel definitions.</summary>
            public static class AdcChannel
            {
                /// <summary>ADC controller.</summary>
                public static class Controller1
                {
                    public const String Id = STM32H7.AdcChannel.Adc1;

                    public const Int32 PA0C = STM32H7.AdcChannel.Channel0;
                    public const Int32 PA1C = STM32H7.AdcChannel.Channel1;
                    public const Int32 PA6 = STM32H7.AdcChannel.Channel3;
                    public const Int32 PB1 = STM32H7.AdcChannel.Channel5;
                    public const Int32 PA7 = STM32H7.AdcChannel.Channel7;
                    public const Int32 PB0 = STM32H7.AdcChannel.Channel9;
                    public const Int32 PC0 = STM32H7.AdcChannel.Channel10;
                    public const Int32 PC2 = STM32H7.AdcChannel.Channel12;
                    public const Int32 PC3 = STM32H7.AdcChannel.Channel13;
                    public const Int32 PA3 = STM32H7.AdcChannel.Channel15;
                    public const Int32 PA0 = STM32H7.AdcChannel.Channel16;
                    public const Int32 PA4 = STM32H7.AdcChannel.Channel18;
                    public const Int32 PA5 = STM32H7.AdcChannel.Channel19;
                }
                /// <summary>ADC controller.</summary>
                public static class Controller3
                {
                    public const String Id = STM32H7.AdcChannel.Adc3;

                    public const Int32 PC2C = STM32H7.AdcChannel.Channel0;
                    public const Int32 PC3C = STM32H7.AdcChannel.Channel1;
                    public const Int32 PF9 = STM32H7.AdcChannel.Channel2;
                    public const Int32 PF7 = STM32H7.AdcChannel.Channel3;
                    public const Int32 PF10 = STM32H7.AdcChannel.Channel6;
                    public const Int32 PF8 = STM32H7.AdcChannel.Channel7;
                    public const Int32 PF6 = STM32H7.AdcChannel.Channel8;
                    public const Int32 PH4 = STM32H7.AdcChannel.Channel15;
                }
            }

            /// <summary>DAC channel definitions.</summary>
            public static class DacChannel
            {
                public const String Id = STM32H7.DacChannel.Id;

                public const Int32 PA4 = STM32H7.DacChannel.Channel1;
                public const Int32 PA5 = STM32H7.DacChannel.Channel2;
            }

            /// <summary>Network controller definitions.</summary>
            public static class NetworkController
            {
                public const String ATWinc15x0 = "GHIElectronics.TinyCLR.NativeApis.ATWINC15xx.NetworkController";
                public const String Enc28j60 = "GHIElectronics.TinyCLR.NativeApis.ENC28J60.NetworkController";
                public const String Ppp = "GHIElectronics.TinyCLR.NativeApis.Ppp.NetworkController";
            }

            /// <summary>PWM pin definitions.</summary>
            public static class PwmChannel
            {
                /// <summary>PWM controller 1.</summary>
                public static class Controller1
                {
                    public const String Id = STM32H7.PwmChannel.Tim1;

                    public const Int32 PK1 = STM32H7.PwmChannel.Channel0;
                    public const Int32 PJ11 = STM32H7.PwmChannel.Channel1;
                    public const Int32 PJ9 = STM32H7.PwmChannel.Channel2;

                }

                /// <summary>PWM controller 2.</summary>
                public static class Controller2
                {
                    public const String Id = STM32H7.PwmChannel.Tim2;

                    public const Int32 PA15 = STM32H7.PwmChannel.Channel0;
                    public const Int32 PB3 = STM32H7.PwmChannel.Channel1;
                    public const Int32 PA3 = STM32H7.PwmChannel.Channel3;
                }

                /// <summary>PWM controller 3.</summary>
                public static class Controller3
                {
                    public const String Id = STM32H7.PwmChannel.Tim3;

                    public const Int32 PC6 = STM32H7.PwmChannel.Channel0;
                    public const Int32 PC7 = STM32H7.PwmChannel.Channel1;
                    public const Int32 PB0 = STM32H7.PwmChannel.Channel2;
                    public const Int32 PB1 = STM32H7.PwmChannel.Channel3;
                }

                /// <summary>PWM controller 4.</summary>
                public static class Controller4
                {
                    public const String Id = STM32H7.PwmChannel.Tim4;

                    public const Int32 PB7 = STM32H7.PwmChannel.Channel1;

                }

                /// <summary>PWM controller 5.</summary>
                public static class Controller5
                {
                    public const String Id = STM32H7.PwmChannel.Tim5;

                    public const Int32 PA0 = STM32H7.PwmChannel.Channel0;
                    public const Int32 PH11 = STM32H7.PwmChannel.Channel1;
                    public const Int32 PH12 = STM32H7.PwmChannel.Channel2;
                    public const Int32 PI0 = STM32H7.PwmChannel.Channel3;
                }

                /// <summary>PWM controller 8.</summary>
                public static class Controller8
                {
                    public const String Id = STM32H7.PwmChannel.Tim8;

                    public const Int32 PI5 = STM32H7.PwmChannel.Channel0;
                    public const Int32 PI6 = STM32H7.PwmChannel.Channel1;
                    public const Int32 PI7 = STM32H7.PwmChannel.Channel2;
                    public const Int32 PI2 = STM32H7.PwmChannel.Channel3;
                }

                /// <summary>PWM controller 12.</summary>
                public static class Controller12
                {
                    public const String Id = STM32H7.PwmChannel.Tim12;

                    public const Int32 PH6 = STM32H7.PwmChannel.Channel0;
                    public const Int32 PH9 = STM32H7.PwmChannel.Channel1;
                }

                /// <summary>PWM controller 13.</summary>
                public static class Controller13
                {
                    public const String Id = STM32H7.PwmChannel.Tim13;

                    public const Int32 PF8 = STM32H7.PwmChannel.Channel0;
                }

                /// <summary>PWM controller 14.</summary>
                public static class Controller14
                {
                    public const String Id = STM32H7.PwmChannel.Tim14;

                    public const Int32 PF9 = STM32H7.PwmChannel.Channel0;
                }

                /// <summary>PWM controller 15.</summary>
                public static class Controller15
                {
                    public const String Id = STM32H7.PwmChannel.Tim15;

                    public const Int32 PE5 = STM32H7.PwmChannel.Channel0;
                    public const Int32 PE6 = STM32H7.PwmChannel.Channel1;
                }

                /// <summary>PWM controller 16.</summary>
                public static class Controller16
                {
                    public const String Id = STM32H7.PwmChannel.Tim16;

                    public const Int32 PB8 = STM32H7.PwmChannel.Channel0;
                }

                /// <summary>PWM controller 17.</summary>
                public static class Controller17
                {
                    public const String Id = STM32H7.PwmChannel.Tim17;

                    public const Int32 PB9 = STM32H7.PwmChannel.Channel0;
                }
            }

            /// <summary>UART port definitions.</summary>
            public static class UartPort
            {
                /// <summary>UART port on PA9 (TX) and PA10 (RX).</summary>
                public const String Usart1 = STM32H7.UartPort.Usart1;

                /// <summary>UART port on PD5 (TX) and PD6 (RX), PD3 (CTS) and PD4 (RTS).</summary>
                public const String Usart2 = STM32H7.UartPort.Usart2;

                /// <summary>UART port on PC10 (TX) and PC11 (RX).</summary>
                public const String Usart3 = STM32H7.UartPort.Usart3;

                /// <summary>UART port on PH13 (TX) and PH14 (RX), PB0 (CTS) and PA15 (RTS).</summary>
                public const String Uart4 = STM32H7.UartPort.Uart4;

                /// <summary>UART port on PB13 (TX) and PB12 (RX), PC9 (CTS) and PC8 (RTS).</summary>
                public const String Uart5 = STM32H7.UartPort.Uart5;

                /// <summary>UART port on PC6 (TX) and PC7 (RX).</summary>
                public const String Usart6 = STM32H7.UartPort.Usart6;

                /// <summary>UART port on PF7 (TX) and PF6 (RX), PF9 (CTS) and PF8 (RTS).</summary>
                public const String Uart7 = STM32H7.UartPort.Uart7;

                /// <summary>UART port on PJ8 (TX) and PJ9 (RX).</summary>
                public const String Uart8 = STM32H7.UartPort.Uart8;
            }

            /// <summary>I2C bus definitions.</summary>
            public static class I2cBus
            {
                /// <summary>I2C bus on PB9 (SDA) and PB8 (SCL).</summary>
                public const String I2c1 = STM32H7.I2cBus.I2c1;
                /// <summary>I2C bus on PB11 (SDA) and PB10 (SCL).</summary>
                public const String I2c2 = STM32H7.I2cBus.I2c2;
                /// <summary>I2C bus on PH8 (SDA) and PH7 (SCL).</summary>
                public const String I2c3 = STM32H7.I2cBus.I2c3;
            }

            /// <summary>SPI bus definitions.</summary>
            public static class SpiBus
            {
                /// <summary>SPI bus on PI3 (MOSI), PI2 (MISO), and PI1 (SCK).</summary>
                public const String Spi2 = STM32H7.SpiBus.Spi2;
                /// <summary>SPI bus on PB5 (MOSI), PB4 (MISO), and PB3 (SCK).</summary>
                public const String Spi3 = STM32H7.SpiBus.Spi3;
                /// <summary>SPI bus on PJ10 (MOSI), PJ11 (MISO), and PK0 (SCK).</summary>
                public const String Spi5 = STM32H7.SpiBus.Spi5;
            }

            /// <summary>CAN bus definitions.</summary>
            public static class CanBus
            {
                /// <summary>CAN bus on PH13 (TX) and PH14 (RX).</summary>
                public const String Can1 = STM32H7.CanBus.Can1;
                /// <summary>CAN bus on PB13 (TX) and PB12 (RX).</summary>
                public const String Can2 = STM32H7.CanBus.Can2;
            }

            /// <summary>Storage controller definitions.</summary>
            public static class StorageController
            {
                /// <summary>API id.</summary>
                public const String SdCard = STM32H7.StorageController.SdCard;
                public const String UsbHostMassStorage = STM32H7.StorageController.UsbHostMassStorage;
                //public const String QuadSpi = STM32H7.StorageController.QuadSpi;
            }

            /// <summary>RTC controller definitions.</summary>
            public static class RtcController
            {
                /// <summary>API id.</summary>
                public const String Id = STM32H7.RtcController.Id;
            }
        }
        #endregion

        #region Ram board sockets
        /// <summary>
        /// Socket #1 on MikroBus.Net Ram Board
        /// </summary>
        public static readonly Socket SocketOne = new Socket
        {
            AdcController = RamBoard.AdcChannel.Controller1.Id,
            AdcChannel = RamBoard.AdcChannel.Controller1.PC0,
            AnPin = STM32H7.GpioPin.PC0,
            Rst = STM32H7.GpioPin.PJ3,
            Cs = STM32H7.GpioPin.PJ4,
            SpiBus = RamBoard.SpiBus.Spi2,
            Sck = STM32H7.GpioPin.PI1,
            Miso = STM32H7.GpioPin.PI2,
            Mosi = STM32H7.GpioPin.PI3,
            PwmController = RamBoard.PwmChannel.Controller2.Id,
            PwmChannel = RamBoard.PwmChannel.Controller2.PA15,
            PwmPin = STM32H7.GpioPin.PA15,
            Int = STM32H7.GpioPin.PJ5,
            ComPort = RamBoard.UartPort.Usart6,
            Rx = STM32H7.GpioPin.PC7,
            Tx = STM32H7.GpioPin.PC6,
            I2cBus = RamBoard.I2cBus.I2c2,
            Scl = STM32H7.GpioPin.PB10,
            Sda = STM32H7.GpioPin.PB11,
            Name = "SocketOne"
        };

        /// <summary>
        /// Socket #2 on MikroBus.Net Ram Board
        /// </summary>
        public static readonly Socket SocketTwo = new Socket
        {
            AdcController = RamBoard.AdcChannel.Controller1.Id,
            AdcChannel = RamBoard.AdcChannel.Controller1.PC3,
            AnPin = STM32H7.GpioPin.PC3,
            Rst = STM32H7.GpioPin.PJ6,
            Cs = STM32H7.GpioPin.PH15,
            SpiBus = RamBoard.SpiBus.Spi2,
            Sck = STM32H7.GpioPin.PI1,
            Miso = STM32H7.GpioPin.PI2,
            Mosi = STM32H7.GpioPin.PI3,
            PwmController = RamBoard.PwmChannel.Controller1.Id,
            PwmChannel = RamBoard.PwmChannel.Controller1.PK1,
            PwmPin = STM32H7.GpioPin.PK1,
            Int = STM32H7.GpioPin.PI15,
            ComPort = RamBoard.UartPort.Usart2,
            Rx = STM32H7.GpioPin.PD6,
            Tx = STM32H7.GpioPin.PD5,
            I2cBus = RamBoard.I2cBus.I2c2,
            Scl = STM32H7.GpioPin.PB10,
            Sda = STM32H7.GpioPin.PB11,
            Name = "SocketTwo"
        };

        /// <summary>
        /// Socket #3 on MikroBus.Net Ram Board
        /// </summary>
        public static readonly Socket SocketThree = new Socket
        {
            AdcController = RamBoard.AdcChannel.Controller1.Id,
            AdcChannel = RamBoard.AdcChannel.Controller1.PC2,
            AnPin = STM32H7.GpioPin.PC2,
            Rst = STM32H7.GpioPin.PH4,
            Cs = STM32H7.GpioPin.PJ15,
            SpiBus = RamBoard.SpiBus.Spi2,
            Sck = STM32H7.GpioPin.PI1,
            Miso = STM32H7.GpioPin.PI2,
            Mosi = STM32H7.GpioPin.PI3,
            PwmController = RamBoard.PwmChannel.Controller3.Id,
            PwmChannel = RamBoard.PwmChannel.Controller3.PB1,
            PwmPin = STM32H7.GpioPin.PB1,
            Int = STM32H7.GpioPin.PK2,
            ComPort = RamBoard.UartPort.Uart8,
            Rx = STM32H7.GpioPin.PJ9,
            Tx = STM32H7.GpioPin.PJ8,
            I2cBus = RamBoard.I2cBus.I2c2,
            Scl = STM32H7.GpioPin.PB10,
            Sda = STM32H7.GpioPin.PB11,
            Name = "SocketThree"
        };

        /// <summary>
        /// Socket #4 on MikroBus.Net Ram Board
        /// </summary>
        public static readonly Socket SocketFour = new Socket
        {
            AdcController = RamBoard.AdcChannel.Controller3.Id,
            AdcChannel = RamBoard.AdcChannel.Controller3.PF8,
            AnPin = STM32H7.GpioPin.PF8,
            Rst = STM32H7.GpioPin.PI12,
            Cs = STM32H7.GpioPin.PA6,
            SpiBus = RamBoard.SpiBus.Spi3,
            Sck = STM32H7.GpioPin.PB3,
            Miso = STM32H7.GpioPin.PB4,
            Mosi = STM32H7.GpioPin.PB5,
            PwmController = RamBoard.PwmChannel.Controller3.Id,
            PwmChannel = RamBoard.PwmChannel.Controller3.PB0,
            PwmPin = STM32H7.GpioPin.PB0,
            Int = STM32H7.GpioPin.PK7,
            ComPort = RamBoard.UartPort.Uart7,
            Rx = STM32H7.GpioPin.PF6,
            Tx = STM32H7.GpioPin.PF7,
            I2cBus = RamBoard.I2cBus.I2c3,
            Scl = STM32H7.GpioPin.PH7,
            Sda = STM32H7.GpioPin.PH8,
            Name = "SocketFour"
        };

        /// <summary>
        /// Socket #5 on MikroBus.Net Ram Board
        /// </summary>
        public static readonly Socket SocketFive = new Socket
        {
            AdcController = RamBoard.AdcChannel.Controller3.Id,
            AdcChannel = RamBoard.AdcChannel.Controller3.PF9,
            AnPin = STM32H7.GpioPin.PF9,
            Rst = STM32H7.GpioPin.PE4,
            Cs = STM32H7.GpioPin.PI4,
            SpiBus = RamBoard.SpiBus.Spi3,
            Sck = STM32H7.GpioPin.PB3,
            Miso = STM32H7.GpioPin.PB4,
            Mosi = STM32H7.GpioPin.PB5,
            PwmController = RamBoard.PwmChannel.Controller5.Id,
            PwmChannel = RamBoard.PwmChannel.Controller5.PH12,
            PwmPin = STM32H7.GpioPin.PH12,
            Int = STM32H7.GpioPin.PE6,
            ComPort = RamBoard.UartPort.Usart1,
            Rx = STM32H7.GpioPin.PA10,
            Tx = STM32H7.GpioPin.PA9,
            I2cBus = RamBoard.I2cBus.I2c3,
            Scl = STM32H7.GpioPin.PH7,
            Sda = STM32H7.GpioPin.PH8,
            Name = "SocketFive"
        };

        /// <summary>
        /// Socket #6 on MikroBus.Net Ram Board
        /// </summary>
        public static readonly Socket SocketSix = new Socket
        {
            AdcController = RamBoard.AdcChannel.Controller3.Id,
            AdcChannel = RamBoard.AdcChannel.Controller3.PF10,
            AnPin = STM32H7.GpioPin.PF10,
            Rst = STM32H7.GpioPin.PG9,
            Cs = STM32H7.GpioPin.PH10,
            SpiBus = RamBoard.SpiBus.Spi3,
            Sck = STM32H7.GpioPin.PB3,
            Miso = STM32H7.GpioPin.PB4,
            Mosi = STM32H7.GpioPin.PB5,
            PwmController = RamBoard.PwmChannel.Controller12.Id,
            PwmChannel = RamBoard.PwmChannel.Controller12.PH9,
            PwmPin = STM32H7.GpioPin.PH9,
            Int = STM32H7.GpioPin.PG10,
            ComPort = RamBoard.UartPort.Uart5,
            Rx = STM32H7.GpioPin.PB12,
            Tx = STM32H7.GpioPin.PB13,
            I2cBus = RamBoard.I2cBus.I2c3,
            Scl = STM32H7.GpioPin.PH7,
            Sda = STM32H7.GpioPin.PH8,
            Name = "SocketSix"
        };
        #endregion

        #region SC20100 sockets
        /// <summary>
        /// Socket #1 on GHI SC20100 dev board
        /// </summary>
        public static readonly Socket SC20100_1 = new Socket
        {
            AdcController = SC20100.AdcChannel.Controller1.Id,
            AdcChannel = SC20100.AdcChannel.Controller1.PC0,
            AnPin = SC20100.GpioPin.PC0,
            Rst = SC20100.GpioPin.PD4,
            Cs = SC20100.GpioPin.PD3,
            Int = SC20100.GpioPin.PC5,
            Rx = SC20100.GpioPin.PD6,
            Tx = SC20100.GpioPin.PD5,
            Scl = SC20100.GpioPin.PB8,
            Sda = SC20100.GpioPin.PB9,
            Sck = SC20100.GpioPin.PB3,
            Miso = SC20100.GpioPin.PB4,
            Mosi = SC20100.GpioPin.PB5,
            SpiBus = SC20100.SpiBus.Spi3,
            I2cBus = SC20100.I2cBus.I2c1,
            PwmController = SC20100.PwmChannel.Controller2.Id,
            PwmChannel = SC20100.PwmChannel.Controller2.PA15,
            PwmPin = SC20100.GpioPin.PA15,
            ComPort = SC20100.UartPort.Usart2,
            Name = "SC20100_1"
        };

        /// <summary>
        /// Socket #2 on GHI SC20100 dev board
        /// </summary>
        public static readonly Socket SC20100_2 = new Socket
        {
            AdcController = SC20100.AdcChannel.Controller1.Id,
            AdcChannel = SC20100.AdcChannel.Controller1.PC1,
            AnPin = SC20100.GpioPin.PC1,
            Rst = SC20100.GpioPin.PD15,
            Cs = SC20100.GpioPin.PD14,
            Int = SC20100.GpioPin.PA8,
            Rx = SC20100.GpioPin.PD9,
            Tx = SC20100.GpioPin.PD8,
            Scl = SC20100.GpioPin.PB8,
            Sda = SC20100.GpioPin.PB9,
            Sck = SC20100.GpioPin.PB3,
            Miso = SC20100.GpioPin.PB4,
            Mosi = SC20100.GpioPin.PB5,
            SpiBus = SC20100.SpiBus.Spi3,
            I2cBus = SC20100.I2cBus.I2c2,
            PwmController = SC20100.PwmChannel.Controller13.Id,
            PwmChannel = SC20100.PwmChannel.Controller13.PA6,
            PwmPin = SC20100.GpioPin.PA6,
            ComPort = SC20100.UartPort.Usart3,
            Name = "SC20100_2"
        };
        #endregion

        #region SC20260 sockets
        /// <summary>
        /// Socket #1 on GHI SC20260 dev board
        /// </summary>
        public static readonly Socket SC20260_1 = new Socket
        {
            AdcController = SC20260.AdcChannel.Controller3.Id,
            AdcChannel = SC20260.AdcChannel.Controller3.PF10,
            AnPin = SC20260.GpioPin.PF10,
            Rst = SC20260.GpioPin.PI8,
            Cs = SC20260.GpioPin.PG12,
            Int = SC20260.GpioPin.PG6,
            Rx = SC20260.GpioPin.PJ9,
            Tx = SC20260.GpioPin.PJ8,
            Scl = SC20260.GpioPin.PB8,
            Sda = SC20260.GpioPin.PB9,
            Sck = SC20260.GpioPin.PB3,
            Miso = SC20260.GpioPin.PB4,
            Mosi = SC20260.GpioPin.PB5,
            SpiBus = SC20260.SpiBus.Spi3,
            I2cBus = SC20260.I2cBus.I2c1,
            PwmController = SC20260.PwmChannel.Controller5.Id,
            PwmChannel = SC20260.PwmChannel.Controller5.PI0,
            PwmPin = SC20260.GpioPin.PI0,
            ComPort = SC20260.UartPort.Uart8,
            Name = "SC20260_1"
        };

        /// <summary>
        /// Socket #2 on GHI SC20260 dev board
        /// </summary>
        public static readonly Socket SC20260_2 = new Socket
        {
            AdcController = SC20260.AdcChannel.Controller1.Id,
            AdcChannel = SC20260.AdcChannel.Controller1.PC2,
            AnPin = SC20260.GpioPin.PC2,
            Rst = SC20260.GpioPin.PI11,
            Cs = SC20260.GpioPin.PC13,
            Int = SC20260.GpioPin.PJ13,
            Rx = SC20260.GpioPin.PC7,
            Tx = SC20260.GpioPin.PC6,
            Scl = SC20260.GpioPin.PB8,
            Sda = SC20260.GpioPin.PB9,
            Sck = SC20260.GpioPin.PB3,
            Miso = SC20260.GpioPin.PB4,
            Mosi = SC20260.GpioPin.PB5,
            SpiBus = SC20260.SpiBus.Spi3,
            I2cBus = SC20260.I2cBus.I2c1,
            PwmController = SC20260.PwmChannel.Controller8.Id,
            PwmChannel = SC20260.PwmChannel.Controller8.PI5,
            PwmPin = SC20260.GpioPin.PI5,
            ComPort = SC20260.UartPort.Usart6,
            Name = "SC20260_2"
        };
        #endregion

        #region FEZ Stick sockets
        /// <summary>
        /// Socket #1 on FEZ Stick Board
        /// </summary>
        public static readonly Socket FEZStick_1 = new Socket
        {
            AdcController = "GHIElectronics.TinyCLR.NativeApis.STM32H7.AdcController\\1",
            AdcChannel = STM32H7.AdcChannel.Channel10,
            AnPin = STM32H7.GpioPin.PC0,
            Rst = STM32H7.GpioPin.PD4,
            Cs = STM32H7.GpioPin.PD3,
            Int = STM32H7.GpioPin.PC5,
            I2cBus = STM32H7.I2cBus.I2c1,
            Scl = STM32H7.GpioPin.PB8,
            Sda = STM32H7.GpioPin.PB9,
            SpiBus = STM32H7.SpiBus.Spi4,
            Sck = STM32H7.GpioPin.PE12,
            Miso = STM32H7.GpioPin.PE13,
            Mosi = STM32H7.GpioPin.PE14,
            PwmController = "GHIElectronics.TinyCLR.NativeApis.STM32H7.PwmController\\15",
            PwmChannel = STM32H7.PwmChannel.Channel0,
            PwmPin = STM32H7.GpioPin.PE5,
            ComPort = STM32H7.UartPort.Usart1,
            Rx = STM32H7.GpioPin.PA10,
            Tx = STM32H7.GpioPin.PA9,
            Name = "FEZStick_1"
        };

        /// <summary>
        /// Socket #2 on FEZ Stick Board
        /// </summary>
        public static readonly Socket FEZStick_2 = new Socket
        {
            AdcController = "GHIElectronics.TinyCLR.NativeApis.STM32H7.AdcController\\0",
            AdcChannel = STM32H7.AdcChannel.Channel0,
            AnPin = STM32H7.GpioPin.PC1,
            Rst = STM32H7.GpioPin.PD15,
            Cs = STM32H7.GpioPin.PD14,
            Int = STM32H7.GpioPin.PA8,
            I2cBus = STM32H7.I2cBus.I2c2,
            Scl = STM32H7.GpioPin.PB10,
            Sda = STM32H7.GpioPin.PB11,
            SpiBus = STM32H7.SpiBus.Spi3,
            Sck = STM32H7.GpioPin.PB3,
            Miso = STM32H7.GpioPin.PB4,
            Mosi = STM32H7.GpioPin.PB5,
            PwmController = "GHIElectronics.TinyCLR.NativeApis.STM32H7.PwmController\\2",
            PwmChannel = STM32H7.PwmChannel.Channel3,
            PwmPin = STM32H7.GpioPin.PA3,
            ComPort = STM32H7.UartPort.Uart5,
            Rx = STM32H7.GpioPin.PB12,
            Tx = STM32H7.GpioPin.PB13,
            Name = "FEZStick_2"
        };
        #endregion

        #region Screwdrivers exposed sockets
        /// <summary>
        /// UART Screw terminal #1
        /// </summary>
        public static readonly Socket UartHeader1 = new Socket
        {
            Rx = STM32H7.GpioPin.PF6,
            Tx = STM32H7.GpioPin.PF7,
            ComPort = RamBoard.UartPort.Uart7,
            Name = "UartHeader1"
        };

        /// <summary>
        /// UART Screw terminal #2
        /// </summary>
        public static readonly Socket UartHeader2 = new Socket
        {
            Rx = STM32H7.GpioPin.PJ9,
            Tx = STM32H7.GpioPin.PJ8,
            ComPort = RamBoard.UartPort.Uart8,
            Name = "UartHeader2"
        };

        /// <summary>
        /// I2C Screw terminal
        /// </summary>
        public static readonly Socket I2cHeader = new Socket
        {
            Scl = STM32H7.GpioPin.PB8,
            Sda = STM32H7.GpioPin.PB9,
            I2cBus = RamBoard.I2cBus.I2c1,
            Name = "I2cHeader"
        };

        /// <summary>
        /// Onboard Flash memory
        /// </summary>
        public static readonly Socket OnboardFlash = new Socket
        {
            Cs = STM32H7.GpioPin.PK3,
            Sck = STM32H7.GpioPin.PK0,
            Miso = STM32H7.GpioPin.PJ11,
            Mosi = STM32H7.GpioPin.PJ10,
            SpiBus = RamBoard.SpiBus.Spi5,
            Name = "OnboardFlash"
        };

        /// <summary>
        /// SPI Screw terminal
        /// </summary>
        public static readonly Socket SpiHeader = new Socket
        {
            Cs = STM32H7.GpioPin.PA4,
            Sck = STM32H7.GpioPin.PB3,
            Miso = STM32H7.GpioPin.PB4,
            Mosi = STM32H7.GpioPin.PB5,
            SpiBus = RamBoard.SpiBus.Spi3,
            Name = "SpiHeader"
        };
        #endregion

        #endregion

        #region Public objects
        /// <summary>
        /// RAM onboard Led 1 (Green)
        /// </summary>
        public static readonly GpioPin Led1;

        /// <summary>
        /// RAM onboard Led 2 (Orange)
        /// </summary>
        public static readonly GpioPin Led2;

        /// <summary>
        /// RAM onboard Led 3 (Red)
        /// </summary>
        public static readonly GpioPin Led3;

        /// <summary>
        /// SC20260D onboard Led 1 (Green)
        /// </summary>
        public static readonly GpioPin SC20260D_Led1;

        /// <summary>
        /// SC20260D onboard Led 2 (Red)
        /// </summary>
        public static readonly GpioPin SC20260D_Led2;

        /// <summary>
        /// Lock object used for I2C
        /// </summary>
        public static readonly Object LockI2C;

        /// <summary>
        /// Lock object used for SPI
        /// </summary>
        public static readonly Object LockSPI;
        #endregion

        #region .ctor
        // Constructor
        static Hardware()
        {
            LockI2C = new Object();
            LockSPI = new Object();
            Led1 = GpioController.GetDefault().OpenPin(SC20260.GpioPin.PK4);
            Led1.SetDriveMode(GpioPinDriveMode.Output);
            Led1.Write(GpioPinValue.Low);
            Led2 = GpioController.GetDefault().OpenPin(SC20260.GpioPin.PK5);
            Led2.SetDriveMode(GpioPinDriveMode.Output);
            Led2.Write(GpioPinValue.Low);
            Led3 = GpioController.GetDefault().OpenPin(SC20260.GpioPin.PK6);
            Led3.SetDriveMode(GpioPinDriveMode.Output);
            Led3.Write(GpioPinValue.Low);
            
            SC20260D_Led1 = GpioController.GetDefault().OpenPin(SC20260.GpioPin.PH11);
            SC20260D_Led1.SetDriveMode(GpioPinDriveMode.Output);
            SC20260D_Led1.Write(GpioPinValue.Low);

            SC20260D_Led2 = GpioController.GetDefault().OpenPin(SC20260.GpioPin.PB0);
            SC20260D_Led2.SetDriveMode(GpioPinDriveMode.Output);
            SC20260D_Led2.Write(GpioPinValue.Low);
        }
        #endregion
    }
    #endregion

    #region Bits manipulation class
    /// <summary>
    /// Bits manipulation class
    /// </summary>
    public class Bits
    {
        /// <summary>
        /// Determines whether a bit is set at a given position in a byte.
        /// </summary>
        /// <param name="value">The byte value.</param>
        /// <param name="pos">The position to check.</param>
        /// <returns>A boolean : true if bit is set, false otherwise</returns>
		public static Boolean IsBitSet(Byte value, Byte pos) => (value & (1 << pos)) != 0;

        /// <summary>
        /// Determines whether a bit is set at a given position in an Int16.
        /// </summary>
        /// <param name="value">The byte value.</param>
        /// <param name="pos">The position to check.</param>
        /// <returns>A boolean : true if bit is set, false otherwise</returns>
		public static Boolean IsBitSet(Int16 value, Byte pos) => (value & (1 << pos)) != 0;

        /// <summary>
        /// Sets or unsets a specified bit.
        /// </summary>
        /// <param name="value">The byte in which a bit will be set/unset</param>
        /// <param name="index">The index of the bit.</param>
        /// <param name="state">if set to <c>true</c> then bit will be 1, else it will be 0.</param>
        public static void Set(ref Byte value, Byte index, Boolean state)
        {
            var mask = (Byte)(1 << index);

            if (state) { value |= mask; }
            else { value &= (Byte)~mask; }
        }

        /// <summary>
        /// Sets or unsets a specified bit.
        /// </summary>
        /// <param name="value">The byte in which a bit will be set/unset</param>
        /// <param name="index">The index of the bit.</param>
        /// <param name="state">if set to <c>true</c> then bit will be 1, else it will be 0.</param>
        public static Byte Set(Byte value, Byte index, Boolean state)
        {
            var mask = (Byte)(1 << index);

            if (state) { value |= mask; }
            else { value &= (Byte)~mask; }

            return value;
        }

        /// <summary>
        /// Toggles a specified bit.
        /// </summary>
        /// <param name="value">The byte in which a bit will be toggled.</param>
        /// <param name="index">The index of the bit.</param>
        public static void Toggle(ref Byte value, Byte index) => value ^= (Byte)(1 << index);

        /// <summary>
        /// Sets a byte's value using a binary string mask.
        /// </summary>
        /// <param name="value">The byte that should be set.</param>
        /// <param name="mask">The bit mask, like "x11x0110". 'x' means "ignore".</param>
        public static void Set(ref Byte value, String mask)
        {
            var valTmp = value;
            for (var i = mask.Length - 1; i >= 0; i--)
            {
                if (mask[i] != 'x') { Set(ref valTmp, (Byte)(7 - i), mask[i] == '1'); }
            }
            value = valTmp;
        }

        /// <summary>
        /// Sets a byte's value using a binary string mask.
        /// </summary>
        /// <param name="value">The byte that should be set.</param>
        /// <param name="mask">The bit mask, like "x11x0110". 'x' means "ignore".</param>
        public static Byte Set(Byte value, String mask)
        {
            var valTmp = value;
            for (var i = mask.Length - 1; i >= 0; i--)
            {
                if (mask[i] != 'x') { Set(ref valTmp, (Byte)(7 - i), mask[i] == '1'); }
            }
            return valTmp;
        }

        /// <summary>
        /// Sets a specified number of bits in a Byte, according to a specified value and a binary mask.
        /// <para>Example : SetRegister(Registers.BW_RATE, 0x20, (Byte)value);</para>
        /// </summary>
        /// <param name="originalValue">The original byte value.</param>
        /// <param name="mask">The mask.</param>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        public static Byte Set(Byte originalValue, Byte mask, Byte value) => (Byte)((originalValue & ~mask) | (value & mask));

        /// <summary>
        /// Sets a specified number of bits in a Byte, according to a specified value and a string mask. Bits marked "1" in the String mask will be replaced by value.
        /// <para>Example : SetRegister(Registers.BW_RATE, "00001111", (Byte)value);</para>
        /// </summary>
        /// <param name="originalValue">The original byte value.</param>
        /// <param name="mask">The mask.</param>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        public static Byte Set(Byte originalValue, String mask, Byte value)
        {
            var i = mask.Length - 1;
            var binMask = ParseBinary(mask);
            while (mask[i] != '1') { i--; }
            return (Byte)((originalValue & ~binMask) | ((value << (7 - i)) & binMask));
        }

        /// <summary>
        /// Parses a number given in a binary format string.
        /// </summary>
        /// <param name="input">The input string, representing bits positions, e.g. "01110011".</param>
        /// <returns>The Int32 representation of the binary number.</returns>
        public static Int32 ParseBinary(String input)
        {
            // Thanks to Jon Skeet for this one
            var output = 0;
            for (var i = 0; i < input.Length; i++)
            {
                if (input[i] == '1') { output |= 1 << (input.Length - i - 1); }
            }
            return output;
        }
    }
    #endregion

    #region DeviceInitialisationException
    /// <summary>
    /// Exception thrown when a new instance of a driver can't be created. It may be because of too short delays or bad commands sent to the device.
    /// </summary>
    [Serializable]
    public class DeviceInitialisationException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DeviceInitialisationException"/> class.
        /// </summary>
        public DeviceInitialisationException() { }
        /// <summary>
        /// Initializes a new instance of the <see cref="DeviceInitialisationException"/> class.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        public DeviceInitialisationException(String message) : base(message) { }
        /// <summary>
        /// Initializes a new instance of the <see cref="DeviceInitialisationException"/> class.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        /// <param name="innerException">The exception that is the cause of the current exception, or a null reference (Nothing in Visual Basic) if no inner exception is specified.</param>
        public DeviceInitialisationException(String message, Exception innerException) : base(message, innerException) { }

        /// <summary>
        /// Gets the <see cref="T:System.Exception" /> instance that caused the current exception.
        /// </summary>
        /// <returns>An instance of Exception that describes the error that caused the current exception. The InnerException property returns the same value as was passed into the constructor, or a null reference (Nothing in Visual Basic) if the inner exception value was not supplied to the constructor. This property is read-only.</returns>
        public new Exception InnerException { get { return base.InnerException; } }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        /// <PermissionSet><IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" PathDiscovery="*AllFiles*" /></PermissionSet>
        public override String ToString() => "DeviceInitialisationException : " + base.Message;
    }
    #endregion

    #region String extension
    /// <summary>
    /// Extensions methods for Strings
    /// </summary>
    public static class MBNStrings
    {
        /// <summary>
        /// Returns a new string that right-aligns the characters in this instance by padding them on the left with a specified Unicode character, for a specified total length.
        /// </summary>
        /// <param name="source">The underlying String object. Omit this parameter when you call that method.</param>
        /// <param name="totalWidth">The number of characters in the resulting string, equal to the number of original characters plus any additional padding characters.</param>
        /// <param name="paddingChar">A Unicode padding character.</param>
        /// <returns>A new string that is equivalent to this instance, but right-aligned and padded on the left with as many paddingChar characters as needed to create a length of totalWidth.
        ///  However, if totalWidth is less than the length of this instance, the method returns a reference to the existing instance.
        ///  If totalWidth is equal to the length of this instance, the method returns a new string that is identical to this instance.</returns>
        /// <example>Example usage
        /// <code language="C#">
        /// String str = "12";
        /// 
        /// Debug.Print("Padded left : " + str.PadLeft(5, '0')); // Should display "Padded left : 00012"
        /// </code>
        /// </example>
        public static String PadLeft(this String source, Int32 totalWidth, Char paddingChar)
        {
            if (source.Length >= totalWidth) return source;
            do
            {
                source = paddingChar + source;
            }
            while (source.Length < totalWidth);

            return source;
        }

        /// <summary>
        /// Returns a new string that left-aligns the characters in this string by padding them on the right with a specified Unicode character, for a specified total length.
        /// </summary>
        /// <param name="source">The underlying String object. Omit this parameter when you call that method.</param>
        /// <param name="totalWidth">The number of characters in the resulting string, equal to the number of original characters plus any additional padding characters.</param>
        /// <param name="paddingChar">A Unicode padding character.</param>
        /// <returns>A new string that is equivalent to this instance, but left-aligned and padded on the right with as many paddingChar characters as needed to create a length of totalWidth.
        ///  However, if totalWidth is less than the length of this instance, the method returns a reference to the existing instance.
        ///  If totalWidth is equal to the length of this instance, the method returns a new string that is identical to this instance.</returns>
        /// <example>Example usage
        /// <code language="C#">
        /// String str = "12";
        /// 
        /// Debug.Print("Padded right : " + str.PadRight(5, '0')); // Should display "Padded right : 12000"
        /// </code>
        /// </example>
        public static String PadRight(this String source, Int32 totalWidth, Char paddingChar)
        {
            if (source.Length >= totalWidth) return source;
            do
            {
                source += paddingChar;
            }
            while (source.Length < totalWidth);

            return source;
        }

        /// <summary>
        /// Get substring of specified number of characters on the right.
        /// </summary>
        /// <param name="length">A Unicode padding character.</param>
        /// <returns>A new string that is a substring of specified number of characters on the right.
        public static String Right(this String value, Int32 length) => value.Substring(value.Length - length);
    }
    #endregion

    #region Enum extension
    /// <summary>
    ///     An Enumeration extension class providing additional functionality to Microsoft Net Framework (4.3).
    /// </summary>
    public static class EnumExtensions
    {
        /// <summary>
        ///     Extension method to determine is a specific flag is set in the reference Enumeration.
        /// </summary>
        /// <param name="source">The source Enumeration to test against.</param>
        /// <param name="flag">The flag to test for.</param>
        /// <returns>True if the referenced array contains the flag passed in the flag parameter, otherwise false.</returns>
        /// <example>Example usage to determine if there are any Alarms returned in the TemperatureHumidityMeasured Event.
        /// <code language="C#">
        /// static void _sht11Click_TemperatureHumidityMeasured(object sender, TemperatureHumidityEventArgs e)
        /// {
        ///    if (e.Alarms.Contains(Alarms.TemperatureHigh)) Debug.Print("High Temperature Alarm Present");
        /// }
        /// </code>
        /// <code language="VB">
        /// Private Shared Sub _sht11Click_TemperatureHumidityMeasured(sender As Object, e As TemperatureHumidityEventArgs)
        ///     If e.Alarms.Contains(Alarms.TemperatureHigh) Then
        ///          Debug.Print("High Temperature Alarm Present")
        ///     End If
        /// End Sub
        /// </code>
        /// </example>
        public static Boolean ContainsFlag(this Enum source, Enum flag)
        {
            var sourceValue = ToUInt64(source);
            var flagValue = ToUInt64(flag);

            return (sourceValue & flagValue) == flagValue;
        }

        /// <summary>
        ///     Extension method to determine is a any flag passed as a parameter array is set in the reference Enumeration.
        /// </summary>
        /// <param name="source">The source Enumeration to test against.</param>
        /// <param name="flags">The parameter array of flags to test for.</param>
        /// <returns>True if the referenced array contains any one of the flags passed in the parameter array, otherwise false.</returns>
        /// <example>Example usage to determine if there are any Alarms returned in the TemperatureHumidityMeasured Event.
        /// <code language="C#">
        /// static void _sht11Click_TemperatureHumidityMeasured(object sender, TemperatureHumidityEventArgs e)
        /// {
        ///    if (e.Alarms.ContainsAny(Alarms.TemperatureHigh, Alarms.HumidityHigh)) Debug.Print("High Temperature and High Humidity Alarms Present");
        /// }
        /// </code>
        /// <code language="VB">
        /// Private Shared Sub _sht11Click_TemperatureHumidityMeasured(sender As Object, e As TemperatureHumidityEventArgs)
        ///     If e.Alarms.ContainsAny(Alarms.TemperatureHigh, Alarms.HumidityHigh) Then
        ///          Debug.Print("High Temperature and High Humidity Alarms Present")
        ///     End If
        /// End Sub
        /// </code>
        /// </example>
        public static Boolean ContainsAnyFlag(this Enum source, params Enum[] flags)
        {
            var sourceValue = ToUInt64(source);

            foreach (Enum flag in flags)
            {
                var flagValue = ToUInt64(flag);

                if ((sourceValue & flagValue) == flagValue)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        ///     Extension method to determine is a any flag passed as a parameter array is set in the reference Enumeration.
        /// </summary>
        /// <param name="source">The source Enumeration to test against.</param>
        /// <param name="flag">The flag to test for.</param>
        /// <returns>True if the referenced array contains the flag passed in the flag parameter, otherwise false.</returns>
        /// <example>Example usage to determine if there are any Alarms returned in the TemperatureHumidityMeasured Event.
        /// <code language="C#">
        /// static void _sht11Click_TemperatureHumidityMeasured(object sender, TemperatureHumidityEventArgs e)
        /// {
        ///    if (e.Alarms.IsSet(Alarms.NoAlarm)) Debug.Print("No alarms present");
        /// }
        /// </code>
        /// <code language="VB">
        /// Private Shared Sub _sht11Click_TemperatureHumidityMeasured(sender As Object, e As TemperatureHumidityEventArgs)
        ///     If e.Alarms.IsSet(Alarms.NoAlarm) Then
        ///         Debug.Print("No alarms present")
        ///     End If
        /// End Sub
        /// </code>
        /// </example>
        public static Boolean IsSet(this Enum source, Enum flag) => (Convert.ToUInt32(source.ToString()) & Convert.ToUInt32(flag.ToString())) != 0;

        private static UInt64 ToUInt64(Object value) => Convert.ToUInt64(value.ToString());
    }
    #endregion

    #region Bytes manipulation extension
    /// <summary>
    /// An utility class for 2's complements
    /// </summary>
    public static class MBNBytes
    {
        /// <summary>
        /// Gets the 2's complement of a Byte value
        /// </summary>
        /// <param name="value">The byte value to convert.</param>
        /// <returns>An Int32 representing the 2's complement</returns>
        public static Int32 TwoComplement(this Byte value)
        {
            if ((value & 0x80) != 0x80) return value;
            Int32 valtmp = value;
            return -1 * ((Byte)~valtmp) - 1;
        }

        /// <summary>
        /// Gets the Byte's 2's complement of a Integer value
        /// </summary>
        /// <param name="value">The Int32 value to convert.</param>
        /// <returns>An Byte representing the 2's complement</returns>
        public static Byte TwoComplement(this Int32 value) => (value & 0x80) != 0x80 ? (Byte)value : (Byte)(0xFF + value + 1);

        /// <summary>
        /// <para>This Extension method reverses the bytes in a Byte Array.</para>
        /// </summary>
        /// <param name="byteArray">The Byte[] that needs to be reversed.</param>
        /// <returns>
        /// <para>the reverse order of the byte array passed.</para>
        /// </returns>
        public static Byte[] Reverse(this Byte[] byteArray)
        {
            var buffer = new Stack();

            foreach (var b in byteArray)
            {
                buffer.Push(b);
            }

            var reversedBuffer = buffer.ToArray();

            for (Byte i = 0; i < byteArray.Length; i++)
            {
                byteArray[i] = (Byte)reversedBuffer[i];
            }

            return byteArray;
        }

    }
    #endregion

    #region Time extensions
    /// <summary>
    /// Extensions for Time methods
    /// </summary>
    public static class MBNTimeExtensions
    {
        /// <summary>
        /// Convert an 8-byte array from NTP format to .NET DateTime.
        /// </summary>
        /// <param name="ntpTime">NTP format 8-byte array containing date and time</param>
        /// <returns>A Standard .NET DateTime</returns>
        public static DateTime ToDateTime(this Byte[] ntpTime)
        {
            UInt64 intpart = 0;
            UInt64 fractpart = 0;

            for (var i = 0; i <= 3; i++)
                intpart = (intpart << 8) | ntpTime[i];

            for (var i = 4; i <= 7; i++)
                fractpart = (fractpart << 8) | ntpTime[i];

            var milliseconds = intpart * 1000 + (fractpart * 1000) / 0x100000000L;

            var timeSince1900 = TimeSpan.FromTicks((Int64)milliseconds * TimeSpan.TicksPerMillisecond);
            return new DateTime(1900, 1, 1).Add(timeSince1900);
        }
    }
    #endregion

    #region Storage class
    /// <summary>
    /// Storage class
    /// </summary>
    /// <example>
    /// <code language="C#">
    /// public class Program
    /// {
    ///     private static Storage _storage1, _storage2;
    ///
    ///     public static void Main()
    ///     {
    ///        _storage1 = new EEpromClick(Hardware.SocketOne, memorySize: 256);  // Here, the original 8KB chip has been replace by a 256KB one ;)
    ///        _storage2 = new OnboardFlash();
    ///
    ///        Debug.Print("Address 231 before : " + _storage1.ReadByte(231));
    ///        _storage1.WriteByte(231, 123);
    ///        Debug.Print("Address 231 after : " + _storage1.ReadByte(231));
    ///
    ///        _storage2.WriteData(400, new Byte[] { 100, 101, 102 }, 0, 3);
    ///        _storage2.ReadData(400, bArray, 0, 3);
    ///        Debug.Print("Read 3 bytes starting @400 (should be 100, 101, 102) : " + bArray[0] + ", " + bArray[1] + ", " + bArray[2]);
    ///     }
    /// </code>
    /// </example>
    public abstract class Storage
    {
        /// <summary>
        /// Gets the memory capacity.
        /// </summary>
        /// <value>
        /// The maximum capacity, in bytes.
        /// </value>
        public abstract Int32 Capacity { get; }
        /// <summary>
        /// Gets the size of a page in memory.
        /// </summary>
        /// <value>
        /// The size of a page in bytes
        /// </value>
        public abstract Int32 PageSize { get; }
        /// <summary>
        /// Gets the size of a sector.
        /// </summary>
        /// <value>
        /// The size of a sector in bytes.
        /// </value>
        public abstract Int32 SectorSize { get; }
        /// <summary>
        /// Gets the size of a block.
        /// </summary>
        /// <value>
        /// The size of a block in bytes.
        /// </value>
        public abstract Int32 BlockSize { get; }
        /// <summary>
        /// Gets the number of pages per cluster.
        /// </summary>
        /// <value>
        /// The number of pages per cluster.
        /// </value>
        public virtual Int32 PagesPerCluster { get { return 4; } }

        /// <summary>
        /// Completely erases the chip.
        /// </summary>
        /// <remarks>This method is mainly used by Flash memory chips, because of their internal behaviour. It can be safely ignored with other memory types.</remarks>
        /// <example>
        /// <code language="C#">
        /// public class Program
        /// {
        ///     private static Storage _storage;
        ///
        ///     public static void Main()
        ///     {
        ///        _storage = new OnboardFlash();
        ///
        ///        _storage.EraseChip();
        ///     }
        /// </code>
        /// </example>
        public abstract void EraseChip();

        /// <summary>
        /// Erases "count" sectors starting at "sector".
        /// </summary>
        /// <param name="sector">The starting sector.</param>
        /// <param name="count">The count of sectors to erase.</param>
        /// <example>
        /// <code language="C#">
        /// public class Program
        /// {
        ///     private static Storage _storage;
        ///
        ///     public static void Main()
        ///     {
        ///        _storage = new OnboardFlash();
        ///
        ///        _storage.EraseSector(10,1);
        ///     }
        /// </code>
        /// </example>
        public abstract void EraseSector(Int32 sector, Int32 count);

        /// <summary>
        /// Erases "count" blocks starting at "sector".
        /// </summary>
        /// <param name="block">The starting block.</param>
        /// <param name="count">The count of blocks to erase.</param>
        /// <example>
        /// <code language="C#">
        /// public class Program
        /// {
        ///     private static Storage _storage;
        ///
        ///     public static void Main()
        ///     {
        ///        _storage = new OnboardFlash();
        ///
        ///        _storage.EraseBlock(10,2);
        ///     }
        /// </code>
        /// </example>
        public abstract void EraseBlock(Int32 block, Int32 count);

        /// <summary>
        /// Writes data to a memory location.
        /// </summary>
        /// <param name="address">The address to write to.</param>
        /// <param name="data">The data to write.</param>
        /// <param name="index">The starting index in the data array.</param>
        /// <param name="count">The count of bytes to write to memory.</param>
        /// <example>
        /// <code language="C#">
        /// public class Program
        /// {
        ///     private static Storage _storage;
        ///
        ///     public static void Main()
        ///     {
        ///        _storage = new EEpromClick(Hardware.Socket2);
        ///
        ///        _storage.WriteData(400, new Byte[] { 100, 101, 102 }, 0, 3);
        ///        _storage.ReadData(400, bArray, 0, 3);
        ///        Debug.Print("Read 3 bytes starting @400 (should be 100, 101, 102) : " + bArray[0] + ", " + bArray[1] + ", " + bArray[2]);
        ///     }
        /// </code>
        /// </example>
        public abstract void WriteData(Int32 address, Byte[] data, Int32 index, Int32 count);
        /// <summary>
        /// Reads data at a specific address.
        /// </summary>
        /// <param name="address">The address to read from.</param>
        /// <param name="data">An array of bytes containing data read back</param>
        /// <param name="index">The starting index to read in the array.</param>
        /// <param name="count">The count of bytes to read.</param>
        /// <example>
        /// <code language="C#">
        /// public class Program
        /// {
        ///     private static Storage _storage;
        ///
        ///     public static void Main()
        ///     {
        ///        _storage = new FlashClick(Hardware.Socket1);
        ///
        ///        _storage.WriteData(400, new Byte[] { 100, 101, 102 }, 0, 3);
        ///        _storage.ReadData(400, bArray, 0, 3);
        ///        Debug.Print("Read 3 bytes starting @400 (should be 100, 101, 102) : " + bArray[0] + ", " + bArray[1] + ", " + bArray[2]);
        ///     }
        /// </code>
        /// </example>
        public abstract void ReadData(Int32 address, Byte[] data, Int32 index, Int32 count);

        /// <summary>
        /// Reads a single byte at a specified address.
        /// </summary>
        /// <param name="address">The address to read from.</param>
        /// <returns>A byte value</returns>
        /// <example>
        /// <code language="C#">
        /// public class Program
        /// {
        ///     private static Storage _Flash;
        ///
        ///     public static void Main()
        ///     {
        ///        _Flash = new OnboardFlash();
        ///
        ///        _Flash.WriteByte(10, 200);
        ///        Debug.Print("Read byte @10 (should be 200) : " + _Flash.ReadByte(10));
        ///        _Flash.WriteByte(200, 201);
        ///        Debug.Print("Read byte @200 (should be 201) : " + _Flash.ReadByte(200));
        ///     }
        /// </code>
        /// </example>
        public Byte ReadByte(Int32 address)
        {
            var tmp = new Byte[1];
            ReadData(address, tmp, 0, 1);
            return tmp[0];
        }

        /// <summary>
        /// Writes a single byte at a specified address.
        /// </summary>
        /// <param name="address">The address to write to.</param>
        /// <param name="value">The value to write at the specified address.</param>
        /// <example>
        /// <code language="C#">
        /// public class Program
        /// {
        ///     private static Storage _Flash;
        ///
        ///     public static void Main()
        ///     {
        ///        _Flash = new OnboardFlash();
        ///
        ///        _Flash.WriteByte(10, 200);
        ///        Debug.Print("Read byte @10 (should be 200) : " + _Flash.ReadByte(10));
        ///        _Flash.WriteByte(200, 201);
        ///        Debug.Print("Read byte @200 (should be 201) : " + _Flash.ReadByte(200));
        ///     }
        /// </code>
        /// </example>
        public void WriteByte(Int32 address, Byte value) => WriteData(address, new[] { value }, 0, 1);
    }
    #endregion
}
