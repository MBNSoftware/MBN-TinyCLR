#if (NANOFRAMEWORK_1_0)
using Windows.Devices.SerialCommunication;
using Windows.Storage.Streams;
#else
using GHIElectronics.TinyCLR.Devices.Uart;
#endif

using MBN;
using MBN.Modules;
using System;
using System.Diagnostics;
using System.Threading;

namespace Examples
{
    internal class Program
    {
        private static USBUARTClick _usbUart;

        public static void Main()
        {
            _usbUart = new USBUARTClick(Hardware.SocketOne, USBUARTClick.BaudRate.Baud460800);

            _usbUart.DataReceived += USBUartUSBUARTDataReceived;
            _usbUart.CableConnectionChanged += USBUartUSBUARTCableConnectionChanged;
            _usbUart.SleepSuspend += USBUARTUartUSBUARTSleepSuspend;

            Debug.WriteLine("USB is connected ? " + _usbUart.USBCableConnected);
            Debug.WriteLine("USB sleeping ? " + _usbUart.USBSuspended);

            Thread.Sleep(Timeout.Infinite);
        }

        private static void USBUARTUartUSBUARTSleepSuspend(Object sender, Boolean isSleeping, DateTime eventTime) => Debug.WriteLine("USB " + (isSleeping ? "Suspend/Sleep" : "Wake") + " event occurred at " + eventTime);

        private static void USBUartUSBUARTCableConnectionChanged(Object sender, Boolean cableConnected, DateTime eventTime) => Debug.WriteLine("Cable " + (cableConnected ? "connection" : "dis-connection") + " event occurred at " + eventTime);

        private static void USBUartUSBUARTDataReceived(Object sender, String message, DateTime eventTime)
        {
            // Echo back to sender
            _usbUart.SendData("Received your message of - \"" + message + "\" at " + eventTime);
            Debug.WriteLine(message);
        }
    }
}
