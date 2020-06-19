using MBN;
using MBN.Modules;

using System.Diagnostics;
using System.Threading;

namespace Examples
{
    public class Program
    {
        private static ButtonRClick _button;

        public static void Main()
        {
            _button = new ButtonRClick(Hardware.SC20100_1) { LEDMode = ButtonRClick.Mode.ToggleWhenPressed };
            _button.ButtonPressed += Button_ButtonPressed;
            _button.ButtonReleased += Button_ButtonReleased;

            Thread.Sleep(Timeout.Infinite);
        }

        static void Button_ButtonReleased(ButtonRClick sender, ButtonRClick.ButtonState state)
        {
            Debug.WriteLine($"Button Released Event with a state of {(state == ButtonRClick.ButtonState.Pressed ? " pressed" : " released")}");
        }

        static void Button_ButtonPressed(ButtonRClick sender, ButtonRClick.ButtonState state)
        {
            Debug.WriteLine($"Button Pressed Event with a state of {(state == ButtonRClick.ButtonState.Pressed ? " pressed" : " released")}");
        }
    }
}
