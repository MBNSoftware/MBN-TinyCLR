/*
 * CapSense Click board driver for TinyCLR 2.0
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

using GHIElectronics.TinyCLR.Devices.I2c;
using System;
using System.Threading;

namespace MBN.Modules
{
    /// <summary>
    /// Main class for the MikroE CapSense Click board driver
    /// <para><b>Pins used :</b> Scl, Sda</para>
    /// </summary>
    /// <example>
    /// <code language="C#">
    /// public class Program
    /// {
    ///    static CapSenseClick _cap;       // CapSense Click board
    ///    static BarGraphClick _bar;       // BarGraph Click board
    ///
    ///    public static void Main()
    ///    {
    ///        _cap = new CapSenseClick(Hardware.SocketOne);     // CapSense on socket 1 at address 0x00
    ///        _bar = new BarGraphClick(Hardware.SocketTwo);                // BarGraph on socket 2
    ///
    ///        _bar.Bars(0);                                            // Clear bars
    ///
    ///        _cap.ButtonPressed += Cap_ButtonPressed;                 // Subscribe to the ButtonPressed event
    ///        _cap.SliderDataChanged += Cap_SliderDataChanged;         // Subscribe to the SliderDataChanged event
    ///
    ///        while (true)
    ///        {
    ///            _cap.CheckButtons();             // Checks if any button is pressed
    ///            _cap.CheckSlider();              // Checks if slider value has changed
    ///            Thread.Sleep(20);
    ///        }
    ///    }
    ///
    ///     static void Cap_SliderDataChanged(object sender, CapSenseClick.SliderEventArgs e)
    ///    {
    ///        if (e.FingerPresent) { _bar.Bars((UInt16)(e.SliderValue / 5)); }     // Using default CapSense resolution of 50, displays 0 to 10 bars
    ///    }
    ///
    ///    static void Cap_ButtonPressed(object sender, CapSenseClick.ButtonPressedEventArgs e)
    ///    {
    ///        // Light on the led of the corresponding button when pressed (and switch off when depressed).
    ///        _cap.LedBottom = e.ButtonBottom;
    ///        _cap.LedTop = e.ButtonTop;
    ///    }
    ///}
    /// </code>
    /// </example>
    public sealed partial class CapSenseClick
    {
        /// <summary>
        /// Occurs after a call to the <see cref="CheckButtons"/> method if a button state has changed.
        /// </summary>
        /// <remarks>See <see cref="ButtonPressedEventArgs"/> for the data returned by the event.</remarks>
        public event ButtonPressedEventHandler ButtonPressed = delegate { };

        /// <summary>
        /// Occurs after a call to the <see cref="CheckSlider"/> method if a the value of the slider has changed.
        /// </summary>
        /// <remarks>See <see cref="SliderEventArgs"/> for the data returned by the event.</remarks>
        public event SliderEventHandler SliderDataChanged = delegate { };

        private readonly I2cDevice _cap;

        private Boolean _ledTop, _ledBottom;                        // Leds states
        private Boolean _buttonTop, _buttonBottom;                  // Buttons
        private Byte _buttonRegister;                                // Buttons status register
        private Boolean _buttonTopPressed, _buttonBottomPressed;      // Buttons states
        private Boolean _sliderEnabled;                             // Slider state
        private Int32 _sliderValue;                                 // Internal slider value
        private Byte[] _intResult;
        private Int32 _scanDelay;
        private Boolean _scanOk;

        /// <summary>
        /// Initializes a new instance of the <see cref="CapSenseClick"/> class.
        /// </summary>
        /// <param name="socket">The socket on which the CapSense Click board is plugged on MikroBus.Net</param>
        /// <param name="address">Address of the I²C device.</param>
        public CapSenseClick(Hardware.Socket socket, Byte address=0x00)
        {
            // Create the driver's I²C configuration
            _cap = I2cController.FromName(socket.I2cBus).GetDevice(new I2cConnectionSettings(address, 100000));
            Init();
        }

        /// <summary>
        /// Starts scanning the buttons and the slider.
        /// </summary>
        /// <param name="scanDelay">The scan delay in milliseconds.</param>
        /// <remarks>Don't put a too small delay as it may clog the I²C bus.</remarks>
        /// <example> This sample shows how to use the StartScan method.
        /// <code language="C#">
        ///         // Starts scanning the sensor
        ///         _cap.StartScan();
        /// </code>
        /// </example>
        public void StartScan(Int32 scanDelay = 100)
        {
            if (_scanOk) return;
            _scanOk = true;
            _scanDelay = scanDelay;
            new Thread(ScanThread).Start();
        }

        /// <summary>
        /// Stops the scanning of the buttons and the slider.
        /// </summary>
        /// <example> This sample shows how to use the StopScan method.
        /// <code language="C#">
        ///         // Stops scanning the sensor
        ///         _cap.StopScan();
        /// </code>
        /// </example>
        public void StopScan() => _scanOk = false;

        private void ScanThread()
        {
            while (_scanOk)
            {
                CheckButtons();
                CheckSlider();
                Thread.Sleep(_scanDelay);
            }
        }

        private void Init()
        {
            // This part is taken from MikroElektronika example
            SetRegister(Registers.COMMAND, 0x08);               // Enter setup mode
            SetRegister(Registers.CS_ENABLE, 0x1F);             // Five pins will be used for Slider pads
            SetRegister(Registers.CS_ENABL0, 0x18);             // Two pins will be used for Button pads
            SetRegister(Registers.GPIO_ENABLE0, 0x03);          // Three pins will be used as GPIO : 2 for LED and 1 as GPIO2
            SetRegister(Registers.DM_STRONG0, 0x03);            // Enable strong reading for GPIO
            SetRegister(Registers.COMMAND, 0x01);               // Store current configuration to NVM
            SetRegister(Registers.COMMAND, 0x06);               // Reconfigure device (POR)
            Thread.Sleep(20);

            /* Slider default resolution = 50
             * Resolution = (SensorsInSlider - 1) * Multiplier
             * Multiplier = 50 / 4 = 12.5
             * MSB = 12 (integer part)
             * LSB = 128 (1/2 fractionnal part)
            */
            SetRegister(Registers.CS_SLID_MULM, 0x0C);          // MSB = 12
            SetRegister(Registers.CS_SLID_MULM, 0x0C);          // Written twice, otherwise MSB doesn't seem to be saved !?
            SetRegister(Registers.CS_SLID_MULL, 0x80);          // LSB = 128
            SliderEnabled = true;       // Enable slider

            SetRegister(Registers.OUTPUT_PORT0, 0x03);          // Leds OFF (inverse logic 0-LED ON, 1-LED OFF)
            _ledTop = _ledBottom = false;
            _buttonTop = _buttonBottom = false;

            _intResult = new Byte[2];
        }

        private Byte ReadByte(Byte register)
        {
            var result = new Byte[1];
            try
            {
                lock (Hardware.LockI2C)
                {
                    _cap.WriteRead(new[] { register }, result);
                }
                if (register == Registers.COMMAND)
                    Thread.Sleep(120);
                else
                    Thread.Sleep(20);
                return result[0];
            }
            catch
            {
                return 0;
            }
        }

        private Int32 ReadInteger(Byte register)
        {
            try
            {
                lock (Hardware.LockI2C)
                {
                    _cap.WriteRead(new[] { register }, _intResult);
                }
                if (register == Registers.COMMAND)
                    Thread.Sleep(120);
                else
                    Thread.Sleep(20);
                return (_intResult[0] << 8) + _intResult[1];
            }
            catch 
            { 
                return 0; 
            }
        }

        private void SetRegister(Byte register, Byte value)
        {
            lock (Hardware.LockI2C)
            {
                _cap.Write(new[] { register, value });
            }
            if (register == Registers.COMMAND)
                Thread.Sleep(120);
            else
                Thread.Sleep(20);
            Thread.Sleep(20);
        }

        /// <summary>
        /// Enable/disable the slider
        /// </summary>
        /// <value>
        /// <c>true</c> to enabled slider, otherwise <c>false</c>.
        /// </value>
        /// <example> This sample shows how to use the SliderEnabled property.
        /// <code language="C#">
        ///         // Enables slider scanning
        ///         _cap.SliderEnabled = true;
        /// </code>
        /// </example>
        public Boolean SliderEnabled
        {
            get { return _sliderEnabled; }
            set
            {
                SetRegister(Registers.CS_SLID_CONFIG, value ? (Byte)0x01 : (Byte)0x00);
                _sliderEnabled = value;
            }
        }

        /// <summary>
        /// Checks if buttons are pressed. An event is raised if any button state is changing.
        /// </summary>
        /// <example> This sample shows how to use the CheckButtons method.
        /// <code language="C#">
        ///         _cap.CheckButtons();
        /// </code>
        /// </example>
        /// <seealso cref="ButtonPressedEventArgs"/>
        public void CheckButtons()
        {
            _buttonRegister = ReadByte(Registers.CS_READ_STATUSM);
            _buttonTopPressed = (_buttonRegister & 0x08) >> 3 == 1;
            _buttonBottomPressed = (_buttonRegister & 0x10) >> 4 == 1;

            if ((_buttonBottom != _buttonBottomPressed) | (_buttonTopPressed != _buttonTop))
            {
                _buttonTop = _buttonTopPressed;
                _buttonBottom = _buttonBottomPressed;

                ButtonPressedEventHandler buttonEvent = ButtonPressed;
                buttonEvent(this, new ButtonPressedEventArgs(_buttonTopPressed, _buttonBottomPressed));
            }
        }

        /// <summary>
        /// Checks the slider value. An event is raised if the slider value has changed.
        /// </summary>
        /// <example> This sample shows how to use the CheckSlider method.
        /// <code language="C#">
        ///         _cap.CheckSlider();
        /// </code>
        /// </example>
        /// <seealso cref="SliderEventArgs"/>
        public void CheckSlider()
        {
            var val = ReadInteger(Registers.CS_READ_CEN_POSM);
            if (val != _sliderValue)
            {
                _sliderValue = val;
                SliderEventHandler sliderEvent = SliderDataChanged;
                sliderEvent(this, new SliderEventArgs(val, val != 65535));
            }
        }

        /// <summary>
        /// Turns ON/OFF the top led (the one above the slider).
        /// </summary>
        /// <summary>
        /// Turns ON/OFF the bottom led (the one below the slider).
        /// </summary>
        /// <example> This sample shows how to use the LedTop property.
        /// <code language="C#">
        ///         // Turns off the top led
        ///         _cap.LedTop = false;
        /// </code>
        /// </example>
        public Boolean LedTop
        {
            get { return _ledTop; }
            set
            {
                var currentState = (Byte)((_ledBottom ? 0 : 2) + (_ledTop ? 0 : 1));
                SetRegister(Registers.OUTPUT_PORT0, (Byte)(value ? currentState & 0xFE : currentState | 0x01));
                _ledTop = value;
            }
        }

        /// <summary>
        /// Turns ON/OFF the bottom led (the one below the slider).
        /// </summary>
        /// <example> This sample shows how to use the LedBottom property.
        /// <code language="C#">
        ///         // Light on the botton led
        ///         _cap.LedBottom = true;
        /// </code>
        /// </example>
        public Boolean LedBottom
        {
            get { return _ledBottom; }
            set
            {
                var currentState = (Byte)((_ledBottom ? 0 : 2) + (_ledTop ? 0 : 1));
                SetRegister(Registers.OUTPUT_PORT0, (Byte)(value ? currentState & 0xFD : currentState | 0x02));
                _ledBottom = value;
            }
        }
    }
}

