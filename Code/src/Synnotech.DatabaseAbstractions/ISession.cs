using System;

namespace Synnotech.DatabaseAbstractions
{
    /// <summary>
    /// Represents an abstraction of a database session
    /// that is able to manipulate data in the target system.
    /// The session should be disposed via <see cref="IDisposable"/>.
    /// Changes to the target system can be committed by calling <see cref="SaveChanges" />.
    /// </summary>
    public interface ISession : IDisposable
    {
        /// <summary>
        /// Stores all changes in the target system.
        /// </summary>
        void SaveChanges();
    }
}