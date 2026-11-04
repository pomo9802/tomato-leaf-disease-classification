using System;
using System.Globalization;

namespace TomatoLeafClassification.Evaluation
{
    /// <summary>
    /// Multi-class confusion matrix and the performance derived from it. In the matrix the rows are
    /// the true classes and the columns the predicted ones.
    /// </summary>
    public class ConfusionMatrix
    {
        private int[,] _counts;
        private string[] _classNames;
        private int _numberOfClasses;
        private int _total;
        private const string ClassNamePrefix = "Tomato___";
        private const int NameWidth = 38;

        /// <summary>
        /// Overlad to use when the class names are not needed.
        /// </summary>
        public ConfusionMatrix(int[] yTrue, int[] yPred, int numberOfClasses) : this(yTrue, yPred, numberOfClasses, DefaultNames(numberOfClasses))
        {
        }

        /// <summary>
        /// Build the matrix by counting, for every sample, its true and predicted pair.
        /// </summary>
        /// <exception cref="ArgumentException">if tthe lengths of the two arrays are different.</exception>
        public ConfusionMatrix(int[] yTrue, int[] yPred, int numberOfClasses, string[] classNames)
        {
            if (yTrue.Length != yPred.Length)
                throw new ArgumentException("yTrue and yPred must have the same length.");

            _numberOfClasses = numberOfClasses;
            _classNames = classNames;
            _counts = new int[numberOfClasses, numberOfClasses];

            for (int i = 0; i < yTrue.Length; i++)
                _counts[yTrue[i], yPred[i]] = _counts[yTrue[i], yPred[i]] + 1;
            _total = yTrue.Length;
        }


        public int NumberOfClasses
        {
            get { return _numberOfClasses; }
        }

        public int Total
        {
            get { return _total; }
        }

        /// <summary>
        /// Overall fraction of correctly classified samples.
        /// </summary>
        public double Accuracy
        {
            get
            {
                int correct = 0;
                for (int i = 0; i < _numberOfClasses; i++)
                    correct = correct + _counts[i, i];
                if (_total == 0) return 0.0;
                return (double)correct / _total;
            }
        }

        /// <summary>
        /// Precision = TP / (TP + FP) for the given class.
        /// </summary>
        public double Precision(int c)
        {
            int tp = _counts[c, c];
            int predictedAsC = 0;
            for (int i = 0; i < _numberOfClasses; i++)
                predictedAsC = predictedAsC + _counts[i, c];
            if (predictedAsC == 0) return 0.0;
            return (double)tp / predictedAsC;
        }

        /// <summary>
        /// Recall = TP / (TP + FN) for the given class.
        /// </summary>
        public double Recall(int c)
        {
            int tp = _counts[c, c];
            int actualC = 0;
            for (int j = 0; j < _numberOfClasses; j++)
                actualC = actualC + _counts[c, j];
            if (actualC == 0) return 0.0;
            return (double)tp / actualC;
        }

        /// <summary>
        /// F1 = harmonic mean of precision and recall for the given class.
        /// </summary>
        public double F1(int c)
        {
            double p = Precision(c);
            double r = Recall(c);
            if (p + r == 0.0) return 0.0;
            return 2.0 * p * r / (p + r);
        }

        /// <summary>
        /// Unweighted mean of the per-class F1.
        /// </summary>
        public double MacroF1
        {
            get
            {
                double sum = 0.0;
                for (int c = 0; c < _numberOfClasses; c++)
                    sum = sum + F1(c);
                return sum / _numberOfClasses;
            }
        }

        /// <summary>
        /// Per-class table: precision, recall, F1 and support.
        /// </summary>
        public string PerClassReport()
        {
            string report =
                "class".PadRight(NameWidth) +
                "precision".PadLeft(10) +
                "recall".PadLeft(10) +
                "f1".PadLeft(10) +
                "support".PadLeft(8) + "\n";
            for (int c = 0; c < _numberOfClasses; c++)
            {
                int support = 0;
                for (int j = 0; j < _numberOfClasses; j++)
                    support = support + _counts[c, j];
                report = report +
                    _classNames[c].Replace(ClassNamePrefix, "").PadRight(NameWidth) +
                    Precision(c).ToString("F3", CultureInfo.InvariantCulture).PadLeft(10) +
                    Recall(c).ToString("F3", CultureInfo.InvariantCulture).PadLeft(10) +
                    F1(c).ToString("F3", CultureInfo.InvariantCulture).PadLeft(10) +
                    support.ToString().PadLeft(8) + "\n";
            }
            report = report +
                "MACRO F1".PadRight(NameWidth) +
                "".PadLeft(10) +
                "".PadLeft(10) +
                MacroF1.ToString("F3", CultureInfo.InvariantCulture).PadLeft(10) +
                _total.ToString().PadLeft(8) + "\n" +
                "\nOverall accuracy: " +
                Accuracy.ToString("F3", CultureInfo.InvariantCulture) + "\n";
            return report;
        }

        /// <summary>
        /// Raw counts grid (true rows x predicted columns).
        /// </summary>
        public string CountsGrid()
        {
            string grid = "true\\pred".PadRight(6);
            for (int j = 0; j < _numberOfClasses; j++)
                grid = grid + j.ToString().PadLeft(6);
            grid = grid + "\n";
            for (int i = 0; i < _numberOfClasses; i++)
            {
                grid = grid + i.ToString().PadRight(6);
                for (int j = 0; j < _numberOfClasses; j++)
                    grid = grid + _counts[i, j].ToString().PadLeft(6);
                grid = grid + "\n";
            }
            return grid;
        }

        /// <summary>
        /// Override of toString() to return a description of the matrix.
        /// </summary>
        public override string ToString()
        {
            return "ConfusionMatrix: " + _numberOfClasses + " classes, " + _total
                 + " samples, accuracy " + Accuracy.ToString("F3", CultureInfo.InvariantCulture)
                 + ", macroF1 " + MacroF1.ToString("F3", CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Builds generic class names ("class_0", "class_1", ...) when none are provided.
        /// </summary>
        private static string[] DefaultNames(int n)
        {
            string[] names = new string[n];
            for (int i = 0; i < n; i++)
                names[i] = "class_" + i;
            return names;
        }
    }
}