using System;
using System.Collections.Generic;

namespace TomatoLeafClassification.ML
{
    /// <summary>
    /// Splits a dataset into training and test subsets while preserving the class proportions
    /// (stratified split).
    /// </summary>
    public class StratifiedSplitter
    {
        private Random _rng; // Random number generator for reproducibility

        /// <summary>
        /// Initializes a new instance of the StratifiedSplitter class with a specific random seed.
        /// </summary>
        /// <param name="seed"></param>
        public StratifiedSplitter(int seed)
        {
            _rng = new Random(seed);
        }

        /// <summary>
        /// Splits the dataset into training and test subsets while preserving class proportions.
        /// </summary>
        /// <param name="inputs"></param>
        /// <param name="outputs"></param>
        /// <param name="trainRatio"></param>
        /// <param name="trainX"></param>
        /// <param name="trainY"></param>
        /// <param name="testX"></param>
        /// <param name="testY"></param>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public void Split(double[][] inputs, int[] outputs, double trainRatio, out double[][] trainX, out int[] trainY,
                          out double[][] testX, out int[] testY)
        {
            if (trainRatio <= 0.0 || trainRatio >= 1.0)
                throw new ArgumentOutOfRangeException("trainRatio");

            // Group sample indices by class.
            Dictionary<int, List<int>> byClass = new Dictionary<int, List<int>>();
            for (int i = 0; i < outputs.Length; i++)
            {
                List<int> list;
                if (!byClass.TryGetValue(outputs[i], out list))
                {
                    list = new List<int>();
                    byClass[outputs[i]] = list;
                }
                list.Add(i);
            }

            List<int> trainIdx = new List<int>();
            List<int> testIdx = new List<int>();

            foreach (KeyValuePair<int, List<int>> kv in byClass)
            {
                List<int> indices = kv.Value;
                Shuffle(indices);
                // Round to nearest integer with elementary arithmetic (no Math.Round).
                int nTrain = (int)(indices.Count * trainRatio + 0.5);
                for (int k = 0; k < indices.Count; k++)
                {
                    if (k < nTrain) trainIdx.Add(indices[k]);
                    else testIdx.Add(indices[k]);
                }
            }

            Shuffle(trainIdx);
            Shuffle(testIdx);

            trainX = new double[trainIdx.Count][];
            trainY = new int[trainIdx.Count];
            for (int i = 0; i < trainIdx.Count; i++)
            {
                trainX[i] = inputs[trainIdx[i]];
                trainY[i] = outputs[trainIdx[i]];
            }

            testX = new double[testIdx.Count][];
            testY = new int[testIdx.Count];
            for (int i = 0; i < testIdx.Count; i++)
            {
                testX[i] = inputs[testIdx[i]];
                testY[i] = outputs[testIdx[i]];
            }
        }

        /// <summary>
        /// In-place Fisher-Yates shuffle.
        /// </summary>
        /// <param name="list">The list to shuffle.</param>
        private void Shuffle(List<int> list)
        {
            for (int i = list.Count - 1; i > 0; i--)
            {
                int j = _rng.Next(i + 1);
                int tmp = list[i];
                list[i] = list[j];
                list[j] = tmp;
            }
        }
    }
}
