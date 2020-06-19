using System;
using System.Diagnostics;
using System.Threading;

using MBN;
using MBN.Modules;

namespace Examples
{
    public static class Program
    {
        private static BMP280 _sensor;

        public static void Main()
        {
            Debug.WriteLine("Program started");

            // Using the SPI Interface
            //_sensor = new BMP280(Hardware.SC20100_1)
            //{
            //    PressureCompensation = PressureCompensationModes.SeaLevelCompensated,
            //    TemperatureUnit = TemperatureUnits.Fahrenheit
            //};

            // Using the I2C Interface
            _sensor = new BMP280(Hardware.SC20100_1, BMP280.I2CAddress.I2CAddress0)
            {
                PressureCompensation = PressureCompensationModes.SeaLevelCompensated,
                TemperatureUnit = TemperatureUnits.Fahrenheit
            };

            Debug.WriteLine($"Device ID is {_sensor.DeviceId}");

            // Set recommended mode using SetRecemmondedMode method.
            _sensor.SetRecommendedMode(BMP280.RecommendedModes.WeatherMonitoring);

            // Or set individual parameters as in below. Note: You must call EnableSettings() method to enforce the user settings.
            //_sensor.PressureSamplingRate = Pressure4Click.OversamplingRates.Osr1;
            //_sensor.TemperatureSamplingRate = Pressure4Click.OversamplingRates.Osr1;
            //_sensor.Filter = Pressure4Click.FilterCoefficient.IIROff;
            //_sensor.StandbyDuration = Pressure4Click.StandbyDurations.MS_0_5;
            //_sensor.OperatingMode = Pressure4Click.Mode.Sleep;
            //_sensor.EnableSettings();

            while (true)
            {
                _sensor.ReadSensor(out Single pressure, out Single temperature, out Single altitude);

                Debug.WriteLine($"Pressure.......: {pressure:F1} hPa");
                Debug.WriteLine($"Temperature....: {temperature:F2} °F");
                Debug.WriteLine($"Altitude.......: {altitude:F0} meters\n");

                Thread.Sleep(2000);
            }
        }
    }
}