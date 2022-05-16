#if (NANOFRAMEWORK_1_0)
using System.Device.Gpio;
#else
using GHIElectronics.TinyCLR.Devices.I2c;
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
        private static RotaryClick _rot;
        private static Int32 _myCounter;
        private static Boolean _fillMode;

        static void Main()
        {
            TestRotary();

            Thread.Sleep(Timeout.Infinite);
        }

        private static void TestRotary()
        {
            _rot = new RotaryClick(Hardware.SocketOne);

            _rot.ButtonPressed += Rot_ButtonPressed;
            _rot.RotationDetected += Rot_RotationDetected;
        }

        private static void Rot_RotationDetected(Object sender, RotaryClick.RotationEventArgs e)
        {
            Debug.WriteLine($"Rotation detected : {(e.Direction == Directions.Clockwise ? "Clockwise" : "CounterClockwise")}, Internal counter = {e.InternalCounter}");
            _myCounter += e.Direction == Directions.Clockwise ? 1 : -1;
            if (_myCounter < 0)
                _myCounter = 0;
            if (_myCounter > 16)
                _myCounter = 16;
            _rot.SetLedPosition((Byte)_myCounter, _fillMode);
        }

        private static void Rot_ButtonPressed(Object sender, RotaryClick.ButtonPressedEventArgs e)
        {
            Hardware.Led1.Write(e.Edge == GpioPinEdge.RisingEdge ? GpioPinValue.High : GpioPinValue.Low);
            if (e.Edge == GpioPinEdge.RisingEdge)
            {
                _fillMode = !_fillMode;
                _rot.SetLedPosition((Byte)_myCounter, _fillMode);
            }
        }
    }
}
