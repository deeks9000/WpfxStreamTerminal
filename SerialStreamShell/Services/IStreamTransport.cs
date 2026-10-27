namespace SerialStreamShell.Services;

public interface IStreamTransport : IDisposable 
{ 
    Stream Stream { get; } 
}
