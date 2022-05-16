#region Licence
// Copyright (C) 2011 by Jakub Bartkowiak (Gralin)
// 
// MIT Licence
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

// 2020, April 07 : Modified by MBN to comply with MBN drivers' scheme and use of SPI for TinyCLR 2.0
#endregion

#if (NANOFRAMEWORK_1_0)
using System.Device.Gpio;
using System.Device.Spi;
#else
using GHIElectronics.TinyCLR.Devices.Gpio;
using GHIElectronics.TinyCLR.Devices.Spi;
#endif

using System;
using System.Diagnostics;
using System.Threading;

namespace MBN.Modules
{
    public sealed partial class NRFC
    {
        private readonly SpiDevice _nrf;
        private readonly Hardware.Socket _socket;

        public enum Acknowledge
        {
            Yes, 
            No
        };

        public enum DataRate
        {
            DR1Mbps, 
            DR2Mbps, 
            DR250kbps
        };

        public enum AddressSlot
        {
            Zero = Registers.RX_ADDR_P0,
            One = Registers.RX_ADDR_P1,
            Two = Registers.RX_ADDR_P2,
            Three = Registers.RX_ADDR_P3,
            Four = Registers.RX_ADDR_P4,
            Five = Registers.RX_ADDR_P5,
        }

#region Delegates

        public delegate void EventHandler();
        public delegate void OnDataReceivedHandler(Byte[] data);
        public delegate void OnInterruptHandler(Status status);

#endregion

        private Byte[] _slot0Address;
        private readonly GpioPin _cePin;
        private readonly Boolean _initialized;
        private readonly GpioPin _irqPin;
        private Boolean _enabled;
        private readonly ManualResetEvent _transmitSuccessFlag;
        private readonly ManualResetEvent _transmitFailedFlag;


        public NRFC(Hardware.Socket socket)
        {
            _socket = socket;
            // Initialize SPI
#if (NANOFRAMEWORK_1_0)
            _nrf = SpiDevice.Create(new SpiConnectionSettings(socket.SpiBus, socket.Cs)
            {
                Mode = SpiMode.Mode0,
                ClockFrequency = 2000000
            });

            // Initialize IRQ Port
            _irqPin = new GpioController().OpenPin(socket.Int);
            _irqPin.SetPinMode(PinMode.InputPullUp);
            _irqPin.ValueChanged += IrqPin_ValueChanged;
            //_irqPin = new InterruptPort(socket.Int, false, Port.ResistorMode.PullUp, Port.InterruptMode.InterruptEdgeLow);

            // Initialize Chip Enable Port
            _cePin = new GpioController().OpenPin(socket.Rst);
            _cePin.SetPinMode(PinMode.Output);
            _cePin.Write(PinValue.Low);
#else
            _nrf = SpiController.FromName(socket.SpiBus).GetDevice(new SpiConnectionSettings()
            {
                ChipSelectType = SpiChipSelectType.Gpio,
                ChipSelectLine = GpioController.GetDefault().OpenPin(socket.Cs),
                Mode = SpiMode.Mode0,
                ClockFrequency = 2000000
            });

            // Initialize IRQ Port
            _irqPin = GpioController.GetDefault().OpenPin(socket.Int);
            _irqPin.SetDriveMode(GpioPinDriveMode.InputPullUp);
            _irqPin.ValueChanged += IrqPin_ValueChanged;
            //_irqPin = new InterruptPort(socket.Int, false, Port.ResistorMode.PullUp, Port.InterruptMode.InterruptEdgeLow);

            // Initialize Chip Enable Port
            _cePin = GpioController.GetDefault().OpenPin(socket.Rst);
            _cePin.SetDriveMode(GpioPinDriveMode.Output);
            _cePin.Write(GpioPinValue.Low);
#endif

            // Module reset time
            Thread.Sleep(100);

            _initialized = true;

            _transmitSuccessFlag = new ManualResetEvent(false);
            _transmitFailedFlag = new ManualResetEvent(false);
        }

#if (NANOFRAMEWORK_1_0)
        private void IrqPin_ValueChanged(object sender, PinValueChangedEventArgs e)
#else
        private void IrqPin_ValueChanged(GpioPin sender, GpioPinValueChangedEventArgs e)
#endif
        {
            if (!_initialized)
                return;

            if (!_enabled)
            {
                // Flush RX FIFO
                Execute(Commands.FLUSH_RX, 0x00, new Byte[0]);
                // Flush TX FIFO 
                Execute(Commands.FLUSH_TX, 0x00, new Byte[0]);
                return;
            }

            // Disable RX/TX
            SetDisabled();

            // Set PRX
            SetReceiveMode();

            // there are 3 rx pipes in rf module so 3 arrays should be enough to store incoming data
            // sometimes though more than 3 data packets are received somehow
            var payloads = new Byte[6][];

            Status status = GetStatus();
            Byte payloadCount = 0;
            var payloadCorrupted = false;

            OnInterrupt(status);

            if (status.DataReady)
            {
                while (!status.RxEmpty)
                {
                    // Read payload size
                    var payloadLength = Execute(Commands.R_RX_PL_WID, 0x00, new Byte[1]);

                    // this indicates corrupted data
                    if (payloadLength[1] > 32)
                    {
                        payloadCorrupted = true;

                        // Flush anything that remains in buffer
                        Execute(Commands.FLUSH_RX, 0x00, new Byte[0]);
                    }
                    else
                    {
                        if (payloadCount >= payloads.Length)
                        {
#if DEBUG
                            Debug.WriteLine("Unexpected payloadCount value = " + payloadCount);
#endif
                            Execute(Commands.FLUSH_RX, 0x00, new Byte[0]);
                        }
                        else
                        {
                            // Read payload data
                            payloads[payloadCount] = Execute(Commands.R_RX_PAYLOAD, 0x00, new Byte[payloadLength[1]]);
                            payloadCount++;
                        }
                    }

                    // Clear RX_DR bit 
                    var result = Execute(Commands.W_REGISTER, Registers.STATUS, new[] { (Byte)(1 << Bits.RX_DR) });
                    status.Update(result[0]);
                }
            }

            if (status.ResendLimitReached)
            {
                // Flush TX FIFO 
                Execute(Commands.FLUSH_TX, 0x00, new Byte[0]);

                // Clear MAX_RT bit in status register
                Execute(Commands.W_REGISTER, Registers.STATUS, new[] { (Byte)(1 << Bits.MAX_RT) });
            }

            if (status.TxFull)
            {
                // Flush TX FIFO 
                Execute(Commands.FLUSH_TX, 0x00, new Byte[0]);
            }

            if (status.DataSent)
            {
                // Clear TX_DS bit in status register
                Execute(Commands.W_REGISTER, Registers.STATUS, new[] { (Byte)(1 << Bits.TX_DS) });
            }

            // Enable RX
            SetEnabled();

            if (payloadCorrupted)
            {
#if DEBUG
                Debug.WriteLine("Corrupted data received");
#endif
            }
            else if (payloadCount > 0)
            {
                if (payloadCount > payloads.Length)
#if DEBUG
                    Debug.WriteLine("Unexpected payloadCount value = " + payloadCount);
#endif

                for (var i = 0; i < System.Math.Min(payloadCount, payloads.Length); i++)
                {
                    var payload = payloads[i];
                    var payloadWithoutCommand = new Byte[payload.Length - 1];
                    Array.Copy(payload, 1, payloadWithoutCommand, 0, payload.Length - 1);
                    OnDataReceived(payloadWithoutCommand);
                }
            }
            else if (status.DataSent)
            {
                _transmitSuccessFlag.Set();
                OnTransmitSuccess();
            }
            else
            {
                _transmitFailedFlag.Set();
                OnTransmitFailed();
            }
        }

        /// <summary>
        ///   Gets a value indicating whether module is enabled (RX or TX mode).
        /// </summary>
        public Boolean IsEnabled
        {
#if (NANOFRAMEWORK_1_0)
            get { return _cePin.Read() == PinValue.High; }
#else
            get { return _cePin.Read() == GpioPinValue.High; }
#endif
        }

        /// <summary>
        ///   Enables the module
        /// </summary>
        public void Enable()
        {
            _enabled = true;
            SetEnabled();
        }

        /// <summary>
        ///   Disables the module
        /// </summary>
        public void Disable()
        {
            _enabled = false;
            SetDisabled();
        }

        /// <summary>
        /// Configure the module basic settings. Module needs to be initiaized.
        /// </summary>
        /// <param name="address">RF address (3-5 bytes). The width of this address determins the width of all addresses used for sending/receiving.</param>
        /// <param name="channel">RF channel (0-127)</param>
        public void Configure(Byte[] address, Byte channel) => Configure(address, channel, DataRate.DR2Mbps);

        /// <summary>
        /// Configure the module basic settings. Module needs to be initiaized.
        /// </summary>
        /// <param name="address">RF address (3-5 bytes). The width of this address determins the width of all addresses used for sending/receiving.</param>
        /// <param name="channel">RF channel (0-127)</param>
        /// <param name="dataRate">Data Rate to use</param>
        public void Configure(Byte[] address, Byte channel, DataRate dataRate)
        {
            CheckIsInitialized();
            AddressWidth.Check(address);

            // Set radio channel
            Execute(Commands.W_REGISTER, Registers.RF_CH,
                    new[]
                        {
                            (Byte) (channel & 0x7F) // channel is 7 bits
                        });

			// Set Data rate
			var regValue = Execute(Commands.R_REGISTER, Registers.RF_SETUP, new Byte[1])[1];			

			switch ( dataRate ) 
            {
                case DataRate.DR1Mbps:
			        regValue &=  (Byte)~(1 << Bits.RF_DR_LOW);  // 0
			        regValue &=  (Byte)~(1 << Bits.RF_DR_HIGH); // 0
                    break;

                case DataRate.DR2Mbps:
                    regValue &=  (Byte)~(1 << Bits.RF_DR_LOW);  // 0
			        regValue |=  (Byte)(1 << Bits.RF_DR_HIGH);  // 1
					break;

                case DataRate.DR250kbps:
                    regValue |=  (Byte)(1 << Bits.RF_DR_LOW);   // 1
			        regValue &=  (Byte)~(1 << Bits.RF_DR_HIGH); // 0
					break;

                default:
                    throw new ArgumentOutOfRangeException("dataRate");
			}	  

			Execute(Commands.W_REGISTER, Registers.RF_SETUP, new[]{regValue});			
			
			// Enable dynamic payload length
            Execute(Commands.W_REGISTER, Registers.FEATURE,
                    new[]
                        {
                            (Byte) (1 << Bits.EN_DPL)
                        });

            // Set auto-ack
            Execute(Commands.W_REGISTER, Registers.EN_AA,
                    new[]
                        {
                            (Byte) (1 << Bits.ENAA_P0 |
                                    1 << Bits.ENAA_P1)
                        });

            // Set dynamic payload length for pipes
            Execute(Commands.W_REGISTER, Registers.DYNPD,
                    new[]
                        {
                            (Byte) (1 << Bits.DPL_P0 |
                                    1 << Bits.DPL_P1)
                        });

            // Flush RX FIFO
            Execute(Commands.FLUSH_RX, 0x00, new Byte[0]);

            // Flush TX FIFO
            Execute(Commands.FLUSH_TX, 0x00, new Byte[0]);

            // Clear IRQ Masks
            Execute(Commands.W_REGISTER, Registers.STATUS,
                    new[]
                        {
                            (Byte) (1 << Bits.MASK_RX_DR |
                                    1 << Bits.MASK_TX_DS |
                                    1 << Bits.MAX_RT)
                        });

            // Set default address
            Execute(Commands.W_REGISTER, Registers.SETUP_AW,
                    new[]
                        {
                            AddressWidth.Get(address)
                        });

            // Set module address
            _slot0Address = address;
            Execute(Commands.W_REGISTER, (Byte)AddressSlot.Zero, address);

            // Set retransmission values
            Execute(Commands.W_REGISTER, Registers.SETUP_RETR,
                    new[]
                        {
                            (Byte) (0x0F << Bits.ARD |
                                    0x0F << Bits.ARC)
                        });

            // Setup, CRC enabled, Power Up, PRX
            SetReceiveMode();
        }

        /// <summary>
        /// Set one of 6 available module addresses
        /// </summary>
        public void SetAddress(AddressSlot slot, Byte[] address)
        {
            CheckIsInitialized();
            AddressWidth.Check(address);
            Execute(Commands.W_REGISTER, (Byte)slot, address);

            if (slot == AddressSlot.Zero)
            {
                _slot0Address = address;
            }
        }

        /// <summary>
        /// Read 1 of 6 available module addresses
        /// </summary>
        public Byte[] GetAddress(AddressSlot slot, Int32 width)
        {
            CheckIsInitialized();
            AddressWidth.Check(width);
            var read = Execute(Commands.R_REGISTER, (Byte)slot, new Byte[width]);
            var result = new Byte[read.Length - 1];
            Array.Copy(read, 1, result, 0, result.Length);
            return result;
        }

        /// <summary>
        ///   Executes a command in NRF24L01+ (for details see module datasheet)
        /// </summary>
        /// <param name = "command">Command</param>
        /// <param name = "addres">Register to write to</param>
        /// <param name = "data">Data to write</param>
        /// <returns>Response byte array. First byte is the status register</returns>
        public Byte[] Execute(Byte command, Byte addres, Byte[] data)
        {
            CheckIsInitialized();

            // This command requires module to be in power down or standby mode
            if (command == Commands.W_REGISTER)
                SetDisabled();

            // Create SPI Buffers with Size of Data + 1 (For Command)
            var writeBuffer = new Byte[data.Length + 1];
            var readBuffer = new Byte[data.Length + 1];

            // Add command and adres to SPI buffer
            writeBuffer[0] = (Byte) (command | addres);

            // Add data to SPI buffer
            Array.Copy(data, 0, writeBuffer, 1, data.Length);

            // Do SPI Read/Write
            lock (_socket.LockSpi)
            {
                _nrf.TransferFullDuplex(writeBuffer, readBuffer);
            }

            // Enable module back if it was disabled
            if (command == Commands.W_REGISTER && _enabled)
                SetEnabled();

            // Return ReadBuffer
            return readBuffer;
        }

        /// <summary>
        ///   Gets module basic status information
        /// </summary>
        public Status GetStatus()
        {
            CheckIsInitialized();

            var readBuffer = new Byte[1];
            // Hardware.SPIBus.Config = _spiConfig;
            lock (_socket.LockSpi)
            {
                _nrf.TransferFullDuplex(new[] { Commands.NOP }, readBuffer);
            }

            return new Status(readBuffer[0]);
        }

        /// <summary>
        ///   Reads the current rf channel value set in module
        /// </summary>
        /// <returns></returns>
        public Byte GetChannel()
        {
            CheckIsInitialized();

            var result = Execute(Commands.R_REGISTER, Registers.RF_CH, new Byte[1]);
            return (Byte) (result[1] & 0x7F);
        }

        /// <summary>
        ///   Gets the module radio frequency [MHz]
        /// </summary>
        /// <returns>Frequency in MHz</returns>
        public Int32 GetFrequency() => 2400 + GetChannel();

        /// <summary>
        ///   Sets the rf channel value used by all data pipes
        /// </summary>
        /// <param name="channel">7 bit channel value</param>
        public void SetChannel(Byte channel)
        {
            CheckIsInitialized();

            var writeBuffer = new[] {(Byte) (channel & 0x7F)};
            Execute(Commands.W_REGISTER, Registers.RF_CH, writeBuffer);
        }

        /// <summary>
        ///   Send <param name = "bytes">bytes</param> to given <param name = "address">address</param>
        ///   This is a non blocking method.
        /// </summary>
        public void SendTo(Byte[] address, Byte[] bytes, Acknowledge acknowledge = Acknowledge.Yes)
        {
            // Chip enable low
            SetDisabled();

            // Setup PTX (Primary TX)
            SetTransmitMode();

            // Write transmit adres to TX_ADDR register. 
            Execute(Commands.W_REGISTER, Registers.TX_ADDR, address);

            // Write transmit adres to RX_ADDRESS_P0 (Pipe0) (For Auto ACK)
            Execute(Commands.W_REGISTER, Registers.RX_ADDR_P0, address);

            // Send payload
            Execute(acknowledge == Acknowledge.Yes ? Commands.W_TX_PAYLOAD : Commands.W_TX_PAYLOAD_NO_ACK, 0x00, bytes);

            // Pulse for CE -> starts the transmission.
            SetEnabled();
        }

        /// <summary>
        ///   Sends <param name = "bytes">bytes</param> to given <param name = "address">address</param>
        ///   This is a blocking method that returns true if data was received by the recipient or false if timeout occured.
        /// </summary>
        public Boolean SendTo(Byte[] address, Byte[] bytes, Int32 timeout)
        {
            DateTime startTime = DateTime.UtcNow; //TODO: TinyCLR shoulf support UtcNow, if not, this needs an ifdef

            while (true)
            {
                _transmitSuccessFlag.Reset();
                _transmitFailedFlag.Reset();

                SendTo(address, bytes);

                //if (WaitHandle.WaitAny(new[] { _transmitSuccessFlag, _transmitFailedFlag }, 200, true) == 0)
                if (WaitHandle.WaitAny(new WaitHandle[] { _transmitSuccessFlag, _transmitFailedFlag }, 200, true) == 0)
                    return true;

                if (DateTime.UtcNow.CompareTo(startTime.AddMilliseconds(timeout)) > 0)
                    return false;
#if DEBUG
                Debug.WriteLine("Retransmitting packet...");
#endif
            }
        }

        

        private void SetEnabled()
        {
            _irqPin.ValueChanged += IrqPin_ValueChanged;
#if (NANOFRAMEWORK_1_0)
            _cePin.Write(PinValue.High);
#else
            _cePin.Write(GpioPinValue.High); 
#endif
        }

        private void SetDisabled()
        {
#if (NANOFRAMEWORK_1_0)
            _cePin.Write(PinValue.Low);
#else
            _cePin.Write(GpioPinValue.Low); 
#endif
            _irqPin.ValueChanged -= IrqPin_ValueChanged;
        }

        private void SetTransmitMode() => Execute(Commands.W_REGISTER, Registers.CONFIG,
                    new[]
                        {
                            (Byte) (1 << Bits.PWR_UP |
                                    1 << Bits.CRCO)
                        });

        private void SetReceiveMode()
        {
            Execute(Commands.W_REGISTER, Registers.RX_ADDR_P0, _slot0Address);

            Execute(Commands.W_REGISTER, Registers.CONFIG,
                    new[]
                        {
                            (Byte) (1 << Bits.PWR_UP |
                                    1 << Bits.CRCO |
                                    1 << Bits.PRIM_RX)
                        });
        }

        private void CheckIsInitialized()
        {
            if (!_initialized)
            {
                throw new InvalidOperationException("Initialize method needs to be called before this call");
            }
        }

        /// <summary>
        ///   Called on every IRQ interrupt
        /// </summary>
        public event OnInterruptHandler OnInterrupt = delegate { };

        /// <summary>
        ///   Occurs when data packet has been received
        /// </summary>
        public event OnDataReceivedHandler OnDataReceived = delegate { };

        /// <summary>
        ///   Occurs when ack has been received for send packet
        /// </summary>
        public event EventHandler OnTransmitSuccess = delegate { };

        /// <summary>
        ///   Occurs when no ack has been received for send packet
        /// </summary>
        public event EventHandler OnTransmitFailed = delegate { };
    }


    public static class AddressWidth
    {
        public const Int32 Min = 3;
        public const Int32 Max = 5;


        public static Byte Get(Byte[] address)
        {
            Check(address);
            return (Byte)(address.Length - 2);
        }

        public static void Check(Byte[] address) => Check(address.Length);

        public static void Check(Int32 addressWidth)
        {
            if (addressWidth < Min || addressWidth > Max)
            {
                throw new ArgumentException("Address width needs to be 3-5 bytes");
            }
        }
    }

    public class Status
    {
        private Byte _reg;

        public Boolean DataReady => (_reg & (1 << NRFC.Bits.RX_DR)) > 0;
        public Boolean DataSent => (_reg & (1 << NRFC.Bits.TX_DS)) > 0;
        public Boolean ResendLimitReached => (_reg & (1 << NRFC.Bits.MAX_RT)) > 0;
        public Boolean TxFull => (_reg & (1 << NRFC.Bits.TX_FULL)) > 0;
        public Byte DataPipe => (Byte)((_reg >> 1) & 7);
        public Boolean DataPipeNotUsed => DataPipe == 6;
        public Boolean RxEmpty => DataPipe == 7;

        public Status(Byte reg) => _reg = reg;

        public void Update(Byte reg) => _reg = reg;

        public override String ToString() => "DataReady: " + DataReady +
                   ", DateSent: " + DataSent +
                   ", ResendLimitReached: " + ResendLimitReached +
                   ", TxFull: " + TxFull +
                   ", RxEmpty: " + RxEmpty +
                   ", DataPipe: " + DataPipe +
                   ", DataPipeNotUsed: " + DataPipeNotUsed;
    }
}

