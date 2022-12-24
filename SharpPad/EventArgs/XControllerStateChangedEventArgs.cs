using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX.XInput;

namespace SharpPad.EventArgs
{
    public class XControllerStateChangedEventArgs : System.EventArgs
    {
        public State NewState { get; }
        public State OldState { get; }

        public XControllerStateChangedEventArgs(State oldState, State newState)
        {
            OldState = oldState;
            NewState = newState;
        }
    }
}
