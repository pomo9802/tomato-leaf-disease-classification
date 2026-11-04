using System;
using System.Collections.Generic;
using TomatoLeafClassification.Evaluation;
using TomatoLeafClassification.ML;
using TomatoLeafClassification.Models;
using TomatoLeafClassification.Util;

namespace TomatoLeafClassification.Pipeline
{
    /// <summary>
    /// Stage 2: training. Loads the features CSV produced by the extraction stage, splits it into a
    /// stratified train/test pair, compares several classifiers by cross-validation on the training
    /// set only, then retrains the winner on the whole training set and reports its performance on
    /// the held-out test set. The test set is touched once, at the end, so the reported score stays honest.
    /// </summary>
    public static class TrainingPipeline
    {
        /// <summary>
        /// Runs the training pipeline: loads the dataset, splits it into training and test sets,
        /// performs cross-validation to select the best model, trains the selected model on the
        /// entire training set and evaluates it on the test set.
        /// </summary>
        public static void Run()
        {
            FeatureDataset data = LoadDataset(Constants.DefaultFeaturesCsv);
            //Stratified split
            double[][] trainX;
            int[] trainY;
            double[][] testX;
            int[] testY;
            SplitDataset(data, out trainX, out trainY, out testX, out testY);

            IClassifier[] models = BuildModels();
            INormalizer normalizerPrototype = new ZScoreNormalizer();

            //Model selection on training set
            CrossValidationResults[] crossValidationResults = CrossValidateAll(models, normalizerPrototype, trainX, trainY, data.NumberOfClasses);

            //Fit on the whole training set
            INormalizer normalizer = normalizerPrototype.CreateNew();
            normalizer.Fit(trainX);
            List<ModelResult> results = TrainAndEvaluateAll(models, normalizer.Transform(trainX), trainY, normalizer.Transform(testX), testY, data);

            //Print results
            ResultReporter.PrintComparison(results);

            double selectedCrossValidationF1;
            ModelResult best = SelectBestByCrossValidation(models, crossValidationResults, results, out selectedCrossValidationF1);
            if (best == null)
            {
                Console.WriteLine("\nNo model could be selected: the cross-validation " + "produced no usable result.");
                return;
            }

            ResultReporter.PrintBestModel(best, selectedCrossValidationF1);
        }

        /// <summary>
        /// Loads the feature CSV and prints its shape.
        /// </summary>
        private static FeatureDataset LoadDataset(string csvPath)
        {
            Console.WriteLine("Loading features from " + csvPath + " ...");
            FeatureDataset data = FeatureDataset.Load(csvPath);
            Console.WriteLine("  " + data);
            return data;
        }

        /// <summary>
        /// Split the dataset keeping the class proportions.
        /// </summary>
        /// <param name="data">The full dataset.</param>
        /// <param name="trainX">Output: training features.</param>
        /// <param name="trainY">Output: training labels.</param>
        /// <param name="testX">Output: test features.</param>
        /// <param name="testY">Output: test labels.</param>
        private static void SplitDataset(FeatureDataset data,
                                         out double[][] trainX, out int[] trainY,
                                         out double[][] testX, out int[] testY)
        {
            StratifiedSplitter splitter = new StratifiedSplitter(Constants.RandomSeed);
            splitter.Split(data.Inputs, data.Outputs, Constants.TrainRatio,
                           out trainX, out trainY, out testX, out testY);
            Console.WriteLine("  train: " + trainX.Length + "  test: " + testX.Length
                            + "  (seed " + Constants.RandomSeed + ")");
        }

        /// <summary>
        /// Builds the models to compare
        /// </summary>
        private static IClassifier[] BuildModels()
        {
            IClassifier[] models = new IClassifier[4];
            models[0] = new DecisionTreeClassifier();
            models[1] = new KnnClassifier(Constants.KNeighbours);
            models[2] = new SvmClassifier();
            models[3] = new MultinomialLogisticClassifier();

            Console.WriteLine("\nModels to compare:");
            for (int m = 0; m < models.Length; m++)
                Console.WriteLine("  " + models[m]);
            return models;
        }

        /// <summary>
        /// Cross validate every model on a stratified subsample of the training set, to speed up
        /// the process and prints the outcome.
        /// </summary>
        private static CrossValidationResults[] CrossValidateAll(IClassifier[] models,
                                             INormalizer normalizerPrototype,
                                             double[][] trainX, int[] trainY, int numberOfClasses)
        {
            double[][] cvX;
            int[] cvY;
            BuildSubsample(trainX, trainY, out cvX, out cvY);

            CrossValidator crossValidator = new CrossValidator(
                Constants.CrossValidationFolds, Constants.RandomSeed);

            Console.WriteLine("\n=== " + Constants.CrossValidationFolds
                            + "-fold cross-validation on " + cvX.Length + " samples (model selection) ===");

            CrossValidationResults[] crossValidationResults = new CrossValidationResults[models.Length];

            for (int m = 0; m < models.Length; m++)
            {
                Console.Write("  " + models[m].Name.PadRight(32));
                try
                {
                    CrossValidationResults result = crossValidator.Evaluate(
                        models[m], normalizerPrototype, cvX, cvY, numberOfClasses);
                    crossValidationResults[m] = result;
                    ResultReporter.PrintCrossValidation(result);
                }
                catch (OutOfMemoryException exception)
                {
                    Console.WriteLine("  SKIPPED: not enough memory (" + exception.Message + ")");
                }
                catch (ArgumentException exception)
                {
                    Console.WriteLine("  SKIPPED: invalid input (" + exception.Message + ")");
                }
                catch (InvalidOperationException exception)
                {
                    Console.WriteLine("  SKIPPED: the learner failed (" + exception.Message + ")");
                }
            }

            return crossValidationResults;
        }

        /// <summary>
        /// Takes a stratified subsample of the training set when it is larger than the cap, to
        /// speed up cross-validation.
        /// </summary>
        private static void BuildSubsample(double[][] trainX, int[] trainY,
                                           out double[][] sampleX, out int[] sampleY)
        {
            if (trainX.Length <= Constants.CrossValidationSubsampleCap)
            {
                sampleX = trainX;
                sampleY = trainY;
                return;
            }

            double ratio = (double)Constants.CrossValidationSubsampleCap / trainX.Length;
            double[][] discardedX;
            int[] discardedY;
            StratifiedSplitter subsampler = new StratifiedSplitter(Constants.RandomSeed);
            subsampler.Split(trainX, trainY, ratio, out sampleX, out sampleY,
                             out discardedX, out discardedY);
        }

        /// <summary>
        /// Train every model on the entire training set and evaluate it on the test set, returning
        /// the results.
        /// </summary>
        private static List<ModelResult> TrainAndEvaluateAll(IClassifier[] models,
                                                             double[][] trainX, int[] trainY,
                                                             double[][] testX, int[] testY,
                                                             FeatureDataset data)
        {
            Console.WriteLine("\n=== Final training and evaluation on the test set ===");
            List<ModelResult> results = new List<ModelResult>();

            for (int m = 0; m < models.Length; m++)
            {
                Console.WriteLine("\nTraining " + models[m].Name + " on the full training set ...");
                try
                {
                    ModelResult result = TrainAndEvaluateOne(
                        models[m], trainX, trainY, testX, testY, data);
                    results.Add(result);
                    Console.WriteLine("  " + result);
                }
                catch (OutOfMemoryException exception)
                {
                    Console.WriteLine("  FAILED: not enough memory (" + exception.Message + ")");
                }
                catch (ArgumentException exception)
                {
                    Console.WriteLine("  FAILED: invalid input (" + exception.Message + ")");
                }
                catch (InvalidOperationException exception)
                {
                    Console.WriteLine("  FAILED: the learner did not converge (" + exception.Message + ")");
                }
            }
            return results;
        }

        /// <summary>
        /// Trains one model and builds its two confusion matrices (train and test).
        /// </summary>
        private static ModelResult TrainAndEvaluateOne(IClassifier model, double[][] trainX, int[] trainY, double[][] testX, int[] testY, FeatureDataset data)
        {
            DateTime startTime = DateTime.Now;
            model.Train(trainX, trainY);
            double seconds = (DateTime.Now - startTime).TotalSeconds;

            ConfusionMatrix trainMatrix = new ConfusionMatrix(trainY, model.Predict(trainX), data.NumberOfClasses, data.ClassNames);
            ConfusionMatrix testMatrix = new ConfusionMatrix(testY, model.Predict(testX), data.NumberOfClasses, data.ClassNames);

            return new ModelResult(model, trainMatrix, testMatrix, seconds);
        }

        /// <summary>
        /// Picks the model with the best cross-validation mean macro F1 score.
        /// </summary>
        private static ModelResult SelectBestByCrossValidation(IClassifier[] models, CrossValidationResults[] crossValidationResults, List<ModelResult> results, out double selectedMacroF1)
        {
            ModelResult best = null;
            selectedMacroF1 = 0.0;

            for (int m = 0; m < models.Length; m++)
            {
                // A model with no cross-validation score cannot be chosen: there is no training-set
                // evidence in its favour.
                if (crossValidationResults[m] == null) continue;

                // A model whose final training failed has no result to report.
                ModelResult candidate = FindResultOf(results, models[m]);
                if (candidate == null) continue;

                if (best == null || crossValidationResults[m].MeanMacroF1 > selectedMacroF1)
                {
                    best = candidate;
                    selectedMacroF1 = crossValidationResults[m].MeanMacroF1;
                }
            }

            return best;
        }

        /// <summary>
        /// Find the result produced by a given model prototype.
        /// </summary>
        private static ModelResult FindResultOf(List<ModelResult> results, IClassifier model)
        {
            for (int i = 0; i < results.Count; i++)
            {
                if (results[i].Model == model) return results[i];
            }
            return null;
        }
    }
}