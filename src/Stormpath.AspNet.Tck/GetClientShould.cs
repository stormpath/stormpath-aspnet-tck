using System.Net;
using System.Threading.Tasks;
using FluentAssertions;
using Xunit;

namespace Stormpath.AspNet.Tck
{
    [Collection(nameof(IntegrationTestCollection))]
    public class GetClientShould
    {
        private readonly StandaloneTestFixture _fixture;

        public GetClientShould(StandaloneTestFixture fixture)
        {
            _fixture = fixture;
        }

        [Fact]
        public async Task GetFromContext()
        {
            // Arrange
            var client = new TestHttpClient(_fixture);

            // Act
            var response = await client.GetAsync("/client");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var expectedTenant = await _fixture.TestApplication.GetTenantAsync();
            (await response.Content.ReadAsStringAsync()).Should().Be(expectedTenant.Href);
        }
    }
}
