using MBN;
using MBN.Modules;

using System.Diagnostics;
using System.Threading;

namespace Examples
{
    public class Program
    {
        private static TempHum8Click _sensor;

        public static void Main()
        {
            _sensor = new TempHum8Click(Hardware.SC20100_2)
            {
                ClockStretch = false,
                DisableOTPReload = true,
                TemperatureUnit = TemperatureUnits.Fahrenheit,
                Resolution = TempHum8Click.SensorResolution.Resolution_12RH_14T
            };

            while (true)
            {
                Debug.WriteLine($"Temperature is {_sensor.ReadTemperature():F1} °F");
                Debug.WriteLine($"   Humidity is {_sensor.ReadHumidity():F1} %RH\n");
                Thread.Sleep(1000);
            }
        }
    }
}
