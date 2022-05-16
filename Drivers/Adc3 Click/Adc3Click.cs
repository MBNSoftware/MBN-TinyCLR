/*
 * ADC3 Click board driver TinyCLR 2.0
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
    /// <summary>Main class for the ADC3 Click driver</summary>
    public class Adc3Click
    {
        private readonly I2cDevice _adc;
        private readonly Byte[] _configRegister = new Byte[1];
        private readonly Byte[] _data = new Byte[2];
        private Int32 _value;
        private readonly Hardware.Socket _socket;

        /// <summary>
        /// Available channels on the module
        /// </summary>
        public enum Channels
        {
            Channel1,
            Channel2,
            Channel3,
            Channel4
        }

        /// <summary>
        /// Conversion modes
        /// </summary>
        public enum ConversionModes
        {
            OneShot,
            Continuous
        }

        /// <summary>
        /// Available sample rates
        /// </summary>
        public enum SampleRates
        {
            _12bits,
            _14bits,
            _16bits
        }

        /// <summary>
        /// Available gain multipliers
        /// Warning : since the MCP3428 is measuring (+/-)2.048V max, then the higher the gain, the lower the max voltage measured but higher precision
        /// </summary>
        public enum GainSelection
        {
            x1,
            x2,
            x4,
            x8
        }

        /// <summary>Initializes a new instance of the <see cref="Adc3Click" /> class.</summary>
        /// <param name="socket">The socket on which the ADC3 Click board is plugged on MikroBus.Net</param>
        /// <param name="address">The I2C address of the module</param>
        public Adc3Click(Hardware.Socket socket, Int32 address = 0x68)
        {
            _socket = socket;
#if (NANOFRAMEWORK_1_0)
            _adc = I2cDevice.Create(new I2cConnectionSettings(socket.I2cBus, address, I2cBusSpeed.StandardMode));
#else
            _adc = I2cController.FromName(socket.I2cBus).GetDevice(new I2cConnectionSettings(address, 100000));
#endif

            // Default power on configuration :
            // - Conversion not completed
            // - Channel 1
            // - Continuous conversion mode
            // - Sample rate 12 bits
            // - Gain x1
            _configRegister[0] = 0b10010000;
        }

        /// <summary>
        /// Reads the voltage according to current parameters (channel, gain, sample rate)
        /// </summary>
        /// <returns>A double indicating the voltage in V.</returns>
        public Double GetVoltage()
        {
            ReadData();
            _value = (_data[0] << 8) + _data[1];

            switch (SampleRate)
            {
                case SampleRates._12bits:
                    return (_data[0] & 0b00001000) == 0 ? (_value >> (Byte)Gain) / 1000.0 : (((UInt16)~_value) >> (Byte)Gain) / -1000.0;   // Resolution is 1mV
                case SampleRates._14bits:
                    return (_data[0] & 0b00100000) == 0 ? (_value >> (Byte)Gain) / 4000.0 : (((UInt16)~_value) >> (Byte)Gain) / -4000.0;   // Resolution is 250µV
                case SampleRates._16bits:
                    return (_data[0] & 0b10000000) == 0 ? (_value >> (Byte)Gain) / 16000.0 : (((UInt16)~_value) >> (Byte)Gain) / -16000.0; // Resolution is 62.5µV
                default:
                    return 0.0;
            }
        }

        private void ReadData()
        {
            lock (_socket.LockI2c)
            {
                _adc.Read(_data);
            }
        }

        private void WriteControlRegister()
        {
            lock (_socket.LockI2c)
            {
                _adc.Write(_configRegister);
            }
        }

        /// <summary>
        /// Gets or sets the conversion mode.
        /// </summary>
        /// <value>
        /// The current conversion mode
        /// </value>
        public ConversionModes ConversionMode
        {
            get => (ConversionModes)((_configRegister[0] & 0b00010000) >> 4);
            set
            {
                _configRegister[0] |= (Byte)((Byte)value << 4);
                WriteControlRegister();
            }
        }

        /// <summary>
        /// Gets or sets the gain
        /// </summary>
        /// <value>
        /// The current gain
        /// </value>
        public GainSelection Gain
        {
            get => (GainSelection)(_configRegister[0] & 0b00000011);
            set
            {
                _configRegister[0] |= (Byte)value;
                WriteControlRegister();
            }
        }

        /// <summary>
        /// Gets or sets the sample rate
        /// </summary>
        /// <value>
        /// The current sample rate
        /// </value>
        public SampleRates SampleRate
        {
            get => (SampleRates)((_configRegister[0] & 0b00001100) >> 2);
            set
            {
                _configRegister[0] |= (Byte)((Byte)value << 2);
                WriteControlRegister();
            }
        }

        /// <summary>
        /// Gets or sets the channel to read
        /// </summary>
        /// <value>
        /// The current channel
        /// </value>
        public Channels Channel
        {
            get => (Channels)((_configRegister[0] & 0b01100000) >> 5);
            set
            {
                _configRegister[0] |= (Byte)((Byte)value << 5);
                WriteControlRegister();
            }
        }
    }
}
