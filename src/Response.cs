using System.Text;

namespace pmn_http_server;

public class HttpResponse
{
    public Dictionary<string, string> Headers { get; set; } = new();
    public string HttpVersion { get; set; }
    public HttpStatus HttpStatus { get; set; }
    public string Body { get; set; }

    public override string ToString()
    {
        var stringBuilder = new StringBuilder();
        var statusString = HttpStatus == HttpStatus.NotFound ? "Not Found" : HttpStatus.ToString();
        stringBuilder.Append($"{HttpVersion} {(int)HttpStatus} {statusString}");
        stringBuilder.Append(Constants.ClrfSeparator);
        stringBuilder.AppendJoin(Constants.ClrfSeparator, Headers.Select(kv => $"{kv.Key}: {kv.Value}"));
        stringBuilder.Append(Constants.ClrfSeparator);
        stringBuilder.Append(Body);
        return stringBuilder.ToString();
    }
}

public enum HttpStatus
{
    Ok = 200,
    Created = 201,
    NotFound = 404,
    BadRequest = 400,
}