#if (NANOFRAMEWORK_1_0)
using Windows.Devices.SerialCommunication;
using Windows.Storage.Streams;
#else
using GHIElectronics.TinyCLR.Devices.Uart;
#endif

using System;
using System.Text;

namespace MBN.Modules
{
    /// <summary>Main GNSS 4 Click class</summary>
    public sealed partial class Gnss4Click
    {
#if (NANOFRAMEWORK_1_0)
        private readonly SerialDevice _gnss;
#else
        private readonly UartController _gnss;
#endif

        private readonly SerialListener _sl;

        /// <summary>Initializes a new instance of the <see cref="Gnss4Click" /> class.</summary>
        /// <param name="socket">The socket on which the module is plugged</param>
        public Gnss4Click(Hardware.Socket socket)
        {
            _sl = new SerialListener('$', '\n');
            _sl.MessageAvailable += Sl_MessageAvailable;

#if (NANOFRAMEWORK_1_0)
            _gnss = SerialDevice.FromId(socket.ComPort);

            // set parameters
            _gnss.BaudRate = 9600;
            _gnss.Parity = SerialParity.None;
            _gnss.StopBits = SerialStopBitCount.One;
            _gnss.Handshake = SerialHandshake.None;
            _gnss.DataBits = 8;
#else
            _gnss = UartController.FromName(socket.ComPort);
            _gnss.SetActiveSettings(new UartSetting() { BaudRate = 9600, DataBits = 8, Parity = UartParity.None, StopBits = UartStopBitCount.One, Handshaking = UartHandshake.None });
#endif
            _gnss.DataReceived += Gnss_DataReceived;

#if (!NANOFRAMEWORK_1_0)
            _gnss.Enable();
#endif
        }

        private void Sl_MessageAvailable(Object sender, EventArgs e) => NMEAParser.Parse((Byte[])_sl.MessagesQueue.Dequeue());

        /// <summary>Sends a command to the GNSS 4 module.</summary>
        /// <param name="cmd">The command, with both the starting '$' and the ending '*'.</param>
        public void SendCommand(string cmd)
        {
#if (NANOFRAMEWORK_1_0)
            DataWriter outputDataWriter = new DataWriter(_gnss.OutputStream);
            var bytesToWrite = Encoding.UTF8.GetBytes($"{cmd}{NMEAParser.CalculateChecksum(Encoding.UTF8.GetBytes(cmd)):X2}\r\n");
            outputDataWriter.WriteBytes(bytesToWrite);
            outputDataWriter.Store();
#else
            _gnss.Write(Encoding.UTF8.GetBytes($"{cmd}{NMEAParser.CalculateChecksum(Encoding.UTF8.GetBytes(cmd)):X2}\r\n"));
            _gnss.Flush();
#endif
        }

#if (NANOFRAMEWORK_1_0)
        private void Gnss_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            DataReader inputDataReader = new DataReader(_gnss.InputStream)
            {
                InputStreamOptions = InputStreamOptions.Partial
            };
#else
        private void Gnss_DataReceived(UartController sender, DataReceivedEventArgs e)
        {
#endif
            var btr = _gnss.BytesToRead;
            while (btr != 0)
            {
                var _buffer = new Byte[btr];
#if (NANOFRAMEWORK_1_0)
                inputDataReader.Load(btr);
                inputDataReader.ReadBytes(_buffer);
#else
                _gnss.Read(_buffer, 0, btr);
#endif
                _sl.Add(_buffer);
                _buffer = null;
                btr = _gnss.BytesToRead;
            }
        }
    }
}
