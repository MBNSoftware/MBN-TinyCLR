/*
 * SpeakUpClick driver for TinyCLR 2.0
 * 
 * Initial revision
 *  
 * Copyright 2020 MikroBus.Net
 * Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License. You may obtain a copy of the License at
 * http://www.apache.org/licenses/LICENSE-2.0
 * Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, 
 * either express or implied. See the License for the specific language governing permissions and limitations under the License.
 * 
 */

#if (NANOFRAMEWORK_1_0)
using Windows.Devices.SerialCommunication;
using Windows.Storage.Streams;
#else
using GHIElectronics.TinyCLR.Devices.Uart;
#endif

using System;

namespace MBN.Modules
{
    /// <summary>
    /// Main class for the SpeakUpClick driver
    /// <para><b>Pins used :</b> Tx, Rx</para>
    /// </summary>
    /// <example>
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
    ///         private static SpeakUpClick _speakUp;
    ///
    ///         public static void Main()
    ///         {
    ///             try
    ///             {
    ///                 _speakUp = new SpeakUpClick(Hardware.SocketTwo);
    ///             }
    ///             catch (PinInUseException)
    ///             {
    ///                 Debug.Print("Error initializing SpeakUp Click board.");
    ///             }
    ///
    ///             _speakUp.SpeakDetected += _speakUp_SpeakDetected;
    ///             _speakUp.Listening = true;
    ///
    ///             Thread.Sleep(Timeout.Infinite);
    ///         }
    ///
    ///         private static void _speakUp_SpeakDetected(object sender, SpeakUpClick.SpeakUpEventArgs e)
    ///         {
    ///             Debug.Print("SpeakUp has received an order, index : " + e.Command);
    ///         }
    ///     }
    /// }
    /// </code>
    /// </example>
    public sealed partial class SpeakUpClick
    {
#if (NANOFRAMEWORK_1_0)
        private static SerialDevice _sp;
#else
        private static UartController _sp;
#endif
        private static Boolean _listening;

        /// <summary>
        /// Occurs when a pre-recorded order has been recognized.
        /// </summary>
        /// <example>
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
        ///         private static SpeakUpClick _speakUp;
        ///
        ///         public static void Main()
        ///         {
        ///             try
        ///             {
        ///                 _speakUp = new SpeakUpClick(Hardware.SocketTwo);
        ///             }
        ///             catch (PinInUseException)
        ///             {
        ///                 Debug.Print("Error initializing SpeakUp Click board.");
        ///             }
        ///
        ///             _speakUp.SpeakDetected += _speakUp_SpeakDetected;
        ///             _speakUp.Listening = true;
        ///
        ///             Thread.Sleep(Timeout.Infinite);
        ///         }
        ///
        ///         private static void _speakUp_SpeakDetected(object sender, SpeakUpClick.SpeakUpEventArgs e)
        ///         {
        ///             Debug.Print("SpeakUp has received an order, index : " + e.Command);
        ///         }
        ///     }
        /// }
        /// </code>
        /// </example>
        public event SpeakUpEventHandler SpeakDetected = delegate { };

        /// <summary>
        /// Initializes a new instance of the <see cref="SpeakUpClick"/> class.
        /// </summary>
        /// <param name="socket">The socket on which the SpeakUpClick module is plugged on MikroBus.Net board</param>
        public SpeakUpClick(Hardware.Socket socket)
        {
#if (NANOFRAMEWORK_1_0)
            _sp = SerialDevice.FromId(socket.ComPort);
            _sp.BaudRate = 115200;
            _sp.DataBits = 8;
            _sp.Parity = SerialParity.None;
            _sp.StopBits = SerialStopBitCount.One;
            _sp.Handshake = SerialHandshake.None;
            //_sp.Enable();
#else
            _sp = UartController.FromName(socket.ComPort);
            _sp.SetActiveSettings(new UartSetting() { BaudRate = 115200, DataBits = 8, Parity = UartParity.None, StopBits = UartStopBitCount.One, Handshaking = UartHandshake.None });
            _sp.Enable();
#endif
        }

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="SpeakUpClick"/> is listening to voice commands.
        /// </summary>
        /// <value>
        ///   <c>true</c> if listening; otherwise, <c>false</c>.
        /// </value>
        /// <example>
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
        ///         private static SpeakUpClick _speakUp;
        ///
        ///         public static void Main()
        ///         {
        ///             try
        ///             {
        ///                 _speakUp = new SpeakUpClick(Hardware.SocketTwo);
        ///             }
        ///             catch (PinInUseException)
        ///             {
        ///                 Debug.Print("Error initializing SpeakUp Click board.");
        ///             }
        ///
        ///             _speakUp.SpeakDetected += _speakUp_SpeakDetected;
        ///             _speakUp.Listening = true;
        ///
        ///             Thread.Sleep(Timeout.Infinite);
        ///         }
        ///
        ///         private static void _speakUp_SpeakDetected(object sender, SpeakUpClick.SpeakUpEventArgs e)
        ///         {
        ///             Debug.Print("SpeakUp has received an order, index : " + e.Command);
        ///         }
        ///     }
        /// }
        /// </code>
        /// </example>
        public Boolean Listening
        {
            get { return _listening; }
            set
            {
                if (value == _listening) { return; }
                if (value)
                {
                    _sp.DataReceived += Sp_DataReceived;
#if (!NANOFRAMEWORK_1_0)
                    _sp.Enable();
#endif
                }
                else
                {
                    _sp.DataReceived -= Sp_DataReceived;
#if (!NANOFRAMEWORK_1_0)
                    _sp.Disable();
#endif
                }
                _listening = value;
            }
        }

#if (NANOFRAMEWORK_1_0)
        private void Sp_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            using (DataReader dataReader = new DataReader(_sp.InputStream))
            {
                dataReader.InputStreamOptions = InputStreamOptions.Partial;
                var nb = dataReader.Load(_sp.BytesToRead);
                var buf = new byte[nb];
                dataReader.ReadBytes(buf);

                SpeakUpEventHandler speakEvent = SpeakDetected;
                speakEvent(this, new SpeakUpEventArgs(buf[0]));
            }
#else
        private void Sp_DataReceived(UartController sender, DataReceivedEventArgs e)
        {
            var nb = _sp.BytesToRead;
            var buf = new Byte[nb];

            _sp.Read(buf, 0, nb);

            SpeakUpEventHandler speakEvent = SpeakDetected;
            speakEvent(this, new SpeakUpEventArgs(buf[0]));
#endif

        }
    }
}

