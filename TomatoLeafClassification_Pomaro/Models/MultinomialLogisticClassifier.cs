using Accord.Math.Optimization;
using Accord.Statistics.Models.Regression;
using Accord.Statistics.Models.Regression.Fitting;

namespace TomatoLeafClassification.Models
{
    public class MultinomialLogisticClassifier : ClassifierBase
    {
        private MultinomialLogisticRegression _model;

        public override string Name
        {
            get { return "Multinomial Logistic Regression"; }
        }

        public override IClassifier CreateNew()
        {
            return new MultinomialLogisticClassifier();
        }

        /// <summary>
        /// Fits the softmax model with the BFGS optimizer.
        /// </summary>
        public override void Train(double[][] inputs, int[] outputs)
        {
            MultinomialLogisticLearning<BroydenFletcherGoldfarbShanno> teacher =
                new MultinomialLogisticLearning<BroydenFletcherGoldfarbShanno>();
            _model = teacher.Learn(inputs, outputs);
            Trained = true;
        }

        /// <summary>
        /// Predict the most probable class for one sample.
        /// </summary>
        public override int Predict(double[] input)
        {
            EnsureTrained();
            return _model.Decide(input);
        }

        /// <summary>
        /// Predict the most probable class for multiple samples.
        /// </summary>
        public override int[] Predict(double[][] inputs)
        {
            EnsureTrained();
            return _model.Decide(inputs);
        }
    }
}
