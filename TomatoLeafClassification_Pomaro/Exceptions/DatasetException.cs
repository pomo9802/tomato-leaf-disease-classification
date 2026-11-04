using System;

namespace TomatoLeafClassification.Exceptions
{
    /// <summary>
    /// Base class for the domain errors of this project: everything that can go wrong while
    /// locating, reading or interpreting the dataset.
    /// </summary>
    public class DatasetException : Exception
    {
        /// <summary>
        /// Creates the exception with a message describing the problem.
        /// </summary>
        /// <param name="message">Description of what went wrong.</param>
        public DatasetException(string message) : base(message)
        {
        }

        /// <summary>
        /// Overload that also keeps the original error. Used when a framework exception is
        /// translated into a domain one: the cause is not lost.
        /// </summary>
        /// <param name="message">Description of what went wrong.</param>
        /// <param name="innerException">The exception that caused this one.</param>
        public DatasetException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
