using System;

namespace IEVRModManager.Exceptions
{
    /// <summary>
    /// Base exception class for all ModManager-related exceptions.
    /// </summary>
    public class ModManagerException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ModManagerException"/> class.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        public ModManagerException(string message) : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ModManagerException"/> class.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        /// <param name="innerException">The exception that is the cause of the current exception.</param>
        public ModManagerException(string message, Exception innerException) 
            : base(message, innerException)
        {
        }
    }
}
