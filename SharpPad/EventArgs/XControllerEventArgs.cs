using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX.XInput;

namespace SharpPad.EventArgs
{
    public class XControllerEventArgs : System.EventArgs
    {
        public Controller Controller { get; }

        public XControllerEventArgs(Controller controller)
        {
            Controller = controller;
        }
    }
}
