using Xunit;

namespace Stormpath.AspNet.Tck
{
    [CollectionDefinition(nameof(IntegrationTestCollection))]
    public class IntegrationTestCollection : ICollectionFixture<StandaloneTestFixture>
    {
        // Intentionally left blank. This class only serves as an anchor for CollectionDefinitionAttribute.
    }
}
