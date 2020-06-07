/*
 * CanSpiClick driver for TinyCLR 2.0
 * 
 * Initial version coded for NETMF by Klaus Brüggemann
 * Adapted to TinyCLR 2.0 by Mikrobus.Net
 * 
 * Copyright 2020 MikroBus.Net / 2015 Klaus Brüggemann
 * Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License. You may obtain a copy of the License at
 * http://www.apache.org/licenses/LICENSE-2.0
 * Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, 
 * either express or implied. See the License for the specific language governing permissions and limitations under the License.
 * 
 */

using GHIElectronics.TinyCLR.Devices.Gpio;
using GHIElectronics.TinyCLR.Devices.Spi;
using System;
using System.Threading;

namespace MBN.Modules
{
    /// <summary>
    /// Main class for the CanSpiClick driver
    /// <para><b>Pins used :</b> Miso, Mosi, Cs, Sck, Rst, Int</para>
    /// </summary>
    public sealed class CanSpiClick
    {
        const Byte cmdREAD = 0x03;
        const Byte cmdWRITE = 0x02;
        const Byte cmdBitModify = 0x05;
        const Byte cmdReadRXB0 = 0x90;
        const Byte cmdReadRXB1 = 0x94;
        const Byte cmdRESET = 0xC0;
        const Byte cmdRTSTXB0 = 0x81;

        const Byte regCANSTAT = 0x0E;
        const Byte regCANCTRL = 0x0F;
        const Byte regCNF1 = 0x2A;
        const Byte regCNF2 = 0x29;
        const Byte regCNF3 = 0x28;
        const Byte regCANINTE = 0x2B;
        const Byte regCANINTF = 0x2C;
        const Byte regEFLG = 0x2D;

        const Byte regTXB0CTRL = 0x30;
        const Byte regTXB0SIDH = 0x31;
        const Byte regTXB0SIDL = 0x32;
        const Byte regTXB0DLC = 0x35;
        const Byte regTXB0D0 = 0x36;

        const Byte regRXB0CTRL = 0x60;
        const Byte regRXB0SIDH = 0x61;
        const Byte regRXB0SIDL = 0x62;
        const Byte regRXB0DLC = 0x65;
        const Byte regRXB0D0 = 0x66;

        const Byte regRXB1CTRL = 0x70;
        const Byte regRXB1SIDH = 0x71;
        const Byte regRXB1SIDL = 0x72;
        const Byte regRXB1DLC = 0x75;
        const Byte regRXB1D0 = 0x76;

        public const Byte normalMode = 0;
        public const Byte sleepMode = 1;
        public const Byte loopbackMode = 2;
        public const Byte listenonlyMode = 3;
        public const Byte configMode = 4;

        public struct Config
        {
            public Byte CNF1, CNF2, CNF3;
        }

        public static Config Baudrate100k = new Config { CNF1 = 0x04, CNF2 = 0xA0, CNF3 = 0x02 }; // 100000 bps (MCP2515 @ 10 Mhz)
        public static Config Baudrate125k = new Config { CNF1 = 0x03, CNF2 = 0xA0, CNF3 = 0x02 }; // 125000 bps (MCP2515 @ 10 Mhz)
        public static Config Baudrate250k = new Config { CNF1 = 0x01, CNF2 = 0xA0, CNF3 = 0x02 }; // 250000 bps (MCP2515 @ 10 Mhz)
        public static Config Baudrate500k = new Config { CNF1 = 0x00, CNF2 = 0xA0, CNF3 = 0x02 }; // 500000 bps (MCP2515 @ 10 Mhz)   


        public class CanMessage
        {
            public DateTime Timestamp;
            public Int32 ArbitrationId;
            public Boolean IsExtendedId;
            public Boolean IsRemoteTransmissionRequest;
            public Byte Length;
            public Byte[] Data;
        }

        private readonly SpiDevice _canSpi;
        private readonly GpioPin _rst;
        private readonly GpioPin _int;
        private readonly Byte[] valueChangedBuffer = new Byte[30];

        /// <summary>
        /// Initializes a new instance of the <see cref="CanSpiClick"/> class.
        /// </summary>
        /// <param name="socket">The socket on which the CANSPIclick module is plugged</param>
        public CanSpiClick(Hardware.Socket socket)
        {
            // Initialize SPI
            _canSpi = SpiController.FromName(socket.SpiBus).GetDevice(new SpiConnectionSettings()
            {
                ChipSelectType = SpiChipSelectType.Gpio,
                ChipSelectLine = GpioController.GetDefault().OpenPin(socket.Cs),
                Mode = SpiMode.Mode3,
                ClockFrequency = 2000000
            });

            _rst = GpioController.GetDefault().OpenPin(socket.Rst);
            _rst.SetDriveMode(GpioPinDriveMode.Output);
            _rst.Write(GpioPinValue.High);

            _int = GpioController.GetDefault().OpenPin(socket.Int);
            _int.SetDriveMode(GpioPinDriveMode.Input);
            _int.ValueChangedEdge = GpioPinEdge.FallingEdge;
            _int.ValueChanged += INT_ValueChanged;

            if (!Reset(ResetModes.Hard))
                throw new NotImplementedException("MCP2515 initialisation failed!");
        }

        /// <summary>
        /// Interrupt Service Routine when pin INT falls LOW (message received)
        /// uses the rollover feature from MAB to RXB1, when RXB0 is full
        /// Note: Only standard identifier, no RTR frame detection
        /// </summary>
        private void INT_ValueChanged(GpioPin sender, GpioPinValueChangedEventArgs e)
        {
            var readCmd = (ReadReg(regCANINTF) & 0x01) == 0x01 ? cmdReadRXB0 : cmdReadRXB1;

            lock (Hardware.LockSPI)
            {
                _canSpi.TransferFullDuplex(new Byte[] { readCmd, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 }, valueChangedBuffer);
                Array.Copy(valueChangedBuffer, 1, valueChangedBuffer, 0, valueChangedBuffer.Length - 1);
            }

            var Msg = new CanMessage()
            {
                Timestamp = e.Timestamp,
                ArbitrationId = valueChangedBuffer[0] << 3 | valueChangedBuffer[1] >> 5,
                IsExtendedId = false,
                IsRemoteTransmissionRequest = false,
                Length = (Byte)(valueChangedBuffer[4] & 0x0F),
                Data = new Byte[(Byte)(valueChangedBuffer[4] & 0x0F)]
            };

            for (Byte i = 0; i < Msg.Length; i++)
                Msg.Data[i] = valueChangedBuffer[i + 5];

            MessageReceivedEventHandler messageReceived = MessageReceived;
            messageReceived(this, new MessageReceivedEventArgs(Msg));
        }

        /// <summary>
        /// Gets or sets the name of the Can node.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        public String Name
        {
            get; set;
        }

        /// <summary>
        /// Initialise the CAN controller
        /// </summary>
        /// <param name="name">Name of CAN node</param>
        /// <param name="baudrate">Speed of CAN bus (predefined Baudrate100k, Baudrate125k, Baudrate250k, Baudrate500k)</param>
        /// <param name="opmode">Operation mode of MCP2515 (5 different modes, normalMode=0)</param>
        /// <returns>True, when OpMode is correvtly set</returns>
        public Boolean Init(String name, Config baudrate, Byte opmode)
        {
            Name = name;

            // Set Timing for selected baudrate 
            WriteReg(regCNF1, baudrate.CNF1);
            WriteReg(regCNF2, baudrate.CNF2);
            WriteReg(regCNF3, baudrate.CNF3);

            // Set BUKT bit to enable rollover feature, so message will be written to RXB1 when RXB0 is full
            ModifyReg(regRXB0CTRL, 0x04, 0xFF);

            // Enable Interrupts when message received in RXB0 and RXB1
            WriteReg(regCANINTE, 0x03);

            // Setting MCP2515 to selected mode
            return SetOpMode(opmode);
        }

        /// <summary>
        /// Clears all interrupts (sending yet possible, receiving needs polling the status register/s)
        /// </summary>
        public void Stop() => WriteReg(regCANINTE, 0); // No Interrupts from MCP2515 anymore

        /// <summary>
        /// Sending a CAN message via transmit buffer TXB0 (one of three available)
        /// Note: Timeout is fixed to 1 ms, within this time message must be transmitted
        /// </summary>
        /// <param name="Msg">CAN message structure, timestamp is set when sending</param>
        /// <returns>True, when CAN message was sent</returns>
        public Boolean WriteMessage(ref CanMessage Msg)
        {
            WriteReg(regTXB0SIDH, (Byte)(Msg.ArbitrationId >> 3));
            WriteReg(regTXB0SIDL, (Byte)(Msg.ArbitrationId << 5));
            WriteReg(regTXB0DLC, Msg.Length);
            for (Byte i = 0; i < Msg.Length; i++)
                WriteReg((Byte)(regTXB0D0 + i), Msg.Data[i]);

            ModifyReg(regTXB0CTRL, 0x08, 0xFF);
            Msg.Timestamp = DateTime.UtcNow;
            Thread.Sleep(1);
            return (ReadReg(regTXB0CTRL) & 0x08) == 0;
        }

        /// <summary>
        /// Reset the CAN controller
        /// Hard reset = Pull down the RST pin
        /// Soft reset = Send Reset command to MCP2515 controller
        /// </summary>
        /// <param name="resetMode"></param>
        /// <returns>True, if MCP2515 is in configuration mode after reset</returns>
        public Boolean Reset(ResetModes resetMode)
        {
            switch (resetMode)
            {
                case ResetModes.Hard:
                    _rst.Write(GpioPinValue.Low);
                    Thread.Sleep(10);
                    _rst.Write(GpioPinValue.High);
                    break;
                case ResetModes.Soft:
                    lock (Hardware.LockSPI)
                    {
                        _canSpi.Write(new Byte[] { cmdRESET });
                    }
                    Thread.Sleep(10);
                    break;
            }
            return (ReadReg(regCANSTAT) >> 5) == configMode;
        }

        /// <summary>
        /// Read the EFLG register of MCP2515
        /// </summary>
        /// <returns>Content of EFLG</returns>
        public Byte Error() => ReadReg(regEFLG);

        private Byte ReadReg(Byte addr)
        {
            var buf = new Byte[3];
            lock (Hardware.LockSPI)
            {
                _canSpi.TransferFullDuplex(new Byte[] { cmdREAD, addr, 0 }, buf);
            }
            return buf[2];
        }

        private void WriteReg(Byte addr, Byte val)
        {
            lock (Hardware.LockSPI)
            {
                _canSpi.Write(new Byte[] { cmdWRITE, addr, val });
            }
        }

        private void ModifyReg(Byte addr, Byte mask, Byte val)
        {
            lock (Hardware.LockSPI)
            {
                _canSpi.Write(new Byte[] { cmdBitModify, addr, mask, val });
            }
        }

        public Boolean SetOpMode(Byte opmode)
        {
            lock (Hardware.LockSPI)
            {
                _canSpi.Write(new Byte[] { cmdBitModify, regCANCTRL, 0xE0, (Byte)(opmode << 5) });
            }
            return (ReadReg(regCANSTAT) >> 5) == opmode;
        }

        /// <summary>
        /// Occurs when a message is received.
        /// </summary>
        public event MessageReceivedEventHandler MessageReceived = delegate { };

        /// <summary>
        /// Delegate for the MessageReceived event.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="MessageReceivedEventArgs"/> instance containing the event data.</param>
        public delegate void MessageReceivedEventHandler(Object sender, MessageReceivedEventArgs e);

        /// <summary>
        /// Class holding arguments for the MessageReceived event.
        /// </summary>
        public class MessageReceivedEventArgs
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="MessageReceivedEventArgs"/> class.
            /// </summary>
            /// <param name="message">Can message</param>
            public MessageReceivedEventArgs(CanMessage message)
            {
                Message = message;
            }

            /// <summary>
            /// Gets the Can message received
            /// </summary>
            /// <value>
            /// A struct containing Can message informations and data
            /// </value>
            public CanMessage Message
            {
                get; private set;
            }
        }
    }
}

