namespace WpfxStreamTerminal.Services;

public interface IStreamClient
{
    Task<IStreamTransport?> ConnectAsync();
}
