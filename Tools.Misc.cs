

namespace Accord.Statistics
{
    using Accord.Diagnostics;
    using Accord.Math;
    using Accord.Math.Decompositions;
    using Accord.Statistics.Kernels;
    using System;
    using System.Collections.Generic;
    using System.Linq;

    static partial class Tools1
    {

       
        public static DoubleRange InnerFence(this DoubleRange quartiles)
        {
            return new DoubleRange(
                quartiles.Min - 1.5 * quartiles.Length,
                quartiles.Max + 1.5 * quartiles.Length);
        }

       
        public static DoubleRange OuterFence(this DoubleRange quartiles)
        {
            return new DoubleRange(
                quartiles.Min - 3 * quartiles.Length,
                quartiles.Max + 3 * quartiles.Length);
        }


        
        public static double[] Rank(this double[] samples, bool alreadySorted = false, bool adjustForTies = true)
        {
            bool hasTies;
            return Rank(samples, hasTies: out hasTies, alreadySorted: alreadySorted, adjustForTies: adjustForTies);
        }

       
        public static double[] Rank(this double[] samples, out bool hasTies,
            bool alreadySorted = false, bool adjustForTies = true)
        {
            int[] idx = Vector.Range(0, samples.Length);

            if (!alreadySorted)
            {
                samples = (double[])samples.Clone();
                Array.Sort(samples, idx);
            }

            double[] ranks = new double[samples.Length];

            double tieSum = 0;
            int tieSize = 0;
            hasTies = false;

            if (samples.Length == 0)
                return new double[0];

            ranks[0] = 1;

            if (adjustForTies)
            {
                int r = 1;
                for (int i = 1; i < ranks.Length; i++)
                {
                    // Check if we have a tie
                    if (samples[i] != samples[i - 1])
                    {
                        // This is not a tie.
                        // Was a tie before?
                        if (tieSize > 0)
                        {
                            // Yes. Then set the previous
                            // elements with the average.

                            for (int j = 0; j < tieSize + 1; j++)
                            {
                                int k = i - j - 1;
                                ranks[k] = (r + tieSum) / (tieSize + 1.0);
                            }

                            tieSize = 0;
                            tieSum = 0;
                        }

                        ranks[i] = ++r;
                    }
                    else
                    {
                        // This is a tie. Compute how 
                        // long we have been in a tie.
                        tieSize++;
                        tieSum += r++;
                        hasTies = true;
                    }
                }

                // Handle the last ties
                if (tieSize > 0)
                {
                    // We were still in the middle of a tie
                    for (int j = 0; j < tieSize + 1; j++)
                    {
                        int k = samples.Length - j - 1;
                        ranks[k] = (r + tieSum) / (tieSize + 1.0);
                    }
                }
            }
            else
            {
                // No need to adjust for ties
                for (int i = 1, r = 1; i < ranks.Length; i++)
                {
                    if (samples[i] == samples[i - 1])
                        hasTies = true;

                    ranks[i] = ++r;
                }
            }

            if (!alreadySorted)
                Array.Sort(idx, ranks);

            return ranks;
        }

        
        public static int[] Ties(this double[] ranks)
        {
            SortedDictionary<double, int> ties;
            return Ties(ranks, out ties);
        }

       
        public static int[] Ties(this double[] ranks, out SortedDictionary<double, int> counts)
        {
            counts = new SortedDictionary<double, int>();
            for (int i = 0; i < ranks.Length; i++)
            {
                double r = ranks[i];

                int c;
                if (!counts.TryGetValue(r, out c))
                    c = 0;

                counts[r] = c + 1;
            }

            int[] ties = new int[counts.Count];
            double[] sorted = counts.Keys.Sorted();
            for (int i = 0; i < sorted.Length; i++)
                ties[i] = counts[sorted[i]];
            return ties;
        }




       
        public static double[,] RandomCovariance(int size, double minValue, double maxValue)
        {
            double[,] A = Accord.Math.Matrix.Random(size, minValue, maxValue, symmetric: true);

            var gso = new GramSchmidtOrthogonalization(A);
            double[,] Q = gso.OrthogonalFactor;

            double[] diagonal = Vector.Random(size, minValue, maxValue).Abs();
            double[,] psd = Matrix.Dot(Q.TransposeAndDotWithDiagonal(diagonal), Q);

            Accord.Diagnostics.Debug.Assert(psd.IsPositiveDefinite());

            return psd;
        }


        public static double Distance(this IKernel kernel, double[] x, double[] y)
        {
            return kernel.Function(x, x) + kernel.Function(y, y) - 2 * kernel.Function(x, y);
        }




        
        public static double[] Center(this double[] observation, double[] result = null)
        {
            return Center(observation, observation.Mean(), result);
        }

      
        public static double[] Center(this double[] values, double mean, double[] result = null)
        {
            if (result == null)
                result = new double[values.Length];

            for (int i = 0; i < values.Length; i++)
                result[i] = values[i] - mean;

            return result;
        }


        
        public static double[,] Center(this double[,] matrix, bool inPlace = false)
        {
            return Center(matrix, Measures.Mean(matrix, dimension: 0), inPlace);
        }

       
        public static double[,] Center(this double[,] matrix, double[] means, bool inPlace = false)
        {
            int rows = matrix.GetLength(0);
            int cols = matrix.GetLength(1);

            double[,] result = inPlace ? matrix : new double[rows, cols];

            for (int i = 0; i < rows; i++)
                for (int j = 0; j < cols; j++)
                    result[i, j] = matrix[i, j] - means[j];

            return result;
        }

       
        public static double[][] Center(this double[][] matrix, bool inPlace = false)
        {
            return Center(matrix, Measures.Mean(matrix, dimension: 0), inPlace);
        }

        
        public static double[][] Center(this double[][] matrix, double[] means, bool inPlace = false)
        {
            double[][] result = matrix;

            if (!inPlace)
            {
                result = new double[matrix.Length][];
                for (int i = 0; i < matrix.Length; i++)
                    result[i] = new double[matrix[i].Length];
            }

            for (int i = 0; i < matrix.Length; i++)
            {
                double[] row = result[i];
                for (int j = 0; j < row.Length; j++)
                    row[j] = matrix[i][j] - means[j];
            }

            return result;
        }


       
        public static double[] Standardize(this double[] values, bool inPlace = false)
        {
            return Standardize(values, Measures.StandardDeviation(values), inPlace);
        }

      
        public static double[] Standardize(this double[] values, double standardDeviation, bool inPlace = false)
        {

            double[] result = inPlace ? values : new double[values.Length];
            for (int i = 0; i < values.Length; i++)
                result[i] = values[i] / standardDeviation;

            return result;
        }

        
        public static double[,] Standardize(this double[,] matrix, bool inPlace = false)
        {
            return Standardize(matrix, Measures.StandardDeviation(matrix), inPlace);
        }

        
        public static double[,] Standardize(this double[,] matrix, double[] standardDeviations, bool inPlace = false, double tol = 1e-12)
        {
            int rows = matrix.GetLength(0);
            int cols = matrix.GetLength(1);

            double[,] result = inPlace ? matrix : new double[rows, cols];

            if (tol == 0)
            {
                for (int i = 0; i < standardDeviations.Length; i++)
                    if (standardDeviations[i] == 0 && tol == 0)
                        throw new ArithmeticException("Standard deviation cannot be" +
                        " zero (cannot standardize the constant variable at column index " + i + ").");
            }

            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < cols; j++)
                {
                    double stdDev = standardDeviations[j] > 0 ? standardDeviations[j] : tol;
                    result[i, j] = matrix[i, j] / stdDev;
                }
            }

            return result;
        }


       
        public static double[][] Standardize(this double[][] matrix, bool inPlace = false)
        {
            return Standardize(matrix, Measures.StandardDeviation(matrix), inPlace);
        }

        
        public static double[][] Standardize(this double[][] matrix, double[] standardDeviations, bool inPlace = false, double tol = 1e-12)
        {
            double[][] result = inPlace ? matrix : Jagged.CreateAs(matrix);

            if (tol == 0)
            {
                for (int i = 0; i < standardDeviations.Length; i++)
                    if (standardDeviations[i] == 0)
                        throw new ArithmeticException("Standard deviation cannot be" +
                        " zero (cannot standardize the constant variable at column index " + i + ").");
            }


            for (int i = 0; i < matrix.Length; i++)
            {
                double[] resultRow = result[i];
                double[] sourceRow = matrix[i];
                for (int j = 0; j < resultRow.Length; j++)
                {
                    double stdDev = standardDeviations[j] > 0 ? standardDeviations[j] : tol;
                    resultRow[j] = sourceRow[j] / stdDev;
                }
            }

            return result;
        }


    }
}

