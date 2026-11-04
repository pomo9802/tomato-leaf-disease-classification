namespace TomatoLeafClassification.Util
{
    /// <summary>
    /// Provides access to configurable constants used throughout the project.
    /// </summary>
    public static class Constants
    {
        public const int ImageSize = 128; // Size of the input images

        public const int ProgressReportEvery = 500; // Report how often to report progress during training

        public const int HsvBinsPerChannel = 8; // Number of bins for each channel in HSV color space

        public const int GlcmGrayLevels = 16; // Number of gray levels for GLCM computation

        public const int GlcmOffsetDistance = 1; // Distance for GLCM computation

        public const string DefaultFeaturesCsv = "features.csv"; // Default CSV file name for storing extracted features

        public const double TrainRatio = 0.8; // Ratio of data to use for training (the rest will be used for testing)

        public const int RandomSeed = 42; // Seed for random number generation to ensure reproducibility

        public const int CrossValidationFolds = 5; // Number of folds for cross-validation

        public const int CrossValidationSubsampleCap = 4000; // Cap on the number of samples to use for cross-validation to speed up the process

        public const int KNeighbours = 5; // Number of neighbors for KNN classifier
    }
}
