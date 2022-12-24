using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX.XInput;

namespace SharpPad
{
    public class XInputManager
    {
        public XController[] GetXControllers()
        {
            var controllers = new[]
            {
                new Controller(UserIndex.One),
                new Controller(UserIndex.Two),
                new Controller(UserIndex.Three),
                new Controller(UserIndex.Four)
            };

            return controllers.Select(x => new XController(x)).ToArray();
        }
    }
}
