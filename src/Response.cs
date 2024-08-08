using System.Text;

namespace pmn_http_server;

public class HttpResponse
{
    public Dictionary<string, string> Headers { get; set; } = new();
    public string HttpVersion { get; set; }
    public HttpStatus HttpStatus { get; set; }
    public string Body { get; set; }
    public CompressionType? CompressionType { get; set; }

    public override string ToString()
    {
        var stringBuilder = new StringBuilder();
        var statusString = HttpStatus == HttpStatus.NotFound ? "Not Found" : HttpStatus.ToString();
        stringBuilder.Append($"{HttpVersion} {(int)HttpStatus} {statusString}");
        stringBuilder.Append(Constants.ClrfSeparator); //Close the request line section
        stringBuilder.AppendJoin(Constants.ClrfSeparator, Headers.Select(kv => $"{kv.Key}: {kv.Value}"));
        if(Headers.Any())stringBuilder.Append(Constants.ClrfSeparator); //CLRF separator for the last header
        stringBuilder.Append(Constants.ClrfSeparator); //Close the header section
        stringBuilder.Append(Body);
        return stringBuilder.ToString();
    }
}

public enum HttpStatus
{
    OK = 200,
    Created = 201,
    NotFound = 404,
    BadRequest = 400,
}

public enum CompressionType
{
    none = 0,
    gzip = 1,
    deflate = 2
}