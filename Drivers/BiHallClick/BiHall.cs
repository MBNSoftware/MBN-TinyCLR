/*
 * BiHall Click driver for TinyCLR 2.0
 * 
 * Version 1.0 :
 *  - Initial release
 *  
 * Copyright 2020 MikroBus.Net
 * Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License. You may obtain a copy of the License at
 * http:///www.apache.org/licenses/LICENSE-2.0
 * Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, 
 * either express or implied. See the License for the specific language governing permissions and limitations under the License.
 */
#if (NANOFRAMEWORK_1_0)
using System.Device.Gpio;
#else
using GHIElectronics.TinyCLR.Devices.Gpio;
#endif

using System;

namespace MBN.Modules
{
    /// <summary>
    /// a new instance of the <see cref="BiHallClick"/> class.
    /// <para><b>This module is a Generic Device.</b></para>
    /// <para><b>Pins used :</b>Int</para>
    /// </summary>
    /// <example>Example usage:
    /// <code language = "C#">
    /// using System.Threading;
    /// using MBN.Modules;
    /// using MBN;
    /// using Microsoft.SPOT;
    ///
    /// namespace Examples
    /// {
    ///     public class Program
    ///     {
    ///         private static BiHallClick _biHall;
    ///
    ///         public static void Main()
    ///         {
    ///             _biHall = new BiHallClick(Hardware.SocketOne);
    ///             _biHall.SwitchStateChanged += BiHallSwitchStateChanged;
    ///
    ///             Thread.Sleep(Timeout.Infinite);
    ///         }
    ///
    ///         static void BiHallSwitchStateChanged(object sender, bool switchState)
    ///         {
    ///             Debug.WriteLine($"Switch state - {switchState}");
    ///         }
    ///     }
    /// }
    /// </code>
    /// </example>
    public sealed class BiHallClick
    {
        #region CTOR

        /// <summary>
        /// Initializes a new instance of the <see cref="BiHallClick"/> class.
        /// </summary>
        /// <param name="socket">The <see cref="Hardware.Socket"/> that the AltitudeClick is inserted into.</param>
        public BiHallClick(Hardware.Socket socket)
        {
#if (NANOFRAMEWORK_1_0)
            _interrupt = new GpioController().OpenPin(socket.Int, PinMode.InputPullDown);
#else
            _interrupt = GpioController.GetDefault().OpenPin(socket.Int);
            _interrupt.SetDriveMode(GpioPinDriveMode.InputPullDown);
#endif
            _interrupt.ValueChanged += Interrupt_ValueChanged;
        }

        #endregion

        #region Fields

        private readonly GpioPin _interrupt;

        #endregion

        #region Private Methods/Internal Interrupt Routines

#if (NANOFRAMEWORK_1_0)
        private void Interrupt_ValueChanged(object sender, PinValueChangedEventArgs e)
        {
            SwitchStateChangedEventHandler tempEvent = SwitchStateChanged;
            tempEvent?.Invoke(this, e.ChangeType ==  PinEventTypes.Rising);
        }
#else
        private void Interrupt_ValueChanged(GpioPin sender, GpioPinValueChangedEventArgs e)
        {
            SwitchStateChangedEventHandler tempEvent = SwitchStateChanged;
            tempEvent?.Invoke(this, e.Edge == GpioPinEdge.RisingEdge);
        }
#endif

        #endregion

        #region Public Properties

        /// <summary>
        ///     Gets the state of the internal switch.
        /// </summary>
        /// <returns>True if the switch is closed (latched) or otherwise false.</returns>
        /// <example>
        /// <code language="C#">
        /// Debug.WriteLine($"Switch state - {_biHall.SwitchState}");
        /// </code>
        /// </example>
#if (NANOFRAMEWORK_1_0)
        public Boolean SwitchState => _interrupt.Read() == PinValue.High;
#else
        public Boolean SwitchState => _interrupt.Read() == GpioPinValue.High;
#endif

        #endregion

        #region Events

        /// <summary>
        /// This is the delegate method used by the <see cref="SwitchStateChanged"/> Event.
        /// </summary>
        /// <param name="sender">The Bi-Hall click that raised the event.</param>
        /// <param name="switchState">The state of the switch when the event was raised.</param>
        public delegate void SwitchStateChangedEventHandler(Object sender, Boolean switchState);

        /// <summary>
        /// The event that is fired when the BiHall Click opens or closes the internal latched switch in the presence of either a North or South Pole Magnetic Field.
        /// </summary>
        public event SwitchStateChangedEventHandler SwitchStateChanged = delegate { };

        #endregion
    }
}