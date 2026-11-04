using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Text;
using TomatoLeafClassification.Data;
using TomatoLeafClassification.Exceptions;
using TomatoLeafClassification.Features;
using TomatoLeafClassification.Output;
using TomatoLeafClassification.Util;

namespace TomatoLeafClassification.Pipeline
{
    /// <summary>
    /// Stage 1: preprocessing. Builds one merged dataset from the "train" and "test" folders,
    /// extracts colour (HSV histogram) and texture (GLCM/Haralick) features from every image, and
    /// saves them to a single CSV.
    /// </summary>
    public static class FeatureExtractionPipeline
    {
        /// <summary>
        /// How many parent directories are searched when looking for the dataset.
        /// </summary>
        private const int MaximumSearchDepth = 8;

        /// <summary>
        /// Name of the folder that holds the dataset.
        /// </summary>
        private const string DatasetFolderName = "tomato_leaf_disease_dataset";

        /// <summary>
        /// Name of the folder inside it that holds 'train' and 'test'.
        /// </summary>
        private const string DatasetSubfolderName = "Tomato Leaf Disease";

        /// <summary>
        /// Width of the class-name column when printing the distribution.
        /// </summary>
        private const int ClassNameColumnWidth = 48;

        /// <summary>
        /// Runs the extraction stage and writes the features CSV.
        /// </summary>
        /// <exception cref="DatasetException">
        /// If the dataset cannot be located or contains no image.
        /// </exception>
        public static void Run()
        {
            string datasetRoot = ResolveDatasetRoot();
            string outputCsv = Constants.DefaultFeaturesCsv;

            Dataset dataset = LoadDataset(datasetRoot);
            PrintClassDistribution(dataset);

            IFeatureExtractor extractor = FeaturePipeline.BuildExtractor();
            Console.WriteLine("Feature vector length: " + extractor.FeatureCount);

            ExtractAll(dataset.Samples, extractor);

            Console.WriteLine("Writing " + outputCsv + " ...");
            FeatureCsvWriter.Write(outputCsv, dataset.Samples, extractor.FeatureNames, datasetRoot);
            Console.WriteLine("Finished. CSV ready to be split into train/test.");
        }

        /// <summary>
        /// Returns the dataset root, found by walking up from the executable folder.
        /// </summary>
        /// <exception cref="DatasetException">If the dataset cannot be located.</exception>
        private static string ResolveDatasetRoot()
        {
            string datasetRoot = FindDatasetRoot();

            if (datasetRoot == null)
                throw new DatasetException(DescribeMissingDataset());

            return datasetRoot;
        }

        /// <summary>
        /// Builds the message shown when the dataset is missing: it names the folders that are
        /// expected and lists every place that was searched, so the user can drop the dataset into
        /// one of them instead of guessing.
        /// </summary>
        private static string DescribeMissingDataset()
        {
            StringBuilder message = new StringBuilder();
            message.AppendLine("Could not locate the dataset.");
            message.AppendLine();
            message.AppendLine("It must be a folder laid out like this:");
            message.AppendLine("    " + DatasetFolderName);
            message.AppendLine("      " + DatasetSubfolderName);
            message.AppendLine("        train\\  (one subfolder per class)");
            message.AppendLine("        test\\   (one subfolder per class)");
            message.AppendLine();
            message.AppendLine("Searched in these places, nearest first:");

            foreach (string candidate in CandidateRoots())
                message.AppendLine("    " + candidate);

            return message.ToString();
        }

        /// <summary>
        /// Merges the 'train' and 'test' folders into a single labelled dataset.
        /// </summary>
        /// <exception cref="DatasetException">If a folder is missing or holds no image.</exception>
        private static Dataset LoadDataset(string datasetRoot)
        {
            Console.WriteLine("Loading and merging dataset...");
            string[] roots = new string[2];
            roots[0] = Path.Combine(datasetRoot, "train");
            roots[1] = Path.Combine(datasetRoot, "test");

            Dataset dataset = Dataset.FromDirectories(roots);
            Console.WriteLine("  " + dataset);
            return dataset;
        }

        /// <summary>
        /// Prints how many samples belong to each class, to expose the imbalance.
        /// </summary>
        private static void PrintClassDistribution(Dataset dataset)
        {
            foreach (KeyValuePair<string, int> entry in dataset.ClassDistribution())
                Console.WriteLine("    " + entry.Key.PadRight(ClassNameColumnWidth) + entry.Value);
        }

        /// <summary>
        /// Extracts the feature vector of every sample, reporting the progress. An unreadable image
        /// is reported and skipped instead of stopping the run: on a dataset of thousands of files
        /// a single corrupted one must not waste the whole extraction.
        /// </summary>
        private static void ExtractAll(List<ImageSample> samples, IFeatureExtractor extractor)
        {
            Console.WriteLine("Extracting features...");
            DateTime startTime = DateTime.Now;
            int skipped = 0;

            for (int s = 0; s < samples.Count; s++)
            {
                try
                {
                    ExtractOne(samples[s], extractor);
                }
                catch (InvalidImageFormatException exception)
                {
                    Console.WriteLine("  [skip] " + exception.Message);
                    skipped++;
                }
                catch (IOException exception)
                {
                    Console.WriteLine("  [skip] " + samples[s].FilePath + ": " + exception.Message);
                    skipped++;
                }

                if ((s + 1) % Constants.ProgressReportEvery == 0)
                {
                    int soFar = (int)(DateTime.Now - startTime).TotalSeconds;
                    Console.WriteLine("  " + (s + 1) + "/" + samples.Count
                                    + "  (" + FormatMinutesSeconds(soFar) + ")");
                }
            }

            int totalElapsed = (int)(DateTime.Now - startTime).TotalSeconds;
            Console.WriteLine("Extraction done in " + FormatMinutesSeconds(totalElapsed)
                            + ", " + skipped + " images skipped.");
        }

        /// <summary>
        /// Loads one image at the configured size and stores its feature vector.
        /// </summary>
        /// <exception cref="InvalidImageFormatException">If the image cannot be decoded.</exception>
        private static void ExtractOne(ImageSample sample, IFeatureExtractor extractor)
        {
            using (Bitmap bitmap = sample.LoadBitmap(Constants.ImageSize))
            {
                sample.Features = extractor.Extract(bitmap);
            }
        }

        /// <summary>
        /// Formats a number of seconds as "mm:ss".
        /// </summary>
        private static string FormatMinutesSeconds(int totalSeconds)
        {
            int minutes = totalSeconds / 60;
            int seconds = totalSeconds % 60;
            return minutes.ToString("00", CultureInfo.InvariantCulture)
                 + ":" + seconds.ToString("00", CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Lists every folder where the dataset could be, nearest to the executable first. The
        /// search walks up because the build output ends up a couple of levels below the project,
        /// and the depth is capped so a missing dataset cannot send the program up to the root of
        /// the drive.
        /// </summary>
        private static List<string> CandidateRoots()
        {
            List<string> candidates = new List<string>();

            string directory = Path.GetFullPath(AppDomain.CurrentDomain.BaseDirectory)
                                   .TrimEnd(Path.DirectorySeparatorChar);

            for (int up = 0; up < MaximumSearchDepth; up++)
            {
                candidates.Add(Path.Combine(directory, DatasetFolderName, DatasetSubfolderName));

                DirectoryInfo parent = Directory.GetParent(directory);
                if (parent == null) break;
                directory = parent.FullName;
            }

            return candidates;
        }

        /// <summary>
        /// Returns the first candidate folder that actually holds the dataset, or null.
        /// </summary>
        private static string FindDatasetRoot()
        {
            foreach (string candidate in CandidateRoots())
                if (Directory.Exists(Path.Combine(candidate, "train")))
                    return candidate;

            return null;
        }
    }
}