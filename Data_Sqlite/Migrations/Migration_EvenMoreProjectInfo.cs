using CSharpSqliteORM;
using Data_Sqlite.Tables;

namespace Data_Sqlite.Migrations;

public class Migration_EvenMoreProjectInfo : IDatabase_Migration
{
    public long migrationId => new DateTime(2026, 7, 5, 17, 43, 10).Ticks;

    public string Up()
    {
        return $@"
            ALTER TABLE {dbo_Project.tableName} ADD COLUMN size INTEGER NULLABLE;
            ALTER TABLE {dbo_Project.tableName} ADD COLUMN lastOpened INTEGER NULLABLE;
            ALTER TABLE {dbo_Project.tableName} ADD COLUMN created INTEGER NULLABLE;
            ALTER TABLE {dbo_Project.tableName} ADD COLUMN notes TEXT NULLABLE;
        ";
    }
}
