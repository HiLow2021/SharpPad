using SharpDX.XInput;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpPad.EventArgs
{
    public class XControllerBatteryLevelChangedEventArgs : System.EventArgs
    {
        public BatteryType BatteryType { get; }
        public BatteryLevel NewBatteryLevel { get; }
        public BatteryLevel OldBatteryLevel { get; }

        public XControllerBatteryLevelChangedEventArgs(BatteryType batteryType, BatteryLevel oldBatteryLevel, BatteryLevel newBatteryLevel)
        {
            BatteryType = batteryType;
            OldBatteryLevel = oldBatteryLevel;
            NewBatteryLevel = newBatteryLevel;
        }
    }
}
