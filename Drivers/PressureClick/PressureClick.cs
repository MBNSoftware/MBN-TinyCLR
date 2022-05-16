/*
 * Pressure Click board driver for TinyCLR 2.0
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
    /// Main class for the Pressure Click board driver
    /// <para><b>Pins used :</b> Scl, Sda</para>
    /// </summary>
    /// <example>
    /// <code language="C#">
    /// public class Program
    /// {
    ///    static PressureClick _pres;
    ///    static Boolean _deviceOk;
    ///
    ///    public static void Main()
    ///    {
    ///        _deviceOk = false;
    ///        while (!_deviceOk)
    ///        {
    ///            try
    ///            {
    ///                _pres = new PressureClick(Hardware.SocketOne);
    ///                _deviceOk = true;
    ///            }
    ///            catch (DeviceInitialisationException)
    ///            {
    ///                Debug.Print("Init failed, retrying...");
    ///            }
    ///        }
    ///
    ///        while (true)
    ///        {
    ///            Debug.Print("Pression = " + _pres.ReadPressure() + " hPa");
    ///            Debug.Print("Temperature = " + _pres.ReadTemperature().ToString("F2") + "°");
    ///            Thread.Sleep(1000);
    ///        }
    ///    }
    ///}
    /// </code>
    /// </example>
    public sealed partial class PressureClick : ITemperature, IPressure
    {
        private readonly I2cDevice _pres;
        private Boolean _powered = true;
        private Byte _odr = 0x64;
        private Int16 _tempRawData;
        private readonly Hardware.Socket _socket;

        /// <summary>
        /// Initializes a new instance of the <see cref="PressureClick"/> class.
        /// </summary>
        /// <param name="socket">The socket on which the Pressure Click board is plugged on MikroBus.Net</param>
        /// <param name="address">Address of the I²C device.</param>
        /// <exception cref="DeviceInitialisationException">Thrown if device failed to initialize.</exception>
        public PressureClick(Hardware.Socket socket, Byte address = 0xBA >> 1)
        {
            _socket = socket;
            // Create the driver's I²C configuration
#if (NANOFRAMEWORK_1_0)
            _pres = I2cDevice.Create(new I2cConnectionSettings(socket.I2cBus, address, I2cBusSpeed.StandardMode));
#else
			_pres = I2cController.FromName(socket.I2cBus).GetDevice(new I2cConnectionSettings(address, 100000));
#endif
            if (!Init())
            {
                throw new DeviceInitialisationException("Device failed to initialize");
            }
        }

        #region Private methods
        private Byte ReadByte(Byte register)
        {
            var result = new Byte[1];

            lock (_socket.LockI2c)
            {
                _pres.WriteRead(new[] { register }, result);
            }

            return result[0];
        }

        private Int32 ReadInteger(Byte register)
        {
            var low = ReadByte(register);
            var high = ReadByte((Byte)(register + 1));

            return (high << 8) + low;
        }

        private void SetRegister(Byte register, Byte value)
        {
            lock (_socket.LockI2c)
            {
                _pres.Write(new[] { register, value });
            }
            Thread.Sleep(20);
        }

        private void SetRegisterBit(Byte register, Byte bitIndex, Boolean state)
        {
            lock (_socket.LockI2c)
            {
                _pres.Write(new[] { register, Bits.Set(ReadByte(register), bitIndex, state) });
            }
            Thread.Sleep(20);
        }

        private Boolean Init()
        {
            Byte err = 0;
            SetRegister(Registers.RES_CONF, 0x78);
            if (ReadByte(Registers.RES_CONF) != 0x78) { err++; }
            SetRegister(Registers.CTRL_REG1, 0x64);
            if (ReadByte(Registers.CTRL_REG1) != 0x64) { err++; }
            SetRegister(Registers.CTRL_REG1, 0xE4);
            if (ReadByte(Registers.CTRL_REG1) != 0xE4) { err++; }

            return err == 0;
        }
        #endregion

        /// <summary>
        /// Gets the device identifier.
        /// </summary>
        /// <value>
        /// The device identifier.
        /// </value>
        /// <example>
        /// <code language="C#">
        ///      Debug.Print ("Pressure sensor : "+_pres.DeviceID);
        /// </code>
        /// </example>
        public String DeviceID
        {
            get { return ReadByte(Registers.WHO_AM_I) == 0xBB ? "LPS331AP" : "Unknown"; }
        }

        /// <summary>
        /// Gets or sets the output data rate.
        /// </summary>
        /// <remarks>See the datasheet as this parameter sets both the temperature and pressure data rate at different frequencies.</remarks>
        /// <value>
        /// The output data rate.
        /// </value>
        /// <example>
        /// <code language="C#">
        ///     // Sets the pressure datarate to 7Hz and temperature to 1Hz
        ///      _pres.OutputDataRate = 2;
        /// </code>
        /// </example>
        public Byte OutputDataRate
        {
            get { return _odr; }
            set
            {
                var valTmp = value > 7 ? (Byte)0 : value;
                _odr = valTmp;
                if ((valTmp == 7) & (ReadByte(Registers.RES_CONF) == 0x7A))         // 0x7A Config not allowed with ODR = 25Hz/25Hz
                {
                    SetRegister(Registers.RES_CONF, 0x64);                          // For ODR 25Hz/25Hz, the suggested configuration for RES_CONF is 6Ah.
                }
                var ctrlReg1 = ReadByte(Registers.CTRL_REG1);
                SetRegister(Registers.CTRL_REG1, (Byte)(ctrlReg1 & (0xFF & (valTmp << 4))));
            }
        }

        /// <summary>
        /// Reads the temperature from the sensor.
        /// </summary>
        /// <param name="source">The source for the measurement.<see cref="TemperatureSources"/></param>
        /// <returns>
        /// A single representing the temperature read from the source, degrees Celsius
        /// </returns>
        /// <example>
        /// <code language="C#">
        ///     // Reads the ambient temperature
        ///     Debug.Print ("Ambient temperature = "+_pres.ReadTemperature());
        /// </code>
        /// </example>
        public Single ReadTemperature(TemperatureSources source = TemperatureSources.Ambient)
        {
            var rawValue = (Int16)ReadInteger(Registers.TEMP_OUT_L);
            _tempRawData = rawValue;
            var tempCelsius = (rawValue & 0x8000) == 0x8000 
                ? (Single)(((-1 * ~rawValue + 1) / 480.0) + 42.5) 
                : (Single)((rawValue / 480.0) + 42.5);
            return TemperatureUnit == TemperatureUnits.Celsius ? tempCelsius : TemperatureUnit == TemperatureUnits.Fahrenheit ? (Single)((tempCelsius * (9.0 / 5)) + 32) : (Single)(tempCelsius + 273.15);
        }

        /// <summary>
        /// Gets the raw data of the temperature value.
        /// </summary>
        /// <value>
        /// Raw data in the range depending on sensor's precision (8/10/12 bits, for example)
        /// </value>
        /// <example>
        /// <code language="C#">
        ///      // Reads the temperature raw data
        ///      Debug.Print("Temperature raw data = " + (_pres as ITemperature).RawData);
        /// </code>
        /// </example>
        Int32 ITemperature.RawData
        {
            get { return _tempRawData; }
        }

        /// <summary>
        /// Gets or sets the temperature unit.
        /// </summary>
        /// <value>
        /// The temperature unit from <see cref="TemperatureUnits"/>
        /// </value>
        /// <example>
        /// <code language="C#">
        ///      // Sets the temperature unit to Kelvin
        ///      _pres.TemperatureUnit = TemperatureUnits.Kelvin;
        /// </code>
        /// </example>
        public TemperatureUnits TemperatureUnit { get; set; } = TemperatureUnits.Celsius;

        /// <summary>
        /// Gets or sets the power mode.
        /// </summary>
        /// <value>
        /// The current power mode of the module.
        /// </value>
        /// <exception cref="System.NotImplementedException">Thrown when trying to use PowerModes.Low.</exception>
        /// <example>
        /// <code language="C#">
        ///      // Power off the module
        ///      _pres.PowerMode = PowerModes.Off;
        /// </code>
        /// </example>
        public PowerModes PowerMode
        {
            get { return _powered ? PowerModes.On : PowerModes.Off; }
            set
            {
                if (value == PowerModes.Low) { throw new NotImplementedException("PowerModes.Low"); }
                SetRegisterBit(Registers.CTRL_REG1, 7, value == PowerModes.On);
                _powered = value == PowerModes.On;
            }
        }

        /// <summary>
        /// Reads the pressure from the sensor.
        /// </summary>
        /// <param name="compensationMode">Indicates if the pressure reading returned by the sensor is see-level compensated or not.</param>
        /// <returns>
        /// A single representing the pressure read from the source, in hPa (hectoPascal)
        /// </returns>
        /// <example>
        /// <code language="C#">
        ///     // Reads the actual pressure, in SeaLevelCompensated mode (default)
        ///      Debug.Print ("Current pressure = "+_pres.ReadPressure()+" hPa");
        /// </code>
        /// </example>
        public Single ReadPressure(PressureCompensationModes compensationMode = PressureCompensationModes.SeaLevelCompensated)
        {
            Int32 high = ReadByte(Registers.PRESS_OUT_H);
            var mid = ReadByte(Registers.PRESS_OUT_L);
            var low = ReadByte(Registers.PRESS_OUT_XL);

            high <<= 8;
            high |= mid;
            high <<= 8;
            high |= low;

            return high >> 12;
        }

        /// <summary>
        /// Gets the raw data of the pressure value.
        /// </summary>
        /// <value>
        /// Raw data in the range depending on sensor's precision (8/10/12 bits, for example)
        /// </value>
        /// <exception cref="NotImplementedException">The module does not provide raw data for pressure.</exception>
        Int32 IPressure.RawData
        {
            get { throw new NotImplementedException("IPressure.RawData"); }
        }
    }
}
