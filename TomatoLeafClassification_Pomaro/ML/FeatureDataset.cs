using System.Collections.Generic;
using System.Globalization;
using System.IO;
using TomatoLeafClassification.Exceptions;

namespace TomatoLeafClassification.ML
{
    /// <summary>
    /// Tabular dataset read back from the features CSV. Holds the input matrix, the
    /// </summary>
    public class FeatureDataset
    {
        private const int MetadataColumns = 3; // Number of metadata columns at the end of every row (id, name, path).

        private double[][] _inputs;

        private int[] _outputs;

        private string[] _featureNames;

        private string[] _classNames;

        /// <summary>
        /// private constructor, instances are created through Load.
        /// </summary>
        /// <param name="inputs">Feature matrix (rows = samples, columns = features).</param>
        /// <param name="outputs">Class id for each sample.</param>
        /// <param name="featureNames">Name of each feature column.</param>
        /// <param name="classNames">Display name for each class id.</param>
        private FeatureDataset(double[][] inputs, int[] outputs, string[] featureNames, string[] classNames)
        {
            _inputs = inputs;
            _outputs = outputs;
            _featureNames = featureNames;
            _classNames = classNames;
        }

        public double[][] Inputs
        { get { return _inputs; } }
        public int[] Outputs
        { get { return _outputs; } }
        public string[] FeatureNames
        { get { return _featureNames; } }
        public string[] ClassNames
        { get { return _classNames; } }

        public int NumberOfSamples
        { get { return _inputs.Length; } }

        public int NumberOfFeatures
        { get { return _featureNames.Length; } }

        public int NumberOfClasses
        { get { return _classNames.Length; } }

        /// <summary>
        /// Loads a CSV whose layout is: [feature columns...], class_id, class_name, file_path. The
        /// last three columns are metadata.
        /// </summary>
        /// <param name="csvPath">Path to the CSV file.</param>
        /// <returns>The dataset built from the file.</returns>
        /// <exception cref="DatasetException">
        /// If the file is missing, empty, or contains invalid values.
        /// </exception>
        public static FeatureDataset Load(string csvPath)
        {
            if (!File.Exists(csvPath))
            {
                throw new DatasetException("Feature CSV not found: " + csvPath);
            }

            string[] lines = File.ReadAllLines(csvPath);
            if (lines.Length < 2)
            {
                throw new DatasetException("The CSV: " + csvPath + " has no rows.");
            }

            string[] featureNames = ParseHeader(lines[0], csvPath);
            int featureCount = featureNames.Length;

            List<double[]> inputs = new List<double[]>();
            List<int> outputs = new List<int>();
            Dictionary<int, string> idToName = new Dictionary<int, string>();
            int maxClassId = -1;

            for (int r = 1; r < lines.Length; r++)
            {
                if (lines[r].Length == 0) { continue; }
                List<string> columns = SplitCsvLine(lines[r]);

                inputs.Add(ParseFeatures(columns, featureCount, r + 1, csvPath));

                int classId = ParseClassId(columns, featureCount, r + 1, csvPath);
                outputs.Add(classId);

                if (!idToName.ContainsKey(classId))
                    idToName[classId] = columns[featureCount + 1];
                if (classId > maxClassId) maxClassId = classId;
            }

            return new FeatureDataset(ToMatrix(inputs), ToArray(outputs), featureNames, BuildClassNames(idToName, maxClassId));
        }

        /// <summary>
        /// Parses the header line of the CSV to extract feature names. The last three columns are
        /// expected to be metadata (class_id, class_name, file_path).
        /// </summary>
        /// <param name="headerLine"></param>
        /// <param name="csvPath"></param>
        /// <returns>An array of feature names.</returns>
        /// <exception cref="DatasetException"></exception>
        private static string[] ParseHeader(string headerLine, string csvPath)
        {
            List<string> header = SplitCsvLine(headerLine);
            int featureCount = header.Count - MetadataColumns;
            if (featureCount <= 0)
                throw new DatasetException("Unexpected header in '" + csvPath + "': no feature column found.");

            string[] featureNames = new string[featureCount];
            for (int c = 0; c < featureCount; c++)
                featureNames[c] = header[c];
            return featureNames;
        }

        /// <summary>
        /// Parses the feature values from a row of the CSV. The last three columns are expected to
        /// be metadata (class_id, class_name, file_path).
        /// </summary>
        /// <param name="columns"></param>
        /// <param name="featureCount"></param>
        /// <param name="lineNumber"></param>
        /// <param name="csvPath"></param>
        /// <returns>An array of feature values.</returns>
        /// <exception cref="DatasetException"></exception>
        private static double[] ParseFeatures(List<string> columns, int featureCount, int lineNumber, string csvPath)
        {
            if (columns.Count < featureCount + MetadataColumns)
                throw new DatasetException("Line " + lineNumber + " of '" + csvPath
                                         + "' has " + columns.Count + " columns instead of "
                                         + (featureCount + MetadataColumns) + ".");

            double[] row = new double[featureCount];
            for (int c = 0; c < featureCount; c++)
            {
                double value;
                if (!double.TryParse(columns[c], NumberStyles.Float, CultureInfo.InvariantCulture, out value))
                    throw new DatasetException("Non-numeric value '" + columns[c] + "' in column "
                                             + c + " at line " + lineNumber + " of '" + csvPath + "'.");
                row[c] = value;
            }
            return row;
        }

        /// <summary>
        /// Parses the class id from a row of the CSV. The last three columns are expected to be
        /// metadata (class_id, class_name, file_path).
        /// </summary>
        /// <param name="columns"></param>
        /// <param name="featureCount"></param>
        /// <param name="lineNumber"></param>
        /// <param name="csvPath"></param>
        /// <returns>The parsed class id.</returns>
        /// <exception cref="DatasetException"></exception>
        private static int ParseClassId(List<string> columns, int featureCount, int lineNumber, string csvPath)
        {
            int classId;
            if (!int.TryParse(columns[featureCount], NumberStyles.Integer,
                              CultureInfo.InvariantCulture, out classId) || classId < 0)
            {
                throw new DatasetException("Invalid class id '" + columns[featureCount]
                                         + "' at line " + lineNumber + " of '" + csvPath + "'.");
            }
            return classId;
        }

        /// <summary>
        /// Builds the array of class names from the mapping of class ids to names. If a class id is
        /// missing from the mapping, a default name "class_{id}" is used.
        /// </summary>
        /// <param name="idToName"></param>
        /// <param name="maxClassId"></param>
        /// <returns>An array of class names.</returns>
        private static string[] BuildClassNames(Dictionary<int, string> idToName, int maxClassId)
        {
            string[] classNames = new string[maxClassId + 1];
            for (int i = 0; i <= maxClassId; i++)
            {
                string name;
                if (idToName.TryGetValue(i, out name)) classNames[i] = name;
                else classNames[i] = "class_" + i;
            }
            return classNames;
        }

        /// <summary>
        /// Converts a list of double arrays (rows) into a jagged array (matrix). Each row in the
        /// list becomes a row in the matrix.
        /// </summary>
        /// <param name="rows"></param>
        /// <returns>A jagged array of feature values.</returns>
        private static double[][] ToMatrix(List<double[]> rows)
        {
            double[][] matrix = new double[rows.Count][];
            for (int i = 0; i < rows.Count; i++)
                matrix[i] = rows[i];
            return matrix;
        }

        /// <summary>
        /// Converts a list of integers into an array of integers. Each element in the list becomes
        /// an element in the array.
        /// </summary>
        /// <param name="values"></param>
        /// <returns>An array of integers.</returns>
        private static int[] ToArray(List<int> values)
        {
            int[] array = new int[values.Count];
            for (int i = 0; i < values.Count; i++)
                array[i] = values[i];
            return array;
        }

        /// <summary>
        /// Splits a CSV line into its constituent fields, handling quoted fields and escaped
        /// quotes. This method correctly handles commas inside quoted fields and double quotes as
        /// escape characters.
        /// </summary>
        /// <param name="line"></param>
        /// <returns>A list of strings representing the fields.</returns>
        private static List<string> SplitCsvLine(string line)
        {
            List<string> fields = new List<string>();
            string field = "";
            bool inQuotes = false;

            for (int i = 0; i < line.Length; i++)
            {
                char character = line[i];
                if (inQuotes)
                {
                    if (character == '"')
                    {
                        if (i + 1 < line.Length && line[i + 1] == '"')
                        {
                            field = field + '"';
                            i++;
                        }
                        else
                        {
                            inQuotes = false;
                        }
                    }
                    else
                    {
                        field = field + character;
                    }
                }
                else
                {
                    if (character == '"')
                        inQuotes = true;
                    else if (character == ',')
                    {
                        fields.Add(field);
                        field = "";
                    }
                    else
                        field = field + character;
                }
            }
            fields.Add(field);
            return fields;
        }

        /// <summary>
        /// Returns a string representation of the FeatureDataset, including the number of samples,
        /// features, and classes.
        /// </summary>
        /// <returns>A string representing the dataset.</returns>
        public override string ToString()
        {
            return "FeatureDataset: " + _inputs.Length + " samples, "
                 + _featureNames.Length + " features, " + _classNames.Length + " classes";
        }
    }
}