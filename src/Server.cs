using System.Net;
using System.Net.Sockets;
using System.Text;

// You can use print statements as follows for debugging, they'll be visible when running tests.
Console.WriteLine("Logs from your program will appear here!");

var server = new TcpListener(IPAddress.Any, 4221);

try
{
    //At this point the server start listenning for connections and add them in a dedicated queue
    server.Start();

    //Loop for handling connections
    while (true)
    {
        //This waits for a pending client connection
        //and the returned socket allows us to communicate with the client
        var socket = server.AcceptSocket();

        Console.WriteLine($"Connection established with a client");

        var readBuffer = new byte[1024];
        
        var receivedBufferSize = socket.Receive(readBuffer, SocketFlags.None);

        var receivedMessage = Encoding.UTF8.GetString(readBuffer, 0, receivedBufferSize);
        
        Console.WriteLine($"Received request: {receivedMessage}");

        var requestTarget = ParseRequestTarget(receivedMessage);

        var responseMessage = requestTarget == "/" ? "HTTP/1.1 200 OK\r\n\r\n" : "HTTP/1.1 404 NotFound\r\n\r\n";
        
        var encodedResponse = Encoding.UTF8.GetBytes(responseMessage);

        var result = socket.Send(encodedResponse);

        if (result == encodedResponse.Length) Console.WriteLine("Response sent successfully");

        socket.Close();
    }
}
catch (Exception e)
{
    Console.WriteLine(e);
}
finally
{
    server.Stop();
}

string? ParseRequestTarget(string s)
{
    var endOfRequestLineIndex = s.IndexOf("\r\n", StringComparison.Ordinal);

    if (endOfRequestLineIndex == -1) return null;

    var requestLine = s[..endOfRequestLineIndex];

    var requestTarget1 = requestLine.Split(' ')[1];
    return requestTarget1;
}
