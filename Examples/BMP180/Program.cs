using MBN;
using MBN.Modules;

using System.Diagnostics;
using System.Threading;

namespace Examples
{
    internal class Program
    {
        private static BMP180 _bmp180;

        private static void Main()
        {
            _bmp180 = new BMP180(Hardware.SocketOne)
            {
                OverSamplingSetting = BMP180.Oss.UltraHighResolution,
                TemperatureUnit = TemperatureUnits.Celsius
            };

            Debug.WriteLine("BMP180 Demo");
            Debug.WriteLine("Is a BMP180 connected? " + _bmp180.IsConnected());
            Debug.WriteLine("BMP180 Sensor OSS is - " + _bmp180.OverSamplingSetting + "\n");

            while (true)
            {
                Debug.WriteLine($"Temperature : {_bmp180.ReadTemperature():F2} °C");
                Debug.WriteLine($"   Pressure : {_bmp180.ReadPressure():F1} Pascals");
                Debug.WriteLine($"   Altitude : {_bmp180.ReadAltitude():F0} meters\n");
                Thread.Sleep(2000);
            }
        }
    }
}
