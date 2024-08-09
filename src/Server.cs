using System.IO.Compression;
using System.Net;
using System.Net.Sockets;
using System.Security.AccessControl;
using System.Text;
using pmn_http_server;

// You can use print statements as follows for debugging, they'll be visible when running tests.
Console.WriteLine("Logs from your program will appear here!");
HashSet<CompressionType> SupportedCompressions = [CompressionType.gzip, CompressionType.deflate];

var server = new TcpListener(IPAddress.Any, 4221);

try
{
    //At this point the server start listening for connections and add them in a dedicated queue
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
            

            var result = socket.Send(responseMessage.ToBytes());

            if (result > 0) Console.WriteLine($"Response sent successfully: {responseMessage}");
            
            Array.Clear(readBuffer);
            
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

return;

HttpResponse HandleRequest(string requestMessage)
{
    var incomingHttpRequest = IncomingHttpRequest.Parse(requestMessage);

    Enum.TryParse(incomingHttpRequest.AcceptEncodings.Intersect(SupportedCompressions.Select(c => c.ToString()))
                                .FirstOrDefault(), out CompressionType compressionType);
    var response = new HttpResponse
    {
        HttpVersion = incomingHttpRequest.HttpVersion,
        CompressionType = compressionType
    };
    
    
    switch (incomingHttpRequest)
    {
        case { Target: "/" }:
            response.HttpStatus = HttpStatus.OK;
            TryCompressResponseBody(response, compressionType, string.Empty);
            return response;
        case { Target: "/user-agent" }:
        {
            incomingHttpRequest.Headers.TryGetValue(Constants.UserAgentHeaderName, out var userAgent);
            response.HttpStatus = HttpStatus.OK;
            TryCompressResponseBody(response, compressionType, userAgent);
            return response;
        }
                
        default:
        {
            if (incomingHttpRequest.Target.StartsWith("/echo/"))
            {
                var endpointParameter = incomingHttpRequest.Target.Split('/')[2];
                response.HttpStatus = HttpStatus.OK;
                TryCompressResponseBody(response, compressionType, endpointParameter);
                return response;
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
                        response.HttpStatus = HttpStatus.OK;
                        response.Headers.TryAdd("Content-Type", "application/octet-stream");
                        TryCompressResponseBody(response, compressionType, content);
                        return response;
                    }
                    case Constants.HttpPostMethod:
                    {
                        using (var streamWriter = File.CreateText(filePath))
                        {
                            streamWriter.Write(incomingHttpRequest.Body);
                        }
                        
                        response.HttpStatus = HttpStatus.Created;
                        return response;
                    }
                }
            }
            
            response.HttpStatus = HttpStatus.NotFound;
            return response;
        }
    }
}

void TryCompressResponseBody(HttpResponse response, CompressionType compressionType, string responseBody)
{
    if (compressionType != CompressionType.none)
    {
        using var compressedStream = new MemoryStream();
        var responseBodyBytes = Encoding.UTF8.GetBytes(responseBody);
        using var gZipStream = new GZipStream(compressedStream, CompressionMode.Compress);
        gZipStream.Write(responseBodyBytes);
        gZipStream.Close();
        response.Body = compressedStream.ToArray();
        response.Headers.TryAdd("Content-Type", "text/plain");
        response.Headers.TryAdd(Constants.ContentEncodingHeaderName, compressionType.ToString());
        response.Headers.TryAdd("Content-Length", $"{response.Body.Length}");
    }
    else
    {
        response.Headers.TryAdd("Content-Type", "text/plain");
        response.Headers.TryAdd("Content-Length", $"{responseBody.Length}");
        response.Body = Encoding.UTF8.GetBytes(responseBody);
    }
}





