/*
 * Stepper Click board driver for TinyCLR 2.0
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
using System.Device.Gpio;
#else
using GHIElectronics.TinyCLR.Devices.Gpio;
#endif

using System;
using System.Threading;

namespace MBN.Modules
{
    /// <summary>
    /// Main class for the MikroE Stepper Click board driver
    /// <para><b>Pins used :</b> Miso, Mosi, Cs, Sck, Pwm, Int, Rst, An</para>
    /// </summary>
    /// <example> This sample shows a basic usage of the Stepper module.
    /// <code language="C#">
    /// using MBN;
    /// using System.Threading;
    ///
    /// namespace StepperClickUWP
    /// {
    ///    public class Program
    ///    {
    ///         private static StepperClick _stepper;
    ///
    ///         static void Main()
    ///         {
    ///            _stepper = new StepperClick(Hardware.SocketTwo);
    ///
    ///            for (var i = 0; i &lt; 400; i++)
    ///            {
    ///                _stepper.Step();
    ///                Thread.Sleep(3);
    ///            }
    ///            _stepper.Direction = StepperClick.Directions.CounterClockwise;
    ///            for (var i = 0; i &lt; 400; i++)
    ///            {
    ///                _stepper.Step();
    ///                Thread.Sleep(3);
    ///            }
    ///             _stepper.StepSize = StepperClick.StepSizes.Half;
    ///             for (var i = 0; i &lt; 400; i++)
    ///             {
    ///                _stepper.Step();
    ///                Thread.Sleep(3);
    ///             }
    ///             _stepper.Direction = StepperClick.Directions.Clockwise;
    ///             for (var i = 0; i &lt; 400; i++)
    ///             {
    ///                _stepper.Step();
    ///                Thread.Sleep(3);
    ///             }
    ///             _stepper.Enabled = false;
    ///
    ///            Thread.Sleep(Timeout.Infinite);
    ///         }
    ///     }
    ///}
    /// </code>
    /// </example>
    public sealed class StepperClick
    {
        #region Enums
        /// <summary>
        /// List of allowed step sizes
        /// </summary>
        public enum StepSizes
        {
            /// <summary>
            /// 1/8 Step
            /// </summary>
            Eighth,
            /// <summary>
            /// 1/4 Step
            /// </summary>
            Fourth,
            /// <summary>
            /// Half Step
            /// </summary>
            Half,
            /// <summary>
            /// Full Step
            /// </summary>
            Full
        };

        /// <summary>
        /// Directions of rotation
        /// </summary>
        public enum Directions
        {
            /// <summary>
            /// Clockwise
            /// </summary>
            Clockwise,
            /// <summary>
            /// CounterClockwise
            /// </summary>
            CounterClockwise
        };
        #endregion

        #region Privates variables
        private readonly GpioPin _ms1, _ms2, _step, _dir, _enable;
        private StepSizes _stepSize;
        private Boolean _enabled = true;
        private Directions _direction = Directions.Clockwise;
        #endregion

        #region Constructor
        /// <summary>
        /// Initializes a new instance of the <see cref="StepperClick"/> class.
        /// </summary>
        /// <param name="socket">The socket on which the Stepper Click board is plugged on MikroBus.Net</param>
        public StepperClick(Hardware.Socket socket)
        {
#if (NANOFRAMEWORK_1_0)
            var gpio = new GpioController();
            _ms1 = gpio.OpenPin(socket.AnPin);
            _ms2 = gpio.OpenPin(socket.Rst);
            _dir = gpio.OpenPin(socket.Cs);
            _enable = gpio.OpenPin(socket.Int);
            _step = gpio.OpenPin(socket.PwmPin);

            _ms1.SetPinMode(PinMode.Output);
            _ms2.SetPinMode(PinMode.Output);
            _dir.SetPinMode(PinMode.Output);
            _enable.SetPinMode(PinMode.Output);
            _step.SetPinMode(PinMode.Output);

            // Default step size to full step
            _ms1.Write(PinValue.Low);
            _ms2.Write(PinValue.Low);

            //Enable outputs
            _enable.Write(PinValue.Low);

            // Default direction is Clockwise
            _dir.Write(PinValue.Low);

            // Ensure no step at startup
            _step.Write(PinValue.Low);
#else
            _ms1 = GpioController.GetDefault().OpenPin(socket.AnPin);
            _ms2 = GpioController.GetDefault().OpenPin(socket.Rst);
            _dir = GpioController.GetDefault().OpenPin(socket.Cs);
            _enable = GpioController.GetDefault().OpenPin(socket.Int);
            _step = GpioController.GetDefault().OpenPin(socket.PwmPin);

            _ms1.SetDriveMode(GpioPinDriveMode.Output);
            _ms2.SetDriveMode(GpioPinDriveMode.Output);
            _dir.SetDriveMode(GpioPinDriveMode.Output);
            _enable.SetDriveMode(GpioPinDriveMode.Output);
            _step.SetDriveMode(GpioPinDriveMode.Output);
            
            // Default step size to full step
            _ms1.Write(GpioPinValue.Low);
            _ms2.Write(GpioPinValue.Low);

            //Enable outputs
            _enable.Write(GpioPinValue.Low);

            // Default direction is Clockwise
            _dir.Write(GpioPinValue.Low);

            // Ensure no step at startup
            _step.Write(GpioPinValue.Low);
#endif
        }
#endregion

#region Public methods
        /// <summary>
        /// Move the motor by one step.
        /// </summary>
        /// <example> This sample shows how to call the Step() method.
        /// <code language="C#">
        /// // Stepper Click board is plugged on socket #2 of the MikroBus.Net mainboard
        /// var _stepper = new StepperClick(Hardware.SocketTwo);
        ///
        /// _stepper.Step();
        /// </code>
        /// </example>
        public void Step()
        {
#if (NANOFRAMEWORK_1_0)
            _step.Write(PinValue.High);
            Thread.Sleep(1);
            _step.Write(PinValue.Low);
#else
            _step.Write(GpioPinValue.High);
            Thread.Sleep(1);
            _step.Write(GpioPinValue.Low);
#endif
            Thread.Sleep(1);
        }

        /// <summary>
        /// Get or set the states of the outputs
        /// </summary>
        /// <value>
        /// True to enable outputs, false otherwise.
        /// </value>
        /// <example> This sample shows how to enable outputs.
        /// <code language="C#">
        /// // Stepper Click board is plugged on socket #2 of the MikroBus.Net mainboard
        /// var _stepper = new StepperClick(Hardware.SocketTwo);
        ///
        /// _stepper.Enabled = true;
        /// </code>
        /// </example>
        public Boolean Enabled
        {
            get { return _enabled; }
            set
            {
#if (NANOFRAMEWORK_1_0)
                _enable.Write(value ? PinValue.Low : PinValue.High);
#else
                _enable.Write(value ? GpioPinValue.Low : GpioPinValue.High);
#endif
                _enabled = value;
            }
        }

        /// <summary>
        /// Get or set the direction of the rotation
        /// </summary>
        /// <value>
        /// Directions.Clockwise or Directions.CounterClockwise
        /// </value>
        /// <example> This sample shows how to change the direction
        /// <code language="C#">
        /// // Stepper Click board is plugged on socket #2 of the MikroBus.Net mainboard
        /// var _stepper = new StepperClick(Hardware.SocketTwo);
        ///
        /// _stepper.Direction = StepperClick.Directions.Clockwise;
        /// </code>
        /// </example>
        public Directions Direction
        {
            get { return _direction; }
            set
            {
#if (NANOFRAMEWORK_1_0)
                _dir.Write(value == Directions.CounterClockwise ? PinValue.High : PinValue.Low);
#else
                _dir.Write(value == Directions.CounterClockwise ? GpioPinValue.High : GpioPinValue.Low);
#endif
                _direction = value;
            }
        }

        /// <summary>
        /// Get or set the size of one step
        /// </summary>
        /// <value>
        /// Full, half, fourth or Eighth
        /// </value>
        /// <example> This sample shows how to set the step size
        /// <code language="C#">
        /// // Stepper Click board is plugged on socket #2 of the MikroBus.Net mainboard
        /// var _stepper = new StepperClick(Hardware.SocketTwo);
        ///
        /// _stepper.StepSize = StepperClick.StepSizes.Half;
        /// </code>
        /// </example>
        public StepSizes StepSize
        {
            get { return _stepSize; }
            set
            {
                switch (value)
                {
#if (NANOFRAMEWORK_1_0)
                    case StepSizes.Full:
                        _ms1.Write(PinValue.Low);
                        _ms2.Write(PinValue.Low);
                        _stepSize = StepSizes.Full;
                        break;
                    case StepSizes.Half:
                        _ms1.Write(PinValue.High);
                        _ms2.Write(PinValue.Low);
                        _stepSize = StepSizes.Half;
                        break;
                    case StepSizes.Fourth:
                        _ms1.Write(PinValue.Low);
                        _ms2.Write(PinValue.High);
                        _stepSize = StepSizes.Fourth;
                        break;
                    case StepSizes.Eighth:
                        _ms1.Write(PinValue.High);
                        _ms2.Write(PinValue.High);
                        _stepSize = StepSizes.Eighth;
                        break;
#else
                    case StepSizes.Full:
                        _ms1.Write(GpioPinValue.Low);
                        _ms2.Write(GpioPinValue.Low);
                        _stepSize = StepSizes.Full;
                        break;
                    case StepSizes.Half:
                        _ms1.Write(GpioPinValue.High);
                        _ms2.Write(GpioPinValue.Low);
                        _stepSize = StepSizes.Half;
                        break;
                    case StepSizes.Fourth:
                        _ms1.Write(GpioPinValue.Low);
                        _ms2.Write(GpioPinValue.High);
                        _stepSize = StepSizes.Fourth;
                        break;
                    case StepSizes.Eighth:
                        _ms1.Write(GpioPinValue.High);
                        _ms2.Write(GpioPinValue.High);
                        _stepSize = StepSizes.Eighth;
                        break;
#endif
                }
            }
        }
#endregion
    }
}
