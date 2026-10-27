using System.IO;
using System.Net.Sockets;

namespace WpfxStreamTerminal.Services;

public class TcpStreamTransport : IStreamTransport
{
    private readonly TcpClient _client;

    public Stream Stream { get; }

    public TcpStreamTransport(TcpClient client)
    {
        _client = client;
        Stream = client.GetStream();
    }

    public void Dispose()
    {
        try
        {
            _client.Dispose();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine(ex.Message);
        }
    }
}