using System.Data.SQLite;
using System.Text;
using CSharpSqliteORM;
using Data_Sqlite.Tables;
using Logic.db;
using Models;
using Models.Data;
using Models.Enums;
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
            collectionId = dbData.collectionId,

            lastOpened = dbData.lastOpened == 0 ? null : dbData.lastOpened,
            created = dbData.created == 0 ? null : dbData.created,
            size = dbData.size == 0 ? null : dbData.size,
            notes = dbData.notes,

            favourited = dbData.favourited
        };
    }

    private dbo_Project MapToDto(ProjectInfo info)
    {
        return new dbo_Project()
        {
            id = info.id,
            name = info.name ?? "",
            directory = info.directory,
            iconPath = info.iconUrl,

            version = info.version,
            packageCount = info.packages,
            pipelineType = (int?)info.renderPipeline,

            tags = info.tags.ToList(),

            lastOpened = info.lastOpened ?? 0,
            created = info.created ?? 0,
            size = info.size ?? 0,
            notes = info.notes,

            favourited = info.favourited,
            collectionId = info.collectionId
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
        string projectLocalName = "p";
        StringBuilder sql = new StringBuilder($"SELECT {projectLocalName}.{nameof(dbo_Project.id)} as id, count(*) OVER() as total_count FROM {dbo_Project.tableName} {projectLocalName}\n");

        List<string> leftJoinClauses = new List<string>();
        List<string> innerJoinClauses = new List<string>();
        List<string> whereClauses = new List<string>();
        List<string> groupClauses = new List<string>();
        List<string> havingClauses = new List<string>();

        // build filters

        if ((search.collections?.Count() ?? 0) > 0)
        {
            whereClauses.Add($"{projectLocalName}.{nameof(dbo_Project.collectionId)} in ({string.Join(",", search.collections!)})");
        }

        if ((search.tags?.Count() ?? 0) > 0)
        {
            string joinName = "pt";
            innerJoinClauses.Add($"{dbo_ProjectTag.tableName} {joinName} on {joinName}.{nameof(dbo_ProjectTag.ProjectId)} = {projectLocalName}.{nameof(dbo_Project.id)}");
            whereClauses.Add($"{joinName}.{nameof(dbo_ProjectTag.TagId)} in ({string.Join(",", search.tags!)})");

            if ((search.collections?.Count() ?? 0) == 0)
                groupClauses.Add($"{projectLocalName}.{nameof(dbo_Project.id)}");

            havingClauses.Add($"COUNT(DISTINCT {joinName}.{nameof(dbo_ProjectTag.TagId)}) = {search.tags!.Count()}");
        }

        if (search.versions.Count() > 0)
        {
            whereClauses.Add($"p.{nameof(dbo_Project.version)} in ({string.Join(",", search.versions.Select(v => $"'{v}'"))})");
        }

        if (!string.IsNullOrEmpty(search.text))
        {
            whereClauses.Add($"p.{nameof(dbo_Project.name)} like '{search.text}%'");
        }

        if (search.requiredOpened)
            whereClauses.Add($"p.{nameof(dbo_Project.lastOpened)} is not null");

        if (search.requiredFavs)
            whereClauses.Add($"p.{nameof(dbo_Project.favourited)} = 1");

        // writing

        if (leftJoinClauses.Count > 0)
        {
            foreach (string join in leftJoinClauses)
                sql.AppendLine($"LEFT JOIN {join}");
        }

        if (innerJoinClauses.Count > 0)
        {
            foreach (string join in innerJoinClauses)
                sql.AppendLine($"INNER JOIN {join}");
        }

        if (whereClauses.Count > 0)
        {
            sql.AppendLine(" WHERE");
            sql.Append(string.Join(" AND ", whereClauses));
        }

        if (groupClauses.Count > 0)
        {
            sql.AppendLine(" GROUP BY ");
            sql.Append(string.Join(" AND ", groupClauses));
        }

        if (havingClauses.Count > 0)
        {
            sql.AppendLine(" HAVING ");
            sql.Append(string.Join(" AND ", havingClauses));
        }

        sql.AppendLine(" ORDER BY ");

        switch (search.order)
        {
            case Models.Enums.ProjectOrder.NameAsc:
            case Models.Enums.ProjectOrder.NameDesc:
                sql.Append(nameof(dbo_Project.name));
                break;

            case Models.Enums.ProjectOrder.LastOpenedAsc:
            case Models.Enums.ProjectOrder.LastOpenedDesc:
                sql.Append(nameof(dbo_Project.lastOpened));
                break;

            case Models.Enums.ProjectOrder.CreatedAsc:
            case Models.Enums.ProjectOrder.CreatedDesc:
                sql.Append(nameof(dbo_Project.created));
                break;

            case Models.Enums.ProjectOrder.SizeAsc:
            case Models.Enums.ProjectOrder.SizeDesc:
                sql.Append(nameof(dbo_Project.size));
                break;
        }

        switch (search.order)
        {
            case Models.Enums.ProjectOrder.NameDesc:
            case Models.Enums.ProjectOrder.LastOpenedDesc:
            case Models.Enums.ProjectOrder.CreatedDesc:
            case Models.Enums.ProjectOrder.SizeDesc:
                sql.Append(" DESC ");
                break;
        }

        if (search.take > 0)
            sql.AppendLine($" LIMIT {search.take} OFFSET {search.skip}");

        return sql.ToString();
    }

    public async Task<ProjectInfo?> GetProjectInfo(int id) => (await FetchInternal([id], MapToInfo)).FirstOrDefault();
    public async Task<ProjectInfo[]> GetProjectInfo(IEnumerable<int> ids) => await FetchInternal(ids, MapToInfo);

    private async Task<T[]> FetchInternal<T>(IEnumerable<int> ids, Func<dbo_Project, T> mapper)
    {
        Dictionary<int, dbo_Project> projects = (await database!.GetItems<dbo_Project>(SQLFilter.In(nameof(dbo_Project.id), ids))).ToDictionary(p => p.id, p => p);
        dbo_ProjectTag[] tags = await database!.GetItems<dbo_ProjectTag>(SQLFilter.In(nameof(dbo_ProjectTag.ProjectId), ids));

        foreach (dbo_ProjectTag tag in tags)
        {
            if (projects.TryGetValue(tag.ProjectId, out dbo_Project? proj) && proj != null)
                proj.tags.Add(tag.TagId);
        }

        return projects.Values.Select(mapper).ToArray();
    }


    public async Task<int> CreateCard(ProjectInfo info) => (await CreateCards([info])).Values.Single();
    public async Task<Dictionary<string, int>> CreateCards(IEnumerable<ProjectInfo> cards)
    {
        dbo_Project[] dbObjs = cards.Select(MapToDto).ToArray();
        await database!.InsertItem(dbObjs);

        // .. need to implement something in my sql orm to get the ids for new items

        Dictionary<string, int> newIds = new Dictionary<string, int>();

        dbo_Project[] dboItems = await database!.GetItems<dbo_Project>(SQLFilter
            .In(nameof(dbo_Project.directory), cards.Select(c => c.directory))
            .OrderDesc(nameof(dbo_Project.id))
            .Limit(cards.Count()));

        foreach (dbo_Project newItem in dboItems)
        {
            if (!newIds.ContainsKey(newItem.directory)) // ordered so if some how there is a duplicate it should still select the newest
                newIds[newItem.directory] = newItem.id;
        }

        return newIds;
    }

    public async Task<TagData[]> GetTags()
    {
        dbo_Tag[] tags = await database!.GetItems<dbo_Tag>();
        return tags.Select(Map).ToArray();

        TagData Map(dbo_Tag db)
        {
            return new TagData()
            {
                collectionId = db.Id,
                collectionName = db.Name,
                colour = db.Colour,
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
                handlingType = (CollectionHandlingTypes)db.HandlingType
            };
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

    public async Task SetCollection(int projId, int colId)
    {
        await database!.Update(new dbo_Project()
        {
            id = projId,
            collectionId = colId,
            directory = string.Empty,
            favourited = false

        }, SQLFilter.Equal(nameof(dbo_Project.id), projId), [nameof(dbo_Project.collectionId)]);
    }

    public async Task CreateTag(TagData src)
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

    public async Task<string?[]> GetConfigValue(string key) => (await database!.GetItems<dbo_Config>(SQLFilter.Equal(nameof(dbo_Config.key), key)))?.Select(k => k.value).ToArray() ?? [];

    public async Task SetConfigValue(string key, string? value)
    {
        dbo_Config config = new dbo_Config()
        {
            key = key,
            value = value
        };

        await database!.AddOrUpdate(config, SQLFilter.Equal(nameof(dbo_Config.key), key));
    }

    public async Task DeleteConfigValue(string key) => await database!.Delete<dbo_Config>(SQLFilter.Equal(nameof(dbo_Config.key), key));

    public async Task SetEditorInfo(Dictionary<string, string> versionJson)
    {
        dbo_UnityInstallInfo[] toAdd = versionJson.Select(v => new dbo_UnityInstallInfo()
        {
            version = v.Key,
            json = v.Value
        }).ToArray();

        await database!.AddOrUpdate(toAdd, (v) => SQLFilter.Equal(nameof(dbo_UnityInstallInfo.version), v.version));
    }

    public async Task<Dictionary<string, string>> GetEditorInfo(IEnumerable<string> versions)
    {
        dbo_UnityInstallInfo[] data = await database!.GetItems<dbo_UnityInstallInfo>(SQLFilter.In(nameof(dbo_UnityInstallInfo.version), versions));
        return data.ToDictionary(d => d.version, d => d.json);
    }

    public async Task DeleteCard(IEnumerable<int> ids)
    {
        await database!.Delete<dbo_ProjectTag>(SQLFilter.In(nameof(dbo_ProjectTag.ProjectId), ids));
        await database!.Delete<dbo_Project>(SQLFilter.In(nameof(dbo_Project.id), ids));
    }

    public async Task UpdateProjectProperties(ProjectInfo info, IEnumerable<string> properties)
        => await UpdateProjectProperties([info], properties);

    public async Task UpdateProjectProperties(IEnumerable<ProjectInfo> updates, IEnumerable<string> properties)
    {
        Dictionary<string, string> columnMappings = new()
        {
            { nameof(ProjectInfo.id), nameof(dbo_Project.id) },

            { nameof(ProjectInfo.name), nameof(dbo_Project.name) },
            { nameof(ProjectInfo.directory), nameof(dbo_Project.directory) },
            { nameof(ProjectInfo.iconUrl), nameof(dbo_Project.iconPath) },

            { nameof(ProjectInfo.version), nameof(dbo_Project.version) },
            { nameof(ProjectInfo.packages), nameof(dbo_Project.packageCount) },
            { nameof(ProjectInfo.renderPipeline), nameof(dbo_Project.pipelineType) },

            { nameof(ProjectInfo.size), nameof(dbo_Project.size) },
            { nameof(ProjectInfo.lastOpened), nameof(dbo_Project.lastOpened) },
            { nameof(ProjectInfo.created), nameof(dbo_Project.created) },
            { nameof(ProjectInfo.notes), nameof(dbo_Project.notes) },

            { nameof(ProjectInfo.favourited), nameof(dbo_Project.favourited) },

            { nameof(ProjectInfo.tags), nameof(dbo_Project.tags) },
            { nameof(ProjectInfo.collectionId), nameof(dbo_Project.collectionId) },
        };

        await database!.Update(updates.Select(MapToDto), (u) => SQLFilter.Equal(nameof(dbo_Project.id), u.id), [.. properties.Select(p => columnMappings[p])]);
    }
}
