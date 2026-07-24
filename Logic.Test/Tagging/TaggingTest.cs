using Data.DataRepos;
using Models.Data;
using Models.Interfaces;

namespace Logic.Test.Tagging;

public class TaggingTest
{
    [SetUp]
    public void Setup()
    {
        DependencyManager.RegisterService<IDataRepository, MockDataRepo>();
        DependencyManager.RegisterService<ITaggingLogic, TaggingLogic>();
    }

    [Test]
    public async Task TestTagCreation()
    {
        ITaggingLogic logic = DependencyManager.GetService<ITaggingLogic>()!;

        TagData newTag = new TagData()
        {
            collectionId = 0,
            collectionName = Guid.NewGuid().ToString(),
            colour = Guid.NewGuid().ToString()
        };

        await logic.CreateTag(newTag);

        TagData[] dbTags = await logic.GetTags();

        Assert.That(dbTags.Length == 1);
        Assert.That(dbTags[0].collectionName == newTag.collectionName);
        Assert.That(dbTags[0].colour == newTag.colour);
    }
}
