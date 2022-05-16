/*
 * HT16K33 driver for TinyCLR 2.0
 * 
 * Version 1.0 :
 *  - Initial revision
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

namespace MBN.Modules
{
    // ReSharper disable once InconsistentNaming
    /// <summary>
    /// Main class for the HT16K33 driver
    /// <para><b>Pins used :</b> Scl, Sda</para>
    /// </summary>
    /// <example>
    /// <code language="C#">
    /// 	public class Program
    ///     {
    ///         private static HT16K33 _disp;
    ///
    ///         public static void Main()
    ///         {
    ///             _disp = new HT16K33(Hardware.SocketTwo) { Brightness = 1 };
    ///
    ///             for (var i = 0.0; i &lt; 20.0; i += 0.01)
    ///             {
    ///                 _disp.Write(i.ToString("F2"));
    ///                 Thread.Sleep(5);
    ///             }
    ///             Thread.Sleep(1000);
    ///
    ///		        for (int i = 0; i &lt; 2000; i++)
    ///             {
    ///                 _disp.Write(i, i > 999);
    ///                 Thread.Sleep(10);
    ///             }
    /// 
    ///             _disp.Blink(HT16K33.BlinkModes.Blink2Hz);
    ///             Thread.Sleep(6000);
    ///		
    ///		        _disp.Display(false);
    ///        
    ///             Thread.Sleep(Timeout.Infinite);
    ///         }
    ///     }
    /// </code>
    /// </example>
    public sealed class HT16K33
    {
        #region Digits
        /// <summary>
        /// Byte values to send to the HT16K33 for displaying digits
        /// </summary>
        public Byte[] Digits = 
        {
            0x3F, // '0'
            0x06, // '1'
            0x5B, // '2'
            0x4F, // '3'
            0x66, // '4'
            0x6D, // '5'
            0x7D, // '6'
            0x07, // '7'
            0x7F, // '8'
            0x6F  // '9'
        };
        /// <summary>
        /// Byte value for the minus sign
        /// </summary>
        public const Byte Minus = 0x40;
        #endregion

        /// <summary>
        /// 
        /// </summary>
        public enum BlinkModes
        {
            /// <summary>
            /// Disables blinking (steady display).
            /// </summary>
            BlinkOff = 0x81,
            /// <summary>
            /// Blinks at 2 Hz frequency
            /// </summary>
            Blink2Hz = 0x83,
            /// <summary>
            /// Blinks at 1 Hz frequency
            /// </summary>
            Blink1Hz = 0x85,
            /// <summary>
            /// Blinks at 0.5 Hz frequency
            /// </summary>
            Blink05Hz = 0x87
        }

        private readonly I2cDevice _disp;                      // I²C configuration

        private PowerModes _powerMode;
        private Byte _brightness;
        private readonly Byte[] _buffer;
        private readonly Hardware.Socket _socket;

        /// <summary>
        /// Initializes a new instance of the <see cref="HT16K33"/> class.
        /// </summary>
        /// <param name="socket">The socket on which the HT16K33 board is plugged on MikroBus.Net board</param>
        /// <param name="address">The address of the display. Default to 0x70.</param>
        public HT16K33(Hardware.Socket socket, Byte address=0x70)
        {
            _socket = socket;
            // Create the driver's I²C configuration
#if (NANOFRAMEWORK_1_0)
            _disp = I2cDevice.Create(new I2cConnectionSettings(socket.I2cBus, address, I2cBusSpeed.FastMode));
#else
            _disp = I2cController.FromName(socket.I2cBus).GetDevice(new I2cConnectionSettings(address, 400000));
#endif

            _buffer = new Byte[11];
            PowerMode = PowerModes.On;
            ClearDisplay();
            Display(true);
            Brightness = 15;
        }

        /// <summary>
        /// Displays an integer with the optionnal colon.
        /// </summary>
        /// <param name="number">The number to display.</param>
        /// <param name="colon">If set to <c>true</c>, then colon ':' will be displayed.</param>
        /// <example>
        /// <code language="C#">
        ///     // Displays 123
        ///     _disp.Write(123);
        /// 
        ///     // Displays 4567 with colon -> 45:67
        ///     _disp.Write(4567, true);
        /// </code>
        /// </example>
        public void Write(Int32 number, Boolean colon=false)
        {
            if ((number > 9999) || (number < -999)) { return; }
            
            _buffer[5] = colon ? (Byte)0xFF : (Byte)0x00;
            if (number >= 0)
            {
                _buffer[9] = Digits[number % 10];
                if (number > 9) { _buffer[7] = Digits[(number / 10) % 10]; }
                if (number > 99) { _buffer[3] = Digits[(number / 100) % 10]; }
                if (number > 999) { _buffer[1] = Digits[(number / 1000) % 10]; }
            }
            else
            {
                _buffer[7] = Minus; // '-'
                _buffer[9] = Digits[-number % 10];
                if (number < -9) { _buffer[7] = Digits[(-number / 10) % 10]; _buffer[3] = Minus; }
                if (number < -99) { _buffer[3] = Digits[(-number / 100) % 10]; _buffer[1] = Minus; }
            }
            lock (_socket.LockI2c)
            {
                _disp.Write(_buffer);
            }
        }

        /// <summary>
        /// Displays a pre-formatted double.
        /// </summary>
        /// <param name="formattedDouble">The formatted double.</param>
        /// <summary>
        /// Clears the display.
        /// </summary>
        /// <example>
        /// <code language="C#">
        ///     Double i = 1.23;
        ///     _disp.Write(i.ToString("F2"));
        /// </code>
        /// </example>
        public void Write(String formattedDouble)
        {
// ReSharper disable once InconsistentNaming
            var dp = formattedDouble.IndexOf('.');
            var ln = formattedDouble.Length;
            if ( dp > 0)  // There's a decimal point
            {
                if (formattedDouble.Length > 5) { return; }
                switch (ln)
                {
                    case 3:        // 9.9
                        _buffer[9] = Digits[formattedDouble[2]-48];
                        _buffer[7] = (Byte)(Digits[formattedDouble[0]-48] | 0x80);
                        _buffer[3] = 0;
                        _buffer[1] = 0;
                        break;
                    case 4:        // 9.99 or 99.9
                        if (dp == 1)    // 9.99
                        {
                            _buffer[9] = Digits[formattedDouble[3] - 48];
                            _buffer[7] = Digits[formattedDouble[2] - 48];
                            _buffer[3] = (Byte)(Digits[formattedDouble[0] - 48] | 0x80);
                            _buffer[1] = 0;
                        }
                        else    // 99.9
                        {
                            _buffer[9] = Digits[formattedDouble[3] - 48];
                            _buffer[7] = (Byte)(Digits[formattedDouble[1] - 48] | 0x80);
                            _buffer[3] = Digits[formattedDouble[0] - 48];
                            _buffer[1] = 0;
                        }
                        break;
                    case 5:        // 9.999 or 99.99 or 999.9
                        _buffer[9] = Digits[formattedDouble[4] - 48];
                        if (dp == 1)    // 9.999
                        {
                            _buffer[7] = Digits[formattedDouble[3] - 48];
                            _buffer[3] = Digits[formattedDouble[2] - 48];
                            _buffer[1] = (Byte)(Digits[formattedDouble[0] - 48] | 0x80);
                        }
                        if (dp == 2)    // 99.99
                        {
                            _buffer[7] = Digits[formattedDouble[3] - 48];
                            _buffer[3] = (Byte)(Digits[formattedDouble[1] - 48] | 0x80);
                            _buffer[1] = Digits[formattedDouble[0] - 48];
                        }
                        if (dp == 3)    // 999.9
                        {
                            
                            _buffer[7] = (Byte)(Digits[formattedDouble[2] - 48] | 0x80);
                            _buffer[3] = Digits[formattedDouble[1] - 48];
                            _buffer[1] = Digits[formattedDouble[0] - 48];
                        }
                        break;
                }
            }
            else
            {
                if (formattedDouble.Length > 4) { return; }
            }
            lock (_socket.LockI2c)
            {
                _disp.Write(_buffer);
            }
        }

        /// <summary>
        /// Sends an array of bytes to the display.
        /// </summary>
        /// <param name="array">The array containing bytes to send.</param>
        public void SendBytes(Byte[] array)
        {
            lock (_socket.LockI2c)
            {
                _disp.Write(array);
            }
        }

        /// <summary>
        /// Clears the display.
        /// </summary>
        /// <example>
        /// <code language="C#">
        ///     _disp.ClearDisplay();
        /// </code>
        /// </example>
        public void ClearDisplay()
        {
            lock (_socket.LockI2c)
            {
                _disp.Write(new Byte[11]);
            }
        }

        /// <summary>
        /// Sets the blinking frequency of the display
        /// </summary>
        /// <param name="blinkMode">The blink mode. <see cref="BlinkModes"/></param>
        /// <example>
        /// <code language="C#">
        ///     _disp.Blink(HT16K33.BlinkModes.Blink2Hz);
        /// </code>
        /// </example>
        public void Blink(BlinkModes blinkMode)
        {
            lock (_socket.LockI2c)
            {
                _disp.Write(new[] { (Byte)blinkMode });
            }
        }

        /// <summary>
        /// Gets or sets the brightness of the display.
        /// </summary>
        /// <value>
        /// The brightness value, from 0 to 16.
        /// </value>
        /// <example>
        /// <code language="C#">
        ///      _disp.Brightness = 6;
        /// </code>
        /// </example>
        public Byte Brightness
        {
            get { return _brightness; }
            set
            {
                _brightness = value > 16 ? (Byte) 16 : value;
                lock (_socket.LockI2c)
                {
                    _disp.Write(new[] { (Byte)(0xDF + _brightness) });
                }
            }
        }

        /// <summary>
        /// Enables or disables the display.
        /// </summary>
        /// <param name="on">if set to <c>true</c> then display is ON, false otherwise.</param>
        /// <remarks>This is not a real power-saving feature, as it only disables access to screen.</remarks>
        /// <seealso cref="PowerMode"/>
        /// <example>
        /// <code language="C#">
        ///      // Disables display
        ///      _disp.Display(false);
        /// </code>
        /// </example>
        public void Display(Boolean on)
        {
            lock (_socket.LockI2c)
            {
                _disp.Write(new[] { on ? (Byte)0x81 : (Byte)0x80 });
            }
        }

        /// <summary>
        /// Gets or sets the power mode.
        /// </summary>
        /// <value>
        /// The current power mode of the module.
        /// </value>
        /// <exception cref="System.NotImplementedException">Thrown if the property is set to the non-existent PowerModes.Low mode.</exception>
        /// <example>
        /// <code language="C#">
        ///      // Turns off power on the board. Only PowerModes.On will wake it up for normal usage.
        ///      _disp.PowerMode = PowerModes.Off;
        /// </code>
        /// </example>
        public PowerModes PowerMode
        {
            get { return _powerMode; }
            set
            {
                if (value == PowerModes.Low) { throw new NotImplementedException("PowerModes.Low"); }
                lock (_socket.LockI2c)
                {
                    _disp.Write(new[] { value == PowerModes.On ? (Byte)0x21 : (Byte)0x20 });
                }
                _powerMode = value;
                Thread.Sleep(10);
            }
        }
    }
}

