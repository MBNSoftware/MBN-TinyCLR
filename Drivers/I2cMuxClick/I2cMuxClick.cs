/*
 * I2C Mux Click driver for TinyCLR 2.0
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
    /// 			_mux = new I2CMuxClick(Hardware.SocketTwo, 0xE0 >> 1, 100000);
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
    public sealed class I2CMuxClick
    {
        private readonly GpioPin _rst;
        private readonly I2cDevice _mux;
        private readonly Hardware.Socket _socket;

#if (NANOFRAMEWORK_1_0)
        public I2CMuxClick(Hardware.Socket socket, byte address, I2cBusSpeed busSpeed)
#else
        public I2CMuxClick(Hardware.Socket socket, Byte address, UInt32 busSpeed)
#endif
        {
            _socket = socket;
#if (NANOFRAMEWORK_1_0)
            _mux = I2cDevice.Create(new I2cConnectionSettings(socket.I2cBus, address, busSpeed));
            _rst = new GpioController().OpenPin(socket.Rst);
            _rst.SetPinMode(PinMode.Output);
            _rst.Write(PinValue.Low);
            Thread.Sleep(100);
            _rst.Write(PinValue.High);
#else
            _mux = I2cController.FromName(socket.I2cBus).GetDevice(new I2cConnectionSettings(address, busSpeed));
            _rst = GpioController.GetDefault().OpenPin(socket.Rst);
            _rst.SetDriveMode(GpioPinDriveMode.Output);
            _rst.Write(GpioPinValue.Low);
            Thread.Sleep(100);
            _rst.Write(GpioPinValue.High);
#endif
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
                var chan = new Byte[1];
                _mux.Read(chan);

                return chan[0];
            }
            set
            {
                lock (_socket.LockI2c)
                {
                    _mux.Write(new[] { value });
                }
            }
        }
    }
}
