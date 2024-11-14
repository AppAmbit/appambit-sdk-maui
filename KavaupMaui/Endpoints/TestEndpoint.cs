using System;
using System.Collections.Generic;
using Kava.API;

namespace KavaupMaui.Endpoints
{
    public class TestEndpoint : IWebAPIEndpoint
    {
        public string Url => "/api/version";

        public string DomainUrl => "https://staging-strapi-tb12.kavaup.io";

        public HttpVerb HttpVerb => HttpVerb.Get;

        public Dictionary<string, string> RequestHeader => null;

        public object Payload { get; set; }
    }
}

