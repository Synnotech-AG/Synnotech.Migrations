using System;
using System.Threading.Tasks;

namespace Synnotech.DatabaseAbstractions
{
    /// <summary>
    /// Represents an abstraction of an asynchronous database session
    /// that is able to manipulate data in the target system.
    /// The session should be disposed.
    /// Changes to the target system can be committed by calling <see cref="SaveChangesAsync" />.
    /// </summary>
    public interface IAsyncSession : IDisposable, IAsyncDisposable
    {
        /// <summary>
        /// Stores all changes in the target system asynchronously.
        /// </summary>
        Task SaveChangesAsync();
    }
}