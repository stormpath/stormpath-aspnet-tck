using System.Net.Http;

namespace Stormpath.AspNet.Tck
{
    public class TestHttpClient : HttpClient
    {
        public TestHttpClient(StandaloneTestFixture fixture)
            : base(CreateHandler())
        {
            BaseAddress = fixture.BaseUri;
        }

        private static HttpMessageHandler CreateHandler() => 
            new HttpClientHandler()
        {
            AllowAutoRedirect = false
        };
    }
}
