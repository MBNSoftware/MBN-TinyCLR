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
using static System.Diagnostics.Debug;

namespace Examples
{
    class Program
    {
        private static AdcClick _adc;

        static void Main()
        {
            try
            {
                _adc = new AdcClick(Hardware.SocketTwo);

                // Sets the range from 0 to 3300 (instead of 0-4095)
                _adc.SetScale(0, 3300);

                while (true)
                {
                    WriteLine($"Ch1 : {_adc.GetChannel(1) / 1000.0:F3} V");
                    Thread.Sleep(200);
                }
            }
            catch (Exception ex) when (Debugger.IsAttached)
            {
                WriteLine("Exception caught : " + ex.Message);
            }
            catch
            {
                while (true)
                {
                    Hardware.Led3.Write(GpioPinValue.High ^ Hardware.Led3.Read());
                    Thread.Sleep(50);
                }
            }
            finally
            {
                Thread.Sleep(Timeout.Infinite);
            }
        }
    }
}
