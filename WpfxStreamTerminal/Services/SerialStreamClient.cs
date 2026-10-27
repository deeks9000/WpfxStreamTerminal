using System.IO.Ports;

namespace WpfxStreamTerminal.Services;

public class SerialStreamClient : IStreamClient
{
    private readonly string _portName;
    private readonly int _baudRate;
    private readonly bool _dtrEnable;

    public SerialStreamClient(string portName, int baudRate, bool dtrEnable = false)
    {
        _portName = portName;
        _baudRate = baudRate;
        _dtrEnable = dtrEnable;
    }

    public Task<IStreamTransport?> ConnectAsync()
    {
        return Task.Run<IStreamTransport?>(() =>
        {
            SerialPort? serialPort = null;

            try
            {
                serialPort = new SerialPort(_portName, _baudRate)
                {
                    ReadTimeout = -1,
                    WriteTimeout = -1,
                    Parity = Parity.None,
                    StopBits = StopBits.One,
                    DataBits = 8,
                    DtrEnable = _dtrEnable
                };

                serialPort.Open();

                var transport = new SerialStreamTransport(serialPort);

                return transport;
            }
            catch
            {
                serialPort?.Dispose();
                throw;
            }
        });
    }
}
