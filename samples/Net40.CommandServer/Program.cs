using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SuperSocket.Server;
using SuperSocket.Server.Host;
using SuperSocket.Command;
using SuperSocket.ProtoBase;

namespace CommandServer
{
    class Program
    {
        static IHostBuilder CreateSocketServerBuilder(string[] args)
        {
            return SuperSocketHostBuilder.Create<StringPackageInfo, CommandLinePipelineFilter>(args)
                .UseCommand((commandOptions) =>
                {
                    // register commands one by one
                    commandOptions.AddCommand<ADD>();
                    commandOptions.AddCommand<MULT>();
                    commandOptions.AddCommand<SUB>();

                    // register all commands in one aassembly
                    //commandOptions.AddCommandAssembly(typeof(SUB).GetTypeInfo().Assembly);
                })
                .ConfigureAppConfiguration((hostCtx, configApp) =>
                {
                    configApp.AddInMemoryCollection(new Dictionary<string, string>
                    {
                        { "serverOptions:name", "TestServer" },
                        { "serverOptions:listeners:0:ip", "Any" },
                        { "serverOptions:listeners:0:port", "4040" }
                    });
                })
                .ConfigureLogging((hostCtx, loggingBuilder) => { loggingBuilder.AddConsole(); });
        }

        static async Task Main(string[] args)
        {
            // Отправьте add 1 5<0D><0A> или (add - маленькими буквами) // результат - 6
            // Отправьте MULT 5 5<0D><0A> или (MULT - БОЛЬШИМИ БУКВАМИ) // результат - 25
            // Отправьте SUB 99 5 33 6<0D><0A>. (SUB - БОЛЬШИМИ БУКВАМИ) // результат - 55
            var host = CreateSocketServerBuilder(args).Build();
            await host.RunAsync();
        }
    }
}