namespace SerialStreamShell.Services;

public interface IStreamServer 
{ 
    Task<IStreamTransport?> AcceptAsync();
}
