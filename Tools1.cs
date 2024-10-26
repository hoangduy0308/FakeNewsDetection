
namespace Accord.Statistics
{
    using System;
    using System.Collections.Generic;
    using Accord.Math;
    using Accord.Math.Decompositions;
    using Accord.Statistics.Kernels;
    using Accord.Statistics.Distributions;
    using Accord.Statistics.Distributions.Fitting;
    using Accord.Compat;

    
    public static partial class Tools1
    {
       

       
        [Obsolete("Please use Classes.GetRatio instead.")]
        public static double[] Proportions(int[] positives, int[] negatives)
        {
            return Accord.Statistics.Classes.GetRatio(positives, negatives);
        }

      
        [Obsolete("Please use Classes.GetRatio instead.")]
        public static double[] Proportions(int[][] data, int positiveColumn, int negativeColumn)
        {
            return Accord.Statistics.Classes.GetRatio(data, positiveColumn, negativeColumn);
        }

      
        [Obsolete("Please use Classes.Summarize instead.")]
        public static int[][] Group(int[][] data, int labelColumn, int dataColumn)
        {
            return Accord.Statistics.Classes.Summarize(data, labelColumn, dataColumn);
        }

      
        [Obsolete("Please use Classes.Expand instead.")]
        public static int[][] Expand(int[] data, int[] positives, int[] negatives)
        {
            return Accord.Statistics.Classes.Expand(data, positives, negatives);
        }

        
        [Obsolete("Please use Classes.Expand instead.")]
        public static int[][] Expand(int[][] data, int labelColumn, int positiveColumn, int negativeColumn)
        {
            return Accord.Statistics.Classes.Expand(data, labelColumn, positiveColumn, negativeColumn);
        }

       
        [Obsolete("Please use Jagged.OneHot instead.")]
        public static double[][] Expand(int[] labels)
        {
            return Jagged.OneHot(labels, labels.DistinctCount());
        }

       
        [Obsolete("Please use Jagged.OneHot instead.")]
        public static double[][] Expand(int[] labels, double negative, double positive)
        {
            return Jagged.OneHot(labels).Replace(0, negative).Replace(1, positive);
        }

        
        [Obsolete("Please use Jagged.OneHot instead.")]
        public static double[][] Expand(int[] labels, int classes)
        {
            return Jagged.OneHot(labels, classes);
        }

        
        [Obsolete("Please use Jagged.OneHot instead.")]
        public static double[][] Expand(int[] labels, int classes, double negative, double positive)
        {
            return Jagged.OneHot(labels, classes).Replace(0, negative).Replace(1, positive);
        }
 


       
      
        public static double Determination(double[] actual, double[] expected)
        {
            // R-squared = 100 * SS(regression) / SS(total)

            double SSe = 0.0;
            double SSt = 0.0;
            double avg = 0.0;
            double d;

            // Calculate expected output mean
            for (int i = 0; i < expected.Length; i++)
                avg += expected[i];
            avg /= expected.Length;

            // Calculate SSe and SSt
            for (int i = 0; i < expected.Length; i++)
            {
                d = expected[i] - actual[i];
                SSe += d * d;

                d = expected[i] - avg;
                SSt += d * d;
            }

            // Calculate R-Squared
            return 1.0 - (SSe / SSt);
        }
       

        
        [Obsolete("Please use Vector.Sample instead.")]
        public static int[] RandomSample(int n, int k)
        {
            return Accord.Math.Vector.Sample(k, n);
        }

        
        [Obsolete("Please use Classes.Random instead.")]
        public static int[] RandomGroups(int size, int groups)
        {
            return Accord.Statistics.Classes.Random(size, groups);
        }

      
        [Obsolete("Please use Classes.Random instead.")]
        public static int[] RandomGroups(int size, double proportion)
        {
            return Accord.Statistics.Classes.Random(size, proportion);
        }

       
        [Obsolete("Please use Classes.Random instead.")]
        public static int[] RandomGroups(int[] labels, int classes, int groups)
        {
            return Accord.Statistics.Classes.Random(labels, classes, groups);
        }

       
        [Obsolete("Please use Vector.Sample instead.")]
        public static int[] Random(int n)
        {
            return Vector.Sample(n);
        }

       
        [Obsolete("Please use Vector.Shuffle instead.")]
        public static void Shuffle<T>(T[] array)
        {
            Vector.Shuffle(array);
        }

        
        [Obsolete("Please use Vector.Shuffle instead.")]
        public static void Shuffle<T>(IList<T> array)
        {
            Vector.Shuffle(array);
        }


      


        public static double[,] Whitening(double[,] value, out double[,] transformMatrix)
        {
            // TODO: Move into PCA and mark as obsolete
            if (value == null)
                throw new ArgumentNullException("value");


            int cols = value.GetLength(1);

            double[,] cov = value.Covariance();

            // Diagonalizes the covariance matrix
            var svd = new SingularValueDecomposition(cov,
                true,  // compute left vectors (to become a transformation matrix)
                false, // do not compute right vectors since they aren't necessary
                true,  // transpose if necessary to avoid erroneous assumptions in SVD
                true); // perform operation in-place, reducing memory usage


            // Retrieve the transformation matrix
            transformMatrix = svd.LeftSingularVectors;

            // Perform scaling to have unit variance
            double[] singularValues = svd.Diagonal;
            for (int i = 0; i < cols; i++)
                for (int j = 0; j < singularValues.Length; j++)
                    transformMatrix[i, j] /= Math.Sqrt(singularValues[j]);

            // Return the transformed data
            return Matrix.Dot(value, transformMatrix);
        }

        
        public static double[][] Whitening(double[][] value, out double[][] transformMatrix)
        {
            // TODO: Move into PCA and mark as obsolete
            if (value == null)
                throw new ArgumentNullException("value");


            int cols = value.Columns();

            double[][] cov = value.Covariance();

            // Diagonalizes the covariance matrix
            var svd = new JaggedSingularValueDecomposition(cov,
                true,  // compute left vectors (to become a transformation matrix)
                false, // do not compute right vectors since they aren't necessary
                true,  // transpose if necessary to avoid erroneous assumptions in SVD
                true); // perform operation in-place, reducing memory usage


            // Retrieve the transformation matrix
            transformMatrix = svd.LeftSingularVectors;

            // Perform scaling to have unit variance
            double[] singularValues = svd.Diagonal;
            for (int i = 0; i < cols; i++)
                for (int j = 0; j < singularValues.Length; j++)
                    transformMatrix[i][j] /= Math.Sqrt(singularValues[j]);

            // Return the transformed data
            return Matrix.Dot(value, transformMatrix);
        }

       
        public static TDistribution Fit<TDistribution>(this double[] observations, double[] weights = null)
            where TDistribution : IFittable<double>, new()
        {
            var dist = new TDistribution();
            dist.Fit(observations, weights);
            return dist;
        }

        
        public static TDistribution Fit<TDistribution>(this double[][] observations, double[] weights = null)
            where TDistribution : IFittable<double[]>, new()
        {
            var dist = new TDistribution();
            dist.Fit(observations, weights);
            return dist;
        }

       
        public static TDistribution Fit<TDistribution, TOptions>(this double[] observations, TOptions options, double[] weights = null)
            where TDistribution : IFittable<double, TOptions>, new()
            where TOptions : class, IFittingOptions
        {
            var dist = new TDistribution();
            dist.Fit(observations, weights, options);
            return dist;
        }

       
        public static TDistribution Fit<TDistribution, TOptions>(this double[][] observations, TOptions options, double[] weights = null)
            where TDistribution : IFittable<double[], TOptions>, new()
            where TOptions : class, IFittingOptions
        {
            var dist = new TDistribution();
            dist.Fit(observations, weights, options);
            return dist;
        }

          
        public static TDistribution FitNew<TDistribution, TObservations>(
            this TDistribution distribution, TObservations[] observations, double[] weights = null)
            where TDistribution : IFittable<TObservations>, ICloneable
        {
            var clone = (TDistribution)distribution.Clone();
            clone.Fit(observations, weights);
            return clone;
        }

        
        public static TDistribution FitNew<TDistribution, TObservations, TOptions>(
            this TDistribution distribution, TObservations[] observations, TOptions options, double[] weights = null)
            where TDistribution : IFittable<TObservations, TOptions>, ICloneable
            where TOptions : class, IFittingOptions
        {
            var clone = (TDistribution)distribution.Clone();
            clone.Fit(observations, weights, options);
            return clone;
        }

    }
}

