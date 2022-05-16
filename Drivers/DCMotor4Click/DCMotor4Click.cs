/*
 * DC Motor 4 Click board driver TinyCLR 2.0
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
using System.Device.Gpio;
using Windows.Devices.Pwm;
#else
using GHIElectronics.TinyCLR.Devices.Gpio;
using GHIElectronics.TinyCLR.Devices.Pwm;
#endif

using System;
using System.Threading;

namespace MBN.Modules
{
    /// <summary>Main class for the DCMotor 4 Click driver</summary>
    public sealed class DCMotor4Click
    {
        private readonly GpioPin _enable;
        private readonly GpioPin _direction;
        private Boolean _motorEnabled;
#if (NANOFRAMEWORK_1_0)
        private readonly PwmPin _pwmOut;
#else
        private readonly PwmChannel _pwmOut;
#endif
        private Int32 _rampIncrement, _rampWaitTime;

        /// <summary>
        /// Available directions for the motor
        /// </summary>
        public enum Directions
        {
            Forward,
            Backward
        };

        /// <summary>Initializes a new instance of the <see cref="DCMotor4Click" /> class.</summary>
        /// <param name="socket">The socket on which the DCMotor 4 Click board is plugged</param>
        /// <param name="frequency">PWM frequency. Depends on the motor.</param>
        public DCMotor4Click(Hardware.Socket socket, Double frequency = 1000.0)
        {
#if (NANOFRAMEWORK_1_0)
            _enable = new GpioController().OpenPin(socket.Cs, PinMode.Output);
            _enable.Write(PinValue.High);

            _direction = new GpioController().OpenPin(socket.AnPin, PinMode.Output);
            _direction.Write(PinValue.High);

            var controller = PwmController.FromId(socket.PwmController);
            _pwmOut = controller.OpenPin(socket.PwmChannel);
#else
            _enable = GpioController.GetDefault().OpenPin(socket.Cs);
            _enable.SetDriveMode(GpioPinDriveMode.Output);
            _enable.Write(GpioPinValue.High);

            _direction = GpioController.GetDefault().OpenPin(socket.AnPin);
            _direction.SetDriveMode(GpioPinDriveMode.Output);
            _direction.Write(GpioPinValue.High);

            var controller = PwmController.FromName(socket.PwmController);
            _pwmOut = controller.OpenChannel(socket.PwmChannel);
#endif
            controller.SetDesiredFrequency(frequency);

            // Motor not running at startup
            Stop();
            IsMoving = false;
        }

        /// <summary>
        /// Gets the status of the motor
        /// </summary>
        /// <value>
        /// true if the motor is rotating, false otherwise
        /// </value>
        public Boolean IsMoving
        {
            get; private set;
        }

        /// <summary>
        /// Enables or disables the rotation
        /// </summary>
        /// <value>
        /// The current status
        /// </value>
        public Boolean Enabled
        {
            get => _motorEnabled;
            set
            {
#if (NANOFRAMEWORK_1_0)
                _enable.Write(value ? PinValue.Low : PinValue.High);
#else
                _enable.Write(value ? GpioPinValue.Low : GpioPinValue.High);
#endif
                _motorEnabled = !value;
            }
        }

        /// <summary>
        /// Moves the motor in the specified direction.
        /// </summary>
        /// <param name="direction">The direction : forward or backward.</param>
        /// <param name="speed">The speed, from 0.0 to 1.0 (100%)</param>
        /// <param name="rampTime">The ramp time if needed, in milliseconds. It's the time that will be taken to start from speed 0.0 to "Speed".</param>
        public void Move(Directions direction, Double speed = 1.0, Int32 rampTime = 0)
        {
            if (IsMoving)
            {
                _pwmOut.Stop();
                Thread.Sleep(200);
            }
#if (NANOFRAMEWORK_1_0)
            _direction.Write(direction == Directions.Backward ? PinValue.Low : PinValue.High);
            _enable.Write(PinValue.Low);
#else
            _direction.Write(direction == Directions.Backward ? GpioPinValue.Low : GpioPinValue.High);
            _enable.Write(GpioPinValue.Low);
#endif
            if (rampTime == 0)
            {
                _pwmOut.SetActiveDutyCyclePercentage(speed);
                IsMoving = true;
                _pwmOut.Start();
            }
            else
            {
                _rampIncrement = (Int32)(speed / 0.05);
                _rampWaitTime = (Int32)(rampTime * 0.05 / speed);
                IsMoving = true;
                new Thread(RampUp).Start();
            }
        }

        /// <summary>
        /// Stops the motor.
        /// </summary>
        /// <param name="rampTime">The ramp time if needed, in milliseconds. It's the time that will be taken to decrease speed from "actual speed" to 0.0.</param>
        public void Stop(Int32 rampTime = 0)
        {
            if (!IsMoving)
            {
                return;
            }
            if (rampTime == 0)
            {
                _pwmOut.Stop();
                IsMoving = false;
#if (NANOFRAMEWORK_1_0)
                _enable.Write(PinValue.High);
#else
                _enable.Write(GpioPinValue.High);
#endif
            }
            else
            {
                _rampWaitTime = (Int32)(rampTime * 0.05 / _pwmOut.GetActiveDutyCyclePercentage());

                new Thread(RampDown).Start();
            }
            _pwmOut.SetActiveDutyCyclePercentage(0.0);
        }

#region Private methods
        private void RampUp()
        {
            var i = 0;
            _pwmOut.Start();
            var duty = _pwmOut.GetActiveDutyCyclePercentage();
            while (i < _rampIncrement)
            {
                duty += 0.05;
                if (Math.Max(duty, 1.0) == duty)
                    duty = 1.0;
                _pwmOut.SetActiveDutyCyclePercentage(duty);
                Thread.Sleep(_rampWaitTime);
                i++;
            }
            IsMoving = true;
        }

        private void RampDown()
        {
            var duty = _pwmOut.GetActiveDutyCyclePercentage();
            while (duty > 0)
            {
                duty -= 0.05;
                if (Math.Min(duty, 0.0) == duty)
                    duty = 0.0;
                _pwmOut.SetActiveDutyCyclePercentage(duty);
                Thread.Sleep(_rampWaitTime);
            }
            _pwmOut.Stop();
            IsMoving = false;
#if (NANOFRAMEWORK_1_0)
            _enable.Write(PinValue.High);
#else
            _enable.Write(GpioPinValue.High);
#endif
        }
#endregion
    }
}

