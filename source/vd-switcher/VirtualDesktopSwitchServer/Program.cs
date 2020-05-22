using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
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
        public static async Task Main(string[] args)
        {
            TodoFixConcurrentDictionaryMissingTypeIssue();

            SynchronizationContext.SetSynchronizationContext(new SynchronizationContext());

            try
            {
                await RunServer();
            }
            catch (Exception ex)
            {
                Logger.Fatal(ex);
            }
        }

        private static async Task RunServer()
        {
            await VirtualDesktopProvider.Default.Initialize();

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

        private static void TodoFixConcurrentDictionaryMissingTypeIssue()
        {
            ///////////////////////////////////////////////////////////////////////////////////////////////////
            ////// TODO - Find out why a missing type for ConcurrentDictionary is thrown from VirtualDesktop
            ///////////////////////////////////////////////////////////////////////////////////////////////////
            var d = new ConcurrentDictionary<int, int>();
            d.AddOrUpdate(1, i => 1, (i, i1) => i);
        }
    }
}
