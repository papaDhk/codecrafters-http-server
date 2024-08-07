namespace pmn_http_server;

public class IncomingHttpRequest
{
    public Dictionary<string, string> Headers { get; set; } = new();
    public string HttpMethod { get; set; }
    public string Target { get; set; }
    public string HttpVersion { get; set; }
    public string Body { get; set; }
}