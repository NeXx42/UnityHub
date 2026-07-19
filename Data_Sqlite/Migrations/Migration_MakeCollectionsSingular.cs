using CSharpSqliteORM;
using Models.Enums;

namespace Data_Sqlite.Migrations;

public class Migration_MakeCollectionsSingular : IDatabase_Migration
{
    public long migrationId => new DateTime(2026, 7, 19, 14, 58, 10).Ticks;

    public string Up()
    {
        return @$"
            DROP TABLE ProjectCollection;
            ALTER TABLE ProjectData ADD COLUMN collectionId INTEGER DEFAULT 1;
            ALTER TABLE Collection ADD COLUMN HandlingType INTEGER DEFAULT {DefaultCollectionIds.InDevelopment};
        ";
    }
}
