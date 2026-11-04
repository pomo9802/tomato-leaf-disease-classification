using System;

namespace TomatoLeafClassification.Exceptions
{
    /// <summary>
    /// Raised when a file that the dataset scan accepted as an image cannot actually be decoded
    /// </summary>
    public class InvalidImageFormatException : DatasetException
    {
        private readonly string _filePath;

        /// <summary>
        /// Creates the exception for a specific file.
        /// </summary>
        /// <param name="filePath">Path of the file that could not be decoded.</param>
        /// <param name="reason">Short explanation of why the file was rejected.</param>
        public InvalidImageFormatException(string filePath, string reason) : base("Cannot read the image " + filePath + ": " + reason + ".")
        {
            _filePath = filePath;
        }

        /// <summary>
        /// Overload that also keeps the original framework exception.
        /// </summary>
        /// <param name="filePath">Path of the file that could not be decoded.</param>
        /// <param name="reason">Short explanation of why the file was rejected.</param>
        /// <param name="innerException">The exception raised by the imaging library.</param>
        public InvalidImageFormatException(string filePath, string reason, Exception innerException)
            : base("Cannot read the image '" + filePath + "': " + reason + ".", innerException)
        {
            _filePath = filePath;
        }

        /// <summary>
        /// Path of the file that could not be decoded.
        /// </summary>
        public string FilePath
        {
            get { return _filePath; }
        }
    }
}
