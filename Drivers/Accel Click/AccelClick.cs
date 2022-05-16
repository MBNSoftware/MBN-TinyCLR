/*
Modified March 2020 by Christophe Gerbier to accomodate with MBN driver's scheme
and add Single Tap/Double Tap detection, with needed properties (Threshold, duration, etc...)

Copyright 2010 Robert Heffernan. All rights reserved.
 
Redistribution and use in source and binary forms, with or without modification, are
permitted provided that the following conditions are met:
 
   1. Redistributions of source code must retain the above copyright notice, this list of
      conditions and the following disclaimer.
 
   2. Redistributions in binary form must reproduce the above copyright notice, this list
      of conditions and the following disclaimer in the documentation and/or other materials
      provided with the distribution.
 
THIS SOFTWARE IS PROVIDED BY Robert Heffernan "AS IS" AND ANY EXPRESS OR IMPLIED
WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND
FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL Robert Heffernan OR
CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR
CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR
SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON
ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING
NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF
ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 
The views and conclusions contained in the software and documentation are those of the
authors and should not be interpreted as representing official policies, either expressed
or implied, of Robert Heffernan.
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
    /// A class to interface to the ADXL345 3-Axis ±2g, ±4g, ±8g, ±16g Accelerometer from Analog Devices (In I2C Mode, the class can be modified to use SPI)
    /// Product Page: http://www.analog.com/en/sensors/inertial-sensors/adxl345/products/product.html
    /// Datasheet: http://www.analog.com/static/imported-files/data_sheets/ADXL345.pdf
    /// </summary>
    /// <example> This sample shows basic sensor's features.
    /// <code language="C#">
    /// using MBN.Modules;
    /// using Microsoft.SPOT;
    /// using MBN;
    /// using System.Threading;
    /// 
    /// namespace Examples
    /// {
    ///     public class Program
    ///     {
    ///         private static AccelClick _accel;
    /// 
    ///         public static void Main()
    ///         {
    ///             _accel = new AccelClick(Hardware.SocketOne);
    ///             Debug.Print("Device ID : " + _accel.DeviceID);
    /// 
    ///             // Set the sensor to fixed resolution, updating every 10ms
    ///             _accel.OutputResolution = AccelClick.OutputResolutions.FixedResoultion;
    ///             _accel.UpdateDelay = 10;
    /// 
    ///             // Single tap/double tap enabled by default, so capture the associated events
    ///             _accel.OnDoubleTap += Accel_OnDoubleTap;
    ///             _accel.OnSingleTap += Accel_OnSingleTap;
    /// 
    ///             // Start polling
    ///             _accel.Start();
    /// 
    ///             while (true)
    ///             {
    ///                 // Prints the 3 axis acceleration values
    ///                 Debug.Print(_accel.CurrentData.ToString());
    /// 
    ///                 Thread.Sleep(100);
    ///             }
    ///         }
    /// 
    ///         static void Accel_OnSingleTap(object sender, EventArgs e)
    ///         {
    ///             Hardware.Led2.Write(true);
    ///             Thread.Sleep(20);
    ///             Hardware.Led2.Write(false);
    ///         }
    /// 
    ///         static void Accel_OnDoubleTap(object sender, EventArgs e)
    ///         {
    ///             Hardware.Led1.Write(true);
    ///             Thread.Sleep(20);
    ///             Hardware.Led1.Write(false);
    ///         }
    ///     }
    /// }
    /// </code>
    /// </example>
    public sealed partial class AccelClick
    {
        /// <summary>
        /// An enum specifying the G Range currently used by the sensor
        /// </summary>
        public enum AccelerationRanges : Byte
        {
            /// <summary>
            /// ±2g
            /// </summary>
            TwoG = 0x00,
            /// <summary>
            /// ±4g
            /// </summary>
            FourG = 0x01,
            /// <summary>
            /// ±8g
            /// </summary>
            EightG = 0x02,
            /// <summary>
            /// ±16g
            /// </summary>
            SixteenG = 0x03
        }

        /// <summary>
        /// An enum specifying the Output Resolution (10bit or Full Range) used by the sensor
        /// </summary>
        public enum OutputResolutions : Byte
        {
            /// <summary>
            /// User's defined resolution
            /// </summary>
            FixedResoultion = 0x00,
            /// <summary>
            /// 10 bits resolution
            /// </summary>
            FullResolution = 0x08
        }
        /// <summary>
        /// Free fall event handler
        /// </summary>
        public event EventHandler OnFreefall = delegate { };
        public event EventHandler OnSingleTap = delegate { };
        public event EventHandler OnDoubleTap = delegate { };


        /// <summary>
        /// A structure holding the current acceleration values returned from the sensor
        /// </summary>
        public struct SensorData
        {
            /// <summary>
            /// The raw X-Axis acceleration data
            /// </summary>
            public Int16 RawX;

            /// <summary>
            /// The raw Y-Axis acceleration data
            /// </summary>
            public Int16 RawY;

            /// <summary>
            /// The raw Z-Axis acceleration data
            /// </summary>
            public Int16 RawZ;

            /// <summary>
            /// The X-Axis acceleration data (in Gs)
            /// </summary>
            public Single X;

            /// <summary>
            /// The Y-Axis acceleration data (in Gs)
            /// </summary>
            public Single Y;

            /// <summary>
            /// The Z-Axis acceleration data (in Gs)
            /// </summary>
            public Single Z;

            public override String ToString() => "X=" + X + "g, Y=" + Y + "g, Z=" + Z + "g";
        }

        /// <summary>
        /// A private helper enum providing descriptive names for the sensor registers
        /// </summary>
        private enum RegisterMap : Byte
        {
            DEVID = 0x00,
            THRESH_TAP = 0x1d,
            OFSX = 0x1e,

            OFSY = 0x1f,
            OFSZ = 0x20,
            DUR = 0x21,
            LATENT = 0x22,
            WINDOW = 0x23,
            THRESH_ACT = 0x24,
            THRESH_INACT = 0x25,
            TIME_INACT = 0x26,
            ACT_INACT_CTL = 0x27,
            THRESH_FF = 0x28,
            TIME_FF = 0x29,
            TAP_AXES = 0x2a,
            ACT_TAP_STATUS = 0x2b,
            BW_RATE = 0x2c,
            POWER_CTL = 0x2d,
            INT_ENABLE = 0x2e,
            INT_MAP = 0x2f,
            INT_SOURCE = 0x30,
            DATA_FORMAT = 0x31,
            DATAX0 = 0x32,
            // DATAX1 to DATAZ1 for reference only. They are not used because read is done in one call with a 6 bytes array
            DATAX1 = 0x33,
            DATAY0 = 0x34,
            DATAY1 = 0x35,
            DATAZ0 = 0x36,
            DATAZ1 = 0x37,
            FIFO_CTL = 0x38,
            FIFO_STATUS = 0x39
        }

        /// <summary>
        /// A private helper enum providing descriptive names for the interrupt source register's bits
        /// </summary>
        [Flags]
        private enum InterruptSource : Byte
        {
            DATA_READY = 0x80,
            SINGLE_TAP = 0x40,
            DOUBLE_TAP = 0x20,
            ACTIVITY = 0x10,

            INACTIVITY = 0x08,
            FREE_FALL = 0x04,
            WATERMARK = 0x02,
            OVERRUN = 0x01
        }

        #region Private variables

        private AccelerationRanges _curRange = AccelerationRanges.SixteenG;
        private OutputResolutions _curRes = OutputResolutions.FullResolution;
        private SensorData _sensorData;
        private readonly I2cDevice _accel;
        private readonly GpioPin intPin;
        private Boolean _scanThread;
        private Boolean _singleTapEnabled, _doubleTapEnabled, _freefallEnabled;
        private readonly Hardware.Socket _socket;

        #endregion

        /// <summary>
        /// Initializes a new instance of the <see cref="AccelClick"/> class.
        /// </summary>
        /// <param name="socket">The <see cref="Hardware.Socket"/> that the Accel Click board is inserted into.</param>
        /// <param name="address">Address of the module. Defaults to 0x1D.</param>
        /// <param name="autoStart">if set to <c>true</c> [automatic start].</param>
        /// <exception cref="SystemException">ADXL345 not detected</exception>
        public AccelClick(Hardware.Socket socket, Byte address=0x1D, Boolean autoStart=false)
        {
            _socket = socket;
            // Create the driver's I²C configuration
#if (NANOFRAMEWORK_1_0)
            _accel = I2cDevice.Create(new I2cConnectionSettings(socket.I2cBus, address, I2cBusSpeed.StandardMode));
#else
            _accel = I2cController.FromName(socket.I2cBus).GetDevice(new I2cConnectionSettings(address, 100000));
#endif

            if (ReadRegister((Byte)RegisterMap.DEVID) != 0xE5) { throw new SystemException("ADXL345 not detected"); }

#if (NANOFRAMEWORK_1_0)
            intPin = new GpioController().OpenPin(socket.Int, PinMode.Input);
#else
            intPin = GpioController.GetDefault().OpenPin(socket.Int);
            intPin.SetDriveMode(GpioPinDriveMode.Input);
#endif
            
            // Initialize our sensor
            InitSensor(autoStart);
        }

        /// <summary>
        /// Gets the device identifier.
        /// </summary>
        /// <example> This sample shows how to use the DeviceID property.
        /// <code language="C#">
        ///             // Gets the device identification byte. Should be 0xE5.
        ///             var AccelID = _accel.DeviceID;
        /// </code>
        /// </example>
        /// <value>
        /// The device identifier. Should be 0xE5, otherwise exception is thrown to show a not detected device.
        /// </value>
        /// <exception cref="System.SystemException">ADXL345 not detected</exception>
        public Byte DeviceID
        {
            get
            {
                if (ReadRegister((Byte) RegisterMap.DEVID) != 0xE5)
                {
                    throw new SystemException("ADXL345 not detected");
                }
                return 0xE5;
            }
        }

        /// <summary>
        /// Private function used to write data into the sensor's registers
        /// </summary>
        /// <param name="register">The register to write to</param>
        /// <param name="value">The value to write into the register</param>
        private void WriteRegister(RegisterMap register, Byte value)
        {
            lock (_socket.LockI2c)
            {
                _accel.Write(new[] { (Byte)register, value });
            }
        }

        /// <summary>
        /// Private function used to read data from the sensor's registers
        /// </summary>
        /// <param name="register">The register to read from</param>
        /// <returns>A byte value containing the data read from the sensor's register</returns>
        private Byte ReadRegister(RegisterMap register)
        {
            var data = new Byte[1];
            lock (_socket.LockI2c)
            {
                _accel.WriteRead(new[] { (Byte)register }, data);
            }

            return data[0];
        }

        /// <summary>
        /// A private function called by the constructor to initialize the sensor for operation and starts measurements
        /// </summary>
        private void InitSensor(Boolean autoStart)
        {
            // Bypass the FIFO
            WriteRegister(RegisterMap.FIFO_CTL, 0x0F); // Bypass FIFO, Trigger on Int1 and 32 FIFO samples

            // Setup freefall thresholds
            WriteRegister(RegisterMap.THRESH_FF, 0x05); // 315mg
            WriteRegister(RegisterMap.TIME_FF, 0x14); // 100ms
            _freefallEnabled = true;

            // Enable the 3 axis in tap detection
            WriteRegister(RegisterMap.TAP_AXES, 0x07);

            //Setup some default values for tap events
            WriteRegister(RegisterMap.DUR, 0x30);
            WriteRegister(RegisterMap.THRESH_TAP, 0x40);
            WriteRegister(RegisterMap.LATENT, 0x60);
            WriteRegister(RegisterMap.WINDOW, 0x80);
            _singleTapEnabled = true;
            _doubleTapEnabled = true;

            // Write interrupt table
            WriteRegister(RegisterMap.INT_ENABLE, (Byte)(InterruptSource.FREE_FALL | InterruptSource.SINGLE_TAP | InterruptSource.DOUBLE_TAP));

            // Setup data format
            WriteRegister(RegisterMap.DATA_FORMAT, (Byte)OutputResolutions.FullResolution | (Byte)AccelerationRanges.SixteenG); // 16g Full Res Mode

            // Enable measurement
            WriteRegister(RegisterMap.POWER_CTL, 0x00);
            Thread.Sleep(100);
            WriteRegister(RegisterMap.POWER_CTL, 0x08);

            intPin.ValueChanged += ADXL345_Interrupt_OnInterrupt;

            if (autoStart) { Start(); }
        }

        /// <summary>
        /// Starts polling the ADXL345.
        /// </summary>
        /// <example> This sample shows how to use the Start method.
        /// <code language="C#">
        ///             // Sets the polling delay to 10ms
        ///             _accel.UpdateDelay = 10;
        /// 
        ///             // Starts polling the sensor
        ///             _accel.Start();
        /// </code>
        /// </example>
        public void Start()
        {
            // Start background update thread
            _scanThread = true;
            new Thread(ADXL345_ThreadMain).Start();
        }

        /// <summary>
        /// Stops polling the ADXL345.
        /// </summary>
        /// <example> This sample shows how to use the Stop method.
        /// <code language="C#">
        ///             // Stops polling the sensor
        ///             _accel.Stop();
        /// </code>
        /// </example>
        public void Stop() => _scanThread = false;

        /// <summary>
        /// Specifies the delay (in milliseconds) used by the background thread to wait between updating the sensor readings
        /// </summary>
        /// <example> This sample shows how to use the UpdateDelay property.
        /// <code language="C#">
        ///             // Updates every 10ms;
        ///             _accel.UpdateDelay = 10;
        /// </code>
        /// </example>
        public Int16 UpdateDelay { get; set; } = 100;

        /// <summary>
        /// Specifies the output resolution the sensor uses when calculating the acceleration data
        /// </summary>
        /// <example> This sample shows how to use the OutputResolution property.
        /// <code language="C#">
        ///             // Sets the acceleration range to 4g
        ///             _accel.OutputResolution = AccelClick.OutputResolutions.FullResolution;
        /// </code>
        /// </example>
        public OutputResolutions OutputResolution
        {
            get
            {
                // Read the data format register
                var val = ReadRegister(RegisterMap.DATA_FORMAT);

                // Mask off the output resolution bit and return it as an OutputResolutions enum
                return (OutputResolutions)(val & 0x08);
            }

            set
            {
                // Read the existing value from the data format register
                var val = ReadRegister(RegisterMap.DATA_FORMAT);

                // Mask off the non output resolution values and then 'OR' with the new resolution value
                val = (Byte)((val & 0xF7) | (Byte)value);

                // Write the modified register back to the sensor
                WriteRegister(RegisterMap.DATA_FORMAT, val);

                // Update our internal conversion value
                _curRes = value;
            }
        }

        /// <summary>
        /// Specifies the G range used by the sensor when calculating acceleration data
        /// </summary>
        /// <example> This sample shows how to use the Range property.
        /// <code language="C#">
        ///             // Sets the acceleration range to 4g
        ///             _accel.Range = AccelClick.AccelerationRanges.FourG;
        /// </code>
        /// </example>
        public AccelerationRanges Range
        {
            get
            {
                // Read the data format register
                var val = ReadRegister(RegisterMap.DATA_FORMAT);

                // Mask off the G range values and return it as a gRange enum
                return (AccelerationRanges)(val & 0x03);
            }

            set
            {
                // Read the existing value from the data format register
                var val = ReadRegister(RegisterMap.DATA_FORMAT);

                // Mask off the non g-related values and then 'OR' with the new range value
                val = (Byte)((val & 0xFC) | (Byte)value);

                // Write the modified register back to the sensor
                WriteRegister(RegisterMap.DATA_FORMAT, val);

                // Update our internal conversion value
                _curRange = value;
            }
        }

        /// <summary>
        /// Specifies the time (in milliseconds, 5ms resolution) the sensor uses to detect a freefall event
        /// (The sensor needs to be under (FreefallThreshold)mg acceleration for (FreefallDetectTime)ms to register freefall
        /// </summary>
        /// <example> This sample shows how to use the FreefallDetectTime property.
        /// <code language="C#">
        ///             // Sets the freefall detection time to 10ms
        ///             _accel.FreefallDetectTime = 10;
        /// </code>
        /// </example>
        public UInt16 FreefallDetectTime
        {
            get
            {
                // Read the freefall time register
                var time = ReadRegister(RegisterMap.TIME_FF);

                // Convert the value into milliseconds and return it
                return (UInt16)(time * 5);
            }

            set
            {
                // Clamp and convert the time from milliseconds to a register value
                var val = (Byte)(((value > 1275) ? 1275 : value) / 5);

                // Write the new register value
                WriteRegister(RegisterMap.TIME_FF, val);
            }
        }

        /// <summary>
        /// Specifies the threshold (in milli-Gs, 62.5mg resolution) the sensor uses to detect a freefall event
        /// (The sensor needs to be under (FreefallThreshold)mg acceleration for (FreefallDetectTime)ms to register freefall
        /// </summary>
        /// <example> This sample shows how to use the FreefallThreshold property.
        /// <code language="C#">
        ///             Debug.Print ("Current freefall threshold = " + _accel.FreefallThreshold.ToString()+" milli-Gs");
        /// </code>
        /// </example>
        public Single FreefallThreshold
        {
            get
            {
                // Read the freefall threshold register
                var val = ReadRegister(RegisterMap.THRESH_FF);

                // Convert the register value into milliGs and return it
                return val * 62.5f;
            }

            set
            {
                // Clamp then convert the value from milliGs into a register value
                var val = (Byte)(((value > 15937.5f) ? 15937.5f : value) / 62.5f);

                // Write the new register value
                WriteRegister(RegisterMap.THRESH_FF, val);
            }
        }

        /// <summary>
        /// Gets or sets the tap latency.
        /// <para>The latent register is eight bits and contains an unsigned time value representing the wait time from the detection of a tap 
        /// event to the start of the time window (defined by the window register) during which a possible second tap event can be detected. 
        /// The scale factor is 1.25 ms/LSB. A value of 0 disables the double tap function. </para>
        /// <para>Minimum recommanded value : 0x10 (20ms)</para>
        /// </summary>
        /// <example> This sample shows how to use the TapLatency property.
        /// <code language="C#">
        ///             // Sets the tap latency to 20ms
        ///             _accel.TapLatency = 0x10;       // 16 * 1.25 ms
        /// </code>
        /// </example>
        /// <value>
        /// The tap latency.
        /// </value>
        public Byte TapLatency
        {
            get { return ReadRegister(RegisterMap.LATENT); }
            set { WriteRegister(RegisterMap.THRESH_FF, value); }
        }

        /// <summary>
        /// Gets or sets the tap threshold
        /// <para>The THRESH_TAP register is eight bits and holds the threshold value for tap interrupts. The data format is unsigned, therefore, 
        /// the magnitude of the tap event is compared with the value  in THRESH_TAP for normal tap detection. The scale factor is 
        /// 62.5 mg/LSB (that is, 0xFF = 16 g). A value of 0 may result in undesirable behavior if single tap/double tap interrupts are enabled.  </para>
        /// <para>Minimum recommanded value : 0x30 (3g)</para>
        /// </summary>
        /// <example> This sample shows how to use the TapThreshold property.
        /// <code language="C#">
        ///             // Sets the tap threshold to its minimum recommended value (3g)
        ///             _accel.TapThreshold = 0x30;     // 48 * 0.0625 g
        /// </code>
        /// </example>
        /// <value>
        /// The tap threshold
        /// </value>
        public Byte TapThreshold
        {
            get { return ReadRegister(RegisterMap.THRESH_TAP); }
            set { WriteRegister(RegisterMap.THRESH_TAP, value); }
        }

        /// <summary>
        /// Gets or sets the tap duration
        /// <para>The DUR register is eight bits and contains an unsigned time value representing the maximum time that an event must be 
        /// above the THRESH_TAP threshold to qualify as a tap event. The scale factor is 625 µs/LSB. A value of 0 disables the single tap/double tap functions. </para>
        /// <para>Minimum recommanded value : 0x10 (10ms)</para>
        /// </summary>
        /// <example> This sample shows how to use the TapDuration property.
        /// <code language="C#">
        ///             // Set the tap duration to 10ms
        ///             _accel.TapDuration = 0x10;      // 16 * 0.625 ms
        /// </code>
        /// </example>
        /// <value>
        /// The tap duration.
        /// </value>
        public Byte TapDuration
        {
            get { return ReadRegister(RegisterMap.DUR); }
            set { WriteRegister(RegisterMap.DUR, value); }
        }

        /// <summary>
        /// Gets or sets the tap window time.
        /// <para>The window register is eight bits and contains an unsigned time value representing the amount of time after the expiration of the 
        /// latency time (determined by the latent register) during which a second valid tap can begin. The scale factor is 1.25 ms/LSB. A 
        /// value of 0 disables the double tap function. </para>
        /// <para>Minimum recommanded value : 0x40 (80ms)</para>
        /// </summary>
        /// <example> This sample shows how to use the TapWindow property.
        /// <code language="C#">
        ///             // Set tap window time to 80ms
        ///             _accel.TapWindow = 0x40;    // 64 * 1.25 ms
        /// </code>
        /// </example>
        /// <value>
        /// The tap window time
        /// </value>
        public Byte TapWindow
        {
            get { return ReadRegister(RegisterMap.WINDOW); }
            set { WriteRegister(RegisterMap.WINDOW, value); }
        }

        /// <summary>
        /// Gets or sets a value indicating whether Single tap detection is enabled.
        /// </summary>
        /// <example> This sample shows how to use the SingleTapEnabled property.
        /// <code language="C#">
        ///             Debug.Print ("Is single-tap enabled ? " + _accel.SingleTapEnabled);
        /// </code>
        /// </example>
        /// <value>
        ///   <c>true</c> if [single tap detection enabled]; otherwise, <c>false</c>.
        /// </value>
        public Boolean SingleTapEnabled
        {
            get { return _singleTapEnabled; }
            set
            {
                WriteRegister(RegisterMap.INT_ENABLE, Bits.Set(ReadRegister(RegisterMap.INT_ENABLE), 6, value));
                _singleTapEnabled = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether Double tap detection is enabled.
        /// </summary>
        /// <example> This sample shows how to use the DoubleTapEnabled property.
        /// <code language="C#">
        ///             Debug.Print ("Is double-tap enabled ? " + _accel.DoubleTapEnabled);
        /// </code>
        /// </example>
        /// <value>
        ///   <c>true</c> if [double tap detection enabled]; otherwise, <c>false</c>.
        /// </value>
        public Boolean DoubleTapEnabled
        {
            get { return _doubleTapEnabled; }
            set
            {
                WriteRegister(RegisterMap.INT_ENABLE, Bits.Set(ReadRegister(RegisterMap.INT_ENABLE), 5, value));
                _doubleTapEnabled = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether Free-fall detection is enabled.
        /// </summary>
        /// <example> This sample shows how to use the FreefallEnabled property.
        /// <code language="C#">
        ///             _accel.FreefallEnabled = true;
        /// </code>
        /// </example>
        /// <value>
        ///   <c>true</c> if [Free-fall detection enabled]; otherwise, <c>false</c>.
        /// </value>
        public Boolean FreefallEnabled
        {
            get { return _freefallEnabled; }
            set
            {
                WriteRegister(RegisterMap.INT_ENABLE, Bits.Set(ReadRegister(RegisterMap.INT_ENABLE), 2, value));
                _freefallEnabled = value;
            }
        }

        /// <summary>
        /// gets the current acceleration data from the sensor
        /// </summary>
        public SensorData CurrentData
        {
            get { return _sensorData; }
        }

        /// <summary>
        /// A private callback function called when there is an interrupt generated from the sensor
        /// </summary>
        //private void ADXL345_Interrupt_OnInterrupt(uint data1, uint data2, DateTime time)
#if (NANOFRAMEWORK_1_0)
        private void ADXL345_Interrupt_OnInterrupt(Object sender, PinValueChangedEventArgs e)
        {
#else
        private void ADXL345_Interrupt_OnInterrupt(Object sender, GpioPinValueChangedEventArgs e)
        {
#endif
            // Read the interrupt source register to see what caused the event
            var source = (InterruptSource) ReadRegister(RegisterMap.INT_SOURCE);

            if ((source & InterruptSource.FREE_FALL) == InterruptSource.FREE_FALL)
            {
                EventHandler onFf = OnFreefall;
                onFf(this, null);
            }

            if ((source & InterruptSource.DOUBLE_TAP) == InterruptSource.DOUBLE_TAP)
            {
                EventHandler onDt = OnDoubleTap;
                onDt(this, null);
                return;
            }

            if ((source & InterruptSource.SINGLE_TAP) == InterruptSource.SINGLE_TAP)
            {
                EventHandler onSt = OnSingleTap;
                onSt(this, null);
            }
        }


        /// <summary>
        /// The main thread function used to update the acceleration values from the sensor
        /// </summary>
        private void ADXL345_ThreadMain()
        {
            // Create the transactions and buffers used to read from the sensor. Only do it once here to save constantly reallocating the same stuff over and over
            var accelData = new Byte[6];

            while (_scanThread)
            {
                lock (_socket.LockI2c)
                {
                    _accel.WriteRead(new[] { (Byte)RegisterMap.DATAX0 }, accelData);
                }

                // Convert the raw byte data into the raw acceleration data for each axis
                _sensorData.RawX = (Int16)(accelData[1] << 8 | accelData[0]);
                _sensorData.RawY = (Int16)(accelData[3] << 8 | accelData[2]);
                _sensorData.RawZ = (Int16)(accelData[5] << 8 | accelData[4]);

                // Take the raw acceleration data and convert it into Gs
                if (_curRes == OutputResolutions.FullResolution)
                {
                    // In "Full Resolution" mode, the sensor maintains a 4mg/LSB resolution
                    _sensorData.X = 0.004f * _sensorData.RawX;
                    _sensorData.Y = 0.004f * _sensorData.RawY;
                    _sensorData.Z = 0.004f * _sensorData.RawZ;
                }
                else
                {
                    // Call the function to convert the fixed resolution to Gs
                    ConvertFixedRange();
                }

                // Sleep until the next update
                Thread.Sleep(UpdateDelay);
            }
        }

        /// <summary>
        /// A private function to convert the raw fixed mode axis data into Gs
        /// </summary>
        private void ConvertFixedRange()
        {
            var res = 0.0f;

            // Get the resolution for the current G range
            switch (_curRange)
            {
                case AccelerationRanges.TwoG:
                    res = 2.0f / 512.0f;
                    break;

                case AccelerationRanges.FourG:
                    res = 4.0f / 512.0f;
                    break;

                case AccelerationRanges.EightG:
                    res = 8.0f / 512.0f;
                    break;

                case AccelerationRanges.SixteenG:
                    res = 16.0f / 512.0f;
                    break;
            }

            // Convert the raw data to Gs
            _sensorData.X = res * _sensorData.RawX;
            _sensorData.Y = res * _sensorData.RawY;
            _sensorData.Z = res * _sensorData.RawZ;
        }
    }
}
