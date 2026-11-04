using Accord.MachineLearning.DecisionTrees;
using Accord.MachineLearning.DecisionTrees.Learning;

namespace TomatoLeafClassification.Models
{
    public class DecisionTreeClassifier : ClassifierBase
    {
        private DecisionTree _tree;

        public override string Name
        {
            get { return "Decision Tree (C4.5)"; }
        }

        public override IClassifier CreateNew()
        {
            return new DecisionTreeClassifier();
        }

        /// <summary>
        /// Grows a C4.5 decision tree from the training data using Accord.NET's C45Learning class.
        /// </summary>
        public override void Train(double[][] inputs, int[] outputs)
        {
            C45Learning teacher = new C45Learning();
            _tree = teacher.Learn(inputs, outputs);
            Trained = true;
        }

        /// <summary>
        /// Classifies a single sample by reaching a leaf in the decision tree and returning the
        /// predicted class.
        /// </summary>
        public override int Predict(double[] input)
        {
            EnsureTrained();
            return _tree.Decide(input);
        }

        /// <summary>
        /// Classifies multiple samples using Accord's batch decision.
        /// </summary>
        public override int[] Predict(double[][] inputs)
        {
            EnsureTrained();
            return _tree.Decide(inputs);
        }
    }
}
