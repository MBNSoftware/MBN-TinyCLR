using MBN;
using MBN.Modules;
using System;
using System.Diagnostics;
using System.Threading;

namespace Examples
{
	class Program
    {
		private static IlluminanceClick illuminance;
		private static UInt16 visible;
		private static UInt16 ir;
		private static UInt16 fullSpectrum;

		public static void Main()
		{
			illuminance = new IlluminanceClick(Hardware.SocketFour, IlluminanceClick.I2CAddress.Primary)
			{
				IntegrationTime = IlluminanceClick.IntegrationTimePeriod._13MS,
				Gain = IlluminanceClick.GainControl.Low,
				AutoGainControl = true
			};

			illuminance.Initialize();

			/* Update these values depending on what you've set above! */
			Debug.WriteLine("-------------------------------------");
			Debug.WriteLine("           Gain: " + illuminance.Gain);
			Debug.WriteLine("AutoGainControl: " + illuminance.AutoGainControl);
			Debug.WriteLine("IntegrationTime: " + illuminance.IntegrationTime);
			Debug.WriteLine("         ChipID: " + illuminance.ChipID);
			Debug.WriteLine("-------------------------------------\n");

			while (true)
			{

				Debug.WriteLine("FullSpectrum Light - " + illuminance.FullSpectrumLight);
				Debug.WriteLine("Visible Light - " + illuminance.VisibleLight);
				Debug.WriteLine("Infrared Light - " + illuminance.InfraredLight);
				Debug.WriteLine("Lux - " + illuminance.ReadLux() + "\n");
				
				illuminance.ReadLuminosity(out fullSpectrum, out visible, out ir);

				Debug.WriteLine("-------------------------------------------");
				Debug.WriteLine("The following readings with AutoGainControl");
				Debug.WriteLine("-------------------------------------------");
				Debug.WriteLine("AutoGain FullSpectrum Light - " + fullSpectrum);
				Debug.WriteLine("     AutoGain Visible Light - " + visible);
				Debug.WriteLine("    AutoGain Infrared Light - " + ir + "\n");

				Thread.Sleep(1000);
			}
		}
	}
}
