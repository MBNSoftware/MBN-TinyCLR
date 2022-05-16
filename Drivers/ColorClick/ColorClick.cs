/*
 * Color Click driver for TinyCLR 2.0
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
using System.Device.I2c;
#else
using GHIElectronics.TinyCLR.Devices.Gpio;
using GHIElectronics.TinyCLR.Devices.I2c;
#endif

using System;
using System.Threading;

namespace MBN.Modules
{
    /// <summary>
    /// Main class for the Color Click driver
    /// <para><b>Pins used :</b> Scl, Sda, Int, An, Cs, Pwm</para>
    /// </summary>
    /// <example>
    /// <code language="C#">
    /// public class Program
    /// {
    ///    static ColorClick _color;
    ///    static Int32[] _tabColors;
    ///
    ///    public static void Main()
    ///    {
    ///        _tabColors = new Int32[4];
    ///
    ///        _color = new ColorClick(Hardware.SocketOne);
    ///
    ///        _color.LedR.Write(true);
    ///        _color.LedG.Write(true);
    ///        _color.LedB.Write(true);
    ///
    ///        _color.Gain = ColorClick.Gains.x4;
    ///        _color.IntegrationTime = 200;
    ///
    ///        Debug.Print("Color ID : " + _color.GetID());
    ///        Debug.Print("Integration time : " + _color.IntegrationTime);
    ///        Debug.Print("Wait time : " + _color.WaitTime);
    ///        Debug.Print("Wait long : " + _color.WaitLong);
    ///        Debug.Print("Gain : " + _color.Gain);
    ///
    ///        while (true)
    ///        {
    ///            var colorTmp = 0.0;
    ///
    ///            for (var i = 0; i &lt; 5; i++)
    ///            {
    ///                _tabColors = _color.GetAllChannels();
    ///                colorTmp += _color.RGBtoHSL(_tabColors[0] / _tabColors[3], _tabColors[1] / _tabColors[3], _tabColors[2] / _tabColors[3]);
    ///            }
    ///            colorTmp /= 16;
    ///
    ///            Debug.Print("Color : " + colorTmp);
    ///
    ///            Thread.Sleep(1200);
    ///        }
    ///    }
    /// }
    /// </code>
    /// </example>
    public sealed partial class ColorClick
    {
        private readonly I2cDevice _color;                      // I²C configuration
        private readonly Hardware.Socket _socket;

        /// <summary>
        /// Occurs when data is ready on the INT line.
        /// </summary>
        public event DataReadyEventHandler DataReady = delegate { };

        /// <summary>
        /// List of available gain values <seealso cref="Gain"/>
        /// </summary>
        public enum Gains
        {
            /// <summary>
            /// Gain x1
            /// </summary>
            x1,
            /// <summary>
            /// Gain x4
            /// </summary>
            x4,
            /// <summary>
            /// Gain x16
            /// </summary>
            x16,
            /// <summary>
            /// Gain x60
            /// </summary>
            x60
        }
        
        /// <summary>
        /// Board's red led
        /// </summary>
        public GpioPin LedR;
        /// <summary>
        /// Board's green led
        /// </summary>
        public GpioPin LedG;
        /// <summary>
        /// Board's blue led
        /// </summary>
        public GpioPin LedB;

        private Double _clear;
        private Double _red;
        private Double _green;
        private Double _blue;

        private Gains _gain = Gains.x1;

        /// <summary>
        /// Initializes a new instance of the <see cref="ColorClick"/> class.
        /// </summary>
        /// <param name="socket">The socket on which the Color Click board is plugged on MikroBus.Net board</param>
        /// <param name="address">The address of the display. Default to 0x29.</param>
        public ColorClick(Hardware.Socket socket, Byte address = 0x29)
        {
            _socket = socket;
#if (NANOFRAMEWORK_1_0)
            _color = I2cDevice.Create(new I2cConnectionSettings(socket.I2cBus, address, I2cBusSpeed.StandardMode));

            LedR = new GpioController().OpenPin(socket.AnPin);
            LedR.SetPinMode(PinMode.Output);
            LedR.Write(PinValue.Low);
            LedG = new GpioController().OpenPin(socket.Cs);
            LedG.SetPinMode(PinMode.Output);
            LedG.Write(PinValue.Low);
            LedB = new GpioController().OpenPin(socket.PwmPin);
            LedB.SetPinMode(PinMode.Output);
            LedB.Write(PinValue.Low);

            GpioPin dataReady = new GpioController().OpenPin(socket.Int);
            dataReady.SetPinMode(PinMode.Input);
            dataReady.ValueChanged += DataReady_ValueChanged;
#else
            _color = I2cController.FromName(socket.I2cBus).GetDevice(new I2cConnectionSettings(address, 100000));

            Init();
            LedR = GpioController.GetDefault().OpenPin(socket.AnPin);
            LedR.SetDriveMode(GpioPinDriveMode.Output);
            LedR.Write(GpioPinValue.Low);
            LedG = GpioController.GetDefault().OpenPin(socket.Cs);
            LedG.SetDriveMode(GpioPinDriveMode.Output);
            LedG.Write(GpioPinValue.Low);
            LedB = GpioController.GetDefault().OpenPin(socket.PwmPin);
            LedB.SetDriveMode(GpioPinDriveMode.Output);
            LedB.Write(GpioPinValue.Low);

            GpioPin dataReady = GpioController.GetDefault().OpenPin(socket.Int);
            dataReady.SetDriveMode(GpioPinDriveMode.Input);
            dataReady.ValueChanged += DataReady_ValueChanged;
#endif
        }

        #region Private methods
#if (NANOFRAMEWORK_1_0)
        private void DataReady_ValueChanged(object sender, PinValueChangedEventArgs e)
#else
        private void DataReady_ValueChanged(GpioPin sender, GpioPinValueChangedEventArgs e)
#endif
        {
            GetChannel('C');
            GetChannel('R');
            GetChannel('G');
            GetChannel('B');
            var colorEvent = DataReady;
            colorEvent(this, new DataReadyEventArgs(_red, _green, _blue, _clear));
            Thread.Sleep(10);
        }

        private void Init()
        {
            Write(Registers.ENABLE, 0x1B);
            Gain = Gains.x1;                // Gain x1
            IntegrationTime = 24;           // Integration time 24 ms
            WaitLong = false;               // "Standard" wait time (otherwise, if true, WaitTime below is 12x more)
            WaitTime = 100;

            Thread.Sleep(100);
        }

        private void Write(Byte register, Byte data)
        {
            lock (_socket.LockI2c)
            {
                _color.Write(new[] { register, data });
            }
        }

        private Byte ReadByte(Byte register)
        {
            var result = new Byte[1];

            lock (_socket.LockI2c)
            {
                _color.WriteRead(new[] { register }, result);
            }

            return result[0];
        }

        private Int32 ReadInt(Byte register)
        {
            var result = new Byte[2];

            lock (_socket.LockI2c)
            {
                _color.WriteRead(new[] { register }, result);
            }

            return result[0] + result[1] << 8;
        }
#endregion

        /// <summary>
        /// Gets the status register.
        /// </summary>
        /// <value>
        /// <para>Bit 4 : RGBC clear channel Interrupt.</para>
        /// <para>Bit 0 : RGBC Valid. Indicates that the RGBC channels have completed an integration cycle.</para>
        /// </value>
        /// <example>
        /// <code language="C#">
        ///         Debug.Print ("Status : "+_color.Status);
        /// </code>
        /// </example>
        public Byte Status
        {
            get { return ReadByte(Registers.STATUS); }
        }

        /// <summary>
        ///  Controls the internal integration time of the RGBC clear and IR channel ADCs.
        /// </summary>
        /// <value>
        /// The desired integration time in ms.
        /// </value>
        /// <example>
        /// <code language="C#">
        ///         _color.IntegrationTime = 200;
        /// </code>
        /// </example>
        public Int32 IntegrationTime
        {
            get
            {
                var regTmp = ReadByte(Registers.ATIME);
                return regTmp == 0 ? 700 : (Int32)((256 - regTmp) * 2.4);
            }
            set
            {
                Write(Registers.ATIME, value == 700 ? (Byte)0x00 : (Byte)(256 - (value / 2.4)));
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether <see cref="IntegrationTime"/> will be 12x longer.
        /// </summary>
        /// <value>
        ///   <c>true</c> if [wait long]; otherwise, <c>false</c>.
        /// </value>
        /// <example>
        /// <code language="C#">
        ///         _color.WaitLong = false;
        /// </code>
        /// </example>
        public Boolean WaitLong
        {
            get { return (ReadByte(Registers.CONFIG) & 0x02) == 0x02; }
            set
            {
                Write(Registers.CONFIG, value ? (Byte)0x02 : (Byte)0x00);
            }
        }

        /// <summary>
        /// Gets or sets the wait time in ms.
        /// <remark>Wait time is set in 2.4ms increments unless the WLONG bit is asserted, in which case the wait times are 12× longer. WTIME is programmed as a 2’s complement number.</remark>
        /// </summary>
        /// <value>
        /// The actual wait time value in ms.
        /// </value>
        /// <example>
        /// <code language="C#">
        ///         Debug.Print("Wait time : " + _color.WaitTime);
        /// </code>
        /// </example>
        public Int32 WaitTime
        {
            get
            {
                var regTmp = (Int32)((256 - ReadByte(Registers.WTIME)) * 2.4);
                return WaitLong ? regTmp * 12 : regTmp;
            }
            set
            {
                Write(Registers.WTIME, value == 700 ? (Byte)0x00 : (Byte)(256 - (value / 2.4)));
            }
        }

        /// <summary>
        /// Gets or sets the gain.
        /// </summary>
        /// <value>
        /// The gain to be set.<seealso cref="Gains"/>
        /// </value>
        /// <example>
        /// <code language="C#">
        /// _color.Gain = ColorClick.Gains.x4;
        /// </code>
        /// </example>
        public Gains Gain
        {
            get { return _gain; }
            set
            {
                Write(Registers.CONTROL, (Byte)value);
                _gain = value;
            }
        }

        /// <summary>
        /// Part number identification.
        /// </summary>
        /// <returns><para>0x14 = TCS34711 &amp; TCS34715</para><para>0x1D = TCS34713 &amp; TCS34717</para></returns>
        /// <example>
        /// <code language="C#">
        ///     Debug.Print ("Chip ID : " + _color.GetID);
        /// </code>
        /// </example>
        public Byte GetID() => ReadByte(Registers.ID);

        /// <summary>
        /// Reads the value of a specified channel.
        /// </summary>
        /// <param name="channel">The channel to read.</param>
        /// <returns>A raw value containing reading of the channel.</returns>
        /// <example>
        /// <code language="C#">
        ///     Debug.Print ("Reading of red channel : " + _color.GetChannel('R'));
        /// </code>
        /// </example>
        public Double GetChannel(Char channel)
        {
            var retVal = -1.0;

            switch (channel)
            {
                case 'C':
                    retVal = ReadInt(Registers.CDATA);
                    _clear = retVal;
                    break;
                case 'R':
                    retVal = ReadInt(Registers.RDATA);
                    _red = retVal;
                    break;
                case 'G':
                    retVal = ReadInt(Registers.GDATA);
                    _green = retVal;
                    break;
                case 'B':
                    retVal = ReadInt(Registers.BDATA);
                    _blue = retVal;
                    break;
            }
            return retVal;
        }

        /// <summary>
        /// Read all channels at once
        /// </summary>
        /// <returns>An array of bytes containing the values read for the Red, Green, Blue and Clear channel, in this order.</returns>
        /// <example>
        /// <code language="C#">
        ///     // Gets RGBC channel all at once
        ///     var AllChannels = GetAllChannels();
        /// </code>
        /// </example>
        public Double[] GetAllChannels()
        {
            var tabTmp = new Double[4];
            tabTmp[0] = GetChannel('R');
            tabTmp[1] = GetChannel('G');
            tabTmp[2] = GetChannel('B');
            tabTmp[3] = GetChannel('C');

            return tabTmp;
        }

// ReSharper disable once InconsistentNaming
        /// <summary>
        /// Converts an RGB value to its HSL equivalent
        /// </summary>
        /// <param name="red">The red value.</param>
        /// <param name="green">The green value.</param>
        /// <param name="blue">The blue value.</param>
        /// <returns>A <see cref="Double"/> representing the HSL value of the RGB components.</returns>
        /// <example>
        /// <code language="C#">
        ///       var _tabColors = _color.GetAllChannels();
        ///       var _colorHSL = _color.RGBtoHSL(_tabColors[0] / _tabColors[3], _tabColors[1] / _tabColors[3], _tabColors[2] / _tabColors[3]);
        /// </code>
        /// </example>
        public Double RGBtoHSL(Double red, Double green, Double blue)
        {
            Double saturation;

            var fmax = Math.Max(Math.Max(red, green), blue);
            var fmin = Math.Min(Math.Min(red, green), blue);

            saturation = fmax > 0 ? (fmax - fmin) / fmax : 0;

            try
            {
                Double hue;
                if (Math.Abs(saturation) < Double.Epsilon)
                    hue = 0;
                else
                {
                    hue = Equals(fmax, red)
                        ? (green - blue)/(fmax - fmin)
                        : Equals(fmax, green) ? 2 + (blue - red)/(fmax - fmin) : 4 + (red - green)/(fmax - fmin);
                    hue /= 6;

                    if (hue < 0)
                        hue += 1;
                }
                return hue;
            }
            catch (Exception)
            {
                return 0.0;
            }
        }
    }
}

