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
        database = new Database_Manager.DatabaseInstance();
        await database.Init(Path.Combine(GlobalConfig.getDataFolder, "data.db"), ExceptionHandler);
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
            renderPipeline = (RenderPipelineTypes?)dbData.pipelineType,

            tags = dbData.tags.Distinct().ToHashSet(),
            collections = dbData.collections.Distinct().ToHashSet()
        };
    }

    public async Task<(int[], int)> Search(ProjectSearch search)
    {
        string sql = GenerateSearchSQL(search);

        int? totalCount = null;
        int[] res = await database!.GetItemsGeneric(sql, DeserializeDatabaseRequest, CancellationToken.None);

        return (res, totalCount ?? 0);

        async Task<int> DeserializeDatabaseRequest(SQLiteDataReader reader)
        {
            totalCount ??= Convert.ToInt32(reader["total_count"]);
            return Convert.ToInt32(reader["id"]);
        }
    }

    private string GenerateSearchSQL(ProjectSearch search)
    {
        StringBuilder sql = new StringBuilder($"SELECT p.{nameof(dbo_Project.id)} as id, count(*) OVER() as total_count FROM {dbo_Project.tableName} p\n");

        List<string> leftJoinClauses = new List<string>();
        List<string> whereClauses = new List<string>();

        // build filters

        if ((search.collections?.Count() ?? 0) > 0)
        {
            leftJoinClauses.Add($"INNER JOIN {dbo_ProjectCollection.tableName} pc on pc.{nameof(dbo_ProjectCollection.ProjectId)} = p.{nameof(dbo_Project.id)}");
            whereClauses.Add($"pc.{nameof(dbo_ProjectCollection.CollectionId)} in ({string.Join(",", search.collections!)})");
        }

        if ((search.tags?.Count() ?? 0) > 0)
        {
            leftJoinClauses.Add($"INNER JOIN {dbo_ProjectTag.tableName} pt on pt.{nameof(dbo_ProjectTag.ProjectId)} = p.{nameof(dbo_Project.id)}");
            whereClauses.Add($"pt.{nameof(dbo_ProjectTag.TagId)} in ({string.Join(",", search.tags!)})");
        }

        if (search.versions.Count() > 0)
        {
            whereClauses.Add($"p.{nameof(dbo_Project.version)} in ({string.Join(",", search.versions.Select(v => $"'{v}'"))})");
        }

        // writing

        if (leftJoinClauses.Count > 0)
        {
            foreach (string join in leftJoinClauses)
                sql.AppendLine(join);
        }

        if (whereClauses.Count > 0)
        {
            sql.AppendLine(" WHERE");
            sql.Append(string.Join(" AND ", whereClauses));
        }

        sql.AppendLine($" LIMIT {search.take} OFFSET {search.skip}");
        return sql.ToString();
    }

    public async Task<ProjectInfo?> GetProjectInfo(int id) => (await FetchInternal([id], MapToInfo)).FirstOrDefault();
    public async Task<ProjectInfo[]> GetProjectInfo(IEnumerable<int> ids) => await FetchInternal(ids, MapToInfo);

    private async Task<T[]> FetchInternal<T>(IEnumerable<int> ids, Func<dbo_Project, T> mapper)
    {
        Dictionary<int, dbo_Project> projects = (await database!.GetItems<dbo_Project>(SQLFilter.In(nameof(dbo_Project.id), ids))).ToDictionary(p => p.id, p => p);
        dbo_ProjectCollection[] collections = await database!.GetItems<dbo_ProjectCollection>(SQLFilter.In(nameof(dbo_ProjectCollection.ProjectId), ids));
        dbo_ProjectTag[] tags = await database!.GetItems<dbo_ProjectTag>(SQLFilter.In(nameof(dbo_ProjectTag.ProjectId), ids));

        foreach (dbo_ProjectCollection col in collections)
        {
            if (projects.TryGetValue(col.ProjectId, out dbo_Project? proj) && proj != null)
                proj.collections.Add(col.CollectionId);
        }

        foreach (dbo_ProjectTag tag in tags)
        {
            if (projects.TryGetValue(tag.ProjectId, out dbo_Project? proj) && proj != null)
                proj.tags.Add(tag.TagId);
        }

        return projects.Values.Select(mapper).ToArray();
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

    public async Task<CollectionData[]> GetTags()
    {
        dbo_Tag[] tags = await database!.GetItems<dbo_Tag>();
        return tags.Select(Map).ToArray();

        CollectionData Map(dbo_Tag db)
        {
            return new CollectionData()
            {
                collectionId = db.Id,
                collectionName = db.Name,
                colour = db.Colour,
                type = "tag"
            };
        }
    }

    public async Task<CollectionData[]> GetCollections()
    {
        dbo_Collection[] tags = await database!.GetItems<dbo_Collection>();
        return tags.Select(Map).ToArray();

        CollectionData Map(dbo_Collection db)
        {
            return new CollectionData()
            {
                collectionId = db.Id,
                collectionName = db.Name,
                colour = db.Colour,
                type = "collection"
            };
        }
    }

    public async Task Migrate(IEnumerable<int> ids)
    {
        // temp code
        dbo_Project[] projs = await database!.GetItems<dbo_Project>(SQLFilter.In(nameof(dbo_Project.id), ids));

        foreach (dbo_Project proj in projs)
        {
            proj.iconPath = Path.Combine(GlobalConfig.getDataFolder, proj.id.ToString(), "icon.png");
            await database.Update(proj, SQLFilter.Equal(nameof(dbo_Project.id), proj.id), nameof(dbo_Project.iconPath));
        }
    }

    public async Task ToggleTag(int projId, int tagId, bool to)
    {
        if (to)
        {
            dbo_ProjectTag tag = new dbo_ProjectTag()
            {
                ProjectId = projId,
                TagId = tagId
            };

            await database!.AddOrUpdate(tag, SQLFilter.Equal(nameof(dbo_ProjectTag.ProjectId), projId).Equal(nameof(dbo_ProjectTag.TagId), tagId));
        }
        else
        {
            await database!.Delete<dbo_ProjectTag>(SQLFilter.Equal(nameof(dbo_ProjectTag.ProjectId), projId).Equal(nameof(dbo_ProjectTag.TagId), tagId));
        }
    }

    public async Task ToggleCollection(int projId, int colId, bool to)
    {
        if (to)
        {
            dbo_ProjectCollection tag = new dbo_ProjectCollection()
            {
                ProjectId = projId,
                CollectionId = colId
            };

            await database!.AddOrUpdate(tag, SQLFilter.Equal(nameof(dbo_ProjectCollection.ProjectId), projId).Equal(nameof(dbo_ProjectCollection.CollectionId), colId));
        }
        else
        {
            await database!.Delete<dbo_ProjectCollection>(SQLFilter.Equal(nameof(dbo_ProjectCollection.ProjectId), projId).Equal(nameof(dbo_ProjectCollection.CollectionId), colId));
        }
    }

    public async Task CreateTag(CollectionData src)
    {
        await database!.InsertItem(new dbo_Tag
        {
            Name = src.collectionName,
            Colour = src.colour
        });
    }

    public async Task CreateCollection(CollectionData src)
    {
        await database!.InsertItem(new dbo_Collection
        {
            Name = src.collectionName,
            Colour = src.colour
        });
    }

    public async Task<string[]> GetProjectVersions()
    {
        string sql = $"select distinct {nameof(dbo_Project.version)} from {dbo_Project.tableName}";
        return await database!.GetItemsGeneric(sql, Deserialize);

        Task<string> Deserialize(SQLiteDataReader reader)
        {
            return Task.FromResult(reader[nameof(dbo_Project.version)].ToString()!);
        }
    }
}
