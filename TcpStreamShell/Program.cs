using System.Net;
using System.Net.Sockets;
using System.Text;
using TcpStreamShell.Services;

namespace TcpStreamShell;

public class Program
{
    public static async Task Main()
    {
        Console.Title = "TCP Stream Shell (Server)";

        CommandShell.Initialize();

        var server = new TcpStreamServer(IPAddress.Loopback, 5000);

        Console.WriteLine("TCP Stream Shell (Server)");

        Console.WriteLine("Listening on 127.0.0.1:5000");

        while (true)
        {
            IStreamTransport? transport = await server.AcceptAsync();

            if (transport == null) break;

            _ = ReceiveAsync(transport);
        }

        server.Dispose();
    }

    private static async Task ReceiveAsync(IStreamTransport transport)
    {
        byte[] buffer = new byte[1024];
        Utf8StringBuilder utf8StringBuilder = new Utf8StringBuilder(buffer.Length);
        int bytesRead;

        int rnd = Random.Shared.Next();
        Console.WriteLine($"[{rnd}] Client connected");
                
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
            catch (IOException ex) when (ex.InnerException is SocketException se && se.SocketErrorCode == SocketError.ConnectionReset)
            {
                // Client closed the window with X button
                Console.WriteLine($"[{rnd}] Client forcibly closed connection");
                break;
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

                Console.WriteLine($"[{rnd}] Received: {line}");

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

        Console.WriteLine($"[{rnd}] Client disconnected.");
    }
}
