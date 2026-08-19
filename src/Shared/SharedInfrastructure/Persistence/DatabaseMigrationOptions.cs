namespace SharedInfrastructure.Persistence;

public sealed class DatabaseMigrationOptions
{
    public const string SectionName = "Database:Migration";

    public bool RunOnStartup { get; set; } = true;
}
