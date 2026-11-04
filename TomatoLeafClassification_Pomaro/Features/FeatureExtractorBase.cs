using System;
using System.Collections.Generic;
using System.Drawing;

namespace TomatoLeafClassification.Features
{
    /// <summary>
    /// Base class shared by the concrete extractors. It provides an Extract(string) overload that
    /// loads the file and delegates to Extract(Bitmap). Concrete classes implement Extract(Bitmap)
    /// and declare their feature names.
    /// </summary>
    public class FeatureExtractorBase : IFeatureExtractor
    {
        /// <summary>
        /// Protected constructor: the class is meant to be used only as a base for concrete
        /// extractors, not to be instantiated directly.
        /// </summary>
        protected FeatureExtractorBase()
        { }

        /// <summary>
        /// Length of the feature vector.
        /// </summary>
        /// <exception cref="NotSupportedException">
        /// if the extractor does not implement this property
        /// </exception>
        public virtual int FeatureCount
        {
            get { throw new NotSupportedException(GetType().Name + " does not implement FeatureCount."); }
        }

        /// <summary>
        /// Name of each feature column.
        /// </summary>
        /// <exception cref="NotSupportedException">
        /// if the extractor does not implement this property
        /// </exception>
        public virtual List<string> FeatureNames
        {
            get { throw new NotSupportedException(GetType().Name + " does not implement FeatureNames."); }
        }

        /// <summary>
        /// Computes the feature vector for one image.
        /// </summary>
        /// <exception cref="NotSupportedException">
        /// if the extractor does not implement this method
        /// </exception>
        public virtual double[] Extract(Bitmap image)
        {
            throw new NotSupportedException(GetType().Name + " does not implement Extract.");
        }

        /// <summary>
        /// Overload: load an image from disk, then extract its features.
        /// </summary>
        public double[] Extract(string filePath)
        {
            using (Bitmap bmp = new Bitmap(filePath))
            {
                return Extract(bmp);
            }
        }
    }
}
