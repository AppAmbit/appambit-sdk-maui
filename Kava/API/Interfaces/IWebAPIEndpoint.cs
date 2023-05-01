namespace Kava.API.Interfaces;

  public interface IWebAPIEndpoint
  {
    /// <summary>
    /// Gets the URL.
    /// </summary>
    /// <value>The URL.</value>
    string Url { get; }
    /// <summary>
    /// Gets the domain URL.
    /// </summary>
    /// <value>The domain URL.</value>
    string DomainUrl { get; }
    /// <summary>
    /// Gets the http verb.
    /// </summary>
    /// <value>The http verb.</value>
    HttpVerb HttpVerb { get; }
    /// <summary>
    /// Gets the request header.
    /// </summary>
    /// <value>The request header.</value>
    Dictionary<string, string> RequestHeader { get; }
    /// <summary>
    /// Gets the payload.
    /// </summary>
    /// <value>The payload.</value>
    object Payload { get; set; }
  }