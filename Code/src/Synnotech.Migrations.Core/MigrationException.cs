using System;
using System.Runtime.Serialization;

namespace Synnotech.Migrations.Core
{
    /// <summary>
    /// Represents an error that occurred during a migration. The cause is usually misconfiguration of the migration engine.
    /// </summary>
    [Serializable]
    public class MigrationException : Exception
    {
        /// <summary>
        /// Initializes a new instance of <see cref="MigrationException" />.
        /// </summary>
        /// <param name="message">The message of the exception.</param>
        /// <param name="innerException">The optional inner exception that led to this exception.</param>
        public MigrationException(string message, Exception? innerException = null) : base(message, innerException) { }

        /// <summary>
        /// Initializes a new instance of <see cref="MigrationException" /> with deserialized data.
        /// </summary>
        protected MigrationException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }
}