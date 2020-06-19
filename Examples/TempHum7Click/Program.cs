using MBN;
using MBN.Modules;

using System.Diagnostics;
using System.Threading;

// ReSharper disable once CheckNamespace
namespace Examples
{
    internal static class Program
    {
        private static TempHum7Click _sensor;

        private static void Main()
        {
            _sensor = new TempHum7Click(Hardware.SC20100_1)
            {
                HumidityMeasurementMode = TempHum7Click.MeasurementMode.NoHold,
                TemperatureMeasurementMode = TempHum7Click.MeasurementMode.NoHold,
                SensorResolution = TempHum7Click.Resolution.High
            };


            Debug.WriteLine($"Device S/N is {_sensor.SerialNumber}");
            Debug.WriteLine($"Firmware Version ID is {_sensor.FirmwareVersion}");

            while (true)
            {
                Debug.WriteLine($"Temperature.....: {_sensor.ReadTemperature():F2}  °C");
                Debug.WriteLine($"Humidity........: {_sensor.ReadHumidity():F2} %RH\n");
                Thread.Sleep(1000);
            }
        }
    }
}
