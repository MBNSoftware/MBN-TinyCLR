/*
 * IRThermo Click board driver for TInyCLR 2.0
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
using System.Device.I2c;
#else
using GHIElectronics.TinyCLR.Devices.I2c;
#endif

using System;

namespace MBN.Modules
{
    /// <summary>
    /// Main class for the IRThermoClick driver
    /// <para><b>Pins used :</b> Scl, Sda</para>
    /// </summary>
    /// <example>
    /// <code language="C#">
    ///		public class Program
    ///    	{
    ///        private static IRThermoClick _ir;
    ///        
    ///        public static void Main()
    ///        {
    ///            _ir = new IRThermoClick(Hardware.SocketThree);
    ///
    ///            Debug.Print("Ambient temperature : " + _ir.ReadTemperature().ToString("F2"));
    ///            Debug.Print("Object temperature : " + _ir.ReadTemperature(TemperatureSources.Object).ToString("F2"));
    ///
    ///            Thread.Sleep(Timeout.Infinite);
    ///        }
    ///		}
    /// </code>
    /// </example>
    public sealed class IRThermoClick : ITemperature
    {
        private readonly I2cDevice _ir;
        private readonly Hardware.Socket _socket;

        /// <summary>
        /// Initializes a new instance of the <see cref="IRThermoClick"/> class.
        /// </summary>
        /// <param name="socket">The socket on which the IRThermo Click board is plugged on MikroBus.Net board</param>
        /// <param name="address">The address of the display. Default to 0x5A.</param>
        public IRThermoClick(Hardware.Socket socket, Byte address = 0x5A)
        {
            _socket = socket;
#if (NANOFRAMEWORK_1_0)
            _ir = I2cDevice.Create(new I2cConnectionSettings(socket.I2cBus, address, I2cBusSpeed.StandardMode));
#else
			_ir = I2cController.FromName(socket.I2cBus).GetDevice(new I2cConnectionSettings(address, 100000));
#endif
        }

        /// <summary>
        /// Reads the temperature from the sensor.
        /// </summary>
        /// <param name="source">The source of the measurement : either "ambient" temperature or "object" temperature.</param>
        /// <returns>
        /// A single representing the temperature read from the source.
        /// </returns>
        /// <example>
        /// <code language="C#">
        ///     // Reads ambient temperature
        ///     Debug.Print ("Ambient temperature = "+_ir.ReadTemperature());
        /// 
        ///     // Reads object temperature
        ///     Debug.Print ("Object temperature = "+_ir.ReadTemperature(TemperatureSources.Object));
        /// </code>
        /// </example>
        public Single ReadTemperature(TemperatureSources source = TemperatureSources.Ambient)
        {
            var result = new Byte[3];

            lock (_socket.LockI2c)
            {
                _ir.WriteRead(new[] { source == TemperatureSources.Ambient ? (Byte)0x06 : (Byte)0x07 }, result);
            }

            RawData = (result[1] << 8) + result[0];
            var tempCelsius = (Single)((RawData*0.02) - 273.15);
            return TemperatureUnit == TemperatureUnits.Celsius ? tempCelsius : TemperatureUnit == TemperatureUnits.Fahrenheit ? (Single)((tempCelsius * (9.0 / 5)) + 32) : (Single)(tempCelsius + 273.15);
        }

        /// <summary>
        /// Gets or sets the temperature unit for the <seealso cref="ReadTemperature"/> method.
        /// <remarks><seealso cref="TemperatureUnits"/></remarks>
        /// </summary>
        /// <value>
        /// The temperature unit used.
        /// </value>
        /// <example>
        /// <code language="C#">
        ///     // Set temperature unit to Fahrenheit
        ///     _ir.TemperatureUnit = TemperatureUnits.Farhenheit;
        /// </code>
        /// </example>
        public TemperatureUnits TemperatureUnit { get; set; } = TemperatureUnits.Celsius;

        /// <summary>
        /// Gets the raw data associated with the temperature read.
        /// </summary>
        /// <value>
        /// Raw data in the range depending on sensor's precision (8/10/12 bits, for example). 
        /// It's a pure number that has no physical meaning by itself. Please use <see cref="ReadTemperature"/> for readings with known units.
        /// </value>
        /// <example>
        /// <code language="C#">
        ///      Debug.Print ("Raw data read from sensor : "+_ir.RawData);
        /// </code>
        /// </example>
        public Int32 RawData { get ; private set; }
    }
}

