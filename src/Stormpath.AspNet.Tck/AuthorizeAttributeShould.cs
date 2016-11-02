using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using FluentAssertions;
using Xunit;

namespace Stormpath.AspNet.Tck
{
    [Collection(nameof(IntegrationTestCollection))]
    public class AuthorizeAttributeShould
    {
        private readonly StandaloneTestFixture _fixture;

        public AuthorizeAttributeShould(StandaloneTestFixture fixture)
        {
            _fixture = fixture;
        }

        [Fact]
        public async Task RedirectForUnauthenticatedBrowserRequest()
        {
            // Arrange
            var client = new TestHttpClient(_fixture);

            // Act
            var request = new HttpRequestMessage(HttpMethod.Get, "/protected");
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("text/html"));
            var response = await client.SendAsync(request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Found);
            response.Headers.Location.OriginalString.Should().StartWith("/login?");
        }

        [Fact]
        public async Task ReturnUnauthorizedForUnauthenticatedJsonRequest()
        {
            // Arrange
            var client = new TestHttpClient(_fixture);

            // Act
            var request = new HttpRequestMessage(HttpMethod.Get, "/protected");
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            var response = await client.SendAsync(request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task AllowBrowserRequestAuthorizedWithHeader()
        {
            // Arrange
            var client = new TestHttpClient(_fixture);

            using (var cleanup = new AutoCleanup(_fixture.Client))
            {
                var email = $"its-{_fixture.TestKey}@testmail.stormpath.com";
                var account = await _fixture.TestApplication.CreateAccountAsync(
                    nameof(AllowBrowserRequestAuthorizedWithHeader),
                    nameof(AuthorizeAttributeShould),
                    email,
                    "Changeme123!!");
                cleanup.MarkForDeletion(account);

                var accessToken = await _fixture.GetAccessToken(account, "Changeme123!!");

                var request = new HttpRequestMessage(HttpMethod.Get, "/protected");
                request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("text/html"));
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

                // Act
                var response = await client.SendAsync(request);

                // Assert
                response.StatusCode.Should().Be(HttpStatusCode.OK);
            }
        }

        [Fact]
        public async Task AllowBrowserRequestAuthorizedWithCookie()
        {
            // Arrange
            var client = new TestHttpClient(_fixture);

            using (var cleanup = new AutoCleanup(_fixture.Client))
            {
                var email = $"its-{_fixture.TestKey}@testmail.stormpath.com";
                var account = await _fixture.TestApplication.CreateAccountAsync(
                    nameof(AllowBrowserRequestAuthorizedWithCookie),
                    nameof(AuthorizeAttributeShould),
                    email,
                    "Changeme123!!");
                cleanup.MarkForDeletion(account);

                var accessToken = await _fixture.GetAccessToken(account, "Changeme123!!");

                var request = new HttpRequestMessage(HttpMethod.Get, "/protected");
                request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("text/html"));
                request.Headers.Add("Cookie", $"access_token={accessToken}");

                // Act
                var response = await client.SendAsync(request);

                // Assert
                response.StatusCode.Should().Be(HttpStatusCode.OK);
            }
        }

        [Fact]
        public async Task AllowJsonRequestAuthorizedWithHeader()
        {
            // Arrange
            var client = new TestHttpClient(_fixture);

            using (var cleanup = new AutoCleanup(_fixture.Client))
            {
                var email = $"its-{_fixture.TestKey}@testmail.stormpath.com";
                var account = await _fixture.TestApplication.CreateAccountAsync(
                    nameof(AllowJsonRequestAuthorizedWithHeader),
                    nameof(AuthorizeAttributeShould),
                    email,
                    "Changeme123!!");
                cleanup.MarkForDeletion(account);

                var accessToken = await _fixture.GetAccessToken(account, "Changeme123!!");

                var request = new HttpRequestMessage(HttpMethod.Get, "/protected");
                request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

                // Act
                var response = await client.SendAsync(request);

                // Assert
                response.StatusCode.Should().Be(HttpStatusCode.OK);
            }
        }

        [Fact]
        public async Task AllowJsonRequestAuthorizedWithCookie()
        {
            // Arrange
            var client = new TestHttpClient(_fixture);

            using (var cleanup = new AutoCleanup(_fixture.Client))
            {
                var email = $"its-{_fixture.TestKey}@testmail.stormpath.com";
                var account = await _fixture.TestApplication.CreateAccountAsync(
                    nameof(AllowJsonRequestAuthorizedWithCookie),
                    nameof(AuthorizeAttributeShould),
                    email,
                    "Changeme123!!");
                cleanup.MarkForDeletion(account);

                var accessToken = await _fixture.GetAccessToken(account, "Changeme123!!");

                var request = new HttpRequestMessage(HttpMethod.Get, "/protected");
                request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                request.Headers.Add("Cookie", $"access_token={accessToken}");

                // Act
                var response = await client.SendAsync(request);

                // Assert
                response.StatusCode.Should().Be(HttpStatusCode.OK);
            }
        }

        [Fact]
        public async Task HandleConcurrentRequests()
        {
            // Arrange
            var client = new TestHttpClient(_fixture);

            using (var cleanup = new AutoCleanup(_fixture.Client))
            {
                var email = $"its-{_fixture.TestKey}@testmail.stormpath.com";
                var account = await _fixture.TestApplication.CreateAccountAsync(
                    nameof(HandleConcurrentRequests),
                    nameof(AuthorizeAttributeShould),
                    email,
                    "Changeme123!!");
                cleanup.MarkForDeletion(account);

                var accessToken = await _fixture.GetAccessToken(account, "Changeme123!!");

                var request1 = new HttpRequestMessage(HttpMethod.Get, "/protected");
                request1.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("text/html"));
                request1.Headers.Add("Cookie", $"access_token={accessToken}");

                var request2 = new HttpRequestMessage(HttpMethod.Get, "/protected");
                request2.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("text/html"));

                // Act
                var responses = await Task.WhenAll(
                    client.SendAsync(request1),
                    client.SendAsync(request2));

                // Assert
                responses[0].StatusCode.Should().Be(HttpStatusCode.OK);
                responses[1].StatusCode.Should().Be(HttpStatusCode.Redirect);
            }
        }
    }
}