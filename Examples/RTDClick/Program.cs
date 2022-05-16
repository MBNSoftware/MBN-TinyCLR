//
// Portions Copyright (c) 2020 Robin Jones (NetworkFusion).  All rights reserved.
// See LICENSE file in the project root for full license information.
//

using MBN;
using MBN.Modules;
using System;
using System.Diagnostics;
using System.Threading;

namespace Examples
{
    public class Program
    {
        private static RTDClick _rtd;

        public static byte config = (
            (byte)RTDClick.ConfigValues.VBIAS_ON |
            (byte)RTDClick.ConfigValues.THREE_WIRE | //note: with default sensor, but should be 2wire or 4wire depending on jumpers
            (byte)RTDClick.ConfigValues.FAULT_CLEAR_STATE |
            (byte)RTDClick.ConfigValues.FILTER_60Hz);


        public static void Main()
        {
            Debug.WriteLine("RTDClick Driver Demo.");

            TestRtd(Hardware.SocketTwo);

            Thread.Sleep(Timeout.Infinite);
        }

        private static void TestRtd(Hardware.Socket socket)
        {
            //note: if using a PT1000, you should do adjust to fit, e.g.
            //_rtd.Initialize(socket, config, 470.00f, MAX31865.SensorType.PT1000);
            _rtd = new RTDClick(socket, config);


            PollingSenario();
            //EventSenario()

        }


        public static void PollingSenario()
        {
            _rtd.SetConvToManual();

            var i = 0;
            var settlingTime = 10; //10ms

            Debug.WriteLine("PRT Data:");

            for ( ; ; )
            {
                //ExecuteOneshot(); could be used, but more accurate results can be acheived by allowing the sensor to settle through multiple polls.
                _rtd.SetConvToAuto();
                Thread.Sleep(settlingTime);
                _rtd.SetConvToManual();

                //note: on startup the sensor can show -248 before it has been initialised properly ?! if the ADC is not attached it will read more 855
                //    if (temperature > -150 && temperature < 150) 
                Debug.WriteLine($"Fault Status: {GetFaultStatus()}, config: {GetCurrentConfig()}");
                // note: reading a sensor past their default accuracy is not really helpful, for production, it would would be wise to use something like:
                //    var trunkatedTemp = System.Math.Truncate((GetTemperature() * 100) / 100);
                Debug.WriteLine($"{i++}: temperature: {GetTemperature()}, resistance: {GetResistance()}");

                Thread.Sleep(15000 - settlingTime); //15 seconds is about right to stop self heating from occuring on the sensor
            }
        }


        public static void EventSenario()
        {

            _rtd.SetConvToAuto(); //Auto is 50 or 60hz, do you really want to read that many times per second? it will heat up the sensor element!

            _rtd.EnableFaultScanner(1000);
            _rtd.FaultEvent += _rtd_FaultEvent;

            _rtd.DataReadyCelsiusEvent += _rtd_DataReadyCelEvent;

            Thread.Sleep(Timeout.Infinite);


        }


        public static float GetResistance()
        {

            return _rtd.GetResistance();
        }


        public static float GetTemperature()
        {

            return _rtd.GetTemperatureCelsius();
        }


        public static string GetFaultStatus()
        {
            return _rtd.GetRegister(0x07).ToString("X");
        }


        public static string GetCurrentConfig()
        {
            return _rtd.GetRegister(0x00).ToString("X");
        }


        public static void ExecuteOneshot()
        {
            _rtd.ExecuteOneShot();
        }


        public static void _rtd_DataReadyCelEvent(object sender, float Data)
        {
            Debug.WriteLine("Temperature: " + GetTemperature() + "c ");
        }


        public static void _rtd_FaultEvent(object sender, byte FaultByte)
        {
            Debug.WriteLine("Fault: " + FaultByte.ToString("X"));
            _rtd.ClearFaults();
        }

    }
}
