namespace Shared.Models.Endpoints.Base;

public class BaseEndpoint : IEndpoint
{
    public string Url { get; set; }
    
    public string BaseUrl { get; set; } = "https://appambitv2-restless-morning-3748.fly.dev/api";
    
    public bool SkipAuthorization { get; set;  } = false;
    
    public object Payload { get; set; } = null;
    
    public Dictionary<string, string> CustomHeader { get; set; } = null;
    
    public HttpMethodEnum Method { get; set; }
}