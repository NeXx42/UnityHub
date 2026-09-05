using CSharpSqliteORM.Structure;

namespace Data_Sqlite.Tables;

public class dbo_ProjectMetadata : IDatabase_Table
{
    public static string tableName => "ProjectMetadata";

    public required int ProjectId { get; set; }

    public required string Key { get; set; }
    public required string Value { get; set; }

    public static Database_Column[] getColumns => [
        new Database_Column() { columnName = nameof(ProjectId), columnType = Database_ColumnType.INTEGER, allowNull = false },

        new Database_Column() { columnName = nameof(Key), columnType = Database_ColumnType.TEXT, allowNull = false },
        new Database_Column() { columnName = nameof(Value), columnType = Database_ColumnType.TEXT, allowNull = false },
    ];
}
