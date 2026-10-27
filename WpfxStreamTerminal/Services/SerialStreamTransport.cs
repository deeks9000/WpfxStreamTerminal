using System.IO;
using System.IO.Ports;

namespace WpfxStreamTerminal.Services;

public class SerialStreamTransport : IStreamTransport
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
            System.Diagnostics.Debug.WriteLine(ex.Message);
        }
    }
}