#if (NANOFRAMEWORK_1_0)
using System.Device.Gpio;
using nanoFramework.Stm32.Pins;
#else
using GHIElectronics.TinyCLR.Devices.Gpio;
using GHIElectronics.TinyCLR.Pins;
#endif

using MBN;
using MBN.Modules;
using System.Diagnostics;
using System.Threading;

namespace Examples
{
    class Program
    {
        private static Keypad4X3 _keypad;

        public static void Main()
        {
            _keypad = new Keypad4X3(
                // Rows
                STM32H7.GpioPin.PE5, STM32H7.GpioPin.PE4, STM32H7.GpioPin.PE3, STM32H7.GpioPin.PE2, 
                // Columns
                STM32H7.GpioPin.PD7, STM32H7.GpioPin.PD4, STM32H7.GpioPin.PD3);
            _keypad.KeyReleased += KeypadKeyReleased;
            _keypad.KeyPressed += KeypadKeyPressed;

            _keypad.StartScan();

            Thread.Sleep(Timeout.Infinite);
        }

        static void KeypadKeyPressed(System.Object sender, Keypad4X3.KeyPressedEventArgs e) => Hardware.Led1.Write(GpioPinValue.High);

        static void KeypadKeyReleased(System.Object sender, Keypad4X3.KeyReleasedEventArgs e)
        {
            Debug.WriteLine("Key : " + e.KeyChar);
            Hardware.Led1.Write(GpioPinValue.Low);
        }
    }
}
