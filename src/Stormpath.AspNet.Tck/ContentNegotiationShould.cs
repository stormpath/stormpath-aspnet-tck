using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using FluentAssertions;
using Xunit;

namespace Stormpath.AspNet.Tck
{
    [Collection(nameof(IntegrationTestCollection))]
    public class ContentNegotiationShould
    {
        private readonly StandaloneTestFixture _fixture;

        public ContentNegotiationShould(StandaloneTestFixture fixture)
        {
            _fixture = fixture;
        }

        /// <summary>
        /// A real-world Accept header (from Chrome): text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,*/*;q=0.8
        /// </summary>
        /// <returns>The asynchronous test.</returns>
        [Fact]
        public async Task ReturnNullForUnauthenticatedRequests()
        {
            // Arrange
            var client = new TestHttpClient(_fixture);

            var request = new HttpRequestMessage(HttpMethod.Get, "/login");
            request.Headers.Add("Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,*/*;q=0.8");

            // Act
            var response = await client.SendAsync(request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            response.Content.Headers.ContentType.MediaType.Should().Be("text/html");
        }
    }
}
