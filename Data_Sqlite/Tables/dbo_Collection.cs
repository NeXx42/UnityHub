using CSharpSqliteORM.Structure;

namespace Data_Sqlite.Tables;

public class dbo_Collection : IDatabase_Table
{
    public static string tableName => "Collection";

    public int Id { get; set; }
    public required string Name { get; set; }
    public string? Colour { get; set; }
    public int HandlingType { get; set; }

    public static Database_Column[] getColumns => [
        new Database_Column() { columnName = nameof(Id), columnType = Database_ColumnType.INTEGER, allowNull = false, autoIncrement = true, isPrimaryKey = true },
        new Database_Column() { columnName = nameof(Name), columnType = Database_ColumnType.TEXT, allowNull = false },
        new Database_Column() { columnName = nameof(Colour), columnType = Database_ColumnType.TEXT, allowNull = true },
        new Database_Column() { columnName = nameof(HandlingType), columnType = Database_ColumnType.INTEGER, allowNull = false, defaultValue = "0" },
    ];
}
