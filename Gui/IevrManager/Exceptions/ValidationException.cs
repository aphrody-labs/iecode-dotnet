using System;
using System.Collections.Generic;
using System.Linq;

namespace IEVRModManager.Exceptions
{
    /// <summary>
    /// Exception thrown when validation fails, containing a list of validation errors.
    /// </summary>
    public class ValidationException : ModManagerException
    {
        /// <summary>
        /// Gets the list of validation errors.
        /// </summary>
        public IReadOnlyList<string> ValidationErrors { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ValidationException"/> class.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        public ValidationException(string message) : base(message)
        {
            ValidationErrors = new[] { message };
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ValidationException"/> class.
        /// </summary>
        /// <param name="errors">The list of validation errors.</param>
        public ValidationException(IEnumerable<string> errors) 
            : base($"Validation failed: {string.Join("; ", errors)}")
        {
            ValidationErrors = errors.ToList().AsReadOnly();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ValidationException"/> class.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        /// <param name="errors">The list of validation errors.</param>
        public ValidationException(string message, IEnumerable<string> errors) 
            : base(message)
        {
            ValidationErrors = errors.ToList().AsReadOnly();
        }
    }
}
