using System;
using System.Threading;
using WindowsDesktop;
using Desktopswitch;
using Grpc.Core;
using NLog;

namespace VirtualDesktopSwitchServer
{
    public class Program
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        [STAThread]
        public static void Main(string[] args)
        {
            SynchronizationContext.SetSynchronizationContext(new SynchronizationContext());

            try
            {
                RunServer();
            }
            catch (Exception ex)
            {
                Logger.Fatal(ex);
            }
        }

        private static void RunServer()
        {
            VirtualDesktop.Configure();

            var serverService = new DesktopSwitchServer();

            var server = new Server
            {
                Services = { DesktopSwither.BindService(serverService) },
                Ports = { new ServerPort("localhost", ApiConstants.PortNumber, ServerCredentials.Insecure) }
            };

            var serverCancelToken = serverService.GetCancellationToken();

            server.Start();

            Logger.Info($"DesktopSwitch gRPC listening on port {ApiConstants.PortNumber}");

            serverCancelToken.WaitHandle.WaitOne(); // Block until server cancelled

            Logger.Info("Server shutting down");

            // ReSharper disable once MethodSupportsCancellation
            server.ShutdownAsync().Wait();
        }
    }
}
