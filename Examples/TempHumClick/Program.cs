using MBN;
using MBN.Modules;

using System.Diagnostics;
using System.Threading;

namespace Examples
{
    internal class Program
    {
        private static TempHumClick _sensor;

        private static void Main()
        {
            _sensor = new TempHumClick(Hardware.SC20100_1)
            {
                TemperatureUnit = TemperatureUnits.Fahrenheit
            };

            _sensor.ConfigureSensor(TempHumClick.TemperatureResolutions.Average128, TempHumClick.HumidityResolutions.Average128, TempHumClick.ODR.OdrOneShot, TempHumClick.BDU.Quiescent);

            while (true)
            {
                Debug.WriteLine($"Temperature: {_sensor.ReadTemperature():F1} °F");
                Debug.WriteLine($"   Humidity: {_sensor.ReadHumidity():F1} %RH\n");
                Thread.Sleep(1000);
            }
        }
    }
}
