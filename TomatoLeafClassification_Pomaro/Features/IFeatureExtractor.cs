using System.Collections.Generic;
using System.Drawing;

namespace TomatoLeafClassification.Features
{
    /// <summary>
    /// Contract for every feature extractor. Given an image, it produces a fixed-length numeric vector.
    /// </summary>
    public interface IFeatureExtractor
    {
        /// <summary>
        /// Length of the feature vector produced by this extractor.
        /// </summary>
        int FeatureCount { get; }

        /// <summary>
        /// Human-readable name of each feature (used as CSV headers).
        /// </summary>
        List<string> FeatureNames { get; }

        /// <summary>
        /// Computes the feature vector for one image.
        /// </summary>
        /// <param name="image">The (already resized) image to describe.</param>
        /// <returns>A fixed-length numeric feature vector.</returns>
        double[] Extract(Bitmap image);
    }
}
