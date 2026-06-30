using CSharpSqliteORM;
using Data_Sqlite.Tables;
using Models;
using Models.Data;
using Models.Interfaces;

namespace Data_Sqlite;

public class SqliteDataRepo : IDataRepository
{
    public async Task Setup()
    {
        string path = Path.Combine(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), GlobalConfig.APPLICATION_NAME));

        if (!Directory.Exists(path))
            Directory.CreateDirectory(path);

        await Database_Manager.Init(Path.Combine(path, "data.db"));
    }

    private ProjectInfo MapToInfo(dbo_Project dbData)
    {
        return new ProjectInfo()
        {
            id = dbData.id,
            name = dbData.name ?? "",
            directory = dbData.directory,

            version = dbData.version,
            packages = dbData.packageCount,
            renderPipeline = (RenderPipelineTypes?)dbData.pipelineType
        };
    }

    private ProjectCard MapToCard(dbo_Project dbData)
    {
        return new ProjectCard()
        {
            id = dbData.id,
            name = dbData.name ?? "",
            directory = dbData.directory
        };
    }


    public async Task<ProjectCard[]> GetProjectCards()
    {
        dbo_Project[] projects = await Database_Manager.GetItems<dbo_Project>();
        return projects.Select(MapToCard).ToArray();
    }

    public async Task<ProjectInfo> GetProjectInfo(int id)
    {
        dbo_Project? project = await Database_Manager.GetItem<dbo_Project>(SQLFilter.Equal(nameof(dbo_Project.id), id));
        return MapToInfo(project);
    }


    public async Task CreateCard(ProjectInfo info)
    {
        await Database_Manager.InsertItem(new dbo_Project
        {
            name = info.name,
            directory = info.directory
        });
    }

    public async Task CreateCards(IEnumerable<ProjectInfo> cards)
    {
        dbo_Project[] dbObjs = cards.Select(MapToDatabaseObject).ToArray();
        await Database_Manager.InsertItem(dbObjs);

        dbo_Project MapToDatabaseObject(ProjectInfo info)
        {
            return new dbo_Project
            {
                name = info.name,
                directory = info.directory,

                version = info.version,
                packageCount = info.packages,
                pipelineType = (int?)info.renderPipeline
            };
        }
    }
}
