/* 
 *  Digipot Click board driver for TinyCLR 2.0
 * 
 * Version 1.0 :
 *  - Initial revision
 *  
 * Copyright 2020 MikroBus.Net
 * Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License. You may obtain a copy of the License at
 * http://www.apache.org/licenses/LICENSE-2.0
 * Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, 
 * either express or implied. See the License for the specific language governing permissions and limitations under the License.
 * Click board driver skeleton generated on 3/2/2014 2:33:52 PM
 *  
 * Datasheet to read here: http://ww1.microchip.com/downloads/en/DeviceDoc/22059b.pdf
 * Click board user guide: http://www.mikroe.com/downloads/get/1716/digipot_click_manual_v100.pdf
 * 
 */

#if (NANOFRAMEWORK_1_0)
using System.Device.Spi;
#else
using GHIElectronics.TinyCLR.Devices.Gpio;
using GHIElectronics.TinyCLR.Devices.Spi;
#endif

using System.Diagnostics;
using System;

namespace MBN.Modules
{
    /// <summary>
    /// Main class for the MikroE Digipot Click board driver
    /// <para><b>Pins used :</b> Miso, Mosi, Sck, Cs.</para>
    /// </summary>
    /// <example>
    /// <code language="C#">
    /// public class Program
    /// {
    ///     static DigiPotClick _dp;
    ///
    ///     public static void Main()
    ///     {
    ///        _dp = new DigiPotClick(Hardware.SocketOne);
    ///
    ///        // Sets resistance to 50 Ohm
    ///        _dp.Resistance = 50;
    ///        Thread.Sleep(1000);
    ///        
    ///        // Sets resistance to 1K Ohm
    ///        _dp.Resistance = 1000;
    /// 
    ///         Thread.Sleep (Timeout.Infinite);
	///	    }
    /// }
    /// </code>
    /// </example>
    public sealed class DigiPotClick
    {
        private readonly SpiDevice _pot;
        private UInt16 _currentResistance;
        private readonly Hardware.Socket _socket;

        /// <summary>
        /// Main class for the MikroE Digipot Click board driver
        /// </summary>
        /// <param name="socket">The socket on which the Digipot Click board is plugged on MikroBus.Net</param>
        /// <param name="initialResistance">The initial resistance the Digipot should be initialized with.</param>
        public DigiPotClick(Hardware.Socket socket, UInt16 initialResistance=0)
        {
            _socket = socket;
            // Initialize SPI
#if (NANOFRAMEWORK_1_0)
            _pot = SpiDevice.Create(new SpiConnectionSettings(socket.SpiBus, socket.Cs)
            {
                Mode = SpiMode.Mode0,
                ClockFrequency = 1000000
            });
#else
            _pot = SpiController.FromName(socket.SpiBus).GetDevice(new SpiConnectionSettings()
            {
                ChipSelectType = SpiChipSelectType.Gpio,
                ChipSelectLine = GpioController.GetDefault().OpenPin(socket.Cs),
                Mode = SpiMode.Mode0,
                ClockFrequency = 1000000
            });
#endif

            _currentResistance = initialResistance;
            Resistance = _currentResistance;
        }

        /// <summary>
        /// Gets or sets the resistance of the Digipot click.
        /// </summary>
        /// <value>
        /// The resistance between 0 and 10K Ohm, measured between the PA and PW pins.
        /// This will show as 111 Ohm to ~9.800 K Ohm on the Ohm meter. Gap between expected and measured resistance is not linear and grows as resistance gets higher.
        /// </value>
        /// <example>
        /// <code language="C#">
        ///        _dp.Resistance = 50;
        /// </code>
        /// </example>
        public UInt16 Resistance
        {
            get
            {
                return _currentResistance;
            }
            set
            {
                var valtmp = (Byte)(value / 40);
                valtmp = (valtmp > 255) ? (Byte)255 : valtmp;
                _currentResistance = value == 0 ? (UInt16)111 : (UInt16)(valtmp*40 + 72);
                
                lock (_socket.LockSpi)
                {
                    _pot.Write(new[] { (Byte)0x00, valtmp, (Byte)0x20, valtmp });
                }
#if DEBUG
                Debug.WriteLine("Digipot: calculated resistance:" + _currentResistance);
#endif                
            }
        }
    }
}

