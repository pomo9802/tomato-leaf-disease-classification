using System;
using System.Globalization;
using TomatoLeafClassification.Models;

namespace TomatoLeafClassification.Evaluation
{
    /// <summary>
    /// Result of a trained and evaluated model. Contains the model, the confusion matrices for
    /// training and testing, and the elapsed time for training and evaluation.
    /// </summary>
    public class ModelResult
    {
        private IClassifier _model;

        private ConfusionMatrix _trainMatrix;

        private ConfusionMatrix _testMatrix;

        private double _elapsedSeconds;

        /// <summary>
        /// Creates the result of a trained and evaluated model.
        /// </summary>
        public ModelResult(IClassifier model, ConfusionMatrix trainMatrix, ConfusionMatrix testMatrix, double elapsedSeconds)
        {
            if (model == null) throw new ArgumentNullException("model");
            if (trainMatrix == null || testMatrix == null) throw new ArgumentNullException("trainMatrix or testMatrix");
            if (elapsedSeconds < 0) throw new ArgumentOutOfRangeException("elapsedSeconds");

            _model = model;
            _trainMatrix = trainMatrix;
            _testMatrix = testMatrix;
            _elapsedSeconds = elapsedSeconds;
        }

        public IClassifier Model
        { get { return _model; } }

        public ConfusionMatrix TrainMatrix
        { get { return _trainMatrix; } }

        public ConfusionMatrix TestMatrix
        { get { return _testMatrix; } }

        public double ElapsedSeconds
        { get { return _elapsedSeconds; } }

        /// <summary>
        /// Override of toString. Canonical text form of a model result (model name, accuracy and
        /// elapsed time).
        /// </summary>
        public override string ToString()
        {
            return string.Format(CultureInfo.InvariantCulture, "{0}: test accuracy {1:F3}, macroF1 {2:F3}", _model.Name, _testMatrix.Accuracy, _testMatrix.MacroF1);
        }
    }
}
