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
using System.Device.Spi;
#else
using GHIElectronics.TinyCLR.Devices.Gpio;
using GHIElectronics.TinyCLR.Devices.Spi;
#endif

using System;

namespace MBN.Modules
{
    /// <summary>
    /// Directions in which the rotation can occur
    /// </summary>
    public enum Directions
    {
        Clockwise,
        CounterClockwise
    }

    /// <summary>
    /// Main class for the Rotary Click
    /// </summary>
    public sealed partial class RotaryClick
    {
        private readonly GpioPin _encA, _encB, _sw;
#if (NANOFRAMEWORK_1_0)
        private PinValue _aState, _aLastState;
#else
        private GpioPinValue _aState, _aLastState;
#endif
        private readonly SpiDevice _rot;
        private readonly Byte[] _buffer = new Byte[2];
        private readonly Hardware.Socket _socket;

        /// <summary>
        /// Occurs when the knob is pressed.
        /// </summary>
        public event ButtonPressedEventHandler ButtonPressed = delegate { };
        /// <summary>
        /// Occurs when the knob is rotated.
        /// </summary>
        public event RotationEventHandler RotationDetected = delegate { };

        /// <summary>
        /// Initializes a new instance of the <see cref="RotaryClick"/> class.
        /// </summary>
        /// <param name="socket">The socket on which the module is plugged.</param>
        public RotaryClick(Hardware.Socket socket)
        {
            _socket = socket;
#if (NANOFRAMEWORK_1_0)
            _rot = SpiDevice.Create(new SpiConnectionSettings(socket.SpiBus, socket.Cs)
            {
                Mode = SpiMode.Mode0,
                ClockFrequency = 2000000
            });
#else
            _rot = SpiController.FromName(socket.SpiBus).GetDevice(new SpiConnectionSettings()
            {
                ChipSelectType = SpiChipSelectType.Gpio,
                ChipSelectLine = GpioController.GetDefault().OpenPin(socket.Cs),
                Mode = SpiMode.Mode0,
                ClockFrequency = 2000000
            });
#endif

            // Initialize the display and the internal counter
            Write(0);
            InternalCounter = 0;
#if (NANOFRAMEWORK_1_0)
            // First encoder
            _encA = new GpioController().OpenPin(socket.PwmPin, PinMode.Input);
            _aLastState = _encA.Read();

            // Second encoder
            _encB = new GpioController().OpenPin(socket.AnPin, PinMode.Input);

            // Button switch
            _sw = new GpioController().OpenPin(socket.Int, PinMode.InputPullDown);
#else
            // First encoder
            _encA = GpioController.GetDefault().OpenPin(socket.PwmPin);
            _encA.SetDriveMode(GpioPinDriveMode.Input);
            _aLastState = _encA.Read();

            // Second encoder
            _encB = GpioController.GetDefault().OpenPin(socket.AnPin);
            _encB.SetDriveMode(GpioPinDriveMode.Input);

            // Button switch
            _sw = GpioController.GetDefault().OpenPin(socket.Int);
            _sw.SetDriveMode(GpioPinDriveMode.InputPullDown);
#endif

            _encA.ValueChanged += EncA_ValueChanged;
            _sw.ValueChanged += Sw_ValueChanged;
        }

        /// <summary>
        /// Gets the direction of the rotation.
        /// </summary>
        /// <value>
        /// Clockwise or Counterclockwise
        /// </value>
        public Directions Direction
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the internal counter value
        /// </summary>
        /// <value>
        /// The internal counter value
        /// </value>
        public Int32 InternalCounter
        {
            get;
            private set;
        }

        /// <summary>
        /// Sets the led position.
        /// </summary>
        /// <param name="pos">The position from 0 to 16.</param>
        /// <param name="fill">if set to <c>true</c>, all leds up to and including "pos" are lit, otherwise only the led at "pos" is lit.</param>
        public void SetLedPosition(Byte pos, Boolean fill = false)
        {
            if (pos == 0)
                Write(0);
            else
                Write((UInt16)(fill ? (1 << pos) - 1 : 1 << --pos));
        }

        /// <summary>
        /// Writes a pattern to the leds
        /// </summary>
        /// <param name="data">An UInt16 value with its bits indicating if the led is lit or not.</param>
        public void Write(UInt16 data)
        {
            _buffer[1] = (Byte)data;
            _buffer[0] = (Byte)(data >> 8);
            lock (_socket.LockSpi)
            {
                _rot.Write(_buffer);
            }
        }

#if (NANOFRAMEWORK_1_0)
        private void EncA_ValueChanged(object sender, PinValueChangedEventArgs e)
        {
#else
        private void EncA_ValueChanged(GpioPin sender, GpioPinValueChangedEventArgs e)
        {
#endif
            _aState = _encA.Read();
            if (_aState != _aLastState)
            {
                if (_encB.Read() != _aState)
                {
                    InternalCounter++;
                    Direction = Directions.Clockwise;
                }
                else
                {
                    InternalCounter--;
                    Direction = Directions.CounterClockwise;
                }
                RotationEventHandler rotationEvent = RotationDetected;
                rotationEvent(this, new RotationEventArgs(Direction, InternalCounter));
            }
            _aLastState = _aState;
        }

#if (NANOFRAMEWORK_1_0)
        private void Sw_ValueChanged(object sender, PinValueChangedEventArgs e)
        {
            ButtonPressedEventHandler buttonEvent = ButtonPressed;
            buttonEvent(this, new ButtonPressedEventArgs(e.ChangeType));
        }
#else
        private void Sw_ValueChanged(GpioPin sender, GpioPinValueChangedEventArgs e)
        {
            ButtonPressedEventHandler buttonEvent = ButtonPressed;
            buttonEvent(this, new ButtonPressedEventArgs(e.Edge));
        }
#endif

    }
}
