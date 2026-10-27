using System.Net;
using System.Net.Sockets;

namespace TcpStreamShell.Services;

public class TcpStreamServer : IStreamServer, IDisposable
{
    private readonly TcpListener _listener;
    private readonly CancellationTokenSource _cts = new CancellationTokenSource();

    public TcpStreamServer(IPAddress address, int port)
    {
        _listener = new TcpListener(address, port);
        _listener.Start();
    }

    public async Task<IStreamTransport?> AcceptAsync()
    {
        try
        {
            var client = await _listener.AcceptTcpClientAsync(_cts.Token);

            var transport = new TcpStreamTransport(client);

            return transport;
        }
        catch (OperationCanceledException)
        {
            return null;
        }
        catch (ObjectDisposedException)
        {
            return null;
        }
    }

    public void Dispose()
    {
        _cts.Cancel();
        _listener.Stop();
    }
}
