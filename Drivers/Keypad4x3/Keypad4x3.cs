/*
 * Keypad4X4 driver for TinyCVLR 2.0
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
#else
using GHIElectronics.TinyCLR.Devices.Gpio;
#endif

using System;
using System.Threading;

namespace MBN.Modules
{
    /// <summary>
    /// Main class for the Keypad4x3 driver
    /// <para><b>Pins used :</b> Various GPIOs, depending on user's choice</para>
    /// </summary>
    /// <example>
    /// <code language="C#">
    ///	   public class Program
    ///    {
    ///        private static Keypad4X3 _keypad;
    ///
    ///        public static void Main()
    ///        {
    ///            // Pins PE5..PE2 are connected to rows, PD7..PD3 to columns
    ///            _keypad = new Keypad4X3(Pin.PE5, Pin.PE4, Pin.PE3, Pin.PE2, Pin.PD7, Pin.PD4, Pin.PD3);
    ///            _keypad.KeyReleased += KeypadKeyReleased;
    ///            _keypad.KeyPressed += KeypadKeyPressed;
    ///
    ///            _keypad.StartScan();
    ///
    ///            Thread.Sleep(Timeout.Infinite);
    ///        }
    ///
    ///        static void KeypadKeyPressed(object sender, Keypad4X3.KeyPressedEventArgs e)
    ///        {
    ///            Hardware.Led1.Write(true);
    ///        }
    ///
    ///        static void KeypadKeyReleased(object sender, Keypad4X3.KeyReleasedEventArgs e)
    ///        {
    ///            Debug.Print("Key : " + e.KeyChar);
    ///            Hardware.Led1.Write(false);
    ///        }
    ///    }
    /// </code>
    /// </example>
    public sealed partial class Keypad4X3
    {
        /// <summary>
        /// Occurs when a key is pressed
        /// </summary>
        public event KeyPressedEventHandler KeyPressed = delegate { };
        /// <summary>
        /// Occurs when a key is released
        /// </summary>
        public event KeyReleasedEventHandler KeyReleased = delegate { };

        private static GpioPin[] _rows;
        private static GpioPin[] _columns;
        private Thread _scanThread;
        private Boolean _scanThreadActive;

        /// <summary>
        /// Initializes a new instance of the <see cref="Keypad4X3"/> class.
        /// </summary>
        /// <param name="row1">Pin connected to the first row</param>
        /// <param name="row2">Pin connected to the second row</param>
        /// <param name="row3">Pin connected to the third row</param>
        /// <param name="row4">Pin connected to the fourth row</param>
        /// <param name="column1">Pin connected to the first column</param>
        /// <param name="column2">Pin connected to the second column</param>
        /// <param name="column3">Pin connected to the third column</param>
        public Keypad4X3(Int32 row1, Int32 row2, Int32 row3, Int32 row4, Int32 column1, Int32 column2, Int32 column3)
        {
#if (NANOFRAMEWORK_1_0)
            var gpioController = new GpioController();
            _rows = new []
            {
                gpioController.OpenPin(row1),
                gpioController.OpenPin(row2),
                gpioController.OpenPin(row3),
                gpioController.OpenPin(row4),
            };
            for (var i=0; i<4; i++)
            {
                _rows[i].SetPinMode(PinMode.Output);
                _rows[i].Write(PinValue.Low);
            }

            _columns = new[]
            {
                gpioController.OpenPin(column1),
                gpioController.OpenPin(column2),
                gpioController.OpenPin(column3),
            };
            for (var i = 0; i < 3; i++)
            {
                _columns[i].SetPinMode(PinMode.InputPullDown);
            }
#else
            _rows = new []
            {
                GpioController.GetDefault().OpenPin(row1),
                GpioController.GetDefault().OpenPin(row2),
                GpioController.GetDefault().OpenPin(row3),
                GpioController.GetDefault().OpenPin(row4),
            };
            for (var i=0; i<4; i++)
            {
                _rows[i].SetDriveMode(GpioPinDriveMode.Output);
                _rows[i].Write(GpioPinValue.Low);
            }

            _columns = new[]
            {
                GpioController.GetDefault().OpenPin(column1),
                GpioController.GetDefault().OpenPin(column2),
                GpioController.GetDefault().OpenPin(column3),
            };
            for (var i = 0; i < 3; i++)
            {
                _columns[i].SetDriveMode(GpioPinDriveMode.InputPullDown);
            }
#endif
        }

        /// <summary>
        /// Enables keys scanning
        /// </summary>
        /// <example>
        /// <code language="C#">
        ///     _keypad.StartScan();
        /// </code>
        /// </example>
        public void StartScan()
        {
            if (_scanThreadActive) { return; }
            _scanThreadActive = true;
            _scanThread = new Thread(ScanThreadMethod);
            _scanThread.Start();
        }

        /// <summary>
        /// Disables keys scanning
        /// </summary>
        /// <example>
        /// <code language="C#">
        ///     _keypad.StopScan();
        /// </code>
        /// </example>
        public void StopScan() => _scanThreadActive = false;

        private void ScanThreadMethod()
        {
            var prevKey = -1;
            while (_scanThreadActive)
            {
                var nbKey = 0;
                // Scans the matrix
                for (var i = 0; i < 4; i++)
                {
                    for (var j = 0; j < 3; j++)
                    {
                        if (!ReadMatrix(i, j)) continue;
                        // A key has been pressed
                        var keyNum = (i * 3) + j + 1;
                        nbKey += keyNum;
                        if ((prevKey != keyNum) && (prevKey == -1))  // A key has been pressed and no other is currently pressed (avoids dealing with multiple keys at the same time)
                        {
                            prevKey = keyNum;
                            KeyPressedEventHandler tempEvent = KeyPressed;
                            tempEvent(this, new KeyPressedEventArgs(keyNum, KeytoChar(keyNum)));
                        }
                        break;
                    }
                }
                if (nbKey == 0)  // No key pressed in this pass
                {
                    // Was there a key pressed before ?
                    if (prevKey != -1)
                    {
                        KeyReleasedEventHandler tempEvent = KeyReleased;
                        tempEvent(this, new KeyReleasedEventArgs(prevKey, KeytoChar(prevKey)));
                    }
                    prevKey = -1;
                }
                // Leave time for other processes
                Thread.Sleep(50);
            }
        }

        private static Boolean ReadMatrix(Int32 row, Int32 column)
        {
#if (NANOFRAMEWORK_1_0)
            _rows[row].Write(PinValue.High);
            PinValue colState = _columns[column].Read();
            _rows[row].Write(PinValue.Low);

            return colState == PinValue.High;
#else
            _rows[row].Write(GpioPinValue.High);
            GpioPinValue colState = _columns[column].Read();
            _rows[row].Write(GpioPinValue.Low);

            return colState == GpioPinValue.High;
#endif
        }

        private static Char KeytoChar(Int32 keyValue)
        {
            Char c;
            if (keyValue < 10) { c = (Char)(keyValue + 48); }
            else
            {
                switch (keyValue)
                {
                    case 10: c = '*';
                        break;
                    case 11: c = '0';
                        break;
                    case 12: c = '#';
                        break;
                    default: c = ' ';   // Should never happen
                        break;
                }
            }

            return c;
        }

        /// <summary>
        /// Do you really expect a driver to return a string ?! Come on ! ;-)
        /// </summary>
        /// <returns>
        /// A <see cref="System.String"/> that contains information, though.
        /// </returns>
        public override String ToString() => "'Keypad 4x3' driver for MBN boards, by MikroBus.Net. Based on GHI's original driver code.";
    }
}

