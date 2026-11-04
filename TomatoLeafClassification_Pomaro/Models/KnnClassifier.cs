using Accord.MachineLearning;

namespace TomatoLeafClassification.Models
{
    public class KnnClassifier : ClassifierBase
    {
        private int _k; // Number of neighbors

        private KNearestNeighbors _knn; // The underlying KNN model from Accord.NET

        public KnnClassifier(int k)
        {
            _k = k;
        }

        public override string Name
        {
            get { return "k-NN"; }
        }

        /// <summary>
        /// Extend the base toString to include the hyperparameter k.
        /// </summary>
        public override string ToString()
        {
            return base.ToString() + " k = " + _k;
        }

        /// <summary>
        /// Creates a new instance of the KnnClassifier with the same hyperparameter k.
        /// </summary>
        public override IClassifier CreateNew()
        {
            return new KnnClassifier(_k);
        }

        /// <summary>
        /// Train the model on labelled data. Uses the Accord.NET KNearestNeighbors class to learn
        /// from the inputs and outputs.
        /// </summary>
        public override void Train(double[][] inputs, int[] outputs)
        {
            _knn = new KNearestNeighbors(k: _k);
            _knn.Learn(inputs, outputs);
            Trained = true;
        }


        /// <summary>
        /// Classifies one sample by majority vote of the k nearest neighbors.
        /// </summary>
        public override int Predict(double[] input)
        {
            EnsureTrained();
            return _knn.Decide(input);
        }

        /// <summary>
        /// Classifies multiple samples using Accord's batch decision.
        /// </summary>
        public override int[] Predict(double[][] inputs)
        {
            EnsureTrained();
            return _knn.Decide(inputs);
        }
    }
}
