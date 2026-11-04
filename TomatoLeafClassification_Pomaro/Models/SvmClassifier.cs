using Accord.MachineLearning.VectorMachines;
using Accord.MachineLearning.VectorMachines.Learning;
using Accord.Statistics.Kernels;

namespace TomatoLeafClassification.Models
{
    public class SvmClassifier : ClassifierBase
    {
        private MulticlassSupportVectorMachine<Linear> _svm;

        public override string Name
        {
            get { return "Support Vector Machine (Linear)"; }
        }

        public override IClassifier CreateNew()
        {
            return new SvmClassifier();
        }

        /// <summary>
        /// Fits the SVM model to the training data.
        /// </summary>
        public override void Train(double[][] inputs, int[] outputs)
        {
            MulticlassSupportVectorLearning<Linear> teacher =
                new MulticlassSupportVectorLearning<Linear>();

            // Accord requires a delegate here to build the inner learner for each binary
            // sub-problem: this lambda is part of using the Accord library.
            teacher.Learner = (param) => new SequentialMinimalOptimization<Linear>()
            {
                UseComplexityHeuristic = true
            };

            _svm = teacher.Learn(inputs, outputs);
            Trained = true;
        }

        /// <summary>
        /// Predict the most probable class for one sample.
        /// </summary>
        public override int Predict(double[] input)
        {
            EnsureTrained();
            return _svm.Decide(input);
        }

        /// <summary>
        /// Predict the most probable class for multiple samples.
        /// </summary>
        public override int[] Predict(double[][] inputs)
        {
            EnsureTrained();
            return _svm.Decide(inputs);
        }
    }
}
