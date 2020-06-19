using MBN;
using MBN.Modules;

using System.Diagnostics;
using System.Threading;

namespace Examples
{
    internal class Program
    {
        private static BMP183 _sensor;

        private static void Main()
        {
            Debug.WriteLine("Program started");

            _sensor = new BMP183(Hardware.SC20100_1)
            {
                OverSamplingSetting = BMP183.Oss.UltraHighResolution,
                TemperatureUnit = TemperatureUnits.Celsius
            };

            Debug.WriteLine("BMP183 Demo");
            Debug.WriteLine("Is a BMP183 connected? " + _sensor.IsConnected());
            Debug.WriteLine($"BMP183 Sensor OSS is - {_sensor.OverSamplingSetting}\n");

            while (true)
            {
                Debug.WriteLine($"Temperature : {_sensor.ReadTemperature():F2} °C");
                Debug.WriteLine($"   Pressure : {_sensor.ReadPressure():F1} Pascals");
                Debug.WriteLine($"   Altitude : {_sensor.ReadAltitude():F0} meters\n");
                Thread.Sleep(2000);
            }
        }
    }
}
