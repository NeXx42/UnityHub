using CSharpSqliteORM;
using Data_Sqlite.Tables;

namespace Data_Sqlite.Migrations;

public class Migration_ProjectInfoIcon : IDatabase_Migration
{
    public long migrationId => new DateTime(2026, 6, 30, 18, 17, 10).Ticks;

    public string Up()
    {
        return $@"
            ALTER TABLE {dbo_Project.tableName} ADD COLUMN iconPath TEXT NULLABLE;
        ";
    }
}
