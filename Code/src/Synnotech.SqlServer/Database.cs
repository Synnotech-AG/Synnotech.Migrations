﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Light.GuardClauses;
using Microsoft.Data.SqlClient;

namespace Synnotech.SqlServer
{
    /// <summary>
    /// Provides helper methods for creating and dropping MS SQL databases.
    /// </summary>
    public static class Database
    {
        /// <summary>
        /// Tries to drop the database for the specified connection string.
        /// This method will connect to the "master" database of the target
        /// SQL server to do this - please ensure that the credentials in the connection string
        /// have enough privileges to perform this operation.
        /// </summary>
        /// <param name="connectionString">The connection string that identifies the target database.</param>
        /// <exception cref="KeyNotFoundException">Invalid key name within the connection string.</exception>
        /// <exception cref="FormatException">Invalid value within the connection string (specifically, when a Boolean or numeric value was expected but not supplied).</exception>
        /// <exception cref="ArgumentException">The supplied connectionString is not valid.</exception>
        /// <exception cref="SqlException">Thrown when the connection to the master database fails or when the command fails to execute.</exception>
        public static async Task<bool> TryDropDatabase(string connectionString)
        {
            var connectionStringBuilder = new SqlConnectionStringBuilder(connectionString);
            DatabaseName targetDatabaseName = connectionStringBuilder.InitialCatalog;
            connectionStringBuilder.InitialCatalog = "master";
#if NETSTANDARD2_0
            using var connectionToMaster =
#else
            await using var connectionToMaster =
#endif
                new SqlConnection(connectionStringBuilder.ConnectionString);

            await connectionToMaster.OpenAsync();
            await connectionToMaster.KillAllDatabaseConnectionsAsync(targetDatabaseName);
            var result = await connectionToMaster.TryDropDatabaseAsync(targetDatabaseName);

            return result > 0;

        }

        /// <summary>
        /// Creates the database for the specified connection string. If it already exists, the
        /// database will be dropped and recreated. Connections to the existing database
        /// will be terminated. This method will connect to the "master" database of the target
        /// SQL server to do this - please ensure that the credentials in the connection string
        /// have enough privileges to perform this operation.
        /// </summary>
        /// <param name="connectionString">The connection string that identifies the target database.</param>
        /// <exception cref="KeyNotFoundException">Invalid key name within the connection string.</exception>
        /// <exception cref="FormatException">Invalid value within the connection string (specifically, when a Boolean or numeric value was expected but not supplied).</exception>
        /// <exception cref="ArgumentException">The supplied connectionString is not valid.</exception>
        /// <exception cref="SqlException">Thrown when the connection to the master database fails.</exception>
        public static async Task DropAndCreateDatabaseAsync(string connectionString)
        {
            var connectionStringBuilder = new SqlConnectionStringBuilder(connectionString);
            DatabaseName targetDatabaseName = connectionStringBuilder.InitialCatalog;
            connectionStringBuilder.InitialCatalog = "master";
#if NETSTANDARD2_0
            using var connectionToMaster =
#else
            await using var connectionToMaster =
#endif
                new SqlConnection(connectionStringBuilder.ConnectionString);

            await connectionToMaster.OpenAsync();
            await connectionToMaster.KillAllDatabaseConnectionsAsync(targetDatabaseName);
            await connectionToMaster.DropAndCreateDatabaseAsync(targetDatabaseName);
        }

        /// <summary>
        /// Execute a T-SQL command (non-query) that kills all active connections to the database
        /// with the specified name. It is safe to run this command when the target database does
        /// not exist.
        /// </summary>
        /// <param name="connectionToMaster">
        /// The SQL connection that will be used to execute the command.
        /// It must target the master database of a SQL server.
        /// </param>
        /// <param name="databaseName">The name of the target database.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="connectionToMaster"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="databaseName"/> is the default instance.</exception>
        /// <exception cref="SqlException">Thrown when the command fails to execute.</exception>
        public static async Task<int> KillAllDatabaseConnectionsAsync(this SqlConnection connectionToMaster, DatabaseName databaseName)
        {
            connectionToMaster.MustNotBeNull(nameof(connectionToMaster));
            databaseName.MustNotBeDefault(nameof(databaseName));

#if NETSTANDARD2_0
            using var command = connectionToMaster.CreateCommand();
#else
            await using var command = connectionToMaster.CreateCommand();
#endif

            command.CommandType = CommandType.Text;
            command.CommandText = $@"
DECLARE @kill varchar(1000) = '';
SELECT @kill = @kill + 'kill ' + CONVERT(varchar(5), session_id) + ';'
FROM sys.dm_exec_sessions
WHERE database_id = db_id('{databaseName}');

EXEC(@kill);
";
            return await command.ExecuteNonQueryAsync();
        }

        /// <summary>
        /// Execute a T-SQL command (non-query) that creates a new database. If the database
        /// already exists, it will be dropped and recreated.
        /// </summary>
        /// <param name="connectionToMaster">
        /// The SQL connection that will be used to execute the command.
        /// It must target the master database of a SQL server.
        /// </param>
        /// <param name="databaseName">The name of the target database.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="connectionToMaster"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="databaseName"/> is the default instance.</exception>
        /// <exception cref="SqlException">Thrown when the command fails to execute.</exception>
        public static async Task<int> DropAndCreateDatabaseAsync(this SqlConnection connectionToMaster, DatabaseName databaseName)
        {
            connectionToMaster.MustNotBeNull(nameof(connectionToMaster));
            databaseName.MustNotBeDefault(nameof(databaseName));

#if NETSTANDARD2_0
            using var command = connectionToMaster.CreateCommand();
#else
            await using var command = connectionToMaster.CreateCommand();
#endif
            command.CommandType = CommandType.Text;
            command.CommandText = $@"
IF db_id('{databaseName}') IS NOT NULL
DROP DATABASE {databaseName};

CREATE DATABASE {databaseName};
";
            return await command.ExecuteNonQueryAsync();
        }

        /// <summary>
        /// Executes a T-SQL command (non-query) that drops a database if it exists.
        /// </summary>
        /// <param name="connectionToMaster">
        /// The SQL connection that will be used to execute the command.
        /// It must target the master database of a SQL server.
        /// </param>
        /// <param name="databaseName">The name of the target database.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="connectionToMaster"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="databaseName"/> is the default instance.</exception>
        /// <exception cref="SqlException">Thrown when the command fails to execute.</exception>
        public static async Task<int> TryDropDatabaseAsync(this SqlConnection connectionToMaster, DatabaseName databaseName)
        {
            connectionToMaster.MustNotBeNull(nameof(connectionToMaster));
            databaseName.MustNotBeDefault(nameof(databaseName));

#if NETSTANDARD2_0
            using var command = connectionToMaster.CreateCommand();
#else
            await using var command = connectionToMaster.CreateCommand();
#endif
            command.CommandType = CommandType.Text;
            command.CommandText = $@"
IF db_id({databaseName}) IS NOT NULL
DROP DATABASE {databaseName};
";
            return await command.ExecuteNonQueryAsync();
        }
    }
}
