using System.IO.Ports;

namespace SerialStreamShell.Services;

public sealed class SerialStreamServer : IStreamServer
{
    private readonly string _portName;
    private readonly int _baudRate;

    public SerialStreamServer(string portName, int baudRate = 9600)
    {
        _portName = portName;
        _baudRate = baudRate;
    }

    public Task<IStreamTransport?> AcceptAsync()
    {     
        var serialPort = new SerialPort(_portName, _baudRate)
        {
            ReadTimeout = -1,
            WriteTimeout = -1,
            Parity = Parity.None,
            DataBits = 8,
            StopBits = StopBits.One
        };

        try
        {
            serialPort.Open();

            var transport = new SerialStreamTransport(serialPort);

            return Task.FromResult<IStreamTransport?>(transport);
        }
        catch
        {
            serialPort.Dispose();
            throw;
        }
    }
}
