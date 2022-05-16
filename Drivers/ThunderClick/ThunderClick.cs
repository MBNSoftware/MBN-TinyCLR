/*
 * Thunder Click board driver for TinyCLR 2.0
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
using System.Device.Gpio;
using System.Device.Spi;
#else
using GHIElectronics.TinyCLR.Devices.Gpio;
using GHIElectronics.TinyCLR.Devices.Spi;
#endif

using System;
using System.Threading;

namespace MBN.Modules
{
    /// <summary>
    /// Main class for the ThunderClick board driver
    /// <para><b>Pins used :</b> Miso, Mosi, Cs, Sck, Int</para>
    /// </summary>
    /// <example> This sample shows basic sensor's features.
    /// <code language="C#">
    /// using System.Threading;
    /// using MBN;
    /// using MBN.Modules;
    /// using Microsoft.SPOT;
    /// 
    /// namespace TestApp
    /// {
    ///     public class Program
    ///     {
    ///         static ThunderClick _thunder;
    /// 
    ///         public static void Main()
    ///         {
    ///             // Create the instance
    ///             _thunder = new ThunderClick(Hardware.SocketOne);
    /// 
    ///             // Subscribe to events
    ///             _thunder.LightningDetected += TH_LightningDetected;
    ///             _thunder.DisturbanceDetected += TH_DisturbanceDetected;
    ///             _thunder.NoiseDetected += TH_NoiseDetected;
    /// 
    ///             // Some information
    ///             Debug.Print("Continuous input noise level " + _thunder.ContinuousInputNoiseLevel + " µV rms");
    /// 
    ///             // Start interrupt scanning
    ///             _thunder.StartIRQ();
    /// 
    ///             Thread.Sleep(Timeout.Infinite);
    ///         }
    /// 
    ///         static void TH_NoiseDetected(object sender, EventArgs e)
    ///         {
    ///             Debug.Print("Noise detected");
    ///         }
    /// 
    ///         static void TH_DisturbanceDetected(object sender, EventArgs e)
    ///         {
    ///             Debug.Print("Disturbance detected");
    ///         }
    /// 
    ///         static void TH_LightningDetected(object sender, MBN.Events.ThunderClickLightningEventArgs e)
    ///         {
    ///             Debug.Print("Lightning detected at " + e.Distance + " km, energy : " + e.Energy);
    ///         }
    ///     }
    /// }
    /// </code>
    /// </example>
    public sealed partial class ThunderClick
    {
        /// <summary>
        /// Occurs when lightning is detected.
        /// </summary>
        /// <example> This sample shows usage of the LightningDetected event.
        /// <code language="C#">
        /// using System.Threading;
        /// using MBN;
        /// using MBN.Modules;
        /// using Microsoft.SPOT;
        /// 
        /// namespace TestApp
        /// {
        ///     public class Program
        ///     {
        ///         static ThunderClick _thunder;
        /// 
        ///         public static void Main()
        ///         {
        ///             // Create the instance
        ///             _thunder = new ThunderClick(Hardware.SocketOne);
        /// 
        ///             // Subscribe to event
        ///             _thunder.LightningDetected += TH_LightningDetected;
        /// 
        ///             // Start interrupt scanning
        ///             _thunder.StartIRQ();
        /// 
        ///             Thread.Sleep(Timeout.Infinite);
        ///         }
        /// 
        ///         static void TH_LightningDetected(object sender, MBN.Events.ThunderClickLightningEventArgs e)
        ///         {
        ///             Debug.Print("Lightning detected at " + e.Distance + " km, energy : " + e.Energy);
        ///         }
        ///     }
        /// }
        /// </code>
        /// </example>
        public event LightningEventHandler LightningDetected = delegate { };

        /// <summary>
        /// Occurs when a disturbance is detected.
        /// </summary>
        /// <example> This sample shows usage of the DisturbanceDetected event.
        /// <code language="C#">
        ///         static ThunderClick _thunder;
        /// 
        ///         public static void Main()
        ///         {
        ///             // Create the instance
        ///             _thunder = new ThunderClick(Hardware.SocketOne);
        /// 
        ///             // Subscribe to event
        ///             _thunder.DisturbanceDetected += TH_DisturbanceDetected;
        /// 
        ///             // Start interrupt scanning
        ///             _thunder.StartIRQ();
        /// 
        ///             Thread.Sleep(Timeout.Infinite);
        ///         }
        /// 
        ///         static void TH_DisturbanceDetected(object sender, EventArgs e)
        ///         {
        ///             Debug.Print("Disturbance detected");
        ///         }
        ///     }
        /// }
        /// </code>
        /// </example>
        public event EventHandler DisturbanceDetected = delegate { };

        /// <summary>
        /// Occurs when noise is detected.
        /// </summary>
        /// <example> This sample shows usage of the NoiseDetected event.
        /// <code language="C#">
        ///         static ThunderClick _thunder;
        /// 
        ///         public static void Main()
        ///         {
        ///             // Create the instance
        ///             _thunder = new ThunderClick(Hardware.SocketOne);
        /// 
        ///             // Subscribe to event
        ///             _thunder.NoiseDetected += TH_NoiseDetected;
        /// 
        ///             // Start interrupt scanning
        ///             _thunder.StartIRQ();
        /// 
        ///             Thread.Sleep(Timeout.Infinite);
        ///         }
        /// 
        ///         static void TH_NoiseDetected(object sender, EventArgs e)
        ///         {
        ///             Debug.Print("Noise detected");
        ///         }
        /// </code>
        /// </example>
        public event EventHandler NoiseDetected = delegate { };

        
        /// <summary>
        /// Analog front-end values
        /// </summary>
        /// <example> This sample shows how to use the AnalogFrontEnd property (AFE).
        /// <code language="C#">
        ///             _thunder.AnalogFrontEnd = ThunderClick.AFE.Outdoor;
        ///             _thunder.AnalogFrontEnd = ThunderClick.AFE.Indoor;
        /// </code>
        /// </example>
        public enum AFE
        {
            /// <summary>
            /// Value when the sensor operates indoor.
            /// </summary>
            /// <remarks>Please see datasheet as this setting will cause different readings whether it is set to Indoor or Outdoor. Default value is ThunderClick.AFE.Indoor.</remarks>
            Indoor = 0x12,
            /// <summary>
            /// Value when the sensor operates outdoor.
            /// </summary>
            /// <remarks>Please see datasheet as this setting will cause different readings whether it is set to Indoor or Outdoor. Default value is ThunderClick.AFE.Indoor.</remarks>
            OutDoor = 0x0E
        };

        private readonly SpiDevice _thunder;
        private AFE _mode;
        private readonly Byte[] _result = new Byte[2];
        private Byte _nfl,_spikeRejection, _minNumberLightning;
        private PowerModes _powerMode;
        private readonly Int32[] INL_Indoor;
        private readonly Int32[] INL_Outdoor;
        private const Byte PRESET_DEFAULT = 0x3C;
        private const Byte CALIB_RCO = 0x3D;
        private readonly GpioPin IRQ;
        private readonly Hardware.Socket _socket;

        /// <summary>
        /// Initializes a new instance of the <see cref="ThunderClick"/> class.
        /// </summary>
        /// <param name="socket">The socket on which the Thunder Click board is plugged on MikroBus.Net board</param>
        public ThunderClick(Hardware.Socket socket)
        {
            _socket = socket;

            // Initialize SPI
#if (NANOFRAMEWORK_1_0)
            _thunder = SpiDevice.Create(new SpiConnectionSettings(socket.SpiBus, socket.Cs)
            {
                Mode = SpiMode.Mode0,
                ClockFrequency = 2000000
            });

            IRQ = new GpioController().OpenPin(socket.Int);
            IRQ.SetPinMode(PinMode.Input);
#else
            _thunder = SpiController.FromName(socket.SpiBus).GetDevice(new SpiConnectionSettings()
            {
                ChipSelectType = SpiChipSelectType.Gpio,
                ChipSelectLine = GpioController.GetDefault().OpenPin(socket.Cs),
                Mode = SpiMode.Mode0,
                ClockFrequency = 2000000
            });

            IRQ = GpioController.GetDefault().OpenPin(socket.Int);
            IRQ.SetDriveMode(GpioPinDriveMode.Input);
#endif

            IRQ.ValueChanged += IRQ_ValueChanged;

            // Direct commands
            lock (_socket.LockSpi)
            {
                _thunder.Write(new Byte[] { PRESET_DEFAULT, 0x96 });                 // Set all registers in default mode
                _thunder.Write(new Byte[] { CALIB_RCO, 0x96 });                      // Calibrate internal oscillators
            }
            _nfl = 2;                                                                   // Noise floor level
            _mode = AFE.Indoor;                                                         // Default mode is Indoor
            _spikeRejection = 2;                                                        // Default value for spike rejection
            _minNumberLightning = 0;                                                    // Minimum number of detected lightnings
            INL_Indoor = new[] { 28, 45, 62, 78, 95, 112, 130, 146 };             // Indoor continuous input noise level values (µV rms)
            INL_Outdoor = new[] { 390, 630, 860, 1100, 1140, 1570, 1800, 2000 };  // Outdoor continuous input noise level values (µV rms)
            _powerMode = PowerModes.On;
        }

#if (NANOFRAMEWORK_1_0)
        private void IRQ_ValueChanged(object sender, PinValueChangedEventArgs e) => Check();
#else
        private void IRQ_ValueChanged(GpioPin sender, GpioPinValueChangedEventArgs e) => Check();
#endif

        /// <summary>
        /// Starts the interrupt pin scanning
        /// </summary>
        /// <example> This sample shows how to call the StartIRQ() method.
        /// <code language="C#">
        ///             _thunder = new ThunderClick(Hardware.SocketOne);
        ///
        ///             // Subscribe to events
        ///             _thunder.LightningDetected += TH_LightningDetected;
        ///             _thunder.DisturbanceDetected += TH_DisturbanceDetected;
        ///             _thunder.NoiseDetected += TH_NoiseDetected;
        ///
        ///             // Start scanning IRQ pin for an event
        ///             _thunder.StartIRQ();
        /// </code>
        /// </example>
        public void StartIRQ() => IRQ.ValueChanged += IRQ_ValueChanged;

        /// <summary>
        /// Stops the interrupt pin scanning
        /// </summary>
        /// <example> This sample shows how to call the StopIRQ() method.
        /// <code language="C#">
        ///             // ThunderClick board is inserted in Socket 1
        ///             _thunder = new ThunderClick(Hardware.SocketOne);
        ///
        ///             // Start scanning IRQ
        ///             _thunder.StartIRQ();
        /// 
        ///             Thread.Sleep(30000);    // Wait 30 sec
        ///             _thunder.StopIRQ();     // Stop scanning IRQ. No more events will be generated
        /// </code>
        /// </example>
        public void StopIRQ() => IRQ.ValueChanged -= IRQ_ValueChanged;

        /// <summary>
        /// Gets or sets the minimum number of lightning events detected in the last 15 minutes before considering the event as a real lightning event.
        /// <para>Value 0 : 1 event</para>
        /// <para>Value 1 : 5 events</para>
        /// <para>Value 2 : 9 events</para>
        /// <para>Value 3 : 16 events</para>
        /// </summary>
        /// <example> This sample shows how to set the MinNumberOfLightning property.
        /// <code language="C#">
        ///             _thunder.MinNumberOfLightning = 1;       // Wait for 5 lightning events before sending an interrupt
        ///
        ///             // Start scanning IRQ
        ///             _thunder.StartIRQ();
        /// </code>
        /// </example>
        public Byte MinNumberOfLightning
        {
            get { return _minNumberLightning; }
            set
            {
                if (value > 3) { value = 3; }
                SetRegister(0x02, "00110000", value);
                _minNumberLightning = value;
            }
        }

        /// <summary>
        /// Gets or sets the spike rejection threshold.  Larger values correspond to more robust disturber rejection, with a decrease of the detection efficiency in the distance.
        /// </summary>
        /// <example> This sample shows how to set the SpikeRejection property.
        /// <code language="C#">
        ///             _thunder.SpikeRejection = 5;
        /// </code>
        /// </example>
        public Byte SpikeRejection
        {
            get { return _spikeRejection; }
            set
            {
                if (value > 11) { value = 11; }
                SetRegister(0x02, "00001111", value);
                _spikeRejection = value;
            }
        }

        /// <summary>
        /// Gets or sets the noise floor level.
        /// </summary>
        /// <value>
        /// The noise floor level from 0 to 7.
        /// </value>
        /// <remarks> 
        /// Please see the datasheet as the value has different meanings if <seealso cref="AnalogFrontEnd"/> is Indoor or Outdoor.
        /// </remarks>
        /// <example> This sample shows how to set the NoiseFloorLevel property.
        /// <code language="C#">
        ///             Debug.Print("Noise floor level " + _thunder.NoiseFloorLevel);
        ///             _thunder.NoiseFloorLevel = 4;
        ///             Debug.Print("Noise floor level " + _thunder.NoiseFloorLevel);
        /// </code>
        /// </example>
        public Byte NoiseFloorLevel
        {
            get { return _nfl; }
            set
            {
                if (value > 7) { value = 7; }
                SetRegister(0x01, "01110000", value);
                _nfl = value;
            }
        }

        /// <summary>
        /// Gets the continuous input noise level, depending on the Noise Floor Level
        /// </summary>
        /// <value>
        /// The continuous input noise level in µV rms.
        /// </value>
        /// <remarks>Please see ther datasheet as the value returns has a different meaning if <seealso cref="AnalogFrontEnd"/> is set to Indoor or OutDoor</remarks>
        /// <example> This sample shows how to get the ContinuousInputNoiseLevel property.
        /// <code language="C#">
        ///             Debug.Print("Continuous input noise level " + _thunder.ContinuousInputNoiseLevel + " µV rms");
        /// </code>
        /// </example>
        public Int32 ContinuousInputNoiseLevel
        {
            get { return AnalogFrontEnd == AFE.Indoor ? INL_Indoor[_nfl] : INL_Outdoor[_nfl]; }
        }

        /// <summary>
        /// Clears the statistics built up by the lightning distance estimation algorithm
        /// </summary>
        /// <example> This sample shows how to clear the internal statistics.
        /// <code language="C#">
        ///             _thunder.ClearStatistics();
        /// </code>
        /// </example>
        public void ClearStatistics()
        {
            SetRegister(0x02, "x1xxxxxx");
            SetRegister(0x02, "x0xxxxxx");
            SetRegister(0x02, "x1xxxxxx");
        }

        /// <summary>
        /// Gets or sets the analog front end.
        /// </summary>
        /// <value>
        /// The analog front end is either Indoor or Outdoor. Defaults to Indoor.
        /// </value>
        /// <example> This sample shows how to use the AnalogFrontEnd property (AFE).
        /// <remarks>Please see datasheet as this setting will cause different readings whether it is set to Indoor or Outdoor. Default value is ThunderClick.AFE.Indoor.</remarks>
        /// <code language="C#">
        ///             _thunder.AnalogFrontEnd = ThunderClick.AFE.Outdoor;
        /// </code>
        /// </example>
        public AFE AnalogFrontEnd
        {
            get { return _mode; }
            set
            {
                SetRegister(0x00, _mode == AFE.Indoor ? "xx10010x" : "xx01110x");
                _mode = value;
            }
        }

        private Int32 Energy()
        {
            var thunderEnergy = GetRegister(0x06) & 0x1F;
            var mid = GetRegister(0x05);
            var low = GetRegister(0x04);

            thunderEnergy <<= 8;
            thunderEnergy |= mid;
            thunderEnergy <<= 8;
            thunderEnergy |= low;

            return thunderEnergy;
        }

        private Int32 Distance()
        {
            var dist = GetRegister(0x07) & 63;
            return dist == 63 ? Int32.MaxValue : dist;       // Returns Int32.MaxValue for "Out of range" distance reading (see datasheet)
        }

        /// <summary>
        /// Checks if lightning, disturbance or noise has been detected. This method can be used when polling the sensor, that is : not in IRQ mode.
        /// <remarks>A separate event is thrown for each case.</remarks>
        /// </summary>
        /// <example> This sample shows how to use the Check() method
        /// <code language="C#">
        ///             // ThunderClick board is inserted in Socket 1
        ///             _thunder = new ThunderClick(Hardware.SocketOne);
        ///
        ///             // Subscribe to events
        ///             _thunder.LightningDetected += TH_LightningDetected;
        ///             _thunder.DisturbanceDetected += TH_DisturbanceDetected;
        ///             _thunder.NoiseDetected += TH_NoiseDetected;
        /// 
        ///             // Poll the board for events every 5 sec
        ///             while (true)
        ///             {
        ///                 _thunder.Check();
        ///                 Thread.Sleep(5000);
        ///             }
        /// 
        ///             Thread.Sleep(Timeout.Infinite);
        ///         }
        /// 
        ///         static void TH_NoiseDetected(object sender, EventArgs e)
        ///         {
        ///             Debug.Print("Noise detected");
        ///         }
        ///
        ///         static void TH_DisturbanceDetected(object sender, EventArgs e)
        ///         {
        ///             Debug.Print("Disturbance detected");
        ///         }
        ///
        ///         static void TH_LightningDetected(object sender, MikroBusNet.Events.LightningEventArgs e)
        ///         {
        ///             Debug.Print(DateTime.Now+" : lightning detected at " + e.Distance + " km, energy : " + e.Energy);
        ///         }
        ///     }
        /// }
        /// </code>
        /// </example>
        public void Check()
        {
            var source = GetRegister(0x03) & 0x0F;
            switch (source)
            {
                case 8 :    // Lightning
                    var energyTmp = Energy();
                    if (energyTmp > 0)
                    {
                        LightningEventHandler lightEvent = LightningDetected;
                        lightEvent(this, new LightningEventArgs(Distance(), energyTmp));
                    }
                    break;
                case 4 :    // Disturbance
                    EventHandler disturbEvent = DisturbanceDetected;
                    disturbEvent(this, null);
                    break;
                case 1 :        // Noise
                    EventHandler noiseEvent = NoiseDetected;
                    noiseEvent(this, null);
                    break;
            }
        }

#region Private methods to get/set registers
        private Byte GetRegister(Byte register)
        {
            lock (_socket.LockSpi)
            {
                _thunder.TransferFullDuplex(new Byte[] { (Byte)(register | 0x40), 0x00 }, _result);
            }

            return _result[0];
        }

/*
        private void SetRegister(Byte register, Byte value)
        {
            Hardware.SPIBus.WriteRead(SpiConfig, new [] { (Byte)(register & 0x3F), value }, Result, 1);
        }
*/

        private void SetRegister(Byte register, String mask)
        {
            lock (_socket.LockSpi)
            {
                _thunder.TransferFullDuplex(new[] { (Byte)(register & 0x3F), Bits.Set(GetRegister(register), mask) }, _result);
            }
        }

        private void SetRegister(Byte register, String mask, Byte value)
        {
            lock (_socket.LockSpi)
            {
                _thunder.TransferFullDuplex(new[] { (Byte)(register & 0x3F), Bits.Set(GetRegister(register), mask, value) }, _result);
            }
        }
#endregion

        /// <summary>
        /// Gets or sets the power mode.
        /// </summary>
        /// <example> This sample shows how to use the PowerMode property.
        /// <code language="C#">
        ///             _thunder.PowerMode = PowerModes.Off;
        /// </code>
        /// </example>
        /// <value>
        /// The current power mode of the module.
        /// </value>
        /// <exception cref="System.NotImplementedException">Thrown if you try to use the PowerModes.Low mode, because this module doesn't have such a mode.</exception>
        public PowerModes PowerMode
        {
            get { return _powerMode; }
            set
            {
                if (value == PowerModes.Low) throw new NotImplementedException("PowerModes.Low");
                _powerMode = value;
                if (value == PowerModes.On)
                {
                    SetRegister(0x00, "xxxxxxx0");
                    lock (_socket.LockSpi)
                    {
                        _thunder.Write(new Byte[] { CALIB_RCO, 0x96 });
                    }
                    SetRegister(0x08, "xx1xxxxx");
                    Thread.Sleep(2);        // A small calibration time is needed for RC oscillators.
                    SetRegister(0x08, "xx0xxxxx");
                }
                else
                {
                    SetRegister(0x00, "xxxxxxx1");
                }
            }
        }
    }
}

