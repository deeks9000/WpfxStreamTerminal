using System.IO;

namespace WpfxStreamTerminal.Services;

public interface IStreamTransport : IDisposable
{
    Stream Stream { get; }
}
