/*
 * UniHall Click board driver for TinyCLR 2.0
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

namespace MBN.Modules
{
    /// <summary>
    /// Main class for the UniHall Click driver
    /// </summary>
    public sealed class UniHallClick
    {
        /// <summary>
        /// Occurs when a magnet is detected/removed
        /// </summary>
        public event MagnetDetectedEventHandler MagnetDetected = delegate { };

        private readonly GpioPin _int;

        /// <summary>
        /// Initializes a new instance of the <see cref="UniHallClick"/> class.
        /// </summary>
        /// <param name="socket">The socket on which the module is plugged.</param>
        public UniHallClick(Hardware.Socket socket)
        {
#if (NANOFRAMEWORK_1_0)
            _int = new GpioController().OpenPin(socket.Int);
            _int.SetPinMode(PinMode.InputPullUp);
#else
            _int = GpioController.GetDefault().OpenPin(socket.Int);
            _int.SetDriveMode(GpioPinDriveMode.InputPullUp);
#endif
            _int.ValueChanged += Int_ValueChanged;
        }

#if (NANOFRAMEWORK_1_0)
        private void Int_ValueChanged(object sender, PinValueChangedEventArgs e)
#else
        private void Int_ValueChanged(GpioPin sender, GpioPinValueChangedEventArgs e)
#endif
        {
            MagnetDetectedEventHandler magnetEvent = MagnetDetected;
            magnetEvent(this, new MagnetDetectedEventArgs(_int.Read()));
        }

        /// <summary>
        /// Occurs when a magnet is present or removed
        /// </summary>
        /// <remarks>See <see cref="MagnetDetectedEventArgs"/> for the data returned by the event.</remarks>
        public delegate void MagnetDetectedEventHandler(Object sender, MagnetDetectedEventArgs e);

        /// <summary>
        /// Event raised when a magnet is present or removed
        /// </summary>
        public class MagnetDetectedEventArgs
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="MagnetDetectedEventArgs"/> class.
            /// </summary>
            /// <param name="value">Reading of the INT pin</param>
#if (NANOFRAMEWORK_1_0)
            public MagnetDetectedEventArgs(PinValue value) => MagnetPresent = value == 0;
#else
            public MagnetDetectedEventArgs(GpioPinValue value) => MagnetPresent = value == 0;
#endif

            /// <summary>
            /// State of the magnet
            /// </summary>
            public Boolean MagnetPresent
            {
                get; private set;
            }
        }
    }
}
