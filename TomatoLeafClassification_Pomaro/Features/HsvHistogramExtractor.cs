using System;
using System.Collections.Generic;
using System.Drawing;

namespace TomatoLeafClassification.Features
{
    /// <summary>
    /// Colour descriptor. Converts every pixel from RGB to HSV and builds three normalized
    /// histograms (Hue, Saturation, Value), each with a configurable number of bins. HSV separates
    /// colour (H, S) from brightness (V), which helps tell lesion colours apart. With 8 bins per
    /// channel this gives 24 features.
    /// </summary>
    public class HsvHistogramExtractor : FeatureExtractorBase
    {
        private int _bins;
        private List<string> _names;

        /// <summary>
        /// Creates the colour extractor.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">
        /// If <paramref name="binsPerChannel"/> is below 1.
        /// </exception>
        public HsvHistogramExtractor(int binsPerChannel)
        {
            if (binsPerChannel < 1)
                throw new ArgumentOutOfRangeException("binsPerChannel");
            _bins = binsPerChannel;
            _names = BuildNames(binsPerChannel);
        }

        public override int FeatureCount
        {
            get { return 3 * _bins; }
        }

        public override List<string> FeatureNames
        {
            get { return _names; }
        }

        /// <summary>
        /// Converts every pixel to HSV and builds a normalized histogram for each of the three
        /// channels, concatenated into one colour feature vector.
        /// </summary>
        public override double[] Extract(Bitmap image)
        {
            int width = image.Width;
            int height = image.Height;
            int n = width * height;

            double[] hHist = new double[_bins];
            double[] sHist = new double[_bins];
            double[] vHist = new double[_bins];

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    Color color = image.GetPixel(x, y);
                    double r = color.R / 255.0;
                    double g = color.G / 255.0;
                    double b = color.B / 255.0;

                    double hue, sat, val;
                    RgbToHsv(r, g, b, out hue, out sat, out val);

                    int hb = BinIndex(hue / 360.0, _bins);
                    int sb = BinIndex(sat, _bins);
                    int vb = BinIndex(val, _bins);

                    hHist[hb] = hHist[hb] + 1.0;
                    sHist[sb] = sHist[sb] + 1.0;
                    vHist[vb] = vHist[vb] + 1.0;
                }
            }

            // Normalize each histogram to sum 1 (invariant to the image size).
            double[] features = new double[3 * _bins];
            for (int i = 0; i < _bins; i++)
            {
                features[i] = hHist[i] / n;
                features[_bins + i] = sHist[i] / n;
                features[2 * _bins + i] = vHist[i] / n;
            }
            return features;
        }

        /// <summary>
        /// Maps a value in [0,1] to a bin index in [0, bins-1].
        /// </summary>
        private static int BinIndex(double value01, int bins)
        {
            int idx = (int)(value01 * bins);
            if (idx < 0) idx = 0;
            if (idx >= bins) idx = bins - 1;
            return idx;
        }

        /// <summary>
        /// Standard RGB (each channel in [0,1]) to HSV conversion. H in [0,360), S in [0,1], V in [0,1].
        /// </summary>
        private static void RgbToHsv(double r, double g, double b,
                                     out double h, out double s, out double v)
        {
            double max = Math.Max(r, Math.Max(g, b));
            double min = Math.Min(r, Math.Min(g, b));
            double delta = max - min;

            if (delta <= 0.0)
                h = 0.0;
            else if (max == r)
                h = (60.0 * ((g - b) / delta) + 360.0) % 360.0;
            else if (max == g)
                h = (60.0 * ((b - r) / delta) + 120.0) % 360.0;
            else
                h = (60.0 * ((r - g) / delta) + 240.0) % 360.0;

            if (max <= 0.0)
                s = 0.0;
            else
                s = delta / max;
            v = max;
        }

        /// <summary>
        /// Builds the feature-column names for the H, S and V histogram bins.
        /// </summary>
        private static List<string> BuildNames(int bins)
        {
            List<string> names = new List<string>();
            for (int i = 0; i < bins; i++)
                names.Add("hsv_H_bin" + i);
            for (int i = 0; i < bins; i++)
                names.Add("hsv_S_bin" + i);
            for (int i = 0; i < bins; i++)
                names.Add("hsv_V_bin" + i);
            return names;
        }
    }
}
