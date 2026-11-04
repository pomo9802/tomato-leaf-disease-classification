using System;

namespace TomatoLeafClassification.ML
{
    public class ZScoreNormalizer : NormalizerBase
    {
        /// <summary>
        /// Return a new unfit standardizer
        /// </summary>
        public override INormalizer CreateNew()
        {
            return new ZScoreNormalizer();
        }

        /// <summary>
        /// Computes per column the mean and standard deviation of the training data. The mean is
        /// stored in Offset and the standard deviation in Scale.
        /// </summary>
        protected override void ComputeParameters(double[][] data, int cols)
        {
            int n = data.Length;
            double[] mean = new double[cols];

            for (int i = 0; i < n; i++)
            {
                double[] row = data[i];
                for (int c = 0; c < cols; c++)
                    mean[c] = mean[c] + row[c];
            }
            for (int c = 0; c < cols; c++)
                mean[c] = mean[c] / n;

            double[] variance = new double[cols];
            for (int i = 0; i < n; i++)
            {
                double[] row = data[i];
                for (int c = 0; c < cols; c++)
                {
                    double diff = row[c] - mean[c];
                    variance[c] = variance[c] + diff * diff;
                }
            }

            for (int c = 0; c < cols; c++)
            {
                Offset[c] = mean[c];
                Scale[c] = Math.Sqrt(variance[c] / n);
            }
        }
    }
}
