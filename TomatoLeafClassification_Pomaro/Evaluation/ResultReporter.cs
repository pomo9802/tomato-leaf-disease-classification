using System;
using System.Collections.Generic;
using System.Globalization;

namespace TomatoLeafClassification.Evaluation
{
    public static class ResultReporter
    {
        private const int NameColumnWidth = 34; //Width of the model name column in the comparison table

        private const int ValueColumnWidth = 10; //Width of the value columns in the comparison table

        /// <summary>
        /// Prints a comparison table of the results of different models, including their training
        /// accuracy, testing accuracy, testing macro F1 score, and elapsed time.
        /// </summary>
        public static void PrintComparison(List<ModelResult> results)
        {
            Console.WriteLine("\n=== Model comparison (train vs test) ===");
            Console.WriteLine(
                "model".PadRight(NameColumnWidth) +
                "trainAcc".PadLeft(ValueColumnWidth) +
                "testAcc".PadLeft(ValueColumnWidth) +
                "testF1".PadLeft(ValueColumnWidth) +
                "time(s)".PadLeft(ValueColumnWidth));

            for (int i = 0; i < results.Count; i++)
            {
                ModelResult result = results[i];
                Console.WriteLine(
                    result.Model.Name.PadRight(NameColumnWidth) +
                    result.TrainMatrix.Accuracy.ToString("F3", CultureInfo.InvariantCulture).PadLeft(ValueColumnWidth) +
                    result.TestMatrix.Accuracy.ToString("F3", CultureInfo.InvariantCulture).PadLeft(ValueColumnWidth) +
                    result.TestMatrix.MacroF1.ToString("F3", CultureInfo.InvariantCulture).PadLeft(ValueColumnWidth) +
                    result.ElapsedSeconds.ToString("F1", CultureInfo.InvariantCulture).PadLeft(ValueColumnWidth));
            }
        }

        /// <summary>
        /// print the detailed report of the selected best model.
        /// </summary>
        public static void PrintBestModel(ModelResult best)
        {
            Console.WriteLine("\nPer-class report (test set):");
            Console.WriteLine(best.TestMatrix.PerClassReport());
            Console.WriteLine("Confusion matrix (true rows x predicted columns):");
            Console.WriteLine(best.TestMatrix.CountsGrid());
        }

        /// <summary>
        /// Overload of PrintBestModel used by the trining stage: print the same report, preceded by
        /// the the name of the selcted model and cross-validation score it was choosen on.
        /// </summary>
        public static void PrintBestModel(ModelResult best, double crossValidationMacroF1)
        {
            Console.WriteLine("\nSelected model: " + best.Model.Name
                            + "  (chosen on cross-validation macroF1 "
                            + crossValidationMacroF1.ToString("F3", CultureInfo.InvariantCulture)
                            + ", measured on the training set only)");
            Console.WriteLine("The test scores below were not used to make this choice.");
            PrintBestModel(best);
        }

        /// <summary>
        /// Print the outcome of the cross validation of one model.
        /// </summary>
        public static void PrintCrossValidation(CrossValidationResults results)
        {
            Console.WriteLine("  accuracy=" + results.MeanAccuracy.ToString("F3", CultureInfo.InvariantCulture)
                + "+/-" + results.StdAccuracy.ToString("F3", CultureInfo.InvariantCulture)
                + "  macroF1=" + results.MeanMacroF1.ToString("F3", CultureInfo.InvariantCulture)
                + "+/-" + results.StdMacroF1.ToString("F3", CultureInfo.InvariantCulture));
        }
    }
}
