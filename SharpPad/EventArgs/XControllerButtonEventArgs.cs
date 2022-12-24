using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX.XInput;

namespace SharpPad.EventArgs
{
    public class XControllerButtonEventArgs : System.EventArgs
    {
        public GamepadButtonFlags Buttons { get; }

        public XControllerButtonEventArgs(GamepadButtonFlags buttons)
        {
            Buttons = buttons;
        }
    }
}
