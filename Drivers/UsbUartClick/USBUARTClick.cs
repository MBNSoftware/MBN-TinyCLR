/*
 * USB UART Click Driver for TinyCLR 2.0
 * 
 * Version 1.0 :
 *  - Initial version
 *  
 * Source for SimpleSerial taken from IggMoe's SimpleSerial Class https://www.ghielectronics.com/community/codeshare/entry/644
 *  
 * Copyright 2020 MikroBus.Net
 * Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License. You may obtain a copy of the License at
 * http://www.apache.org/licenses/LICENSE-2.0
 * Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, 
 * either express or implied. See the License for the specific language governing permissions and limitations under the License.
 */

#if (NANOFRAMEWORK_1_0)
using System.Device.Gpio;
using Windows.Devices.SerialCommunication;
using Windows.Storage.Streams;
#else
using GHIElectronics.TinyCLR.Devices.Gpio;
using GHIElectronics.TinyCLR.Devices.Uart;
#endif

using System;
using System.Text;

// ReSharper disable once CheckNamespace
namespace MBN.Modules
{
    /// <summary>
    /// A MikroBusNet driver for the MikroE USB UART Click board.
    /// <para><b>This module is a Generic Device</b></para>
    /// <para><b>Pins used :</b>Tx, Rx, Rst, Int, Pwm and Cs</para>
    /// <para><b>References required:</b> MikroBus.Net, Microsoft.SPOT.Native, Microsoft.Spot.Hardware, mscorlib</para>
    /// </summary>
    /// <remarks>
    /// Required References - Microsoft.SPOT.Hardware,  Microsoft.SPOT.Hardware.SerialPort, Microsoft.SPOT.Native, Microsoft.SPOT.IO, mscorlib
    /// </remarks>
    /// <example>
    /// <code language = "C#">
    /// using MBN;
    /// using System.IO.Ports;
    /// using MBN.Enums;
    /// using Microsoft.SPOT;
    /// using System.Threading;
    ///
    /// namespace USB_UART_TestApp
    /// {
    ///    public class Program
    ///    {
    ///       private static USBUARTClick _usbUart;
    ///
    ///       public static void Main()
    ///       {
    ///         _usbUart= new USBUARTClick(Hardware.SocketFour, BaudRate.Baud9600, Handshake.None) {GlitchFilterTime = new TimeSpan(0, 0, 0, 0, 50)};
    ///
    ///        _usbUart.DataReceived += USBUartUSBUARTDataReceived;
    ///        _usbUart.CableConnectionChanged += USBUartUSBUARTCableConnectionChanged;
    ///        _usbUart.SleepSuspend += USBUARTUartUSBUARTSleepSuspend;
    ///
    ///        Debug.Print("USB is connected ? " + _usbUart.USBCableConnected);
    ///        Debug.Print("USB sleeping ? " + _usbUart.USBSuspended);
    ///
    ///        Thread.Sleep(Timeout.Infinite);
    ///        }
    ///
    ///        static void USBUARTUartUSBUARTSleepSuspend(object sender, bool isSleeping, DateTime eventTime)
    ///        {
    ///           Debug.Print("USB " + (isSleeping ? "Suspend/Sleep" : "Wake") + " event occurred at " + eventTime);
    ///        }
    ///
    ///        static void USBUartUSBUARTCableConnectionChanged(object sender, bool cableConnected, DateTime eventTime)
    ///        {
    ///            Debug.Print("Cable " + (cableConnected ? "connection" : "dis-connection") + " event occurred at " + eventTime);
    ///        }
    ///
    ///        static void USBUartUSBUARTDataReceived(object sender, string message, DateTime eventTime)
    ///        {
    ///            // Echo back to sender
    ///            _usbUart.SendData("Received your message of - \"" + message + "\" at " + eventTime);
    ///            Debug.Print(message);
    ///        }
    ///     }
    /// }
    /// </code>
    /// </example>
    public sealed partial class USBUARTClick
    {
        #region CTOR

        /// <summary>
        ///     Default constructor
        /// </summary>
        /// <param name="socket">The socket in which the USB UART click board is inserted into.</param>
        /// <param name="baudRate">The baud rate for serial commumications.</param>
        /// <param name="dataBits"></param>
        /// <param name="parity"></param>
        /// <param name="stopBitCount"></param>
        /// <param name="handShake"></param>
#if (NANOFRAMEWORK_1_0)
        public USBUARTClick(Hardware.Socket socket, BaudRate baudRate, ushort dataBits = 8, SerialParity parity = SerialParity.None, SerialStopBitCount stopBitCount = SerialStopBitCount.One, SerialHandshake handShake = SerialHandshake.None)
        {
            _serial = SerialDevice.FromId(socket.ComPort);
            // set parameters
            _serial.BaudRate = (uint)baudRate;
            _serial.DataBits = dataBits;
            _serial.Parity = parity;
            _serial.StopBits = stopBitCount;
            _serial.Handshake = handShake;

            _powerPin = new GpioController().OpenPin(socket.PwmPin);
            _powerPin.SetPinMode(PinMode.InputPullUp);
            _powerPin.ValueChanged += PowerPin_ValueChanged;

            _sleepPin = new GpioController().OpenPin(socket.Cs);
            _sleepPin.SetPinMode(PinMode.Input);
            _sleepPin.ValueChanged += SleepPin_ValueChanged;
#else
        public USBUARTClick(Hardware.Socket socket, BaudRate baudRate, Int32 dataBits = 8, UartParity parity = UartParity.None, UartStopBitCount stopBitCount = UartStopBitCount.One, UartHandshake handShake = UartHandshake.None)
        {
            _serial = UartController.FromName(socket.ComPort);
            _serial.SetActiveSettings(new UartSetting() { BaudRate = (Int32)baudRate, DataBits = dataBits, Parity = parity, StopBits = stopBitCount, Handshaking = handShake });
            _serial.Enable();

            _powerPin = GpioController.GetDefault().OpenPin(socket.PwmPin);
            _powerPin.SetDriveMode(GpioPinDriveMode.InputPullUp);
            _powerPin.ValueChanged += PowerPin_ValueChanged;

            _sleepPin = GpioController.GetDefault().OpenPin(socket.Cs);
            _sleepPin.SetDriveMode(GpioPinDriveMode.Input);
            _sleepPin.ValueChanged += SleepPin_ValueChanged;

            _serial.ErrorReceived += Serial_ErrorReceived;
#endif

            _serial.DataReceived += Serial_DataReceived;

            _simpleSerial = new SimpleSerial(_serial);
        }

        #endregion

        #region Fields

#if (NANOFRAMEWORK_1_0)
        private readonly SerialDevice _serial;
#else
        private readonly UartController _serial;
#endif
        private readonly SimpleSerial _simpleSerial;
        private String[] _dataIn;

        private static GpioPin _sleepPin;
        private static GpioPin _powerPin;

#endregion

#region ENUMS

        /// <summary>
        ///     Enumeration for BaudRate selection of the USBUARTClick
        /// </summary>
        public enum BaudRate
        {
            /// <summary>
            ///     BaudRate 2400
            /// </summary>
            Baud2400 = 2400,

            /// <summary>
            ///     BaudRate 4800
            /// </summary>
            Baud4800 = 4800,

            /// <summary>
            ///     BaudRate 9600
            /// </summary>
            Baud9600 = 9600,

            /// <summary>
            ///     BaudRate 14,400
            /// </summary>
            Baud14400 = 14400,

            /// <summary>
            ///     BaudRate 19200
            /// </summary>
            Baud19200 = 19200,

            /// <summary>
            ///     BaudRate 28,800
            /// </summary>
            Baud28800 = 28800,
            /// <summary>
            ///     Default BaudRate 38400
            /// </summary>
            Baud38400 = 38400,

            /// <summary>
            ///     BaudRate 57600
            /// </summary>
            Baud57600 = 57600,

            /// <summary>
            ///     BaudRate 115200
            /// </summary>
            Baud115200 = 115200,

            /// <summary>
            ///     BaudRate 230400
            /// </summary>
            Baud230400 = 230400,

            /// <summary>
            ///     BaudRate 460800
            /// </summary>
            Baud460800 = 460800,

            /// <summary>
            ///     BaudRate 921600
            /// </summary>
            Baud921600 = 921600
        }

#endregion

#region Public Properties

        /// <summary>
        /// Gets the status of the USB Cable connection to USB UART Click.
        /// </summary>
        /// <returns>True is a USB cable is connected to the USB port of the USBUART Click or otherwise false.</returns>
        /// <returns>This property is useful to determine if a cable or PC is attached to the click. For example, you would not want to send data to an attached device (PC) if it is not connected to the click or a buffer overflow might happen.</returns>
        /// <example>
        /// <code language = "C#">
        /// Debug.Print("USB is connected ? " + _usbUart.USBCableConnected);
        /// </code>
        /// </example>
#if (NANOFRAMEWORK_1_0)
        public Boolean USBCableConnected => _powerPin.Read() == PinValue.Low;
#else
        public Boolean USBCableConnected => _powerPin.Read() == GpioPinValue.Low;
#endif

        /// <summary>
        /// Gets the Sleep/Suspend status of the USB connection to the USBUART Click.
        /// </summary>
        /// <returns>True is a USB connection is in the Sleep/Suspend mode or otherwise false.</returns>
        /// <returns>This property is useful to determine if the USB connection is Sleeping or Suspended. For example, you would not want to send data to an attached device (PC) if it is sleeping or suspended or a buffer overflow might happen.</returns>
        /// <example>
        /// <code language = "C#">
        /// Debug.Print("USB sleeping ? " + _usbUart.USBSuspended);
        /// </code>
        /// </example>
#if (NANOFRAMEWORK_1_0)
        public Boolean USBSuspended => _sleepPin.Read() == PinValue.Low;
#else
        public Boolean USBSuspended => _sleepPin.Read() == GpioPinValue.Low;
#endif
#endregion

#region Public Methods

        /// <summary>
        /// Send a <see cref="System.String"/> of data to the USB client connected to the USBUART click.
        /// </summary>
        /// <param name="data">String data to send.</param>
        /// <example>
        /// <code language = "C#">
        /// _usbUart.SendData("Received your message - " + message);
        /// </code>
        /// <code language = "VB">
        /// _usbUart.SendData("Received your message - " <![CDATA[&]]> message)
        /// </code>
        /// </example>
        public void SendData(string data)
        {
#if (NANOFRAMEWORK_1_0)
            DataWriter outputDataWriter = new DataWriter(_serial.OutputStream);
            var bytesToWrite = Encoding.UTF8.GetBytes(data);
            outputDataWriter.WriteBytes(bytesToWrite);
            outputDataWriter.Store();        
#else
            _serial.Write(Encoding.UTF8.GetBytes(data));
            _serial.Flush();
#endif
        }

#endregion

#region Events
#if (NANOFRAMEWORK_1_0)
        private void SleepPin_ValueChanged(object sender, PinValueChangedEventArgs e) => UsbSleep(e.ChangeType == PinEventTypes.Falling, DateTime.UtcNow);

        private void PowerPin_ValueChanged(object sender, PinValueChangedEventArgs e) => IscableConnectionChanged(e.ChangeType == PinEventTypes.Falling, DateTime.UtcNow);
#else
        private void SleepPin_ValueChanged(GpioPin sender, GpioPinValueChangedEventArgs e) => UsbSleep(e.Edge == GpioPinEdge.FallingEdge, DateTime.UtcNow);

        private void PowerPin_ValueChanged(GpioPin sender, GpioPinValueChangedEventArgs e) => IscableConnectionChanged(e.Edge == GpioPinEdge.FallingEdge, DateTime.UtcNow);
#endif

#if (!NANOFRAMEWORK_1_0)
        private void Serial_ErrorReceived(UartController sender, ErrorReceivedEventArgs e)
        {
            ErrorReceivedHandler handler = ErrorReceived;
            handler?.Invoke(this, e, DateTime.UtcNow);
        }
#endif

#if (NANOFRAMEWORK_1_0)
        private void Serial_DataReceived(object sender, SerialDataReceivedEventArgs e)
#else
        private void Serial_DataReceived(UartController sender, DataReceivedEventArgs e)
#endif
        {
            _dataIn = _simpleSerial.Deserialize();
            foreach (String str in _dataIn)
            {
                DataReceived?.Invoke(this, str, DateTime.UtcNow);
            }
        }

        /// <summary>
        ///  Represents the delegate that is used for the <see cref="DataReceived"/> event.
        /// </summary>
        /// <param name="sender">The USBUART Click that raised the event.</param>
        /// <param name="message">The <see cref="System.String"/> data that was received.</param>
        /// <param name="eventTime">The time that the event occurred.</param>
        public delegate void DataReceivedHandler(Object sender, String message, DateTime eventTime);

        /// <summary>
        /// Raised when the USBUART Click receives data.
        /// </summary>
        public event DataReceivedHandler DataReceived;

        /// <summary>
        ///  Represents the delegate that is used for the <see cref="ErrorReceived"/> event.
        /// </summary>
        /// <param name="sender">The USBUART Click that raised the event.</param>
        /// <param name="e">The <see cref="ErrorReceivedEventArgs"/></param>
        /// <param name="eventTime">The time that the event occurred.</param>
        public delegate void ErrorReceivedHandler(Object sender, ErrorReceivedEventArgs e, DateTime eventTime);

#if (!NANOFRAMEWORK_1_0)
        /// <summary>
        /// Raised when the USBUART Click receives an Error in data transmission.
        /// </summary>
        public event ErrorReceivedHandler ErrorReceived;
#endif

        /// <summary>
        /// Represents the delegate that is used for the <see cref="CableConnectionChanged"/> event.
        /// </summary>
        /// <param name="sender">The USBUART Click that raised the event.</param>
        /// <param name="cableConnected">Cable connection parameter.</param>
        /// <param name="eventTime">The time that the event occurred.</param>
        public delegate void CableConnectionEventHandler(Object sender, Boolean cableConnected, DateTime eventTime);

        /// <summary>
        /// Raised when the USB Cable connection to the USBUART has changed.
        /// </summary>
        public event CableConnectionEventHandler CableConnectionChanged;

        private void IscableConnectionChanged(Boolean cableConnected, DateTime eventTime)
        {
            CableConnectionEventHandler handler = CableConnectionChanged;
            handler?.Invoke(this, cableConnected, eventTime);
        }

        /// <summary>
        /// Represents the delegate that is used for the <see cref="SleepSuspend"/> event.
        /// </summary>
        /// <param name="sender">The USBUART Click that raised the event.</param>
        /// <param name="isSleeping">Sleep or suspend parameter.</param>
        /// <param name="eventTime">The time that the event occurred.</param>
        public delegate void SleepEventHandler(Object sender, Boolean isSleeping, DateTime eventTime);

        /// <summary>
        /// Raised when the USB connection enters Sleep or Suspend mode.
        /// </summary>
        public event SleepEventHandler SleepSuspend;

        private void UsbSleep(Boolean isSleeping, DateTime eventTime)
        {
            SleepEventHandler handler = SleepSuspend;
            handler?.Invoke(this, isSleeping, eventTime);
        }

#endregion
    }
}