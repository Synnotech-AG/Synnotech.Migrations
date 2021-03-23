using System;
using System.Threading.Tasks;

namespace Synnotech.Migrations.Core
{
    /// <summary>
    /// Represents an async session with the target system. This session can be disposed via
    /// <see cref="IDisposable" />. Changes to the target system can also be committed by calling <see cref="SaveChangesAsync" />.
    /// </summary>
    public interface IAsyncSession : IDisposable
    {
        /// <summary>
        /// Commits the changes made in this session to target system.
        /// </summary>
        Task SaveChangesAsync();
    }
}