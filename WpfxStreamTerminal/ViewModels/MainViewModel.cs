using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows.Input;
using WpfxStreamTerminal.Services;

namespace WpfxStreamTerminal.ViewModels;

public class MainViewModel : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;

    private IStreamTransport? _transport;

    private ProtocolType _protocol = ProtocolType.Tcp;
    private string _host = "127.0.0.1";
    private int _port = 5000;
    private string _serialPort = "COM4";
    private int _baudRate = 9600;
    private bool _dtrEnable = false;
    private string _connectionStatusText = "DISCONNECTED";
    private string _line = string.Empty;
    private string _lineSent = string.Empty;
    private string _lineReceived = string.Empty;
    private bool _isConnected;
    private bool _keepText = false;
    private bool _autoScroll = true;

    public MainViewModel()
    {
        StartStreamCommand = new AsyncCommand(
           execute: async () => await StartStreamAsync(),
           onError: ex => System.Diagnostics.Debug.WriteLine($"Command error: {ex.Message}")
        );

        StopStreamCommand = new AsyncCommand(
           execute: async () => await StopStreamAsync(),
           onError: ex => System.Diagnostics.Debug.WriteLine($"Command error: {ex.Message}")
        );

        SendLineCommand = new AsyncCommand(
            execute: async () => await SendLineAsync(),
            onError: ex => System.Diagnostics.Debug.WriteLine($"Command error: {ex.Message}")
        );
    }
       
    private async Task SendAsync(string text)
    {
        try
        {
            if (_transport == null) return;

            byte[] bytes = Encoding.UTF8.GetBytes(text + "\r");

            await _transport.Stream.WriteAsync(bytes, 0, bytes.Length);  

            LineSent = $"TX >> {text}";
        }
        catch (Exception ex) // OperationCanceledException, IOException, and general exceptions
        {
            System.Diagnostics.Debug.WriteLine(ex.Message);
        }
    }
       
    private async Task ReceiveAsync(IStreamTransport transport)
    {
        byte[] buffer = new byte[1024];
        Utf8StringBuilder utf8StringBuilder = new Utf8StringBuilder(buffer.Length);
        int bytesRead;

        while (true)
        {
            try
            {
                bytesRead = await transport.Stream.ReadAsync(buffer, 0, buffer.Length);
            }
            catch (Exception ex) // OperationCanceledException, IOException, and general exceptions
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
                break;
            }

            if (bytesRead == 0) break; // EOF or connection closed

            utf8StringBuilder.Append(buffer, bytesRead);

            int index = utf8StringBuilder.FindLineTerminator('\n');

            while (index >= 0)
            {
                string line = utf8StringBuilder.ToString(0, index).Trim('\r');

                utf8StringBuilder.Remove(0, index + 1);

                LineReceived = line;

                index = utf8StringBuilder.FindLineTerminator('\n');
            }
        }

        // If server closes connection, clean-up
        IsConnected = false;
        ConnectionStatusText = "DISCONNECTED";

        try
        {
            transport.Dispose();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine(ex.Message);
        }
    }

    //------------------------------------------------------------------------------------------------------------
    // Properties

    public ProtocolType Protocol
    {
        get => _protocol;

        set
        {
            _protocol = value;
            OnPropertyChanged();
        }
    }

    public string Host
    {
        get => _host;

        set
        {
            if (_host == value) return;
            _host = value;
            OnPropertyChanged();
        }
    }

    public int Port
    {
        get => _port;

        set
        {
            if (_port == value) return;
            _port = value;
            OnPropertyChanged();
        }
    }

    public string SerialPort
    {
        get => _serialPort;

        set
        {
            if (_serialPort == value) return;
            _serialPort = value;
            OnPropertyChanged();
        }
    }

    public int BaudRate
    {
        get => _baudRate;

        set
        {
            if (_baudRate == value) return;
            _baudRate = value;
            OnPropertyChanged();
        }
    }

    public bool DtrEnable
    {
        get => _dtrEnable;

        set
        {
            if (_dtrEnable == value) return;
            _dtrEnable = value;
            OnPropertyChanged();
        }
    }

    public bool IsConnected
    {
        get => _isConnected;
        
        private set
        {
            if (_isConnected == value) return;
            _isConnected = value;
            OnPropertyChanged();
        }
    }

    public string ConnectionStatusText
    {
        get => _connectionStatusText;
        
        private set
        {
            if (_connectionStatusText == value) return;
            _connectionStatusText = value;
            OnPropertyChanged();
        }
    }

    public string Line
    {
        get => _line;

        set
        {
            if (_line == value) return;
            _line = value;
            OnPropertyChanged();
        }
    }

    public string LineSent
    {
        get => _lineSent;
        
        private set
        {
            _lineSent = value;
            OnPropertyChanged();
        }
    }

    public string LineReceived
    {
        get => _lineReceived;
        
        private set
        {
            _lineReceived = value;
            OnPropertyChanged();
        }
    }

    public bool KeepText
    {
        get => _keepText;
        
        set
        {
            if (_keepText == value) return;
            _keepText = value;
            OnPropertyChanged();
        }
    }

    public bool AutoScroll
    {
        get => _autoScroll;
        
        set
        {
            if (_autoScroll == value) return;
            _autoScroll = value;
            OnPropertyChanged();
        }
    }

    public bool IsStreamConnecting
    {
        get
        {
            bool canExecute = StartStreamCommand.CanExecute(null);

            return !canExecute;
        }
    }


    //---------------------------------------------
    // COMMANDS

    public ICommand StartStreamCommand { get; }

    public ICommand StopStreamCommand { get; }
    
    public ICommand SendLineCommand { get; }


    //---------------------------------------------
    // COMMAND METHODS
      
    private async Task StartStreamAsync()
    {
        if (IsConnected) return;

        IStreamClient client;
        string connectedStatus;
        string failureStatus;

        switch (Protocol)
        {
            case ProtocolType.Serial:
                client = new SerialStreamClient(SerialPort, BaudRate, DtrEnable);
                connectedStatus = $"CONNECTED SERIAL - {SerialPort}";
                failureStatus = $"Failed to connect {SerialPort}";
                break;

            case ProtocolType.Tcp:
                client = new TcpStreamClient(Host, Port);
                connectedStatus = $"CONNECTED TCP - {Host}:{Port}";
                failureStatus = $"Failed to connect {Host}:{Port}";
                break;

            default:
                ConnectionStatusText = "Unsupported protocol";
                return;
        }

        IStreamTransport? transport;

        try
        {
            transport = await client.ConnectAsync();
        }
        catch
        {
            ConnectionStatusText = failureStatus;
            return;
        }

        if (transport == null) return;

        _transport = transport;
        ConnectionStatusText = connectedStatus;
        IsConnected = true;

        _ = Task.Run(() => ReceiveAsync(transport));
    }

    private Task StopStreamAsync()
    {
        if (IsStreamConnecting)
        {
            return Task.CompletedTask;
        }

        try
        {
            _transport?.Dispose();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine(ex.Message);
        }
        finally
        {
            _transport = null;
            IsConnected = false;
            ConnectionStatusText = "DISCONNECTED";
        }

        return Task.CompletedTask;
    }

    private async Task SendLineAsync()
    {
        if (IsConnected == false) return;

        await SendAsync(Line);

        if (KeepText == false)
        {
            Line = string.Empty;
        }
    }


    //------------------------------------------------------------------------------------------------------------
    // Events

    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
