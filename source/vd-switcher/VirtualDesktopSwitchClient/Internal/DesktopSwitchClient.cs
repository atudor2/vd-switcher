using System;
using Desktopswitch;
using Grpc.Core;
using VirtualDesktopCommon;

namespace VirtualDesktopSwitchClient.Internal
{
    internal class DesktopSwitchClient : IDesktopSwitchClient
    {
        private readonly DesktopSwither.DesktopSwitherClient _client;
        private readonly Channel _channel;

        public DesktopSwitchClient()
        {
            _channel = new Channel("127.0.0.1", ApiConstants.PortNumber, ChannelCredentials.Insecure);
            _client = new DesktopSwither.DesktopSwitherClient(_channel);
        }

        public bool SwitchDesktopLeft() => SwitchDesktop(SwitchOperationType.Left);

        public bool SwitchDesktopRight() => SwitchDesktop(SwitchOperationType.Right);

        public bool CycleDesktopLeft() => SwitchDesktop(SwitchOperationType.CycleLeft);

        public bool CycleDesktopRight() => SwitchDesktop(SwitchOperationType.CycleRight);

        public bool IsWindowOnCurrentDesktop(IntPtr hWnd)
        {
            return _client.IsHWndOnDesktop(new WindowHandle() { HWnd = hWnd.ToInt64() }).UnwrapBoolResult();
        }

        private bool SwitchDesktop(SwitchOperationType operationType)
        {
            return _client.SwitchDesktop(new SwitchOperation() { Operation = operationType }).UnwrapBoolResult();
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                _channel.ShutdownAsync().Wait();
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
