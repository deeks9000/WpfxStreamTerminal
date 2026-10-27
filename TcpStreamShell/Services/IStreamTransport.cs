namespace TcpStreamShell.Services;

public interface IStreamTransport : IDisposable 
{ 
    Stream Stream { get; } 
}
