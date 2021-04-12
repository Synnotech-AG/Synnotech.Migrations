using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Light.GuardClauses;
using Microsoft.Data.SqlClient;

namespace Synnotech.SqlServer
{
    /// <summary>
    /// Provides helper methods for creating MS SQL databases.
    /// </summary>
    public static class Database
    {
        /// <summary>
        /// Creates the database for the specified connection string. If it already exists, the
        /// database will be dropped and recreated. Existing connections to the existing database
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
            var targetDatabaseName = connectionStringBuilder.InitialCatalog;
            connectionStringBuilder.InitialCatalog = "master";
#if NETSTANDARD2_0
            using var masterConnection = 
#else
            await using var masterConnection =
#endif
                new SqlConnection(connectionStringBuilder.ConnectionString);

            await masterConnection.OpenAsync();

            SqlCommand command;
#if NETSTANDARD2_0
            using (command = masterConnection.CreateCommand())
#else
            await using (command = masterConnection.CreateCommand())
#endif
            {
                await command.ExecuteKillAllDatabaseConnectionsAsync(targetDatabaseName);
            }

#if NETSTANDARD2_0
            using (command = masterConnection.CreateCommand())
#else
            await using (command = masterConnection.CreateCommand())
#endif
            {
                await command.ExecuteDropAndCreateDatabaseAsync(targetDatabaseName);
            }
        }

        /// <summary>
        /// Executes a T-SQL statement (non-query) that terminates all existing
        /// connections to the target database, using the specified command.
        /// </summary>
        /// <param name="command">
        /// The SQL command that will be used to execute the terminate-connections statement.
        /// It must be associated with a <see cref="SqlConnection"/> that targets the
        /// master database of a SQL server.
        /// </param>
        /// <param name="databaseName">The name of the target database.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="command"/> or <paramref name="databaseName"/> are null.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="databaseName"/> is an empty string or contains only white space.</exception>
        /// <exception cref="SqlException">Thrown when the command execution fails.</exception>
        public static Task<int> ExecuteKillAllDatabaseConnectionsAsync(this SqlCommand command, string databaseName)
        {
            command.MustNotBeNull(nameof(command));
            databaseName.MustNotBeNullOrWhiteSpace(nameof(databaseName));

            command.CommandType = CommandType.Text;
            command.CommandText = @"
DECLARE @kill varchar(1000) = '';
SELECT @kill = @kill + 'kill ' + CONVERT(varchar(5), session_id) + ';'
FROM sys.dm_exec_sessions
WHERE database_id = db_id(@DatabaseName);

EXEC(@kill);
";
            command.Parameters.AddWithValue("@DatabaseName", databaseName);
            return command.ExecuteNonQueryAsync();
        }

        /// <summary>
        /// Executes a T-SQL statement (non-query) that creates a new database with the specified name,
        /// using the specified command. If the target database already exists, it will be dropped.
        /// </summary>
        /// <param name="command">
        /// The SQL command that will be used to execute the statement.
        /// It must be associated with a <see cref="SqlConnection"/> that targets the
        /// master database of a SQL server.
        /// </param>
        /// <param name="databaseName">The name of the target database.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="command"/> or <paramref name="databaseName"/> are null.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="databaseName"/> is an empty string or contains only white space.</exception>
        /// <exception cref="SqlException">Thrown when the command execution fails.</exception>
        public static Task<int> ExecuteDropAndCreateDatabaseAsync(this SqlCommand command, string databaseName)
        {
            command.MustNotBeNull(nameof(command));
            databaseName.MustNotBeNullOrWhiteSpace(nameof(databaseName));

            command.CommandType = CommandType.Text;
            command.CommandText = @"
IF db_id(@DatabaseName) IS NOT NULL
DROP DATABASE {targetDatabaseName};

CREATE DATABASE @DatabaseName;
";
            command.Parameters.AddWithValue("@DatabaseName", databaseName);
            return command.ExecuteNonQueryAsync();
        }
    }
}
