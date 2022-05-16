/*
 * BarGraph Click board driver for TinyCLR 2.0
 * 
 *  Version 1.0 :
 *  - Initial revision
 * 
 * Copyright 2020 MikroBus.Net
 * Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License. You may obtain a copy of the License at
 * http://www.apache.org/licenses/LICENSE-2.0
 * Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, 
 * either express or implied. See the License for the specific language governing permissions and limitations under the License.
 */

#if (NANOFRAMEWORK_1_0)
using Windows.Devices.Pwm;
using System.Device.Spi;
#else
using GHIElectronics.TinyCLR.Devices.Gpio;
using GHIElectronics.TinyCLR.Devices.Pwm;
using GHIElectronics.TinyCLR.Devices.Spi;
#endif

using System;

namespace MBN.Modules
{
    /// <summary>
    /// Main class for the MikroE BarGraph Click board driver
    /// <para><b>Pins used :</b> Miso, Mosi, Cs, Sck, Pwm, Int</para>
    /// </summary>
    /// <example> This sample shows a basic usage of the BarGraph module.
    /// <code language="C#">
    /// using System;
    /// using System.Threading;
    /// using MBN;
    /// using MBN.Modules;
    ///
    /// namespace Example
    /// {
    ///     public class Program
    ///     {
    ///         static BarGraph _bar;
    ///         
    ///         public static void Main()
    ///         {
    ///             // BarGraph Click board is plugged on socket #2 of the MikroBus.Net mainboard
    ///             _bar = new BarGraphClick(Hardware.SocketTwo);
    ///
    ///             _bar.Brightness = 0.2;
    ///             for (int j = 0; j &lt; 3; j++)
    ///             {
    ///                 for (UInt16 i = 0; i &lt;= 10; i++)
    ///                 {
    ///                     _bar.Bars(i, Fill);
    ///                     Thread.Sleep(50);
    ///                 }
    ///                 for (UInt16 i = 0; i &lt;= 10; i++)
    ///                 {
    ///                     _bar.Bars((UInt16)(10 - i), Fill);
    ///                     Thread.Sleep(50);
    ///                 }
    ///             }
    ///             Thread.Sleep(Timeout.Infinite);
    ///         }
    ///     }
    /// }
    /// </code>
    /// </example>
    public sealed class BarGraphClick
    {
        private readonly SpiDevice _bargraph;                // SPI configuration
        private Double _pwmLevel;                            // Brightness level
#if (NANOFRAMEWORK_1_0)
        private readonly PwmPin _pwm;                    // Brightness control
#else
        private readonly PwmChannel _pwm;                    // Brightness control
#endif
        private readonly Byte[] _data = new byte[2];
        private UInt16 _value;
        private readonly Hardware.Socket _socket;

        /// <summary>
        /// Initializes a new instance of the <see cref="BarGraphClick"/> class.
        /// </summary>
        /// <param name="socket">The socket on which the BarGraph Click board is plugged on MikroBus.Net</param>
        /// <param name="initialBrightness">Initial brightness in the range 0.0 (no display) to 1.0 (full brightness)</param>
        public BarGraphClick(Hardware.Socket socket, Double initialBrightness = 1.0)
        {
            _socket = socket;
            // Initialize SPI
#if (NANOFRAMEWORK_1_0)
            _bargraph = SpiDevice.Create(new SpiConnectionSettings(socket.SpiBus, socket.Cs)
            {
                Mode = SpiMode.Mode0,
                ClockFrequency = 2000000
            });
            // Initialize PWM and set initial brightness
            var PWM = PwmController.FromId(socket.PwmController);
            PWM.SetDesiredFrequency(5000);
            _pwm = PWM.OpenPin(socket.PwmChannel);
#else
            _bargraph = SpiController.FromName(socket.SpiBus).GetDevice(new SpiConnectionSettings()
            {
                ChipSelectType = SpiChipSelectType.Gpio,
                ChipSelectLine = GpioController.GetDefault().OpenPin(socket.Cs),
                Mode = SpiMode.Mode0,
                ClockFrequency = 2000000
            });
            // Initialize PWM and set initial brightness
            var PWM = PwmController.FromName(socket.PwmController);
            PWM.SetDesiredFrequency(5000);
            _pwm = PWM.OpenChannel(socket.PwmChannel);
#endif
            _pwm.SetActiveDutyCyclePercentage(initialBrightness);
            _pwm.Start();
        }

        /// <summary>
        /// Gets or sets the brightness of the bargraph.
        /// </summary>
        /// <value>
        /// The brightness level, in the range 0.0 to 1.0
        /// </value>
        /// <example> This sample shows how to set the Brightness property.
        /// <code language="C#">
        ///             // BarGraph Click board is plugged on socket #2 of the MikroBus.Net mainboard
        ///             _bar = new BarGraphClick(Hardware.SocketTwo);
        ///
        ///             _bar.Brightness = 0.2;
        ///             _bar.Bars(5);
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
        /// Displays the specified number of bars.
        /// </summary>
        /// <param name="nbBars">The number of bars to display (0 to 10).</param>
        /// <param name="fill">Boolean : true = fills preceding leds (default), false = light on the single led</param>
        /// <example> This sample shows how to call the Bars() method.
        /// <code language="C#">
        ///             // BarGraph Click board is plugged on socket #2 of the MikroBus.Net mainboard, with initial brightness set to half
        ///             _bar = new BarGraphClick(Hardware.SocketTwo { Brightness=0.5 });
        ///
        ///             _bar.Bars(5);           // Light on 5 bars
        /// 
        ///             Thread.Sleep(2000);     // Wait 2 sec
        /// 
        ///             _bar.Bars(6,false);     // Light on the 6th bar only
        /// </code>
        /// </example>
        public void Bars(UInt16 nbBars, Boolean fill = true)
        {
            _value = (UInt16)((1 << (fill ? nbBars : nbBars - 1)) - (fill ? 1 : 0));
            _data[0] = (Byte)((_value >> 8) & 0xFF);
            _data[1] = (Byte)(_value & 0xFF);
            lock (_socket.LockSpi)
            {
                _bargraph.Write(_data);
            }
        }

        /// <summary>
        /// Sends a bit mask to the chip.
        /// </summary>
        /// <param name="bars">10 bits mask to send.</param>
        /// <example> This sample shows how to call the SendMask() method.
        /// <code language="C#">
        ///         static BarGraph _bar;
        ///         static UInt16[] _fillMasks;
        ///         
        ///         public static void Main()
        ///         {
        ///             _fillMasks = new UInt16[10] { 512, 768, 896, 960, 992, 1008, 1016, 1020, 1022, 1023 };
        ///             // BarGraph Click board is plugged on socket #2 of the MikroBus.Net mainboard, with initial brightness set to half
        ///             _bar = new BarGraphClick(Hardware.SocketTwo { Brightness=0.5 });
        ///
        ///             for (int j = 0; j &lt; 3; j++)
        ///             {
        ///                 for (UInt16 i = 0; i &lt; 10; i++)
        ///                 {
        ///                     Leds.SendMask(_fillMasks[i]);
        ///                     Thread.Sleep(50);
        ///                 }
        ///                 for (UInt16 i = 0; i &lt; 10; i++)
        ///                 {
        ///                     Leds.SendMask(_fillMasks[9 - i]);
        ///                     Thread.Sleep(50);
        ///                 }
        ///             }
        ///         }
        /// </code>
        /// </example>
        public void WriteMask(UInt16 bars)
        {
            _data[0] = (Byte)((bars >> 8) & 0xFF);
            _data[1] = (Byte)(bars & 0xFF);
            lock (_socket.LockSpi)
            {
                _bargraph.Write(_data);
            }
        }
    }
}
