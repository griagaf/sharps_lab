using DbUp;
using DbUp.Engine;

namespace Lab4.Persistence;

public static class DatabaseMigrator
{
    // Выполняет ещё не применённые скрипты из Migrations/ по порядку имён.
    // Применённые DbUp записывает в таблицу schemaversions.
    public static DatabaseUpgradeResult Migrate(string connectionString)
    {
        UpgradeEngine upgrader = DeployChanges.To
            .PostgresqlDatabase(connectionString)
            .WithScriptsEmbeddedInAssembly(typeof(DatabaseMigrator).Assembly)
            .WithTransactionPerScript()
            .LogToNowhere()
            .Build();

        return upgrader.PerformUpgrade();
    }
}
