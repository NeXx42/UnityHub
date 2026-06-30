using CSharpSqliteORM.Structure;

namespace Data_Sqlite.Tables;

public class dbo_ProjectTag : IDatabase_Table
{
    public static string tableName => "ProjectTag";

    public required int ProjectId { get; set; }
    public required int TagId { get; set; }

    public static Database_Column[] getColumns => [
        new Database_Column() { columnName = nameof(ProjectId), columnType = Database_ColumnType.INTEGER, allowNull = false },
        new Database_Column() { columnName = nameof(TagId), columnType = Database_ColumnType.INTEGER, allowNull = false },
    ];
}
