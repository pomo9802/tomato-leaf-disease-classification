using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using TomatoLeafClassification.Data;

namespace TomatoLeafClassification.Output
{
    /// <summary>
    /// Writes the extracted features to a single CSV file, one row per image: the feature columns
    /// first, then the integer label, the class name and the path of the image relative to the dataset root.
    /// </summary>
    public static class FeatureCsvWriter
    {
        /// <summary>
        /// Writes the samples' feature vectors and metadata to a CSV file. Paths are written relative
        /// to <paramref name="basePath"/>, so the file does not depend on where the dataset sits.
        /// </summary>
        public static void Write(string path, List<ImageSample> samples, List<string> featureNames, string basePath)
        {
            using (StreamWriter writer = new StreamWriter(path, false, Encoding.UTF8))
            {
                // Header row.
                string header = "";
                for (int i = 0; i < featureNames.Count; i++)
                {
                    header = header + featureNames[i] + ",";
                }
                header = header + "class_id,class_name,file_path";
                writer.WriteLine(header);

                // Data rows.
                for (int s = 0; s < samples.Count; s++)
                {
                    ImageSample sample = samples[s];
                    if (sample.Features == null) continue;

                    string line = "";
                    for (int c = 0; c < sample.Features.Length; c++)
                    {
                        line = line + sample.Features[c].ToString("R", CultureInfo.InvariantCulture) + ",";
                    }
                    line = line + sample.Label.ToString(CultureInfo.InvariantCulture) + ",";
                    line = line + EscapeCsv(sample.ClassName) + ",";
                    line = line + EscapeCsv(MakeRelative(basePath, sample.FilePath));
                    writer.WriteLine(line);
                }
            }
        }

        /// <summary>
        /// Returns the path of a file relative to the dataset root, so that the CSV is portable
        /// across machines. A file that lies outside that root is written unchanged.
        /// </summary>
        private static string MakeRelative(string basePath, string fullPath)
        {
            if (basePath == null || basePath.Length == 0) return fullPath;

            string root = basePath.TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar;

            if (fullPath.StartsWith(root, StringComparison.OrdinalIgnoreCase))
                return fullPath.Substring(root.Length);

            return fullPath;
        }

        /// <summary>
        /// Quotes a field if it contains a comma or a double quote.
        /// </summary>
        private static string EscapeCsv(string field)
        {
            if (field.IndexOf(',') >= 0 || field.IndexOf('"') >= 0)
                return "\"" + field.Replace("\"", "\"\"") + "\"";
            return field;
        }
    }
}