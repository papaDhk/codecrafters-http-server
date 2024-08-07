using System.Net;
using System.Net.Sockets;
using System.Text;
using pmn_http_server;

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

        var incomingHttpRequest = ParseRequestTarget(receivedMessage);

        string responseMessage;

        var responseMessage = requestTarget == "/" ? "HTTP/1.1 200 OK\r\n\r\n" : "HTTP/1.1 404 Not Found\r\n\r\n";
        
        var encodedResponse = Encoding.UTF8.GetBytes(responseMessage);

        var result = socket.Send(encodedResponse);

        if (result == encodedResponse.Length) Console.WriteLine($"Response sent successfully: {responseMessage}");

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

IncomingHttpRequest ParseRequestTarget(string requestString)
{
    var requestParts = requestString.Split("\r\n");


    var requestLine = requestParts.FirstOrDefault();
    //var requestBody = requestParts.LastOrDefault();
    

    var requestLineParts = requestLine?.Split(' ');

    return new IncomingHttpRequest
    {
        HttpMethod = requestLineParts?[0],
        Target = requestLineParts?[1],
        HttpVersion = requestLineParts?[2],
    };
}
