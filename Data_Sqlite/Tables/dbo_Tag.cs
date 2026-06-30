using CSharpSqliteORM.Structure;

namespace Data_Sqlite.Tables;

public class dbo_Tag : IDatabase_Table
{
    public static string tableName => "Tag";

    public int Id { get; set; }
    public required string Name { get; set; }
    public string? Colour { get; set; }

    public static Database_Column[] getColumns => [
        new Database_Column() { columnName = nameof(Id), columnType = Database_ColumnType.INTEGER, allowNull = false, autoIncrement = true, isPrimaryKey = true },
        new Database_Column() { columnName = nameof(Name), columnType = Database_ColumnType.TEXT, allowNull = false },
        new Database_Column() { columnName = nameof(Colour), columnType = Database_ColumnType.TEXT, allowNull = true },
    ];
}
