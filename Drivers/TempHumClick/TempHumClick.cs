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
    /// Main driver class for the TempHum Click
    /// <para><b>Pins used :</b> Scl, Sda (I²C bus)</para>
    /// <para><b>This module is an I2C Device.</b></para>
    /// </summary>
    /// <example>Example usage:
    /// <code language = "C#">
    /// </code>
    /// </example>
    public sealed class TempHumClick
    {
        #region .ctor

        /// <summary>
        /// Create a new TempHum Click temperature and humidity sensor.
        /// </summary>
        /// <param name="socket"></param>
        public TempHumClick(Hardware.Socket socket)
        {
            _socket = socket;
#if (NANOFRAMEWORK_1_0)
            _sensor = I2cDevice.Create(new I2cConnectionSettings(socket.I2cBus, I2cAddress, I2cBusSpeed.StandardMode));
#else
			_sensor = I2cController.FromName(socket.I2cBus).GetDevice(new I2cConnectionSettings(I2cAddress, 100000));
#endif

                Reset();

                if (DeviceID() != HTS221_WHOAMI_VALUE) throw new DeviceInitialisationException("TempHum Click not found on the I2C Bus.");

                SetHeaterState(false);

                ReadCalibrationData();
        }

        #endregion

        #region Constants

        private const Byte I2cAddress = 0x5F;
        private const Byte HTS221_WHOAMI_VALUE = 0xBC;

        // Registers
        private const Byte HTS221_WHO_AM_I = 0x0F;
        private const Byte HTS221_AV_CONF = 0x10;
        private const Byte HTS221_CTRL_REG1 = 0x20;
        private const Byte HTS221_CTRL_REG2 = 0x21;
        //private const byte HTS221_CTRL_REG3 = 0x22;
        private const Byte HTS221_STATUS_REG = 0x27;
        private const Byte HTS221_HUMIDITY_OUT_L = 0x28;
        private const Byte HTS221_HUMIDITY_OUT_H = 0x29;
        private const Byte HTS221_TEMP_OUT_L = 0x2A;
        private const Byte HTS221_TEMP_OUT_H = 0x2B;

        // Calibration registers
        private const Byte HTS221_H0_rH_x2 = 0x30;
        private const Byte HTS221_H1_rH_x2 = 0x31;
        private const Byte HTS221_T0_degC_x8 = 0x32;
        private const Byte HTS221_T1_degC_x8 = 0x33;
        private const Byte HTS221_T1_T0_msb = 0x35;
        private const Byte HTS221_H0_T0_OUT_L = 0x36;
        private const Byte HTS221_H0_T0_OUT_H = 0x37;
        private const Byte HTS221_H1_T0_OUT_L = 0x3A;
        private const Byte HTS221_H1_T0_OUT_H = 0x3B;
        private const Byte HTS221_T0_OUT_L = 0x3C;
        private const Byte HTS221_T0_OUT_H = 0x3D;
        private const Byte HTS221_T1_OUT_L = 0x3E;
        private const Byte HTS221_T1_OUT_H = 0x3F;

        private const Byte HTS221_HEATER_MASK = 0x02;
        private const Byte HTS221_HEATER_BIT = 1;
        private const Byte HTS221_BDU_MASK = 4;
        private const Byte HTS221_BDU_BIT = 2;
        private const Byte HTS221_ODR_MASK = 0x03;
        private const Byte HTS221_ODR_Bit = 0;
        private const Byte HTS221_POWER_STATE_MASK = 0x80;
        private const Byte HTS221_POWER_STATE_BIT = 7;
        private const Byte HTS221_BOOT_MASK = 0x80;
        private const Byte HTS221_TEMPERATURE_RESOLUTION_MASK = 0x38;
        private const Byte HTS221_TEMPERATURE_RESOLUTION_BIT = 0x03;
        private const Byte HTS221_HUMIDITY_RESOLUTION_MASK = 0x07;
        private const Byte HTS221_HUMIDITY_RESOLUTION_BIT = 0x00;
        private const Byte HTS221_HUMIDITY_DATA_AVAILABLLE_BIT = 1;
        private const Byte HTS221_TEMPERATURE_DATA_AVAILABLLE_BIT = 0;

        #endregion

        #region Public ENUMS

        /// <summary>
        /// Temperature resolution used to configure register AV_CONF (0x10)
        /// </summary>
        public enum TemperatureResolutions
        {
            /// <summary>
            /// Use 2 samples for averaging.
            /// </summary>
            Average2 = 0,

            /// <summary>
            /// Use 4 samples for averaging.
            /// </summary>
            Average4,

            /// <summary>
            /// Use 8 samples for averaging.
            /// </summary>
            Average8,

            /// <summary>
            /// Use 16 samples for averaging. This is the default configuration.
            /// </summary>
            Average16,

            /// <summary>
            /// Use 32 samples for averaging.
            /// </summary>
            Average32,

            /// <summary>
            /// Use 64 samples for averaging.
            /// </summary>
            Average64,

            /// <summary>
            /// Use 128 samples for averaging.
            /// </summary>
            Average128,

            /// <summary>
            /// Use 256 samples for averaging.
            /// </summary>
            Average256
        }

        /// <summary>
        /// Humidity resolution used to configure register AV_CONF (0x10)
        /// </summary>
        public enum HumidityResolutions
        {
            /// <summary>
            /// Use 4 samples for averaging.
            /// </summary>
            Average4 = 0,

            /// <summary>
            /// Use 8 samples for averaging.
            /// </summary>
            Average8,

            /// <summary>
            /// Use 16 samples for averaging.
            /// </summary>
            Average16,

            /// <summary>
            /// Use 32 samples for averaging. This is the default configuration.
            /// </summary>
            Average32,

            /// <summary>
            /// Use 64 samples for averaging.
            /// </summary>
            Average64,

            /// <summary>
            /// Use 128 samples for averaging.
            /// </summary>
            Average128,

            /// <summary>
            /// Use 256 samples for averaging.
            /// </summary>
            Average256,

            /// <summary>
            /// Use 512 samples for averaging.
            /// </summary>
            Average512
        }

        /// <summary>
        /// Output data rate configuration
        /// </summary>
        public enum ODR
        {
            /// <summary>
            /// One-Shot measurement for both Temperature and Humidity.
            /// </summary>
            OdrOneShot = 0,

            /// <summary>
            /// One Hertz measurement
            /// </summary>
            Odr1Hz,

            /// <summary>
            /// Seven (7) Hertz measurement
            /// </summary>
            Odr7Hz,

            /// <summary>
            /// Twelve Point Five (12.5) Hertz measurement.
            /// </summary>
            Odr12Hz5
        }

        /// <summary>
        ///  Block Data Update inhibits the output register update between the reading of the upper and lower temperature and Humidity registers.
        /// </summary>
        public enum BDU
        {
            /// <summary>
            /// The lower and upper register parts are updated continuously
            /// </summary>
            Continuous = 0x00,

            /// <summary>
            /// The output registers not updated until both MSB and LSB of the temperature and Humidity registers are read.
            /// This feature prevents the reading of LSB and MSB related to different samples.
            /// </summary>
            Quiescent = 0x01
        }

        /// <summary>
        /// ENUM for various powere modes the HTS221 supports
        /// </summary>
        public enum PowerModes
        {
            /// <summary>
            /// Power is off.
            /// </summary>
            Off = 0,

            /// <summary>
            /// Power is on.
            /// </summary>
            On= 1
        }

        #endregion

        #region Private Fields

        private I2cDevice _sensor;
        private readonly Hardware.Socket _socket;
        private Int32 registerData;
        private Int16 sensorData;
        private Single temperature;
        private Single humidity;

        // Fields used for calibration values.
        private Int16 T0_OUT;
        private Int16 T1_OUT;
        private Int16 H0_T0_OUT;
        private Int16 H1_T0_OUT;
        private UInt16 T0_degC;
        private UInt16 T1_degC;
        private Byte H0_rH;
        private Byte H1_rH;

        #endregion

        #region Public Methods

        /// <summary>
        /// Configures the sensor using the supplied parameters.
        /// </summary>
        /// <remarks>This method additionally Powers On the Temp<![CDATA[&]]>Hum Click.</remarks>
        /// <param name="tempResolution">The <see cref="TemperatureResolutions"/> to use.</param>
        /// <param name="humidResolution">The <see cref="HumidityResolutions"/> to use.</param>
        /// <param name="outputDataRate">The <see cref="ODR"/> to use.</param>
        /// <param name="blockDataUpdate">The <see cref="BDU"/> to use.</param>
        /// <example>
        /// <code language = "C#">
        /// _tempHum.ConfigureSensor(TempHumClick.TemperatureResolutions.Average32, TempHumClick.HumidityResolutions.Average64, TempHumClick.ODR.Odr1Hz, TempHumClick.BDU.Quiescent);
        /// </code>
        /// </example>
        public void ConfigureSensor(TemperatureResolutions tempResolution, HumidityResolutions humidResolution, ODR outputDataRate, BDU blockDataUpdate)
        {
            /* The following code is equivalent to setting:
            // PowerMode = PowerModes.On;
            // SetHeaterState(false);
            // TemperatureResolution = tempResolution;
            // HumidityResolution = humidResolution;
            // SetBDU(blockDataUpdate);
            // OutputDataRate(outputDataRate);
            */

            registerData = (Byte)(((Byte)tempResolution << HTS221_TEMPERATURE_RESOLUTION_BIT) | ((Byte)humidResolution << HTS221_HUMIDITY_RESOLUTION_BIT));
            WriteRegister(HTS221_AV_CONF, (Byte)registerData);
            registerData = (Byte)((1 << HTS221_POWER_STATE_BIT) | ((Byte)blockDataUpdate << HTS221_BDU_BIT) | ((Byte)outputDataRate << HTS221_ODR_Bit));
            WriteRegister(HTS221_CTRL_REG1, (Byte)registerData);
        }

        /// <summary>
        /// The Heater is used to control an internal heating element, that can effectively be used to speed up the sensor recovery time in case of condensation.
        /// The heater can be operated only by an external controller, which means that it has to be switched on/off directly by software.
        /// Humidity and temperature output should not be read during the heating cycle.
        /// </summary>
        /// <param name="status">Set to True to turn on the heater or false to turn off the heater.</param>
        /// <example>
        /// <code language = "C#">
        /// _tempHum.SetHeaterState(true);
        /// </code>
        /// </example>
        public void SetHeaterState(Boolean status)
        {
            registerData = ReadRegister(HTS221_CTRL_REG2);

            registerData &= ~HTS221_HEATER_MASK;
            registerData |= (Byte)((status ? 1 : 0) << HTS221_HEATER_BIT);

            WriteRegister(HTS221_CTRL_REG2, (Byte)registerData);
        }

        /// <summary>
        /// The BDU bit is used to inhibit the output register update between the reading of the upper and lower register parts.
        /// In default mode (BDU = Continuous), the lower and upper register parts are updated continuously.
        /// If it is not certain whether the read will be faster than output data rate, it is recommended to set the BDU bit to �Quiescent�.
        /// In this way, after the reading of the lower (upper) register part, the content of that output register is not updated until the upper (lower) part is read also.
        /// This feature prevents the reading of LSB and MSB related to different samples.
        /// </summary>
        /// <param name="value">The BlockDataUpdate to use.</param>
        /// <example>
        /// <code language = "C#">
        /// _tempHum.SetBDU(TempHumClick.BDU.Quiescent);
        /// </code>
        /// </example>
        public void SetBDU(BDU value) // ToDo = Property or method?
        {
            registerData = ReadRegister(HTS221_CTRL_REG1);
            registerData &= ~HTS221_BDU_MASK;
            registerData |= (Byte)value << HTS221_BDU_BIT;

            WriteRegister(HTS221_CTRL_REG1, (Byte)registerData);
        }

        /// <summary>
        /// The OutputDataRate <see cref="ODR"/> permit changes to the output data rates of humidity and temperature samples.
        /// The default value corresponds to a �one-shot� configuration for both humidity and temperature output.
        /// </summary>
        /// <param name="value">The <see cref="ODR"/> to use.</param>
        /// <example>
        /// <code language = "C#">
        /// _tempHum.OutputDataRate(TempHumClick.ODR.Odr1Hz);
        /// </code>
        /// </example>
        public void SetOutputDataRate(ODR value) // ToDo - Property or method?
        {
            registerData = ReadRegister(HTS221_CTRL_REG1);
            registerData &= ~HTS221_ODR_MASK;
            registerData |= (Byte)value << HTS221_ODR_Bit; // Don't really need the shift operation here.

            WriteRegister(HTS221_CTRL_REG1, (Byte)registerData);
        }

        /// <summary>Reads the temperature.</summary>
        /// <param name="source">The <see cref="TemperatureSources"/> to use.</param>
        /// <returns>A single representing the temperature read from the source, degrees Celsius.</returns>
        /// <example>
        /// <code language = "C#">
        /// Debug.WriteLine("Temperature.......: " + _tempHum.ReadTemperature().ToString("F2") + " �F");
        /// </code>
        /// </example>
        public Single ReadTemperature(TemperatureSources source = TemperatureSources.Ambient)
        {
            if (source == TemperatureSources.Object) throw new NotSupportedException("This module does not support reading object temperature. Use ambient instead.");

            if ((ReadRegister(HTS221_CTRL_REG1) & 0x03) == 0x00) // If we are in one-shot mode, enable a one-shot conversion.
            {
                // Set one-shot enable while forcing the heater off also BOOT bit (bit 7) should be 0.
                WriteRegister(HTS221_CTRL_REG2, 0x01);
            }

            // Wait here for completion of conversion either in one-shot mode or via ODR cycle
            WaitForSensorData(HTS221_STATUS_REG, HTS221_TEMPERATURE_DATA_AVAILABLLE_BIT);

            sensorData = ReadRegister(HTS221_TEMP_OUT_L);
            sensorData |= (Int16)(ReadRegister(HTS221_TEMP_OUT_H) << 8);

            temperature = (sensorData - T0_OUT) / (Single)(T1_OUT - T0_OUT);
            temperature = T0_degC + temperature * (T1_degC - T0_degC);
            temperature /= 8;

            return ScaleTemperature(temperature);
        }

        /// <summary>
        /// Reads relative humidity.
        /// </summary>
        /// <param name="humidityMeasurementMode"></param>
        /// <returns>A single representing the humidity read from the source as %RH.</returns>
        /// <example>
        /// <code language = "C#">
        /// Debug.WriteLine("Humidity..........: " + _tempHum.ReadHumidity().ToString("F2") + " %RH");
        /// </code>
        /// </example>
        public Single ReadHumidity(HumidityMeasurementModes humidityMeasurementMode = HumidityMeasurementModes.Relative)
        {
            if (humidityMeasurementMode == HumidityMeasurementModes.Absolute) throw new NotSupportedException("This module does not support reading absolute humidity. Use relative humidity mode instead.");

            if ((ReadRegister(HTS221_CTRL_REG1) & 0x03) == 0x00) // If we are in one-shot mode, enable a one-shot conversion.
            {
                // Set one-shot enable while forcing the heater off also BOOT bit (bit 7) should be 0.
                WriteRegister(HTS221_CTRL_REG2, 0x01);
            }

            // Wait here for completion of conversion either in one-shot mode or via ODR cycle
            WaitForSensorData(HTS221_STATUS_REG, HTS221_HUMIDITY_DATA_AVAILABLLE_BIT);

            sensorData = ReadRegister(HTS221_HUMIDITY_OUT_L);
            sensorData |= (Int16)(ReadRegister(HTS221_HUMIDITY_OUT_H) << 8);

            humidity = (sensorData - H0_T0_OUT) / (Single)(H1_T0_OUT - H0_T0_OUT);
            humidity = H0_rH + humidity * (H1_rH - H0_rH);
            humidity /= 2;

            return humidity;
        }

        /// <summary>Resets the module.</summary>
        /// <remarks>Resetting this module will additionally turn off the internal heating element and set the One-Shot enable bit to Zero (0).</remarks>
        /// <example>
        /// <code language = "C#">
        /// _tempHum.Reset(ResetModes.Soft);
        /// </code>
        /// </example>
        public void Reset()
        {
            WriteRegister(HTS221_CTRL_REG2, HTS221_BOOT_MASK);

            do
            {
                registerData = ReadRegister(HTS221_CTRL_REG2);
            } while ((registerData & 0x80) == 0x80);
        }

        /// <summary>Gets or sets the power mode.</summary>
        /// <value>The current power mode of the module.</value>
        /// <remarks>If the module has no power modes, then GET should always return PowerModes.ON while SET should throw a NotImplementedException.</remarks>
        /// <example>
        /// <code language = "C#">
        /// _tempHum.PowerMode = PowerModes.On;
        /// </code>
        /// <code language = "VB">
        /// _tempHum.PowerMode = PowerModes.On
        /// </code>
        /// </example>
        public PowerModes PowerMode
        {
            get => (ReadRegister(HTS221_CTRL_REG1) & 0x80) == 0x80 ? PowerModes.On : PowerModes.Off;
            set
            {
                registerData = ReadRegister(HTS221_CTRL_REG1);
                registerData &= ~HTS221_POWER_STATE_MASK;
                registerData |= (Byte)(value == PowerModes.On ? 1 : 0) << HTS221_POWER_STATE_BIT;

                WriteRegister(HTS221_CTRL_REG1, (Byte)registerData);
            }
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Sets or gets the numbers of averaged temperature samples per conversion.
        /// </summary>
        /// <remarks>The default value on Power up is <see cref="TemperatureResolutions.Average16"/></remarks>
        /// <example>
        /// <code language = "C#">
        /// _tempHum.TemperatureResolution = TempHumClick.TemperatureResolutions.Average64;
        /// </code>
        /// </example>
        public TemperatureResolutions TemperatureResolution
        {
            get
            {
                registerData = ReadRegister(HTS221_AV_CONF);
                return (TemperatureResolutions)((registerData & HTS221_TEMPERATURE_RESOLUTION_MASK) >> HTS221_TEMPERATURE_RESOLUTION_BIT);
            }
            set
            {
                if ((Int32)value < 0) value = 0;
                if ((Int32)value > 0x07) value = TemperatureResolutions.Average256;

                registerData = ReadRegister(HTS221_AV_CONF);

                registerData &= ~HTS221_TEMPERATURE_RESOLUTION_MASK;
                registerData |= (Byte)value << HTS221_TEMPERATURE_RESOLUTION_BIT;

                WriteRegister(HTS221_AV_CONF, (Byte)registerData);
            }
        }

        /// <summary>
        /// Sets or gets the numbers of averaged humidity samples per conversion.
        /// </summary>
        /// <remarks>The default value on Power up is <see cref="HumidityResolutions.Average32"/></remarks>
        /// <example>
        /// <code language = "C#">
        /// _tempHum.HumidityResolution = TempHumClick.HumidityResolutions.Average64;
        /// </code>
        /// </example>
        public HumidityResolutions HumidityResolution
        {
            get
            {
                registerData = ReadRegister(HTS221_AV_CONF);
                return (HumidityResolutions)((registerData & HTS221_HUMIDITY_RESOLUTION_MASK) >> HTS221_HUMIDITY_RESOLUTION_BIT);
            }
            set
            {
                if ((Int32)value < 0) value = 0;
                if ((Int32)value > 0x07) value = HumidityResolutions.Average512;

                registerData = ReadRegister(HTS221_AV_CONF);

                registerData &= ~HTS221_HUMIDITY_RESOLUTION_MASK;
                registerData |= (Byte)value << HTS221_HUMIDITY_RESOLUTION_BIT; // Todo Don't really need this shift operation.

                WriteRegister(HTS221_AV_CONF, (Byte)registerData);
            }
        }

        /// <summary>
        /// Reads the Device ID from the HTS221 IC
        /// </summary>
        /// <returns></returns>
        /// <example>
        /// <code language = "C#">
        /// Debug.WriteLine("Device ID is 0x" + _tempHum.DeviceID().ToString("x2"));
        /// </code>
        /// <code language = "VB">
        /// Debug.WriteLine("Device ID is 0x" <![CDATA[&]]> _tempHum.DeviceID().ToString("x2"))
        /// </code>
        /// </example>
        public Byte DeviceID()
        {
            return ReadRegister(HTS221_WHO_AM_I);
        }

        ///<summary>
        /// Gets or sets the unit of measure for reporting the temperature. Defaults to Degrees C.
        /// </summary>
        public TemperatureUnits TemperatureUnit { get; set; } = TemperatureUnits.Celsius;


        #endregion

        #region Private Methods

        private void ReadCalibrationData()
        {
            T0_OUT = ReadRegister(HTS221_T0_OUT_L);
            T0_OUT |= (Int16)(ReadRegister(HTS221_T0_OUT_H) << 8);

            T1_OUT = ReadRegister(HTS221_T1_OUT_L);
            T1_OUT |= (Int16)(ReadRegister(HTS221_T1_OUT_H) << 8);

            H0_T0_OUT = ReadRegister(HTS221_H0_T0_OUT_L);
            H0_T0_OUT |= (Int16)(ReadRegister(HTS221_H0_T0_OUT_H) << 8);

            H1_T0_OUT = ReadRegister(HTS221_H1_T0_OUT_L);
            H1_T0_OUT |= (Int16)(ReadRegister(HTS221_H1_T0_OUT_H) << 8);

            Byte msb = ReadRegister(HTS221_T1_T0_msb);
            Byte T0_msb = (Byte)(msb & 0x03);
            Byte T1_msb = (Byte)((msb & 0x0C) >> 2);
            T0_degC = (UInt16)(ReadRegister(HTS221_T0_degC_x8) | (T0_msb << 8));
            T1_degC = (UInt16)(ReadRegister(HTS221_T1_degC_x8) | (T1_msb << 8));

            H0_rH = ReadRegister(HTS221_H0_rH_x2);
            H1_rH = ReadRegister(HTS221_H1_rH_x2);
        }

        private void WaitForSensorData(Byte register, Byte bit)
        {
            while ((ReadRegister(register) & bit) != bit)
            {
                Thread.Sleep(5);
            }
        }

        private Byte ReadRegister(Byte registerAddress)
        {
            Byte[] writeBuffer = {registerAddress};
            Byte[] readBuffer = new Byte[1];

            lock (_socket.LockI2c)
            {
                _sensor.WriteRead(writeBuffer, readBuffer);
            }

            return readBuffer[0];
        }

        private void WriteRegister(Byte registerAddress, Byte data)
        {
            lock (_socket.LockI2c)
            {
                _sensor.Write(new[] {registerAddress, data});
            }
        }

        private Single ScaleTemperature(Single temperatureValue)
        {
            switch (TemperatureUnit)
            {
                case TemperatureUnits.Celsius:
                {
                    return temperatureValue;
                }

                case TemperatureUnits.Fahrenheit:
                {
                    return temperatureValue * 1.8F + 32F;
                }

                case TemperatureUnits.Kelvin:
                {
                    return temperatureValue + 273.15F;
                }

                default:
                {
                    return temperatureValue;
                }
            }
        }

        #endregion
    }
}
