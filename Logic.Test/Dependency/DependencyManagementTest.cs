using Data.DataRepos;
using Models.Interfaces;

namespace Logic.Test.Dependency;

public class DependencyManagementTest
{
    [SetUp]
    public void Setup()
    {
    }

    [Test]
    public async Task TestDataRepo()
    {
        DependencyManager.RegisterService<IDataRepository, MockDataRepo>();
        IDataRepository? dataRepo = DependencyManager.GetService<IDataRepository>();

        Assert.That(dataRepo, Is.Not.Null);

        await dataRepo.CreateCollection(new Models.Data.CollectionData() { collectionId = 0, collectionName = "" });
        var res = await dataRepo.GetCollections();

        Assert.That(res.Length != 0);
    }
}
