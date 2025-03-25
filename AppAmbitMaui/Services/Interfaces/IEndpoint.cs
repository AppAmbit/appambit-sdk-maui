namespace AppAmbit.Services.Interfaces;

internal interface IEndpoint
{
    string Url { get; set; }
    
    string BaseUrl { get; set; }
    
    object Payload { get; }
    
    HttpMethodEnum Method { get; }

    Dictionary<string, string> CustomHeader { get; set; }
    
    bool SkipAuthorization { get; set; }
}

internal enum HttpMethodEnum
{
    Get, Post, Put, Delete, Patch
}