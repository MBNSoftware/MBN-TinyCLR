/*
 * Proximity 3 Click board driver TinyCLR 2.0
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
    /// <summary>Main class for the Proximity 3 Click driver</summary>
    public class Proximity3Click
    {
        #region Enums
        /// <summary>
        /// Output resolution
        /// </summary>
        public enum Resolution
        {
            _12bits,
            _16bits
        }
        /// <summary>
        /// Integration times
        /// </summary>
        public enum IntegrationTimes
        {
            _50ms,
            _100ms,
            _200ms,
            _400ms
        }
        #endregion

        #region Private variables
        private readonly I2cDevice _prox;
        private readonly Byte[] _result = new Byte[2];
        private readonly Hardware.Socket _socket;
        private Byte[] _conv = new Byte[2];
        private Byte _alsConf = 0b01000000;
        private Byte _psConf1 = 0b00101010;
        private Byte _psConf2 = 0b00001011;
        #endregion

        /// <summary>Initializes a new instance of the <see cref="Proximity3Click" /> class.</summary>
        /// <param name="socket">The socket on which the ADC3 Click board is plugged on MikroBus.Net</param>
        /// <param name="address">The I2C address of the module</param>
        public Proximity3Click(Hardware.Socket socket, Byte address = 0x51)
        {
            _socket = socket;
#if (NANOFRAMEWORK_1_0)
            _prox = I2cDevice.Create(new I2cConnectionSettings(socket.I2cBus, address, I2cBusSpeed.StandardMode));
#else
			_prox = I2cController.FromName(socket.I2cBus).GetDevice(new I2cConnectionSettings(address, 100000));
#endif

            if (ChipRevision.Major == 0x10 & ChipRevision.Minor == 0x58)
            {
                _prox.Write(new Byte[] { ALS_CONF_REG, _alsConf, 0b00000000 });
                _prox.Write(new Byte[] { PS_CONF1_CONF2_REG, _psConf1, _psConf2 });
                _prox.Write(new Byte[] { PS_CONF3_MS_REG, 0b01110000, 0b00000111 });
                WriteInteger(PS_THDL_REG, 5000);
                WriteInteger(PS_THDH_REG, 12000);
                WriteInteger(ALS_THDL_REG, 500);
                WriteInteger(ALS_THDH_REG, 5000);

            }
            else
                throw new Exception("Proximity 3 module not detected");
        }

        #region Public methods/properties

        /// <summary>
        /// Gets or sets the ambient light sensor low interrupt threshold.
        /// </summary>
        /// <value>
        /// The als low interrupt threshold.
        /// </value>
        public Int16 AlsLowThreshold
        {
            get
            {
                return (Int16)ReadInteger(ALS_THDL_REG);
            }
            set
            {
                WriteInteger(ALS_THDL_REG, value);
            }
        }

        /// <summary>
        /// Gets or sets the ambient light sensor high interrupt threshold.
        /// </summary>
        /// <value>
        /// The als high interrupt threshold.
        /// </value>
        public Int16 AlsHighThreshold
        {
            get
            {
                return (Int16)ReadInteger(ALS_THDH_REG);
            }
            set
            {
                WriteInteger(ALS_THDH_REG, value);
            }
        }

        /// <summary>
        /// Gets or sets the proximity low interrupt threshold.
        /// </summary>
        /// <value>
        /// The ps low interrupt threshold.
        /// </value>
        public Int16 PsLowThreshold
        {
            get
            {
                return (Int16)ReadInteger(PS_THDL_REG);
            }
            set
            {
                WriteInteger(PS_THDL_REG, value);
            }
        }

        /// <summary>
        /// Gets or sets the proximity sensor high interrupt threshold.
        /// </summary>
        /// <value>
        /// The ps high interrupt threshold.
        /// </value>
        public Int16 PsHighThreshold
        {
            get
            {
                return (Int16)ReadInteger(PS_THDH_REG);
            }
            set
            {
                WriteInteger(PS_THDH_REG, value);
            }
        }

        /// <summary>
        /// Gets or sets the integration time.
        /// </summary>
        /// <value>
        /// The integration time. <seealso cref="IntegrationTimes"/>
        /// </value>
        public IntegrationTimes IntegrationTime
        {
            get
            {
                return (IntegrationTimes)((_alsConf & 0b11000000) >> 6);
            }
            set
            {
                _alsConf &= 0b00111111;
                _alsConf |= (Byte)((Byte)value << 6);
                lock (_socket.LockI2c)
                {
                    _prox.Write(new Byte[] { ALS_CONF_REG, _alsConf, 0b00000000 });
                }
            }
        }

        /// <summary>
        /// Gets or sets the ambient light sensor power mode.
        /// </summary>
        /// <value>
        /// The als power mode (<see cref="PowerModes"/>)
        /// </value>
        public PowerModes AlsPowerMode
        {
            get
            {
                return (_alsConf & 0b00000001) == 0 ? PowerModes.On : PowerModes.Off;
            }
            set
            {
                if (value == PowerModes.On)
                    _alsConf &= 0b11111110;
                else
                    _alsConf |= 0b00000001;
                lock (_socket.LockI2c)
                {
                    _prox.Write(new Byte[] { ALS_CONF_REG, _alsConf, 0b00000000 });
                }
            }
        }

        /// <summary>
        /// Gets or sets the proximity sensor power mode.
        /// </summary>
        /// <value>
        /// The als power mode (<see cref="PowerModes"/>)
        /// </value>
        public PowerModes PsPowerMode
        {
            get
            {
                return (_psConf1 & 0b00000001) == 0 ? PowerModes.On : PowerModes.Off;
            }
            set
            {
                if (value == PowerModes.On)
                    _psConf1 &= 0b11111110;
                else
                    _psConf1 |= 0b00000001;
                lock (_socket.LockI2c)
                {
                    _prox.Write(new Byte[] { PS_CONF1_CONF2_REG, _psConf1, _psConf2 });
                }
            }
        }

        /// <summary>
        /// Gets or sets the proximity sensor resolution.
        /// </summary>
        /// <value>
        /// The ps resolution (<see cref="Resolution"/>)
        /// </value>
        public Resolution PsResolution
        {
            get
            {
                return (_psConf2 & 0b00001000) == 0 ? Resolution._12bits : Resolution._16bits;
            }
            set
            {
                if (value == Resolution._12bits)
                    _psConf2 &= 0b11110111;
                else
                    _psConf2 |= 0b00001000;
                lock (_socket.LockI2c)
                {
                    _prox.Write(new Byte[] { PS_CONF1_CONF2_REG, _psConf1, _psConf2 });
                }
            }
        }

        /// <summary>
        /// Gets the chip revision.
        /// </summary>
        /// <value>
        /// The chip revision.
        /// </value>
        public Version ChipRevision
        {
            get
            {
                var tmp = ReadBytes(DEVICEID_REG);
                return new Version(tmp[1], tmp[0]);      // Register #1 Product ID Revision Register
            }
        }

        /// <summary>
        /// Gets the ambient light value
        /// </summary>
        /// <value>
        /// The ambient light.
        /// </value>
        public Int16 AmbientLight => (Int16)ReadInteger(AMBIENT_REG);

        /// <summary>
        /// Gets the distance value
        /// </summary>
        /// <value>
        /// The distance.
        /// </value>
        public Int32 Distance => ReadInteger(PROXIMITY_REG);

        #endregion

        #region Private methods
        private Int32 ReadInteger(Byte register)
        {
            _prox.WriteRead(new Byte[] { register }, _result);

            return (_result[1] << 8) + _result[0];
        }

        private Byte[] ReadBytes(Byte register)
        {
            _prox.WriteRead(new Byte[] { register }, _result);

            return new[] { _result[0], _result[1] };
        }

        private void WriteInteger(Byte register, Int16 value)
        {
            _conv = System.BitConverter.GetBytes(value); //TODO: requires "System" as "MBN"  overrides it!
            _prox.Write(new Byte[] { register, _conv[0], _conv[1] });
        }
        #endregion

        #region Registers        
        /// <summary>
        /// ALS integration time, persistence, interrupt, and function enable / disable
        /// </summary>
        public const Byte ALS_CONF_REG = 0x00;
        /// <summary>
        /// ALS high interrupt threshold
        /// </summary>
        public const Byte ALS_THDH_REG = 0x01;
        /// <summary>
        /// ALS low interrupt threshold
        /// </summary>
        public const Byte ALS_THDL_REG = 0x02;
        /// <summary>
        /// PS duty ratio, integration time, persistence, and PS enable / disable
        /// </summary>
        public const Byte PS_CONF1_CONF2_REG = 0x03;
        /// <summary>
        /// PS multi pulse, active force mode, sunlight feature
        /// </summary>
        public const Byte PS_CONF3_MS_REG = 0x04;
        /// <summary>
        /// PS cancellation level setting
        /// </summary>
        public const Byte PS_CANC_REG = 0x05;
        /// <summary>
        /// PS low interrupt threshold setting
        /// </summary>
        public const Byte PS_THDL_REG = 0x06;
        /// <summary>
        /// PS high interrupt threshold setting
        /// </summary>
        public const Byte PS_THDH_REG = 0x07;
        /// <summary>
        /// Proximity sensor output data
        /// </summary>
        public const Byte PROXIMITY_REG = 0x08;
        /// <summary>
        /// The ambient light sensor output data
        /// </summary>
        public const Byte AMBIENT_REG = 0x09;
        /// <summary>
        /// White data
        /// </summary>
        public const Byte WHITE_REG = 0x0A;
        /// <summary>
        /// ALS and PS interrupt flags
        /// </summary>
        public const Byte INT_FLAG_REG = 0x0D;
        /// <summary>
        /// Device ID
        /// </summary>
        public const Byte DEVICEID_REG = 0x0E;
        #endregion
    }
}
