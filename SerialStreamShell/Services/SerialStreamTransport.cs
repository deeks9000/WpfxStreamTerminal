using System.IO.Ports;

namespace SerialStreamShell.Services;

public sealed class SerialStreamTransport : IStreamTransport
{
    private readonly SerialPort _serialPort;

    public Stream Stream { get; }

    public SerialStreamTransport(SerialPort serialPort)
    {
        _serialPort = serialPort;
        Stream = serialPort.BaseStream;
    }

    public void Dispose()
    {
        try
        {
            _serialPort.Dispose();
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
    }
}
