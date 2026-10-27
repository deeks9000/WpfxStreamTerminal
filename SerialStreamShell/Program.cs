using SerialStreamShell.Services;
using System.Text;

namespace SerialStreamShell;

public class Program
{
    public static async Task Main()
    {
        Console.Title = "Serial Stream Shell (Server)";

        CommandShell.Initialize();

        string comPort = "COM5";
        int baudRate = 9600;

        var server = new SerialStreamServer(comPort, baudRate);

        Console.WriteLine("Serial Stream Shell (Server)");

        Console.WriteLine($"Opening {comPort} at {baudRate} baud...");

        try
        {
            IStreamTransport? transport = await server.AcceptAsync();

            if (transport == null) return;

            Console.WriteLine("Serial port opened");

            await ReceiveAsync(transport);
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error: {ex.Message}");
        }
    }

    private static async Task ReceiveAsync(IStreamTransport transport)
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
            catch (OperationCanceledException)
            {
                break;
            }
            catch (IOException)
            {
                break; // Connection closed
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                break;
            }

            if (bytesRead == 0) break; // EOF or connection closed

            utf8StringBuilder.Append(buffer, bytesRead);

            int index = utf8StringBuilder.FindLineTerminator('\r');

            while (index >= 0)
            {
                string line = utf8StringBuilder.ToString(0, index).Trim();

                utf8StringBuilder.Remove(0, index + 1);

                Console.WriteLine($"Received: {line}");

                string response = CommandShell.GetResponse(line);

                byte[] outBytes = Encoding.UTF8.GetBytes(response + "\r\n");

                try
                {
                    await transport.Stream.WriteAsync(outBytes, 0, outBytes.Length);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    break;
                }

                index = utf8StringBuilder.FindLineTerminator('\r');
            }
        }

        transport.Dispose();
    }
}
