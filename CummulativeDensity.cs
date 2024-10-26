

namespace Accord.Statistics.Distributions.Univariate
{
    using System;
    using Accord.Math;
    using Accord.Statistics.Distributions;
    using Accord.Statistics.Distributions.Fitting;
    using Accord.Statistics.Distributions.Multivariate;
    using AForge;

  
    [Serializable]
    public class CummulativeDensity : UnivariateContinuousDistribution,
        IFittableDistribution<double, NormalOptions>,
        ISampleableDistribution<double>, IFormattable,
        IUnivariateFittableDistribution
    {

        // Distribution parameters
        private double mean = 0;   // mean μ
        private double stdDev = 1; // standard deviation σ

        // Distribution measures
        private double? entropy;

        // Derived measures
        private double variance = 1; // σ²
        private double lnconstant;   // log(1/sqrt(2*pi*variance))

        private bool immutable;

        // 97.5 percentile of standard normal distribution
        private const double p95 = 1.95996398454005423552;

        /// <summary>
        ///   Constructs a Normal (Gaussian) distribution
        ///   with zero mean and unit standard deviation.
        /// </summary>
        /// 
        public CummulativeDensity()
        {
            initialize(mean, stdDev, stdDev * stdDev);
        }

        /// <summary>
        ///   Constructs a Normal (Gaussian) distribution
        ///   with given mean and unit standard deviation.
        /// </summary>
        /// 
        /// <param name="mean">The distribution's mean value μ (mu).</param>
        /// 
        public CummulativeDensity(double mean)
        {
            initialize(mean, stdDev, stdDev * stdDev);
        }

        /// <summary>
        ///   Constructs a Normal (Gaussian) distribution
        ///   with given mean and standard deviation.
        /// </summary>
        /// 
        /// <param name="mean">The distribution's mean value μ (mu).</param>
        /// <param name="stdDev">The distribution's standard deviation σ (sigma).</param>
        /// 
        public CummulativeDensity(double mean, double stdDev)
        {
            if (stdDev <= 0)
            {
                throw new ArgumentOutOfRangeException("stdDev",
                    "Standard deviation must be positive.");
            }

            initialize(mean, stdDev, stdDev * stdDev);
        }



        public override double Mean
        {
            get { return mean; }
        }
        public void getdata()
        {

        }
       
       
        public override double Median
        {
            get
            {
                Accord.Diagnostics.Debug.Assert(mean.IsEqual(base.Median, 1e-10));
                return mean;
            }
        }

    
        public override double Variance
        {
            get { return variance; }
        }

     
        public override double StandardDeviation
        {
            get { return stdDev; }
        }

        
        public override double Mode
        {
            get { return mean; }
        }

      
        public double Skewness
        {
            get { return 0; }
        }

        
        public double Kurtosis
        {
            get { return 0; }
        }

         
        public override DoubleRange Support
        {
            get { return new DoubleRange(Double.NegativeInfinity, Double.PositiveInfinity); }
        }

      
        public override double Entropy
        {
            get
            {
                if (!entropy.HasValue)
                    entropy = 0.5 * (Math.Log(2.0 * Math.PI * variance) + 1);

                return entropy.Value;
            }
        }

        
        public override double DistributionFunction(double x)
        {
            return Normal.Function((x - mean) / stdDev);
        }

        
        public override double ComplementaryDistributionFunction(double x)
        {
            return Normal.Complemented((x - mean) / stdDev);
        }


       
        public double InverseDistributionFunction(double p)
        {
            double inv = Normal.Inverse(p);

            double icdf = mean + stdDev * inv;

#if DEBUG
            double baseValue = base.InverseDistributionFunction(p);
            double r1 = DistributionFunction(baseValue);
            double r2 = DistributionFunction(icdf);

            bool close = r1.IsEqual(r2, 1e-6);

            if (!close)
            {
                throw new Exception();
            }
#endif

            return icdf;
        }

        
        public override double ProbabilityDensityFunction(double x)
        {
            double z = (x - mean) / stdDev;
            double lnp = lnconstant - z * z * 0.5;

            return Math.Exp(lnp);
        }

        
        public override double LogProbabilityDensityFunction(double x)
        {
            double z = (x - mean) / stdDev;
            double lnp = lnconstant - z * z * 0.5;

            return lnp;
        }

      
        public double ZScore(double x)
        {
            return (x - mean) / stdDev;
        }



      
        public static CummulativeDensity Standard
        {
            get { return standard; }
        }

        private static readonly CummulativeDensity standard = new CummulativeDensity()
        {
            immutable = true
        };

       
        public override void Fit(double[] observations, double[] weights, IFittingOptions options)
        {
            NormalOptions normalOptions = options as NormalOptions;
            if (options != null && normalOptions == null)
                throw new ArgumentException("The specified options' type is invalid.", "options");

            Fit(observations, weights, normalOptions);
        }

        
        public void Fit(double[] observations, double[] weights, NormalOptions options)
        {
            if (immutable)
                throw new InvalidOperationException("NormalDistribution.Standard is immutable.");

            double mu, var;

            if (weights != null)
            {
#if DEBUG
                for (int i = 0; i < weights.Length; i++)
                    if (Double.IsNaN(weights[i]) || Double.IsInfinity(weights[i]))
                        throw new ArgumentException("Invalid numbers in the weight vector.", "weights");
#endif

                // Compute weighted mean
                mu = Measures.WeightedMean(observations, weights);

                // Compute weighted variance
                var = Measures.WeightedVariance(observations, weights, mu);
            }
            else
            {
                // Compute weighted mean
                mu = Measures.Mean(observations);

                // Compute weighted variance
                var = Measures.Variance(observations, mu);
            }

            if (options != null)
            {
                // Parse optional estimation options
                double regularization = options.Regularization;

                if (var == 0 || Double.IsNaN(var) || Double.IsInfinity(var))
                    var = regularization;
            }

            if (Double.IsNaN(var) || var <= 0)
            {
                throw new ArgumentException("Variance is zero. Try specifying "
                    + "a regularization constant in the fitting options.");
            }

            initialize(mu, Math.Sqrt(var), var);
        }


        
        public override object Clone()
        {
            return new NormalDistribution(mean, stdDev);
        }


        
        public override string ToString(string format, IFormatProvider formatProvider)
        {
            return String.Format(formatProvider, "N(x; μ = {0}, σ² = {1})",
                mean.ToString(format, formatProvider),
                variance.ToString(format, formatProvider));
        }


        private void initialize(double mu, double dev, double var)
        {
            this.mean = mu;
            this.stdDev = dev;
            this.variance = var;

            // Compute derived values
            this.lnconstant = -Math.Log(Constants.Sqrt2PI * dev);
        }


       
        public static NormalDistribution Estimate(double[] observations)
        {
            return Estimate(observations, null, null);
        }

      
        public static NormalDistribution Estimate(double[] observations, NormalOptions options)
        {
            return Estimate(observations, null, options);
        }

    
        public static NormalDistribution Estimate(double[] observations, double[] weights, NormalOptions options)
        {
            NormalDistribution n = new NormalDistribution();
            n.Fit(observations, weights, options);
            return n;
        }


        
        public MultivariateNormalDistribution ToMultivariateDistribution()
        {
            return new MultivariateNormalDistribution(
                new double[] { mean }, new double[,] { { variance } });
        }

       
       
        public double[] Generate(int samples, double[] result)
        {
            return Random(mean, stdDev, samples, result);
        }

       
        public  double Generate()
        {
            return Random(mean, stdDev);
        }

       
        public static double Random(double mean, double stdDev)
        {
            return Random() * stdDev + mean;
        }

        
        public static double[] Random(double mean, double stdDev, int samples)
        {
            return Random(mean, stdDev, samples, new double[samples]);
        }

        
        public static double[] Random(double mean, double stdDev, int samples, double[] result)
        {
            Random(samples, result);
            for (int i = 0; i < samples; i++)
                result[i] = result[i] * stdDev + mean;
            return result;
        }



        [ThreadStatic]
        private static bool useSecond = false;

        [ThreadStatic]
        private static double secondValue = 0;

      
        public static double[] Random(int samples, double[] result)
        {
            var rand = Accord.Math.Random.Generator.Random;

            bool useSecond = CummulativeDensity.useSecond;
            double secondValue = CummulativeDensity.secondValue;

            for (int i = 0; i < samples; i++)
            {
                // check if we can use second value
                if (useSecond)
                {
                    // return the second number
                    useSecond = false;
                    result[i] = secondValue;
                    continue;
                }

             

                double x1, x2, w, firstValue;

                // generate new numbers
                do
                {
                    x1 = rand.NextDouble() * 2.0 - 1.0;
                    x2 = rand.NextDouble() * 2.0 - 1.0;
                    w = x1 * x1 + x2 * x2;
                }
                while (w >= 1.0);

                w = Math.Sqrt((-2.0 * Math.Log(w)) / w);

                // get two standard random numbers
                firstValue = x1 * w;
                secondValue = x2 * w;

                useSecond = true;

                // return the first number
                result[i] = firstValue;
            }

            CummulativeDensity.useSecond = useSecond;
            CummulativeDensity.secondValue = secondValue;

            return result;
        }

       
        public static double Random()
        {
            var rand = Accord.Math.Random.Generator.Random;

            // check if we can use second value
            if (useSecond)
            {
                // return the second number
                useSecond = false;
                return secondValue;
            }

            double x1, x2, w, firstValue;

            // generate new numbers
            do
            {
                x1 = rand.NextDouble() * 2.0 - 1.0;
                x2 = rand.NextDouble() * 2.0 - 1.0;
                w = x1 * x1 + x2 * x2;
            }
            while (w >= 1.0);

            w = Math.Sqrt((-2.0 * Math.Log(w)) / w);

            // get two standard random numbers
            firstValue = x1 * w;
            secondValue = x2 * w;

            useSecond = true;

            // return the first number
            return firstValue;
        }
    }
}
