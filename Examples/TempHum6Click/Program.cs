using MBN;
using MBN.Modules;

using System.Diagnostics;
using System.Threading;


namespace Examples
{
    public class Program
    {
        private static TempHum6Click _sensor;

        public static void Main()
        {
            _sensor = new TempHum6Click(Hardware.SC20100_2)
            {
                TemperatureUnit = TemperatureUnits.Fahrenheit
            };

            Debug.WriteLine($"PartID is 0x{_sensor.PartID:x3}");
            Debug.WriteLine($"UID is {_sensor.UniqueID}");

            _sensor.PowerMode = PowerModes.Low;

            TempHum6Click.SensorConfiguration configuration = new TempHum6Click.SensorConfiguration
            {
                LowPower = false,
                EnableHumidityMeasurement = true,
                EnableTemperatureMeasurement = true,
                HumidityMeasurementMode = TempHum6Click.MeasurementMode.Continuous,
                TemperatureMeasurementMode = TempHum6Click.MeasurementMode.Continuous
            };

            _sensor.ConfigureSensor(configuration);

            while (true)
            {
                Debug.WriteLine($"Temperature .......: {_sensor.ReadTemperature():F2} °F");
                Debug.WriteLine($"Humidity...........: {_sensor.ReadHumidity():F2} %RH");
                Thread.Sleep(2000);
            }
        }
    }
}