using System.Collections.Generic;
using System.IO;
using TomatoLeafClassification.Exceptions;

namespace TomatoLeafClassification.Data
{
    /// <summary>
    /// A collection of labelled image samples. The static factory merges several source directories
    /// (for example the original "train" and "test" folders) into a single dataset, so the
    /// train/test split can be decided later.
    /// </summary>
    public class Dataset
    {
        private List<ImageSample> _samples;
        private LabelEncoder _encoder;

        /// <summary>
        /// Private constructor; instances are created through <see cref="FromDirectories"/>.
        /// </summary>
        private Dataset(List<ImageSample> samples, LabelEncoder encoder)
        {
            _samples = samples;
            _encoder = encoder;
        }

        public List<ImageSample> Samples
        {
            get { return _samples; }
        }

        public LabelEncoder Encoder
        {
            get { return _encoder; }
        }

        public int Count
        {
            get { return _samples.Count; }
        }

        /// <summary>
        /// True if the file name ends with a supported image extension.
        /// </summary>
        private static bool IsImageFile(string file)
        {
            string extension = Path.GetExtension(file).ToLowerInvariant();
            return extension == ".jpg" || extension == ".jpeg" || extension == ".png" || extension == ".bmp";
        }

        /// <summary>
        /// Builds a single merged dataset from one or more root directories. Each root is expected
        /// to contain one sub-folder per class; images from folders with the same name across roots
        /// share the same label.
        /// </summary>
        /// <exception cref="DatasetException">
        /// If no root is given, a root is missing, or no image is found.
        /// </exception>
        public static Dataset FromDirectories(string[] roots)
        {
            if (roots == null || roots.Length == 0)
                throw new DatasetException("At least one root directory is required.");

            List<ImageSample> samples = new List<ImageSample>();
            List<string> classNames = new List<string>();

            foreach (string root in roots)
            {
                if (!Directory.Exists(root))
                    throw new DatasetException("Root directory not found: " + root);

                CollectFromRoot(root, samples, classNames);
            }

            if (samples.Count == 0)
                throw new DatasetException("No supported image file was found in the given directories.");

            LabelEncoder encoder = new LabelEncoder();
            encoder.Fit(classNames);
            foreach (ImageSample sample in samples)
                sample.Label = encoder.Encode(sample.ClassName);

            return new Dataset(samples, encoder);
        }

        /// <summary>
        /// Scans one root directory: every sub-folder is a class, every supported image inside it
        /// becomes a sample.
        /// </summary>
        private static void CollectFromRoot(string root, List<ImageSample> samples, List<string> classNames)
        {
            string[] classDirectories = Directory.GetDirectories(root);
            foreach (string classDirectory in classDirectories)
            {
                string className = Path.GetFileName(classDirectory);
                string[] files = Directory.GetFiles(classDirectory);
                foreach (string file in files)
                {
                    if (IsImageFile(file))
                    {
                        samples.Add(new ImageSample(file, className));
                        classNames.Add(className);
                    }
                }
            }
        }

        /// <summary>
        /// Number of samples per class name.
        /// </summary>
        public Dictionary<string, int> ClassDistribution()
        {
            Dictionary<string, int> counts = new Dictionary<string, int>();
            foreach (ImageSample sample in _samples)
            {
                int current;
                if (counts.TryGetValue(sample.ClassName, out current))
                    counts[sample.ClassName] = current + 1;
                else
                    counts[sample.ClassName] = 1;
            }
            return counts;
        }

        /// <summary>
        /// Override of ToString() to show the number of samples and classes in the dataset.
        /// </summary>
        public override string ToString()
        {
            return "Dataset: " + _samples.Count + " samples, " + _encoder.Count + " classes";
        }
    }
}
