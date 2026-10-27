namespace TcpStreamShell.Services;

public interface IStreamServer 
{ 
    Task<IStreamTransport?> AcceptAsync();
}
