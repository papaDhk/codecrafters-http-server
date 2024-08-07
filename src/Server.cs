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

        //This handle the request concurrently
        Task.Run(() =>
        {
            Console.WriteLine($"Connection established with a client");

            var readBuffer = new byte[1024];

            var receivedBufferSize = socket.Receive(readBuffer, SocketFlags.None);

            var receivedMessage = Encoding.UTF8.GetString(readBuffer, 0, receivedBufferSize);

            Console.WriteLine($"Received request: {receivedMessage}");

            var responseMessage = HandleRequest(receivedMessage);
            
            var encodedResponse = Encoding.UTF8.GetBytes(responseMessage);

            var result = socket.Send(encodedResponse);

            if (result == encodedResponse.Length) Console.WriteLine($"Response sent successfully: {responseMessage}");

            socket.Close();
        });
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

string HandleRequest(string s)
{
    var incomingHttpRequest = IncomingHttpRequest.Parse(s);
    
    switch (incomingHttpRequest)
    {
        case { Target: "/" }:
            return $"HTTP/1.1 200 OK{Constants.ClrfSeparator}{Constants.ClrfSeparator}";
        case { Target: "/user-agent" }:
        {
            incomingHttpRequest.Headers.TryGetValue(Constants.UserAgentHeaderName, out var userAgent);
            return 
                $"HTTP/1.1 200 OK{Constants.ClrfSeparator}Content-Type: text/plain{Constants.ClrfSeparator}Content-Length: {userAgent?.Length}{Constants.ClrfSeparator}{Constants.ClrfSeparator}{userAgent}";
        }
                
        default:
        {
            if (incomingHttpRequest.Target.StartsWith("/echo/"))
            {
                var endpointParameter = incomingHttpRequest.Target.Split('/')[2];
                return 
                    $"HTTP/1.1 200 OK{Constants.ClrfSeparator}" +
                    $"Content-Type: text/plain{Constants.ClrfSeparator}" +
                    $"Content-Length: {endpointParameter.Length}{Constants.ClrfSeparator}" +
                    $"{Constants.ClrfSeparator}{endpointParameter}";
            }

            if(incomingHttpRequest.Target.StartsWith("/files/"))
            {
                var directory = args.Length >= 2 ? args[1] : string.Empty;
                Console.WriteLine("The director is: " + directory);

                var fileName = incomingHttpRequest.Target.Split('/')[2];
                var filePath = $"{directory}{fileName}";
                
                Console.WriteLine("The full path is: " + filePath);

                switch (incomingHttpRequest.HttpMethod)
                {
                    case Constants.HttpGetMethod when File.Exists(filePath):
                    {
                        var contentBytes = File.ReadAllBytes(filePath);
                        var content = Encoding.UTF8.GetString(contentBytes);
                        return $"HTTP/1.1 200 OK{Constants.ClrfSeparator}Content-Type: application/octet-stream" +
                               $"{Constants.ClrfSeparator}Content-Length: {contentBytes.Length}" +
                               $"{Constants.ClrfSeparator}{Constants.ClrfSeparator}" +
                               $"{content}";
                    }
                    case Constants.HttpPostMethod:
                        using (var streamWriter = File.CreateText(filePath))
                        {
                            streamWriter.Write(incomingHttpRequest.Body);
                        }
                        return $"HTTP/1.1 201 Created{Constants.ClrfSeparator}{Constants.ClrfSeparator}";
                }
            }
            
            return $"HTTP/1.1 404 Not Found{Constants.ClrfSeparator}{Constants.ClrfSeparator}";
            
        }
    }
}


