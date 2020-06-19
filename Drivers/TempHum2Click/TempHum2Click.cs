/*
 * TempHum2 Click TinyCLR driver for MikroBUS.Net
 * 
 * Initial revision coded by Stephen Cardinale
 * 
 * Copyright 2020 Stephen Cardinale and MikroBUS.Net
 *
 * Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License. You may obtain a copy of the License at
 * http://www.apache.org/licenses/LICENSE-2.0
 * Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, 
 * either express or implied. See the License for the specific language governing permissions and limitations under the License.
 */

using GHIElectronics.TinyCLR.Devices.I2c;

using System;
using System.Threading;

namespace MBN.Modules
{
    public sealed class TempHum2Click : ITemperature, IHumidity
    {
        #region .ctor

        /// <summary>
        /// Initializes a new instance of the TempHum 2 Click driver.
        /// </summary>
        /// <param name="socket">The socket in which the TempHum 2 click is inserted on the Quail mainboard.</param>
        public TempHum2Click(Hardware.Socket socket)
        {

            _sensor = I2cController.FromName(socket.I2cBus).GetDevice(new I2cConnectionSettings( 0x70, 100000));

            Thread.Sleep(50); // Time fromm VDD >= 1.64V until ready for a full conversion.

            Reset(ResetModes.Soft);

            TemperatureUnit = TemperatureUnits.Celsius;
            _speedMode = SpeedModes.Normal;
            _operatingMode = OperatingModes.Hold;
            _dataRegister = Si7034_CMD_READ_TEMPHUM_HOLDMODE_NORMAL_MODE;
        }

        #endregion

        #region Private Fields

        private static I2cDevice _sensor;
        private static SpeedModes _speedMode;
        private static OperatingModes _operatingMode;
        private UInt16 _dataRegister;

        #endregion

        #region Private Constants

        private const UInt16 Si7034_CMD_READ_ELECTRONIC_ID1 = 0xFA0F;
        private const UInt16 Si7034_CMD_READ_ELECTRONIC_ID2 = 0xFCC9;
        private const Byte Si7034_CMD_RESET = 0xFE;
        private const UInt16 Si7034_CMD_QUERY_DEVICE_WITH_RESPONSE = 0xEFC8;
        private const Byte Si7034_CMD_WRITE_HEATER_CONTROL_REGISTER = 0xE6;
        private const Byte Si7034_CMD_READ_HEATER_CONTROL_REGISTER = 0xE7;
        private const UInt16 Si7034_CMD_READ_FIRMWARE_REVISION = 0x84F1;
        private const UInt16 Si7034_CMD_READ_TEMPHUM_HOLDMODE_NORMAL_MODE = 0x7CA2;
        private const UInt16 Si7034_CMD_READ_TEMPHUM_NONHOLDMODE_NORMAL_MODE = 0x7866;
        private const UInt16 Si7034_CMD_READ_TEMPHUM_HOLDMODE_FAST_MODE = 0x6458;
        private const UInt16 Si7034_CMD_READ_TEMPHUM_NONHOLDMODE_FAST_MODE = 0x609C;

        #endregion

        #region Public ENUMS

        /// <summary>
        /// The Si7034-A10 can read the termperature and humidity in either Fast mode or Normal mode.
        /// </summary>
        public enum SpeedModes
        {
            /// <summary>
            /// In Normal Mode, the Si7034-A10 reads the temperature sequenctially. Temperature is read first then the humidity is measured.
            /// </summary>
            Normal = 0,

            /// <summary>
            /// In fast mode, the Si7034-A10 measures both temperature and humidity concurrently, thus speeding up the conversion time.
            /// </summary>
            Fast = 1
        }

        /// <summary>
        /// The Si7034-A10 on the TempHum 2 Click can operate in Hold Mode with clock stretching or NoHold Mode.
        /// The only difference is with the NoHold mode, the I2C Write/Read methods need to wait a predescribed time to poll the Si7034-A10.
        /// </summary>
        public enum OperatingModes
        {
            /// <summary>
            /// Hold mode with clock stretching.
            /// </summary>
            Hold = 0,

            /// <summary>
            /// No hold mode with I2C Write/Read methods needs to wait a predescribed time to poll the Si7034-A10
            /// </summary>
            NoHold = 1
        }

        /// <summary>
        ///     Used to control heating output when the heat function is turned on.
        /// </summary>
        public enum HeaterCurrentSettings
        {
            /// <summary>Heating element is supplied with 6.4 mA of current. This is the power on default.</summary>
            MA_6_4 = 0x00,

            /// <summary>Heating element is supplied with 9.7 mA of current.</summary>
            MA_9_7 = 0x01,

            /// <summary>Heating element is supplied with 13.1 mA of current.</summary>
            MA_13_1 = 0x02,

            /// <summary>Heating element is supplied with 15.82 mA of current.</summary>
            MA_15_82 = 0x03,

            /// <summary>Heating element is supplied with 19.6 mA of current.</summary>
            MA_19_6 = 0x04,

            /// <summary>Heating element is supplied with 22.74 mA of current.</summary>
            MA_22_74 = 0x05,

            /// <summary>Heating element is supplied with 25.88 mA of current.</summary>
            MA_25_88 = 0x06,

            /// <summary>Heating element is supplied with 29.02 mA of current.</summary>
            MA_29_02 = 0x07,

            /// <summary>Heating element is supplied with 32.4 mA of current.</summary>
            MA_32_4 = 0x08,

            /// <summary>Heating element is supplied with 35.3 mA of current.</summary>
            MA_35_3 = 0x09,

            /// <summary>Heating element is supplied with 38.44 mA of current.</summary>
            MA_38_44 = 0x0A,

            /// <summary>Heating element is supplied with 41.58 mA of current.</summary>
            MA_41_58 = 0x0B,

            /// <summary>Heating element is supplied with 44.72 mA of current.</summary>
            MA_44_72 = 0x0C,

            /// <summary>Heating element is supplied with 47.86 mA of current.</summary>
            MA_47_86 = 0x0D,

            /// <summary>Heating element is supplied with 51.0 mA of current.</summary>
            MA_51_0 = 0x0E,

            /// <summary>Heating element is supplied with 53.5 mA of current.</summary>
            MA_53_5 = 0x0F
        }

        #endregion

        #region Private Methods

        private static void WriteByte(Byte registerAddress, Byte value)
        {
            lock (Hardware.LockI2C)
            {
                Byte[] writeBuffer = { registerAddress, value };
                _sensor.Write(writeBuffer);
            }
        }

        private static void WriteByte(Byte registerAddress)
        {
            lock (Hardware.LockI2C)
            {
                Byte[] writeBuffer = { registerAddress };
                _sensor.Write(writeBuffer);
            }
        }

        private static Byte ReadByte(Byte registerAddress)
        {
            lock (Hardware.LockI2C)
            {
                Byte[] writeBuffer = { registerAddress };
                Byte[] registerData = new Byte[1];
                _sensor.WriteRead(writeBuffer, registerData);
                return registerData[0];
            }
        }

        private static Byte ReadRegister(UInt16 registerAddress)
        {
            lock (Hardware.LockI2C)
            {
                Byte[] writeBuffer = { (Byte)(registerAddress >> 8), (Byte)(registerAddress & 0x00FF) };
                Byte[] registerData = new Byte[1];
                _sensor.WriteRead(writeBuffer, registerData);
                return registerData[0];
            }
        }

        private static Byte[] ReadRegister(UInt16 registerAddress, Byte numberOfBytesToRead)
        {
            Byte[] writeBuffer = { (Byte)(registerAddress >> 8), (Byte)(registerAddress & 0x00FF) };
            Byte[] registerData = new Byte[numberOfBytesToRead];

            lock (Hardware.LockI2C)
            {
                _sensor.Write(writeBuffer);

                if (_operatingMode == OperatingModes.NoHold)
                {
                    Thread.Sleep(_speedMode == SpeedModes.Normal ? 15 : 5);
                }

                _sensor.Read(registerData);

                return registerData;
            }
        }

        private static Byte CalculateCRC8(Byte[] data)
        {
            const Int16 polynomial = 0x131; // P(x) = 2^8 + 2^5 + 2^4 + 1
            Byte crc = 0xFF; // Initialize the CRC to 0xFF

            // Iterate through each byte in the passed data
            for (Byte byteCounter = 0; byteCounter < data.Length; byteCounter++)
            {
                crc ^= data[byteCounter];

                for (Byte bit = 0; bit <= 7; bit++) // Once for each bit in the Byte
                {
                    crc = (crc & 0x80) == 0x80 ? (Byte)((crc << 1) ^ polynomial) : (Byte)(crc << 1);
                }
            }

            return crc;
        }

        private Single ScaleTemperature(Single temperature)
        {
            switch (TemperatureUnit)
            {
                case TemperatureUnits.Celsius:
                {
                    return temperature;
                }

                case TemperatureUnits.Fahrenheit:
                {
                    return temperature * 1.8F + 32F;
                }

                case TemperatureUnits.Kelvin:
                {
                    return temperature + 273.15F;
                }

                default:
                {
                    return temperature;
                }
            }
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets the firmware revision of the Si7034-A10.
        /// </summary>
        public Byte FirmwareRevision => ReadRegister(Si7034_CMD_READ_FIRMWARE_REVISION);

        /// <summary>
        /// Gets the Si7034-A10 unique serial number.
        /// </summary>
        public Int32 DeviceSerialNumber
        {
            get
            {
                Byte[] readSNAReg = ReadRegister(Si7034_CMD_READ_ELECTRONIC_ID1, 8);

                if (CalculateCRC8(new[] { readSNAReg[0] }) != readSNAReg[1]) return Int32.MaxValue;
                if (CalculateCRC8(new[] { readSNAReg[2] }) != readSNAReg[3]) return Int32.MaxValue;
                if (CalculateCRC8(new[] { readSNAReg[4] }) != readSNAReg[5]) return Int32.MaxValue;
                if (CalculateCRC8(new[] { readSNAReg[6] }) != readSNAReg[7]) return Int32.MaxValue;

                Int32 serialNumber = readSNAReg[0];
                serialNumber <<= 8;
                serialNumber |= readSNAReg[2];
                serialNumber <<= 8;
                serialNumber |= readSNAReg[4];
                serialNumber <<= 8;
                serialNumber |= readSNAReg[6];
                serialNumber <<= 8;

                Byte[] readSNBReg = ReadRegister(Si7034_CMD_READ_ELECTRONIC_ID2, 6);

                if (CalculateCRC8(new[] { readSNBReg[1] }) != readSNAReg[2]) return Int32.MaxValue;
                if (CalculateCRC8(new[] { readSNBReg[4] }) != readSNBReg[5]) return Int32.MaxValue;

                serialNumber |= readSNBReg[0];
                serialNumber <<= 8;
                serialNumber |= readSNBReg[1];
                serialNumber <<= 8;
                serialNumber |= readSNBReg[3];
                serialNumber <<= 8;
                serialNumber |= readSNBReg[4];

                return serialNumber <<= 8;
            }
        }

        /// <summary>
        /// Enables or disables the on-die heating element of the Si7034-A10 IC.
        /// </summary>
        /// <remarks>Enabling the heating curcuit is useful in driving off condensation or to implement a dew-point measurement or to test the module. </remarks>
        public Boolean HeaterEnable
        {
            get
            {
                Byte registerData = ReadByte(Si7034_CMD_READ_HEATER_CONTROL_REGISTER);
                return Bits.IsBitSet(registerData, 4);
            }
            set
            {
                Byte registerData = ReadByte(Si7034_CMD_READ_HEATER_CONTROL_REGISTER);
                Bits.Set(ref registerData, 4, value);
                WriteByte(Si7034_CMD_WRITE_HEATER_CONTROL_REGISTER, registerData);
            }
        }

        /// <summary>
        /// Gets or sets the current to apply to the on-die heating circuitry.
        /// </summary>
        public HeaterCurrentSettings HeaterCurrent
        {
            get
            {
                Byte registerData = ReadByte(Si7034_CMD_READ_HEATER_CONTROL_REGISTER);
                return (HeaterCurrentSettings) (registerData & 0x0F);
            }
            set
            {
                Byte registerData = ReadByte(Si7034_CMD_READ_HEATER_CONTROL_REGISTER);
                registerData ^= 0x0F;
                registerData |= (Byte) value;
                WriteByte(Si7034_CMD_WRITE_HEATER_CONTROL_REGISTER, registerData);
            }
        }

        /// <summary>
        /// Gets or sets the unit of measure for reporting the temperature. Defaults to Degrees C.
        /// </summary>
        public TemperatureUnits TemperatureUnit { get; set; }

        #endregion

        #region Public Methods

        /// <summary>
        /// I have no idea on what this is supposed to do but it queries the device and returns true or false.
        /// Maybe useful in identifying the TempHu 2 Click in a multi-device configuration.
        /// </summary>
        public Boolean QueryDevice()
        {
            Byte[] registerData = ReadRegister(Si7034_CMD_QUERY_DEVICE_WITH_RESPONSE, 3);

            Int32 readData = registerData[0];
            readData <<= 8;
            readData |= registerData[1];
            readData <<= 8;
            readData |= registerData[2];

            return readData == 0x0000472B;
        }

        /// <summary>
        /// Sets the mechanism to measure the temperature and humidity with.
        /// </summary>
        /// <param name="operatingMode">The TempHum2 Click operates in two modes of operation. Hold and NoHold modes. See <see cref="OperatingModes"/> for an explanation of each type.</param>
        /// <param name="speedMode">The TempHum2 Click operates has two speed modes. Normal and Fast mode.  See <see cref="SpeedModes"/> for an explanation of each type.</param>
        public void SetMode(OperatingModes operatingMode, SpeedModes speedMode)
        {
            _speedMode = speedMode;
            _operatingMode = operatingMode;

            switch (_speedMode)
            {
                case SpeedModes.Normal when _operatingMode == OperatingModes.Hold:
                {
                    _dataRegister = Si7034_CMD_READ_TEMPHUM_HOLDMODE_NORMAL_MODE;
                    break;
                }

                case SpeedModes.Normal when _operatingMode == OperatingModes.NoHold:
                {
                    _dataRegister = Si7034_CMD_READ_TEMPHUM_NONHOLDMODE_NORMAL_MODE;
                    break;
                }

                case SpeedModes.Fast when _operatingMode == OperatingModes.Hold:
                {
                    _dataRegister = Si7034_CMD_READ_TEMPHUM_HOLDMODE_FAST_MODE;
                    break;
                }

                case SpeedModes.Fast when _operatingMode == OperatingModes.NoHold:
                {
                    _dataRegister = Si7034_CMD_READ_TEMPHUM_NONHOLDMODE_FAST_MODE;
                    break;
                }

                default:
                {
                    _dataRegister = Si7034_CMD_READ_TEMPHUM_HOLDMODE_NORMAL_MODE;
                    break;
                }
            }
        }

        /// <summary>
        /// Reads the temperature and humidity from the TempHum 2 Click
        /// </summary>
        /// <param name="temperature">The temperature is returned in the <see cref="TemperatureUnits"/> as specified in the TemperatureUnit property.</param>
        /// <param name="humidity">The humidity is returned as % RH.</param>
        /// <returns>True of the measurement was without Checksum Error, or False if an error occurred during the measurement cycle.</returns>
        public Boolean ReadSensor(out Single temperature, out Single humidity)
        {
            Byte[] registerData = ReadRegister(_dataRegister, 6);
            if (CalculateCRC8(new[] { registerData[0], registerData[1] }) != registerData[2])
            {
                temperature = Single.MinValue;
                humidity = Single.MinValue;
                return false;
            }

            if (CalculateCRC8(new[] { registerData[3], registerData[4] }) != registerData[5])
            {
                temperature = Single.MinValue;
                humidity = Single.MinValue;
                return false;
            }

            // Temperature
            UInt16 iTempData = registerData[0];
            iTempData <<= 8;
            iTempData |= registerData[1];
            Single fTempData = -45.0f + 175.0f * (iTempData / 65535.0f);

            //Humidity
            UInt16 iHumData = registerData[3];
            iHumData <<= 8;
            iHumData |= registerData[4];
            Single fHumData = 100.0f * (iHumData / 65535.0f);

            temperature = ScaleTemperature(fTempData);
            humidity = fHumData;

            return QueryDevice();
        }

        /// <summary>
        /// Resets the Altitude 3 Click and reloads the calibration data into OTP memory.
        /// </summary>
        /// <param name="resetMode">Th <see cref="ResetModes"/> to use.</param>
        /// <example>
        /// Example usage:
        /// <code language="C#">
        /// // Reset the Altitude 3 Click
        /// _sensor.Reset;
        /// </code>
        /// <code language="VB">
        /// ' Reset the Altitude 3 Click
        /// _sensor.Reset
        /// </code>
        /// </example>
        public Boolean Reset(ResetModes resetMode)
        {
            if (resetMode == ResetModes.Hard) throw new NotSupportedException("This module does not support hard resets. Use a soft reset instead.");

            WriteByte(Si7034_CMD_RESET);

            Thread.Sleep(2);

            return true;
        }

        #endregion

        #region Interface Implementations

        /// <summary>
        /// Reads the Temperature
        /// </summary>
        /// <param name="source">The <see cref="TemperatureSources"/> to use.</param>
        /// <exception cref="NotSupportedException">A NotSupportedException will be thrown if attempting to read the temperature with this method.
        /// Direct reading of temperature is not supported with this module. Use the <see cref="ReadSensor"/> method instead.
        /// </exception>
        public Single ReadTemperature(TemperatureSources source)
        {
            throw new NotSupportedException("This modules does not support direct reading of raw temperature data.");
        }

        /// <summary>
        /// Reads the Humidity
        /// </summary>
        /// <param name="measurementMode">The <see cref="HumidityMeasurementModes"/> to use.</param>
        /// <exception cref="NotSupportedException">A NotSupportedException will be thrown if attempting to read the humidity with this method.
        /// Direct reading of humidity is not supported with this module. Use the <see cref="ReadSensor"/> method instead.
        /// </exception>
        public Single ReadHumidity(HumidityMeasurementModes measurementMode)
        {
            throw new NotSupportedException("This modules does not support direct reading of raw temperature data.");
        }

        /// <summary>Gets the raw data of the humidity value.</summary>
        /// <value>
        /// Raw data in the range depending on sensor's precision (8/10/12 bits, for example)
        /// </value>
        /// <exception cref="NotSupportedException">A NotSupportedException will be thrown if attempting to read the raw humidity data with this property.
        /// Direct reading of humidity raw data is not supported with this module.
        /// </exception>
        Int32 IHumidity.RawData => throw new NotSupportedException("This modules does not support direct reading of raw humidity data.");

        /// <summary>Gets the raw data of the temperature value.</summary>
        /// <value>
        /// Raw data in the range depending on sensor's precision (8/10/12 bits, for example)
        /// </value>
        /// <exception cref="NotSupportedException">A NotSupportedException will be thrown if attempting to read the raw temperature data with this property.
        /// Direct reading of temperature raw data is not supported with this module.
        /// </exception>
        Int32 ITemperature.RawData => throw new NotSupportedException("This modules does not support direct reading of raw temperature data.");

        #endregion
    }
}
