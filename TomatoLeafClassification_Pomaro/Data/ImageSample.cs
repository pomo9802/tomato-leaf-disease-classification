using System;
using System.Drawing;
using System.IO;
using TomatoLeafClassification.Exceptions;

namespace TomatoLeafClassification.Data
{
    /// <summary>
    /// One labelled image on disk. It stores where the image is and which class it belongs to.
    /// </summary>
    public class ImageSample
    {
        private string _filePath;
        private string _className;
        private int _label;
        private double[] _features;

        /// <summary>
        /// Creates a sample from its file path and class name leaving the label unset.
        /// </summary>
        /// <exception cref="ArgumentException">If the path or the class name is empty.</exception>
        public ImageSample(string filePath, string className)
        {
            if (string.IsNullOrEmpty(filePath))
                throw new ArgumentException("The file path cannot be empty.", "filePath");
            if (string.IsNullOrEmpty(className))
                throw new ArgumentException("The class name cannot be empty.", "className");

            _filePath = filePath;
            _className = className;
            _label = -1;
            _features = null;
        }

        public string FilePath
        {
            get { return _filePath; }
        }

        public string ClassName
        {
            get { return _className; }
        }

        /// <summary>
        /// Integer label, assigned by the LabelEncoder after loading. The setter refuses negative
        /// ids: -1 is the "not encoded yet".
        /// </summary>
        public int Label
        {
            get { return _label; }
            set
            {
                if (value < 0)
                    throw new ArgumentOutOfRangeException("value", "The class label cannot be negative.");
                _label = value;
            }
        }

        /// <summary>
        /// Extracted feature vector, filled by a feature extractor.
        /// </summary>
        public double[] Features
        {
            get { return _features; }
            set
            {
                if (value == null || value.Length == 0)
                    throw new ArgumentException("The feature vector cannot be null or empty.", "value");
                _features = value;
            }
        }

        /// <summary>
        /// Loads the image and resizes it to a square of the requested side.
        /// </summary>
        /// <exception cref="InvalidImageFormatException">
        /// If the file cannot be decoded as an image.
        /// </exception>
        public Bitmap LoadBitmap(int size)
        {
            try
            {
                using (Bitmap original = new Bitmap(_filePath))
                {
                    return new Bitmap(original, new Size(size, size));
                }
            }
            catch (FileNotFoundException exception)
            {
                throw new InvalidImageFormatException(_filePath, "the file no longer exists", exception);
            }
            catch (OutOfMemoryException exception)
            {
                throw new InvalidImageFormatException(_filePath, "the image data is corrupted or unsupported", exception);
            }
            catch (ArgumentException exception)
            {
                throw new InvalidImageFormatException(_filePath, "the file is not a readable image", exception);
            }
        }

        /// <summary>
        /// Override to provide a human-readable description of the sample, including its file name,
        /// class name, label, and feature vector length.
        /// </summary>
        public override string ToString()
        {
            string featureState;
            if (_features == null) featureState = "no features";
            else featureState = _features.Length + " features";

            return Path.GetFileName(_filePath) + " [" + _className
                 + ", id " + _label + ", " + featureState + "]";
        }
    }
}
