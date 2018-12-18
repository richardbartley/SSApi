// See here for multiple migrations support and process.
// https://docs.microsoft.com/en-us/ef/core/managing-schemas/migrations/providers


Add-Migration InitialCreate -Context DataContext -OutputDir Migrations\SqlServerMigrations
Add-Migration InitialCreate -Context SqliteContext -OutputDir Migrations\SqliteMigrations

VS2017 Package Manager Console (Make sure right project is selected in Package Manager Console).
Update-database -Context DataContext
Update-database -Context SqliteContext

VSCODE
dotnet ef database update
