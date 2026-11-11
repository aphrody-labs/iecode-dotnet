using System;

namespace IEVRModManager.Exceptions
{
    /// <summary>
    /// Exception thrown when there's an error loading or saving configuration.
    /// </summary>
    public class ConfigurationException : ModManagerException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ConfigurationException"/> class.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        public ConfigurationException(string message) : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ConfigurationException"/> class.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        /// <param name="innerException">The exception that is the cause of the current exception.</param>
        public ConfigurationException(string message, Exception innerException) 
            : base(message, innerException)
        {
        }
    }
}
