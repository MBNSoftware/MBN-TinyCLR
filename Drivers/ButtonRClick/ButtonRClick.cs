/*
 * ButtonR Click driver for TinyCLR 2.0
 * 
 * Initial revision coded by Stephen Cardinale
 *  
 * Copyright 2020 Stephen Cardinale
 * Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License. You may obtain a copy of the License at
 * http://www.apache.org/licenses/LICENSE-2.0
 * Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, 
 * either express or implied. See the License for the specific language governing permissions and limitations under the License.
 * 
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
    /// Main class for the ButtonRClick driver
    /// <para><b>Pins used :</b> Int and Pwm</para>
    /// </summary>
    /// <example>Example program:
    /// <code language = "C#">
    /// using MBN;
    /// using MBN.Modules;
    ///
    /// using System.Diagnostics;
    /// using System.Threading;
    ///
    /// namespace Examples
    /// {
    ///     public class Program
    ///     {
    ///         private static ButtonRClick _button;
    ///
    ///         public static void Main()
    ///         {
    ///             _button = new ButtonRClick(Hardware.SocketOne) { LEDMode = ButtonRClick.Mode.ToggleWhenPressed };
    ///             _button.ButtonPressed += _button_ButtonPressed;
    ///             _button.ButtonReleased += _button_ButtonReleased;
    ///
    ///             Thread.Sleep(Timeout.Infinite);
    ///         }
    ///
    ///         static void _button_ButtonReleased(ButtonRClick sender, ButtonRClick.ButtonState state)
    ///         {
    ///             Debug.WriteLine($"Button Released Event with a state of {(state == ButtonRClick.ButtonState.Pressed ? " pressed" : " released")}");
    ///         }
    ///
    ///         static void _button_ButtonPressed(ButtonRClick sender, ButtonRClick.ButtonState state)
    ///         {
    ///             Debug.WriteLine($"Button Pressed Event with a state of {(state == ButtonRClick.ButtonState.Pressed ? " pressed" : " released")}");
    ///         }
    ///     }
    /// }
    /// </code>
    /// </example>
    public sealed class ButtonRClick
    {
        #region CTOR

        /// <summary>
        /// Initializes a new instance of the <see cref="ButtonRClick"/> class.
        /// </summary>
        /// <param name="socket">The socket on which the ButtonRClick module is plugged on MikroBus.Net board</param>
        public ButtonRClick(Hardware.Socket socket)
        {
#if (NANOFRAMEWORK_1_0)
            _button = new GpioController().OpenPin(socket.Int, PinMode.InputPullDown);
            _button.ValueChanged += ButtonValueChanged;

            _led = new GpioController().OpenPin(socket.PwmPin, PinMode.Output);
            _led.Write(PinValue.Low);
#else
            _button = GpioController.GetDefault().OpenPin(socket.Int);
            _button.SetDriveMode(GpioPinDriveMode.InputPullDown);
            _button.ValueChanged += ButtonValueChanged;

            _led = GpioController.GetDefault().OpenPin(socket.PwmPin);
            _led.SetDriveMode(GpioPinDriveMode.Output);
            _led.Write(GpioPinValue.Low);
#endif
        }

        #endregion

        #region Fields

        private readonly GpioPin _button;
        private readonly GpioPin _led;
        private Mode _currentMode = Mode.OnWhilePressed;

        #endregion

        #region ENUMS

    	/// <summary>
		///     The state of the button.
		/// </summary>
		public enum ButtonState
		{
			/// <summary>
			///     The button is pressed.
			/// </summary>
			Pressed = 0,

			/// <summary>
			///     The button is released.
			/// </summary>
			Released = 1
		}

        /// <summary>
        /// The various modes a LED can be set to.
        /// </summary>
        public enum Mode
        {
            /// <summary>
            /// The LED is on regardless of the button state.
            /// </summary>
            On,

            /// <summary>
            /// The LED is off regardless of the button state.
            /// </summary>
            Off,

            /// <summary>
            /// The LED changes state whenever the button is pressed.
            /// </summary>
            ToggleWhenPressed,

            /// <summary>
            /// The LED changes state whenever the button is released.
            /// </summary>
            ToggleWhenReleased,

            /// <summary>
            /// The LED is on while the button is pressed.
            /// </summary>
            OnWhilePressed,

            /// <summary>
            /// The LED is on except when the button is pressed.
            /// </summary>
            OnWhileReleased
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets or sets the LED's current mode of operation.
        /// </summary>
        /// <example>This sample shows how to use the Mode property.
        /// <code language = "C#">
        /// button.Mode = ButtonRClick.LedMode.OnWhilePressed;
        /// </code>
        /// </example>
        /// <value>The <see cref="Mode"/> of he Led.</value>
        public Mode LEDMode
        {
            get => _currentMode;
            set
            {
                _currentMode = value;

                if (_currentMode == Mode.On || _currentMode == Mode.OnWhilePressed && Pressed || _currentMode == Mode.OnWhileReleased && !Pressed) TurnLedOn();
                else if (_currentMode == Mode.Off || _currentMode == Mode.OnWhileReleased && Pressed || _currentMode == Mode.OnWhilePressed && !Pressed) TurnLedOff();
            }
        }

        /// <summary>
        /// Whether or not the LED is currently on or off.
        /// </summary>
        /// <example>This sample shows how to use the IsLedOn property.
        /// <code language = "C#">
        /// Debug.Print("LED on ? " + _button.IsLedOn);
        /// </code>
        /// </example>
        /// <value>A <see cref="System.Boolean"/> indicating whether the ButtonR Led is on.</value>
#if (NANOFRAMEWORK_1_0)
        public Boolean IsLedOn => _led.Read() == PinValue.High;
#else
        public Boolean IsLedOn => _led.Read() == GpioPinValue.High;
#endif

        /// <summary>
        /// Whether or not the button is pressed.
        /// </summary>
        /// <example>This sample shows how to use the Pressed property.
        /// <code language = "C#">
        ///  Debug.Print("Button pressed ? " + _button.Pressed);
        /// </code>
        /// </example>
        /// <value>A <see cref="System.Boolean"/> indicating whether the ButtonR click is pressed.</value>
#if (NANOFRAMEWORK_1_0)
        public Boolean Pressed => _button.Read() == PinValue.High;
#else
        public Boolean Pressed => _button.Read() == GpioPinValue.High;
#endif

        #endregion

        #region Public Methods

        /// <summary>
        /// Turns on the LED.
        /// </summary>
        /// <example>This sample shows how to use the TurnLedOn method.
        /// <code language = "C#">
        /// if (_button.IsLedOn) _button.TurnLedOff();
        /// </code>
        /// </example>
        /// <returns> <see cref="System.Void"/></returns>
        public void TurnLedOn()
        {
#if (NANOFRAMEWORK_1_0)
            _led.Write(PinValue.High);
#else
            _led.Write(GpioPinValue.High);
#endif
        }

        /// <summary>
        /// Turns off the LED.
        /// </summary>
        /// <example>This sample shows how to use the TurnLedOff method.
        /// <code language = "C#">
        /// if (!_button.IsLedOn) _button.TurnLedOff();
        /// </code>
        /// </example>
        /// <returns> <see cref="System.Void"/></returns>
        public void TurnLedOff()
        {
#if (NANOFRAMEWORK_1_0)
            _led.Write(PinValue.Low);
#else
            _led.Write(GpioPinValue.Low);
#endif
        }

        /// <summary>
        /// Turns the LED off if it is on and on if it is off.
        /// </summary>
        /// <example>This sample shows how to use the ToggleLed method.
        /// <code language = "C#">
        /// _button.ToggleLed();
        /// </code>
        /// </example>
        /// <returns> <see cref="System.Void"/></returns>
        public void ToggleLed()
        {
#if (NANOFRAMEWORK_1_0)
            _led.Toggle();
#else
            if (IsLedOn) TurnLedOff();
            else TurnLedOn();
#endif
        }

        #endregion

        #region Events

		/// <summary>
		///     Represents the delegate that is used to handle the <see cref="ButtonReleased" /> and <see cref="ButtonPressed" />
		///     events.
		/// </summary>
		/// <param name="sender">The <see cref="ButtonRClick" /> object that raised the event.</param>
		/// <param name="state">The state of the Button</param>
		public delegate void ButtonEventHandler(ButtonRClick sender, ButtonState state);

		private ButtonEventHandler _onButtonEvent;
#if (NANOFRAMEWORK_1_0)
        private void ButtonValueChanged(object sender, PinValueChangedEventArgs e)
        {
            ButtonState state = e.ChangeType == PinEventTypes.Falling ? ButtonState.Released : ButtonState.Pressed;
#else
        private void ButtonValueChanged(GpioPin sender, GpioPinValueChangedEventArgs e)
        {
            ButtonState state = e.Edge== GpioPinEdge.FallingEdge ? ButtonState.Released : ButtonState.Pressed;
#endif

			switch (state)
			{
				case ButtonState.Released:
                {
                    switch (_currentMode)
                    {
                        case Mode.OnWhilePressed:
                        {
                            TurnLedOff();
                            break;
                        }

                        case Mode.OnWhileReleased:
                        {
                            TurnLedOn();
                            break;
                        }

                        case Mode.ToggleWhenReleased:
                        {
                            ToggleLed();
                            break;
                        }

                        case Mode.On:
                        {
                            break;
                        }

                        case Mode.Off:
                        {
                            break;
                        }

                        case Mode.ToggleWhenPressed:
                        {
                            break;
                        }

                        default:
                        {
                            throw new ArgumentOutOfRangeException();
                        }
                    }

                    break;
                }

                case ButtonState.Pressed:
                {
                    switch (_currentMode)
                    {
                        case Mode.OnWhilePressed:
                        {
                            TurnLedOn();
                            break;
                        }

                        case Mode.OnWhileReleased:
                        {
                            TurnLedOff();
                            break;
                        }

                        case Mode.ToggleWhenPressed:
                        {
                            ToggleLed();
                            break;
                        }

                        case Mode.On:
                        {
                            break;
                        }

                        case Mode.Off:
                        {
                            break;
                        }

                        case Mode.ToggleWhenReleased:
                        {
                            break;
                        }

                        default:
                        {
                            throw new ArgumentOutOfRangeException();
                        }
                    }

                    break;
                }

                default:
                {
                    throw new ArgumentOutOfRangeException();
                }
            }

		    OnButtonEvent(this, state);
		}

        /// <summary>
        ///     Raised when the button is released.
        /// </summary>
        public event ButtonEventHandler ButtonReleased;

        /// <summary>
        ///     Raised when the button is pressed.
        /// </summary>
        public event ButtonEventHandler ButtonPressed;

        private void OnButtonEvent(ButtonRClick sender, ButtonState state)
        {
            if (_onButtonEvent == null)
            {
                _onButtonEvent = OnButtonEvent;
            }
            switch (state)
            {
                case ButtonState.Pressed:
                {
                    ButtonPressed?.Invoke(sender, ButtonState.Pressed);
                    break;
                }

                case ButtonState.Released:
                {
                    ButtonReleased?.Invoke(sender, ButtonState.Released);
                    break;
                }

                default:
                {
                    throw new ArgumentOutOfRangeException(nameof(state));
                }
            }
        }

        #endregion
    }
}