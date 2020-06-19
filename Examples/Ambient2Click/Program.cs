using MBN;
using MBN.Modules;

using System;
using System.Diagnostics;
using System.Threading;

namespace Examples
{
    public static class Program
    {
        private static Ambient2Click _light;

        public static void Main()
        {
            _light = new Ambient2Click(Hardware.SC20100_1, Ambient2Click.I2CAddresses.Address0)
            {
                OperatingMode = Ambient2Click.OperatingModes.Continuous,
                ConversionTime = Ambient2Click.ConversionTimes.Ms800,
                ConsecutiveFaults = Ambient2Click.Faults.OneFault,
                DetectionRange = Ambient2Click.Range.AutoFullScale,
            };

            _light.SetAlertLimits(10, 50);

            _light.DetectionRange = Ambient2Click.Range.AutoFullScale;


            while (true)
            {
                _light.ReadSensor(out Int32 luxValue, out Boolean hasAlert, out Ambient2Click.AlertType type);
                Debug.WriteLine($"LUX is {luxValue} has an alert? {hasAlert} of type {type}");
                Thread.Sleep(1000);
            }
        }
    }
}
