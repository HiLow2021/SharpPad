using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using SharpDX;
using SharpDX.XInput;
using SharpPad.EventArgs;

namespace SharpPad
{
    public class XController : IDisposable
    {
        private readonly Controller controller;
        private CancellationTokenSource tokenSource;
        private CancellationToken token;

        public int Interval { get; set; } = 10;
        public int IntervalOnDisconnected { get; set; } = 1000;
        public bool IsConnected => controller.IsConnected;
        public bool IsRunning { get; private set; }
        public UserIndex UserIndex => controller.UserIndex;
        public BatteryType BatteryType => controller.GetBatteryInformation(BatteryDeviceType.Gamepad).BatteryType;
        public BatteryLevel BatteryLevel => controller.GetBatteryInformation(BatteryDeviceType.Gamepad).BatteryLevel;

        public event EventHandler<XControllerEventArgs> Elapsed;
        public event EventHandler<XControllerEventArgs> ConnectionChanged;
        public event EventHandler<XControllerButtonEventArgs> ButtonDown;
        public event EventHandler<XControllerButtonEventArgs> ButtonUp;
        public event EventHandler<XControllerStateChangedEventArgs> StateChanged;
        public event EventHandler<XControllerBatteryLevelChangedEventArgs> BatteryLevelChanged;

        internal XController(Controller controller)
        {
            this.controller = controller;
        }

        public void Run()
        {
            if (IsRunning)
            {
                return;
            }

            try
            {
                IsRunning = true;
                tokenSource = new CancellationTokenSource();
                token = tokenSource.Token;
                Task.Run(PollingTask).ContinueWith(x => IsRunning = false);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        public void Stop()
        {
            tokenSource?.Cancel();
        }

        public void SetVibration(ushort leftMotorSpeed, ushort rightMotorSpeed)
        {
            var vibration = new Vibration()
            {
                LeftMotorSpeed = leftMotorSpeed,
                RightMotorSpeed = rightMotorSpeed
            };

            SetVibration(vibration);
        }

        public void SetVibration(Vibration vibration)
        {
            if (controller.IsConnected)
            {
                controller.SetVibration(vibration);
            }
        }

        public void Dispose()
        {
            Stop();
            tokenSource?.Dispose();
        }

        private async Task PollingTask()
        {
            var previousIsConnected = IsConnected;
            var previousState = controller.GetState();
            var previousBatteryLevel = BatteryLevel;

            while (!token.IsCancellationRequested)
            {
                try
                {
                    if (previousIsConnected != IsConnected)
                    {
                        ConnectionChanged?.Invoke(this, new XControllerEventArgs(controller));
                    }

                    previousIsConnected = IsConnected;

                    if (!IsConnected)
                    {
                        await Task.Delay(IntervalOnDisconnected, token);

                        continue;
                    }

                    if (previousBatteryLevel != BatteryLevel)
                    {
                        BatteryLevelChanged?.Invoke(this, new XControllerBatteryLevelChangedEventArgs(BatteryType, previousBatteryLevel, BatteryLevel));
                    }

                    previousBatteryLevel = BatteryLevel;

                    var state = controller.GetState();

                    if (state.PacketNumber != previousState.PacketNumber)
                    {
                        var buttons = state.Gamepad.Buttons;
                        var previousButtons = previousState.Gamepad.Buttons;
                        var buttonChanges = buttons ^ previousButtons;
                        var buttonDowns = buttonChanges & buttons;
                        var buttonUps = buttonChanges & previousButtons;

                        if (buttonDowns > 0)
                        {
                            ButtonDown?.Invoke(this, new XControllerButtonEventArgs(buttonDowns));
                        }
                        if (buttonUps > 0)
                        {
                            ButtonUp?.Invoke(this, new XControllerButtonEventArgs(buttonUps));
                        }

                        StateChanged?.Invoke(this, new XControllerStateChangedEventArgs(previousState, state));
                    }

                    previousState = state;
                    Elapsed?.Invoke(this, new XControllerEventArgs(controller));

                    await Task.Delay(Interval, token);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);

                    if (e is SharpDXException)
                    {
                        continue;
                    }

                    throw;
                }
            }
        }
    }
}
