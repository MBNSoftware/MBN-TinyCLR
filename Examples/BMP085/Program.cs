using MBN;
using MBN.Modules;

using System.Diagnostics;
using System.Threading;

namespace Examples
{
    internal static class Program
    {
        private static BMP085 _bmp085;

        private static void Main()
        {
            _bmp085 = new BMP085(Hardware.SC20100_1, Hardware.SC20100_1.Int)
            {
                OverSamplingSetting = BMP085.Oss.UltraHighResolution,
                TemperatureUnit = TemperatureUnits.Celsius
            };

            Debug.WriteLine("BMP085 Demo");
            Debug.WriteLine($"BMP085 Sensor OSS is - {_bmp085.OverSamplingSetting}\n");

            while (true)
            {
                Debug.WriteLine($"Temperature : {_bmp085.ReadTemperature():F2} °C");
                Debug.WriteLine($"   Pressure : {_bmp085.ReadPressure():F1} Pascals");
                Debug.WriteLine($"   Altitude : {_bmp085.ReadAltitude():F0} meters\n");
                Thread.Sleep(2000);
            }
        }
    }
}
