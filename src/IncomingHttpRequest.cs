namespace pmn_http_server;

public class IncomingHttpRequest
{
    public Dictionary<string, string> Headers { get; set; } = new();
    public string HttpMethod { get; set; }
    public string Target { get; set; }
    public string HttpVersion { get; set; }
    public string Body { get; set; }
    
    public static IncomingHttpRequest Parse(string requestString)
    {
        var requestParts = requestString.Split(Constants.ClrfSeparator);
        
        var requestLine = requestParts.FirstOrDefault();
        //var requestBody = requestParts.LastOrDefault();

        var requestLineParts = requestLine?.Split(' ');
        
        var incomingHttpRequest = new IncomingHttpRequest
        {
            HttpMethod = requestLineParts?[0],
            Target = requestLineParts?[1],
            HttpVersion = requestLineParts?[2],
        };
        
        var beginningOfHeaders = requestString.IndexOf(Constants.ClrfSeparator, StringComparison.Ordinal) + Constants.ClrfSeparator.Length;
        var endOfHeaders = requestString.IndexOf(Constants.ClrfSeparator+Constants.ClrfSeparator, StringComparison.Ordinal);

        if (endOfHeaders > beginningOfHeaders)
        {
            var headers = requestString[beginningOfHeaders..endOfHeaders].Split(Constants.ClrfSeparator);

            foreach (var header in headers)
            {
                var headerNameEndPosition = header.IndexOf(':');
                var headerName = header[..headerNameEndPosition];
                var headerValue = header[(headerNameEndPosition + 1)..];
                incomingHttpRequest.Headers[headerName] = headerValue.Trim();
            }
        }

        incomingHttpRequest.Body = requestString[(endOfHeaders + Constants.ClrfSeparator.Length*2)..];
        
        return incomingHttpRequest;
    }
}