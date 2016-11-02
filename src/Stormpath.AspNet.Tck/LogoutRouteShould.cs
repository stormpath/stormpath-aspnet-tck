using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using FluentAssertions;
using Newtonsoft.Json;
using Xunit;

namespace Stormpath.AspNet.Tck
{
    [Collection(nameof(IntegrationTestCollection))]
    public class LogoutRouteShould
    {
        private readonly StandaloneTestFixture _fixture;

        public LogoutRouteShould(StandaloneTestFixture fixture)
        {
            _fixture = fixture;
        }

        [Fact]
        public async Task DeleteCookiesProperly()
        {
            // Arrange
            var client = new TestHttpClient(_fixture);

            using (var cleanup = new AutoCleanup(_fixture.Client))
            {
                // Create a user
                var application = await _fixture.Client.GetApplicationAsync(_fixture.TestApplication.Href);
                var email = $"its-{_fixture.TestKey}@testmail.stormpath.com";
                var account = await application.CreateAccountAsync(
                    nameof(DeleteCookiesProperly),
                    nameof(LogoutRouteShould),
                    email,
                    "Changeme123!!");
                cleanup.MarkForDeletion(account);

                // Get a token
                var payload = new Dictionary<string, string>
                {
                    ["grant_type"] = "password",
                    ["username"] = email,
                    ["password"] = "Changeme123!!"
                };

                var tokenResponse = await client.PostAsync("/oauth/token", new FormUrlEncodedContent(payload));
                tokenResponse.EnsureSuccessStatusCode();

                var tokenResponseContent = JsonConvert.DeserializeObject<Dictionary<string, string>>(await tokenResponse.Content.ReadAsStringAsync());
                var accessToken = tokenResponseContent["access_token"];
                var refreshToken = tokenResponseContent["refresh_token"];

                // Create a logout request
                var logoutRequest = new HttpRequestMessage(HttpMethod.Post, "/logout");
                logoutRequest.Headers.Add("Cookie", $"access_token={accessToken}");
                logoutRequest.Headers.Add("Cookie", $"refresh_token={refreshToken}");
                logoutRequest.Content = new FormUrlEncodedContent(new KeyValuePair<string, string>[0]);

                // Act
                var logoutResponse = await client.SendAsync(logoutRequest);
                logoutResponse.EnsureSuccessStatusCode();

                // Assert
                var setCookieHeaders = logoutResponse.Headers.GetValues("Set-Cookie").ToArray();
                setCookieHeaders.Length.Should().Be(2);
                setCookieHeaders.Should().Contain("access_token=; path=/; expires=Thu, 01-Jan-1970 00:00:00 GMT; HttpOnly");
                setCookieHeaders.Should().Contain("refresh_token=; path=/; expires=Thu, 01-Jan-1970 00:00:00 GMT; HttpOnly");
            }
        }

        /// <summary>
        /// Regression test for https://github.com/stormpath/stormpath-dotnet-owin-middleware/issues/44
        /// </summary>
        /// <returns>The asynchronous test.</returns>
        [Fact]
        public async Task LogoutWithExpiredAccessToken()
        {
            // Arrange
            var client = new TestHttpClient(_fixture);

            using (var cleanup = new AutoCleanup(_fixture.Client))
            {
                // Create a user
                var application = await _fixture.Client.GetApplicationAsync(_fixture.TestApplication.Href);
                var email = $"its-{_fixture.TestKey}@testmail.stormpath.com";
                var account = await application.CreateAccountAsync(
                    nameof(LogoutWithExpiredAccessToken),
                    nameof(LogoutRouteShould),
                    email,
                    "Changeme123!!");
                cleanup.MarkForDeletion(account);

                // Get a token
                var payload = new Dictionary<string, string>
                {
                    ["grant_type"] = "password",
                    ["username"] = email,
                    ["password"] = "Changeme123!!"
                };

                var tokenResponse = await client.PostAsync("/oauth/token", new FormUrlEncodedContent(payload));
                tokenResponse.EnsureSuccessStatusCode();

                var tokenResponseContent = JsonConvert.DeserializeObject<Dictionary<string, string>>(await tokenResponse.Content.ReadAsStringAsync());
                var refreshToken = tokenResponseContent["refresh_token"];

                // Create a logout request
                var logoutRequest = new HttpRequestMessage(HttpMethod.Post, "/logout");
                logoutRequest.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                logoutRequest.Headers.Add("Cookie", $"refresh_token={refreshToken}");
                logoutRequest.Content = new FormUrlEncodedContent(new KeyValuePair<string, string>[0]);

                // Act
                var logoutResponse = await client.SendAsync(logoutRequest);
                logoutResponse.EnsureSuccessStatusCode();

                // Assert
                var setCookieHeaders = logoutResponse.Headers.GetValues("Set-Cookie").ToArray();
                setCookieHeaders.Length.Should().Be(2);
                setCookieHeaders.Should().Contain("access_token=; path=/; expires=Thu, 01-Jan-1970 00:00:00 GMT; HttpOnly");
                setCookieHeaders.Should().Contain("refresh_token=; path=/; expires=Thu, 01-Jan-1970 00:00:00 GMT; HttpOnly");
            }
        }

        [Fact]
        public async Task SucceedForAnonymousJsonRequest()
        {
            // Arrange
            var client = new TestHttpClient(_fixture);

            // Create a logout request
            var logoutRequest = new HttpRequestMessage(HttpMethod.Post, "/logout");
            logoutRequest.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            logoutRequest.Content = new FormUrlEncodedContent(new KeyValuePair<string, string>[0]);

            // Act
            var logoutResponse = await client.SendAsync(logoutRequest);
            logoutResponse.EnsureSuccessStatusCode();

            // Assert
            var setCookieHeaders = logoutResponse.Headers.GetValues("Set-Cookie").ToArray();
            setCookieHeaders.Length.Should().Be(2);
            setCookieHeaders.Should().Contain("access_token=; path=/; expires=Thu, 01-Jan-1970 00:00:00 GMT; HttpOnly");
            setCookieHeaders.Should().Contain("refresh_token=; path=/; expires=Thu, 01-Jan-1970 00:00:00 GMT; HttpOnly");
        }

        [Fact]
        public async Task SucceedForAnonymousHtmlRequest()
        {
            // Arrange
            var client = new TestHttpClient(_fixture);

            // Create a logout request
            var logoutRequest = new HttpRequestMessage(HttpMethod.Post, "/logout");
            logoutRequest.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("text/html"));
            logoutRequest.Content = new FormUrlEncodedContent(new KeyValuePair<string, string>[0]);

            // Act
            var logoutResponse = await client.SendAsync(logoutRequest);
            logoutResponse.StatusCode.Should().Be(HttpStatusCode.Found);

            // Assert
            var setCookieHeaders = logoutResponse.Headers.GetValues("Set-Cookie").ToArray();
            setCookieHeaders.Length.Should().Be(2);
            setCookieHeaders.Should().Contain("access_token=; path=/; expires=Thu, 01-Jan-1970 00:00:00 GMT; HttpOnly");
            setCookieHeaders.Should().Contain("refresh_token=; path=/; expires=Thu, 01-Jan-1970 00:00:00 GMT; HttpOnly");
        }
    }
}
