using MBN;
using MBN.Modules;

using System;
using System.Diagnostics;
using System.Threading;

namespace Examples
{
    internal class Program
    {
        private static Altitude2Click _sensor;

        private static void Main()
        {
            _sensor = new Altitude2Click(Hardware.SC20100_1, Altitude2Click.I2CAddress.AddressTwo)
            {
                TemperatureUnit = TemperatureUnits.Fahrenheit,
                PressureCompensationMode = PressureCompensationModes.SeaLevelCompensated,
                PressureOverSamplingRate = Altitude2Click.OverSamplingRate.ADC4096,
                TemperatureOverSamplingRate = Altitude2Click.OverSamplingRate.ADC4096
            };

            for (;;)
            {
                _sensor.ReadSensor(out Single temperature, out Single pressure, out Single altitude);
                Debug.WriteLine($"Temperature.........: {temperature:F2} °F");
                Debug.WriteLine($"Pressure............: {pressure:F0} Pascals");
                Debug.WriteLine($"Altitude............: {altitude:F0} meters");
                Debug.WriteLine("-----------------------------------");
                Thread.Sleep(2000);
            }
        }
    }
}
