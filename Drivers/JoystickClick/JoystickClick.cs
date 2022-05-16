/*
 * JoystickClick driver for TinyCLR 2.0
 * 
 * Initial revision
 *  
 * Copyright 2020 MikroBus.Net
 * Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License. You may obtain a copy of the License at
 * http://www.apache.org/licenses/LICENSE-2.0
 * Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, 
 * either express or implied. See the License for the specific language governing permissions and limitations under the License.
 */

/*
 * WARNING : This module won't work on SC20260D Mikrobus socket #2 because CS (PC13) and INT (PJ13) share the same pin number 13.
 * Interrupts cannot be used simultaneously on pins with the same number. See http://docs.ghielectronics.com/software/tinyclr/tutorials/gpio.html
 */

#if (NANOFRAMEWORK_1_0)
using System.Device.Gpio;
using System.Device.I2c;
#else
using GHIElectronics.TinyCLR.Devices.Gpio;
using GHIElectronics.TinyCLR.Devices.I2c;
#endif

using System;
using System.Threading;

namespace MBN.Modules
{
    /// <summary>
    /// Main class for the JoystickClick driver
    /// <para>The module is an I2C device. <b>Pins used :</b> Scl, Sda, Int, Rst, Cs</para>
    /// <example>
    /// <code language="C#">
    ///	using System;
    ///	using MBN.Enums;
    ///	using MBN.Modules;
    ///	using MBN;
    ///	using System.Threading;
    ///	using Microsoft.SPOT;
    ///	
    ///	namespace TestJoystick
    ///	{
    ///	    public class Program
    ///	    {
    ///			private static JoystickClick _joy;
    ///	
    ///	        public static void Main()
    ///	        {
    ///	            _joy = new JoystickClick(Hardware.SocketThree) { DeadZone = new SByte[] { 100, -100, 100, -100 }, TimeBase = 7 };
    ///	
    ///	            _joy.InterruptLine.OnInterrupt += InterruptLineOnInterruptLine;
    ///	            _joy.Button.OnInterrupt += Button_OnInterrupt;
    ///	
    ///	            Thread.Sleep(Timeout.Infinite);
    ///	        }
    ///	
    ///	        private static void Button_OnInterrupt(uint data1, uint data2, DateTime time)
    ///	        {
    ///	            Hardware.Led1.Write(data2 != 0);
    ///	        }
    ///	
    ///	        private static void InterruptLineOnInterruptLine(uint data1, uint data2, DateTime time)
    ///	        {
    ///	            var pos = _joy.GetKnobPosition();
    ///	        }
    ///	    }
    ///	}
    /// </code>
    /// </example>
    /// </summary>
    public sealed partial class JoystickClick
    {
        /// <summary>
        /// Structure containing actual knob position coordinates
        /// </summary>
        public struct KnobPosition
        {
            /// <summary>
            /// The X-axis value
            /// </summary>
            public Int32 X;
            /// <summary>
            /// The Y-axis value
            /// </summary>
            public Int32 Y;

            /// <summary>
            /// Initializes a new instance of the <see cref="KnobPosition"/> struct.
            /// </summary>
            /// <param name="posX">The x-axis position.</param>
            /// <param name="posY">The y-axis position.</param>
            public KnobPosition(Int32 posX, Int32 posY)
            {
                X = posX;
                Y = posY;
            }
        }

        /// <summary>
        /// The interrupt line used to signal that the knob position has passed the user-defined dead-zone.
        /// </summary>
        public GpioPin InterruptLine;

        /// <summary>
        /// Used to signal that the joystick button has been pressed/released.
        /// </summary>
        public GpioPin Button;

        private readonly GpioPin _reset;
        private readonly I2cDevice _joystick;       // I²C configuration
        private readonly Hardware.Socket _socket;

        #region Private methods
        private Byte ReadRegister(Byte register)
        {
            var result = new Byte[1];

            lock (_socket.LockI2c)
            {
                _joystick.WriteRead(new[] { register }, result);
            }

            return result[0];
        }

        private SByte ReadSignedRegister(Byte register)
        {
            var result = new Byte[1];

            lock (_socket.LockI2c)
            {
                _joystick.WriteRead(new[] { register }, result);
            }
            return (SByte)result[0];
        }

        private void WriteRegister(Byte register, Byte data)
        {
            lock (_socket.LockI2c)
            {
                _joystick.Write(new[] { register, data });
            }
        }

        private Boolean DataReady() => (ReadRegister(Registers.CONTROL1) & 0x01) == 0x01;
        #endregion

        /// <summary>
        /// Initializes a new instance of the <see cref="JoystickClick"/> class.
        /// </summary>
        /// <param name="socket">The socket on which the JoystickClick module is plugged on MikroBus.Net board</param>
        /// <param name="address">The address of the module.</param>
        public JoystickClick(Hardware.Socket socket, Byte address = 0x40)
        {
            _socket = socket;
            // Create the driver's I²C configuration
#if (NANOFRAMEWORK_1_0)
            _joystick = I2cDevice.Create(new I2cConnectionSettings(socket.I2cBus, address, I2cBusSpeed.StandardMode));
#else
			_joystick = I2cController.FromName(socket.I2cBus).GetDevice(new I2cConnectionSettings(address, 100000));
#endif

            WriteRegister(Registers.CONTROL1, 0b11110000);
#if (NANOFRAMEWORK_1_0)
            _reset = new GpioController().OpenPin(socket.PwmPin);
            _reset.SetPinMode(PinMode.Output);

            Button = new GpioController().OpenPin(socket.Cs);
            Button.SetPinMode(PinMode.Input);
            
            InterruptLine = new GpioController().OpenPin(socket.Int);
            InterruptLine.SetPinMode(PinMode.InputPullUp);
#else
            _reset = GpioController.GetDefault().OpenPin(socket.PwmPin);
            _reset.SetDriveMode(GpioPinDriveMode.Output);

            Button = GpioController.GetDefault().OpenPin(socket.Cs);
            Button.SetDriveMode(GpioPinDriveMode.Input);
            
            InterruptLine = GpioController.GetDefault().OpenPin(socket.Int);
            InterruptLine.SetDriveMode(GpioPinDriveMode.InputPullUp);
#endif

            Reset(ResetModes.Hard);

            Sensitivity = 0x3F; // Max sensitivity
            Scaling = 0x09;     // 100% scaling

            PowerMode = PowerModes.On;      // Interrupt mode

            TimeBase = 3;
            ReadRegister(0x11);     // Don't care about the first data available
        }

        /// <summary>
        /// Gets the knob position.
        /// </summary>
        /// <example>
        /// <code language="C#">
        ///             var pos = _joy.GetKnobPosition();
        /// </code>
        /// </example>
        /// <returns>A structure containing the actual coordinates of the knob.</returns>
        public KnobPosition GetKnobPosition() => new KnobPosition(ReadSignedRegister(Registers.X), ReadSignedRegister(Registers.Y));

        /// <summary>
        /// Gets or sets the dead zone. In interrupt mode, no interrupt will be generated while the knob is inside this zone.
        /// </summary>
        /// <example>
        /// <code language="C#">
        ///             _joy.DeadZone = new SByte[] { 100, -100, 100, -100 };
        /// </code>
        /// </example>
        /// <value>
        /// An array of signed-bytes containing the limits of the dead-zone. Xp stands for "X positive" and Xn for "X negative".
        /// </value>
        /// <exception cref="System.ArgumentException">Thrown if array size if not 4 SBytes.</exception>
        public SByte[] DeadZone
        {
            get
            {
                return new[]
                {
                    ReadSignedRegister(Registers.Xp), 
                    ReadSignedRegister(Registers.Xn), 
                    ReadSignedRegister(Registers.Yp),
                    ReadSignedRegister(Registers.Yn)
                };
            }
            set
            {
                if (value.Length != 4) { throw new ArgumentException(); }
                WriteRegister(Registers.Xp, (Byte)value[0]);
                WriteRegister(Registers.Xn, (Byte)value[1]);
                WriteRegister(Registers.Yp, (Byte)value[2]);
                WriteRegister(Registers.Yn, (Byte)value[3]);
            }
        }

        /// <summary>
        /// Gets or sets the time base used internally by the chip to poll the position. See datasheet for exact meaning of the value.
        /// </summary>
        /// <example>
        /// <code language="C#">
        ///             _joy.TimeBase = 3;  // 100 ms internal polling
        /// </code>
        /// </example>
        /// <value>
        /// The time base.
        /// </value>
        public Byte TimeBase
        {
            get => (Byte)((ReadRegister(Registers.CONTROL1) & 0x70) >> 4);
            set => WriteRegister(Registers.CONTROL1, Bits.Set(ReadRegister(Registers.CONTROL1), "01110000", value > 7 ? (Byte)7 : value));
        }

        /// <summary>
        /// Gets or sets the scaling used to fill the SByte space with actual knob position. Please refer to the datasheet for complete understanding of this value.
        /// </summary>
        /// <example>
        /// <code language="C#">
        ///             _joy.Scaling = 0x09;        // Scale : 100%
        /// </code>
        /// </example>
        /// <value>
        /// The scaling factor.
        /// </value>
        public Byte Scaling
        {
            get => ReadRegister(Registers.T_CTRL);
            set => WriteRegister(Registers.T_CTRL, value > 79 ? (Byte)79 : value);
        }

        /// <summary>
        /// Gets or sets the sensitivity of the chip to the magnets.
        /// <para>With MikroE Click board, it should be set to the max value 0x3F.</para>
        /// </summary>
        /// <example>
        /// <code language="C#">
        ///             _joy.Sensitivity = 0x3F;
        /// </code>
        /// </example>
        /// <value>
        /// The sensitivity factor.
        /// </value>
        public Byte Sensitivity
        {
            get => ReadRegister(Registers.AGC);
            set => WriteRegister(Registers.AGC, value > 0x3F ? (Byte)0x3F : value);
        }


        /// <summary>
        /// Gets or sets the power mode.
        /// </summary>
        /// <example> This sample shows how to use the PowerMode property.
        /// <code language="C#">
        ///             _JoystickClick.PowerMode = PowerModes.Off;
        /// </code>
        /// </example>
        /// <value>
        /// The current power mode of the module.
        /// </value>
        /// <exception cref="System.NotImplementedException">Thrown if the property is set to PowerModes.Off, as the module doesn't support this mode.</exception>
        public PowerModes PowerMode
        {
            get => (ReadRegister(Registers.CONTROL1) & 0x80) == 0x80 ? PowerModes.Low : PowerModes.On;
            set
            {
                if (value == PowerModes.Off)
                {
                    throw new NotImplementedException();
                }
                WriteRegister(Registers.CONTROL1, Bits.Set(ReadRegister(Registers.CONTROL1), value == PowerModes.On ? "0XXX010X" : "1XXX100X"));
            }
        }

        /// <summary>
        /// Resets the module
        /// </summary>
        /// <param name="resetMode">The reset mode :
        /// <para>SOFT reset : generally by sending a software command to the chip</para><para>HARD reset : generally by activating a special chip's pin</para></param>
        /// <returns></returns>
        public Boolean Reset(ResetModes resetMode)
        {
            if (resetMode == ResetModes.Hard)
            {
#if (NANOFRAMEWORK_1_0)
                _reset.Write(PinValue.Low);
                Thread.Sleep(200);
                _reset.Write(PinValue.High);
#else
                _reset.Write(GpioPinValue.Low);
                Thread.Sleep(200);
                _reset.Write(GpioPinValue.High);
#endif
            }
            else
            {
                WriteRegister(Registers.CONTROL1, 0x02);
            }
            Thread.Sleep(1200);
            do 
            { 
                Thread.Sleep(100);
                var toto = ReadRegister(Registers.CONTROL1);
            } while ((ReadRegister(Registers.CONTROL1) & 0xF0) != 0xF0);

            WriteRegister(Registers.CONTROL2, 0x84);

            return true;
        }
    }
}


