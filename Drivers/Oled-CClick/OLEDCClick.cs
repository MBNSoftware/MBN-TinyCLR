/*
 * OLED-C Click driver for TinyCLR 2.0.
 * 
 * Initial version coded by Stephen Cardinale
 * 
 *  
 * Copyright 2020 Stephen Cardinale
 * Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License. You may obtain a copy of the License at
 * http://www.apache.org/licenses/LICENSE-2.0
 * Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, 
 * either express or implied. See the License for the specific language governing permissions and limitations under the License.
 */

#if (NANOFRAMEWORK_1_0)
using System.Device.Gpio;
using Windows.Devices.Spi;
using nanoFramework.UI;
using nanoFramework.Presentation;
#else
using GHIElectronics.TinyCLR.Devices.Gpio;
using GHIElectronics.TinyCLR.Devices.Spi;
using System.Drawing;
using Microsoft.SPOT;
#endif

using System;
using System.Threading;


namespace MBN.Modules
{
    ///<summary>
    /// Main class for the OLED-C Click driver
    /// <para><b>Pins used:</b> Mosi, Miso, Sck, Cs, Rst, Pwm, An</para>
    /// <para><b>This module is an SPIDevice</b></para>
    /// <para><b>References required:</b> MikroBus.Net, Microsoft.SPOT.Graphics, Microsoft.SPOT.Native, Microsoft.Spot.Hardware, mscorlib</para>
    /// </summary>
    /// <example>
    /// <code language="C#">
    /// using MBN;
    /// using MBN.Modules;
	///
    /// using Microsoft.SPOT;
    /// using Microsoft.SPOT.Presentation.Media;
	///
    /// using System.Threading;
	///
    /// namespace Examples
    /// {
    ///     public class Program
    ///     {
    ///         static OLEDCClick _oled;
    ///         static readonly MikroFont _font1 = FontManager.GetFont(FontManager.FontName.TahomaReg12);
	///
    ///         public static void Main()
    ///         {
    ///             _oled = new OLEDCClick(Hardware.SocketOne) { FrameRate = OLEDCClick.FrameRates.OCS_140_4Hz};
	///
    ///             _oled.Canvas.Clear(KnownColors.Wheat);
    ///             _oled.Canvas.DrawText("Hello", _font1, KnownColors.Red, 0, (_oled.CanvasHeight - _font1.FontHeight) / 2, 96, _font1.FontHeight, true);
    ///             _oled.Flush();
	///
    ///             Thread.Sleep(Timeout.Infinite);
    ///         }
    ///     }
    /// }
    /// </code>
    /// </example>
    public class OLEDCClick
	{
		#region Constants

		private const Byte SEPS114A_SOFT_RESET = 0x01;
		private const Byte SEPS114A_DISPLAY_ON_OFF = 0x02;
		private const Byte SEPS114A_ANALOG_CONTROL = 0x0F;
		private const Byte SEPS114A_STANDBY_ON_OFF = 0x14;
		private const Byte SEPS114A_OSC_ADJUST = 0x1A;
		private const Byte SEPS114A_ROW_SCAN_DIRECTION = 0x09;
		private const Byte SEPS114A_DISPLAY_X1 = 0x30;
		private const Byte SEPS114A_DISPLAY_X2 = 0x31;
		private const Byte SEPS114A_DISPLAY_Y1 = 0x32;
		private const Byte SEPS114A_DISPLAY_Y2 = 0x33;
		private const Byte SEPS114A_DISPLAYSTART_X = 0x38;
		private const Byte SEPS114A_DISPLAYSTART_Y = 0x39;
		private const Byte SEPS114A_CPU_IF = 0x0D;
		private const Byte SEPS114A_MEM_X1 = 0x34;
		private const Byte SEPS114A_MEM_X2 = 0x35;
		private const Byte SEPS114A_MEM_Y1 = 0x36;
		private const Byte SEPS114A_MEM_Y2 = 0x37;
		private const Byte SEPS114A_MEMORY_WRITE_READ = 0x1D;
		private const Byte SEPS114A_DDRAM_DATA_ACCESS_PORT = 0x08;
		private const Byte SEPS114A_DISCHARGE_TIME = 0x18;
		private const Byte SEPS114A_PEAK_PULSE_DELAY = 0x16;
		private const Byte SEPS114A_PEAK_PULSE_WIDTH_R = 0x3A;
		private const Byte SEPS114A_PEAK_PULSE_WIDTH_G = 0x3B;
		private const Byte SEPS114A_PEAK_PULSE_WIDTH_B = 0x3C;
		private const Byte SEPS114A_PRECHARGE_CURRENT_R = 0x3D;
		private const Byte SEPS114A_PRECHARGE_CURRENT_G = 0x3E;
		private const Byte SEPS114A_PRECHARGE_CURRENT_B = 0x3F;
		private const Byte SEPS114A_COLUMN_CURRENT_R = 0x40;
		private const Byte SEPS114A_COLUMN_CURRENT_G = 0x41;
		private const Byte SEPS114A_COLUMN_CURRENT_B = 0x42;
		private const Byte SEPS114A_ROW_OVERLAP = 0x48;
		private const Byte SEPS114A_SCAN_OFF_LEVEL = 0x49;
		private const Byte SEPS114A_ROW_SCAN_ON_OFF = 0x17;
		private const Byte SEPS114A_ROW_SCAN_MODE = 0x13;
		private const Byte SEPS114A_SCREEN_SAVER_CONTEROL = 0xD0;
		//private const byte SEPS114A_SS_SLEEP_TIMER = 0xD1;
		private const Byte SEPS114A_SCREEN_SAVER_MODE = 0xD2;
		private const Byte SEPS114A_SS_UPDATE_TIMER = 0xD3;
		private const Byte SEPS114A_RGB_IF = 0xE0;
		private const Byte SEPS114A_RGB_POL = 0xE1;
		private const Byte SEPS114A_DISPLAY_MODE_CONTROL = 0xE5;

		#endregion

		#region Fields

		internal const Int32 _canvasWidth = 96;
		internal const Int32 _canvasHeight = 96;

        private FrameRates _frameRate;
		private readonly Hardware.Socket _socket;
		private readonly GpioPin _dataCommandPin;
		private readonly GpioPin _resetPin;
		private readonly GpioPin _readWritePin;

		private readonly SpiDevice _oled;
		private PowerModes _powerMode;

		#endregion

		#region ENUMS

		/// <summary>
		/// 
		/// </summary>
		public enum ScreenSaverMode
		{
			/// <summary>
			/// 
			/// </summary>
			PanLeft = 0,
			/// <summary>
			/// 
			/// </summary>
			PanRight = 1,
			/// <summary>
			/// 
			/// </summary>
			DownScroll = 2,
			/// <summary>
			/// 
			/// </summary>
			UpScroll = 3,
			/// <summary>
			/// 
			/// </summary>
			FadeIn = 4,
			/// <summary>
			/// 
			/// </summary>
			FadeOut = 5,
			/// <summary>
			/// 
			/// </summary>
			FadeInOut = 6,
			/// <summary>
			/// 
			/// </summary>
			PixelVibration = 7
		}

		/// <summary>
		/// Then enumeration containing values used to adjust the FrameRate of the SEPS114A IC Oscillator.
		/// 80 frames/second (Minimum frame rate) - 140 frames/second (Maximum Frame rate)
		/// OCS_140Hz = 0x0C, OCS_140_2Hz = 0x0D, OCS_140_3Hz = 0x0E, OCS_140_4Hz = 0x0F all will result in the maxim framer rate per datasheet.
		/// </summary>
		public enum FrameRates
		{
			/// <summary>
			/// 80 frames/second - Minimum Frame rate
			/// </summary>
			OCS_80Hz = 0x00,
			/// <summary>
			/// 85 frames/second
			/// </summary>
			OCS_85Hz = 0x01,
			/// <summary>
			/// 90 frames/second
			/// </summary>
			OCS_90Hz = 0x02,
			/// <summary>
			/// 95 frames/second (Default).
			/// </summary>
			OCS_95Hz = 0x03,
			/// <summary>
			/// 100 frames/second
			/// </summary>
			OCS_100Hz = 0x04,
			/// <summary>
			/// 105 frames/second
			/// </summary>
			OCS_105Hz = 0x05,
			/// <summary>
			/// 110 frames/second
			/// </summary>
			OCS_110Hz = 0x06,
			/// <summary>
			/// 115 frames/second
			/// </summary>
			OCS_115Hz = 0x07,
			/// <summary>
			/// 120 frames/second
			/// </summary>
			OCS_120Hz = 0x08,
			/// <summary>
			/// 125 frames/second
			/// </summary>
			OCS_125Hz = 0x09,
			/// <summary>
			/// 130 frames/second
			/// </summary>
			OCS_130Hz = 0x0A,
			/// <summary>
			/// 135 frames/second
			/// </summary>
			OCS_135Hz = 0x0B,
			/// <summary>
			/// 140 frames/second - Maximum Frame rate
			/// </summary>
			OCS_140Hz = 0x0C,
			/// <summary>
			/// 140 frames/second - Maximum Frame rate
			/// </summary>
			OCS_140_2Hz = 0x0D,
			/// <summary>
			/// 140 frames/second - Maximum Frame rate
			/// </summary>
			OCS_140_3Hz = 0x0E,
			/// <summary>
			/// 140 frames/second - Maximum Frame rate
			/// </summary>
			OCS_140_4Hz = 0x0F
		}

		#endregion

		#region CTOR

		/// <summary>
		/// Initializes a new instance of the <see cref="OLEDCClick"/> class.
		/// </summary>
		/// <param name="socket">The socket on which the OLEDc Click board is inserted on MikroBus.Net</param>
		/// <example>Example usage:
		/// <code language = "C#">
		///	var _oled = new OLEDCClick(Hardware.SocketFour);
		/// </code>
		/// </example>
		public OLEDCClick(Hardware.Socket socket)
		{
			_socket = socket;
#if (NANOFRAMEWORK_1_0)
            _oled = SpiDevice.FromId(socket.SpiBus, new SpiConnectionSettings(socket.Cs)
            {
                Mode = SpiMode.Mode3,
                ClockFrequency = 40 * 1000 * 1000
            });

			_resetPin = new GpioController().OpenPin(socket.Rst);
			_resetPin.SetPinMode(PinMode.Output);
			_resetPin.Write(PinValue.High);

			_dataCommandPin = new GpioController().OpenPin(socket.PwmPin);
			_dataCommandPin.SetPinMode(PinMode.Output);
			_dataCommandPin.Write(PinValue.High);

			_readWritePin = new GpioController().OpenPin(socket.AnPin);
			_readWritePin.SetPinMode(PinMode.Output);
			_readWritePin.Write(PinValue.High);
#else
            _oled = SpiController.FromName(socket.SpiBus).GetDevice(new SpiConnectionSettings()
            {
                ChipSelectType = SpiChipSelectType.Gpio,
                ChipSelectLine = GpioController.GetDefault().OpenPin(socket.Cs),
				ChipSelectActiveState = false,
                Mode = SpiMode.Mode3,
                ClockFrequency = 40 * 1000 * 1000
            });

			_resetPin = GpioController.GetDefault().OpenPin(socket.Rst);
            _resetPin.SetDriveMode(GpioPinDriveMode.Output);
            _resetPin.Write(GpioPinValue.High);

            _dataCommandPin = GpioController.GetDefault().OpenPin(socket.PwmPin);
            _dataCommandPin.SetDriveMode(GpioPinDriveMode.Output);
            _dataCommandPin.Write(GpioPinValue.High);

            _readWritePin = GpioController.GetDefault().OpenPin(socket.AnPin);
            _readWritePin.SetDriveMode(GpioPinDriveMode.Output);
            _readWritePin.Write(GpioPinValue.High);
#endif

			Canvas = new MikroBitmap(_canvasWidth, _canvasHeight);

			InitilizeOLED();
		}

		#endregion

		#region Public Properties

		/// <summary>
		/// Gets the default canvas surface to draw on.
		/// </summary>
		/// <example>Example usage:
		/// <code language = "C#">
		///	var newCanvas = new byte[96 * 96];
		/// _oled.Canvas.Pixels = newCanvas;
		/// </code>
		/// </example>
		public MikroBitmap Canvas { get; }

        /// <summary>
		/// Gets the Height of the Canvas.
		/// </summary>
		/// <example>Example usage:
		/// <code language = "C#">
		///	Debug.Print("Canvas height - " + _oled.Canvas.Height);
		/// </code>
		/// </example>
		public Int32 CanvasHeight => _canvasHeight;

        /// <summary>
		/// Gets the Width of the Canvas.
		/// </summary>
		/// <example>Example usage:
		/// <code language = "C#">
		///	Debug.Print("Canvas width - " + _oled.Canvas.Width);
		/// </code>
		/// </example>
		public Int32 CanvasWidth => _canvasWidth;

        /// <summary>
		/// Gets or sets the FrameRate of the of the SEPS114A IC Oscillator.
		/// </summary>
		/// <example>Example usage:
		/// <code language = "C#">
		///	_oled.FrameRate = OLEDCClick.FrameRates.OCS_140Hz;
		/// </code>
		/// </example>
		public FrameRates FrameRate
		{
			get => _frameRate;
            set
			{
				SendCommand(SEPS114A_OSC_ADJUST, (Byte)value);
				_frameRate = value;
			}
		}

		/// <summary>
		/// Gets or sets the power mode.
		/// </summary>
		/// <value>
		/// The current power mode of the module.
		/// </value>
		/// <remarks>
		/// If the module has no power modes, then GET should always return PowerModes.ON while SET should throw a NotImplementedException.
		/// </remarks>
		/// <example>Example usage:
		/// <code language = "C#">
		///	Debug.Print("Current PowerMode - " + _oled.PowerMode);
		///	_oled.PowerMode = PowerModes.Low;
		///	Debug.Print("New PowerMode - " + _oled.PowerMode);
		///	_oled.PowerMode = PowerModes.On;
		///	Debug.Print("New PowerMode - " + _oled.PowerMode);
		/// </code>
		/// </example>
		public PowerModes PowerMode
		{
			get => _powerMode;
            set
			{
				switch (_powerMode)
				{
					case PowerModes.Off:
					{
						throw new NotSupportedException("This module does not support PowerModes.Off");
					}
					case PowerModes.Low:
					{
						SendCommand(SEPS114A_STANDBY_ON_OFF, 0x01);
						SendCommand(SEPS114A_STANDBY_ON_OFF, 0x00);
						break;
					}
					case PowerModes.On:
					{
						SendCommand(SEPS114A_STANDBY_ON_OFF, 0x00);
						SendCommand(SEPS114A_STANDBY_ON_OFF, 0x01);
						break;
					}
					default:
					{
						throw new NotSupportedException("Unknown Reset method passed.");
					}
				}
				Thread.Sleep(5); // Wait for 5ms (1ms Delay Minimum)
				_powerMode = value;
			}
		}

		#endregion

		#region Public Methods

		/// <summary>
		/// Flushes the default Canvas to the OLED-C Click Display
		/// </summary>
		/// <returns></returns>
		/// <example>Example usage:
		/// <code language = "C#">
		/// _oled.Canvas.DrawRectangle(KnownColors.Red, KnownColors.Red, new Rect(0, 0, 96, 16));
		///	_oled.Flush();
		/// </code>
		/// </example>
		public void Flush()
		{
			lock (Canvas)
			{
				EnableDDRAMAccess();
				SendData(Canvas.Pixels);
			}
		}

		/// <summary>
		/// Flushes the supplied Bitmap object to OLED-C Click Display
		/// </summary>
		/// <param name="bitmap">The MikroBitmap object to flush to the OLED-C display.</param>
		/// <returns>True if the operation was successful or otherwise false. for instance, attempting to flush at a destination that is outside the dimensions of the OLED-C display. </returns>
		/// <example>Example usage:
		/// <code language = "C#">
		///	MikroBitmap newCanvas = new MikroBitmap(96, 96);
		///	newCanvas.DrawRectangle(KnownColors.Red, KnownColors.Red, new Rect(0, 0, 96, 16));
		///	_oled.Flush(newCanvas);
		/// </code>
		/// </example>
		public void Flush(MikroBitmap bitmap)
		{
			lock (bitmap)
			{
				EnableDDRAMAccess();
				SendData(bitmap.Pixels);
			}
		}

	    private UInt16[] Rgb8ToBgr5(Byte[] orig)
	    {
	        Int32 byteCounter = -1;
	        Int32 arraySize = orig.Length;
	        UInt16[] tmp = new UInt16[96*96];
	        for (Int32 i = 0; i < arraySize; i += 4)
	        {
	            tmp[++byteCounter] = (UInt16) ((orig[i + 2] >> 3) | ((orig[i + 1] & 0xfc) << 3) | ((orig[i] & 0xf8) << 8));
	        }

	        return tmp;
	    }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="bitmap"></param>
        public void Flush(Bitmap bitmap)
        {
            lock (bitmap)
            {
                EnableDDRAMAccess();
                //// If using our native Graphics conversions
                //UInt16[] bmp = new UInt16[96 * 96];
                //Graphics.Rgb8ToBgr5(bitmap.GetBitmap(), bmp);
                //SendData(bmp);

                // If using the managed method (slower, of course)
                SendData(Rgb8ToBgr5(bitmap.ToMikroBitmap()));
            }
        }

        /// <summary>
        /// Starts the Screen Saver (Vertical scroll, Horizontal panning, Fade in/out, etc.)
        /// </summary>
        /// <param name="mode">The <see cref="ScreenSaverMode"/> to use.</param>
        /// <param name="scrollDelay">The delay between frames.</param>
        /// <remarks>The <see cref="ScreenSaverMode.PixelVibration"/> uses the delay parameter as a Pixel offset for the vibrations rather than a delay between frames.</remarks>
        /// <example>Example usage:
        /// <code language = "C#">
        ///	for (byte x = 0; x <![CDATA[<]]> 8; x++)
        ///	{
        ///		_oled.StartScreenSaver((OLEDCClick.ScreenSaverMode) x, x == 7 ? (byte) 0x02 : (byte) 0x00);
        ///		Thread.Sleep(5000);
        ///		_oled.StopScreenSaver();
        ///	}
        /// </code>
        /// </example>
        public void StartScreenSaver(ScreenSaverMode mode, Byte scrollDelay)
		{
			SendCommand(SEPS114A_SCREEN_SAVER_MODE, (Byte)mode);
			SendCommand(SEPS114A_SS_UPDATE_TIMER, scrollDelay);
			SendCommand(SEPS114A_SCREEN_SAVER_CONTEROL, 0x88);
		}

		/// <summary>
		/// Stops the ScreenSaver.
		/// </summary>
		/// <remarks>The ScreenSaver must be stopped before changing ScreenSaver modes.</remarks>
		/// <example>Example usage:
		/// <code language = "C#">
		///	_oled.StopScreenSaver();
		/// </code>
		/// </example>
		public void StopScreenSaver()
		{
			SendCommand(SEPS114A_SCREEN_SAVER_CONTEROL, 0x00);
		}

		/// <summary>
		/// Resets the OLED-c Click.
		/// </summary>
		/// <example>Example usage:
		/// <code language = "C#">
		/// _oled.Reset(ResetModes.Soft);
		/// </code>
		/// </example>
		/// <exception cref="NotSupportedException">A NotSupportedException will be thrown if an invalid reset method is passed as a parameter.</exception>
		public Boolean Reset(ResetModes resetMode)
		{
			switch (resetMode)
			{
                case ResetModes.Soft:
                {
                    throw new NotSupportedException("This Modules does not support software resets. Use a hard reset to reset this module.");
                }
				case ResetModes.Hard:
				{
#if (NANOFRAMEWORK_1_0)
						_resetPin.Write(PinValue.High);
						_dataCommandPin.Write(PinValue.High);
						_readWritePin.Write(PinValue.High);
#else
					_resetPin.Write(GpioPinValue.High);
					_dataCommandPin.Write(GpioPinValue.High);
					_readWritePin.Write(GpioPinValue.High);
#endif

						InitilizeOLED();
					break;
				}
				default:
				{
					throw new NotSupportedException("Unknown Reset method passed.");
				}
			}
			return true;
		}

		/// <summary>
		/// Turns on or off the OLED-C Click Display.
		/// </summary>
		/// <param name="on">Set to true to turn on the display or false to turn off the display.</param>
		/// <remarks>This is not the same as setting the <see cref="PowerMode"/> to Low or Off. This merely turns off the OLED Screen.</remarks>
		/// <example>Example usage:
		/// <code language = "C#">
		///	_oled.SetDisplayOn(false);
		///	Thread.Sleep(1000);
		///	_oled.SetDisplayOn(true);
		/// </code>
		/// </example>
		public void SetDisplayOn(Boolean on)
		{
			SendCommand(SEPS114A_DISPLAY_ON_OFF, (Byte)(on ? 0x01 : 0x00));
		}

#endregion

#region Private Methods

        private void InitilizeOLED()
		{
#if (NANOFRAMEWORK_1_0)
			_readWritePin.Write(PinValue.Low);
			Thread.Sleep(100);

			_resetPin.Write(PinValue.Low);
			Thread.Sleep(10);
			_resetPin.Write(PinValue.High);
#else
			_readWritePin.Write(GpioPinValue.Low);
			Thread.Sleep(100);

			_resetPin.Write(GpioPinValue.Low);
			Thread.Sleep(10);
			_resetPin.Write(GpioPinValue.High);
#endif
			Thread.Sleep(10);

			/*  Soft reset */
			SendCommand(SEPS114A_SOFT_RESET, 0x00);
			/* Standby ON/OFF*/
			SendCommand(SEPS114A_STANDBY_ON_OFF, 0x01);                 // Standby on
			Thread.Sleep(5);                                            // Wait for 5ms (1ms Delay Minimum)
			SendCommand(SEPS114A_STANDBY_ON_OFF, 0x00);                 // Standby off
			Thread.Sleep(5);                                            // 1ms Delay Minimum (1ms Delay Minimum)
			/* Display OFF */
			SendCommand(SEPS114A_DISPLAY_ON_OFF, 0x00);
			/* Set Oscillator operation */
			SendCommand(SEPS114A_ANALOG_CONTROL, 0x00);                 // using external resistor and internal OSC
			/* Set frame rate */
			SendCommand(SEPS114A_OSC_ADJUST, (Byte)FrameRates.OCS_95Hz); // frame rate : 95Hz --- was 0x03 for 95 hz, now 0x0A for 130hz , 0x0F for 140
			/* Set active display area of panel */
			SendCommand(SEPS114A_DISPLAY_X1, 0x00);
			SendCommand(SEPS114A_DISPLAY_X2, 0x5F);
			SendCommand(SEPS114A_DISPLAY_Y1, 0x00);
			SendCommand(SEPS114A_DISPLAY_Y2, 0x5F);
			/* Select the RGB data format and set the initial state of RGB interface port */
			SendCommand(SEPS114A_RGB_IF, 0x00);                          // RGB 8bit interface was 0x00 for 8-bit
			/* Set RGB polarity */
			SendCommand(SEPS114A_RGB_POL, 0x00);
			/* Set display mode control */
			SendCommand(SEPS114A_DISPLAY_MODE_CONTROL, 0x80);           // SWAP:BGR, Reduce current : Normal, DC[1:0] : Normal was 0x80
			/* Set MCU Interface */
			SendCommand(SEPS114A_CPU_IF, 0x00);                         // MPU External interface mode, 8bits
			/* Set Memory Read/Write mode */
			SendCommand(SEPS114A_MEMORY_WRITE_READ, 0x00);
			/* Set row scan direction */
			SendCommand(SEPS114A_ROW_SCAN_DIRECTION, 0x00);             // Column : 0 --> Max, Row : 0 --> Max --- was 0x00
			/* Set row scan mode */
			SendCommand(SEPS114A_ROW_SCAN_MODE, 0x00);                  // Alternate scan mode --- was 0x00
			/* Set column current */
			SendCommand(SEPS114A_COLUMN_CURRENT_R, 0x6E);
			SendCommand(SEPS114A_COLUMN_CURRENT_G, 0x4F);
			SendCommand(SEPS114A_COLUMN_CURRENT_B, 0x77);
			/* Set row overlap */
			SendCommand(SEPS114A_ROW_OVERLAP, 0x00);                    // Band gap only
			/* Set discharge time */
			SendCommand(SEPS114A_DISCHARGE_TIME, 0x01);                 // Discharge time : normal discharge
			/* Set peak pulse delay */
			SendCommand(SEPS114A_PEAK_PULSE_DELAY, 0x00);
			/* Set peak pulse width */
			SendCommand(SEPS114A_PEAK_PULSE_WIDTH_R, 0x02);
			SendCommand(SEPS114A_PEAK_PULSE_WIDTH_G, 0x02);
			SendCommand(SEPS114A_PEAK_PULSE_WIDTH_B, 0x02);
			/* Set pre-charge current */
			SendCommand(SEPS114A_PRECHARGE_CURRENT_R, 0x14);
			SendCommand(SEPS114A_PRECHARGE_CURRENT_G, 0x50);
			SendCommand(SEPS114A_PRECHARGE_CURRENT_B, 0x19);
			/* Set row scan on/off  */
			SendCommand(SEPS114A_ROW_SCAN_ON_OFF, 0x00);                // Normal row scan
			/* Set scan off level */
			SendCommand(SEPS114A_SCAN_OFF_LEVEL, 0x04);                 // VCC_C*0.75
			/* Set memory access point */
			SendCommand(SEPS114A_DISPLAYSTART_X, 0x00);
			SendCommand(SEPS114A_DISPLAYSTART_Y, 0x00);

			/* Clear the display */
			if (!OledClearAll()) throw new DeviceInitialisationException("Not enough memory to complete initialization.");
			/* Display ON */
			SendCommand(SEPS114A_DISPLAY_ON_OFF, 0x01);

			_powerMode = PowerModes.On;
			_frameRate = FrameRates.OCS_95Hz;
		}

		// Sends a command to the OLED C Display
		private void SendCommand(Byte reg_index, Byte reg_value)
		{
            lock (_socket.LockSpi)
            {
#if (NANOFRAMEWORK_1_0)
				_dataCommandPin.Write(PinValue.Low);
                _oled.Write(new[] { reg_index });

                _dataCommandPin.Write(PinValue.High);
                _oled.Write(new[] { reg_value });
#else
				_dataCommandPin.Write(GpioPinValue.Low);
                _oled.Write(new[] { reg_index });

                _dataCommandPin.Write(GpioPinValue.High);
                _oled.Write(new[] { reg_value });
#endif

			}
		}

		// Send data  as byte[] to OLED C display
        private void SendData(Byte[] data_value)
		{
			lock (_socket.LockSpi)
			{
#if (NANOFRAMEWORK_1_0)
				_dataCommandPin.Write(PinValue.High);
#else
				_dataCommandPin.Write(GpioPinValue.High);
#endif
				_oled.Write(data_value);
            }
        }

		// Send data as UInt16[] to OLED C display
        private void SendData(UInt16[] data)
		{
#if (NANOFRAMEWORK_1_0)
			_dataCommandPin.Write(PinValue.High);
#else
			_dataCommandPin.Write(GpioPinValue.High);
#endif

			lock (_socket.LockSpi)
            {
                for (Int32 x = 0; x < data.Length; x++)
                {
                    _oled.Write(new[] { (Byte) (data[x] >> 8),  (Byte) (data[x] & 0xFF)});
                }
            }
        }

		// Enable writing data to memory of the OLED C Display
        private void EnableDDRAMAccess()
        {
#if (NANOFRAMEWORK_1_0)
			_dataCommandPin.Write(PinValue.Low);
#else
			_dataCommandPin.Write(GpioPinValue.Low);
#endif

			lock (_socket.LockSpi)
            {
                _oled.Write(new[] { SEPS114A_DDRAM_DATA_ACCESS_PORT });
            }
        }

		// Clears the screen upon reboot or reset.
		private Boolean OledClearAll()
		{
			try
			{
				UInt16[] data = new UInt16[9216];
				SendCommand(SEPS114A_MEMORY_WRITE_READ, 0x02);
				SendCommand(SEPS114A_MEM_X1, 0x00);
				SendCommand(SEPS114A_MEM_X2, 0x5F);
				SendCommand(SEPS114A_MEM_Y1, 0x00);
				SendCommand(SEPS114A_MEM_Y2, 0x5F);
				EnableDDRAMAccess();
				for (Int32 a = 0; a < 9216; a++)
				{
					data[a] = 0x00;
				}
				SendData(data);

			}
			catch (OutOfMemoryException)
			{
				return false;
			}
			return true;
		}

#endregion

	}
}
