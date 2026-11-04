using System;
using System.Collections.Generic;
using TomatoLeafClassification.Evaluation;
using TomatoLeafClassification.Models;

namespace TomatoLeafClassification.ML
{
    /// <summary>
    /// Stratified k-fold cross-validation on the training set, used for model selection.
    /// </summary>
    public class CrossValidator
    {
        private int _folds;

        private Random _rng;

        public CrossValidator(int folds, int seed)
        {
            if (folds < 2) throw new ArgumentOutOfRangeException("folds");
            _folds = folds;
            _rng = new Random(seed);
        }

        /// <summary>
        /// Runs stratified k-fold cross validation.
        /// </summary>
        /// <param name="modelPrototype">The prototype for creating classifiers.</param>
        /// <param name="normalizerPrototype">The prototype for creating normalizers.</param>
        /// <param name="inputs">The input features.</param>
        /// <param name="outputs">The true labels.</param>
        /// <param name="numberOfClasses">The number of classes.</param>
        /// <returns>The cross-validation results.</returns>
        public CrossValidationResults Evaluate(IClassifier modelPrototype, INormalizer normalizerPrototype, double[][] inputs, int[] outputs, int numberOfClasses)
        {
            int[] foldOf = AssignFolds(outputs, numberOfClasses);

            double[] accuracies = new double[_folds];
            double[] macroF1s = new double[_folds];

            for (int f = 0; f < _folds; f++)
            {
                // Build train / test index lists for this fold.
                List<int> trainIdx = new List<int>();
                List<int> testIdx = new List<int>();
                for (int i = 0; i < inputs.Length; i++)
                {
                    if (foldOf[i] == f) testIdx.Add(i);
                    else trainIdx.Add(i);
                }

                double[][] trainX = new double[trainIdx.Count][];
                int[] trainY = new int[trainIdx.Count];
                for (int i = 0; i < trainIdx.Count; i++)
                {
                    trainX[i] = inputs[trainIdx[i]];
                    trainY[i] = outputs[trainIdx[i]];
                }

                double[][] testX = new double[testIdx.Count][];
                int[] testY = new int[testIdx.Count];
                for (int i = 0; i < testIdx.Count; i++)
                {
                    testX[i] = inputs[testIdx[i]];
                    testY[i] = outputs[testIdx[i]];
                }

                // Fit the normalizer inside the fold (train part only).
                INormalizer normalizer = normalizerPrototype.CreateNew();
                normalizer.Fit(trainX);
                double[][] trainXn = normalizer.Transform(trainX);
                double[][] testXn = normalizer.Transform(testX);

                IClassifier model = modelPrototype.CreateNew();
                model.Train(trainXn, trainY);
                int[] pred = model.Predict(testXn);

                ConfusionMatrix cm = new ConfusionMatrix(testY, pred, numberOfClasses);
                accuracies[f] = cm.Accuracy;
                macroF1s[f] = cm.MacroF1;

                Console.Write(".");   // progress
            }

            return new CrossValidationResults(_folds,
                Mean(accuracies), Std(accuracies),
                Mean(macroF1s), Std(macroF1s));
        }

        /// <summary>
        /// Assign each sample to a fold so every class is spread as evenly as possible across the foldss
        /// </summary>
        private int[] AssignFolds(int[] outputs, int numberOfClasses)
        {
            int[] foldOf = new int[outputs.Length];

            for (int c = 0; c < numberOfClasses; c++)
            {
                List<int> indices = new List<int>();
                for (int i = 0; i < outputs.Length; i++)
                {
                    if (outputs[i] == c) indices.Add(i);
                }
                Shuffle(indices);

                for (int k = 0; k < indices.Count; k++)
                    foldOf[indices[k]] = k % _folds;
            }
            return foldOf;
        }

        /// <summary>
        /// In place shuffle of an index list using Fisher-Yates algorithm.
        /// </summary>
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

        /// <summary>
        /// Mean of an array (0 if empty).
        /// </summary>
        private static double Mean(double[] values)
        {
            if (values.Length == 0) return 0.0;
            double sum = 0.0;
            for (int i = 0; i < values.Length; i++)
                sum = sum + values[i];
            return sum / values.Length;
        }

        /// <summary>
        /// Population standard deviation of an array (0 if empty).
        /// </summary>
        private static double Std(double[] values)
        {
            if (values.Length == 0) return 0.0;
            double mean = Mean(values);
            double acc = 0.0;
            for (int i = 0; i < values.Length; i++)
                acc = acc + (values[i] - mean) * (values[i] - mean);
            return Math.Sqrt(acc / values.Length);
        }
    }
}
