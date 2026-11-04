using System;
using System.IO;
using TomatoLeafClassification.Util;

namespace TomatoLeafClassification.Pipeline
{
    /// <summary>
    /// Runs the two stages of the pipeline in order. It exists so that Main stays a plain sequence
    /// of calls: the order of the stages lives here, where it can be changed without touching the
    /// entry point. When a features CSV is already on disk the user decides whether to reuse it,
    /// because that choice is the difference between a run of seconds and one of minutes.
    /// </summary>
    public static class PipelineRunner
    {
        /// <summary>
        /// Extracts the features when needed, then trains and evaluates the models.
        /// </summary>
        public static void Run()
        {
            if (ShouldExtract())
            {
                FeatureExtractionPipeline.Run();
                Console.WriteLine();
            }

            TrainingPipeline.Run();
        }

        /// <summary>
        /// Decides whether the features must be computed again: with no CSV there is nothing to
        /// choose, with one the user is asked.
        /// </summary>
        private static bool ShouldExtract()
        {
            string csv = Constants.DefaultFeaturesCsv;

            if (!File.Exists(csv))
            {
                Console.WriteLine("No '" + csv + "' found -> extracting the features "
                                + "(about 10 minutes).\n");
                return true;
            }

            Console.WriteLine("Found an existing '" + csv + "' (" + Describe(csv) + ").");
            Console.WriteLine("  [U] use it      -> start training right away");
            Console.WriteLine("  [R] rebuild it  -> extract the features again, "
                            + "about 10 minutes");
            return AskRebuild();
        }

        /// <summary>
        /// Reads the choice, insisting until it is one of the two accepted.
        /// </summary>
        private static bool AskRebuild()
        {
            while (true)
            {
                Console.Write("Your choice [U/R]: ");
                string answer = Console.ReadLine();

                if (answer == null) return false;
                answer = answer.Trim().ToLowerInvariant();

                if (answer == "u") { Console.WriteLine(); return false; }
                if (answer == "r") { Console.WriteLine(); return true; }

                Console.WriteLine("Please type U or R.");
            }
        }

        /// <summary>
        /// Describes the CSV so the user can tell how stale it is.
        /// </summary>
        private static string Describe(string csv)
        {
            FileInfo info = new FileInfo(csv);
            return (info.Length / 1024) + " KB, written on "
                 + info.LastWriteTime.ToString("yyyy-MM-dd HH:mm");
        }
    }
}