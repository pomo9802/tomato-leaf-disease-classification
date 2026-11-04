using System;
using System.Collections.Generic;
using System.Drawing;

namespace TomatoLeafClassification.Features
{
    /// <summary>
    /// Texture descriptor based on the Gray Level Co-occurrence Matrix (GLCM) and Haralick
    /// statistics. The image is turned to gray, quantized to a few levels, and a co-occurrence
    /// matrix is accumulated over four directions (0, 45, 90, 135 degrees).
    /// </summary>
    public class HaralickTextureExtractor : FeatureExtractorBase
    {
        private int _levels;
        private int _distance;
        private List<string> _names;

        /// <summary>
        /// Creates the texture extractor.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">
        /// If the arguments are below their minimum.
        /// </exception>
        public HaralickTextureExtractor(int grayLevels, int distance)
        {
            if (grayLevels < 2)
                throw new ArgumentOutOfRangeException("grayLevels");
            if (distance < 1)
                throw new ArgumentOutOfRangeException("distance");
            _levels = grayLevels;
            _distance = distance;

            _names = new List<string>();
            _names.Add("glcm_contrast");
            _names.Add("glcm_correlation");
            _names.Add("glcm_energy");
            _names.Add("glcm_homogeneity");
            _names.Add("glcm_entropy");
            _names.Add("glcm_dissimilarity");
        }

        public override int FeatureCount
        {
            get { return _names.Count; }
        }

        public override List<string> FeatureNames
        {
            get { return _names; }
        }

        /// <summary>
        /// Builds a gray-level co-occurrence matrix (GLCM) from the image and derives six Haralick
        /// texture descriptors from it.
        /// </summary>
        public override double[] Extract(Bitmap image)
        {
            int width = image.Width;
            int height = image.Height;

            // 1) Grayscale (luminance) + quantization to _levels gray levels.
            int[] q = new int[width * height];
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    Color color = image.GetPixel(x, y);
                    double gray = 0.299 * color.R + 0.587 * color.G + 0.114 * color.B;
                    int level = (int)(gray / 256.0 * _levels);
                    if (level >= _levels) level = _levels - 1;
                    q[y * width + x] = level;
                }
            }

            // 2) Accumulate the GLCM over four directions, made symmetric.
            double[,] glcm = new double[_levels, _levels];
            int d = _distance;
            int[] dyList = new int[] { 0, -d, -d, -d };
            int[] dxList = new int[] { d, d, 0, -d };

            for (int dir = 0; dir < 4; dir++)
            {
                int dy = dyList[dir];
                int dx = dxList[dir];
                for (int y = 0; y < height; y++)
                {
                    int y2 = y + dy;
                    if (y2 < 0 || y2 >= height) continue;
                    for (int x = 0; x < width; x++)
                    {
                        int x2 = x + dx;
                        if (x2 < 0 || x2 >= width) continue;
                        int a = q[y * width + x];
                        int c = q[y2 * width + x2];
                        glcm[a, c] = glcm[a, c] + 1.0;
                        glcm[c, a] = glcm[c, a] + 1.0;
                    }
                }
            }

            // 3) Normalize to a probability distribution.
            double total = 0.0;
            for (int i = 0; i < _levels; i++)
                for (int j = 0; j < _levels; j++)
                    total = total + glcm[i, j];
            if (total <= 0.0)
                return new double[_names.Count];
            for (int i = 0; i < _levels; i++)
                for (int j = 0; j < _levels; j++)
                    glcm[i, j] = glcm[i, j] / total;

            // 4) Haralick descriptors from the normalized GLCM.
            return ComputeHaralick(glcm);
        }

        /// <summary>
        /// Computes the six Haralick descriptors from a normalized GLCM.
        /// </summary>
        private double[] ComputeHaralick(double[,] p)
        {
            double contrast = 0.0;
            double energy = 0.0;
            double homogeneity = 0.0;
            double entropy = 0.0;
            double dissimilarity = 0.0;

            // Marginal means.
            double muX = 0.0;
            double muY = 0.0;
            for (int i = 0; i < _levels; i++)
                for (int j = 0; j < _levels; j++)
                {
                    muX = muX + i * p[i, j];
                    muY = muY + j * p[i, j];
                }

            // Marginal standard deviations.
            double sigmaX = 0.0;
            double sigmaY = 0.0;
            for (int i = 0; i < _levels; i++)
                for (int j = 0; j < _levels; j++)
                {
                    sigmaX = sigmaX + (i - muX) * (i - muX) * p[i, j];
                    sigmaY = sigmaY + (j - muY) * (j - muY) * p[i, j];
                }
            sigmaX = Math.Sqrt(sigmaX);
            sigmaY = Math.Sqrt(sigmaY);

            double corrNum = 0.0;
            for (int i = 0; i < _levels; i++)
                for (int j = 0; j < _levels; j++)
                {
                    double pij = p[i, j];
                    int diff = i - j;
                    contrast = contrast + diff * diff * pij;
                    dissimilarity = dissimilarity + Math.Abs(diff) * pij;
                    energy = energy + pij * pij;
                    homogeneity = homogeneity + pij / (1.0 + diff * diff);
                    if (pij > 0.0)
                        entropy = entropy - pij * Math.Log(pij);
                    corrNum = corrNum + i * j * pij;
                }

            double correlation;
            if (sigmaX > 0.0 && sigmaY > 0.0)
                correlation = (corrNum - muX * muY) / (sigmaX * sigmaY);
            else
                correlation = 0.0;

            double[] result = new double[6];
            result[0] = contrast;
            result[1] = correlation;
            result[2] = energy;
            result[3] = homogeneity;
            result[4] = entropy;
            result[5] = dissimilarity;
            return result;
        }
    }
}
