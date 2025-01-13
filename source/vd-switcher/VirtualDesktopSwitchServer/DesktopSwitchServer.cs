using System;
using System.Threading;
using System.Threading.Tasks;
using WindowsDesktop;
using Desktopswitch;
using Grpc.Core;
using NLog;
using VirtualDesktopCommon;

namespace VirtualDesktopSwitchServer
{
    internal class DesktopSwitchServer : DesktopSwither.DesktopSwitherBase
    {
        private readonly CancellationTokenSource _cancellationTokenSrc;
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public DesktopSwitchServer()
        {
            _cancellationTokenSrc = new CancellationTokenSource();
        }

        public CancellationToken GetCancellationToken()
        {
            return _cancellationTokenSrc.Token;
        }

        public override Task<BoolResult> PingServer(Empty request, ServerCallContext context)
        {
            Logger.Debug(() => "Received request: PingServer()");
            return Task.FromResult(new BoolResult { Result = true });
        }

        public override Task<Empty> StopServer(Empty request, ServerCallContext context)
        {
            Logger.Debug(() => "Received request: StopServer()");

            _cancellationTokenSrc.Cancel();

            return Task.FromResult(new Empty());
        }

        public override Task<BoolResult> IsHWndOnDesktop(WindowHandle request, ServerCallContext context)
        {
            var hWnd = new IntPtr(request.HWnd);

            Logger.Debug(() => $"Received request: IsHWndOnDesktop({ hWnd.ToHexString() })");

            var result = VirtualDesktop.IsCurrentVirtualDesktop(hWnd);
            return Task.FromResult(new BoolResult { Result = result });
        }

        public override Task<BoolResult> SwitchDesktop(SwitchOperation operation, ServerCallContext context)
        {
            Logger.Debug(() => $"Received request: SwitchDesktop({ operation.Operation }). The Current Desktop is {VirtualDesktop.Current.Id}");

            switch (operation.Operation)
            {
                case SwitchOperationType.Left:
                    VirtualDesktop.Current.GetLeft()?.Switch();
                    break;
                case SwitchOperationType.Right:
                    VirtualDesktop.Current.GetRight()?.Switch();
                    break;
                case SwitchOperationType.CycleLeft:
                    CycleDesktop(VirtualDesktop.Current, d => d.GetLeft(), d => d.GetRight());
                    break;
                case SwitchOperationType.CycleRight:
                    CycleDesktop(VirtualDesktop.Current, d => d.GetRight(), d => d.GetLeft());
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            return Task.FromResult(new BoolResult { Result = true });
        }

        private static void CycleDesktop(VirtualDesktop desktop, Func<VirtualDesktop, VirtualDesktop> cycleFunc, Func<VirtualDesktop, VirtualDesktop> reverseCycleFunc)
        {
            var d = cycleFunc(desktop);
            if (d != null)
            {
                // Simple case - still desktops present to the side being cycled
                d.Switch();
                return;
            }

            // We need to go backwards to the otherside of the stack:
            // We are not using VirtualDesktop.GetDesktops() as unsure if the ordering is valid in the array
            var lastDesktop = desktop;
            var prevDesktop = reverseCycleFunc(lastDesktop);

            while (prevDesktop != null)
            {
                lastDesktop = prevDesktop;
                prevDesktop = reverseCycleFunc(lastDesktop);
            }
            lastDesktop.Switch();
        }
    }
}