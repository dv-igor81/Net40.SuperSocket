using System;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SuperSocket.Connection;
using SuperSocket.Server;
using SuperSocket.Server.Abstractions;
using SuperSocket.Server.Host;
using SuperSocket.ProtoBase;

namespace EchoServer
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var host = SuperSocketHostBuilder
                .Create<TextPackageInfo, LinePipelineFilter>(args)
                .UsePackageHandler(async (session, package) =>
                {
                    await session.SendAsync(Encoding.UTF8.GetBytes(package.Text + "\r\n"));
                })
                .ConfigureSuperSocket(options =>
                {
                    options.Name = "Echo Server";
                    options.AddListener(new ListenOptions
                        {
                            Ip = "Any",
                            Port = 4040
                        }
                    );
                })
                .ConfigureLogging((hostCtx, loggingBuilder) =>
                {
                    loggingBuilder.AddConsole();
                })
                .Build();

            await host.RunAsync();
        }
    }
}
