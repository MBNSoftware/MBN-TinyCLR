#if (NANOFRAMEWORK_1_0)
using System.Device.Gpio;
using Windows.Devices.SerialCommunication;
using Windows.Storage.Streams;
#else
using GHIElectronics.TinyCLR.Devices.Gpio;
using GHIElectronics.TinyCLR.Devices.Uart;
#endif

using System;
using System.Text;
using System.Threading;

namespace MBN.Modules
{
    /// <summary>Main GPS2 Click class</summary>
    public sealed partial class GPS2Click
    {
#if (NANOFRAMEWORK_1_0)
        private readonly SerialDevice _gps2;
#else
        private readonly UartController _gps2;
#endif

        private readonly SerialListener _sl;
        private readonly GpioPin _wakeUp, _onOff;

        /// <summary>Initializes a new instance of the <see cref="GPS2Click" /> class.</summary>
        /// <param name="socket">The socket on which the module is plugged</param>
        public GPS2Click(Hardware.Socket socket)
        {
            _sl = new SerialListener('$', '\n');
            _sl.MessageAvailable += Sl_MessageAvailable;

#if (NANOFRAMEWORK_1_0)
            _wakeUp = new GpioController().OpenPin(socket.AnPin);
            _wakeUp.SetPinMode(PinMode.Input);

            _onOff = new GpioController().OpenPin(socket.PwmPin);
            _onOff.SetPinMode(PinMode.Output);
            // Force full mode
            _onOff.Write(PinValue.Low);
            Thread.Sleep(100);
            _onOff.Write(PinValue.High);
            Thread.Sleep(100);
            _onOff.Write(PinValue.Low);
            Thread.Sleep(1);
            _onOff.Write(PinValue.High);
            Thread.Sleep(100);
#else
            _wakeUp = GpioController.GetDefault().OpenPin(socket.AnPin);
            _wakeUp.SetDriveMode(GpioPinDriveMode.Input);

            _onOff = GpioController.GetDefault().OpenPin(socket.PwmPin);
            _onOff.SetDriveMode(GpioPinDriveMode.Output);
            // Force full mode
            _onOff.Write(GpioPinValue.Low);
            Thread.Sleep(100);
            _onOff.Write(GpioPinValue.High);
            Thread.Sleep(100);
            _onOff.Write(GpioPinValue.Low);
            Thread.Sleep(1);
            _onOff.Write(GpioPinValue.High);
            Thread.Sleep(100);
#endif

#if (NANOFRAMEWORK_1_0)
            _gps2 = SerialDevice.FromId(socket.ComPort);

            // set parameters
            _gps2.BaudRate = 4800;
            _gps2.Parity = SerialParity.None;
            _gps2.StopBits = SerialStopBitCount.One;
            _gps2.Handshake = SerialHandshake.None;
            _gps2.DataBits = 8;
#else
            _gps2 = UartController.FromName(socket.ComPort);
            _gps2.SetActiveSettings(new UartSetting() { BaudRate = 4800, DataBits = 8, Parity = UartParity.None, StopBits = UartStopBitCount.One, Handshaking = UartHandshake.None });
#endif
            _gps2.DataReceived += Gps2_DataReceived;

#if (!NANOFRAMEWORK_1_0)
            _gps2.Enable();
#endif
        }

        private void Sl_MessageAvailable(Object sender, EventArgs e) => NMEAParser.Parse((Byte[])_sl.MessagesQueue.Dequeue());

        /// <summary>Sends a command to the GPS2 module.</summary>
        /// <param name="cmd">The command, with both the starting '$' and the ending '*'.</param>
        public void SendCommand(string cmd)
        {
#if (NANOFRAMEWORK_1_0)
            DataWriter outputDataWriter = new DataWriter(_gps2.OutputStream);
            var bytesToWrite = Encoding.UTF8.GetBytes($"{cmd}{NMEAParser.CalculateChecksum(Encoding.UTF8.GetBytes(cmd)):X2}\r\n");
            outputDataWriter.WriteBytes(bytesToWrite);
            outputDataWriter.Store();
#else
            _gps2.Write(Encoding.UTF8.GetBytes($"{cmd}{NMEAParser.CalculateChecksum(Encoding.UTF8.GetBytes(cmd)):X2}\r\n"));
            _gps2.Flush();
#endif
        }

#if (NANOFRAMEWORK_1_0)
        private void Gps2_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            DataReader inputDataReader = new DataReader(_gps2.InputStream)
            {
                InputStreamOptions = InputStreamOptions.Partial
            };
#else
        private void Gps2_DataReceived(UartController sender, DataReceivedEventArgs e)
        {
#endif
            var btr = _gps2.BytesToRead;
            while (btr != 0)
            {
                var _buffer = new Byte[btr];
#if (NANOFRAMEWORK_1_0)
                inputDataReader.Load(btr);
                inputDataReader.ReadBytes(_buffer);
#else
                _gps2.Read(_buffer, 0, btr);
#endif
                _sl.Add(_buffer);
                _buffer = null;
                btr = _gps2.BytesToRead;
            }
        }
    }
}
