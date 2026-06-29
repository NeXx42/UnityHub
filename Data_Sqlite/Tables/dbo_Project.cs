using CSharpSqliteORM.Structure;

namespace Data_Sqlite.Tables;

public class dbo_Project : IDatabase_Table
{
    public static string tableName => "ProjectData";

    public int id { get; set; }

    public string? name { get; set; }
    public required string directory { get; set; }

    public static Database_Column[] getColumns => [
        new Database_Column { columnName = nameof(id), columnType = Database_ColumnType.INTEGER, isPrimaryKey = true, autoIncrement = true, allowNull = false },

        new Database_Column { columnName = nameof(name), columnType = Database_ColumnType.TEXT, allowNull = true },
        new Database_Column { columnName = nameof(directory), columnType = Database_ColumnType.TEXT, allowNull = false },
    ];
}
