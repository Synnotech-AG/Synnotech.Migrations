# Synnotech.Migrations

*A lightweight generic migration engine for .NET*

[![Synnotech Logo](synnotech-large-logo.png)](https://www.synnotech.de/)

[![License](https://img.shields.io/badge/License-MIT-green.svg?style=for-the-badge)](https://github.com/Synnotech-AG/Synnotech.Migrations/blob/main/LICENSE)
[![NuGet](https://img.shields.io/badge/NuGet-0.8.0-blue.svg?style=for-the-badge)](https://www.nuget.org/packages?q=Synnotech.Migrations)

When starting a new project, do you always spent too much time on how you can migrate your database? Do you want a migration mechanism that is independent of any persistence technology, like ORMs for relational databases? Worry no more because **Synnotech.Migrations** offers just that! Our generic migration engine is written in such a way so that you can easily adapt it to your own data access layer, no matter the technology: relational databases with and without ORMs, document or graph databases, the file system, or web services - the possibilities are endless.

**Synnotech.Migrations** also offers integration packages that get you started quickly:

- [Raven DB](https://github.com/Synnotech-AG/Synnotech.Migrations/tree/main/Code/src/Synnotech.Migrations.RavenDB)
- Linq2Db *to be completed*
- Entity Framework Core *to be completed*
- Entity Framework *to be completed*

Please visit the corresponding pages for further documentation.

## Our Philosophy for Migrations

- **The dev is in control**: Synnotech.Migrations is unobtrusive, it does not care about the internal structure of your migrations. We recommend choosing the best tools to manipulate your target system, e.g. TSQL for MS SQL Server, PL/SQL for Oracle, RQL or the RavenDB.Client for Raven DB, HTTP calls for a web service. Simply execute the code you want within your migrations. 
- **We don't look back**: Migrations should not have a `Down` method to reverse it. Not only does this extend the time you need to implement a migration, but it also might lead to data loss, e.g. when a column or table is dropped that was previously added. If one of your migrations is erroneous, write a new one that fixes it.
- **No addtional CLI tools**: Synnotech.Migrations offers you an API that you can directly call within your apps. You don't need additional tools for the command line.

## Supported Frameworks

**Synnotech.Migrations** is build for .NET Standard 2.0 and .NET Standard 2.1, so you can use it on [all platforms that support it](https://docs.microsoft.com/en-us/dotnet/standard/net-standard), e.g. like:

- .NET 5 or newer
- .NET Core 2.0 or newer
- .NET Framework 4.6.1 or newer
- Mono 4.6 or newer
- Unity 2018.1

## Contributions Are Welcome

If you have questions or have an idea for a new feature, please create an issue.

*TODO: we need to create contributing guidelines*
