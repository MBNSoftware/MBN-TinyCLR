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
    /// <summary>Main GNSS ZOE Click class</summary>
    public sealed class GnssZoeClick
    {
        private readonly SpiDevice _zoe;
        private readonly Byte[] _rBuff;
        private readonly SerialListener _sl;

        /// <summary>Initializes a new instance of the <see cref="GnssZoeClick" /> class.</summary>
        /// <param name="socket">The socket on which the module is plugged</param>
        public GnssZoeClick(Hardware.Socket socket, Boolean startPolling = true, Int32 pollingFrequency = 1000)
        {
            _sl = new SerialListener('$', '\n');
            _sl.MessageAvailable += Sl_MessageAvailable;

            // Initialize SPI
#if (NANOFRAMEWORK_1_0)
            _zoe = SpiDevice.Create(new SpiConnectionSettings(socket.SpiBus, socket.Cs)
            {
                Mode = SpiMode.Mode0,
                ClockFrequency = 4000000
            });
#else
            _zoe = SpiController.FromName(socket.SpiBus).GetDevice(new SpiConnectionSettings()
            {
                ChipSelectType = SpiChipSelectType.Gpio,
                ChipSelectLine = GpioController.GetDefault().OpenPin(socket.Cs),
                Mode = SpiMode.Mode0,
                ClockFrequency = 4000000
            });
#endif

            _rBuff = new Byte[1024];

            if (startPolling)
                StartPolling(pollingFrequency);
            PollingActive = startPolling;
            PollingFrequency = pollingFrequency;
        }

        /// <summary>
        /// Gets or sets a value indicating whether polling is active or not.
        /// </summary>
        /// <value>
        ///   <c>true</c> if polling is active; otherwise, <c>false</c>.
        /// </value>
        public Boolean PollingActive
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the polling frequency, in milliseconds.
        /// </summary>
        /// <value>
        /// The actual polling frequency.
        /// </value>
        public Int32 PollingFrequency
        {
            get;
            set;
        }

        private void PollingThread()
        {
            while (PollingActive)
            {
                lock (Hardware.LockSPI)
                {
                    _zoe.Read(_rBuff);
                }

                // Valid data received
                if (_rBuff[0] != 0xFF)
                    _sl.Add(_rBuff);

                Thread.Sleep(PollingFrequency);
            }
        }

        /// <summary>
        /// Starts polling data from the module.
        /// </summary>
        /// <param name="frequency">The frequency to poll, in milliseconds.</param>
        public void StartPolling(Int32 frequency)
        {
            if (PollingActive)
            {
                StopPolling();
                Thread.Sleep(PollingFrequency);
            }
            PollingActive = true;
            PollingFrequency = frequency;
            new Thread(PollingThread).Start();
        }

        /// <summary>
        /// Stops polling the module.
        /// </summary>
        public void StopPolling() => PollingActive = false;

        private void Sl_MessageAvailable(Object sender, EventArgs e) => NMEAParser.Parse((Byte[])_sl.MessagesQueue.Dequeue());
    }
}
