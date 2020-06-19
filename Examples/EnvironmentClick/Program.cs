using MBN;
using MBN.Modules;

using System.Diagnostics;
using System.Threading;

namespace Examples
{
    internal class Program
    {
        private static EnvironmentClick _sensor;

        private static void Main()
        {
            _sensor = new EnvironmentClick(Hardware.SC20100_1, EnvironmentClick.I2CAddress.Address1);

            _sensor.SetRecommendedMode(EnvironmentClick.RecommendedModes.WeatherMonitoring);
            _sensor.SetHeaterProfile(200, 250, EnvironmentClick.HeaterProfiles.Profile_0);
            _sensor.SelectHeaterProfile(EnvironmentClick.HeaterProfiles.Profile_0);

            Debug.WriteLine($"Device ID is 0x{_sensor.DeviceId:x2}");
            Debug.WriteLine($"Temperature Sampling Rate is {_sensor.TemperatureSamplingRate}");
            Debug.WriteLine($"Pressure Sampling Rate is {_sensor.PressureSamplingRate}");
            Debug.WriteLine($"Humidity Sampling Rate is {_sensor.HumiditySamplingRate}");
            Debug.WriteLine($"Filter setting is {_sensor.Filter}");

            while (true)
            {
                Debug.WriteLine("------EnvironmentClick Click ------");
                Debug.WriteLine($"Temperature.............: {_sensor.ReadTemperature():F2} *C");
                Debug.WriteLine($"Humidity................: {_sensor.ReadHumidity():F2} %RH");
                Debug.WriteLine($"Pressure................: {_sensor.ReadPressure():F1} mBar");
                Debug.WriteLine($"Altitude................: {_sensor.ReadAltitude():F1} meters");
                Debug.WriteLine($"Gas Resistance..........: {_sensor.ReadGasResistance()} kOhms");
                Debug.WriteLine($"Gas Heater Stable?.......: {_sensor.HeaterStable}");
                Debug.WriteLine($"Gas Reading Valid?.......: {_sensor.GasReadingValid}\n");
                Thread.Sleep(2000);
            }
        }
    }
}
