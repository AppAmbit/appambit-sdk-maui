namespace AppAmbit.Services
{
    public class LoggingHandler : DelegatingHandler
    {
        public LoggingHandler(HttpMessageHandler innerHandler)
            : base(innerHandler)
        { }
        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            if (request.Content != null)
            {
                var reqBody = await request.Content.ReadAsStringAsync(cancellationToken);
                Console.WriteLine(reqBody);
            }

            var response = await base.SendAsync(request, cancellationToken);
            await APIService.CalculateRequestSize(request);

            if (response.Content != null)
            {
                var respBody = await response.Content.ReadAsStringAsync(cancellationToken);
                Console.WriteLine(respBody);
            }
            return response;
        }
    }
}