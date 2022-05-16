/*
 * RFID Click MBN Driver for TinyCLR 2.0
 * 
 * Version 1.0 :
 *  - Initial release
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
using System.Diagnostics;
using System.Text;
using System.Threading;

namespace MBN.Modules
{
    /// <summary>
    /// Main class for the RFID Click board driver
    /// <para><b>Pins used :</b> Miso, Mosi, Cs, Sck, An, Rst, Int</para>
    /// </summary>
    /// <example>Example of how to use the RFID Click
    /// <code language = "C#">
    /// using System;
    /// using System.Reflection;
    /// using System.Text;
    /// using System.Threading;
    /// using MBN.Enums;
    /// using MBN.Exceptions;
    /// using Microsoft.SPOT;
    /// using Microsoft.SPOT.Hardware;
    /// using MBN.Extensions;
    ///
    /// namespace RFIDClickExample
    /// {
    /// 	public class Program 
    /// 	{
    /// 		private static RFIDClick _rfid;
    /// 		private static DevantechLcd03 _lcd;
    ///
    /// 		public static void Main()
    ///         {
    ///             try
    ///             {
    ///                 _rfid = new RFIDClick(Hardware.SocketOne);
    ///                 InitLcd();
    /// 
    ///                 Debug.WriteLine("RFID identification : " + _rfid.Identification());
    /// 
    ///                 _lcd.Write(1, 4, "Calibration...");
    ///                 _rfid.Calibration(Hardware.Led2);
    ///                 _lcd.Write(1, 4, "              ");
    /// 
    ///                 _rfid.TagDetected += _rfid_TagDetected;
    ///                 _rfid.TagRemoved += _rfid_TagRemoved;
    /// 
    ///                 _rfid.DetectionEnabled = true;
    ///             }
    ///             catch (Exception ex)
    ///             {
    ///                 Debug.WriteLine(ex.Message);
    ///             }
    /// 
    ///             Thread.Sleep(Timeout.Infinite);
    ///         }
    /// 
    ///         static void _rfid_TagRemoved(object sender, TagRemovedEventArgs e)
    ///         {
    ///             Hardware.Led1.Write(false);
    ///             _lcd.Write(1, 4, "                    ");
    ///         }
    /// 
    ///         static void _rfid_TagDetected(object sender, TagDetectedEventArgs e)
    ///         {
    ///             Hardware.Led1.Write(true);
    ///             _lcd.Write(1, 4, e.TagID.Substring(0, e.TagID.Length - 6));
    ///         }
    /// 
    ///         private static void InitLcd()
    ///         {
    ///             _lcd = new DevantechLcd03(Hardware.SocketTwo, 0xC8 >> 1)
    ///             {
    ///                 BackLight = true,
    ///                 Cursor = DevantechLcd03.Cursors.Hide
    ///             };
    ///             _lcd.ClearScreen();
    ///             _lcd.Write(1, 1, "    MikroBus.Net");
    ///             _lcd.Write(1, 2, "RFID Click demo");
    ///         }
    ///     }
    /// }
    /// </code>
    /// </example>
    public sealed class RFIDClick
    {
        /// <summary>
        /// Occurs when is detected
        /// </summary>
        public event TagDetectedEventHandler TagDetected = delegate { };
        /// <summary>
        /// Occurs when a previously detected tag is removed
        /// </summary>
        public event TagRemovedEventHandler TagRemoved = delegate { };

        /// <summary>
        /// The protocols enumeration
        /// </summary>
        public enum RFIDProtocol
        {
            /// <summary>
            /// No protocol selected yet
            /// </summary>
            NONE, 
            /// <summary>
            /// ISO IEC 14443-A protocol
            /// </summary>
            ISO_IEC_14443_A,
            /// <summary>
            /// ISO IEC 14443-B protocol
            /// </summary>
            ISO_IEC_14443_B,
            /// <summary>
            /// ISO IEC 15693 protocol
            /// </summary>
            ISO_IEC_15693,
            /// <summary>
            /// ISO IEC 18092 protocol
            /// </summary>
            ISO_IEC_18092
        };

        private static class Control
        {
            public const Byte Command = 0x00;
            public const Byte Read = 0x02;
        }

        private static class Commands
        {
            public const Byte Identification = 0x01;
            public const Byte ProtocolSelect = 0x02;
            public const Byte SendReceive = 0x04;
            public const Byte Idle = 0x07;
            public const Byte ReadRegister = 0x08;
            public const Byte WriteRegister = 0x09;
            public const Byte Echo = 0x55;
        }

        /// <summary>
        /// CR95HF identification structure
        /// </summary>
        public class IDN
        {
            /// <summary>
            /// Name of the chip
            /// </summary>
            public String Name;
            /// <summary>
            /// The rom revision
            /// </summary>
            public Byte RomRevision;
            /// <summary>
            /// The CRC
            /// </summary>
            public Int32 CRC;

            /// <summary>
            /// Returns a string that represents the active object
            /// </summary>
            /// <returns>
            /// String that represents the active object in a human readable form.
            /// </returns>
            /// <filterpriority>2</filterpriority>
            public override String ToString() => Name + ", ROM rev " + RomRevision + ", CRC : 0x" + BitConverter.ToString(new[] { (Byte)(CRC >> 8), (Byte)CRC }, ' ');
        }

        private readonly SpiDevice _rfid;
        private readonly Byte[] _rBuffer, _tmpBuffer;
        private readonly GpioPin _irqIn;
        private readonly GpioPin _dataReady;
        private Boolean _canSend;
        private RFIDProtocol _protocol;
        private Boolean _detection, _calibrationDone, _calibrationRunning;
        private String _lastTag;
        private Byte _lastError;
        private Int32 _countRemoved;
        private UInt32 _lastTagID;
        private PowerModes _powerMode;
        private GpioPin _blinkPin;
        private readonly Hardware.Socket _socket;

        /// <summary>
        /// Initializes a new instance of the <see cref="RFIDClick"/> class.
        /// </summary>
        /// <param name="socket">The socket on which the RFID Click board is plugged on MikroBus.Net board</param>
        /// <exception cref="DeviceInitialisationException">RFID module not detected</exception>
        public RFIDClick(Hardware.Socket socket)
        {
            _socket = socket;
            _rBuffer = new Byte[261];
            _tmpBuffer = new byte[260];

#if (NANOFRAMEWORK_1_0)
            GpioPin _rst = new GpioController().OpenPin(socket.Rst);
            _rst.SetPinMode(PinMode.Output);
            GpioPin _an = new GpioController().OpenPin(socket.AnPin);
            _an.SetPinMode(PinMode.Output);
            _irqIn = new GpioController().OpenPin(socket.PwmPin);
            _irqIn.SetPinMode(PinMode.Output);

            // Set SPI mode on the chip

            _rst.Write(PinValue.High);
            _an.Write(PinValue.Low);
            _irqIn.Write(PinValue.High);
#else
            GpioPin _rst = GpioController.GetDefault().OpenPin(socket.Rst);
            _rst.SetDriveMode(GpioPinDriveMode.Output);
            GpioPin _an = GpioController.GetDefault().OpenPin(socket.AnPin);
            _an.SetDriveMode(GpioPinDriveMode.Output);
            _irqIn = GpioController.GetDefault().OpenPin(socket.PwmPin);
            _irqIn.SetDriveMode(GpioPinDriveMode.Output);

            // Set SPI mode on the chip

            _rst.Write(GpioPinValue.High);
            _an.Write(GpioPinValue.Low);
            _irqIn.Write(GpioPinValue.High);
#endif

            // Now that the chip is in SPI mode, we can create the SPI configuration and talk to the module
#if (NANOFRAMEWORK_1_0)
            _rfid = SpiDevice.Create(new SpiConnectionSettings(socket.SpiBus, socket.Cs)
            {
                Mode = SpiMode.Mode0,
                ClockFrequency = 2000000
            });
#else
            _rfid = SpiController.FromName(socket.SpiBus).GetDevice(new SpiConnectionSettings()
            {
                ChipSelectType = SpiChipSelectType.Gpio,
                ChipSelectLine = GpioController.GetDefault().OpenPin(socket.Cs),
                Mode = SpiMode.Mode0,
                ClockFrequency = 2000000
            });
#endif

            _rfid.Write(new byte[] { 0x01, 0x01 });

            Thread.Sleep(100);
            ToggleIRQ_IN();
            Thread.Sleep(100);


#if (NANOFRAMEWORK_1_0)
            _dataReady = new GpioController().OpenPin(socket.Int);
            _dataReady.SetPinMode(PinMode.InputPullUp);
#else
            _dataReady = GpioController.GetDefault().OpenPin(socket.Int);
            _dataReady.SetDriveMode(GpioPinDriveMode.InputPullUp);
#endif
            //_dataReady.ValueChangedEdge = GpioPinEdge.FallingEdge;
            _dataReady.ValueChanged += DataReady_ValueChanged;

            if (Echo() != 0x55)
            {
                //throw new DeviceInitialisationException("RFID device not found");
            }

            IndexMod_Gain();
            AutoFDet();
            _protocol = RFIDProtocol.NONE;  // Protocol can't be set before calibration has been done
            _calibrationDone = false;
            _powerMode = PowerModes.On;
            _lastTag = "";
        }

#if (NANOFRAMEWORK_1_0)
        private void DataReady_ValueChanged(object sender, PinValueChangedEventArgs e)
#else
        private void DataReady_ValueChanged(GpioPin sender, GpioPinValueChangedEventArgs e)
#endif
        {
            lock (_socket.LockSpi)
            {
                //Debug.WriteLine($"Dataready : {e.Timestamp}, {e.Edge}");
                _rfid.TransferFullDuplex(new[] { Control.Read }, _rBuffer);
            }
            _canSend = true;
        }

        /// <summary>
        /// Gets or sets a value indicating whether tag detection is enabled.
        /// </summary>
        /// <value>
        ///   <c>true</c> if [detection enabled]; otherwise, <c>false</c>.
        /// </value>
        /// <example>
        /// <code language = "C#">
        ///                 _rfid = new RFIDClick(Hardware.SocketOne);
        ///                 InitLcd();
        /// 
        ///                 Debug.WriteLine("RFID identification : " + _rfid.Identification());
        /// 
        ///                 _lcd.Write(1, 4, "Calibration...");
        ///                 _rfid.Calibration(Hardware.Led2);
        ///                 _lcd.Write(1, 4, "              ");
        /// 
        ///                 _rfid.TagDetected += _rfid_TagDetected;
        ///                 _rfid.TagRemoved += _rfid_TagRemoved;
        /// 
        ///                 // Starts detection
        ///                 _rfid.DetectionEnabled = true;
        /// </code>
        /// </example>
        public Boolean DetectionEnabled
        {
            get { return _detection; }
            set
            {
                if (!_calibrationDone) return;
                if (value == _detection) return;
                _detection = value;
                if (value)
                {
                    if (_protocol == RFIDProtocol.NONE) { Protocol = RFIDProtocol.ISO_IEC_14443_A; }    // Protocol has to be set after calibration is done, so if it's not set yet, then default to ISO IEC 14443_A
                    PowerMode = PowerModes.On;      // Force PowerMode to ON because we requested tag detection
                    _countRemoved = 0;
                    new Thread(DetectionThread).Start();
                }
                else
                {
                    _countRemoved = 0;
                    Thread.Sleep(200);
                    ToggleIRQ_IN();
                    Thread.Sleep(100);
                }
            }
        }

        /// <summary>
        /// Gets or sets the protocol used for the tag detection
        /// </summary>
        /// <value>
        /// The requested protocol.
        /// </value>
        /// <exception cref="System.NotImplementedException">
        /// Protocol ISO IEC 18092
        /// or
        /// Protocol ISO IEC 14443_B
        /// or
        /// Protocol ISO IEC 15693
        /// </exception>
        /// <example>
        /// <code language = "C#">
        ///                 _rfid = new RFIDClick(Hardware.SocketOne);
        /// 
        ///                 _rfid.Calibration(Hardware.Led2);
        /// 
        ///                 _rfid.Protocol = RFIDClick.RFIDProtocol.ISO_IEC_14443_A;
        /// </code>
        /// </example>
        public RFIDProtocol Protocol
        {
            get { return _protocol; }
            set
            {
                if ((_protocol != RFIDProtocol.NONE) && (_detection)) { return; }
                _protocol = value;
                switch (_protocol)
                {
                    case RFIDProtocol.ISO_IEC_14443_A:
                        SendCommand(Commands.ProtocolSelect, new Byte[] { 0x02, 0x00 });
                        break;
                    case RFIDProtocol.ISO_IEC_18092:
                        throw new NotImplementedException("Protocol ISO IEC 18092");
                    case RFIDProtocol.ISO_IEC_14443_B:
                        throw new NotImplementedException("Protocol ISO IEC 14443_B");
                    case RFIDProtocol.ISO_IEC_15693:
                        throw new NotImplementedException("Protocol ISO IEC 15693");
                }
                if (_rBuffer[0] != 0)
                {
                    Debug.WriteLine("Error setting protocol, error code : 0x"+BitConverter.ToString(new [] { _rBuffer[0] }));
                }
            }
        }

        /// <summary>
        /// Calibrations the specified indicator.
        /// </summary>
        /// <param name="indicator">an optionnal indicator. Since the calibration is taking a few seconds to complete, it may be useful to have an indicator of activity.
        /// This can be an onboard led, for example.</param>
        /// <example>
        /// <code language = "C#">
        ///                 _rfid = new RFIDClick(Hardware.SocketOne);
        /// 
        ///                 _rfid.Calibration(Hardware.Led2);
        /// 
        /// </code>
        /// </example>
        public void Calibration(GpioPin indicator=null)
        {
            if (_detection) { return; }
            var blink = new Thread(Blinky);

            if (indicator != null)
            {
                _blinkPin = indicator;
                _calibrationRunning = true;
                blink.Start();
            }

            Debug.WriteLine("Calibration start");

            SendCommand(Commands.Idle, new Byte[] { 0x03, 0xA1, 0x00, 0xF8, 0x01, 0x18, 0x00, 0x20, 0x60, 0x60, 0x00, 0x00, 0x3F, 0x01 });
            SendCommand(Commands.Idle, new Byte[] { 0x03, 0xA1, 0x00, 0xF8, 0x01, 0x18, 0x00, 0x20, 0x60, 0x60, 0x00, 0xFC, 0x3F, 0x01 });
            SendCommand(Commands.Idle, new Byte[] { 0x03, 0xA1, 0x00, 0xF8, 0x01, 0x18, 0x00, 0x20, 0x60, 0x60, 0x00, 0x7C, 0x3F, 0x01 });
            SendCommand(Commands.Idle, new Byte[] { 0x03, 0xA1, 0x00, 0xF8, 0x01, 0x18, 0x00, 0x20, 0x60, 0x60, 0x00, 0x3C, 0x3F, 0x01 });
            SendCommand(Commands.Idle, new Byte[] { 0x03, 0xA1, 0x00, 0xF8, 0x01, 0x18, 0x00, 0x20, 0x60, 0x60, 0x00, 0x5C, 0x3F, 0x01 });
            SendCommand(Commands.Idle, new Byte[] { 0x03, 0xA1, 0x00, 0xF8, 0x01, 0x18, 0x00, 0x20, 0x60, 0x60, 0x00, 0x6C, 0x3F, 0x01 });
            SendCommand(Commands.Idle, new Byte[] { 0x03, 0xA1, 0x00, 0xF8, 0x01, 0x18, 0x00, 0x20, 0x60, 0x60, 0x00, 0x74, 0x3F, 0x01 });
            SendCommand(Commands.Idle, new Byte[] { 0x03, 0xA1, 0x00, 0xF8, 0x01, 0x18, 0x00, 0x20, 0x60, 0x60, 0x00, 0x70, 0x3F, 0x01 });
            _calibrationDone = true;

            if (indicator != null)
            {
                _calibrationRunning = false;
            }

            Debug.WriteLine("Calibration done");
        }

        /// <summary>
        /// Gets the CR95HF identifications data.
        /// </summary>
        /// <returns>A <seealso cref="IDN"/> structure containing identification information.</returns>
        /// <example>
        /// <code language = "C#">
        ///                 _rfid = new RFIDClick(Hardware.SocketOne);
        /// 
        ///                 Debug.WriteLine("RFID identification : " + _rfid.Identification());
        /// </code>
        /// </example>
        public IDN Identification()
        {
            if (_detection) { return null; }
            SendCommand(Commands.Identification);
            if (_rBuffer[0] != 0x00) { Debug.WriteLine("Error"); return null; }

            // 0 terminated string ID ! So, don't take the last 0 value otherwise we have an exception in the String encoding...
            var datalength = _rBuffer[1] - 2;

            return new IDN
            {
                Name = new String(Encoding.UTF8.GetChars(_rBuffer, 2, datalength - 2)),    // Don't take the last char as it's ROM revision
                RomRevision = (Byte)(_rBuffer[datalength] - 48),
                CRC = (_rBuffer[datalength + 2] << 8) + _rBuffer[datalength + 3]
            };
        }

        /// <summary>
        /// Gets or sets the power mode.
        /// </summary>
        /// <value>
        /// The current power mode of the module.
        /// </value>
        /// <exception cref="System.NotImplementedException">This module does not have PowerModes.Off feature.</exception>
        /// <example>
        /// <code language = "C#">
        ///                 _rfid = new RFIDClick(Hardware.SocketOne);
        /// 
        ///                 _rfid.PowerMode = PowerModes.Low;
        /// </code>
        /// </example>
        public PowerModes PowerMode
        {
            get { return _powerMode; }
            set
            {
                if (value == PowerModes.Off) throw new NotImplementedException("PowerMode.Off");
                if (_detection) return;
                SendCommand(Commands.WriteRegister, new Byte[] { 0x09, 0x04, 0x68, 0x01, 0x07, value == PowerModes.On ? (Byte)0x00 : (Byte)0x10 });
                _powerMode = value;
            }
        }

#region Private methods
        private void Blinky()
        {
            while (_calibrationRunning)
            {
#if (NANOFRAMEWORK_1_0)
                _blinkPin.Write(PinValue.High);
                Thread.Sleep(100);
                _blinkPin.Write(PinValue.Low);
#else
                _blinkPin.Write(GpioPinValue.High);
                Thread.Sleep(100);
                _blinkPin.Write(GpioPinValue.Low);
#endif
                Thread.Sleep(100);
            }
        }

        private Byte Echo()
        {
            if (_detection) { return 0; }
            SendCommand(Commands.Echo);
            return _rBuffer[1];
        }

        private void SendCommand(Byte command)
        {
            lock (_socket.LockSpi)
            {
                _rfid.Write(command == Commands.Echo ? new[] { Control.Command, command } : new[] { Control.Command, command, (Byte)0x00 });
            }
            _canSend = false;
            WaitForData();
        }

        private void SendCommand(Byte command, Byte[] parameters)
        {
            var tabTmp = new Byte[parameters.Length + 3];
            tabTmp[0] = Control.Command;
            tabTmp[1] = command;
            tabTmp[2] = (Byte) parameters.Length;
            for (var i = 0; i < parameters.Length; i++) { tabTmp[i + 3] = parameters[i]; }
            lock (_socket.LockSpi)
            {
                _rfid.Write(tabTmp);
            }
            _canSend = false;
            WaitForData();
        }

        private void ToggleIRQ_IN()
        {
#if (NANOFRAMEWORK_1_0)
            _irqIn.Write(PinValue.Low);
            Thread.Sleep(1);
            _irqIn.Write(PinValue.High);
#else
            _irqIn.Write(GpioPinValue.Low);
            Thread.Sleep(1);
            _irqIn.Write(GpioPinValue.High);
#endif
            Thread.Sleep(100);
        }

        private void GetTagID()
        {
            if (!_calibrationDone) return;
            _lastError = 0;
            switch (Protocol)
            {
                case RFIDProtocol.ISO_IEC_14443_A:
                    SendCommand(Commands.SendReceive, new Byte[] { 0x26, 0x07 });
                    String tagTmp;
                    switch (_rBuffer[0])
                    {
                        case 0x80:
                            SendCommand(Commands.SendReceive, new Byte[] { 0x93, 0x20, 0x08 });
                            if (_rBuffer[0] == 0x80)
                            {
                                var tabTmp = new Byte[4];
                                for (var i = 5; i >= 2; i--) { tabTmp[5 - i] = _rBuffer[i]; }
                                tagTmp = BitConverter.ToString(tabTmp);

                                if (tagTmp != _lastTag)
                                {
                                    _countRemoved = 0;
                                    TagDetectedEventHandler tagDetectedEvent = TagDetected;
                                    _lastTag = tagTmp;
                                    _lastTagID = Tag2UInt32(tabTmp);
                                    tagDetectedEvent(this, new TagDetectedEventArgs(tagTmp, _lastTagID, _rBuffer[6]));
                                }
                                else { _countRemoved--; }
                            }
                            break;
                        case 0x87:
                            _countRemoved++;
                            if ((_countRemoved > 2) && (_lastTag != ""))
                            {
                                tagTmp = _lastTag;
                                _lastTag = "";
                                TagRemovedEventHandler tagRemovedEvent = TagRemoved;
                                tagRemovedEvent(this, new TagRemovedEventArgs(tagTmp, _lastTagID, _rBuffer[6]));
                                _countRemoved = 0;
                            }
                            break;
                        case 0x90: // Ignore this one
                            break;
                        default:
                            _lastError = _rBuffer[0];
                            Debug.WriteLine("Last error : " + _lastError);
                            break;
                    }
                    break;
                case RFIDProtocol.ISO_IEC_18092:
                case RFIDProtocol.ISO_IEC_14443_B:
                case RFIDProtocol.ISO_IEC_15693:
                    break;
            }
        }

        private UInt32 Tag2UInt32(Byte[] values) => (UInt32)((values[0] << 24) + (values[1] << 16) + (values[2] << 8) + values[3]);

        private void DetectionThread()
        {
            while (_detection)
            {
                GetTagID();
                Thread.Sleep(100);
            }
        }

        private void IndexMod_Gain()
        {
            if (_detection) { return; }
            SendCommand(Commands.WriteRegister, new Byte[] { 0x09, 0x04, 0x68, 0x01, 0x01, 0x50 });
        }

        private void AutoFDet()
        {
            if (_detection) { return; }
            SendCommand(Commands.WriteRegister, new Byte[] { 0x09, 0x04, 0x0A, 0x01, 0x02, 0xA1 });
        }

        private void WaitForData()
        {
            while (!_canSend) { Thread.Sleep(5); }
        }
#endregion
    }

#region BitConverter
//TODO: this overrides the System.BitConverter class for other drivers!
    internal static class BitConverter  // Borrowed from "jimox"
    {
        public static String ToString(Byte[] value, Char sep = ':', Int32 index = 0) => ToString(value, sep, index, value.Length - index);

        public static String ToString(Byte[] value, Char sep, Int32 index, Int32 length)
        {
            var c = new Char[length * 3];
            Byte b;

            for (Int32 y = 0, x = 0; y < length; ++y, ++x)
            {
                b = (Byte)(value[index + y] >> 4);
                c[x] = (Char)(b > 9 ? b + 0x37 : b + 0x30);
                b = (Byte)(value[index + y] & 0xF);
                c[++x] = (Char)(b > 9 ? b + 0x37 : b + 0x30);
                if (sep != ' ') { c[++x] = sep; }
            }
            return new String(c, 0, c.Length - 1);
        }
    }
#endregion
}

