/*
 * Relay Click board driver for TinyCLR 2.0
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
#else
using GHIElectronics.TinyCLR.Devices.Gpio;
#endif

using System;
using System.Threading;

namespace MBN.Modules
{
    /// <summary>
    /// Main class for the MikroE Relay Click board driver
    /// <para><b>Pins used :</b> Pwm, Cs</para>
    /// <para><b>This is a Generic Module</b></para>
    /// </summary>
    /// <example>
    /// <code language="C#">
    /// #region Usings
    ///
    /// using MBN;
    /// using MBN.Modules;
    ///
    /// using System;
    /// using System.Diagnostics;
    /// using System.Threading;
    ///
    /// #endregion
    ///
    /// namespace Examples
    /// {
    ///     internal class Program
    ///     {
    ///         private static void Main()
    ///         {
    ///             // Relay Click board is plugged on socket #1 of the MikroBus.Net mainboard
    ///             // Relay 0 will be OFF and Relay 1 will be ON at startup
    ///             RelayClick _relays = new RelayClick(Hardware.SocketOne, relay1InitialState: true);
    ///
    ///             // Register to the event generated when a relay state has been changed
    ///             _relays.RelayStateChanged += Relays_RelayStateChanged;
    ///
    ///             // Sets relay 0 to ON using the SetRelay() method
    ///             _relays.SetRelay(0, true);
    ///             Thread.Sleep(2000);
    ///
    ///             // Sets relay 0 to OFF using the Relay0 property
    ///             _relays.Relay0 = false;
    ///             Thread.Sleep(2000);
    ///
    ///             // Sets relay 1 to ON using the SetRelay() method
    ///             _relays.SetRelay(1, true);
    ///             Thread.Sleep(2000);
    ///
    ///             // Sets relay 1 to OFF using the Relay0 property
    ///             _relays.Relay1 = false;
    ///             Thread.Sleep(2000);
    ///
    ///             // Gets relay 0 state using the Relay0 property
    ///             Debug.WriteLine("Relay 0 state : " + _relays.Relay0);
    ///
    ///             // Gets relay 1 state using the Relay0 property
    ///             Debug.WriteLine("Relay 1 state : " + _relays.Relay1);
    ///
    ///             // Gets relay 1 state using the GetRelay() method
    ///             Thread.Sleep(Timeout.Infinite);
    ///
    ///             Debug.WriteLine("Relay 1 state : " + _relays.GetRelay(1));
    ///         }
    ///
    ///         private static void Relays_RelayStateChanged(Object sender, RelayStateChangedEventArgs e)
    ///         {
    ///             Debug.WriteLine(e.Relay == 0 ? "Relay 0 state has changed" : "Relay 1 state has changed");
    ///         }
    ///     }
    /// }
    /// </code>
    /// </example>
    public sealed class RelayClick
    {
        #region .ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="RelayClick"/> class.
        /// </summary>
        /// <param name="socket">he socket on which the Relay Click board is plugged on MikroBus.Net</param>
        /// <param name="relay0InitialState">Sets the initial state of relay 0 : <c>true</c> = ON, <c>false</c> = OFF.</param>
        /// <param name="relay1InitialState">Sets the initial state of relay 1 : <c>true</c> = ON, <c>false</c> = OFF.</param>
        public RelayClick(Hardware.Socket socket, Boolean relay0InitialState = false, Boolean relay1InitialState = false)
        {
            // Init the array containing the relays states
            _states = new[] {relay0InitialState, relay1InitialState};

            // Initialize hardware ports with requested initial states
#if (NANOFRAMEWORK_1_0)
            _r0 = new GpioController().OpenPin(socket.PwmPin, PinMode.Output);
            _r1 = new GpioController().OpenPin(socket.Cs, PinMode.Output);
            _r0.Write(relay0InitialState ? PinValue.High : PinValue.Low);
            _r1.Write(relay1InitialState ? PinValue.High : PinValue.Low);
#else
            _r0 = GpioController.GetDefault().OpenPin(socket.PwmPin);
            _r1 = GpioController.GetDefault().OpenPin(socket.Cs);
            _r0.SetDriveMode(GpioPinDriveMode.Output);
            _r1.SetDriveMode(GpioPinDriveMode.Output);
            _r0.Write(relay0InitialState ? GpioPinValue.High : GpioPinValue.Low);
            _r1.Write(relay1InitialState ? GpioPinValue.High : GpioPinValue.Low);
#endif

            _relays = new[] {_r0, _r1};

        }

        #endregion

        #region Private Fields

        private readonly Boolean[] _states;
        private readonly GpioPin[] _relays;
        private readonly GpioPin _r0, _r1;

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets or sets a value indicating whether Relay0 will be ON or OFF
        /// </summary>
        /// <value>
        ///   <c>true</c> if Relay0 should be ON, otherwise <c>false</c>.
        /// </value>
        public Boolean Relay0
        {
            get => GetRelay(0);
            set => SetRelay(0, value);
        }

        /// <summary>
        /// Gets or sets a value indicating whether Relay1 will be ON or OFF
        /// </summary>
        /// <value>
        ///   <c>true</c> if Relay1 should be ON, otherwise <c>false</c>.
        /// </value>
        public Boolean Relay1
        {
            get => GetRelay(1);
            set => SetRelay(1, value);
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Sets the state of a specified relay.
        /// </summary>
        /// <param name="relay">The relay : 0 or 1.</param>
        /// <param name="state">if set to <c>true</c>, relay will be ON otherwise it will be OFF.</param>
        /// <exception cref="IndexOutOfRangeException">Thrown if the relay number is greater than 1.</exception>
        public void SetRelay(Byte relay, Boolean state)
        {
            if (relay > 1)
            {
                throw new IndexOutOfRangeException("relay");
            }

            if (state == _states[relay]) return;
#if (NANOFRAMEWORK_1_0)
            _relays[relay].Write(state ? PinValue.High : PinValue.Low);
#else
            _relays[relay].Write(state ? GpioPinValue.High : GpioPinValue.Low);
#endif
            _states[relay] = state;

            RelayStateChangedEventHandler relayEvent = RelayStateChanged;
            relayEvent?.Invoke(this, new RelayStateChangedEventArgs(relay, !state, state));
            Thread.Sleep(10); // Max operate time, from datasheet
        }

        /// <summary>
        /// Gets the state of a specified relay.
        /// </summary>
        /// <param name="relay">The relay : 0 or 1.</param>
        /// <returns>A <cref>Boolean</cref> indicating the state. <c>true</c> means "Relay ON", <c>false</c> means "Relay OFF".</returns>
        public Boolean GetRelay(Byte relay)
        {
            try
            {
                return _states[relay];
            }
            catch
            {
                throw new IndexOutOfRangeException(nameof(relay));
            }
        }

        #endregion

        #region Events

        /// <summary>
        /// Occurs when the state of a relay has changed.
        /// </summary>
        public event RelayStateChangedEventHandler RelayStateChanged = delegate { };

        /// <summary>
        /// Public delegate for the RelayStateChanged event.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">The <see cref="RelayStateChangedEventArgs"/> instance containing the event data.</param>
        public delegate void RelayStateChangedEventHandler(Object sender, RelayStateChangedEventArgs e);


        #endregion
    }

    /// <summary>
    /// RelayStateChanged arguments
    /// </summary>
    public class RelayStateChangedEventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RelayStateChangedEventArgs"/> class.
        /// </summary>
        /// <param name="eRelay">The specified relay.</param>
        /// <param name="eOldState">The state of the relay before the event was thrown.</param>
        /// <param name="eNewState">The new state of the relay.</param>
        public RelayStateChangedEventArgs(Byte eRelay, Boolean eOldState, Boolean eNewState)
        {
            Relay = eRelay;
            OldState = eOldState;
            NewState = eNewState;
        }

        /// <summary>
        /// Gets the relay number.
        /// </summary>
        /// <value>
        /// The relay number.
        /// </value>
        public Byte Relay { get; }

        /// <summary>
        /// Gets a value indicating the state of the relay before the event was thrown.
        /// </summary>
        /// <value>
        ///   <c>true</c> if relay was ON; otherwise, <c>false</c>.
        /// </value>
        public Boolean OldState { get; }

        /// <summary>
        /// Gets a value indicating the new state of the relay.
        /// </summary>
        /// <value>
        ///   <c>true</c> if relay has been set to ON; otherwise, <c>false</c>.
        /// </value>
        public Boolean NewState { get; }
    }
}

