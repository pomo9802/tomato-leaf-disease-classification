namespace TomatoLeafClassification.Models
{
    /// <summary>
    /// Uniform interface for all classifiers. Hides the Accord specific type behind a common interface.
    /// </summary>
    public interface IClassifier
    {
        string Name { get; }

        /// <summary>
        /// Trains the model on labelled data.
        /// </summary>
        void Train(double[][] inputs, int[] outputs);

        /// <summary>
        /// Predict the class of a single sample.
        /// </summary>
        int Predict(double[] input);

        /// <summary>
        /// Predict the class of multiple samples.
        /// </summary>
        int[] Predict(double[][] inputs);

        /// <summary>
        /// Return an untrined instance of the same model with same hyperparameters.
        /// </summary>
        IClassifier CreateNew();
    }
}
