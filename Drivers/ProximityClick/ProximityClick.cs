/*
 * Proximity Click board driver for TinyCLR 2.0
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
using System.Threading;

namespace MBN.Modules
{
    /// <summary>
    /// Main class for the Proximity Click board driver
    /// <para><b>Pins used :</b> Scl, Sda, Int</para>
    /// </summary>
    /// <example>
    /// <code language="C#">
    /// public class Program
    /// {
    ///     static ProximityClick _prox;
    /// 
    ///     public static void Main()
    ///     {
    ///         _prox = new ProximityClick(Hardware.SocketTwo);                   // Proximity at address 0x70 on socket 2
    /// 
    ///         Debug.Print("Chip revision : " + _prox.ChipRevision);             // Get chip version and firmware revision
    /// 
    ///         // Set IR Led current to 200 mA  (20 x 10). 
    ///         // Warning : different values of current will cause different readings for the same distance (see datasheet).
    ///         _prox.IRLedCurrent = 20;
    ///         _prox.ProximityRate = 1;     // Set Proximity rate measurement to 3.9 measures/s
    /// 
    ///         Debug.Print("Ambient light : " + _prox.AmbientLight());           // Get ambient light value
    /// 
    ///         while (true)
    ///         {
    ///             Debug.Print("Proximity : " + _prox.Distance());               // Get proximity value
    ///             Thread.Sleep(100);
    ///         }
    ///     }
    /// }
    /// </code>
    /// </example>
    public sealed partial class ProximityClick
    {
        private readonly I2cDevice _prox;      // I²C configuration
        private readonly Hardware.Socket _socket;

        /// <summary>
        /// Initializes a new instance of the <see cref="ProximityClick"/> class.
        /// </summary>
        /// <param name="socket">The socket on which the Proximity Click board is plugged on MikroBus.Net</param>
        /// <param name="address">Address of the I²C device.</param>
        public ProximityClick(Hardware.Socket socket, Byte address=0x13)
        {
            _socket = socket;
#if (NANOFRAMEWORK_1_0)
            _prox = I2cDevice.Create(new I2cConnectionSettings(socket.I2cBus, address, I2cBusSpeed.StandardMode));
#else
			_prox = I2cController.FromName(socket.I2cBus).GetDevice(new I2cConnectionSettings(address, 100000));
#endif
        }

        #region Private methods
        private Int32 ReadInteger(Byte register)
        {
            var result = new Byte[2];

            lock (_socket.LockI2c)
            {
                _prox.Write(new[] { register });
            }
            Thread.Sleep(5);
            lock (_socket.LockI2c)
            {
                _prox.Read(result);
            }

            return (result[0] << 8) + result[1];
        }

        private Byte ReadByte(Byte register, Byte mask = 0)
        {
            var result = new Byte[1];

            lock (_socket.LockI2c)
            {
                _prox.Write(new[] { register });
            }
            Thread.Sleep(5);
            lock (_socket.LockI2c)
            {
                _prox.Read(result);
            }

            return mask == 0 ? result[0] : (Byte)(result[0] & mask);
        }

        private void WriteByte(Byte register, Byte value=0, Byte mask=0)
        {
            lock (_socket.LockI2c)
            {
                _prox.Write(mask != 0 ? new[] { register, (Byte)(value & mask) } : new[] { register, value });
            }
        }

        private static Byte SetBit(Byte input, Byte bit, Boolean state) => state ? (Byte)(input | (Byte)(1 << bit)) : (Byte)(input & (Byte)(0xFF - (1 << bit)));
        #endregion

        /// <summary>
        /// Returns a measure of the ambient light.
        /// </summary>
        /// <returns>Int32 : ambient light measurement.</returns>
        /// <example>
        /// <code language="C#">
        ///     Debug.Print("Ambient light : " + _prox.AmbientLight());
        /// </code>
        /// </example>
        public Int32 AmbientLight()
        {
            var tmp = SetBit(ReadByte(Registers.Command), 4, true);                // Sets ALS on-demand bit
            tmp = SetBit(tmp, 0, false);                                            // Clear self-timed measurement bit
            tmp = SetBit(tmp, 2, false);
            WriteByte(Registers.Command, tmp );                    // Clear ALS periodic measurement and send command
            while (ReadByte(Registers.Command, 0x40) != 0x40) { }

            return ReadInteger(Registers.AmbientLightResult);                       // Register #5 and #6 Ambient Light Result Register
        }

        /// <summary>
        /// Measure the proximity of an object.
        /// </summary>
        /// <remarks>Warning : value returned by this function is based on a logarithmic scale !
        /// <para>Also, different settings for IR Led current will change the value returned for the same distance.</para>
        /// </remarks>
        /// <returns>Int32 : proximity of the object. The higher the value, the closer the object.</returns>
        /// <example>
        /// <code language="C#">
        ///     Debug.Print("Proximity : " + _prox.Distance());
        /// </code>
        /// </example>
        public Int32 Distance()
        {
            var tmp = SetBit(ReadByte(Registers.Command), 3, true);                // Sets proximity on-demand bit
            tmp = SetBit(tmp, 0, false);                                            // Clear self-timed measurement bit
            WriteByte(Registers.Command, SetBit(tmp, 1, false));                    // Clear proximity periodic measurement and send command
            while (ReadByte(Registers.Command, 0x20) != 0x20) { }                  // Wait for proximity data ready flag
            return ReadInteger(Registers.ProximityMeasurementResult);                     // Register #7 and #8 Proximity Measurement Result Register
        }

        /// <summary>
        /// Gets or sets the proximity IR test signal frequency.
        /// The proximity measurement is using a square IR signal as measurement signal. Four different values are possible:
        /// <para>0 = 390.625 kHz (DEFAULT)</para>
        /// <para>1 = 781.25 kHz</para>
        /// <para>2 = 1.5625 MHz</para>
        /// <para>3 = 3.125 MHz</para>
        /// </summary>
        /// <value>
        /// Value from 0 to 3.
        /// </value>
        /// <example>
        /// <code language="C#">
        ///     // Sets the IR frequency to 1.5625 MHz
        ///     _prox.ProximityFrequency = 2;
        /// </code>
        /// </example>
        public Byte ProximityFrequency
        {
            get { return (Byte)(ReadByte(Registers.ProximityModulator, 0x18) >> 3); }
            set { WriteByte(Registers.ProximityModulator, (Byte)((ReadByte(Registers.ProximityModulator, 0x18) & 0xE7) | (value << 3))); }  // Register #15 Proximity Modulator Timing Adjustment
        }

        /// <summary>
        /// This function can be used for performing faster ambient light measurements.
        /// <para>Disable = False (DEFAULT)</para>
        /// <para>Enable = True</para>
        /// </summary>
        /// <value>
        /// True or False
        /// </value>
        /// <example>
        /// <code language="C#">
        ///     // Enables continuous conversion mode
        ///     _prox.AmbientLightContinuousConversionMode = true;
        /// </code>
        /// </example>
        public Boolean AmbientLightContinuousConversionMode
        {
            get { return ReadByte(Registers.AmbientLightParameter, 0x80) == 0x80; }     // Register #4 Ambient Light Parameter Register
            set
            {
                var oldByte = ReadByte(Registers.AmbientLightParameter);
                WriteByte(Registers.AmbientLightParameter, value ? (Byte)(oldByte | 0x80) : (Byte)(oldByte & 0x7F));
            }
        }

        /// <summary>
        /// Ambient light measurement rate, according to the following values :
        ///<para>0 : 1 samples/s</para>
        ///<para>1 : 2 samples/s = DEFAULT</para>
        ///<para>2 : 3 samples/s</para>
        ///<para>3 : 4 samples/s</para>
        ///<para>4 : 5 samples/s</para>
        ///<para>5 : 6 samples/s</para>
        ///<para>6 : 8 samples/s</para>
        ///<para>7 : 10 samples/s</para>
        /// </summary>
        /// <value>
        /// 0 to 7
        /// </value>
        /// <example>
        /// <code language="C#">
        ///     // Sets the number of samples to 5 samples/s
        ///     _prox.AmbientLightMeasurementRate = 4;
        /// </code>
        /// </example>
        public Byte AmbientLightMeasurementRate
        {
            get { return (Byte)(ReadByte(Registers.AmbientLightParameter, 0x70) >> 4); }       // Register #4 Ambient Light Parameter Register
            set { WriteByte(Registers.AmbientLightParameter, (Byte)((ReadByte(Registers.AmbientLightParameter, 0x70) & 0x8F) | (value << 4))); }
        }

        /// <summary>
        /// Averaging function (number of measurements per run)
        ///<para>Bit values sets the number of single conversions done during one measurement cycle. Result is the average value of all conversions.</para>
        ///<para>Number of conversions = 2^value e.g. 0 = 1 conv., 1 = 2 conv, 2 = 4 conv., ….7 = 128 conv.</para>
        ///<para>DEFAULT = 32 conv. (bit 2 to bit 0: 101)</para>
        /// </summary>
        /// <value>
        /// 0 to 7
        /// </value>
        /// <example>
        /// <code language="C#">
        ///     // Averaging on 1 measure only
        ///     _prox.AmbientLightAveragingFunction = 0;
        /// </code>
        /// </example>
        public Byte AmbientLightAveragingFunction
        {
            get { return ReadByte(Registers.AmbientLightParameter, 0x07); }                    // Register #4 Ambient Light Parameter Register
            set { WriteByte(Registers.AmbientLightParameter, (Byte)(ReadByte(Registers.AmbientLightParameter) | (value & 0x07))); }
        }

        /// <summary>
        /// Automatic offset compensation.
        /// <para><c>Enable</c> = True (DEFAULT), <c>Disable</c> = False</para>
        /// <para>In order to compensate a technology, package or temperature related drift of the ambient light values there is a built in automatic offset compensation function.</para>
        /// <para>With active auto offset compensation the offset value is measured before each ambient light measurement and subtracted automatically from actual reading.</para>
        /// </summary>
        /// <value>
        /// True or False
        /// </value>
        /// <example>
        /// <code language="C#">
        ///     // Disables auto offset compensation
        ///     _prox.AmbientLightAutoOffsetCompensation = false;
        /// </code>
        /// </example>
        public Boolean AmbientLightAutoOffsetCompensation
        {
            get { return ReadByte(Registers.AmbientLightParameter, 0x08) == 0x08; }     // Register #4 Ambient Light Parameter Register
            set 
            {
                var oldByte = ReadByte(Registers.AmbientLightParameter);
                WriteByte(Registers.AmbientLightParameter, value ? (Byte)(oldByte | 0x08) : (Byte)(oldByte & 0xF7)); 
            }
        }

        /// <summary>
        /// Rate of Proximity Measurement (no. of measurements per second)
        /// <para>0 : 1.95 measurements/s (DEFAULT)</para>
        /// <para>1 : 3.90625 measurements/s</para>
        /// <para>2 : 7.8125 measurements/s</para>
        /// <para>3 : 16.625 measurements/s</para>
        /// <para>4 : 31.25 measurements/s</para>
        /// <para>5 : 62.5 measurements/s</para>
        /// <para>6 : 125 measurements/s</para>
        /// <para>7 : 250 measurements/s</para>
        /// </summary>
        /// <remarks>The value returned by the proximity measurement for the same distance will vary depending on the selected rate.</remarks>
        /// <example>
        /// <code language="C#">
        ///     // Sets the proximity rate to 3.90625 measurements/s
        ///     _prox.ProximityRate = 1;
        /// </code>
        /// </example>
        public Byte ProximityRate
        {
            get { return ReadByte(Registers.ProximityRate, 0x07); }            // Register #2 Rate of Proximity Measurement
            set { WriteByte(Registers.ProximityRate, value, 0x07); }
        }

        /// <summary>
        /// Sets the LED current value for proximity measurement. The value is adjustable in steps of 10 mA from 0 mA to 200 mA.
        /// <para>IR LED current = Value (dec.) x 10 mA.</para>
        /// <para>Valid Range = 0 to 20d. e.g. 0 = 0 mA , 1 = 10 mA, …., 20 = 200 mA (2 = 20 mA = DEFAULT)</para>
        /// <para>LED Current is limited to 200 mA for values higher as 20d.</para>
        /// </summary>
        /// <remarks>The value returned by the proximity measurement for the same distance will vary depending on the selected current value.</remarks>
        /// <example>
        /// <code language="C#">
        ///     // Sets the IR led current to 30 mA
        ///     _prox.IRLedCurrent = 3;
        /// </code>
        /// </example>
        public Byte IRLedCurrent
        {
            get { return (Byte)(ReadByte(Registers.IRLedCurrent, 0x3F) * 10); }           // Register #3 LED Current Setting for Proximity Mode
            set { WriteByte(Registers.IRLedCurrent, (Byte)(value / 10), 0x3F); }
        }

        /// <summary>
        /// Returns the Product ID and revision of the firmware.
        /// </summary>
        /// <example>
        /// <code language="C#">
        ///     // Gets the chip revision info
        ///     Debug.Print ("Chip revision : "+_prox.ChipRevision);
        /// </code>
        /// </example>
        public Version ChipRevision
        {
            get
            {
                var tmp = ReadByte(Registers.ProductIDRevision);
                return new Version((tmp & 0xF0) >> 4, tmp & 0x0F);      // Register #1 Product ID Revision Register
            }
        }
    }
}

