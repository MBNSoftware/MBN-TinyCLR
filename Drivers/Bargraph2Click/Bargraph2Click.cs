/*
 * BarGraph2 Click board driver for TinyCLR 2.0
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
    public sealed class Bargraph2Click
    {
        private readonly SpiDevice _bargraph;
        private Double _pwmLevel;
#if (NANOFRAMEWORK_1_0)
        private readonly PwmPin _pwm;
#else
        private readonly PwmChannel _pwm;
#endif
        private UInt32 _mask = 0b0000_0000000000_1111111111;
        private readonly Byte[] _data = new byte[3];
        private readonly Hardware.Socket _socket;

        /// <summary>
        /// Initializes a new instance of the <see cref="Bargraph2Click"/> class.
        /// </summary>
        /// <param name="socket">The socket on which the BarGraph2 Click board is plugged on MikroBus.Net</param>
        /// <param name="initialBrightness">Initial brightness in the range 0.0 (no display) to 1.0 (full brightness)</param>
        public Bargraph2Click(Hardware.Socket socket, Double initialBrightness = 1.0)
        {
            _socket = socket;
            // Initialize SPI
#if (NANOFRAMEWORK_1_0)
            _bargraph = SpiDevice.Create(new SpiConnectionSettings(socket.SpiBus, socket.Cs)
            {
                Mode = SpiMode.Mode3,
                ClockFrequency = 2000000
            });
#else
            _bargraph = SpiController.FromName(socket.SpiBus).GetDevice(new SpiConnectionSettings()
            {
                ChipSelectType = SpiChipSelectType.Gpio,
                ChipSelectLine = GpioController.GetDefault().OpenPin(socket.Cs),
                Mode = SpiMode.Mode3,
                ClockFrequency = 2000000
            });
#endif

            // Initialize PWM and set initial brightness
#if (NANOFRAMEWORK_1_0)
            var PWM = PwmController.FromId(socket.PwmController);
            PWM.SetDesiredFrequency(5000);
            _pwm = PWM.OpenPin(socket.PwmPin);
            _pwm.SetActiveDutyCyclePercentage(initialBrightness);
#else
            var PWM = PwmController.FromName(socket.PwmController);
            PWM.SetDesiredFrequency(5000);
            _pwm = PWM.OpenChannel(socket.PwmChannel);
            _pwm.SetActiveDutyCyclePercentage(initialBrightness);
            _pwm.Start();
#endif
        }

        /// <summary>
        /// Gets or sets the brightness of the bargraph.
        /// </summary>
        /// <value>
        /// The brightness level, in the range 0.0 to 1.0
        /// </value>
        /// <example> This sample shows how to set the Brightness property.
        /// <code language="C#">
        ///             // BarGraph2 Click board is plugged on socket #2 of the MikroBus.Net mainboard
        ///             _bar = new BarGraph2(Hardware.SocketTwo);
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

        //public UInt32 Mask { get; set; } = 0b0000_0000000000_1111111111;      // All leds green by default

        /// <summary>
        /// Sets the mask that is applied to the led to display different colors.
        /// </summary>
        /// <param name="mask">A string representing the color for each led. "R" means read, "O" means Orange, "G" and any other letter means Green.</param>
        /// <remarks>
        /// The default mask is "GGGGGGGGGG", which sets all leds to green. The mask is always applied when the Bars() method is called.
        /// </remarks>
        /// <example> This sample shows how to call the SetMask() method.
        /// <code language="C#">
        ///             // BarGraph2 Click board is plugged on socket #2 of the MikroBus.Net mainboard, with initial brightness set to half
        ///             _bar = new BarGraph2(Hardware.SocketTwo { Brightness=0.5 });
        ///
        ///             _bar.SetMask("GGGGGOOORR");
        ///             _bar.Bars(7, true);     // Light on 5 bars, the first 5 being green and the 2 remaining being orange
        /// </code>
        /// </example>
        public void SetMask(String mask)
        {
            _mask = 0;
            for (var i = 0; i < mask.Length; i++)
            {
                if (mask[i] == 'R') _mask |= (UInt32)(1 << (i + 10));
                else if (mask[i] == 'O') _mask |= (UInt32)((1 << i) + (1 << (i + 10)));
                else _mask |= (UInt32)(1 << i);
            }
        }

        /// <summary>
        /// Displays the specified number of bars.
        /// </summary>
        /// <param name="nbBars">The number of bars to display (0 to 10).</param>
        /// <param name="fill">Boolean : true = fills preceding leds, false (default value) = light on the single led</param>
        /// <example> This sample shows how to call the Bars() method.
        /// <code language="C#">
        ///             // BarGraph2 Click board is plugged on socket #2 of the MikroBus.Net mainboard, with initial brightness set to half
        ///             _bar = new BarGraph2(Hardware.SocketTwo { Brightness=0.5 });
        ///
        ///             _bar.Bars(5, true);     // Light on 5 bars
        ///             Thread.Sleep(2000);     // Wait 2 sec
        ///             _bar.Bars(6,false);     // Light on the 6th bar only
        /// </code>
        /// </example>
        public void Bars(Byte nbBars, Boolean fill = false)
        {
            var _bars = (UInt32)((1 << (fill ? nbBars : nbBars - 1)) - (fill ? 1 : 0));
            _bars += _bars << 10;
            Write(_bars & _mask);
        }

        /// <summary>
        /// Sends raw data to the module.
        /// </summary>
        /// <param name="data">An unsigned integer representing the leds and their color</param>
        /// <example> This sample shows how to call the Write() method.
        /// <code language="C#">
        ///             // BarGraph2 Click board is plugged on socket #2 of the MikroBus.Net mainboard, with initial brightness set to half
        ///             _bar = new BarGraph2(Hardware.SocketTwo { Brightness=0.5 });
        ///
        ///             _bar.Write(0b0000_0000000000_1111111111);     // All leds on and green
        ///             _bar.Write(0b0000_1111111111_0000000000);     // First 5 leds green and last 5 leds red
        /// </code>
        /// </example>
        public void Write(UInt32 data)
        {
            _data[0] = (Byte)(data >> 16);
            _data[1] = (Byte)(data >> 8);
            _data[2] = (Byte)data;
            lock (_socket.LockSpi)
            {
                _bargraph.Write(_data);
            }
        }
    }
}
