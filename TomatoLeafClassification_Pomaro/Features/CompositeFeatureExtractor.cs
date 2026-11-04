using System;
using System.Collections.Generic;
using System.Drawing;

namespace TomatoLeafClassification.Features
{
    /// <summary>
    /// Combines several extractors into one by concatenating their outputs.
    /// </summary>
    public class CompositeFeatureExtractor : IFeatureExtractor
    {
        private List<IFeatureExtractor> _extractors;
        private List<string> _names;

        /// <summary>
        /// Builds a composite from the given extractors, concatenating their feature names.
        /// </summary>
        /// <exception cref="ArgumentException">If no extractor is provided.</exception>
        public CompositeFeatureExtractor(IFeatureExtractor[] extractors)
        {
            if (extractors == null || extractors.Length == 0)
                throw new ArgumentException("At least one extractor is required.");

            _extractors = new List<IFeatureExtractor>();
            _names = new List<string>();
            for (int i = 0; i < extractors.Length; i++)
            {
                _extractors.Add(extractors[i]);
                List<string> partNames = extractors[i].FeatureNames;
                for (int k = 0; k < partNames.Count; k++)
                    _names.Add(partNames[k]);
            }
        }

        public int FeatureCount
        {
            get { return _names.Count; }
        }

        public List<string> FeatureNames
        {
            get { return _names; }
        }

        /// <summary>
        /// Runs every sub-extractor and concatenates their vectors into one.
        /// </summary>
        public double[] Extract(Bitmap image)
        {
            double[] result = new double[_names.Count];
            int offset = 0;
            for (int i = 0; i < _extractors.Count; i++)
            {
                double[] part = _extractors[i].Extract(image);
                for (int k = 0; k < part.Length; k++)
                    result[offset + k] = part[k];
                offset = offset + part.Length;
            }
            return result;
        }
    }
}
