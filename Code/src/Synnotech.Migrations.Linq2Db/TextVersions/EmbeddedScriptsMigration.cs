using System;
using System.Threading;
using System.Threading.Tasks;
using Light.EmbeddedResources;
using Light.GuardClauses;
using LinqToDB.Data;

namespace Synnotech.Migrations.Linq2Db.TextVersions
{
    /// <summary>
    /// Represents a migration that executes several SQL scripts against the target
    /// database. The scripts must all be embedded resources that reside within the same
    /// namespace as your deriving migration class.
    /// </summary>
    public abstract class EmbeddedScriptsMigration : Migration
    {
        /// <summary>
        /// Initializes a new instance of <see cref="EmbeddedScriptsMigration" />
        /// </summary>
        /// <param name="scriptNames">The names of the scripts that will be executed. The order matters (of course!).</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="scriptNames"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="scriptNames"/> is empty.</exception>
        protected EmbeddedScriptsMigration(params string[] scriptNames) =>
            ScriptNames = scriptNames.MustNotBeNullOrEmpty(nameof(scriptNames));

        private string[] ScriptNames { get; }

        /// <summary>
        /// Executes the embedded SQL scripts against the target database.
        /// </summary>
        public sealed override async Task ApplyAsync(DataConnection dataConnection, CancellationToken cancellationToken = default)
        {
            foreach (var scriptName in ScriptNames)
            {
                await dataConnection.ExecuteAsync(this.GetEmbeddedResource(scriptName), cancellationToken);
            }
        }
    }
}