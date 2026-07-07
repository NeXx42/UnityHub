using CSharpSqliteORM;
using Data_Sqlite.Tables;

namespace Data_Sqlite.Migrations;

public class Migration_ProjectFavs : IDatabase_Migration
{
    public long migrationId => new DateTime(2026, 7, 7, 19, 47, 10).Ticks;

    public string Up()
    {
        return @$"ALTER TABLE {dbo_Project.tableName} ADD COLUMN favourited BIT NOT NULL DEFAULT 0";
    }
}
