using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using SharpDX.XInput;
using SharpPad.EventArgs;

namespace SharpPad.Demo
{
    public partial class Form1 : Form
    {
        private readonly ToolStripMenuItem[] menuItems;
        private readonly XInputManager manager = new XInputManager();
        private XController[] controllers;
        private XController targetController;
        private bool isVibrating;

        public Form1()
        {
            InitializeComponent();

            menuItems = new ToolStripMenuItem[]
            {
                pad1ToolStripMenuItem,
                pad2ToolStripMenuItem,
                pad3ToolStripMenuItem,
                pad4ToolStripMenuItem
            };

            Load += (sender, e) =>
            {
                SearchControllers();

                if (controllers.Length > 0)
                {
                    var userIndex = controllers.First().UserIndex;
                    var index = ConvertUserIndexToInt(userIndex);

                    Start(menuItems[index], userIndex);
                }
            };
            FormClosing += (sender, e) =>
            {
                foreach (var item in controllers)
                {
                    item.Dispose();
                }
            };
            exitToolStripMenuItem.Click += (sender, e) => Application.Exit();
            detectToolStripMenuItem.Click += (sender, e) => SearchControllers();
            pad1ToolStripMenuItem.Click += (sender, e) => Start(sender as ToolStripMenuItem, UserIndex.One);
            pad2ToolStripMenuItem.Click += (sender, e) => Start(sender as ToolStripMenuItem, UserIndex.Two);
            pad3ToolStripMenuItem.Click += (sender, e) => Start(sender as ToolStripMenuItem, UserIndex.Three);
            pad4ToolStripMenuItem.Click += (sender, e) => Start(sender as ToolStripMenuItem, UserIndex.Four);
            button1.Click += async (sender, e) =>
            {
                if (isVibrating)
                {
                    return;
                }

                isVibrating = true;
                targetController?.SetVibration((ushort)trackBar1.Value, (ushort)trackBar1.Value);
                await Task.Delay(3000);
                targetController?.SetVibration(0, 0);
                isVibrating = false;
            };
            button2.Click += (sender, e) => textBox1.Clear();

            void Start(ToolStripMenuItem menuItem, UserIndex userIndex)
            {
                ChangeToolStripMenuItemChecked(menuItem);
                StartController(userIndex);
            }
        }

        private void ChangeToolStripMenuItemChecked(ToolStripMenuItem toolStripMenuItem)
        {
            foreach (var item in menuItems)
            {
                item.Checked = false;
            }

            toolStripMenuItem.Checked = true;
        }

        private int ConvertUserIndexToInt(UserIndex userIndex)
        {
            switch (userIndex)
            {
                case UserIndex.One: return 0;
                case UserIndex.Two: return 1;
                case UserIndex.Three: return 2;
                case UserIndex.Four: return 3;
                default: throw new NotSupportedException();
            }
        }

        private void SearchControllers()
        {
            targetController?.Stop();
            controllers = manager.GetXControllers().Where(x => x.IsConnected).ToArray();

            foreach (var item in menuItems)
            {
                item.Checked = false;
                item.Enabled = false;
            }
            foreach (var item in controllers)
            {
                var index = ConvertUserIndexToInt(item.UserIndex);
                var toolStripMenuItem = menuItems[index];

                toolStripMenuItem.Enabled = true;
            }

            ShowStatusMessage(controllers.Length > 0);
        }

        private void StartController(UserIndex userIndex)
        {
            var controller = controllers.FirstOrDefault(x => x.UserIndex == userIndex);

            if (controller == targetController)
            {
                return;
            }

            if (targetController != null)
            {
                targetController.ButtonDown -= XController_ButtonDown;
                targetController.ButtonUp -= XController_ButtonUp;
                targetController.StateChanged -= XController_StateChanged;
                targetController.BatteryLevelChanged -= XController_BatteryLevelChanged;
                targetController.Stop();
            }
            if (controller != null)
            {
                controller.ButtonDown += XController_ButtonDown;
                controller.ButtonUp += XController_ButtonUp;
                controller.StateChanged += XController_StateChanged;
                controller.BatteryLevelChanged += XController_BatteryLevelChanged;
                controller.Run();
                ShowBatteryLevelMessage(controller.BatteryType, controller.BatteryLevel);
            }

            targetController = controller;

            void XController_ButtonDown(object sender, XControllerButtonEventArgs e)
            {
                ShowMessage(e.Buttons.ToString() + " が " + nameof(XController.ButtonDown));
            }
            void XController_ButtonUp(object sender, XControllerButtonEventArgs e)
            {
                ShowMessage(e.Buttons.ToString() + " が " + nameof(XController.ButtonUp));
            }
            void XController_StateChanged(object sender, XControllerStateChangedEventArgs e)
            {
                var message = string.Empty;
                var gamepad = e.NewState.Gamepad;
                var buttonFlags = Enum.GetNames(typeof(GamepadButtonFlags)).Where(x => string.Compare(x, "None", StringComparison.OrdinalIgnoreCase) != 0);
                var buttonDownFlags = gamepad.Buttons.ToString().Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(x => x.Trim());
                var newLine = Environment.NewLine;
                var separator = " : ";

                foreach (var item in buttonFlags)
                {
                    message += item + separator + buttonDownFlags.Contains(item).ToString() + newLine;
                }

                message += nameof(gamepad.LeftThumbX) + separator + gamepad.LeftThumbX + newLine;
                message += nameof(gamepad.LeftThumbY) + separator + gamepad.LeftThumbY + newLine;
                message += nameof(gamepad.RightThumbX) + separator + gamepad.RightThumbX + newLine;
                message += nameof(gamepad.RightThumbY) + separator + gamepad.RightThumbY + newLine;
                message += nameof(gamepad.LeftTrigger) + separator + gamepad.LeftTrigger + newLine;
                message += nameof(gamepad.RightTrigger) + separator + gamepad.RightTrigger + newLine;

                ShowButtonStates(message);
            }
            void XController_BatteryLevelChanged(object sender, XControllerBatteryLevelChangedEventArgs e)
            {
                ShowBatteryLevelMessage(e.BatteryType, e.NewBatteryLevel);
            }
        }

        private void ShowButtonStates(string message)
        {
            label1.Invoke(new Action(() => label1.Text = message));
        }

        private void ShowMessage(string message)
        {
            textBox1.Invoke(new Action(() => textBox1.AppendText(message + Environment.NewLine)));
        }

        private void ShowStatusMessage(bool isConnected)
        {
            if (isConnected)
            {
                ShowStatusMessage("コントローラ接続中");
            }
            else
            {
                ShowStatusMessage("XInput 対応コントローラが接続されていません");
            }
        }

        private void ShowStatusMessage(string message)
        {
            toolStripStatusLabel1.Text = message;
        }

        private void ShowBatteryLevelMessage(BatteryType batteryType, BatteryLevel batteryLevel)
        {
            statusStrip1.Invoke(new Action(() => toolStripStatusLabel2.Text = $"タイプ : {batteryType}  残量 : {batteryLevel}"));
        }
    }
}
