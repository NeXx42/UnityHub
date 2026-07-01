using System.Data.SQLite;
using System.Text;
using CSharpSqliteORM;
using Data_Sqlite.Tables;
using Models;
using Models.Data;
using Models.Interfaces;

namespace Data_Sqlite;

public class SqliteDataRepo : IDataRepository
{
    private Database_Manager.DatabaseInstance? database;

    public async Task Setup()
    {
        string path = Path.Combine(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), GlobalConfig.APPLICATION_NAME));

        if (!Directory.Exists(path))
            Directory.CreateDirectory(path);

        database = new Database_Manager.DatabaseInstance();
        await database.Init(Path.Combine(path, "data.db"), ExceptionHandler);
    }

    private void ExceptionHandler(Exception e, string? sql)
    {
        Console.WriteLine(sql);
    }

    private ProjectInfo MapToInfo(dbo_Project dbData)
    {
        return new ProjectInfo()
        {
            id = dbData.id,
            name = dbData.name ?? "",
            directory = dbData.directory,
            iconUrl = dbData.iconPath,

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
            directory = dbData.directory,
            version = dbData.version,
            iconUrl = dbData.iconPath,
        };
    }


    public async Task<(int[], int)> Search(ProjectSearch search)
    {
        StringBuilder sql = new StringBuilder($"SELECT p.{nameof(dbo_Project.id)} as id, count(*) OVER() as total_count FROM {dbo_Project.tableName} p");
        sql.Append($" LIMIT {search.take} OFFSET {search.skip}");

        int? totalCount = null;
        int[] res = await database!.GetItemsGeneric(sql.ToString(), DeserializeDatabaseRequest, CancellationToken.None);

        return (res, totalCount ?? 0);

        async Task<int> DeserializeDatabaseRequest(SQLiteDataReader reader)
        {
            totalCount ??= Convert.ToInt32(reader["total_count"]);
            return Convert.ToInt32(reader["id"]);
        }
    }

    public async Task<ProjectCard[]> GetCardInfo(IEnumerable<int> ids)
    {
        dbo_Project[] projects = await database!.GetItems<dbo_Project>(SQLFilter.In(nameof(dbo_Project.id), ids));
        return projects.Select(MapToCard).ToArray();
    }

    public async Task<ProjectInfo> GetProjectInfo(int id)
    {
        dbo_Project? project = await database!.GetItem<dbo_Project>(SQLFilter.Equal(nameof(dbo_Project.id), id));
        return MapToInfo(project);
    }


    public async Task CreateCard(ProjectInfo info)
    {
        await database!.InsertItem(new dbo_Project
        {
            name = info.name,
            directory = info.directory
        });
    }

    public async Task CreateCards(IEnumerable<ProjectInfo> cards)
    {
        dbo_Project[] dbObjs = cards.Select(MapToDatabaseObject).ToArray();
        await database!.InsertItem(dbObjs);

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
