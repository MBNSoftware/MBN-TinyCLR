/*
 * I2cMux2Click driver for TinyCLR 2.0
 *  Version 1.0
 *  - Initial version
 *  
 * Copyright 2020 Mikrobus.Net
 * Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License. You may obtain a copy of the License at
 * http://www.apache.org/licenses/LICENSE-2.0
 * Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, 
 * either express or implied. See the License for the specific language governing permissions and limitations under the License.
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
    /// Main class for the I2CMux Click driver
    /// <para>This module is an I2C Device. <b>Pins used :</b> Scl, Sda, Rst</para>
    /// </summary>
    /// <example>Example usage:
    /// <code language = "C#">
    /// using MBN;
    /// using MBN.Modules;
    /// using System.Threading;
    ///
    /// namespace HTU21DClickTestApp
    /// {
    /// 	public class Program
    /// 	{
    /// 		private static I2CMuxClick _mux;
    /// 		private static ProximityClick _prox1;
    /// 		private static ProximityClick _prox2;
    ///
    /// 		public static void Main()
    /// 		{
    /// 			_mux = new I2CMux2Click(Hardware.SocketTwo, 0xE0 >> 1, 100000);
    /// 			// Note that the four sensors have the same I2C address
    /// 			_sensors[0] = new ProximityClick(0xA0 >> 1, 100000);
    ///             _sensors[1] = new ProximityClick(0xA0 >> 1, 100000);
    ///             _sensors[2] = new ProximityClick(0xA0 >> 1, 100000);
    ///             _sensors[3] = new ProximityClick(0xA0 >> 1, 100000);
    ///             
    ///            for (var i = 0; i< 4; i++)
    ///            {
    ///                 _mux.ActiveChannels = (Byte) (1 << i);
    ///                 Debug.WriteLine($"Proximity Click on channel {_mux.ActiveChannels} reads a distance of {_sensors[i].Distance} m");
    ///             }
    /// 			
    ///             Thread.Sleep(Timeout.Infinite);
    /// 		}
    /// 	}
    /// }
    /// </code>
    /// </example>
    public sealed partial class I2cMux2Click
    {
        private readonly GpioPin _rst, _int;
        private readonly I2cDevice _mux2;
        private readonly Hardware.Socket _socket;
        private Boolean _intEnabled;
        private readonly Byte[] _channelValue = new Byte[1];
        private readonly Byte[] _channelSet = new Byte[1];

        /// <summary>
        /// Occurs when an interrupt is detected on any channel.
        /// </summary>
        public event InterruptEventHandler InterruptDetected = delegate { };

        /// <summary>
        /// Initializes a new instance of the <see cref="I2cMux2Click"/> class.
        /// <para>Interrupts are enabled by default.</para>
        /// </summary>
        /// <param name="socket">The socket on which the module is plugged</param>
        /// <param name="address">The I2C address of the module</param>
        /// <param name="busSpeed">The bus speed.</param>
#if (NANOFRAMEWORK_1_0)
        public I2cMux2Click(Hardware.Socket socket, byte address = 0xE0 >> 1, I2cBusSpeed busSpeed = I2cBusSpeed.StandardMode)
#else
        public I2cMux2Click(Hardware.Socket socket, Byte address = 0xE0 >> 1, UInt32 busSpeed = 100000)
#endif
        {
            _socket = socket;
#if (NANOFRAMEWORK_1_0)
            _mux2 = I2cDevice.Create(new I2cConnectionSettings(socket.I2cBus, address, busSpeed));
            _rst = new GpioController().OpenPin(socket.Rst);
            _rst.SetPinMode(PinMode.Output);
            _rst.Write(PinValue.Low);
            Thread.Sleep(100);
            _rst.Write(PinValue.High);

            _int = new GpioController().OpenPin(socket.Int);
            _int.SetPinMode(PinMode.InputPullUp);
#else
            _mux2 = I2cController.FromName(socket.I2cBus).GetDevice(new I2cConnectionSettings(address, busSpeed));
            _rst = GpioController.GetDefault().OpenPin(socket.Rst);
            _rst.SetDriveMode(GpioPinDriveMode.Output);
            _rst.Write(GpioPinValue.Low);
            Thread.Sleep(100);
            _rst.Write(GpioPinValue.High);

            _int = GpioController.GetDefault().OpenPin(socket.Int);
            _int.SetDriveMode(GpioPinDriveMode.InputPullUp);
#endif
            EnableInterrupts();
        }

        /// <summary>
        /// Enables the interrupts.
        /// </summary>
        public void EnableInterrupts()
        {
            if (!_intEnabled)
            {
                _int.ValueChanged += Int_ValueChanged;
                _intEnabled = true;
            }
        }

        /// <summary>
        /// Disables the interrupts.
        /// </summary>
        public void DisableInterrupts()
        {
            if (_intEnabled)
            {
                _int.ValueChanged -= Int_ValueChanged;
                _intEnabled = false;
            }
        }

#if (NANOFRAMEWORK_1_0)
        private void Int_ValueChanged(object sender, PinValueChangedEventArgs e)
        {
            if (e.ChangeType == PinEventTypes.Falling)
#else
        private void Int_ValueChanged(GpioPin sender, GpioPinValueChangedEventArgs e)
        {
            if (e.Edge == GpioPinEdge.FallingEdge)
#endif
            {
                lock (_socket.LockI2c)
                {
                    _mux2.Read(_channelValue);
                }
                InterruptEventHandler intEvent = InterruptDetected;
                intEvent(this, new InterruptEventArgs((Byte)(_channelValue[0] >> 4)));
            }
        }

        /// <summary>
		/// Gets or sets the active channel(s), starting at 0
        /// Easiest way to determine the active channels is to use this syntax : 0b0000xxxx
        /// where "x" is set to 1 to enable the channel. e.g. : ActiveChannels = 0b00000100 to activate channel 2
		/// </summary>
        public Byte ActiveChannels
        {
            get
            {
                lock (_socket.LockI2c)
                {
                    _mux2.Read(_channelSet);
                }

                return _channelSet[0];
            }
            set
            {
                _channelSet[0] = value;
                lock (_socket.LockI2c)
                {
                    _mux2.Write(_channelSet);
                }
            }
        }
    }
}
