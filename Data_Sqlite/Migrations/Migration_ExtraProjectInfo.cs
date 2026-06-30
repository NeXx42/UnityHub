using CSharpSqliteORM;
using Data_Sqlite.Tables;

namespace Data_Sqlite.Migrations;

public class Migration_ExtraProjectInfo : IDatabase_Migration
{
    public long migrationId => new DateTime(2026, 6, 30, 17, 43, 10).Ticks;

    public string Up()
    {
        return $@"
            ALTER TABLE {dbo_Project.tableName} ADD COLUMN version TEXT NULLABLE;
            ALTER TABLE {dbo_Project.tableName} ADD COLUMN packageCount INTEGER NULLABLE;
            ALTER TABLE {dbo_Project.tableName} ADD COLUMN pipelineType INTEGER NULLABLE;
        ";
    }
}
