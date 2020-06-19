#region Usings

using GHIElectronics.TinyCLR.Devices.I2c;

using System;
using System.Threading;

#endregion

namespace MBN.Modules
{
    /// <inheritdoc cref="ITemperature" />
    /// <inheritdoc cref="IHumidity" />
    /// <inheritdoc cref="IPressure" />
    /// <summary>
    ///     Main class for the Environment Click driver
    ///     <para>
    ///         <b>This is an I2C device.</b>
    ///     </para>
    ///     <para><b>Pins used :</b> Scl, Sda</para>
    /// </summary>
    /// <example>
    ///     <code language="C#">
    /// using System.Threading;
    /// using MBN;
    /// using MBN.Enums;
    /// using MBN.Exceptions;
    /// using MBN.Modules;
    /// using Microsoft.SPOT;
    /// 
    /// namespace Example
    /// {
    ///     public static class Program
    ///     {
    ///         private static EnvironmentClick _environment;
    ///         private static Timer _dataCollector;
    /// 
    ///         public static void Main()
    ///         {
    ///             try
    ///             {
    ///                 _environment = new EnvironmentClick(Hardware.SocketOne, EnvironmentClick.I2CAddress.Address1, ClockRatesI2C.Clock100KHz, 1500);
    ///                 _environment.SetRecommendedMode(EnvironmentClick.RecommendedModes.WeatherMonitoring);
    ///                 _environment.SetHeaterProfile(250, 250, EnvironmentClick.HeaterProfiles.Profile_0);
    ///                 _environment.SelectHeaterProfile(EnvironmentClick.HeaterProfiles.Profile_0);
    ///             }
    ///             catch (DeviceInitialisationException ex)
    ///             {
    ///                 Debug.Print (ex.Message);
    ///                 throw();
    ///             }
    /// 
    ///             Debug.Print("Device ID is 0x" + _environment.DeviceId.ToString("X2"));
    ///             Debug.Print("Temperature Sampling Rate is " + _environment.TemperatureSamplingRate);
    ///             Debug.Print("Pressure Sampling Rate is " + _environment.PressureSamplingRate);
    ///             Debug.Print("Humidity Sampling Rate is " + _environment.HumiditySamplingRate);
    ///             Debug.Print("Filter setting is " + _environment.Filter);
    /// 
    ///             _dataCollector = new Timer(SensorTimerTick, null, 2000, 2000);
    /// 
    ///             Thread.Sleep(Timeout.Infinite);
    ///         }
    /// 
    ///         private static void SensorTimerTick(object state)
    ///         {
    ///             Debug.Print("-----EnvironmentClick Click -----");
    ///             Debug.Print("Temperature.............: " + _environment.ReadTemperature().ToString("F2") + " °C");
    ///             Debug.Print("Humidity................: " + _environment.ReadHumidity().ToString("F2") + " %RH");
    ///             Debug.Print("Pressure................: " + _environment.ReadPressure().ToString("F1") + " mBar");
    ///             Debug.Print("Altitude................: " + _environment.ReadAltitude().ToString("F1") + " meters");
    ///             Debug.Print("Gas Resistance..........: " + _environment.ReadGasResistance() + " kOhms");
    ///             Debug.Print("Gas Heater Stable?......: " + _environment.HeaterStable);
    ///             Debug.Print("Gas Reading Valid? .....: " + _environment.GasReadingValid + "\n");
    /// 
    ///             // Uncomment below to use the ReadSensor() method.
    ///             // float[] _readings = new float[3];
    ///             // _readings = _environment.ReadSensor();
    ///             // Debug.Print("Temperature.....: " + _readings[0].ToString("F2") + " °C");
    ///             // Debug.Print("Pressure........: " + _readings[1].ToString("F1") + " mBar");
    ///             // Debug.Print("Humidity........: " + _readings[2].ToString("F2") + " %RH\n");
    ///         }
    ///     }
    /// }
    /// </code>
    ///     <code language="VB">
    /// Option Explicit On
    /// Option Strict On
    /// 
    /// Imports System.Threading
    /// Imports MBN
    /// Imports MBN.Enums
    /// Imports MBN.Exceptions
    /// Imports MBN.Modules
    /// Imports Microsoft.SPOT
    /// 
    /// Namespace Example
    ///     Module Program
    ///         Private _environment As EnvironmentClick
    ///         Private _dataCollector As Timer
    /// 
    ///         Sub Main()
    ///             Try
    ///                 _environment = New EnvironmentClick(Hardware.SocketOne, EnvironmentClick.I2CAddress.Address1, ClockRatesI2C.Clock100KHz, 1500)
    ///                 _environment.SetRecommendedMode(EnvironmentClick.RecommendedModes.WeatherMonitoring)
    ///                 _environment.SetHeaterProfile(250, 250, EnvironmentClick.HeaterProfiles.Profile_0)
    ///                 _environment.SelectHeaterProfile(EnvironmentClick.HeaterProfiles.Profile_0)
    ///             Catch ex As DeviceInitialisationException
    ///                 Debug.Print(ex.Message)
    ///                 Throw New DeviceInitialisationException()
    ///             End Try
    /// 
    ///             Debug.Print("Device ID is 0x" <![CDATA[&]]> _environment.DeviceId.ToString("X2"))
    ///             Debug.Print("Temperature Sampling Rate is " <![CDATA[&]]> _environment.TemperatureSamplingRate)
    ///             Debug.Print("Pressure Sampling Rate is " <![CDATA[&]]> _environment.PressureSamplingRate)
    ///             Debug.Print("Humidity Sampling Rate is " <![CDATA[&]]> _environment.HumiditySamplingRate)
    ///             Debug.Print("Filter setting is " <![CDATA[&]]> _environment.Filter)
    /// 
    ///             _dataCollector = New Timer(AddressOf SensorTimerTick, Nothing, 2000, 2000)
    /// 
    ///             Thread.Sleep(Timeout.Infinite)
    ///         End Sub
    /// 
    ///         Private Sub SensorTimerTick(ByVal state As Object)
    ///             Debug.Print("-----EnvironmentClick Click -----")
    ///             Debug.Print("Temperature.............: " <![CDATA[&]]> _environment.ReadTemperature().ToString("F2") <![CDATA[&]]> " °C")
    ///             Debug.Print("Humidity................: " <![CDATA[&]]> _environment.ReadHumidity().ToString("F2") <![CDATA[&]]> " %RH")
    ///             Debug.Print("Pressure................: " <![CDATA[&]]> _environment.ReadPressure().ToString("F1") <![CDATA[&]]> " mBar")
    ///             Debug.Print("Altitude................: " <![CDATA[&]]> _environment.ReadAltitude().ToString("F1") <![CDATA[&]]> " meters")
    ///             Debug.Print("Gas Resistance..........: " <![CDATA[&]]> _environment.ReadGasResistance() <![CDATA[&]]> " kOhms")
    ///             Debug.Print("Gas Heater Stable?......: " <![CDATA[&]]> _environment.HeaterStable)
    ///             Debug.Print("Gas Reading Valid? .....: " <![CDATA[&]]> _environment.GasReadingValid <![CDATA[&]]> Constants.vbCrLf)
    /// 
    ///             'Dim below As Uncomment to use the ReadSensor method
    ///             'Dim _readings As Single() = New Single(2) {}
    ///             '_readings = _environment.ReadSensor()
    ///             'Debug.Print("Temperature.....: " <![CDATA[&]]> _readings(0).ToString("F2") <![CDATA[&]]> " °C")
    ///             'Debug.Print("Pressure........: " <![CDATA[&]]> _readings(1).ToString("F1") <![CDATA[&]]> " mBar")
    ///             'Debug.Print("Humidity........: " <![CDATA[&]]> _readings(2).ToString("F2") <![CDATA[&]]> " %RH" <![CDATA[&]]> Constants.vbCrLf)
    ///         End Sub
    ///     End Module
    /// End Namespace
    /// </code>
    /// </example>
    public sealed class EnvironmentClick : ITemperature, IHumidity, IPressure
    {
        #region Internal Structures

        internal struct CalibrationData
        {
            // Temperature data
            internal static UInt16 par_t1;
            internal static Int16 par_t2;
            internal static Int16 par_t3;

            // Pressure data
            internal static UInt16 par_p1;
            internal static Int16 par_p2;
            internal static Int16 par_p3;
            internal static Int16 par_p4;
            internal static Int16 par_p5;
            internal static Int16 par_p6;
            internal static Int16 par_p7;
            internal static Int16 par_p8;
            internal static Int16 par_p9;
            internal static UInt16 par_p10;

            // Humidity data
            internal static UInt16 par_h1;
            internal static UInt16 par_h2;
            internal static Int16 par_h3;
            internal static Int16 par_h4;
            internal static Int16 par_h5;
            internal static UInt16 par_h6;
            internal static Int16 par_h7;

            // t_fine size
            internal static Single t_fine;

            // Gas Data
            internal static Int16 par_gh1;
            internal static Int16 par_gh2;
            internal static Int16 par_gh3;

            internal static Single calAmbTemp = 25; // On first call, need to update this value.

            internal static UInt16 resHeatRange;
            internal static Int16 resHeatValue;
            internal static UInt16 rangeSoftwareError;
        }

        #endregion

        #region .ctor

        /// <summary>
        ///     Initializes a new instance of the <see cref="EnvironmentClick" /> class using the I2C interface.
        /// </summary>
        /// <param name="socket">The socket on which the BarometerClick module is plugged on MikroBus.Net board</param>
        /// <param name="slaveAddress">The address of the module.</param>
        public EnvironmentClick(Hardware.Socket socket, I2CAddress slaveAddress)
        {
            _sensor = I2cController.FromName(socket.I2cBus).GetDevice(new I2cConnectionSettings((Int32) slaveAddress, 100000));

            Reset();

            if (DeviceId != BME680_CHIP_ID) throw new DeviceInitialisationException("Failed to find EnvironmentClick Click on the I2C Bus. Please check your constructor for correct I2CAddress");

            // Read the Calibration Data
            ReadCalibrationData();

            SetRecommendedMode(RecommendedModes.WeatherMonitoring);
        }

        #endregion

        #region Enumerations

        /// <summary>
        ///     Possible I2C addresses that the EnvironmentClick Click supports.
        /// </summary>
        public enum I2CAddress
        {
            /// <summary>
            ///     I2C Address 0. Jumper position on the EnvironmentClick Click soldered to position Zero (0).
            /// </summary>
            Address0 = 0x76,

            /// <summary>
            ///     I2C Address 1. Jumper position on the EnvironmentClick Click soldered to position One (1).
            /// </summary>
            Address1 = 0x77
        }

        /// <summary>
        ///     Preconfigured operating modes, recommended by the Bosch.
        /// </summary>
        public enum RecommendedModes
        {
            /// <summary>
            ///     Only a very low data rate is needed. Power consumption is minimal. Noise of pressure values is of no concern.
            ///     Humidity, pressure and temperature are monitored.
            /// </summary>
            WeatherMonitoring,

            /// <summary>
            ///     A low data rate is needed. Power consumption is minimal.  Humidity and temperature are monitored. Pressure is
            ///     skipped.
            /// </summary>
            HumiditySensing,

            /// <summary>
            ///     Lowest possible altitude noise is needed. A very low bandwidth is preferred. Increased power consumption  is
            ///     tolerated.  Humidity  is  measured  to  help  detect  room  changes.
            /// </summary>
            IndoorNavigation,

            /// <summary>
            ///     Low altitude noise is needed. The required bandwidth is ~2 Hz in order to respond quickly to altitude  changes
            ///     (e.g.  be  able  to  dodge  a  flying  monster  in  a  game).
            ///     Increased  power consumption is tolerated. Humidity sensor is disabled.
            /// </summary>
            Gaming,

            /// <summary>
            ///     Maximum power consumption
            /// </summary>
            FullPower
        }

        internal enum OperationModes : byte
        {
            Sleep = 0x00,
            ForcedMode = 0x01
        }

        /// <summary>
        ///     Infinite Impulse Response Filter Coefficient.
        /// </summary>
        public enum FilterCoefficient : byte
        {
            /// <summary>
            ///     IIR Filter is off.
            /// </summary>
            IIROff = 0x00,

            /// <summary>
            ///     IIR Filter is 1.
            /// </summary>
            IIR1 = 0x01,

            /// <summary>
            ///     IIR Filter is 3.
            /// </summary>
            IIR3 = 0x02,

            /// <summary>
            ///     IIR Filter is 7.
            /// </summary>
            IIR7 = 0x03,

            /// <summary>
            ///     IIR Filter is 15.
            /// </summary>
            IIR15 = 0x04,

            /// <summary>
            ///     IIR Filter is 31.
            /// </summary>
            IIR31 = 0x05,

            /// <summary>
            ///     IIR Filter is 63.
            /// </summary>
            IIR63 = 0x06,

            /// <summary>
            ///     IIR Filter is 127.
            /// </summary>
            IIR127 = 0x07
        }

        /// <summary>
        ///     Oversampling rates
        /// </summary>
        public enum SamplingRate : byte
        {
            /// <summary>
            ///     No oversampling, output is set to 0x8000
            /// </summary>
            Skipped = 0,

            /// <summary>
            ///     Oversampling x 1
            /// </summary>
            Osr1 = 1,

            /// <summary>
            ///     Oversampling x 2
            /// </summary>
            Osr2 = 2,

            /// <summary>
            ///     Oversampling x 4
            /// </summary>
            Osr4 = 3,

            /// <summary>
            ///     Oversampling x 8
            /// </summary>
            Osr8 = 4,

            /// <summary>
            ///     Oversampling x 16
            /// </summary>
            Osr16 = 5
        }

        /// <summary>
        ///     Available user defined gas hot plate settings to use.
        /// </summary>
        public enum HeaterProfiles : byte
        {
            /// <summary>
            ///     Profile 0
            /// </summary>
            Profile_0 = 0x00,

            /// <summary>
            ///     Profile 1
            /// </summary>
            Profile_1 = 0x01,

            /// <summary>
            ///     Profile 2
            /// </summary>
            Profile_2 = 0x02,

            /// <summary>
            ///     Profile 3
            /// </summary>
            Profile_3 = 0x03,

            /// <summary>
            ///     Profile 4
            /// </summary>
            Profile_4 = 0x04,

            /// <summary>
            ///     Profile 5
            /// </summary>
            Profile_5 = 0x05,

            /// <summary>
            ///     Profile 6
            /// </summary>
            Profile_6 = 0x06,

            /// <summary>
            ///     Profile 7
            /// </summary>
            Profile_7 = 0x07,

            /// <summary>
            ///     Profile 8
            /// </summary>
            Profile_8 = 0x08,

            /// <summary>
            ///     Profile 9
            /// </summary>
            Profile_9 = 0x09
        }

        #endregion

        #region Constants

        // Registers
        private const Byte BME680_RESET = 0xE0;
        private const Byte BME680_ID = 0xD0;
        private const Byte BME680_CONFIG = 0x75;
        private const Byte BME680_CTRL_MEAS = 0x74;
        private const Byte BME680_CTRL_HUM = 0x72;
        private const Byte BME680_HUM_MSB = 0x25;
        //private const Byte BME680_HUM_LSB = 0x26;
        private const Byte BME680_TEMP_MSB = 0x22;
        //private const Byte BME680_TEMP_LSB = 0x23;
        //private const Byte BME680_TEMP_XLSB = 0x24;
        private const Byte BME680_PRESS_MSB = 0x1F;
        //private const Byte BME680_PRESS_LSB = 0x20;
        //private const Byte BME680_PRESS_XLSB = 0x21;
        private const Byte BME680_GAS_R_MSB = 0x2A;
        //private const Byte BME680_GAS_R_LSB = 0x2B;
        private const Byte BME680_EAS_STATUS_0 = 0x1D;

        // Gas Registers
        private const Byte BME680_CTRL_GAS_1 = 0x71;
        private const Byte BME680_GAS_WAIT_0 = 0x64;
        private const Byte BME680_RES_HEAT_0 = 0x5A;

        // Registers used for reading the calibration coefficients
        private const Byte BME680_CALIB_ADDR_1 = 0x89; // 25 bytes of calibration data
        private const Byte BME680_CALIB_ADDR_2 = 0xE1; // 16 bytes of calibration data

        private const Byte BME680_COEFF_SIZE = 41;
        private const Byte BME680_COEFF_ADDR1_LEN = 25;
        private const Byte BME680_COEFF_ADDR2_LEN = 16;

        private const Byte BME680_RES_HEAT_VAL = 0x00;
        private const Byte BME680_RES_HEAT_RANGE = 0x02;
        private const Byte BME680_RANGE_SW_ERR = 0x04;

        private const Byte BME680_BIT_H1_DATA_MSK = 0x0F;
        private const Int32 BME680_HUM_REG_SHIFT_VAL = 0x04;

        private const Byte BME680_RHRANGE_MSK = 0x30;
        private const Byte BME680_RSERROR_MSK = 0xF0;

        private const Byte BME680_CHIP_ID = 0x61;

        // Data: Gas range constants for resistance calculation
        private readonly Single[] const_array1 =
            {1, 1, 1, 1, 1, 0.99F, 1, 0.992F, 1, 1, 0.998F, 0.995F, 1, 0.99F, 1, 1};

        private readonly Single[] const_array2 =
        {
            8000000, 4000000, 2000000, 1000000, 499500.4995F, 248262.1648F, 125000, 63004.03226F, 31281.28128F, 15625,
            7812.5F, 3906.25F, 1953.125F, 976.5625F, 488.28125F, 244.140625F
        };

        // Array Index to Field data mapping for Calibration Data
        private const Byte BME680_T2_LSB_REG = 1;
        private const Byte BME680_T2_MSB_REG = 2;
        private const Byte BME680_T3_REG = 3;
        private const Byte BME680_P1_LSB_REG = 5;
        private const Byte BME680_P1_MSB_REG = 6;
        private const Byte BME680_P2_LSB_REG = 7;
        private const Byte BME680_P2_MSB_REG = 8;
        private const Byte BME680_P3_REG = 9;
        private const Byte BME680_P4_LSB_REG = 11;
        private const Byte BME680_P4_MSB_REG = 12;
        private const Byte BME680_P5_LSB_REG = 13;
        private const Byte BME680_P5_MSB_REG = 14;
        private const Byte BME680_P7_REG = 15;
        private const Byte BME680_P6_REG = 16;
        private const Byte BME680_P8_LSB_REG = 19;
        private const Byte BME680_P8_MSB_REG = 20;
        private const Byte BME680_P9_LSB_REG = 21;
        private const Byte BME680_P9_MSB_REG = 22;
        private const Byte BME680_P10_REG = 23;
        private const Byte BME680_H2_MSB_REG = 25;
        private const Byte BME680_H2_LSB_REG = 26;
        private const Byte BME680_H1_LSB_REG = 26;
        private const Byte BME680_H1_MSB_REG = 27;
        private const Byte BME680_H3_REG = 28;
        private const Byte BME680_H4_REG = 29;
        private const Byte BME680_H5_REG = 30;
        private const Byte BME680_H6_REG = 31;
        private const Byte BME680_H7_REG = 32;
        private const Byte BME680_T1_LSB_REG = 33;
        private const Byte BME680_T1_MSB_REG = 34;
        private const Byte BME680_GH2_LSB_REG = 35;
        private const Byte BME680_GH2_MSB_REG = 36;
        private const Byte BME680_GH1_REG = 37;
        private const Byte BME680_GH3_REG = 38;

        #endregion

        #region Fields

        private readonly I2cDevice _sensor;

        #endregion

        #region Private Methods

        private void ReadCalibrationData()
        {
            // Read and store coefficient data
            Byte[] calib1 = ReadRegister(BME680_CALIB_ADDR_1, BME680_COEFF_ADDR1_LEN);
            Byte[] calib2 = ReadRegister(BME680_CALIB_ADDR_2, BME680_COEFF_ADDR2_LEN);
            Byte[] calibrationData = CombineByteArrays(calib1, calib2);

            if (calibrationData.Length != BME680_COEFF_SIZE) throw new Exception(); // ToDo - Remove after testing.

            // Temperature related coefficients
            CalibrationData.par_t1 =
                (UInt16) ConcatBytes(calibrationData[BME680_T1_MSB_REG], calibrationData[BME680_T1_LSB_REG]);
            CalibrationData.par_t2 =
                (Int16) ConcatBytes(calibrationData[BME680_T2_MSB_REG], calibrationData[BME680_T2_LSB_REG]);
            CalibrationData.par_t3 = calibrationData[BME680_T3_REG];

            // Pressure related coefficients
            CalibrationData.par_p1 =
                (UInt16) ConcatBytes(calibrationData[BME680_P1_MSB_REG], calibrationData[BME680_P1_LSB_REG]);
            CalibrationData.par_p2 =
                (Int16) ConcatBytes(calibrationData[BME680_P2_MSB_REG], calibrationData[BME680_P2_LSB_REG]);
            CalibrationData.par_p3 = calibrationData[BME680_P3_REG];
            CalibrationData.par_p4 =
                (Int16) ConcatBytes(calibrationData[BME680_P4_MSB_REG], calibrationData[BME680_P4_LSB_REG]);
            CalibrationData.par_p5 =
                (Int16) ConcatBytes(calibrationData[BME680_P5_MSB_REG], calibrationData[BME680_P5_LSB_REG]);
            CalibrationData.par_p6 = calibrationData[BME680_P6_REG];
            CalibrationData.par_p7 = calibrationData[BME680_P7_REG];
            CalibrationData.par_p8 =
                (Int16) ConcatBytes(calibrationData[BME680_P8_MSB_REG], calibrationData[BME680_P8_LSB_REG]);
            CalibrationData.par_p9 =
                (Int16) ConcatBytes(calibrationData[BME680_P9_MSB_REG], calibrationData[BME680_P9_LSB_REG]);
            CalibrationData.par_p10 = calibrationData[BME680_P10_REG];

            // Humidity related coefficients
            CalibrationData.par_h1 = (UInt16) ((calibrationData[BME680_H1_MSB_REG] << BME680_HUM_REG_SHIFT_VAL) |
                                               (calibrationData[BME680_H1_LSB_REG] & BME680_BIT_H1_DATA_MSK));
            CalibrationData.par_h2 = (UInt16) ((calibrationData[BME680_H2_MSB_REG] << BME680_HUM_REG_SHIFT_VAL) |
                                               (calibrationData[BME680_H2_LSB_REG] >> BME680_HUM_REG_SHIFT_VAL));
            CalibrationData.par_h3 = calibrationData[BME680_H3_REG];
            CalibrationData.par_h4 = calibrationData[BME680_H4_REG];
            CalibrationData.par_h5 = calibrationData[BME680_H5_REG];
            CalibrationData.par_h6 = calibrationData[BME680_H6_REG];
            CalibrationData.par_h7 = calibrationData[BME680_H7_REG];

            // Gas related coefficients
            CalibrationData.par_gh1 = calibrationData[BME680_GH1_REG];
            CalibrationData.par_gh2 =
                (Int16) ConcatBytes(calibrationData[BME680_GH2_MSB_REG], calibrationData[BME680_GH2_LSB_REG]);
            CalibrationData.par_gh3 = calibrationData[BME680_GH3_REG];

            // Other coefficients
            CalibrationData.resHeatRange = (UInt16) ((ReadRegister(BME680_RES_HEAT_RANGE)[0] & BME680_RHRANGE_MSK) / 16);
            CalibrationData.resHeatValue = ReadRegister(BME680_RES_HEAT_VAL)[0];
            Byte[] value = ReadRegister(BME680_RANGE_SW_ERR, 2);
            CalibrationData.rangeSoftwareError = (UInt16) ((ConcatBytes(value[1], value[0]) & BME680_RSERROR_MSK) / 10);
        }

        private static Int32 ConcatBytes(Byte msb, Byte lsb)
        {
            return (msb << 8) | lsb;
        }

        private void WriteRegister(Byte registerAddess, Byte data)
        {
            lock (Hardware.LockI2C)
            {
                _sensor.Write(new [] {registerAddess, data});
            }
        }

        private Byte[] ReadRegister(Byte registerAddess, Byte numberOfBytesToRead = 1)
        {
            Byte[] readBuffer = new Byte[numberOfBytesToRead];
            lock (Hardware.LockI2C)
            {
                _sensor.WriteRead(new[] {registerAddess}, readBuffer);
            }

            return readBuffer;
        }

        private static Byte[] CombineByteArrays(Byte[] a1, Byte[] a2)
        {
            Byte[] destinationArray = new Byte[a1.Length + a2.Length];
            Array.Copy(a1, 0, destinationArray, 0, a1.Length);
            Array.Copy(a2, 0, destinationArray, a1.Length, a2.Length);
            return destinationArray;
        }

        private static Single CompensateTemperature(Int32 temperatureADC)
        {
            Single var1 = (temperatureADC / 16384.0F - CalibrationData.par_t1 / 1024.0F) * CalibrationData.par_t2;
            Single var2 = (temperatureADC / 131072.0F - CalibrationData.par_t1 / 8192.0F) *
                          (temperatureADC / 131072.0F - CalibrationData.par_t1 / 8192.0F) *
                          (CalibrationData.par_t3 * 16.0F);
            CalibrationData.t_fine = var1 + var2;
            return CalibrationData.t_fine / 5120;
        }

        private static Single CompensatePressure(Int32 pressureADC)
        {
            Single var1 = CalibrationData.t_fine / 2.0F - 64000.0F;
            Single var2 = var1 * var1 * (CalibrationData.par_p6 / 131072.0F);
            var2 += var1 * CalibrationData.par_p5 * 2.0F;
            var2 = var2 / 4.0F + CalibrationData.par_p4 * 65536.0F;
            var1 = (CalibrationData.par_p3 * var1 * var1 / 16384.0F + CalibrationData.par_p2 * var1) / 524288.0F;
            var1 = (1.0F + var1 / 32768.0F) * CalibrationData.par_p1;
            Single val = 1048576.0f - pressureADC;

            if (!(Math.Abs(var1) > 0.0)) return 0.0F;

            val = (val - var2 / 4096.0F) * 6250.0F / var1;
            var1 = CalibrationData.par_p9 * val * val / 2147483648.0F;
            var2 = val * (CalibrationData.par_p8 / 32768.0F);
            Single var3 = val / 256.0F * (val / 256.0F) * (val / 256.0F) * (CalibrationData.par_p10 / 131072.0F);
            return val + (var1 + var2 + var3 + CalibrationData.par_p7 * 128.0F) / 16.0F;
        }

        private static Single CompensateHumidity(Int32 humADC)
        {
            Single temp_comp = CalibrationData.t_fine / 5120.0F;
            Single var1 = humADC - (CalibrationData.par_h1 * 16.0F + CalibrationData.par_h3 / 2.0F * temp_comp);
            Single var2 = (Single) (var1 * (CalibrationData.par_h2 / 262144.0F *
                                            (1.0 + CalibrationData.par_h4 / 16384.0F * temp_comp +
                                             CalibrationData.par_h5 / 1048576.0F * temp_comp * temp_comp)));
            Single var3 = CalibrationData.par_h6 / 16384.0F;
            Single var4 = CalibrationData.par_h7 / 2097152.0F;
            Single val = var2 + (var3 + var4 * temp_comp) * var2 * var2;

            // Normalize to between 0 to 100 if outside bounds.
            if (val > 100.0)
            {
                val = 100.0F;
            }
            else if (val < 0.0)
            {
                val = 0.0F;
            }

            return val;
        }

        private Boolean MeasuringGas()
        {
            return (ReadRegister(BME680_EAS_STATUS_0)[0] & 0x40) == 0x40;
        }

        private static Single CalculatePressureAsl(Single uncompensatedPressure)
        {
            Single seaLevelCompensation = (Single) (101325F * Math.Pow((288F - 0.0065F * 143F) / 288F, 5.256F));
            return 101325F + (uncompensatedPressure - seaLevelCompensation);
        }

        private void ForcedRead(Boolean gasMeasurementEnabled)
        {
            SetGasMeasurement(gasMeasurementEnabled);

            Byte temp = ReadRegister(BME680_CTRL_MEAS)[0];
            temp |= (Byte) OperationModes.ForcedMode;
            WriteRegister(BME680_CTRL_MEAS, temp);

            while (Measuring()) Thread.Sleep(1);

            if (!gasMeasurementEnabled) return;

            while (MeasuringGas()) Thread.Sleep(1);
            SetGasMeasurement(false);
        }

        private Boolean Measuring()
        {
            return (ReadRegister(BME680_EAS_STATUS_0)[0] & 0x20) == 0x20;
        }

        private Single CalculateGasResistance(Int32 gasResADC, Int32 gasRange)
        {
            Single var1 = (1340.0F + 5.0F * CalibrationData.rangeSoftwareError) * const_array1[gasRange];
            Single gasres = var1 * const_array2[gasRange] / (gasResADC - 512.0F + var1);
            return gasres;
        }

        private static Byte CalculateHeaterResistance(UInt32 targetTemp)
        {
            if (targetTemp > 400) targetTemp = 400; // Maximum temperature

            Single var1 = CalibrationData.par_gh1 / 16.0F + 49.0F;
            Single var2 = CalibrationData.par_gh2 / 32768.0F * 0.0005F + 0.00235F;
            Single var3 = CalibrationData.par_gh2 / 1024.0F;
            Single var4 = var1 * (1.0F + var2 * targetTemp);
            Single var5 = var4 + var3 * CalibrationData.calAmbTemp;
            return (Byte) (3.4 * (var5 * (4F / (4F + CalibrationData.resHeatRange)) *
                                  (1 / (1 + CalibrationData.resHeatValue * 0.002)) - 25));
        }

        private static Byte CalculateHeatDuration(Int32 dur)
        {
            UInt16 factor = 0;
            UInt16 durval;

            if (dur >= 0xFC)
            {
                durval = 0xFF; // Max duration
            }
            else
            {
                while (dur > 0x3F)
                {
                    dur /= 4;
                    factor += 1;
                }

                durval = (UInt16) (dur + factor * 64);
            }

            return (Byte) durval;
        }

        private void SetGasMeasurement(Boolean state)
        {
            Byte configValue = ReadRegister(BME680_CTRL_GAS_1)[0];

            if (state)
            {
                configValue |= 0x10;
            }
            else
            {
                configValue &= 0xEF;
            }

            WriteRegister(BME680_CTRL_GAS_1, configValue);
        }

        #endregion

        #region Public Properties

        /// <summary>
        ///     Controls the time constant of the IIR filter.
        ///     <p>
        ///         Although this property is exposed, it is recommended against setting this property directly. Use the
        ///         <see cref="SetRecommendedMode" /> method to avoid improper settings.
        ///     </p>
        /// </summary>
        /// <value>
        ///     The Filter Coefficient. See data sheet for the values associated to this coefficient.
        /// </value>
        /// <example>
        ///     Example usage:
        ///     <code language="C#">
        /// _environment.Filter = EnvironmentClick.FilterCoefficient.IIR15;
        /// </code>
        /// </example>
        public FilterCoefficient Filter
        {
            get => (FilterCoefficient) (ReadRegister(BME680_CONFIG)[0] >> 2);
            set
            {
                if ((Int32) value > 7) value = FilterCoefficient.IIR127;
                if ((Int32) value < 0) value = 0;

                Byte registerData = ReadRegister(BME680_CONFIG)[0];
                registerData |= (Byte) ((Byte) value << 2);
                WriteRegister(BME680_CONFIG, registerData);
            }
        }

        /// <summary>
        ///     Gets or sets the humidity sampling rate.
        ///     <p>
        ///         Although this property is exposed, it is recommended against setting this property directly. Use the
        ///         <see cref="SetRecommendedMode" /> method to avoid improper settings.
        ///     </p>
        /// </summary>
        /// <value>
        ///     The humidity sampling rate. See the <seealso cref="SamplingRate" /> for oversampling rates.
        /// </value>
        /// <example>
        ///     Example usage:
        ///     <code language="C#">
        /// _environment.HumiditySamplingRate = EnvironmentClick.OversamplingRates.Osr1;
        /// </code>
        /// </example>
        public SamplingRate HumiditySamplingRate
        {
            get => (SamplingRate) (ReadRegister(BME680_CTRL_HUM)[0] & 0x07);
            set
            {
                Byte registerData = ReadRegister(BME680_CTRL_HUM)[0];
                registerData |= (Byte) value;
                WriteRegister(BME680_CTRL_HUM, registerData);
            }
        }

        /// <summary>
        ///     Gets or sets the temperature sampling rate.
        ///     <p>
        ///         Although this property is exposed, it is recommended against setting this property directly. Use the
        ///         <see cref="SetRecommendedMode" /> method to avoid improper settings.
        ///     </p>
        /// </summary>
        /// <value>
        ///     The temperature sampling rate. See the <seealso cref="SamplingRate" /> for oversampling rates.
        /// </value>
        /// <example>
        ///     Example usage:
        ///     <code language="C#">
        /// _environment.TemperatureSamplingRate = EnvironmentClick.OversamplingRates.Osr1;
        /// </code>
        /// </example>
        public SamplingRate TemperatureSamplingRate
        {
            get => (SamplingRate) (ReadRegister(BME680_CTRL_MEAS)[0] >> 5);
            set
            {
                Byte registerData = ReadRegister(BME680_CTRL_MEAS)[0];
                registerData |= (Byte) ((Byte) value << 5);
                WriteRegister(BME680_CTRL_MEAS, registerData);
            }
        }

        /// <summary>
        ///     Gets or sets the pressure sampling rate.
        ///     <p>
        ///         Although this property is exposed, it is recommended against setting this property directly. Use the
        ///         <see cref="SetRecommendedMode" /> method to avoid improper settings.
        ///     </p>
        /// </summary>
        /// <value>
        ///     The pressure sampling rate. See the <seealso cref="SamplingRate" /> for oversampling rates.
        /// </value>
        /// <example>
        ///     Example usage:
        ///     <code language="C#">
        /// _environment.PressureSamplingRate = EnvironmentClick.OversamplingRates.Osr1;
        /// </code>
        /// </example>
        public SamplingRate PressureSamplingRate
        {
            get => (SamplingRate) ((ReadRegister(BME680_CTRL_MEAS)[0] >> 2) & 0x07);
            set
            {
                Byte registerData = ReadRegister(BME680_CTRL_MEAS)[0];
                registerData |= (Byte) ((Byte) value << 2);
                WriteRegister(BME680_CTRL_MEAS, registerData);
            }
        }

        /// <summary>
        ///     Gets the identifier of the chip.
        /// </summary>
        /// <value>
        ///     Should be 0x60. Other value means error.
        /// </value>
        /// <example>
        ///     Example usage:
        ///     <code language="C#">
        /// Debug.Print("Device ID is " + _environment.DeviceId);
        /// </code>
        /// </example>
        public Byte DeviceId => ReadRegister(BME680_ID)[0];

        /// <summary>
        ///     This property indicates whether the last gas meaurement resulted in a valid reading.
        /// </summary>
        /// <returns>Returns true if the last gas reading was valid or otherwise false.</returns>
        /// <remarks>This property is updated after each gas measurement.</remarks>
        /// <example>
        ///     <code language="C#">
        /// Debug.Print("Gas Reading Valid? .....: " + _environment.GasReadingValid);
        /// </code>
        /// </example>
        public Boolean GasReadingValid { get; private set; }

        /// <summary>
        ///     This property indicates whether the hot plate reached the desired temperature in the allocated time using the
        ///     selected Heater Profile.
        /// </summary>
        /// <returns>Returns true if the hot plate reached the desired temperature in the specified time or otherwise false.</returns>
        /// <remarks>This property is updated after each gas measurement.</remarks>
        /// <example>
        ///     <code language="C#">
        /// Debug.Print("Gas Heater Stable?......: " + _environment.HeaterStable);
        /// </code>
        /// </example>
        public Boolean HeaterStable { get; private set; }

        #endregion

        #region Public Methods

        /// <summary>
        ///     Reads the sea level compensated Altitude in meters.
        /// </summary>
        /// <returns>The sea level compensated Altitude in meters.</returns>
        /// <example>
        ///     <code language="C#">
        /// Debug.Print("Altitude................: " + _environment.ReadAltitude().ToString("F1") + " meters");
        /// </code>
        /// </example>
        public Single ReadAltitude()
        {
            if (TemperatureSamplingRate == SamplingRate.Skipped || PressureSamplingRate == SamplingRate.Skipped) return Single.MinValue;
            Single pressure = ReadPressure();
            return (Single) (44330.77F * (1.0F - Math.Pow(pressure * 100F / CalculatePressureAsl(pressure * 100F), 0.190263F)));
        }

        /// <summary>
        ///     Reads temperature, pressure and relative humidity respectively.
        /// </summary>
        /// <returns>
        ///     Returns an array of <see cref="System.Single" /> containing the Temperature in Celsius, Pressure in mBar and
        ///     Relative Humidity respectively.
        /// </returns>
        /// <remarks>The Pressure reading is not Sea Level compensated.</remarks>
        /// <example>
        ///     <code language="C#">
        /// float[] _readings = new float[3];
        /// _readings = _environment.ReadSensor();
        /// Debug.Print("Temperature.....: " + _readings[0].ToString("F2") + " °C");
        /// Debug.Print("Pressure........: " + _readings[1].ToString("F1") + " mBar");
        /// Debug.Print("Humidity........: " + _readings[2].ToString("F2") + " %RH\n");
        /// </code>
        /// </example>
        public Single[] ReadSensor()
        {
            Single[] result = {0.0F, 0.0F, 0.0F};

            ForcedRead(false);

            Byte[] registerData = ReadRegister(BME680_PRESS_MSB, 8);
            result[0] = CompensateTemperature((registerData[3] << 12) | (registerData[4] << 4) | (registerData[5] >> 4));
            result[1] = CompensatePressure((registerData[0] << 12) | (registerData[1] << 4) | (registerData[2] >> 4)) / 100F;
            result[2] = CompensateHumidity((registerData[6] << 8) | registerData[7]);

            return result;
        }

        /// <summary>
        ///     Sets operating mode according to Bosch recommended modes of operation.
        /// </summary>
        /// <param name="mode">The desired mode.</param>
        /// <example>
        ///     Example usage:
        ///     <code language="C#">
        /// _environment.SetRecommendedMode(EnvironmentClick.RecommendedModes.WeatherMonitoring);
        /// </code>
        /// </example>
        public void SetRecommendedMode(RecommendedModes mode)
        {
            switch (mode)
            {
                case RecommendedModes.Gaming:
                {
                    HumiditySamplingRate = SamplingRate.Skipped;
                    TemperatureSamplingRate = SamplingRate.Osr1;
                    PressureSamplingRate = SamplingRate.Osr4;
                    Filter = FilterCoefficient.IIR127;
                    break;
                }
                case RecommendedModes.HumiditySensing:
                {
                    HumiditySamplingRate = SamplingRate.Osr1;
                    TemperatureSamplingRate = SamplingRate.Osr1;
                    PressureSamplingRate = SamplingRate.Skipped;
                    Filter = FilterCoefficient.IIROff;
                    break;
                }
                case RecommendedModes.IndoorNavigation:
                {
                    HumiditySamplingRate = SamplingRate.Osr1;
                    TemperatureSamplingRate = SamplingRate.Osr1;
                    PressureSamplingRate = SamplingRate.Osr16;
                    Filter = FilterCoefficient.IIR3;
                    break;
                }
                case RecommendedModes.WeatherMonitoring:
                {
                    HumiditySamplingRate = SamplingRate.Osr1;
                    TemperatureSamplingRate = SamplingRate.Osr1;
                    PressureSamplingRate = SamplingRate.Osr1;
                    Filter = FilterCoefficient.IIR1;
                    break;
                }
                case RecommendedModes.FullPower:
                {
                    HumiditySamplingRate = SamplingRate.Osr16;
                    TemperatureSamplingRate = SamplingRate.Osr16;
                    PressureSamplingRate = SamplingRate.Osr16;
                    Filter = FilterCoefficient.IIR63;
                    break;
                }
                default:
                {
                    throw new ArgumentOutOfRangeException(nameof(mode));
                }
            }
        }

        /// <summary>
        ///     Selects heater profile that will be used for gas resistance measurement.
        /// </summary>
        /// <param name="heaterProfile">Heater profile to set-point.</param>
        /// <example>
        ///     <code language="C#">
        /// _environment.SelectHeaterProfile(EnvironmentClick.HeaterProfiles.Profile_0);
        /// </code>
        /// </example>
        public void SelectHeaterProfile(HeaterProfiles heaterProfile)
        {
            WriteRegister(BME680_CTRL_GAS_1, (Byte) heaterProfile);
        }

        /// <summary>
        ///     The operation of the gas sensing part of EnvironmentClick Click (BME680) involves two steps.
        ///     1.) Heating the gas sensor hot plate to a target temperature (typically between 200 °C and 400 °C) and keep that
        ///     temperature for a certain duration of time.
        ///     2.) Measuring the resistance of the gas sensitive layer
        /// </summary>
        /// <param name="targetTemperature">The hot plate target temperature (typically between 200 °C and 400 °C)</param>
        /// <param name="heaterOnTime">The duration of time to keep the gas sensor hot plate at specified temperature.</param>
        /// <param name="profile">One of the <see cref="HeaterProfiles" /> to assign this temperature/time pair to.</param>
        /// <example>
        ///     <code language="C#">
        /// _environment.SetHeaterProfile(250, 250, EnvironmentClick.HeaterProfiles.Profile_0);
        /// </code>
        /// </example>
        public void SetHeaterProfile(UInt16 targetTemperature, UInt16 heaterOnTime, HeaterProfiles profile)
        {
            const Byte baseWaitRegister = BME680_GAS_WAIT_0;
            const Byte baseHeatRegister = BME680_RES_HEAT_0;

            // Define heater-on time.
            WriteRegister((Byte) (baseWaitRegister + profile), CalculateHeatDuration(heaterOnTime));

            // Set heater temperature.
            WriteRegister((Byte) (baseHeatRegister + profile), CalculateHeaterResistance(targetTemperature));
        }

        /// <summary>
        ///     Reads the gas resistance.
        /// </summary>
        /// <remarks>
        ///     Though not true Indoor Air Quality (IAQ) from the Bosch BSEC Software, the gas resistance will indicate air
        ///     quality. The higher the value, the better the air quality. The lower the value, the more polluted the air.
        /// </remarks>
        /// <returns>Gas resistance in kOhms.</returns>
        /// <example>
        ///     <code language="C#">
        /// Debug.Print("Gas Resistance..........: " + _environment.ReadGasResistance() + " kOhms");
        /// </code>
        /// </example>
        public Single ReadGasResistance()
        {
            ForcedRead(true);

            Int32 rawTemp = (this as ITemperature).RawData;

            Byte[] registerData = ReadRegister(BME680_GAS_R_MSB, 3);
            Int32 gasResistance = (registerData[0] << 2) | (registerData[1] >> 6);
            Int32 gasRange = registerData[1] & 0x0F;

            CalibrationData.calAmbTemp = CompensateTemperature(rawTemp);
            Single val = CalculateGasResistance(gasResistance, gasRange);

            // Update the GasReadingValid and HeaterStable properties.
            GasReadingValid = (registerData[1] & 0x20) == 0x20;
            HeaterStable = (registerData[1] & 0x10) == 0x10;

            return val / 1000.0F;
        }

        /// <summary>
        ///     Resets the module
        /// </summary>
        /// <example>
        ///     Example usage:
        ///     <code language="C#">
        /// _environment.Reset(ResetModes.Soft);
        /// </code>
        /// </example>
        public Boolean Reset()
        {
            WriteRegister(BME680_RESET, 0xB6);

            do
            {
                Thread.Sleep(10);
            } while (ReadRegister(BME680_RESET)[0] != 0);

            return ReadRegister(BME680_RESET)[0] == 0;
        }

        #endregion

        #region Interface Implementations

        /// <inheritdoc />
        /// <summary>
        ///     Reads the temperature.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <returns>
        ///     A single representing the temperature read from the source, degrees Celsius
        /// </returns>
        /// <exception cref="NotSupportedException">
        ///     An ApplicationException will be thrown if attempting to read object temperature
        ///     as the EnvironmentClick Click does not support object temperature measurement.
        /// </exception>
        /// <example>
        ///     Example usage:
        ///     <code language="C#">
        /// Debug.Print("Temperature is " + _environment.ReadTemperature(TemperatureSources.Ambient).ToString("F2") + " °C");
        /// </code>
        /// </example>
        public Single ReadTemperature(TemperatureSources source = TemperatureSources.Ambient)
        {
            if (source == TemperatureSources.Object) throw new NotSupportedException("The EnvironmentClick click does not support object temperature measurement.");
            if (TemperatureSamplingRate == SamplingRate.Skipped) return Single.MaxValue;
            ForcedRead(false);
            return CompensateTemperature((this as ITemperature).RawData);
        }

        /// <inheritdoc />
        /// <summary>
        ///     Reads the relative or absolute humidity value from the sensor.
        /// </summary>
        /// <returns>
        ///     A single representing the relative/absolute humidity as read from the sensor, in percentage (%) for relative
        ///     reading or value in case of absolute reading.
        /// </returns>
        /// <exception cref="NotSupportedException">
        ///     An ApplicationException will be thrown if attempting to read absolute pressure
        ///     as the EnvironmentClick Click does not support reading absolute humidity measurement.
        /// </exception>
        /// <example>
        ///     Example usage:
        ///     <code language="C#">
        /// Debug.Print("Humidity is " + _environment.ReadHumidity(HumidityMeasurementModes.Relative).ToString("F2") + " %RH");
        /// </code>
        /// </example>
        public Single ReadHumidity(HumidityMeasurementModes measurementMode = HumidityMeasurementModes.Relative)
        {
            if (measurementMode == HumidityMeasurementModes.Absolute)
                throw new NotSupportedException("The EnvironmentClick Click does not support absolute humidity measurement. Use Relative instead.");
            if (HumiditySamplingRate == SamplingRate.Skipped) return Single.MaxValue;
            ForcedRead(false);
            return CompensateHumidity((this as IHumidity).RawData);
        }

        /// <inheritdoc />
        /// <summary>
        ///     Reads the pressure from the sensor.
        /// </summary>
        /// <param name="compensationMode">
        ///     Indicates if the pressure reading returned by the sensor is see-level compensated or
        ///     not.
        /// </param>
        /// <returns>
        ///     A <see cref="T:System.Single" /> representing the pressure read from the source, in milliBars.
        /// </returns>
        /// <example>
        ///     Example usage:
        ///     <code language="C#">
        /// Debug.Print("Pressure is " + _environment.ReadPressure(PressureCompensationModes.Uncompensated).ToString("F1") + " mBar");
        /// </code>
        /// </example>
        public Single ReadPressure(PressureCompensationModes compensationMode = PressureCompensationModes.SeaLevelCompensated)
        {
            if (TemperatureSamplingRate == SamplingRate.Skipped || PressureSamplingRate == SamplingRate.Skipped) return Single.MaxValue;
            ForcedRead(false);
            Single rawPressure = CompensatePressure((this as IPressure).RawData);
            return compensationMode == PressureCompensationModes.Uncompensated
                ? rawPressure / 100.0F
                : CalculatePressureAsl(rawPressure) / 100.0F;
        }

        /// <inheritdoc />
        /// <summary>Gets the raw data of the temperature value.</summary>
        /// <value>
        ///     Raw data in the range depending on sensor's precision (8/10/12 bits, for example)
        /// </value>
        /// <example>
        ///     <code language="C#">
        /// Debug.Print((_environment as ITemperature).RawData.ToString());
        /// </code>
        /// </example>
        Int32 ITemperature.RawData
        {
            get
            {
                while (Measuring()) Thread.Sleep(10);
                Byte[] registerData = ReadRegister(BME680_TEMP_MSB, 3);
                return (registerData[0] << 12) | (registerData[1] << 4) | (registerData[2] >> 4);
            }
        }

        /// <inheritdoc />
        /// <summary>Gets the raw data of the pressure value.</summary>
        /// <value>
        ///     Raw data in the range depending on sensor's precision (8/10/12 bits, for example)
        /// </value>
        /// <example>
        /// <code language="C#">
        /// Debug.Print((_environment as IPressure).RawData.ToString());
        /// </code>
        /// </example>
        Int32 IPressure.RawData
        {
            get
            {
                while (Measuring()) Thread.Sleep(10);
                Byte[] registerData = ReadRegister(BME680_PRESS_MSB, 3);
                return (registerData[0] << 12) | (registerData[1] << 4) | (registerData[2] >> 4);
            }
        }

        /// <inheritdoc />
        /// <summary>Gets the raw data of the humidity value.</summary>
        /// <value>
        ///     Raw data in the range depending on sensor's precision (8/10/12 bits, for example)
        /// </value>
        /// <example>
        /// <code language="C#">
        /// Debug.Print((_environment as IHumidity).RawData.ToString());
        /// </code>
        /// </example>
        Int32 IHumidity.RawData
        {
            get
            {
                while (Measuring()) Thread.Sleep(10);
                Byte[] registerData = ReadRegister(BME680_HUM_MSB, 2);
                return (registerData[0] << 8) | registerData[1];
            }
        }

        /// <summary>
        /// //ToDo - Scale temperature readings according to TemperUnit property.
        /// </summary>
        public TemperatureUnits TemperatureUnit { get; set; }

        #endregion
    }
}