using MBN;
using MBN.Modules;

using System.Diagnostics;
using System.Threading;

namespace Examples
{
    internal class Program
    {

        private static BarometerClick _sensor;

        private static void Main()
        {
            _sensor = new BarometerClick(Hardware.SC20100_1, BarometerClick.I2CAddress.Address1);
            _sensor.SetRecommendedMode(BarometerClick.RecommendedModes.WeatherMonitoring);

            Debug.WriteLine("-----Barometer Click Demo-----");

            while (true)
            {
                Debug.WriteLine($"Temperature-----: {_sensor.ReadTemperature():F1} *C");
                Debug.WriteLine($"Pressure--------: {_sensor.ReadPressure(PressureCompensationModes.SeaLevelCompensated):F1} mBar");
                Debug.WriteLine($"Altitude--------: {_sensor.ReadAltitude():F0} meters\n");

                Thread.Sleep(1000);
            }

        }
    }
}
