using MBN;
using MBN.Modules;
using System;
using System.Diagnostics;
using System.Threading;

namespace Examples
{
    class Program
    {
        static void Main()
        {
            TestGnss5(Hardware.SocketOne);

            Thread.Sleep(Timeout.Infinite);
        }

        private static void TestGnss5(Hardware.Socket socket)
        {
            Gnss5Click _gnss5 = new Gnss5Click(socket);

            GPSUtilities.GSAFrameReceived += GPSUtilities_GSAFrameReceived;
            GPSUtilities.GGAFrameReceived += GPSUtilities_GGAFrameReceived;
            GPSUtilities.RMCFrameReceived += GPSUtilities_RMCFrameReceived;
            GPSUtilities.GSVFrameReceived += GPSUtilities_GSVFrameReceived;

        }

        private static void GPSUtilities_GSVFrameReceived(Object sender, GPSUtilities.GSVFrameEventArgs e)
        {
            Debug.WriteLine($"{GPSUtilities.FrameCount} - GSV frame received : Signal origin = {e.SignalOrigin}, Satellite[0] = {e.NumberOfMessages}");
        }

        private static void GPSUtilities_GSAFrameReceived(Object sender, GPSUtilities.GSAFrameEventArgs e)
        {
            Debug.WriteLine($"{GPSUtilities.FrameCount} - GSA frame received : Signal origin = {e.SignalOrigin}, 2D/3D mode {e.Auto2D3D}, Fix mode {e.FixMode}");
        }

        private static void GPSUtilities_GGAFrameReceived(Object sender, GPSUtilities.GGAFrameEventArgs e)
        {
            Debug.WriteLine($"{GPSUtilities.FrameCount} - GGA frame received : Signal origin = {e.SignalOrigin}, Latitude {e.Latitude:F2} {e.LatitudeHemisphere}, Longitude {e.Longitude:F2} {e.LongitudePosition}");
        }

        private static void GPSUtilities_RMCFrameReceived(Object sender, GPSUtilities.RMCFrameEventArgs e)
        {
            Debug.WriteLine($"{GPSUtilities.FrameCount} - RMC frame received : Signal origin = {e.SignalOrigin}, Latitude {e.Latitude:F2} {e.LatitudeHemisphere}, Longitude {e.Longitude:F2} {e.LongitudePosition}");
        }


    }
}
