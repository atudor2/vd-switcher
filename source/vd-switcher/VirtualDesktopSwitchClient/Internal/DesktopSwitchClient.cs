using System;
using Desktopswitch;
using Grpc.Core;
using NLog;
using VirtualDesktopCommon;

namespace VirtualDesktopSwitchClient.Internal
{
    internal class DesktopSwitchClient : IDesktopSwitchClient
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private readonly DesktopSwither.DesktopSwitherClient _client;
        private readonly Channel _channel;

        public DesktopSwitchClient()
        {
            _channel = new Channel("127.0.0.1", ApiConstants.PortNumber, ChannelCredentials.Insecure);
            _client = new DesktopSwither.DesktopSwitherClient(_channel);
            Logger.Trace($"Client Connection to channel {_channel.Target}");
        }

        public bool SwitchDesktopLeft() => SwitchDesktop(SwitchOperationType.Left);

        public bool SwitchDesktopRight() => SwitchDesktop(SwitchOperationType.Right);

        public bool CycleDesktopLeft() => SwitchDesktop(SwitchOperationType.CycleLeft);

        public bool CycleDesktopRight() => SwitchDesktop(SwitchOperationType.CycleRight);

        public bool IsWindowOnCurrentDesktop(IntPtr hWnd)
        {
            return TraceRpcCall(() => _client.IsHWndOnDesktop(new WindowHandle() { HWnd = hWnd.ToInt64() }).UnwrapBoolResult(), "IsWindowOnCurrentDesktop", hWnd);
        }

        private bool SwitchDesktop(SwitchOperationType operationType)
        {
            return TraceRpcCall(() => _client.SwitchDesktop(new SwitchOperation() { Operation = operationType }).UnwrapBoolResult(), "SwitchDesktop", operationType);
        }

        private static T TraceRpcCall<T>(Func<T> func, string method, params object[] args)
        {
            Logger.Trace(() => $"Begin gRPC Call => {method}({ string.Join(", ", args) })");
            var result = func();
            Logger.Trace(() => $"End gRPC Call => {method}() => {result}");

            return result;
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
