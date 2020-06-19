using MBN;
using MBN.Modules;

using System.Diagnostics;
using System.Threading;

namespace Examples
{
    internal class Program
    {
        private static TempHum10Click _sensor;

        private static void Main()
        {
            _sensor = new TempHum10Click(Hardware.SC20100_2)
            {
                TemperatureUnits = TemperatureUnits.Kelvin,
                HumiditySamplingRate = TempHum10Click.HumiditySampling.EightTimes,
                TemperatureSamplingRate = TempHum10Click.TemperatureSampling.SixteenTimes
            };

            for (; ; )
            {
                Debug.WriteLine($"Temperature.....: {_sensor.ReadTemperature():F2} °K");
                Debug.WriteLine($"Humidity........: {_sensor.ReadHumidity():F2} %RH\n");
                Thread.Sleep(2000);
            }
        }
    }
}
