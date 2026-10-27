using System.Net.Sockets;

namespace WpfxStreamTerminal.Services;

public class TcpStreamClient : IStreamClient
{
    private readonly string _host;
    private readonly int _port;

    public TcpStreamClient(string host, int port)
    {
        _host = host;
        _port = port;
    }

    public async Task<IStreamTransport?> ConnectAsync()
    {
        var tcpClient = new TcpClient();

        try
        {
            await tcpClient.ConnectAsync(_host, _port);

            var transport = new TcpStreamTransport(tcpClient);

            return transport;
        }
        catch
        {
            tcpClient.Dispose();
            throw;
        }
    }
}
