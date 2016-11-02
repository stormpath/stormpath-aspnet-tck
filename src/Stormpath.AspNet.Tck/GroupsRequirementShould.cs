using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using FluentAssertions;
using Xunit;

namespace Stormpath.AspNet.Tck
{
    [Collection(nameof(IntegrationTestCollection))]
    public class GroupsRequirementShould
    {
        private readonly StandaloneTestFixture _fixture;

        public GroupsRequirementShould(StandaloneTestFixture fixture)
        {
            _fixture = fixture;
        }

        [Fact]
        public async Task RedirectBearerBrowserRequestWithoutGroup()
        {
            // Arrange
            var client = new TestHttpClient(_fixture);

            using (var cleanup = new AutoCleanup(_fixture.Client))
            {
                var email = $"its-{_fixture.TestKey}@testmail.stormpath.com";
                var account = await _fixture.TestApplication.CreateAccountAsync(
                    nameof(RedirectBearerBrowserRequestWithoutGroup),
                    nameof(GroupsRequirementShould),
                    email,
                    "Changeme123!!");
                cleanup.MarkForDeletion(account);

                var accessToken = await _fixture.GetAccessToken(account, "Changeme123!!");

                var request = new HttpRequestMessage(HttpMethod.Get, "/requireGroup");
                request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("text/html"));
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

                // Act
                var response = await client.SendAsync(request);

                // Assert
                response.StatusCode.Should().Be(HttpStatusCode.Redirect);
            }
        }

        [Fact]
        public async Task RedirectCookieBrowserRequestWithoutGroup()
        {
            // Arrange
            var client = new TestHttpClient(_fixture);

            using (var cleanup = new AutoCleanup(_fixture.Client))
            {
                var email = $"its-{_fixture.TestKey}@testmail.stormpath.com";
                var account = await _fixture.TestApplication.CreateAccountAsync(
                    nameof(RedirectBearerBrowserRequestWithoutGroup),
                    nameof(GroupsRequirementShould),
                    email,
                    "Changeme123!!");
                cleanup.MarkForDeletion(account);

                var accessToken = await _fixture.GetAccessToken(account, "Changeme123!!");

                var request = new HttpRequestMessage(HttpMethod.Get, "/requireGroup");
                request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("text/html"));
                request.Headers.Add("Cookie", new[] {$"access_token={accessToken}"});

                // Act
                var response = await client.SendAsync(request);

                // Assert
                response.StatusCode.Should().Be(HttpStatusCode.Redirect);
            }
        }

        [Fact]
        public async Task RedirectCookieBrowserRequestUsingRefreshToken()
        {
            // Arrange
            var client = new TestHttpClient(_fixture);

            using (var cleanup = new AutoCleanup(_fixture.Client))
            {
                var email = $"its-{_fixture.TestKey}@testmail.stormpath.com";
                var account = await _fixture.TestApplication.CreateAccountAsync(
                    nameof(RedirectCookieBrowserRequestUsingRefreshToken),
                    nameof(GroupsRequirementShould),
                    email,
                    "Changeme123!!");
                cleanup.MarkForDeletion(account);

                var grantResult = await _fixture.GetGrantResult(account, "Changeme123!!");
                var refreshToken = grantResult.RefreshTokenString;

                var request = new HttpRequestMessage(HttpMethod.Get, "/requireGroup");
                request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("text/html"));
                request.Headers.Add("Cookie", new[] { $"refresh_token={refreshToken}" });

                // Act
                var response = await client.SendAsync(request);

                // Assert
                response.StatusCode.Should().Be(HttpStatusCode.Redirect);
            }
        }


        [Fact]
        public async Task ReturnUnauthorizedForBearerJsonRequestWithoutGroup()
        {
            // Arrange
            var client = new TestHttpClient(_fixture);

            using (var cleanup = new AutoCleanup(_fixture.Client))
            {
                var email = $"its-{_fixture.TestKey}@testmail.stormpath.com";
                var account = await _fixture.TestApplication.CreateAccountAsync(
                    nameof(ReturnUnauthorizedForBearerJsonRequestWithoutGroup),
                    nameof(GroupsRequirementShould),
                    email,
                    "Changeme123!!");
                cleanup.MarkForDeletion(account);

                var accessToken = await _fixture.GetAccessToken(account, "Changeme123!!");

                var request = new HttpRequestMessage(HttpMethod.Get, "/requireGroup");
                request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

                // Act
                var response = await client.SendAsync(request);

                // Assert
                response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
            }
        }

        [Fact]
        public async Task ReturnUnauthorizedForCookieJsonRequestWithoutGroup()
        {
            // Arrange
            var client = new TestHttpClient(_fixture);

            using (var cleanup = new AutoCleanup(_fixture.Client))
            {
                var email = $"its-{_fixture.TestKey}@testmail.stormpath.com";
                var account = await _fixture.TestApplication.CreateAccountAsync(
                    nameof(ReturnUnauthorizedForCookieJsonRequestWithoutGroup),
                    nameof(GroupsRequirementShould),
                    email,
                    "Changeme123!!");
                cleanup.MarkForDeletion(account);

                var accessToken = await _fixture.GetAccessToken(account, "Changeme123!!");

                var request = new HttpRequestMessage(HttpMethod.Get, "/requireGroup");
                request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                request.Headers.Add("Cookie", new[] { $"access_token={accessToken}" });

                // Act
                var response = await client.SendAsync(request);

                // Assert
                response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
            }
        }

        [Fact]
        public async Task AllowBearerBrowserRequestWithCorrectGroup()
        {
            // Arrange
            var client = new TestHttpClient(_fixture);

            using (var cleanup = new AutoCleanup(_fixture.Client))
            {
                var group = await _fixture.TestDirectory.CreateGroupAsync("adminIT", "Stormpath.AspNetCore test group");
                cleanup.MarkForDeletion(group);

                var email = $"its-{_fixture.TestKey}@testmail.stormpath.com";
                var account = await _fixture.TestApplication.CreateAccountAsync(
                    nameof(AllowBearerBrowserRequestWithCorrectGroup),
                    nameof(GroupsRequirementShould),
                    email,
                    "Changeme123!!");
                cleanup.MarkForDeletion(account);

                await account.AddGroupAsync(group);

                var accessToken = await _fixture.GetAccessToken(account, "Changeme123!!");

                var request = new HttpRequestMessage(HttpMethod.Get, "/requireGroup");
                request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("text/html"));
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

                // Act
                var response = await client.SendAsync(request);

                // Assert
                response.StatusCode.Should().Be(HttpStatusCode.OK);
            }
        }

        [Fact]
        public async Task AllowCookieBrowserRequestWithCorrectGroup()
        {
            // Arrange
            var client = new TestHttpClient(_fixture);

            using (var cleanup = new AutoCleanup(_fixture.Client))
            {
                var group = await _fixture.TestDirectory.CreateGroupAsync("adminIT", "Stormpath.AspNetCore test group");
                cleanup.MarkForDeletion(group);

                var email = $"its-{_fixture.TestKey}@testmail.stormpath.com";
                var account = await _fixture.TestApplication.CreateAccountAsync(
                    nameof(AllowCookieBrowserRequestWithCorrectGroup),
                    nameof(GroupsRequirementShould),
                    email,
                    "Changeme123!!");
                cleanup.MarkForDeletion(account);

                await account.AddGroupAsync(group);

                var accessToken = await _fixture.GetAccessToken(account, "Changeme123!!");

                var request = new HttpRequestMessage(HttpMethod.Get, "/requireGroup");
                request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("text/html"));
                request.Headers.Add("Cookie", new[] { $"access_token={accessToken}" });

                // Act
                var response = await client.SendAsync(request);

                // Assert
                response.StatusCode.Should().Be(HttpStatusCode.OK);
            }
        }

        /// <summary>
        /// Regression test for https://github.com/stormpath/stormpath-aspnetcore/issues/28
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task AllowCookieBrowserRequestUsingRefreshToken()
        {
            // Arrange
            var client = new TestHttpClient(_fixture);

            using (var cleanup = new AutoCleanup(_fixture.Client))
            {
                var group = await _fixture.TestDirectory.CreateGroupAsync("adminIT", "Stormpath.AspNetCore test group");
                cleanup.MarkForDeletion(group);

                var email = $"its-{_fixture.TestKey}@testmail.stormpath.com";
                var account = await _fixture.TestApplication.CreateAccountAsync(
                    nameof(AllowCookieBrowserRequestUsingRefreshToken),
                    nameof(GroupsRequirementShould),
                    email,
                    "Changeme123!!");
                cleanup.MarkForDeletion(account);

                await account.AddGroupAsync(group);

                var grantResult = await _fixture.GetGrantResult(account, "Changeme123!!");
                var refreshToken = grantResult.RefreshTokenString;

                var request = new HttpRequestMessage(HttpMethod.Get, "/requireGroup");
                request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("text/html"));
                request.Headers.Add("Cookie", new[] { $"refresh_token={refreshToken}" });

                // Act
                var response = await client.SendAsync(request);

                // Assert
                response.StatusCode.Should().Be(HttpStatusCode.OK);
            }
        }

        [Fact]
        public async Task AllowBearerJsonRequestWithCorrectGroup()
        {
            // Arrange
            var client = new TestHttpClient(_fixture);

            using (var cleanup = new AutoCleanup(_fixture.Client))
            {
                var group = await _fixture.TestDirectory.CreateGroupAsync("adminIT", "Stormpath.AspNetCore test group");
                cleanup.MarkForDeletion(group);

                var email = $"its-{_fixture.TestKey}@testmail.stormpath.com";
                var account = await _fixture.TestApplication.CreateAccountAsync(
                    nameof(AllowBearerJsonRequestWithCorrectGroup),
                    nameof(GroupsRequirementShould),
                    email,
                    "Changeme123!!");
                cleanup.MarkForDeletion(account);

                await account.AddGroupAsync(group);

                var accessToken = await _fixture.GetAccessToken(account, "Changeme123!!");

                var request = new HttpRequestMessage(HttpMethod.Get, "/requireGroup");
                request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

                // Act
                var response = await client.SendAsync(request);

                // Assert
                response.StatusCode.Should().Be(HttpStatusCode.OK);
            }
        }

        [Fact]
        public async Task AllowCookieJsonRequestWithCorrectGroup()
        {
            // Arrange
            var client = new TestHttpClient(_fixture);

            using (var cleanup = new AutoCleanup(_fixture.Client))
            {
                var group = await _fixture.TestDirectory.CreateGroupAsync("adminIT", "Stormpath.AspNetCore test group");
                cleanup.MarkForDeletion(group);

                var email = $"its-{_fixture.TestKey}@testmail.stormpath.com";
                var account = await _fixture.TestApplication.CreateAccountAsync(
                    nameof(AllowCookieJsonRequestWithCorrectGroup),
                    nameof(GroupsRequirementShould),
                    email,
                    "Changeme123!!");
                cleanup.MarkForDeletion(account);

                await account.AddGroupAsync(group);

                var accessToken = await _fixture.GetAccessToken(account, "Changeme123!!");

                var request = new HttpRequestMessage(HttpMethod.Get, "/requireGroup");
                request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                request.Headers.Add("Cookie", new[] { $"access_token={accessToken}" });

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
                var group = await _fixture.TestDirectory.CreateGroupAsync("adminIT", "Stormpath.AspNetCore test group");
                cleanup.MarkForDeletion(group);

                var email = $"its-{_fixture.TestKey}@testmail.stormpath.com";
                var account = await _fixture.TestApplication.CreateAccountAsync(
                    nameof(HandleConcurrentRequests),
                    nameof(GroupsRequirementShould),
                    email,
                    "Changeme123!!");
                cleanup.MarkForDeletion(account);
                await account.AddGroupAsync(group);

                var account2 = await _fixture.TestApplication.CreateAccountAsync(
                    $"{nameof(HandleConcurrentRequests)} #2",
                    nameof(GroupsRequirementShould),
                    $"its-{_fixture.TestKey}-2@testmail.stormpath.com",
                    "Changeme123!!");
                cleanup.MarkForDeletion(account2);

                var accessToken1 = await _fixture.GetAccessToken(account, "Changeme123!!");
                var accessToken2 = await _fixture.GetAccessToken(account2, "Changeme123!!");

                var request1 = new HttpRequestMessage(HttpMethod.Get, "/requireGroup");
                request1.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("text/html"));
                request1.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken1);

                var request2 = new HttpRequestMessage(HttpMethod.Get, "/requireGroup");
                request2.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("text/html"));
                request2.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken2);

                var grantResult3 = await _fixture.GetGrantResult(account, "Changeme123!!");
                var refreshToken3 = grantResult3.RefreshTokenString;

                var request3 = new HttpRequestMessage(HttpMethod.Get, "/requireGroup");
                request3.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("text/html"));
                request3.Headers.Add("Cookie", new[] { $"refresh_token={refreshToken3}" });


                // Act
                var responses = await Task.WhenAll(
                    client.SendAsync(request1),
                    client.SendAsync(request2),
                    client.SendAsync(request3));

                // Assert
                responses[0].StatusCode.Should().Be(HttpStatusCode.OK);
                responses[1].StatusCode.Should().Be(HttpStatusCode.Redirect);
                responses[2].StatusCode.Should().Be(HttpStatusCode.OK);
            }
        }
    }
}
