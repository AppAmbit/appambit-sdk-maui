using System;
using Kava.API;

namespace KavaupMaui.Endpoints
{
	public class GoogleEndpoint : IWebAPIEndpoint
    {
        public string Url => "/v1beta/accountSummaries";

        public string DomainUrl => "https://analyticsadmin.googleapis.com";

        public HttpVerb HttpVerb => HttpVerb.Get;

        public Dictionary<string, string> RequestHeader => null;

        public object Payload { get; set; }
	}
}

