/*
 * DCMotor Click board driver for TinyCLR 2.0
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
// ReSharper disable once InconsistentNaming
    /// <summary>
    /// Main class for the DCMotor Click board driver
    /// <para><b>Pins used :</b> An, Rst, Cs, Pwm, Int</para>
    /// </summary>
    /// <example>
    /// <code language="C#">
    /// public class Program
    /// {
    ///    static DCMotorClick _motor;
    ///
    ///    public static void Main()
    ///    {
    ///        _motor = new DCMotorClick(Hardware.SocketOne);
    ///
    ///        _motor.OnFault += Motor_OnFault;
    ///
    ///        _motor.Move(DCMotorClick.Directions.Forward, 1.0, 2000);
    ///        Thread.Sleep(5000);
    ///        _motor.Stop(2000);
    ///
    ///        while (_motor.IsMoving) { Thread.Sleep(10); }
    ///
    ///        Debug.Print("Sleep");
    ///        _motor.Sleeping = true;
    ///     }
    /// }
    /// </code>
    /// </example>
    public sealed class DCMotorClick
    {
        /// <summary>
        /// Main event handler for the <see cref="OnFault"/> event.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        public delegate void EventHandler(Object sender, EventArgs e);

        /// <summary>
        /// Occurs when a fault has been detected, mainly overcurrent.
        /// </summary>
        public event EventHandler OnFault = delegate { };               // Fault event raised to user

        /// <summary>
        /// Directions of the motor moves
        /// </summary>
        public enum Directions 
        {
            /// <summary>
            /// Move forward.
            /// </summary>
            Forward,
            /// <summary>
            /// Move backward.
            /// </summary>
            Backward 
        };

        private readonly GpioPin _fault;
        private readonly GpioPin _select1, _select2, _sleep;
#if (NANOFRAMEWORK_1_0)
        private readonly PwmPin _pwmOut;
#else
        private readonly PwmChannel _pwmOut;
#endif
        private PowerModes _powerMode;

        // Internal variables
        private Int32 _rampIncrement, _rampWaitTime;

        /// <summary>
        /// Initializes a new instance of the <see cref="DCMotorClick"/> class.
        /// </summary>
        /// <param name="socket">The socket on which the DCMotor Click board is plugged on MikroBus.Net</param>
        /// <param name="frequency">The frequency of the PWM output. Default value is 1000 Hz</param>
        /// <param name="dutyCycle">The initial duty cycle of PWM. Default to 0.0 %, that is : motor stopped</param>
        public DCMotorClick(Hardware.Socket socket, Double frequency = 1000.0, Double dutyCycle = 0.0)
        {
#if (NANOFRAMEWORK_1_0)
            // Select1/2 : selection of decay modes. Only Fast decay implemented here.
            _select1 = new GpioController().OpenPin(socket.Rst, PinMode.Output);
            _select1.Write(PinValue.Low);

            _select2 = new GpioController().OpenPin(socket.Cs, PinMode.Output);
            _select2.Write(PinValue.Low);

            _sleep = new GpioController().OpenPin(socket.AnPin, PinMode.Output);
            _sleep.Write(PinValue.High);

            _fault = new GpioController().OpenPin(socket.Int, PinMode.Input);

            var PWM = PwmController.FromId(socket.PwmController);
            PWM.SetDesiredFrequency(frequency);
            _pwmOut = PWM.OpenPin(socket.PwmChannel);
#else
            // Select1/2 : selection of decay modes. Only Fast decay implemented here.
            _select1 = GpioController.GetDefault().OpenPin(socket.Rst);
            _select1.SetDriveMode(GpioPinDriveMode.Output);
            _select1.Write(GpioPinValue.Low);

            _select2 = GpioController.GetDefault().OpenPin(socket.Cs);
            _select2.SetDriveMode(GpioPinDriveMode.Output);
            _select2.Write(GpioPinValue.Low);

            _sleep = GpioController.GetDefault().OpenPin(socket.AnPin);
            _sleep.SetDriveMode(GpioPinDriveMode.Output);
            _sleep.Write(GpioPinValue.High);

            _fault = GpioController.GetDefault().OpenPin(socket.Int);
            _fault.SetDriveMode(GpioPinDriveMode.Input);
            var PWM = PwmController.FromName(socket.PwmController);
            PWM.SetDesiredFrequency(frequency);
            _pwmOut = PWM.OpenChannel(socket.PwmChannel);
#endif

            _fault.ValueChanged += Fault_ValueChanged;

            _pwmOut.SetActiveDutyCyclePercentage(dutyCycle);



            IsMoving = false;                                                      // Motor not running
            _powerMode = PowerModes.On;
        }

#if (NANOFRAMEWORK_1_0)
        private void Fault_ValueChanged(object sender, PinValueChangedEventArgs e)
        {
#else
        private void Fault_ValueChanged(GpioPin sender, GpioPinValueChangedEventArgs e)
        {
#endif
            EventHandler tempEvent = OnFault;
            tempEvent(this, null);
        }

        /// <summary>
        /// Gets a value indicating whether the motor is currently moving.
        /// </summary>
        /// <value>
        ///   <c>true</c> if the motor is moving; otherwise, <c>false</c>.
        /// </value>
        /// <example>
        /// <code language="C#">
        ///     while (_motor.IsMoving) { Thread.Sleep(10); }
        /// </code>
        /// </example>

        public Boolean IsMoving { get; private set; }

        /// <summary>
        /// Moves the motor in the specified direction.
        /// </summary>
        /// <param name="direction">The direction : forward or backward.</param>
        /// <param name="speed">The speed, from 0.0 to 1.0 (100%)</param>
        /// <param name="rampTime">The ramp time if needed, in milliseconds. It's the time that will be taken to start from speed 0.0 to "Speed".</param>
        /// <example>
        /// <code language="C#">
        ///     // Moves the motor forward with a ramptime to full speed of 2 sec
        ///     _motor.Move(DCMotorClick.Directions.Forward, 1.0, 2000);
        /// </code>
        /// </example>
        public void Move(Directions direction, Double speed = 1.0, Int32 rampTime = 0)
        {
            if (IsMoving) { _pwmOut.Stop(); Thread.Sleep(200); }
#if (NANOFRAMEWORK_1_0)
            _select2.Write((direction == Directions.Backward) ? PinValue.High : PinValue.Low);
#else
            _select2.Write((direction == Directions.Backward) ? GpioPinValue.High : GpioPinValue.Low);
#endif
            if (rampTime == 0) 
            { 
                _pwmOut.SetActiveDutyCyclePercentage(speed);
                IsMoving = true; 
                _pwmOut.Start(); 
            }
            else
            {
                _rampIncrement = (Int32)(speed/0.05);
                _rampWaitTime = (Int32)(rampTime * 0.05 / speed);
                IsMoving = true;
                new Thread(RampUp).Start();
            }

        }

        /// <summary>
        /// Stops the motor.
        /// </summary>
        /// <param name="rampTime">The ramp time if needed, in milliseconds. It's the time that will be taken to decrease speed from "actual speed" to 0.0.</param>
        /// <example>
        /// <code language="C#">
        ///     // Stops the motor with a rampdown time of 2 sec
        ///     _motor.Stop(2000);
        /// </code>
        /// </example>
        public void Stop(Int32 rampTime=0)
        {
            if (!IsMoving) { return; }
            if (rampTime == 0) { _pwmOut.Stop(); IsMoving = false; }
            else
            {
                _rampWaitTime = (Int32)(rampTime * 0.05 / _pwmOut.GetActiveDutyCyclePercentage());
                new Thread(RampDown).Start();
            }
        }

        #region Private methods
        private void RampUp()
        {
            var i = 0;
            _pwmOut.Start();
            while (i < _rampIncrement)
            {
                var duty = _pwmOut.GetActiveDutyCyclePercentage();
                _pwmOut.SetActiveDutyCyclePercentage((duty + 0.05) > 1.0 ? 1.0 : duty + 0.05);
                Thread.Sleep(_rampWaitTime);
                i++;
            }
        }

        private void RampDown()
        {
            while (_pwmOut.GetActiveDutyCyclePercentage() > 0)
            {
                var duty = _pwmOut.GetActiveDutyCyclePercentage();
                _pwmOut.SetActiveDutyCyclePercentage((duty - 0.05) < 0.0 ? 0.0 : duty - 0.05);
                Thread.Sleep(_rampWaitTime);
            }
            _pwmOut.Stop();
            IsMoving = false;
        }

        #endregion

        /// <summary>
        /// Gets or sets the power mode.
        /// </summary>
        /// <value>
        /// The current power mode of the module.
        /// </value>
        /// <exception cref="System.NotImplementedException">This module does not have a PowerModes.Off feature.</exception>
        /// <example>
        /// <code language="C#">
        ///     _motor.PowerMode = PowerModes.On;
        /// </code>
        /// </example>
        public PowerModes PowerMode
        {
            get { return _powerMode; }
            set
            {
                if (value == PowerModes.Off) { throw new NotImplementedException("PowerModes.Off");}
                _powerMode = value;
#if (NANOFRAMEWORK_1_0)
                _sleep.Write((value == PowerModes.Low) ? PinValue.High : PinValue.Low);
#else
                _sleep.Write((value == PowerModes.Low) ? GpioPinValue.High : GpioPinValue.Low);
#endif
            }
        }
    }
}

