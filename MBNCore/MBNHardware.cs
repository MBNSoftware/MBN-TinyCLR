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

#if (NANOFRAMEWORK_1_0)
using System.Device.Gpio;
using nanoFramework.Stm32.Pins;
#else
using GHIElectronics.TinyCLR.Devices.Gpio;
using GHIElectronics.TinyCLR.Pins;
#endif

using System;
using System.Collections;

namespace MBN
{

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
            /// SPI bus lock object
            /// </summary>
            public Object LockSpi;

            /// <summary>
            /// I2C Bus lock object
            /// </summary>
            public Object LockI2c;

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
            I2cBus = SC20100.I2cBus.I2c1,
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
            LockI2c = new Object(),
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
            LockSpi = new Object(),
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
            SocketOne.LockI2c = new Object();
            SocketTwo.LockI2c = SocketOne.LockI2c;
            SocketThree.LockI2c = SocketOne.LockI2c;
            SocketFour.LockI2c = new Object();
            SocketFive.LockI2c = SocketFour.LockI2c;
            SocketSix.LockI2c = SocketFour.LockI2c;

            SocketOne.LockSpi = new Object();
            SocketTwo.LockSpi = SocketOne.LockSpi;
            SocketThree.LockSpi = SocketOne.LockSpi;
            SocketFour.LockSpi = new Object();
            SocketFive.LockSpi = SocketFour.LockSpi;
            SocketSix.LockSpi = SocketFour.LockSpi;
            SpiHeader.LockSpi = SocketFour.LockSpi;

            SC20100_1.LockSpi = new Object();
            SC20100_2.LockSpi = SC20100_1.LockSpi;
            SC20100_1.LockI2c = new Object();
            SC20100_2.LockI2c = SC20100_1.LockI2c;

            SC20260_1.LockSpi = new Object();
            SC20260_2.LockSpi = SC20260_1.LockSpi;
            SC20260_1.LockI2c = new Object();
            SC20260_2.LockI2c = SC20260_1.LockI2c;

            FEZStick_1.LockSpi = new Object();
            FEZStick_2.LockSpi = new Object();
            FEZStick_1.LockI2c = new Object();
            FEZStick_2.LockI2c = new Object();

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

}
