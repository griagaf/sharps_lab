using DbUp.Engine;
using Lab4.Persistence;
using Npgsql;
using Testcontainers.PostgreSql;

namespace Lab4.Tests.Integration;

// Один postgres в докере на все тесты класса, со схемой из тех же миграций, что у программы.
public class PostgresFixture : IAsyncLifetime
{
    private readonly PostgreSqlContainer _container = new PostgreSqlBuilder("postgres:18").Build();

    public NpgsqlDataSource DataSource { get; private set; } = null!;

    public async Task InitializeAsync()
    {
        await _container.StartAsync();
        string connectionString = _container.GetConnectionString();

        DatabaseUpgradeResult migration = DatabaseMigrator.Migrate(connectionString);

        if (!migration.Successful)
        {
            throw migration.Error;
        }

        DataSource = NpgsqlDataSource.Create(connectionString);
    }

    public async Task DisposeAsync()
    {
        await DataSource.DisposeAsync();
        await _container.DisposeAsync();
    }
}
