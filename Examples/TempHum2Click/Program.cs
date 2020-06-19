using MBN;
using MBN.Modules;

using System;
using System.Diagnostics;
using System.Threading;

namespace Examples
{
    internal class Program
    {
        private static TempHum2Click _sensor;
        private static Single _temperature;
        private static Single _humidity;
        private static Boolean _result;

        private static void Main()
        {
            _sensor = new TempHum2Click(Hardware.SC20100_2)
            {
                TemperatureUnit = TemperatureUnits.Fahrenheit
            };

            _sensor.SetMode(TempHum2Click.OperatingModes.NoHold, TempHum2Click.SpeedModes.Fast);

            Debug.WriteLine($"Device Serial Number is {_sensor.DeviceSerialNumber}");
            Debug.WriteLine($"Device Firmware Revision is {_sensor.FirmwareRevision}");
            Debug.WriteLine($"QueryDevice result is {_sensor.QueryDevice()}");

            Debug.WriteLine($"Heater current is {_sensor.HeaterCurrent}");
            _sensor.HeaterCurrent = TempHum2Click.HeaterCurrentSettings.MA_53_5;
            Debug.WriteLine($"Heater current is {_sensor.HeaterCurrent}");
            _sensor.HeaterCurrent = TempHum2Click.HeaterCurrentSettings.MA_6_4;
            Debug.WriteLine($"Heater current is {_sensor.HeaterCurrent}");

            Debug.WriteLine($"Heater circuit enabled? {_sensor.HeaterEnable}");
            _sensor.HeaterEnable = true;
            Debug.WriteLine($"Heater circuit enabled? {_sensor.HeaterEnable}");
            _sensor.HeaterEnable = false;
            Debug.WriteLine($"Heater circuit enabled? {_sensor.HeaterEnable}");

            while (true)
            {
                _result = _sensor.ReadSensor(out _temperature, out _humidity);
                if (_result)
                {
                    Debug.WriteLine($"Temperature is {_temperature:F1} °F");
                    Debug.WriteLine($"   Humidity is {_humidity:F1} %RH\n");
                }
                else
                {
                    Debug.WriteLine("Invalid data");
                }

                Thread.Sleep(2000);
            }
        }
    }
}