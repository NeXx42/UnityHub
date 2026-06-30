using CSharpSqliteORM.Structure;

namespace Data_Sqlite.Tables;

public class dbo_ProjectCollection : IDatabase_Table
{
    public static string tableName => "ProjectCollection";

    public required int ProjectId { get; set; }
    public required int CollectionId { get; set; }

    public static Database_Column[] getColumns => [
        new Database_Column() { columnName = nameof(ProjectId), columnType = Database_ColumnType.INTEGER, allowNull = false },
        new Database_Column() { columnName = nameof(CollectionId), columnType = Database_ColumnType.INTEGER, allowNull = false },
    ];
}
