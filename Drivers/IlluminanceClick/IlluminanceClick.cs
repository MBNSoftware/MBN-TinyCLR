/*
 * Illuminance Click driver for TinyCLR 2.0
 * 
 * Initial version
 *  
 * Copyright 2020 Mikrobus.Net
 * Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License. You may obtain a copy of the License at
 * http://www.apache.org/licenses/LICENSE-2.0
 * Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, 
 * either express or implied. See the License for the specific language governing permissions and limitations under the License.
 */

// ToDo - Add Methodology and Property/Method to enable/disable Interrupt signaling when light Levels is outside set ranges.
// ToDo - Implement manual integration timePeriod.

#if (NANOFRAMEWORK_1_0)
using System.Device.I2c;
#else
using GHIElectronics.TinyCLR.Devices.I2c;
#endif

using System;
using System.Threading;

namespace MBN.Modules
{
	/// <summary>
	/// Main class for the Illuminance Click driver.
	/// <para>This module is a SPI Device.</para>
	/// <para><b>Pins used :</b> Scl, Sda, Int</para>
	/// <para>The Illuminance Click is a 16-bit digital (I2C) light sensor, with adjustable GainControl and IntegrationTime.</para>
	/// <para><b>With the exception of <see cref="IntegrationTimePeriod.Manual"/> (which is not implemented in this release), the Illuminance Click
	/// needs to be powered up and powered down for each access to the Channel Data. This is done automatically in the driver.</b></para>
	/// </summary>
	/// <example>Example usage:
	/// <code language = "C#">
	/// using System;
	/// using System.Threading;
	/// using MBN;
	/// using MBN.Enums;
	/// using MBN.Modules;
	/// using Microsoft.SPOT;
	///
	/// namespace IlluminanceclickTestApp
	/// {
	/// 	public class Program
	/// 	{
	///
	/// 		private static IlluminanceClick illuminance;
	///
	/// 		public static void Main()
	/// 		{
	/// 			illuminance = new IlluminanceClick(Hardware.SocketFour, IlluminanceClick.I2CAddress.Primary, ClockRatesI2C.Clock400KHz)
	/// 			{
	/// 				IntegrationTime = IlluminanceClick.IntegrationTimePeriod._13MS,
	/// 				Gain = IlluminanceClick.GainControl.Low,
	/// 				AutoGainControl = true
	/// 			};
	///
	/// 			illuminance.Initialize();
	///
	///
	/// 			/* Update these values depending on what you've set above! */
	/// 			Debug.Print("-------------------------------------");
	/// 			Debug.Print("           Gain: " + illuminance.Gain);
	/// 			Debug.Print("AutoGainControl: " + illuminance.AutoGainControl);
	/// 			Debug.Print("IntegrationTime: " + illuminance.IntegrationTime);
	/// 			Debug.Print("         ChipID: " + illuminance.ChipID);
	/// 			Debug.Print("-------------------------------------\n");
	///
	/// 			while (true)
	/// 			{
	///
	/// 				Debug.Print("FullSpectrum Light - " + illuminance.FullSpectrumLight);
	/// 				Debug.Print("Visible Light - " + illuminance.VisibleLight);
	/// 				Debug.Print("Infrared Light - " + illuminance.InfraredLight);
	/// 				Debug.Print("Lux - " +  illuminance.ReadLux() + "\n");
	///
	/// 				UInt16 visible;
	/// 				UInt16 ir;
	/// 				UInt16 fullSpectrum;
	/// 				illuminance.ReadLuminosity(out fullSpectrum, out visible, out ir);
	///
	/// 				Debug.Print("-------------------------------------------");
	/// 				Debug.Print("The following readings with AutoGainControl");
	/// 				Debug.Print("-------------------------------------------");
	/// 				Debug.Print("AutoGain FullSpectrum Light - " + fullSpectrum);
	/// 				Debug.Print("     AutoGain Visible Light - " + visible);
	/// 				Debug.Print("    AutoGain Infrared Light - " + ir + "\n");
	///
	/// 				Thread.Sleep(1000);
	///
	/// 			}
	/// 		}
	/// 	}
	/// }
	/// </code>
	/// <code language = "VB">
	/// '' ''Option Explicit On
	/// Option Strict On
	///
	/// Imports System
	/// Imports Microsoft.SPOT
	/// Imports System.Threading
	/// Imports MBN.Modules
	/// Imports MBN
	///
	/// Namespace Examples
	///
	/// 	Public Module Module1
	///
	/// 		Private illuminance As IlluminanceClick
	///
	/// 		Sub Main()
	///
	/// 			Debug.Print(Resources.GetString(Resources.StringResources.String1))
	///
	/// 			illuminance = New IlluminanceClick(Hardware.SocketOne, IlluminanceClick.I2CAddress.Primary, Enums.ClockRatesI2C.Clock400KHz, 100)
	///
	/// 			With illuminance
	/// 				.IntegrationTime = IlluminanceClick.IntegrationTimePeriod._13MS
	/// 				.Gain = IlluminanceClick.GainControl.Low
	/// 				.AutoGainControl = True
	/// 			End With
	///
	/// 			illuminance.Initialize()
	///
	/// 			' Update these values depending on what you've set above!
	/// 			Debug.Print("-------------------------------------")
	/// 			Debug.Print("           Gain: "   <![CDATA[&]]> illuminance.Gain)
	/// 			Debug.Print("AutoGainControl: "   <![CDATA[&]]> illuminance.AutoGainControl)
	/// 			Debug.Print("IntegrationTime: "   <![CDATA[&]]> illuminance.IntegrationTime)
	/// 			Debug.Print("         ChipID: "   <![CDATA[&]]> illuminance.ChipID)
	/// 			Debug.Print("-------------------------------------"   <![CDATA[&]]> Microsoft.VisualBasic.Constants.vbCrLf)
	///
	/// 			While (True)
	/// 				Debug.Print("FullSpectrum Light - "   <![CDATA[&]]> illuminance.FullSpectrumLight)
	/// 				Debug.Print("Visible Light - "   <![CDATA[&]]> illuminance.VisibleLight)
	/// 				Debug.Print("Infrared Light - "   <![CDATA[&]]> illuminance.InfraredLight)
	/// 				Debug.Print("Lux - "   <![CDATA[&]]> illuminance.ReadLux()   <![CDATA[&]]> Microsoft.VisualBasic.Constants.vbCrLf)
	///
	/// 				Dim visible As UInt16
	/// 				Dim ir As UInt16
	/// 				Dim fullSpectrum As UInt16
	///
	/// 				illuminance.ReadLuminosity(fullSpectrum, visible, ir)
	///
	/// 				Debug.Print("-------------------------------------------")
	/// 				Debug.Print("The following readings with AutoGainControl")
	/// 				Debug.Print("-------------------------------------------")
	/// 				Debug.Print("AutoGain FullSpectrum Light - "   <![CDATA[&]]> fullSpectrum)
	/// 				Debug.Print("     AutoGain Visible Light - "   <![CDATA[&]]> visible)
	/// 				Debug.Print("    AutoGain Infrared Light - "   <![CDATA[&]]> ir   <![CDATA[&]]> Microsoft.VisualBasic.Constants.vbCrLf)
	///
	/// 				Thread.Sleep(1000)
	/// 				End While
	///
	/// 		End Sub
	///
	/// 	End Module
	///
	/// End Namespace
	/// </code>
	/// </example>
	public sealed class IlluminanceClick
	{
		#region CTOR

		/// <summary>
		/// Initializes a new instance of the <see cref="IlluminanceClick"/> class.
		/// </summary>
		/// <param name="socket">The socket on which the Illuminance Click module is plugged on MikroBus.Net board</param>
		/// <param name="slaveAddress">The user selectable I2C Address of the module based on Logic Level of Jumper J1. See <see cref="I2CAddress"/> for more information.</param>
		public IlluminanceClick(Hardware.Socket socket, I2CAddress slaveAddress)
		{
			_socket = socket;
#if (NANOFRAMEWORK_1_0)
			_illuminance = I2cDevice.Create(new I2cConnectionSettings(socket.I2cBus, (int)slaveAddress, I2cBusSpeed.FastMode));
#else
			_illuminance = I2cController.FromName(socket.I2cBus).GetDevice(new I2cConnectionSettings((Int32) slaveAddress, 4000000));
#endif
			if (!Init()) throw new DeviceInitialisationException($"Error initializing device at address 0x{slaveAddress:x2}");
		}

#endregion

#region Fields

		private IntegrationTimePeriod _integrationTime = IntegrationTimePeriod._402MS;
		private GainControl _gainControl = GainControl.Low;
		private UInt16 _channel0;
		private UInt16 _channel1;
		private readonly Hardware.Socket _socket;

		//I2C
		private readonly I2cDevice _illuminance;       // I²C configuration

#endregion

#region Constants

		// Used to Calculate Lux
		private const Byte LUX_LUXSCALE = 14; // scale by 2^14
		private const Byte LUX_RATIOSCALE = 9; // scale ratio by 2^9

		// Integration time scaling factors
		private const Byte LUX_CHSCALE = 10; // scale channel values by 2^10
		private const UInt16 LUX_CHSCALE_TINT0 = 0x7517; // 322/11 * 2^CH_SCALE
		private const UInt16 LUX_CHSCALE_TINT1 = 0x0fe7; // 322/81 * 2^CH_SCALE

		// Coefficients used for LUX calculation
		private const UInt16 K1T = 0x0040; // 0.125 * 2^RATIO_SCALE
		private const UInt16 B1T = 0x01f2; // 0.0304 * 2^LUX_SCALE
		private const UInt16 M1T = 0x01be; // 0.0272 * 2^LUX_SCALE

		private const UInt16 K2T = 0x0080; // 0.250 * 2^RATIO_SCALE
		private const UInt16 B2T = 0x0214; // 0.0325 * 2^LUX_SCALE
		private const UInt16 M2T = 0x02d1; // 0.0440 * 2^LUX_SCALE

		private const UInt16 K3T = 0x00c0; // 0.375 * 2^RATIO_SCALE
		private const UInt16 B3T = 0x023f; // 0.0351 * 2^LUX_SCALE
		private const UInt16 M3T = 0x037b; // 0.0544 * 2^LUX_SCALE

		private const UInt16 K4T = 0x0100; // 0.50 * 2^RATIO_SCALE
		private const UInt16 B4T = 0x0270; // 0.0381 * 2^LUX_SCALE
		private const UInt16 M4T = 0x03fe; // 0.0624 * 2^LUX_SCALE

		private const UInt16 K5T = 0x0138; // 0.61 * 2^RATIO_SCALE
		private const UInt16 B5T = 0x016f; // 0.0224 * 2^LUX_SCALE
		private const UInt16 M5T = 0x01fc; // 0.0310 * 2^LUX_SCALE

		private const UInt16 K6T = 0x019a; // 0.80 * 2^RATIO_SCALE
		private const UInt16 B6T = 0x00d2; // 0.0128 * 2^LUX_SCALE
		private const UInt16 M6T = 0x00fb; // 0.0153 * 2^LUX_SCALE

		private const UInt16 K7T = 0x029a; // 1.3 * 2^RATIO_SCALE
		private const UInt16 B7T = 0x0018; // 0.00146 * 2^LUX_SCALE
		private const UInt16 M7T = 0x0012; // 0.00112 * 2^LUX_SCALE

		private const UInt16 K8T = 0x029a; // 1.3 * 2^RATIO_SCALE
		private const UInt16 B8T = 0x0000; // 0.000 * 2^LUX_SCALE
		private const UInt16 M8T = 0x0000; // 0.000 * 2^LUX_SCALE

		// Auto-GainControl thresholds
		//private const UInt16 AGC_THI_13MS = 4850;
		//private const UInt16 AGC_TLO_13MS = 100;
		//private const UInt16 AGC_THI_101MS = 36000;
		//private const UInt16 AGC_TLO_101MS = 200;
		//private const UInt16 AGC_THI_402MS = 63000;
		//private const UInt16 AGC_TLO_402MS = 500;

		// Clipping thresholds used for LUX calculation
		private const UInt16 CLIPPING_13MS = 4900;
		private const UInt16 CLIPPING_101MS = 37000;
		private const UInt16 CLIPPING_402MS = 65000;

#endregion

#region ENUMS

#region Private ENUMS

		private enum AddressRegisters
		{
			Control = 0x00,			// Control of basic function
			Timing = 0x01,			// IntegrationTime/Gain Control
			//Treshlowlow = 0x02,		// Low byte of Low Interrupt threshold
			//Treshlowhigh = 0x03,	// High byte of Low Interrupt threshold
			//Treshhighlow = 0x04,	// Low byte of High Interrupt threshold
			//Treshhighhigh = 0x05,	// High byte of High Interrupt threshold
			//Interrupt = 0x06,		// Interrupt Control
			Id = 0x0A,				// Part number / Rev Id
			Data0low = 0x0C,		// Low byte of ADC channel 0
			//Data0high = 0x0D,		// High byte of ADC channel 0
			Data1low = 0x0E,		// Low byte of ADC channel 1
			//Data1high = 0x0F		// High byte of ADC channel 1
		}

		private enum Channels
		{
			Channel0,
			Channel1,
			TwoChannels
		}

		private enum CommandByte
		{
			//BlockBit = 0x10,
			WordBit = 0x20,
			//ClearBit = 0x40,
			CommandBit = 0x80
		}

		private enum ControlByte
		{
			PowerOff = 0x00,
			PowerOn = 0x03
		}

#endregion

#region Public ENUMS

		/// <summary>
		/// Hardware based I2C Address Selection (J1)
		/// </summary>
		public enum I2CAddress
        {
			/// <summary>
			/// I2C Address Selection Jumper (J1) in Logic Level 1 (factory default).
			/// </summary>
			Primary = 0x49,
			/// <summary>
			/// I2C Address Selection Jumper (J1) in Logic Level 0.
			/// </summary>
			Secondary = 0x29,
			/// <summary>
			/// I2C Address Selection Jumper (J1) removed.
			/// </summary>
			Tertiary = 0x39
		}

		/// <summary>
		/// IntegrationTime Period
		/// </summary>
		/// <remarks>A higher IntegrationTime essentially increases the resolution of the device, since the analog converter inside the chip has a longer timePeriod to take more samples.</remarks>
		public enum IntegrationTimePeriod
		{
			/// <summary>
			/// Fast but low resolution
			/// </summary>
			_13MS,
			/// <summary>
			/// Medium resolution and speed
			/// </summary>
			_101MS,
			/// <summary>
			/// 16-bit data but slowest conversions
			/// </summary>
			_402MS,
			/// <summary>
			/// Manual integration timePeriod. Currently not implemented in this release.
			/// </summary>
			Manual
		}

		/// <summary>
		/// Gain Control used to adjust sensitivity.
		/// </summary>
		public enum GainControl
		{
			/// <summary>
			/// Use in bright light to avoid sensor saturation
			/// </summary>
			Low = 0,
			/// <summary>
			/// Use in low light to boost sensitivity
			/// </summary>
			High = 1
		}

#endregion

#endregion

#region Private Methods

		private Boolean Init()
		{
			var data = GetRegister(AddressRegisters.Id);
			return (data & 0x0A) == 0;
		}

		private void Enable() => SetRegister(AddressRegisters.Control, (Byte)ControlByte.PowerOn);

		private void Disable() => SetRegister(AddressRegisters.Control, (Byte)ControlByte.PowerOff);

		private void SetRegister(AddressRegisters address, Byte value)
		{
			var command = (Byte)((Byte)CommandByte.CommandBit | (Byte)address);
			Byte[] writeBuffer = { command, value };

			lock (_socket.LockI2c)
			{
				_illuminance.Write(writeBuffer);
			}
		}

		private Byte GetRegister(AddressRegisters address)
		{
			var command = (Byte)((Byte)CommandByte.CommandBit | (Byte)address);
			var writeBuffer = new[] { command };
			var readBuffer = new Byte[1];

			lock (_socket.LockI2c)
			{
				_illuminance.WriteRead(writeBuffer, readBuffer);
			}

			return readBuffer[0];
		}

		private UInt16 GetWord(AddressRegisters address)
		{
			var command = (Byte)((Byte)CommandByte.CommandBit | (Byte)CommandByte.WordBit | (Byte)address);
			var writeBuffer = new[] { command };
			var readBuffer = new Byte[2];

			lock (_socket.LockI2c)
			{
				_illuminance.WriteRead(writeBuffer, readBuffer);
			}

			return (UInt16)((readBuffer[1] * 256) | (readBuffer[0]));
		}

		private UInt16 GetChannelData(Channels channel)
		{
			UInt16 value = 0;

			Enable();

			// Wait x ms for ADC to complete
			switch (_integrationTime)
			{
				case IntegrationTimePeriod._13MS:
				{
					Thread.Sleep(14);
					break;
				}
				case IntegrationTimePeriod._101MS:
				{
					Thread.Sleep(102);
					break;
				}
				case IntegrationTimePeriod._402MS:
				{
					Thread.Sleep(403);
					break;
				}
			}

			switch (channel)
			{
				case Channels.Channel0:
				{
					// Read a two byte value from channel 0 (fullSpectrum + infrared)
					value = GetWord(AddressRegisters.Data0low);
					break;
				}
				case Channels.Channel1:
				{
					// Read a two byte value from channel 1 (infrared)
					value = GetWord(AddressRegisters.Data1low);
					break;
				}
				case Channels.TwoChannels:
				{
					_channel0 = GetWord(AddressRegisters.Data0low);
					_channel1 = GetWord(AddressRegisters.Data1low);
					break;
				}
			}
			Disable();
			return value;
		}

#endregion

#region Public Properties

		/// <summary>
		/// Gets or sets the power mode of the Illuminance Click.
		/// </summary>
		/// <value>
		/// The current power mode of the module.
		/// </value>
		/// <example>Example usage:
		/// <code language="C#">
		/// // None provided as this module does not support PowerModes.
		/// </code>
		/// <code language="VB">
		/// ' None provided as this module does not support PowerModes.
		/// </code>
		/// </example>
		/// <exception cref="System.NotSupportedException">A NotSupportedException will be thrown if setting this property as this module does not support user setting of the PowerMode.</exception>
		public PowerModes PowerMode
		{
			get
			{
				var power = GetRegister((Byte)AddressRegisters.Control);
				return Bits.IsBitSet(power, 0) && Bits.IsBitSet(power, 1) ? PowerModes.On : PowerModes.Off;
			}
			set
			{
				throw new NotImplementedException("This module does not support the user changing the PowerMode as it is done automatically by the driver.");
			}
		}

		/// <summary>
		/// Gets or sets the AutoGain Control of the Illuminance Click.
		/// </summary>
		/// <remarks>AutoGainControl only works with the <see cref="ReadLuminosity"/> method. Set this property to true to prevent sensor saturation.</remarks>
		/// <example>Example usage:
		/// <code language = "C#">
		/// Debug.Print("AutoGainControl - before: " + illuminance.AutoGainControl);
		/// illuminance.AutoGainControl = true;
		/// Debug.Print("AutoGainControl - after: " + illuminance.AutoGainControl);
		/// </code>
		/// <code language = "VB">
		/// Debug.Print("AutoGainControl - before: " <![CDATA[&]]> illuminance.AutoGainControl)
		/// illuminance.AutoGainControl = true
		/// Debug.Print("AutoGainControl - after: " <![CDATA[&]]> illuminance.AutoGainControl)
		/// </code>
		/// </example>
		public Boolean AutoGainControl { get; set; } = true;

		/// <summary>
		/// Gets or sets the Gain of the Illuminance Click (sensitivity to light)
		/// </summary>
		/// <remarks>Use <see cref="GainControl.Low"/> for intense lighting situations (outdoors) to avoid sensor saturation or <see cref="GainControl.High"/> for low lighting situations (indoors).</remarks>
		/// <example>Example usage:
		/// <code language = "C#">
		/// Debug.Print("Gain - before: " + illuminance.Gain);
		/// illuminance.Gain = IlluminanceClick.GainControl.x16;
		/// Debug.Print("Gain - after: " + illuminance.Gain);
		/// </code>
		/// <code language = "VB">
		/// Debug.Print("Gain - before: " <![CDATA[&]]> illuminance.Gain)
		/// illuminance.Gain = IlluminanceClick.GainControl.x16
		/// Debug.Print("Gain - after: " <![CDATA[&]]> illuminance.Gain)
		/// </code>
		/// </example>
		public GainControl Gain
		{
			get
			{
				var timing = GetRegister(AddressRegisters.Timing);

				return Bits.IsBitSet(timing, 4) ? GainControl.High : GainControl.Low;
			}
			set
			{
				_gainControl = value;

				var timing = GetRegister(AddressRegisters.Timing);
				timing = Bits.Set(timing, 4, value == GainControl.High);
				SetRegister(AddressRegisters.Timing, timing);
			}
		}

		/// <summary>
		/// Gets or sets the IntegrationTime for the Illuminance Click
		/// </summary>
		/// <remarks>Adjusting the <see cref="IntegrationTimePeriod"/> essentially increases the resolution of the device, since the analog converter inside the chip has a longer timePeriod to take more samples.</remarks>
		/// <exception cref="NotImplementedException">A NotImplementedException will be thrown if attempting to set the IntegrationTime to Manual as it is currently not supported in this release.</exception>
		/// <example>Example usage:
		/// <code language = "C#">
		/// Debug.Print("IntegrationTime - before: " + illuminance.IntegrationTime);
		/// illuminance.IntegrationTime = IlluminanceClick.IntegrationTimePeriod._101MS;
		/// Debug.Print("IntegrationTime - after: " + illuminance.IntegrationTime);
		/// </code>
		/// <code language = "VB">
		/// Debug.Print("IntegrationTime - before: " <![CDATA[&]]> illuminance.IntegrationTime)
		/// illuminance.IntegrationTime = IlluminanceClick.IntegrationTimePeriod._101MS
		/// Debug.Print("IntegrationTime - after: " <![CDATA[&]]> illuminance.IntegrationTime)
		/// </code>
		/// </example>
		public IntegrationTimePeriod IntegrationTime
		{
			get
			{
				var timing = GetRegister(AddressRegisters.Timing);

				return !Bits.IsBitSet(timing, 0) && !Bits.IsBitSet(timing, 1)
					? IntegrationTimePeriod._13MS
					: Bits.IsBitSet(timing, 0) && !Bits.IsBitSet(timing, 1)
					? IntegrationTimePeriod._101MS
					: !Bits.IsBitSet(timing, 0) && Bits.IsBitSet(timing, 1) ? IntegrationTimePeriod._402MS : IntegrationTimePeriod.Manual;
			}
			set
			{
				if (value == IntegrationTimePeriod.Manual) throw new NotImplementedException("Manual IntegrationTime is currently not supported.");

				_integrationTime = value;

				var timing = GetRegister(AddressRegisters.Timing);

				switch (value)
				{
					case IntegrationTimePeriod._13MS:
						timing = Bits.Set(timing, 0, false);
						timing = Bits.Set(timing, 1, false);
						break;
					case IntegrationTimePeriod._101MS:
						timing = Bits.Set(timing, 0, true);
						timing = Bits.Set(timing, 1, false);
						break;
					case IntegrationTimePeriod._402MS:
						timing = Bits.Set(timing, 0, false);
						timing = Bits.Set(timing, 1, true);
						break;
				}
				SetRegister(AddressRegisters.Timing, timing);
			}
		}

		/// <summary>
		/// Gets the ChipID and Silicone Revision.
		/// </summary>
		/// <example>Example usage:
		/// <code language = "C#">
		/// Debug.Print("ChipID: " + illuminance.ChipID);
		/// </code>
		/// <code language = "VB">
		/// Debug.Print("ChipID: " <![CDATA[&]]> illuminance.ChipID)
		/// </code>
		/// </example>
		public String ChipID
		{
			get
			{
				Enable();
				var chipId = GetRegister(AddressRegisters.Id);
				Disable();

				return (chipId >> 4) == 5
					? "TSL2561T/FN/CL : Revision " + (chipId & 0x0F)
					: (chipId >> 4) == 4
					? "TSL2560T/FN/CL : Revision " + (chipId & 0x0F)
					: (chipId >> 4) == 1
					? "TSL2561CS : Revision " + (chipId & 0x0F)
					: (chipId >> 4) == 0 ? "TSL2560CS : Revision " + (chipId & 0x0F) : "Unknown ChipId";
			}
		}

		/* - Reserved for future use when Interrupt signaling is implemented

		/// <summary>
		/// Gets or sets the LowLow Threshold Register
		/// </summary>
		public Byte ThresholdLowLow
		{
			get
			{
				Enable();
				var lowLow = GetRegister(AddressRegisters.Treshlowlow);
				PowerMode = PowerModes.Off
				return lowLow;
			}
			set
			{
				SetRegister(AddressRegisters.Treshlowlow, value);
			}
		}

		/// <summary>
		/// Gets or sets the LowHigh Threshold Register
		/// </summary>
		public Byte ThresholdLowHigh
		{
			get
			{
				Enable();
				var lowhigh = GetRegister(AddressRegisters.Treshlowhigh);
				PowerMode = PowerModes.Off
				return lowhigh;
			}
			set
			{
				SetRegister(AddressRegisters.Treshlowhigh, value);
			}
		}

		/// <summary>
		/// Gets or sets the HighLow Register.
		/// </summary>
		public Byte ThresholdHighLow
		{
			get
			{
				Enable();
				var highLow = GetRegister(AddressRegisters.Treshhighlow);
				PowerMode = PowerModes.Off
				return highLow;
			}
			set
			{
				SetRegister(AddressRegisters.Treshhighlow, value);
			}
		}

		/// <summary>
		/// Gets or sets the HighHigh Threshold Register.
		/// </summary>
		public Byte ThresholdHighHigh
		{
			get
			{
				Enable();
				var highHigh = GetRegister(AddressRegisters.Treshhighhigh);
				PowerMode = PowerModes.Off
				return highHigh;
			}
			set
			{
				SetRegister(AddressRegisters.Treshhighhigh, value);
			}
		}

		/// <summary>
		/// Gets the Interrupt Register.
		/// </summary>
		public Byte Interrupt
		{
			get
			{
				Enable();
				var interrupt = GetRegister(AddressRegisters.Interrupt);
				PowerMode = PowerModes.Off
				return interrupt;
			}
		}

 		/// <summary>
		/// Gets the Channel0LowByte Register (VisibleLight and Ir)
		/// </summary>
		public Byte Channel0LowByte
		{
			get
			{
				var register = GetRegister(AddressRegisters.Data0low);
				return register;
			}
		}

		/// <summary>
		/// Gets the Channel0HighByte Register (VisibleLight and Ir)
		/// </summary>
		public Byte Channel0HighByte
		{
			get
			{
				var register = GetRegister(AddressRegisters.Data0high);
				return register;
			}
		}

		/// <summary>
		/// Gets the Channel1LowByte Register (Ir only)
		/// </summary>
		public Byte Channel1LowByte
		{
			get
			{
				byte register = GetRegister(AddressRegisters.Data1low);
				return register;
			}
		}

    	/// <summary>
		/// Gets the Channel1HighByte Register (Ir only)
		/// </summary>
		public Byte Channel1HighByte
		{
			get
			{
				var register = GetRegister(AddressRegisters.Data1high);
				return register;
			}
		}
		*/

		/// <summary>
		/// Gets the FullSpectrum Light from the Illuminance Click (Visible Light and Ir combined)
		/// </summary>
		/// <remarks>
		/// Sensor output resolution is dependent on the <see cref="IntegrationTime"/> settings.
		/// For <see cref="IntegrationTimePeriod._13MS"/>, the maximum output of the sensor is 5047 per Channel,
		/// for <see cref="IntegrationTimePeriod._101MS"/>, the maximum output of the sensor is 37177 per Channel
		/// and for <see cref="IntegrationTimePeriod._13MS"/>, the maximum output of the sensor is 65535 per Channel.
		/// You can use these values to calculate a percentage (%) of light against full scale.
		/// </remarks>
		/// <example>Example usage:
		/// <code language = "C#">
		/// Debug.Print("FullSpectrum Light - " + illuminance.FullSpectrumLight.ToString());
		/// </code>
		/// <code language = "VB">
		/// Debug.Print("FullSpectrum Light - " <![CDATA[&]]> illuminance.FullSpectrumLight.ToString())
		/// </code>
		/// </example>
		public UInt16 FullSpectrumLight
		{
			get
			{
				var channelData = GetChannelData(Channels.Channel0);
				return channelData;
			}
		}

		/// <summary>
		/// Gets the Visible Light from the Illuminance Click
		/// <para><b>This is a calculated values as the Illuminance click doe not directly measure Visible Light.</b></para>
		/// </summary>
		/// <remarks>
		/// Sensor output resolution is dependent on the <see cref="IntegrationTime"/> settings.
		/// For <see cref="IntegrationTimePeriod._13MS"/>, the maximum output of the sensor is 5047 per Channel,
		/// for <see cref="IntegrationTimePeriod._101MS"/>, the maximum output of the sensor is 37177 per Channel
		/// and for <see cref="IntegrationTimePeriod._13MS"/>, the maximum output of the sensor is 65535 per Channel.
		/// You can use these values to calculate a percentage (%) of light against full scale.
		/// </remarks>
		/// <example>Example usage:
		/// <code language = "C#">
		/// Debug.Print("Visible Light - " + illuminance.VisibleLight.ToString())
		/// </code>
		/// <code language = "VB">
		/// Debug.Print("Visible Light - " <![CDATA[&]]> illuminance.VisibleLight.ToString())
		/// </code>
		/// </example>
		public UInt16 VisibleLight
		{
			get
			{
				return (UInt16) (FullSpectrumLight - InfraredLight);
			}
		}

		/// <summary>
		/// Gets the Infrared Light from the Illuminance click (InfraRed only)
		/// </summary>
		/// <remarks>
		/// Sensor output resolution is dependent on the <see cref="IntegrationTime"/> settings.
		/// For <see cref="IntegrationTimePeriod._13MS"/>, the maximum output of the sensor is 5047 per Channel,
		/// for <see cref="IntegrationTimePeriod._101MS"/>, the maximum output of the sensor is 37177 per Channel
		/// and for <see cref="IntegrationTimePeriod._13MS"/>, the maximum output of the sensor is 65535 per Channel.
		/// You can use these values to calculate a percentage (%) of light against full scale.
		/// </remarks>
		/// <example>Example usage:
		/// <code language = "C#">
		/// Debug.Print("Infrared Light - " + illuminance.InfraredLight.ToString())
		/// </code>
		/// <code language = "VB">
		/// Debug.Print("Infrared Light - " <![CDATA[&]]> illuminance.InfraredLight.ToString())
		/// </code>
		/// </example>
		public UInt16 InfraredLight
		{
			get
			{
				var channelData = GetChannelData(Channels.Channel1);
				return channelData;
			}
		}

#endregion

#region Public Methods

		/// <summary>
		/// Initializes the Illuminance Click with the default values.
		/// <para>Powers Up, Sets GainControl to 1X and IntegrationTimePeriod to 402 MS, Disables IRQ Signaling and then powers the Illuminance Click down to conserve power.</para>
		/// </summary>
		/// <remarks>If you set the <see cref="Gain"/> and <see cref="IntegrationTime"/> properties, the values from the properties will be used instead of the default values.</remarks>
		/// <example>Example usage:
		/// <code language = "C#">
		/// illuminance.Initialize();
		/// </code>
		/// <code language = "VB">
		/// illuminance.Initialize()
		/// </code>
		/// </example>
		public void Initialize() => Initialize(_gainControl, _integrationTime);

		/// <summary>
		/// Initializes the Illuminance Click with user defined Gain and IntegrationTime.
		/// </summary>
		/// <param name="gain">The user defined Gain. See <see cref="GainControl"/> for more information.</param>
		/// <param name="intergrationTime">The IntegrationTimePeriod. See <see cref="IntegrationTimePeriod"/> for more information.</param>
		/// <example>Example usage:
		/// <code language = "C#">
		/// illuminance.Initialize(IlluminanceClick.GainControl.x1, IlluminanceClick.IntegrationTimePeriod._101MS);
		/// </code>
		/// <code language = "VB">
		/// illuminance.Initialize(IlluminanceClick.GainControl.x1, IlluminanceClick.IntegrationTimePeriod._101MS)
		/// </code>
		/// </example>
		public void Initialize(GainControl gain, IntegrationTimePeriod intergrationTime)
        {
			Gain = gain;
			IntegrationTime = intergrationTime;
        }

		/// <summary>
		/// Gets the fullSpectrum (mixed lighting) and IR only values from the Illuminance Click, adjusting GainControl if the <see cref="AutoGainControl"/> property is enabled.
		/// </summary>
		/// <param name="fullSpectrum">The reference to the <see cref="UInt16"/> to hold the value of the FullsSectrum Light (Mixed Lighting - Visible and Infrared combined).</param>
		/// <param name="visible">The reference to the <see cref="UInt16"/> to hold the value of the Visible Light (Visible only).</param>
		/// <param name="ir">The reference to the <see cref="UInt16"/> to hold the value of the Infrared Light (Infrared only).</param>
		/// <example>Example usage:
		/// <code language = "C#">
		/// UInt16 visible;
		///	UInt16 ir;
		///	UInt16 fullSpectrum;
		/// 
		///	illuminance.ReadLuminosity(out fullSpectrum, out visible, out ir);
		///
		///	Debug.Print("FullSpectrum Light - " + fullSpectrum);
		///	Debug.Print("Visible Light - " + visible);
		///	Debug.Print("Infrared Light - " + ir + "\n");
		/// </code>
		/// <code language = "VB">
		/// Dim visible As UInt16
		/// Dim ir As UInt16
		/// Dim fullSpectrum As UInt16
		/// 
		/// illuminance.ReadLuminosity(fullSpectrum, visible, ir)
		///
		/// Debug.Print("FullSpectrum Light - " <![CDATA[&]]> fullSpectrum)
		/// Debug.Print("Visible Light - " <![CDATA[&]]> visible)
		/// Debug.Print("Infrared Light - " <![CDATA[&]]> ir)
		/// </code>
		/// </example>
		public void ReadLuminosity(out UInt16 fullSpectrum, out UInt16 visible,  out UInt16 ir)
		{
			var valid = false;
			UInt16 _b;
			UInt16 _ir;
			IntegrationTimePeriod _it = _integrationTime;
			var _agcCheck = false;

			if (!AutoGainControl)
			{
				fullSpectrum = GetChannelData(Channels.Channel0);
				ir = GetChannelData(Channels.Channel1);
				visible = (UInt16) (fullSpectrum - ir);
				return;
			}
			GainControl oldGain = Gain;
			IntegrationTimePeriod oldIntegrationTime = IntegrationTime;

			do
			{
				UInt16 _hi;
				UInt16 _lo;

				switch (_it)
				{
					case IntegrationTimePeriod._13MS:
					{
						_hi = (UInt16)IntegrationTimePeriod._13MS;
						_lo = (UInt16)IntegrationTimePeriod._13MS;
						break;
					}
					case IntegrationTimePeriod._101MS:
					{
						_hi = (UInt16)IntegrationTimePeriod._101MS;
						_lo = (UInt16)IntegrationTimePeriod._101MS;
						break;
					}
					default:
					{
						_hi = (UInt16)IntegrationTimePeriod._402MS;
						_lo = (UInt16)IntegrationTimePeriod._402MS;
						break;
					}
				}

				_b = GetChannelData(Channels.Channel0);

				/* Run an auto-GainControl check if we haven't already done so ... */
				if (!_agcCheck)
				{
					if ((_b < _lo) && (_gainControl == GainControl.Low))
					{
						/* Increase the GainControl and try again */
						Gain = GainControl.High;
						/* Drop the previous conversion results */
						_b = GetChannelData(Channels.Channel0);
						_ir = GetChannelData(Channels.Channel1);
						/* Set a flag to indicate we've adjusted the GainControl */
						_agcCheck = true;
					}
					else if ((_b > _hi) && (_gainControl == GainControl.High))
					{
						/* Drop GainControl to 1x and try again */
						Gain = GainControl.Low;
						/* Drop the previous conversion results */
						_b = GetChannelData(Channels.Channel0);
						_ir = GetChannelData(Channels.Channel1);
						/* Set a flag to indicate we've adjusted the GainControl */
						_agcCheck = true;
					}
					else
					{
						/* Nothing to look at here, keep moving ....
						   Reading is either valid, or we're already at the chips limits */
						_b = GetChannelData(Channels.Channel0);
						_ir = GetChannelData(Channels.Channel1);
						valid = true;
					}
				}
				else
				{
					/* If we've already adjusted the GainControl once, just return the new results.
					   This avoids endless loops where a value is at one extreme pre-GainControl,
					   and the other extreme post-GainControl */
					_b = GetChannelData(Channels.Channel0);
					_ir = GetChannelData(Channels.Channel1);
					valid = true;
				}
			} while (!valid);

			fullSpectrum = _b;
			ir = _ir;
			visible = (UInt16) (_b - ir);

			Gain = oldGain;
			IntegrationTime = oldIntegrationTime;
		}

		/// <summary>
		/// Returns the calculated LUX as read from the Illuminance Click.
		/// </summary>
		/// <returns>The calculated LUX or 65535 if the sensor is saturated.</returns>
		/// <example>Example usage:
		/// <code language = "C#">
		/// Debug.Print("LUX - " + illuminance.ReadLux());
		/// </code>
		/// <code language = "VB">
		/// Debug.Print("LUX - " + illuminance.ReadLux())
		/// </code>
		/// </example>
		public UInt32 ReadLux()
		{
			UInt64 chScale;

			// Read Channels
			GetChannelData(Channels.TwoChannels);

			// Make sure the sensor isn't saturated
			UInt16 clipThreshold;

			switch (_integrationTime)
			{
				case IntegrationTimePeriod._13MS:
					{
						clipThreshold = CLIPPING_13MS;
						break;
					}
				case IntegrationTimePeriod._101MS:
					{
						clipThreshold = CLIPPING_101MS;
						break;
					}
				default:
					{
						clipThreshold = CLIPPING_402MS;
						break;
					}
			}

			// return max Lux if the sensor is saturated
			if ((_channel0 > clipThreshold) || (_channel1 > clipThreshold))
			{
				return UInt16.MaxValue;
			}

			// Get the correct scale depending on the integration timePeriod
			switch (_integrationTime)
			{
				case IntegrationTimePeriod._13MS:
					{
						chScale = LUX_CHSCALE_TINT0;
						break;
					}
				case IntegrationTimePeriod._101MS:
					{
						chScale = LUX_CHSCALE_TINT1;
						break;
					}
				default:
					{
						// No scaling ... integration timePeriod = 402ms
						chScale = 1 << LUX_CHSCALE;
						break;
					}
			}

			// Scale for GainControl (x1 or x16)
			if ((Byte)_gainControl != 16)
			{
				chScale <<= 4;
			}

			// Scale the channel value
			var CH0 = (_channel0 * chScale) >> LUX_CHSCALE;
			var CH1 = (_channel1 * chScale) >> LUX_CHSCALE;

			// Find the ratio of the channel values (_channel1/_channel0)
			UInt64 ratio1 = 0;
			if (CH0 != 0)
			{
				ratio1 = (CH1 << (LUX_RATIOSCALE + 1)) / CH0;
			}

			// round the ratio value
			var ratio = (ratio1 + 1) >> 1;

			UInt32 b = 0, m = 0;

			//if ((ratio >= 0) && (ratio <= K1T))
			if (ratio <= K1T)
			{
				b = B1T; m = M1T;
			}
			else if (ratio <= K2T)
			{
				b = B2T; m = M2T;
			}
			else if (ratio <= K3T)
			{
				b = B3T; m = M3T;
			}
			else if (ratio <= K4T)
			{
				b = B4T; m = M4T;
			}
			else if (ratio <= K5T)
			{
				b = B5T; m = M5T;
			}
			else if (ratio <= K6T)
			{
				b = B6T; m = M6T;
			}
			else if (ratio <= K7T)
			{
				b = B7T; m = M7T;
			}
			else if (ratio > K8T)
			{
				b = B8T; m = M8T;
			}

			var temp = (CH0 * b) - (CH1 * m);

			temp += 1 << (LUX_LUXSCALE - 1);

			// strip off fractional portion and return LUX
			return (UInt32)(temp >> LUX_LUXSCALE);
		}
#endregion
	}
}

