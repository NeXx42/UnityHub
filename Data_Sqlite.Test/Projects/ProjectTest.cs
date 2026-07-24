using Models.Data;
using Models.Interfaces;

namespace Data_Sqlite.Test.Projects;

public class ProjectTest
{
    private string tempPath;
    private IDataRepository data;

    [SetUp]
    public async Task Setup()
    {
        tempPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(tempPath);

        SqliteDataRepo repo = new SqliteDataRepo();
        await repo.Setup_Test(Path.Combine(tempPath, $"{Guid.NewGuid()}.db"));

        data = repo;
    }

    [TearDown]
    public async Task Cleanup()
    {
        Directory.Delete(tempPath, true);
    }


    [Test]
    public async Task Test_ProjectCreation()
    {
        ProjectInfo info = new ProjectInfo()
        {
            id = -1,
            collectionId = 0,
            directory = Guid.NewGuid().ToString(),
            name = Guid.NewGuid().ToString(),
        };

        int newId = await data.CreateCard(info);
        ProjectInfo? dbItem = await data.GetProjectInfo(newId);

        Assert.That(dbItem, Is.Not.Null);
        Assert.That(newId == dbItem.id);
        Assert.That(info.directory == dbItem.directory);
        Assert.That(info.name == dbItem.name);
    }
}
