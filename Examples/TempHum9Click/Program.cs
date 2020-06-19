using MBN;
using MBN.Modules;

using System.Diagnostics;
using System.Threading;

namespace Examples
{
    internal class Program
    {
        private static TempHum9Click _sensor;

        private static void Main()
        {
            _sensor = new TempHum9Click(Hardware.SC20100_1)
            {
                PowerMode = PowerModes.On,
                ClockStretch = true
            };

            Debug.WriteLine($"ID is {_sensor.ReadID()}");

            while (true)
            {
                Debug.WriteLine("----------Temp&Hum 9 Click----------");
                Debug.WriteLine($"Temperature...............: {_sensor.ReadTemperature():F2} °F");
                Debug.WriteLine($"Humidity..................: {_sensor.ReadHumidity():F2} %RH");
                Debug.WriteLine($"Temperature RawData is... : {((ITemperature)_sensor).RawData}");
                Debug.WriteLine($"Humidity RawData is...... : {((IHumidity)_sensor).RawData}");
                Thread.Sleep(2000);
            }
        }
    }
}
