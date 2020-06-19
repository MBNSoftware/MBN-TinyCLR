using MBN;
using MBN.Modules;

using System;
using System.Diagnostics;
using System.Threading;

namespace Examples
{
    class Program
    {
        private static RTC3Click _rtc;

        private static void Main()
        {
            _rtc = new RTC3Click(Hardware.SC20100_1);

            _rtc.EnableTrickleCharger(true);
            _rtc.RTCEnabled = true;

            DateTime currentTime = _rtc.GetDateTime();

            if (currentTime.Ticks >= new DateTime(2020, 05, 26, 22, 15, 30).Ticks)
            {
                Debug.WriteLine("DateTime already set");
            }
            else
            {
                Debug.WriteLine("RTC not set, setting new DateTime");
                _rtc.SetDateTime(new DateTime(2020, 05, 26, 22, 15, 30));
            }

            while (true)
            {
                Debug.WriteLine($"Current RTC date/time is { _rtc.GetDateTime():G}");
                Thread.Sleep(1000);
            }
        }
    }
}