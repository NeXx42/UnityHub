using CSharpSqliteORM.Structure;

namespace Data_Sqlite.Tables;

public class dbo_UnityInstallInfo : IDatabase_Table
{
    public static string tableName => "UnityInstallInfo";

    public required string version { get; set; }
    public required string json { get; set; }


    public static Database_Column[] getColumns => [
        new Database_Column() { columnName = nameof(version), columnType = Database_ColumnType.TEXT, allowNull = false, isPrimaryKey = true },
        new Database_Column() { columnName = nameof(json), columnType = Database_ColumnType.TEXT, allowNull = false },
    ];
}
