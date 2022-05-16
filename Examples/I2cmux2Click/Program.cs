#if (NANOFRAMEWORK_1_0)
using System.Device.Gpio;
#else
using GHIElectronics.TinyCLR.Devices.Gpio;
#endif

using MBN;
using MBN.Modules;
using System;
using System.Diagnostics;
using System.Text;
using System.Threading;

namespace Examples
{
    class Program
    {
        static void Main()
        {
            DemoMux2();
        }

        private static void DemoMux2()
        {
            var _mux = new I2cMux2Click(Hardware.SocketTwo, 0xE0 >> 1, 100000);
            _mux.InterruptDetected += _mux_InterruptDetected;

            GpioPin _pin1 = GpioController.GetDefault().OpenPin(Hardware.SocketSix.Rst);
            _pin1.SetDriveMode(GpioPinDriveMode.Output);
            _pin1.Write(GpioPinValue.High);

            GpioPin _pin2 = GpioController.GetDefault().OpenPin(Hardware.SocketSix.Int);
            _pin2.SetDriveMode(GpioPinDriveMode.Output);
            _pin2.Write(GpioPinValue.High);

            _mux.ActiveChannels = 0b00001000;
            var _lcd = new DevantechLcd03(Hardware.SocketThree, 0xC8 >> 1)
            {
                BackLight = true,
                Cursor = DevantechLcd03.Cursors.Hide
            };
            _lcd.ClearScreen();
            _lcd.Write(1, 1, "MBN rules");
            _lcd.Write(1, 4, $"{DateTime.Now}");

            _pin1.Write(GpioPinValue.Low);
            _pin2.Write(GpioPinValue.Low);
            Thread.Sleep(500);
            _pin1.Write(GpioPinValue.High);
            _pin2.Write(GpioPinValue.High);


            Thread.Sleep(2000);
            _lcd.ClearScreen();
            _lcd.BackLight = false;
        }

        private static void _mux_InterruptDetected(Object sender, I2cMux2Click.InterruptEventArgs e)
        {
            var sb = new StringBuilder();
            var chan = e.Channels;
            Debug.WriteLine($"Interrupt detected, value {e.Channels}");
            for (Byte i = 0; i < 4; i++)
            {
                if ((chan & 1) == 1)
                {
                    sb.Append($" Channel {i}{(i == 3 ? "." : ",")}");
                }
                chan >>= 1;
            }
            Debug.WriteLine($"Interrupt thrown on{sb}");
            sb.Clear();
        }

    }
}
