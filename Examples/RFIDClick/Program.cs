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
        private static RFIDClick _rfid;
        private static DevantechLcd03 _lcd;
        private static Timer _timerLcd;

        public static void Main()
        {
            try
            {
                _rfid = new RFIDClick(Hardware.SocketOne);
                InitLcd();

                Debug.WriteLine("RFID identification : " + _rfid.Identification());

                _lcd.Write(1, 4, "Calibration...");
                _rfid.Calibration(Hardware.Led2);
                _lcd.Write(1, 4, "              ");

                _rfid.TagDetected += Rfid_TagDetected;
                _rfid.TagRemoved += Rfid_TagRemoved;

                InitTimer();

                _rfid.DetectionEnabled = true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }

            Thread.Sleep(Timeout.Infinite);
        }

        static void InitTimer() => _timerLcd = new Timer(DimLCD, null, new TimeSpan(0, 0, 15), new TimeSpan(0, 0, 0));

        static void DimLCD(Object state) => _lcd.BackLight = false;

        static void Rfid_TagRemoved(Object sender, TagRemovedEventArgs e)
        {
            Hardware.Led1.Write(GpioPinValue.Low);
            Debug.WriteLine("Tag removed : " + e.TagID);
            _lcd.Write(1, 3, "                    ");
            _lcd.Write(1, 4, "                    ");
        }

        static void Rfid_TagDetected(Object sender, TagDetectedEventArgs e)
        {
            _lcd.BackLight = true;
            Hardware.Led1.Write(GpioPinValue.High);
            Debug.WriteLine("Tag detected : " + e.TagID);
            _lcd.Write(1, 3, e.TagID.ToString());
            _lcd.Write(1, 4, e.TagIDHex + "  #" + e.CRC);
            _timerLcd.Change(new TimeSpan(0, 0, 15), new TimeSpan(0, 0, 0));    // Dim Backlight after 15 seconds
        }

        private static void InitLcd()
        {
            _lcd = new DevantechLcd03(Hardware.SocketTwo, 0xC8 >> 1)
            {
                BackLight = true,
                Cursor = DevantechLcd03.Cursors.Hide
            };
            _lcd.ClearScreen();
            _lcd.Write(1, 1, "    MikroBus.Net");
            _lcd.Write(1, 2, "RFID Click demo");
        }

    }
}
