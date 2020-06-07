using MBN;
using MBN.Modules;
using System;
using System.Diagnostics;
using System.Threading;

namespace Examples
{
    public class Program
    {
        static CapSenseClick _cap;       // CapSense Click board

        public static void Main()
        {
            _cap = new CapSenseClick(Hardware.SC20260_1);     // CapSense on socket 1 at address 0x00

            _cap.ButtonPressed += Cap_ButtonPressed;                 // Subscribe to the ButtonPressed event
            _cap.SliderDataChanged += Cap_SliderDataChanged;         // Subscribe to the SliderDataChanged event

            while (true)
            {
                _cap.CheckButtons();             // Checks if any button is pressed
                _cap.CheckSlider();              // Checks if slider value has changed
                Thread.Sleep(50);
            }
        }

        static void Cap_SliderDataChanged(Object sender, CapSenseClick.SliderEventArgs e)
        {
            if (e.FingerPresent) 
            { 
                Debug.WriteLine($"Slider value = {e.SliderValue / 5}"); // Using default CapSense resolution of 50, displays values from 0 to 10
            }
        }

        static void Cap_ButtonPressed(Object sender, CapSenseClick.ButtonPressedEventArgs e)
        {
            // Light on the led of the corresponding button when pressed (and switch off when depressed).
            _cap.LedBottom = e.ButtonBottom;
            _cap.LedTop = e.ButtonTop;
        }
    }
}
