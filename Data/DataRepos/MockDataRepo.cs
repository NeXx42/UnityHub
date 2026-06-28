using Models.Data;
using Models.Interfaces;

namespace Data.DataRepos;

public class MockDataRepo : IDataRepository
{
    public Task Setup()
    {
        return Task.CompletedTask;
    }

    public async Task<ProjectCard[]> GetProjectCards()
    {
        await Task.Delay(1000);

        return [
            new ProjectCard(){
                id = 0,
                name = "Test 1",
                directory = "~/Documents/Test1"
            },
            new ProjectCard(){
                id = 1,
                name = "Test 1",
                directory = "~/Documents/Test1"
            },
            new ProjectCard(){
                id = 2,
                name = "Test 1",
                directory = "~/Documents/Test1"
            }
        ];
    }

    public async Task<ProjectInfo> GetProjectInfo(int id)
    {
        await Task.Delay(1000);

        return new ProjectInfo()
        {
            id = id,
            directory = "/home/test/test/test/test/test/test/test/test/project",
            name = "PROJECT_NAME??!",

        };
    }
}
