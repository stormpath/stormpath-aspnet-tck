using System.Net;
using System.Threading.Tasks;
using FluentAssertions;
using Xunit;

namespace Stormpath.AspNet.Tck
{
    [Collection(nameof(IntegrationTestCollection))]
    public class GetConfigShould
    {
        private readonly StandaloneTestFixture _fixture;

        public GetConfigShould(StandaloneTestFixture fixture)
        {
            _fixture = fixture;
        }

        [Fact]
        public async Task GetFromContext()
        {
            // Arrange
            var client = new TestHttpClient(_fixture);

            // Act
            var response = await client.GetAsync("/config");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            (await response.Content.ReadAsStringAsync()).Should().Be(_fixture.TestApplication.Name);
        }
    }
}
