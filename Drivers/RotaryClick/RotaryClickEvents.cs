/*
 * Rotary Click board driver for TinyCLR 2.0
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
    public sealed partial class RotaryClick
    {
        /// <summary>
        /// Occurs when the rotary knob is pressed
        /// </summary>
        /// <remarks>See <see cref="ButtonPressedEventArgs"/> for the data returned by the event.</remarks>
        public delegate void ButtonPressedEventHandler(Object sender, ButtonPressedEventArgs e);

        /// <summary>
        /// Occurs when the button is rotating
        /// </summary>
        /// <remarks>See <see cref="RotationEventArgs"/> for the data returned by the event.</remarks>
        public delegate void RotationEventHandler(Object sender, RotationEventArgs e);

        /// <summary>
        /// Event raised when a button has been pressed or released
        /// </summary>
        public class ButtonPressedEventArgs
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="ButtonPressedEventArgs"/> class.
            /// </summary>
            /// <param name="gpioPinEdge">Rising or falling edge</param>
#if (NANOFRAMEWORK_1_0)
            public ButtonPressedEventArgs(PinEventTypes gpioPinEdge)
#else
            public ButtonPressedEventArgs(GpioPinEdge gpioPinEdge)
#endif
            {
                Edge = gpioPinEdge;
            }

            /// <summary>
            /// State of the button edge
            /// </summary>
#if (NANOFRAMEWORK_1_0)
            public PinEventTypes Edge
#else
            public GpioPinEdge Edge
#endif
            {
                get; private set;
            }
        }

        /// <summary>
        /// Event raised when the knob is rotating
        /// </summary>
        public class RotationEventArgs
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="RotationEventArgs"/> class.
            /// </summary>
            /// <param name="direction">Direction of the move : clockwise or counterclockwise</param>
            /// <param name="counter">Value of the internal counter</param>
            public RotationEventArgs(Directions direction, Int32 counter)
            {
                Direction = direction;
                InternalCounter = counter;
            }

            /// <summary>
            /// Value of the internal counter.
            /// </summary>
            public Int32 InternalCounter
            {
                get; private set;
            }

            /// <summary>
            /// Direction of the move : clockwise or counterclockwise
            /// </summary>
            public Directions Direction
            {
                get; private set;
            }
        }
    }
}
