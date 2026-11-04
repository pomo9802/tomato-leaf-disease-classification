using System;
using System.IO;
using TomatoLeafClassification.Exceptions;
using TomatoLeafClassification.Pipeline;

namespace TomatoLeafClassification
{
    /// <summary>
    /// Entry point of the application. It holds no logic of its own: it starts the pipeline and
    /// turns any error into a message for the user. This is the only place where exceptions are
    /// caught and reported; the classes of the other layers throw and never print, so they stay
    /// reusable outside a console.
    /// </summary>
    internal class Program
    {
        /// <summary>
        /// Starts the pipeline and reports any failure.
        /// </summary>
        private static void Main()
        {
            try
            {
                PipelineRunner.Run();
            }
            catch (InvalidImageFormatException exception)
            {
                ReportError("Image error", exception.Message);
            }
            catch (DatasetException exception)
            {
                ReportError("Dataset error", exception.Message);
            }
            catch (IOException exception)
            {
                ReportError("File error", exception.Message);
            }
            catch (UnauthorizedAccessException exception)
            {
                ReportError("Access denied", exception.Message);
            }
            catch (Exception exception)
            {
                ReportError("Unexpected error", exception.Message);
            }

            WaitBeforeClosing();
        }

        /// <summary>
        /// Prints an error message in red, then restores the console colours.
        /// </summary>
        private static void ReportError(string kind, string message)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("\n" + kind + ": " + message);
            Console.ResetColor();
        }

        /// <summary>
        /// Keeps the console window open so the results stay readable when the program was started
        /// by double-clicking it rather than from a shell.
        /// </summary>
        private static void WaitBeforeClosing()
        {
            if (!Environment.UserInteractive) return;

            Console.WriteLine("\nDone. Press any key to close...");
            Console.ReadKey();
        }
    }
}