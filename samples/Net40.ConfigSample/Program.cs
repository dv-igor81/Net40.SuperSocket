using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SuperSocket.Http;
using SuperSocket.Server.Host;
using SuperSocket.ProtoBase;

namespace ConfigSample
{
    class Program
    {
        static async Task Main(string[] args)
        {               
            var host = SuperSocketHostBuilder.Create<HttpRequest, HttpPipelineFilter>(args)
                .UsePackageHandler(async (session, package) =>
                {
                    await session.SendAsync(Encoding.UTF8.GetBytes("OK"));
                    //await session.SendAsync(Encoding.UTF8.GetBytes(package.Text + "\r\n"));
                })
                .ConfigureLogging((hostCtx, loggingBuilder) =>
                {
                    // register your logging library here
                    loggingBuilder.AddConsole();
                }).Build();

            await host.RunAsync();
        }
    }
}