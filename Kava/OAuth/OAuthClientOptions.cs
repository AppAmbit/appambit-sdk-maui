namespace Kava.Oauth;

public class OAuthClientOptions
{
    public OAuthClientOptions()
    {
        Scope = "openid";
        RedirectUri = "myapp://callback";
        Browser = new WebBrowserAuthenticator();
    }

    public string Authority { get; set; }

    public string ClientId { get; set; }

    public string RedirectUri { get; set; }

    public string Scope { get; set; }

    public IdentityModel.OidcClient.Browser.IBrowser Browser { get; set; }

    public string IssuerName { get; set; }

    public string AuthorizeEndpoint { get; set; }

    public string TokenEndpoint { get; set; }

    
}
