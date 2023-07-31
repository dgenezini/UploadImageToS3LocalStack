namespace UploadImageToS3.IntegrationTests.TestFixtures;

[CollectionDefinition("LocalStackTestcontainer Collection")]
public class LocalStackTestcontainerCollection : 
    ICollectionFixture<LocalStackTestcontainerFixture>
{
}