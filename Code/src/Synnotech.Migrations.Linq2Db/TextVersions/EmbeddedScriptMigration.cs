﻿using System;
using System.Threading;
using System.Threading.Tasks;
using Light.EmbeddedResources;
using Light.GuardClauses;
using LinqToDB.Data;

namespace Synnotech.Migrations.Linq2Db.TextVersions
{
    /// <summary>
    /// Represents a migration that executes a single SQL script against the target
    /// database. This script must be an embedded resource that resides within the
    /// same namespace as your deriving migration class.
    /// </summary>
    public abstract class EmbeddedScriptMigration : Migration
    {
        /// <summary>
        /// Initializes a new instance of <see cref="EmbeddedScriptMigration" />.
        /// </summary>
        /// <param name="scriptName">
        /// The name of the embedded file that contains the SQL script. You must only provide
        /// the name of the file, the namespace will be derived automatically from your deriving class.
        /// </param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="scriptName" /> is null.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="scriptName" /> is an empty string or contains only white space.</exception>
        protected EmbeddedScriptMigration(string scriptName) =>
            ScriptName = scriptName.MustNotBeNullOrWhiteSpace(nameof(scriptName));

        private string ScriptName { get; }

        /// <summary>
        /// Executes the embedded SQL script against the target database.
        /// </summary>
        public sealed override Task ApplyAsync(DataConnection dataConnection, CancellationToken cancellationToken = default) =>
            dataConnection.ExecuteAsync(this.GetEmbeddedResource(ScriptName), cancellationToken);
    }
}