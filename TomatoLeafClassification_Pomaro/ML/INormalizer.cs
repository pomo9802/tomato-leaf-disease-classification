namespace TomatoLeafClassification.ML
{
    /// <summary>
    /// Feature scaler. It is fit on the trining data and then applied to both train and test data
    /// </summary>
    public interface INormalizer
    {
        /// <summary>
        /// Learn the training parameters from the training data only.
        /// </summary>
        void Fit(double[][] trainingInputs);

        /// <summary>
        /// Scale a single feature vector
        /// </summary>
        double[] Transform(double[] row);

        /// <summary>
        /// Scale a set of feature vectors
        /// </summary>
        double[][] Transform(double[][] rows);

        /// <summary>
        /// </summary>
        INormalizer CreateNew();
    }
}
