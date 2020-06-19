using GHIElectronics.TinyCLR.Devices.Uart;
using System;
using System.Text;
using static MBN.Modules.GPSUtilities;

namespace MBN.Modules
{
    /// <summary>Main GNSS 4 Click class</summary>
    public sealed partial class Gnss4Click
    {
        private readonly UartController _gnss;
        private readonly SerialListener _sl;

        /// <summary>Initializes a new instance of the <see cref="Gnss4Click" /> class.</summary>
        /// <param name="socket">The socket on which the module is plugged</param>
        public Gnss4Click(Hardware.Socket socket)
        {
            _sl = new SerialListener('$', '\n');
            _sl.MessageAvailable += Sl_MessageAvailable;

            _gnss = UartController.FromName(socket.ComPort);
            _gnss.SetActiveSettings(9600, 8, UartParity.None, UartStopBitCount.One, UartHandshake.None);
            _gnss.DataReceived += Gnss_DataReceived;
            _gnss.Enable();
        }

        private void Sl_MessageAvailable(Object sender, EventArgs e)
        {
            var param = (Byte[])_sl.MessagesQueue.Dequeue();
            var _chars = new Char[param.Length];

            Encoding.UTF8.GetDecoder().Convert(param, 0, param.Length, _chars, 0, param.Length, false, out _, out var _charsUsed, out _);
            var strtmp = new String(_chars, 0, _charsUsed).Trim('\r', '\n');

            if (strtmp != String.Empty)
                Parse(strtmp);
        }

        /// <summary>Sends a command to the GNSS 4 module.</summary>
        /// <param name="cmd">The command, without both the starting '$' and the ending '*'.</param>
        public void SendCommand(String cmd)
        {
            _gnss.Write(Encoding.UTF8.GetBytes($"${cmd}*{CalculateChecksum(cmd):X2}\r\n"));
            _gnss.Flush();
        }

        private void Gnss_DataReceived(UartController sender, DataReceivedEventArgs e)
        {
            var btr = _gnss.BytesToRead;
            while (btr != 0)
            {
                var _buffer = new Byte[btr];
                _gnss.Read(_buffer, 0, btr);
                _sl.Add(_buffer);
                _buffer = null;
                btr = _gnss.BytesToRead;
            }
        }
    }
}
