using CSharpSqliteORM.Structure;
using Models.Data;

namespace Data_Sqlite.Tables;

public class dbo_Project : IDatabase_Table
{
    public static string tableName => "ProjectData";

    public int id { get; set; }

    public string? name { get; set; }
    public required string directory { get; set; }
    public string? iconPath { get; set; }

    public string? version { get; set; }
    public int? packageCount { get; set; }
    public int? pipelineType { get; set; }

    public long size { get; set; }
    public long lastOpened { get; set; }
    public long created { get; set; }
    public string? notes { get; set; }

    public required bool favourited { get; set; }

    public List<int> tags = [];
    public List<int> collections = [];

    public static Database_Column[] getColumns => [
        new Database_Column { columnName = nameof(id), columnType = Database_ColumnType.INTEGER, isPrimaryKey = true, autoIncrement = true, allowNull = false },

        new Database_Column { columnName = nameof(name), columnType = Database_ColumnType.TEXT, allowNull = true },
        new Database_Column { columnName = nameof(directory), columnType = Database_ColumnType.TEXT, allowNull = false },
        new Database_Column { columnName = nameof(iconPath), columnType = Database_ColumnType.TEXT, allowNull = true },

        new Database_Column { columnName = nameof(version), columnType = Database_ColumnType.TEXT, allowNull = true },
        new Database_Column { columnName = nameof(packageCount), columnType = Database_ColumnType.INTEGER, allowNull = true },
        new Database_Column { columnName = nameof(pipelineType), columnType = Database_ColumnType.INTEGER, allowNull = true },

        new Database_Column { columnName = nameof(size), columnType = Database_ColumnType.INTEGER, allowNull = true },
        new Database_Column { columnName = nameof(lastOpened), columnType = Database_ColumnType.INTEGER, allowNull = true },
        new Database_Column { columnName = nameof(created), columnType = Database_ColumnType.INTEGER, allowNull = true },
        new Database_Column { columnName = nameof(notes), columnType = Database_ColumnType.TEXT, allowNull = true },

        new Database_Column { columnName = nameof(favourited), columnType = Database_ColumnType.BIT, allowNull = false, defaultValue = "0" },
    ];
}
