using TomatoLeafClassification.Features;
using TomatoLeafClassification.Util;

namespace TomatoLeafClassification.Pipeline
{
    /// <summary>
    /// Single point of truth for how the feature vector is built, so that every image in the
    /// dataset is always described by exactly the same features, in the same order. Every size
    /// comes from Constants, so nothing has to be changed in two places.
    /// </summary>
    public static class FeaturePipeline
    {
        /// <summary>
        /// Builds the composite extractor used everywhere: an HSV colour histogram plus
        /// GLCM/Haralick texture features.
        /// </summary>
        public static IFeatureExtractor BuildExtractor()
        {
            IFeatureExtractor[] parts = new IFeatureExtractor[2];
            parts[0] = new HsvHistogramExtractor(Constants.HsvBinsPerChannel);
            parts[1] = new HaralickTextureExtractor(Constants.GlcmGrayLevels, Constants.GlcmOffsetDistance);
            return new CompositeFeatureExtractor(parts);
        }
    }
}