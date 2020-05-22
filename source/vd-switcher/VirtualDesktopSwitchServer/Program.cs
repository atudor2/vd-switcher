using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using WindowsDesktop;
using Desktopswitch;
using Grpc.Core;

namespace VirtualDesktopSwitchServer
{
    public class Program
    {
        private const int Port = 50051;

        public static async Task Main(string[] args)
        {
            TodoFixConcurrentDictionaryMissingTypeIssue();

            SynchronizationContext.SetSynchronizationContext(new SynchronizationContext());
            await VirtualDesktopProvider.Default.Initialize();

            Console.WriteLine($"ID: {VirtualDesktop.Current.Id}");

            var server = new Server
            {
                Services = { DesktopSwither.BindService(new DesktopSwitchServer()) },
                Ports = { new ServerPort("localhost", Port, ServerCredentials.Insecure) }
            };
            server.Start();

            Console.WriteLine("Switcher server listening on port " + Port);
            Console.WriteLine("Press any key to stop the server...");
            Console.ReadKey();

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
