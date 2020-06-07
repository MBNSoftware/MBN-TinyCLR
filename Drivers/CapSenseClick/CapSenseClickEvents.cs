/*
 * CapSense Click board driver for TinyCLR 2.0
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

using System;

namespace MBN.Modules
{
    public sealed partial class CapSenseClick
    {
        /// <summary>
        /// Occurs after a call to the <see cref="CheckButtons"/> method if a button state has changed.
        /// </summary>
        /// <remarks>See <see cref="ButtonPressedEventArgs"/> for the data returned by the event.</remarks>
        public delegate void ButtonPressedEventHandler(Object sender, ButtonPressedEventArgs e);

        /// <summary>
        /// Occurs after a call to the <see cref="CheckSlider"/> method if a the value of the slider has changed.
        /// </summary>
        /// <remarks>See <see cref="SliderEventArgs"/> for the data returned by the event.</remarks>
        public delegate void SliderEventHandler(Object sender, SliderEventArgs e);

        /// <summary>
        /// Event raised when a button has been pressed or released after a call to the <see cref="CheckButtons"/> method.
        /// </summary>
        public class ButtonPressedEventArgs
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="ButtonPressedEventArgs"/> class.
            /// </summary>
            /// <param name="eButtonTop">State of the top button (the one above the slider)</param>
            /// <param name="eButtonBottom">State of the bottom button (the one below the slider)</param>
            public ButtonPressedEventArgs(Boolean eButtonTop, Boolean eButtonBottom)
            {
                ButtonTop = eButtonTop;
                ButtonBottom = eButtonBottom;
            }

            /// <summary>
            /// State of the top button. True = button pressed, false = button released.
            /// </summary>
            public Boolean ButtonTop { get; private set; }

            /// <summary>
            /// State of the bottom button. True = button pressed, false = button released.
            /// </summary>
            public Boolean ButtonBottom { get; private set; }
        }

        /// <summary>
        /// Event raised when the value of the slider has changed after a call to the <see cref="CheckSlider"/> method.
        /// </summary>
        public class SliderEventArgs
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="SliderEventArgs"/> class.
            /// </summary>
            /// <param name="eSliderValue">Current value of the slider</param>
            /// <param name="eFingerPresent">Indicate whether a finger is present or not on the slider</param>
            public SliderEventArgs(Int32 eSliderValue, Boolean eFingerPresent)
            {
                FingerPresent = eFingerPresent;
                SliderValue = eSliderValue;
            }

            /// <summary>
            /// Value of the slider. Value will be in the range defined by the content of registers 0x77 and 0x78 (Multiplier).
            /// </summary>
            public Int32 SliderValue { get; private set; }

            /// <summary>
            /// True = finger present on the slider, false otherwise.
            /// </summary>
            public Boolean FingerPresent { get; private set; }
        }
    }
}