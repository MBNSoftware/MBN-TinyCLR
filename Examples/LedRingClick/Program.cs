using MBN;
using MBN.Modules;
using System;
using System.Diagnostics;
using System.Threading;
using static System.Diagnostics.Debug;
#if (NANOFRAMEWORK_1_0)
gpio = System.Device.Gpio;
#else
using gpio = GHIElectronics.TinyCLR.Devices.Gpio;
#endif

namespace Examples
{
    class Program
    {
        private static LedRingClick _ledRing;

        static void Main()
        {
            try
            {
                WriteLine("Program started");
                _ledRing = new LedRingClick(Hardware.SocketOne);
                for (Byte i = 0; i <= 32; i++)
                {
                    _ledRing.SetPosition(i);
                    Thread.Sleep(100);
                }
                for (Byte i = 0; i <= 32; i++)
                {
                    _ledRing.SetPosition(i, true);
                    Thread.Sleep(100);
                }
                Demo();
            }

            catch (Exception ex) when (Debugger.IsAttached)
            {
                WriteLine("Exception caught : " + ex.Message);
            }
            catch
            {
                while (true)
                {
#if (NANOFRAMEWORK_1_0)
                    Hardware.Led3.Write(gpio.PinValue.High ^ Hardware.Led3.Read());
#else
                    Hardware.Led3.Write(gpio.GpioPinValue.High ^ Hardware.Led3.Read());
#endif
                    Thread.Sleep(100);
                }
            }
            finally
            {
                WriteLine("Entering infinite loop...");
                while (true)
                {
                    Thread.Sleep(100);
                }
            }
        }

        private static void Demo()
        {
            Int64 val = 1;

            for (var z = 0; z <= 32; z++)
            {
                _ledRing.Write((UInt32)((val << z) - 1));
                Thread.Sleep(25);
            }
            Thread.Sleep(100);
            for (var z = 32; z >= 0; z--)
            {
                _ledRing.Write((UInt32)((val << z) - 1));
                Thread.Sleep(25);
            }
            Thread.Sleep(100);

            for (var z = 0; z <= 32; z++)
            {
                _ledRing.Write((UInt32)((val << z) - 1));
                Thread.Sleep(25);
            }
            Thread.Sleep(100);

            Int64 value = UInt32.MaxValue;

            for (var z = 0; z <= 32; z++)
            {
                value -= val << z;
                _ledRing.Write((UInt32)value);
                Thread.Sleep(25);
            }
            Thread.Sleep(100);

            for (var j = 0; j < 16; j++)
            {
                value = 0x11111111;

                for (var z = 0; z < 4; z++)
                {
                    _ledRing.Write((UInt32)(value << z));
                    Thread.Sleep(50);
                }
            }

            for (var j = 0; j < 8; j++)
            {
                _ledRing.Write(0xFFFFFFFF);
                Thread.Sleep(200);
                _ledRing.Write(0x00000000);
                Thread.Sleep(200);
            }

            // The following is taken from MikroE demo. I have only changed timings so that led is circling faster
            UInt32 led = 0x00000001;
            var var_time = 100;
            var i = 0;

            while (true)
            {
                _ledRing.Write(led);
                Thread.Sleep(var_time);
                led <<= 1;
                if (led == 0)
                {
                    led = 1;
                    i++;
                    if (i == 0)
                        var_time = 100;
                    else if (i == 1)
                        var_time = 50;
                    else if (i == 2)
                        var_time = 20;
                    else if (i == 3)
                        var_time = 10;
                    else if (i == 4)
                    {
                        var_time = 100;
                        i = 0;
                    }
                }
            }
        }
    }
}
