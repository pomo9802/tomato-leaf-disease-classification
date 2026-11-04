using System;

namespace TomatoLeafClassification.ML
{
    /// <summary>
    /// Shared implementation of the INormalizer interface for column-wise normalization. Concrete
    /// classes only compute the two parameters (offset and scale) from the trining data.
    /// </summary>
    public class NormalizerBase : INormalizer
    {
        protected double[] Offset; //subtracted from each column
        protected double[] Scale; //divided from each column

        /// <summary>
        /// Protected constructor: the class is meant to be used only as a base for concrete
        /// normalizers, not to be instantiated directly.
        /// </summary>
        protected NormalizerBase()
        { }

        /// <summary>
        /// Learns the normalization parameters from the training data.
        /// </summary>
        public void Fit(double[][] trainingInputs)
        {
            if (trainingInputs == null || trainingInputs.Length == 0)
                throw new ArgumentException("Training inputs are empty.");

            int cols = trainingInputs[0].Length;
            Offset = new double[cols];
            Scale = new double[cols];
            ComputeParameters(trainingInputs, cols);

            // Guard against zero-variance / constant columns.
            for (int c = 0; c < cols; c++)
            {
                if (Scale[c] == 0.0) Scale[c] = 1.0;
            }
        }

        /// <summary>
        /// Fills the Offset and Scale arrays with the normalization parameters computed from the
        /// training data.
        /// </summary>
        /// <exception cref="NotSupportedException">
        /// if the normalizer does not implement this method
        /// </exception>
        protected virtual void ComputeParameters(double[][] data, int cols)
        {
            throw new NotSupportedException(GetType().Name + " does not implement ComputeParameters.");
        }

        /// <summary>
        /// Returns an unfit normalizer og the same kind.
        /// </summary>
        /// <exception cref="NotSupportedException">
        /// if the normalizer does not implement this method
        /// </exception>
        public virtual INormalizer CreateNew()
        {
            throw new NotSupportedException(GetType().Name + " does not implement CreateNew.");
        }

        /// <summary>
        /// Scales one feature vector as out = (x - offset) / scale.
        /// </summary>
        /// <exception cref="InvalidOperationException">If the normalizer was not fit first.</exception>
        public double[] Transform(double[] row)
        {
            if (Offset == null)
                throw new InvalidOperationException("Normalizer must be fit before use.");

            double[] result = new double[row.Length];
            for (int c = 0; c < row.Length; c++)
                result[c] = (row[c] - Offset[c]) / Scale[c];
            return result;
        }

        /// <summary>
        /// scales every rows of a matricx.
        /// </summary>
        /// <exception cref="InvalidOperationException">If the normalizer was not fit first.</exception>
        public double[][] Transform(double[][] rows)
        {
            double[][] result = new double[rows.Length][];
            for (int i = 0; i < rows.Length; i++)
                result[i] = Transform(rows[i]);
            return result;
        }
    }
}
