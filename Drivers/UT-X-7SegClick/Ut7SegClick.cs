#if (NANOFRAMEWORK_1_0)
using System.Device.Gpio;
using System.Device.Spi;
#else
using GHIElectronics.TinyCLR.Devices.Gpio;
using GHIElectronics.TinyCLR.Devices.Spi;
#endif

using System;

namespace MBN.Modules
{
    public class Ut7SegClick
    {
        #region DigitTable
        /// <summary>
        /// Contains binary values for digits
        /// </summary>
        public Byte[] DigitTable =
        {
            0b00111111, // '0'
            0b00000110, // '1'   Bit order :
            0b01011011, // '2'   (dp)(g)(f)(e)(d)(c)(b)(a)
            0b01001111, // '3'
            0b01100110, // '4'    _a_
            0b01101101, // '5'  f|   |b
            0b01111100, // '6'   |_g_|
            0b00000111, // '7'  e|   |c
            0b01111111, // '8'   |_d_|.dp
            0b01100111, // '9'
        };
        #endregion

        private readonly SpiDevice _seg;
        private readonly Hardware.Socket _socket;
        private readonly GpioPin _en;
        private readonly Byte[] _data = new Byte[2];

        /// <summary>
        /// Initializes a new instance of the <see cref="Uts7SegClick"/> class.
        /// </summary>
        /// <param name="socket">The socket on which the module is plugged.</param>
        /// <param name="initialState">Enable (default) or disable the module upon initialization.</param>
        public Ut7SegClick(Hardware.Socket socket, Boolean initialState = true)
        {
            _socket = socket;
            // Initialize SPI
#if (NANOFRAMEWORK_1_0)
            _seg = SpiDevice.Create(new SpiConnectionSettings(socket.SpiBus, socket.Cs)
            {
                Mode = SpiMode.Mode3,
                ClockFrequency = 2000000
            });

            _en = new GpioController().OpenPin(socket.PwmPin);
            _en.SetPinMode(PinMode.Output);
            _en.Write(initialState ? PinValue.High : PinValue.Low);
#else
            _seg = SpiController.FromName(socket.SpiBus).GetDevice(new SpiConnectionSettings()
            {
                ChipSelectType = SpiChipSelectType.Gpio,
                ChipSelectLine = GpioController.GetDefault().OpenPin(socket.Cs),
                Mode = SpiMode.Mode3,
                ClockFrequency = 2000000
            });

            _en = GpioController.GetDefault().OpenPin(socket.PwmPin);
            _en.SetDriveMode(GpioPinDriveMode.Output);
            _en.Write(initialState ? GpioPinValue.High : GpioPinValue.Low);
#endif
        }

        /// <summary>
        /// Sets a value indicating whether this <see cref="Uts7SegClick"/> is enabled.
        /// </summary>
        /// <value>
        ///   <c>true</c> if enabled; otherwise, <c>false</c>.
        /// </value>
        public Boolean Enabled
        {
            set
            {
#if (NANOFRAMEWORK_1_0)
                _en.Write(value ? PinValue.High : PinValue.Low);
#else
                _en.Write(value ? GpioPinValue.High : GpioPinValue.Low);
#endif
            }
        }

        /// <summary>
        /// Gets byte value representing the digit in the digit table
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
        ///         private static Uts7SegClick _seven;
        /// 
        ///         public static void Main()
        ///         {
        ///             _seven = new Uts7SegClick(Hardware.SocketOne, 0.05);
        /// 
        ///             // Displays from 0 to 9.9
        ///             // Trick : no float here, only bytes, the dot is added as soon as i > 9
        ///             for (byte i = 0; i &lt; 100; i++)
        ///             {
        ///                 _seven.Write(i &lt; 10
        ///                     ? new byte[] {_seven.GetDigit(i), 0x00}
        ///                     : new [] {_seven.GetDigit((byte) (i%10)), (byte) (_seven.GetDigit((byte) (i/10)) | 0b10000000)});
        ///                 Thread.Sleep(75);
        ///             }
        /// 
        ///             Thread.Sleep(Timeout.Infinite);
        ///         }
        ///     }
        /// }
        /// </code>
        /// </example>
        public Byte GetDigit(Byte digit) => DigitTable[digit];

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
        ///         private static Uts7SegClick _seven;
        /// 
        ///         public static void Main()
        ///         {
        ///             _seven = new Uts7SegClick(Hardware.SocketOne, 0.05);
        /// 
        ///                 _seven.Write(new Byte[] { _seven.GetDigit(i), 0x00 }
        ///             }
        ///             Thread.Sleep(2000);
        /// 
        ///             _seven.Clear();
        /// 
        /// </code>
        /// </example>
        public void Clear()
        {
            _data[0] = 0x00;
            _data[1] = 0x00;
            lock (_socket.LockSpi)
            {
                _seg.Write(_data);
            }
        }

        /// <summary>
        /// Sends a Uint16 (2 bytes) to the 7Seg.
        /// <para>16bit value with upper 8bit being left digit and lower 8bits being right digit</para>
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
        ///         private static Uts7SegClick _seven;
        /// 
        ///         public static void Main()
        ///         {
        ///             _seven = new Uts7SegClick(Hardware.SocketOne, 0.05);
        /// 
        ///             _seven.Write(0b10011011_00111011);
        ///             
        ///             Thread.Sleep(Timeout.Infinite);
        ///         }
        ///     }
        /// }
        /// </code>
        /// </example>
        public void Write(UInt16 data)
        {
            _data[0] = (Byte)(data >> 8);
            _data[1] = (Byte)(data);
            lock (_socket.LockSpi)
            {
                _seg.Write(_data);
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
        ///         private static Uts7SegClick _seven;
        /// 
        ///         public static void Main()
        ///         {
        ///             _seven = new Uts7SegClick(Hardware.SocketOne, 0.05);
        /// 
        ///             _seven.Write(new [] { 0x23, 0x55 });
        ///             
        ///             Thread.Sleep(Timeout.Infinite);
        ///         }
        ///     }
        /// }
        /// </code>
        /// </example>
        public void Write(Byte[] data)
        {
            lock (_socket.LockSpi)
            {
                _seg.Write(data);
            }
        }
    }
}
