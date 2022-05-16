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
        private static Storage _eeprom;
        private static readonly Byte[] Data = new Byte[3];

        static void Main()
        {
            try
            {
                _eeprom = new EEpromClick(Hardware.SocketFour, 0xA0 >> 1);

                WriteLine("Address 231 before : " + _eeprom.ReadByte(231));
                _eeprom.WriteByte(231, 123);
                WriteLine("Address 231 after : " + _eeprom.ReadByte(231));

                _eeprom.WriteData(400, new Byte[] { 111, 112, 113 }, 0, 3);
                _eeprom.ReadData(400, Data, 0, 3);
                WriteLine("Read 3 bytes starting @400 (should be 111, 112, 113) : " + Data[0] + ", " + Data[1] + ", " + Data[2]);

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
