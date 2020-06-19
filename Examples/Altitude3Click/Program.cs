using MBN;
using MBN.Modules;

using System;
using System.Diagnostics;
using System.Threading;

namespace Examples
{
    internal static class Program
    {
        private static Altitude3Click _sensor;

        private static void Main()
        {
            _sensor = new Altitude3Click(Hardware.SC20100_1)
            {
                TemperatureUnit = TemperatureUnits.Fahrenheit,
                PressureCompensation = PressureCompensationModes.SeaLevelCompensated,
                MeasurementMode = Altitude3Click.Mode.UltraLowNoise,
            };

            Debug.WriteLine("Device ID is " + _sensor.DeviceID);

            while (true)
            {
                _sensor.ReadSensor(out Single temperature, out Single pressure, out Single altitude);

                Debug.WriteLine("---------------------------------");
                Debug.WriteLine($"Temperature.......: {temperature:F2} °F");
                Debug.WriteLine($"Pressure..........: {pressure:F0} Pascals");
                Debug.WriteLine($"Altitude..........: {altitude:F0} meters");

                Thread.Sleep(1000);
            }
        }
    }
}
