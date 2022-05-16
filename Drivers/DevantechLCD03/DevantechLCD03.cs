/*
 * Devantech LCD03 driver for TinyCLR 2.0
 * 
 *  Version 1.0 :
 *  - Initial revision
 * 
 * Copyright 2018 MikroBus.Net
 * Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License. You may obtain a copy of the License at
 * http://www.apache.org/licenses/LICENSE-2.0
 * Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, 
 * either express or implied. See the License for the specific language governing permissions and limitations under the License.
 */

#if (NANOFRAMEWORK_1_0)
using Windows.Devices.SerialCommunication;
using Windows.Storage.Streams;
using System.Device.I2c;
#else
using GHIElectronics.TinyCLR.Devices.Uart;
using GHIElectronics.TinyCLR.Devices.I2c;
#endif

using System;

namespace MBN.Modules
{
    /// <summary>
    /// Main class for the Devantech LCD03 driver
    /// <para><b>Pins used :</b> Scl, Sda in I²C mode. Tx, Rx in UART mode.</para>
    /// </summary>
    /// <example>
    /// <code language="C#">
    /// public class Program
    /// {
    ///     private static DevantechLcd03 _lcd;
    ///
    ///     public static void Main()
    ///     {
    ///         _lcd = new DevantechLcd03(Hardware.SocketOne, 0xC8 >> 1)
    ///         {
    ///             BackLight = true,
    ///             Cursor = DevantechLcd03.Cursors.Hide
    ///         };
    ///         _lcd.ClearScreen();
    ///         _lcd.Write(1, 1, "Hello world !");
    ///         _lcd.Write(1, 3, "Version " + _lcd.DriverVersion);
    ///     }
    /// }
    /// </code>
    /// </example>
    public sealed class DevantechLcd03
    {
        /// <summary>
        /// List of cursor types available on the LCD
        /// </summary>
        public enum Cursors
        {
            /// <summary>
            /// Hides the cursor
            /// </summary>
            Hide,
            /// <summary>
            /// Cursor is a steady underline
            /// </summary>
            Underline,
            /// <summary>
            /// Cursor is a blinking block
            /// </summary>
            Blink
        };

#if (NANOFRAMEWORK_1_0)
        private readonly SerialDevice _lcdSerial;
#else
        private readonly UartController _lcdSerial;
#endif
        private readonly I2cDevice _lcdI2C;
        private Cursors _cursor;
        private Boolean _backLight;
        private readonly Boolean _isUart;
        private readonly Hardware.Socket _socket;

        /// <summary>
        /// Initializes a new instance of the <see cref="DevantechLcd03"/> class using serial communication (UART)
        /// </summary>
        /// <param name="socket">The socket on MBN board where the Lcd is connected</param>
        public DevantechLcd03(Hardware.Socket socket)
        {
            try
            {
#if (NANOFRAMEWORK_1_0)
                _lcdSerial = SerialDevice.FromId(socket.ComPort);
                // set parameters
                _lcdSerial.BaudRate = 9600;
                _lcdSerial.DataBits = 8;
                _lcdSerial.Parity = SerialParity.None;
                _lcdSerial.StopBits = SerialStopBitCount.Two;
                _lcdSerial.Handshake = SerialHandshake.None;
#else
                _lcdSerial = UartController.FromName(socket.ComPort);
                _lcdSerial.SetActiveSettings(new UartSetting() { BaudRate = 9600, DataBits = 8, Parity = UartParity.None, StopBits = UartStopBitCount.Two, Handshaking = UartHandshake.None });
                _lcdSerial.Enable();
#endif

                _isUart = true;
                Init();
            }
            catch { }
        }
                
        /// <summary>
        /// Initializes a new instance of the <see cref="DevantechLcd03"/> class using I²C communication
        /// </summary>
        /// <param name="socket">The socket on MBN board where the Lcd is connected</param>
        /// <param name="address">I²C address (7 bits) of the LCD. </param>
        public DevantechLcd03(Hardware.Socket socket, Int32 address)
        {
            _socket = socket;
            try
            {
#if (NANOFRAMEWORK_1_0)
                _lcdI2C = I2cDevice.Create(new I2cConnectionSettings(socket.I2cBus, address, I2cBusSpeed.StandardMode));
#else
                _lcdI2C = I2cController.FromName(socket.I2cBus).GetDevice(new I2cConnectionSettings(address, 100000));
#endif
                _isUart = false;
                Init();
            }
            catch { }
        }

        private void Init()
        {
            _backLight = false;
            _cursor = Cursors.Blink;
        }

		/// <summary>
        /// Gets or sets the cursor shape.
        /// </summary>
        /// <value>
        /// The actual cursor's shape
        /// </value>
        /// <example>
        /// <code language="C#">
        ///     _lcd.Cursor = DevantechLcd03.Cursors.Hide;
        /// </code>
        /// </example>
        public Cursors Cursor
        {
            get { return _cursor; }
            set
            {
#if (NANOFRAMEWORK_1_0)
                if (_isUart)
                {
                    using (DataWriter outputDataWriter = new DataWriter(_lcdSerial.OutputStream))
                    {
                        outputDataWriter.WriteByte((byte)(4 + value));
                        outputDataWriter.Store();
                    }
                }
#else
            if (_isUart) _lcdSerial.Write(new[] { (Byte)(4 + value) });
#endif
                else
                {
                    lock (_socket.LockI2c)
                    {
                        _lcdI2C.Write(new[] { (Byte)0, (Byte)(4 + value) });
                    }
                }
                _cursor = value;
            }
        }

		/// <summary>
        /// Gets or sets a value indicating whether back light is turned on or off.
        /// </summary>
        /// <value>
        ///   <c>true</c> if back light is on, otherwise <c>false</c>.
        /// </value>
        /// <example>
        /// <code language="C#">
        ///     _lcd.BackLight = true;
        /// </code>
        /// </example>
        public Boolean BackLight
        {
            get { return _backLight; }
            set
            {
#if (NANOFRAMEWORK_1_0)
                if (_isUart)
                {
                    using (DataWriter outputDataWriter = new DataWriter(_lcdSerial.OutputStream))
                    {
                        var bytesToWrite = new[] { value ? (byte)19 : (byte)20 };
                        outputDataWriter.WriteBytes(bytesToWrite);
                        outputDataWriter.Store();
                    }
                }
#else
                if (_isUart) _lcdSerial.Write(new[] { value ? (Byte)19 : (Byte)20 });
#endif
                else
                {
                    lock (_socket.LockI2c)
                    {
                        _lcdI2C.Write(new[] { (Byte)0, value ? (Byte)19 : (Byte)20 });
                    }
                }
                _backLight = value;
            }
        }

		/// <summary>
        /// Sets the cursor at specified coordinates
        /// </summary>
        /// <param name="x">The column (1 to 20)</param>
        /// <param name="y">The line (1 to 4)</param>
        /// <example>
        /// <code language="C#">
        ///     // Sets the cursor position on column 10 of line 2
        ///     _lcd.Cursor(10,2);
        /// </code>
        /// </example>
        public void SetCursor(Byte x, Byte y)
        {
            if (x <= 0 || x > 20 || y <= 0 || y > 4) { return; }

#if (NANOFRAMEWORK_1_0)
            if (_isUart)
            {
                using (DataWriter outputDataWriter = new DataWriter(_lcdSerial.OutputStream))
                {
                    var bytesToWrite = new byte[] { 3, y, x };
                    outputDataWriter.WriteBytes(bytesToWrite);
                    outputDataWriter.Store();
                }
            }
#else
            if (_isUart) _lcdSerial.Write(new Byte[] { 3, y, x });
#endif
            else
            {
                lock (_socket.LockI2c)
                {
                    _lcdI2C.Write(new Byte[] { 0, 3, y, x });
                }
            }
        }
		
		/// <summary>
        /// Writes the specified text at the current cursor's position
        /// </summary>
        /// <param name="text">The text to display</param>
        /// <example>
        /// <code language="C#">
        ///     // Write the "Hello world !" text at the current cursor's position
        ///     _lcd.Write("Hello world !");
        /// </code>
        /// </example>
        public void Write(String text)
        {
#if (NANOFRAMEWORK_1_0)
            if (_isUart)
            {
                using (DataWriter outputDataWriter = new DataWriter(_lcdSerial.OutputStream))
                {
                    var bytesToWrite = System.Text.Encoding.UTF8.GetBytes(text);
                    outputDataWriter.WriteBytes(bytesToWrite);
                    outputDataWriter.Store();
                }
            }
#else
            if (_isUart) _lcdSerial.Write(System.Text.Encoding.UTF8.GetBytes(text));
#endif
            else
            {
                lock (_socket.LockI2c)
                {
                    _lcdI2C.Write(System.Text.Encoding.UTF8.GetBytes((Byte)0 + text));
                }
            }
        }

		/// <summary>
        /// Writes the specified text at (x,y) coordinates
        /// </summary>
        /// <param name="x">The column (1 to 20</param>
        /// <param name="y">The line (1 to 4)</param>
        /// <param name="text">The text to display</param>
        /// <example>
        /// <code language="C#">
        ///     // Write the "Hello world !" text on the first row, first column
        ///     _lcd.Write(1,1,"Hello world !");
        /// </code>
        /// </example>
        public void Write(Byte x, Byte y, String text)
        {
            if (x <= 0 || x > 20 || y <= 0 || y > 4) { return; }
            SetCursor(x, y);
            Write(text);
        }

		/// <summary>
        /// Clears the screen.
        /// </summary>
        /// <example>
        /// <code language="C#">
        ///     _lcd.ClearScreen();
        /// </code>
        /// </example>
        public void ClearScreen()
        {
#if (NANOFRAMEWORK_1_0)
            if (_isUart)
            {
                using (DataWriter outputDataWriter = new DataWriter(_lcdSerial.OutputStream))
                {
                    outputDataWriter.WriteByte(12);
                    outputDataWriter.Store();
                }
            }
#else
            if (_isUart) _lcdSerial.Write(new Byte[] { 12 });
#endif
            else
            {
                lock (_socket.LockI2c)
                {
                    _lcdI2C.Write(new Byte[] { 0, 12 });
                }
            }
        }
    }
}
