#if (NANOFRAMEWORK_1_0)
using System.Device.Gpio;
#else
using GHIElectronics.TinyCLR.Devices.Gpio;
#endif

using MBN;
using MBN.Modules;
using System;
using System.Diagnostics;
using System.Threading;

namespace Examples
{
    class Program
    {
        static ColorClick _color;
        static Double[] _tabColors;

        public static void Main()
        {
            _tabColors = new Double[4];

            _color = new ColorClick(Hardware.SC20260_1);

            _color.LedR.Write(GpioPinValue.High);
            _color.LedG.Write(GpioPinValue.High);
            _color.LedB.Write(GpioPinValue.High);

            _color.Gain = ColorClick.Gains.x4;
            _color.IntegrationTime = 200;

            Debug.WriteLine($"Color ID : {_color.GetID()}");
            Debug.WriteLine($"Integration time : {_color.IntegrationTime}");
            Debug.WriteLine($"Wait time : {_color.WaitTime}");
            Debug.WriteLine($"Wait long : {_color.WaitLong}");
            Debug.WriteLine($"Gain : {_color.Gain}");

            while (true)
            {
                var colorTmp = 0.0;

                for (var i = 0; i < 5; i++)
                {
                    _tabColors = _color.GetAllChannels();
                    colorTmp += _color.RGBtoHSL(_tabColors[0] / _tabColors[3], _tabColors[1] / _tabColors[3], _tabColors[2] / _tabColors[3]);
                }
                colorTmp /= 16;

                Debug.WriteLine("Color : " + colorTmp);

                Thread.Sleep(1200);
            }
        }
    }
}
