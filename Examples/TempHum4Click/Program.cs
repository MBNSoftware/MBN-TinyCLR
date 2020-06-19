using MBN;
using MBN.Modules;

using System;
using System.Diagnostics;
using System.Threading;


namespace Examples
{
    public class Program
    {
        private static TempHum4Click _sensor;

        private static Single temperature, humidity;

        public static void Main()
        {
            _sensor = new TempHum4Click(Hardware.SC20100_1, TempHum4Click.I2CAddress.AddressOne) { TemperatureUnit = TemperatureUnits.Fahrenheit };

            _sensor.ConfigureSensor(TempHum4Click.AcquisitionModes.Sequential, TempHum4Click.TemperatureResolutions.ElevenBit, TempHum4Click.HumidityResolutions.EightBit, TempHum4Click.HeaterModes.Disabled);

            Debug.WriteLine("Manufacturer ID - 0x" + _sensor.GetManufacturerId().ToString("X"));
            Debug.WriteLine("Device ID - 0x" + _sensor.GetDeviceId().ToString("X"));
            Debug.WriteLine("SN - " + _sensor.GetSerialNumber());

            while (true)
            {
                /* For Sequential Acquisition Mode use the following code to obtain temperature and humidity data
                   You will need to change the Acquisition Mode to Sequential.*/
                _sensor.ReadSensor(out temperature, out humidity);
                Debug.WriteLine($"Temperature....: {temperature:F2} °F");
                Debug.WriteLine($"Humidity.......: {humidity:F2} %RH");

                /* For independent measurement mode use the following code to obtain temperature and humidity data
                   You will need to change the Acquisition Mode to Independent.*/
                //Debug.WriteLine($"Temperature....: {_sensor.ReadTemperature():F2} °F");
                //Debug.WriteLine($"Humidity.......: {_sensor.ReadHumidity():F2} %RH");

                Thread.Sleep(2000);
            }
        }
    }
}
