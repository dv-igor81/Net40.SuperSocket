using Microsoft.Extensions.Logging;

namespace SuperSocket
{
    public interface ILoggerAccessor
    {
        ILogger Logger { get; }
    }
}