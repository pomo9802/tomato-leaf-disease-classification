using System.Globalization;

namespace TomatoLeafClassification.Evaluation
{
    /// <summary>
    /// Result of a cross-validation. Contains the number of folds, the mean and standard deviation
    /// of accuracy and macroF1 across folds.
    /// </summary>
    public class CrossValidationResults
    {
        private int _folds; // Number of folds used in cross-validation

        private double _meanAccuracy;

        private double _stdAccuracy;

        private double _meanMacroF1;

        private double _stdMacroF1;

        public CrossValidationResults(int folds, double meanAccuracy, double stdAccuracy, double meanMacroF1, double stdMacroF1)
        {
            _folds = folds;
            _meanAccuracy = meanAccuracy;
            _stdAccuracy = stdAccuracy;
            _meanMacroF1 = meanMacroF1;
            _stdMacroF1 = stdMacroF1;
        }

        public int Folds
        { get { return _folds; } }
        public double MeanAccuracy
        { get { return _meanAccuracy; } }
        public double StdAccuracy
        { get { return _stdAccuracy; } }
        public double MeanMacroF1
        { get { return _meanMacroF1; } }
        public double StdMacroF1
        { get { return _stdMacroF1; } }

        /// <summary>
        /// Override of toString. Canonical text form of cross-validation results (number of folds,
        /// mean accuracy and std, mean macroF1 and std).
        /// </summary>
        public override string ToString()
        {
            return string.Format(CultureInfo.InvariantCulture,
                                 "{0}-fold CV: accuracy {1:F3} +/- {2:F3}, macroF1 {3:F3} +/- {4:F3}",
                                 _folds, _meanAccuracy, _stdAccuracy, _meanMacroF1, _stdMacroF1);
        }
    }
}
