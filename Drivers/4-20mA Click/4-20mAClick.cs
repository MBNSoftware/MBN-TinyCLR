/*
 * 4-20mA Click board driver for TinyCLR 2.0
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
using System.Device.Spi;
#else
using GHIElectronics.TinyCLR.Devices.Spi;
using GHIElectronics.TinyCLR.Devices.Gpio;
#endif

using System;
using System.Threading;

namespace MBN.Modules
{
    /// <summary>
    /// 4-20 mA Transmitter
    /// </summary>
    public sealed class T4_20mAClick
    {
        private readonly SpiDevice _trs;
        private UInt16 _4mACalibration, _20mACalibration;
        private readonly Byte[] _data = new Byte[2];
        private Boolean _calibrationDone;
        private readonly Hardware.Socket _socket;

        /// <summary>
        /// Initializes a new instance of the <see cref="T4_20mAClick"/> class.
        /// </summary>
        /// <param name="socket">The socket on which the module is plugged.</param>
        public T4_20mAClick(Hardware.Socket socket)
        {
#if (NANOFRAMEWORK_1_0)
            _trs = SpiDevice.Create(new SpiConnectionSettings(socket.SpiBus, socket.Cs)
            {
                Mode = SpiMode.Mode0,
                ClockFrequency = 2000000
            });
#else
            _trs = SpiController.FromName(socket.SpiBus).GetDevice(new SpiConnectionSettings()
            {
                ChipSelectType = SpiChipSelectType.Gpio,
                ChipSelectLine = GpioController.GetDefault().OpenPin(socket.Cs),
                Mode = SpiMode.Mode0,
                ClockFrequency = 2000000
            });
#endif
            _socket = socket;
        }

        /// <summary>
        /// Scales the specified value from the range [0..1023] into the range [_4mACalibration.._20mACalibration].
        /// </summary>
        /// <param name="value">The value in the range [0..1023]</param>
        /// <returns>The value scaled to [_4mACalibration.._20mACalibration]</returns>
        private UInt16 Scale(UInt16 value) => (UInt16)(((_20mACalibration - _4mACalibration) * value / 1023) + _4mACalibration);

        /// <summary>
        /// Sets the calibration data for the 4mA and 20mA values.
        /// </summary>
        /// <param name="low">The value that represents 4mA.</param>
        /// <param name="high">The value that represents 20mA.</param>
        public void SetCalibrationData(UInt16 low, UInt16 high)
        {
            _4mACalibration = low;
            _20mACalibration = high;
            _calibrationDone = true;
        }

        /// <summary>
        /// Writes a value to the transmitter.
        /// </summary>
        /// <param name="value">The value to send.</param>
        /// <param name="scaled">if set to <c>true</c> then the value is scaled using calibration data,
        ///  otherwise it is sent as raw data.</param>
        /// <exception cref="System.Exception">Calibration not done when the calibration data is not set.</exception>
        public void WriteDAC(UInt16 value, Boolean scaled = true)
        {
            if (scaled && !_calibrationDone)
                throw new Exception("Calibration not done.");
            if (scaled)
                value = Scale(value);

            _data[0] = (Byte)((value >> 8) & 0x0F);
            _data[0] |= 0x30;
            _data[1] = (Byte)(value & 0xFF);
            lock (_socket.LockSpi)
            {
                _trs.Write(new Byte[] { _data[0], _data[1] });
            }
        }
    }

    /// <summary>
    /// 4-20 mA Receiver
    /// </summary>
    public sealed class R4_20mAClick
    {
        private readonly SpiDevice _rec;
        private readonly Byte[] _data = new Byte[2];
        private readonly UInt16 _4mACalibration, _20mACalibration;
        private readonly Hardware.Socket _socket;

        /// <summary>
        /// Initializes a new instance of the <see cref="R4_20mAClick"/> class.
        /// </summary>
        /// <param name="socket">The socket on which the module is plugged.</param>
        /// <param name="calibration4mA">The calibration value for 4mA.</param>
        /// <param name="calibration20mA">The calibration value for 20mA.</param>
        public R4_20mAClick(Hardware.Socket socket, UInt16 calibration4mA, UInt16 calibration20mA)
        {
#if (NANOFRAMEWORK_1_0)
            _rec = SpiDevice.Create(new SpiConnectionSettings(socket.SpiBus, socket.Cs)
            {
                Mode = SpiMode.Mode0,
                ClockFrequency = 2000000
            });
#else
            _rec = SpiController.FromName(socket.SpiBus).GetDevice(new SpiConnectionSettings()
            {
                ChipSelectType = SpiChipSelectType.Gpio,
                ChipSelectLine = GpioController.GetDefault().OpenPin(socket.Cs),
                Mode = SpiMode.Mode0,
                ClockFrequency = 2000000
            });
#endif
            _socket = socket;
            _4mACalibration = calibration4mA;
            _20mACalibration = calibration20mA;
        }

        /// <summary>
        /// Reads a value from the Receiver
        /// </summary>
        /// <param name="iterations">The number of read iterations before returning a weighted result.</param>
        /// <param name="iterationsDelay">The delay in ms between each iteration.</param>
        /// <returns>A UInt16 scaled in the [0..1023] range.</returns>
        public UInt16 ReadDAC(Byte iterations = 1, Byte iterationsDelay = 2)
        {
            var value = 0;
            for (var i = 0; i < iterations; i++)
            {
                lock (_socket.LockSpi)
                {
                    _rec.Read(_data);
                }
                _data[0] = (Byte)(_data[0] & 0b00011111);
                value += (UInt16)(((_data[0] << 8) + _data[1]) >> 1);
                Thread.Sleep(iterationsDelay);
            }

            return Scale(value / iterations);
        }

        /// <summary>
        /// Scales the specified value from the range [_4mACalibration.._20mACalibration] into the range [0..1023].
        /// </summary>
        /// <param name="value">The value in the range [_4mACalibration.._20mACalibration]</param>
        /// <returns>The value scaled to [0, 1023]</returns>
        private UInt16 Scale(Int32 value) => (UInt16)(1023 * (value - _4mACalibration) / _20mACalibration);
    }
}