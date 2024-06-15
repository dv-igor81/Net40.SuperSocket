using System.IO.Ports.Net40;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SuperSocket;
using SuperSocket.ProtoBase;
using SuperSocket.SerialIO;
using SuperSocket.Server;
using SuperSocket.Server.Host;

namespace Net40.EchoServer.SerialIO
{
    internal class Program
    {
        public static async Task  Main(string[] args)
        {
            var host = SuperSocketHostBuilder
                .Create<TextPackageInfo, LinePipelineFilter>(args)
                .UsePackageHandler(async (session, package) =>
                {
                    await session.SendAsync(Encoding.UTF8.GetBytes(package.Text + "\r\n"));
                })
                /*.ConfigureSuperSocket(options =>
                {
                    options.Name = "SIOServer";
                    options.AddListener(
                        new SerialIOListenOptions()
                        {
                            PortName = "COM3", // serial port name
                            BaudRate = 115200, // baudRate of the serial port
                            Parity = Parity.None,
                            StopBits = StopBits.One,
                            Databits = 8 // value limit 5 to 8
                        }
                    );
                })*/
                .UseSerialIO()
                .ConfigureLogging((hostCtx, loggingBuilder) =>
                {
                    loggingBuilder.AddConsole();
                })
                .Build();
            await host.RunAsync();
        }
    }
}