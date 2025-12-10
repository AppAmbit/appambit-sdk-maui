namespace AppAmbit.Services.Interfaces;

public interface IEndpoint
{
    string Url { get; set; }
    
    string BaseUrl { get; set; }
    
    object Payload { get; }
    
    HttpMethodEnum Method { get; }

    Dictionary<string, string> CustomHeader { get; set; }
    
    bool SkipAuthorization { get; set; }
}

public enum HttpMethodEnum
{
    Get, Post, Put, Delete, Patch
}
