using System;
using System.Threading.Tasks;
using WindowsDesktop;
using Desktopswitch;
using Grpc.Core;
using VirtualDesktopCommon;

namespace VirtualDesktopSwitchServer
{
    internal class DesktopSwitchServer : DesktopSwither.DesktopSwitherBase
    {
        public override Task<BoolResult> IsHWndOnDesktop(WindowHandle request, ServerCallContext context)
        {
            var hWnd = new IntPtr(request.HWnd);

            Log(() => $"Received request for hWnd desktop check: { hWnd.ToHexString() }");

            var result = VirtualDesktopHelper.IsCurrentVirtualDesktop(hWnd);
            return Task.FromResult(new BoolResult { Result = result });
        }

        public override Task<BoolResult> SwitchDesktop(SwitchOperation operation, ServerCallContext context)
        {
            Log(() => $"Received switch operation: { operation.Operation }");

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

            return Task.FromResult(new BoolResult { Result = true});
        }
        private static void Log(Func<string> func)
        {
            Console.WriteLine(func());
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