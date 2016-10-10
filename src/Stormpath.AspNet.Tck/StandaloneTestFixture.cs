using System;
using System.Linq;
using System.Threading.Tasks;
using Stormpath.SDK;
using Stormpath.SDK.Account;
using Stormpath.SDK.Application;
using Stormpath.SDK.Client;
using Stormpath.SDK.Directory;
using Stormpath.SDK.Http;
using Stormpath.SDK.Oauth;
using Stormpath.SDK.Serialization;
using Stormpath.SDK.Sync;

namespace Stormpath.AspNet.Tck
{
    public class StandaloneTestFixture
    {
        public StandaloneTestFixture()
            : this(Clients.Builder()
                .SetHttpClient(HttpClients.Create().SystemNetHttpClient())
                .SetSerializer(Serializers.Create().JsonNetSerializer())
                .Build())
        {
        }

        public StandaloneTestFixture(IClient client)
        {
            Client = client;
            TestKey = Guid.NewGuid().ToString();

            TestApplication = client.GetApplications().Where(app => app.Name == "My Application").Synchronously().Single();
            TestDirectory = TestApplication.GetDefaultAccountStore() as IDirectory;
        }

        public async Task<string> GetAccessToken(IAccount account, string password)
        {
            var grantRequest = OauthRequests.NewPasswordGrantRequest()
                .SetLogin(account.Email)
                .SetPassword(password)
                .Build();
            var grantResponse = await TestApplication.NewPasswordGrantAuthenticator()
                .AuthenticateAsync(grantRequest);

            return grantResponse.AccessTokenString;
        }

        public Uri BaseUri = new Uri("http://localhost:8080");

        public IClient Client { get; }

        public string TestKey { get; }

        public IApplication TestApplication { get; private set; }

        public IDirectory TestDirectory { get; private set; }
    }
}