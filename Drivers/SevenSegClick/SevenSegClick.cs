/*
 * Seven Segment Click driver for TinyCLR 2.0
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
using System.Device.Spi;
using Windows.Devices.Pwm;
#else
using GHIElectronics.TinyCLR.Devices.Gpio;
using GHIElectronics.TinyCLR.Devices.Spi;
using GHIElectronics.TinyCLR.Devices.Pwm;
#endif

using System;

namespace MBN.Modules
{
    /// <summary>
    /// Main class for the 7Seg board driver
    /// <para><b>Pins used :</b> Miso, Mosi, Cs, Sck, Pwm</para>
    /// </summary>
    public sealed class SevenSegClick
    {
        private static SpiDevice _sevenSeg;
#if (NANOFRAMEWORK_1_0)
        private static PwmPin _pwm;
#else
        private static PwmChannel _pwm;
#endif
        private Double _pwmLevel;                            // Brightness level
        private readonly Hardware.Socket _socket;

#region CharTable
        /// <summary>
        /// Contains binary values for digits and chars
        /// </summary>
        public Byte[] CharTable =
        {
            0x80, // '-'
            0x01, // '.'   Bit order :
            0x00, // '/'   (g)(f)(e)(d)(c)(a)(b)(dp)
            0x7E, // '0'
            0x0A, // '1'    _a_
            0xB6, // '2'  f|   |b
            0x9E, // '3'   |_g_|
            0xCA, // '4'  e|   |c
            0xDC, // '5'   |_d_|.dp
            0xFC, // '6'
            0x0E, // '7'
            0xFE, // '8'
            0xDE, // '9'
            0x00, // ':'
            0x00, // ';'
            0x00, // '<'
            0x00, // '='
            0x00, // '>'
            0x00, // '?'
            0x00, // '@'
            0xEE, // 'A'
            0xF8, // 'B'
            0x74, // 'C'
            0xBA, // 'D'
            0xF4, // 'E'
            0xE4, // 'F'
            0x7C, // 'G'
            0xEA, // 'H'
            0x0A, // 'I'
            0x3A, // 'J'
            0x00, // 'K'
            0x70, // 'L'
            0x00, // 'M'
            0x6E, // 'N'
            0x7E, // 'O'
            0xE6, // 'P'
            0xCE, // 'Q'
            0x64, // 'R'
            0xDC, // 'S'
            0xF0, // 'T'
            0x7A, // 'U'
            0x00, // 'V'
            0x00, // 'W'
            0x00, // 'X'
            0xDA, // 'Y'
            0x00, // 'Z'
            0x00, // '['
            0x00, // '/'
            0x00, // ']'
            0x00, // '^'
            0x10  // '_'
        };
#endregion

        /// <summary>
        /// Initializes a new instance of the <see cref="SevenSegClick"/> class.
        /// </summary>
        /// <param name="socket">The socket on which the 7Seg Click board is plugged on the MBN board</param>
        /// <param name="initialBrightness">Initial brightness in the range 0.0 (no display) to 1.0 (full brightness)</param>
        public SevenSegClick(Hardware.Socket socket, Double initialBrightness = 1.0)
        {
            _socket = socket;
#if (NANOFRAMEWORK_1_0)
            _sevenSeg = SpiDevice.Create(new SpiConnectionSettings(socket.SpiBus, socket.Cs)
            {
                Mode = SpiMode.Mode0,
                ClockFrequency = 2000000
            });
#else
            _sevenSeg = SpiController.FromName(socket.SpiBus).GetDevice(new SpiConnectionSettings()
            {
                ChipSelectType = SpiChipSelectType.Gpio,
                ChipSelectLine = GpioController.GetDefault().OpenPin(socket.Cs),
                Mode = SpiMode.Mode0,
                ClockFrequency = 2000000
            });
#endif

            // Sets initial brightness
#if (NANOFRAMEWORK_1_0)
            var PWM = PwmController.GetDefault();
            PWM.SetDesiredFrequency(5000);
            _pwm = PWM.OpenPin(socket.PwmChannel);
#else
            var PWM = PwmController.FromName(socket.PwmController);
            PWM.SetDesiredFrequency(5000);
            _pwm = PWM.OpenChannel(socket.PwmChannel);
#endif
            Brightness = initialBrightness;
        }

        /// <summary>
        /// Gets byte value representing the character in the char table
        /// </summary>
        /// <param name="character">The character.</param>
        /// <returns>A byte to send to the board</returns>
        /// <example> This sample shows how to call the GetChar() method.
        /// <code language="C#">
        /// using System.Threading;
        /// using MBN;
        /// using MBN.Modules;
        ///
        /// namespace Examples
        /// {
        ///     public class Program
        ///     {
        ///         private static SevenSegClick _seven;
        /// 
        ///         public static void Main()
        ///         {
        ///             _seven = new SevenSegClick(Hardware.SocketOne, 0.05);
        /// 
        ///             // Displays all alpha chars on the right display, using the GetChar() method
        ///             for (char i = 'A'; i &lt;= 'Z'; i++)
        ///             {
        ///                 _seven.SendBytes(new byte[] { _seven.GetChar(i), 0x00 });
        ///                 Thread.Sleep(200);
        ///             }
        /// 
        ///             Thread.Sleep(Timeout.Infinite);
        ///         }
        ///     }
        /// }
        /// </code>
        /// </example>
        public Byte GetChar(Char character) => ((character >= '-') && (character <= '_')) ? CharTable[character - '-'] : (Byte)0x00;

        /// <summary>
        /// Gets byte value representing the digit in the char table
        /// </summary>
        /// <param name="digit">The digit.</param>
        /// <returns>A byte to send to the board</returns>
        /// <example> This sample shows how to call the GetDigit() method.
        /// <code language="C#">
        /// using System.Threading;
        /// using MBN;
        /// using MBN.Modules;
        ///
        /// namespace Examples
        /// {
        ///     public class Program
        ///     {
        ///         private static SevenSegClick _seven;
        /// 
        ///         public static void Main()
        ///         {
        ///             _seven = new SevenSegClick(Hardware.SocketOne, 0.05);
        /// 
        ///             // Displays from 0 to 9.9
        ///             // Trick : no float here, only bytes, the dot is added as soon as i > 9
        ///             for (byte i = 0; i &lt; 100; i++)
        ///             {
        ///                 _seven.SendBytes(i &lt; 10
        ///                     ? new byte[] {_seven.GetDigit(i), 0x00}
        ///                     : new [] {_seven.GetDigit((byte) (i%10)), (byte) (_seven.GetDigit((byte) (i/10)) + 1)});
        ///                 Thread.Sleep(75);
        ///             }
        /// 
        ///             Thread.Sleep(Timeout.Infinite);
        ///         }
        ///     }
        /// }
        /// </code>
        /// </example>
        public Byte GetDigit(Byte digit) => digit <= 9 ? CharTable[digit + 3] : (Byte)0x00;

        /// <summary>
        /// Gets or sets the brightness of the bargraph.
        /// </summary>
        /// <value>
        /// The brightness level, in the range 0.0 to 1.0
        /// </value>
        /// <example> This sample shows how to use the Brightness property.
        /// <code language="C#">
        /// using System.Threading;
        /// using MBN;
        /// using MBN.Modules;
        ///
        /// namespace Examples
        /// {
        ///     public class Program
        ///     {
        ///         private static SevenSegClick _seven;
        /// 
        ///         public static void Main()
        ///         {
        ///             _seven = new SevenSegClick(Hardware.SocketOne, 0.05);
        /// 
        ///             // Displays from 0 to 9.9
        ///             // Trick : no float here, only bytes, the dot is added as soon as i > 9
        ///             for (byte i = 0; i &lt; 100; i++)
        ///             {
        ///                 _seven.SendBytes(i &lt; 10
        ///                     ? new byte[] {_seven.GetDigit(i), 0x00}
        ///                     : new [] {_seven.GetDigit((byte) (i%10)), (byte) (_seven.GetDigit((byte) (i/10)) + 1)});
        ///                 Thread.Sleep(75);
        ///             }
        /// 
        ///             _seven.Brightness = 0.25;
        /// 
        ///             // Scrolls from A to Z
        ///             _seven.SendBytes(new byte[] { 0xEE, 0x00 });
        /// 
        ///             for (byte i = 66; i &lt; 91; i++)
        ///             {
        ///                 _seven.SendBytes(new [] { _seven.CharTable[i - 44], _seven.CharTable[i - 45] });
        ///                 Thread.Sleep(200);
        ///             }
        /// 
        ///             Thread.Sleep(Timeout.Infinite);
        ///         }
        ///     }
        /// }
        /// </code>
        /// </example>
        public Double Brightness
        {
            get { return _pwmLevel; }
            set
            {
                _pwmLevel = (value > 1.0) || (value < 0.0) ? 1.0 : value;
                _pwm.SetActiveDutyCyclePercentage(_pwmLevel);
                _pwm.Start();
            }
        }

        /// <summary>
        /// Clears the display, without affecting brightness.
        /// </summary>
        /// <example> This sample shows how to call the Clear() method.
        /// <code language="C#">
        /// using System.Threading;
        /// using MBN;
        /// using MBN.Modules;
        ///
        /// namespace Examples
        /// {
        ///     public class Program
        ///     {
        ///         private static SevenSegClick _seven;
        /// 
        ///         public static void Main()
        ///         {
        ///             _seven = new SevenSegClick(Hardware.SocketOne, 0.05);
        /// 
        ///             // Displays from 0 to 9.9
        ///             // Trick : no float here, only bytes, the dot is added as soon as i > 9
        ///             for (byte i = 0; i &lt; 100; i++)
        ///             {
        ///                 _seven.SendBytes(i &lt; 10
        ///                     ? new byte[] {_seven.GetDigit(i), 0x00}
        ///                     : new [] {_seven.GetDigit((byte) (i%10)), (byte) (_seven.GetDigit((byte) (i/10)) + 1)});
        ///                 Thread.Sleep(75);
        ///             }
        /// 
        ///             _seven.Clear();
        /// 
        ///             // Scrolls from A to Z
        ///             _seven.SendBytes(new byte[] { 0xEE, 0x00 });
        /// 
        ///             for (byte i = 66; i &lt; 91; i++)
        ///             {
        ///                 _seven.SendBytes(new [] { _seven.CharTable[i - 44], _seven.CharTable[i - 45] });
        ///                 Thread.Sleep(200);
        ///             }
        /// 
        ///             Thread.Sleep(Timeout.Infinite);
        ///         }
        ///     }
        /// }
        /// </code>
        /// </example>
        public void Clear()
        {
            lock (_socket.LockSpi)
            {
                _sevenSeg.Write(new Byte[] { 0x00, 0x00 });
            }
        }

        /// <summary>
        /// Sends a 2 bytes array to the 7Seg.
        /// <para>First byte is left display, second byte is right display</para>
        /// </summary>
        /// <param name="tab">The array of bytes.</param>
        /// <example> This sample shows how to call the SendBytes() method.
        /// <code language="C#">
        /// using System.Threading;
        /// using MBN;
        /// using MBN.Modules;
        ///
        /// namespace Examples
        /// {
        ///     public class Program
        ///     {
        ///         private static SevenSegClick _seven;
        /// 
        ///         private static readonly byte[] Spin1 = 
        ///         {
        ///             0x00, 0x04, 
        ///             0x00, 0x40,
        ///             0x00, 0x20,
        ///             0x00, 0x10,
        ///             0x10, 0x00,
        ///             0x08, 0x00,
        ///             0x02, 0x00,
        ///             0x04, 0x00
        ///         };
        /// 
        ///         public static void Main()
        ///         {
        ///             _seven = new SevenSegClick(Hardware.SocketOne, 0.05);
        /// 
        ///             for (int j = 0; j &lt; 10; j++)
        ///             {
        ///                 for (byte i = 0; i &lt; 16; i += 2)
        ///                 {
        ///                     _seven.SendBytes(new [] { Spin1[i], Spin1[i + 1] });
        ///                     Thread.Sleep(50);
        ///                 }
        ///             }
        ///             Thread.Sleep(Timeout.Infinite);
        ///         }
        ///     }
        /// }
        /// </code>
        /// </example>
        public void SendBytes(Byte[] tab)
        {
            lock (_socket.LockSpi)
            {
                _sevenSeg.Write(tab);
            }
        }
    }
}
