
namespace Accord.Statistics.Distributions.Univariate
{
    using System;
    using Accord.Math;
    using Accord.Statistics.Distributions.Fitting;
    using AForge;
    using Tools = Statistics.Tools;

   
    [Serializable]
    public class EmpiricalCummulativeDensityDistribution : UnivariateContinuousDistribution,
        IFittableDistribution<double, EmpiricalOptions>,
        ISampleableDistribution<double>
    {

        // Distribution parameters
        double[] samples;
        double smoothing;

        WeightType type;
        double[] weights;
        int[] repeats;

        double sumOfWeights;
        int numberOfSamples;


        // Derived measures
        double? mean;
        double? variance;
        double? entropy;
        double? mode;

        double constant;


       
        public EmpiricalCummulativeDensityDistribution(double[] samples)
        {
            initialize(samples, null, null, null);
        }

    
        public EmpiricalCummulativeDensityDistribution(double[] samples, double smoothing)
        {
            initialize(samples, null, null, smoothing);
        }

       
        public EmpiricalCummulativeDensityDistribution(double[] samples, double[] weights)
        {
            initialize(samples, weights, null, null);
        }
        public void getdata()
        {

        }
       
       
        public EmpiricalCummulativeDensityDistribution(double[] samples, int[] weights)
        {
            initialize(samples, null, weights, null);
        }

       
        public EmpiricalCummulativeDensityDistribution(double[] samples, double[] weights, double smoothing)
        {
            initialize(samples, weights, null, smoothing);
        }

      
        public EmpiricalCummulativeDensityDistribution(double[] samples, int[] weights, double smoothing)
        {
            initialize(samples, null, weights, smoothing);
        }

    
        public double[] Samples
        {
            get { return samples; }
        }

       
        public double[] Weights
        {
            get
            {
                if (weights == null)
                {
                    weights = new double[samples.Length];
                    for (int i = 0; i < weights.Length; i++)
                        weights[i] = Counts[i] / (double)Length;
                }

                return weights;
            }
        }

        public int[] Counts
        {
            get
            {
                if (repeats == null)
                {
                    repeats = new int[samples.Length];
                    for (int i = 0; i < repeats.Length; i++)
                        repeats[i] = 1;
                }

                return repeats;
            }
        }

     
        public int Length
        {
            get { return numberOfSamples; }
        }

        /// <summary>
        ///   Gets the bandwidth smoothing parameter
        ///   used in the kernel density estimation.
        /// </summary>
        /// 
        public double Smoothing
        {
            get { return smoothing; }
        }


        public override double Mean
        {
            get
            {
                if (mean == null)
                {
                    if (type == WeightType.None)
                        mean = Measures.Mean(samples);

                    else if (type == WeightType.Repetition)
                        mean = Measures.WeightedMean(samples, repeats);

                    else if (type == WeightType.Fraction)
                        mean = Measures.WeightedMean(samples, weights);
                }

                return mean.Value;
            }
        }

        
        public override double Mode
        {
            get
            {
                if (mode == null)
                {
                    if (type == WeightType.None)
                        mode = Measures.Mode(samples);

                    else if (type == WeightType.Repetition)
                        mode = Measures.WeightedMode(samples, repeats);

                    else if (type == WeightType.Fraction)
                        mode = Measures.WeightedMode(samples, weights);
                }

                return mode.Value;
            }
        }

      
        public override double Variance
        {
            get
            {
                if (variance == null)
                {
                    if (type == WeightType.None)
                        variance = Measures.Variance(samples);

                    else if (type == WeightType.Repetition)
                        variance = Measures.WeightedVariance(samples, repeats);

                    else if (type == WeightType.Fraction)
                        variance = Measures.WeightedVariance(samples, weights);
                }

                return variance.Value;
            }
        }

      
        public override double Entropy
        {
            get
            {
                if (entropy == null)
                {
                    if (type == WeightType.None)
                        entropy = Measures.Entropy(samples, ProbabilityDensityFunction);

                    else if (type == WeightType.Repetition)
                        entropy = Measures.WeightedEntropy(samples, repeats, ProbabilityDensityFunction);

                    else if (type == WeightType.Fraction)
                        entropy = Measures.WeightedEntropy(samples, weights, ProbabilityDensityFunction);
                }

                return entropy.Value;
            }
        }

       
        public override DoubleRange Support
        {
            get { return new DoubleRange(Double.NegativeInfinity, Double.PositiveInfinity); }
        }

       
        public override double DistributionFunction(double x)
        {
            if (type == WeightType.None)
            {
                int sum = 0; // Normal sample, no weights
                for (int i = 0; i < samples.Length; i++)
                {
                    if (samples[i] <= x)
                        sum++;
                }

                return sum / (double)numberOfSamples;
            }

            if (type == WeightType.Repetition)
            {
                int sum = 0; // Repetition counts weights
                for (int i = 0; i < samples.Length; i++)
                {
                    if (samples[i] <= x)
                        sum += repeats[i];
                }

                return sum / (double)numberOfSamples;
            }

            if (type == WeightType.Fraction)
            {
                double sum = 0; // Fractional weights
                for (int i = 0; i < samples.Length; i++)
                {
                    if (samples[i] <= x)
                        sum += weights[i];
                }

                return sum / sumOfWeights;
            }

            throw new InvalidOperationException();
        }

       
        public override double ProbabilityDensityFunction(double x)
        {
            // References:
            //  - Bishop, Christopher M.; Pattern Recognition and Machine Learning. 

            double p = 0;

            if (type == WeightType.None)
            {
                // Normal samples, not using any weights
                for (int i = 0; i < samples.Length; i++)
                {
                    double z = (x - samples[i]) / smoothing;
                    p += Math.Exp(-z * z * 0.5);
                }
            }

            else if (type == WeightType.Repetition)
            {
                // Weighted sample using discrete counts
                for (int i = 0; i < samples.Length; i++)
                {
                    double z = (x - samples[i]) / smoothing;
                    p += repeats[i] * Math.Exp(-z * z * 0.5);
                }
            }

            else if (type == WeightType.Fraction)
            {
                // Weighted sample using fractional weights
                for (int i = 0; i < samples.Length; i++)
                {
                    double z = (x - samples[i]) / smoothing;
                    p += weights[i] * Math.Exp(-z * z * 0.5);
                }
            }

            return p * constant;
        }

       
        public override void Fit(double[] observations, double[] weights, IFittingOptions options)
        {
            Fit(observations, weights, options as EmpiricalOptions);
        }

         
        public void Fit(double[] observations, double[] weights, EmpiricalOptions options)
        {
            double? smoothing = null;
            bool inPlace = false;

            if (options != null)
            {
                smoothing = options.SmoothingRule(observations, weights, null);
                inPlace = options.InPlace;
            }

            if (!inPlace)
            {
                observations = (double[])observations.Clone();

                if (weights != null)
                    weights = (double[])weights.Clone();
            }

            initialize(observations, weights, null, smoothing);
        }

       
        public void Fit(double[] observations, int[] weights, EmpiricalOptions options)
        {
            double? smoothing = null;
            bool inPlace = false;

            if (options != null)
            {
                smoothing = options.SmoothingRule(observations, null, weights);
                inPlace = options.InPlace;
            }

            if (!inPlace)
            {
                observations = (double[])observations.Clone();

                if (weights != null)
                    weights = (int[])weights.Clone();
            }

            initialize(observations, null, weights, smoothing);
        }

       
        public override object Clone()
        {
            var clone = new EmpiricalCummulativeDensityDistribution();

            clone.type = type;
            clone.sumOfWeights = sumOfWeights;
            clone.numberOfSamples = numberOfSamples;
            clone.smoothing = smoothing;
            clone.constant = constant;

            clone.samples = (double[])samples.Clone();

            if (weights != null)
                clone.weights = (double[])weights.Clone();

            if (repeats != null)
                clone.repeats = (int[])repeats.Clone();

            return clone;
        }


        private EmpiricalCummulativeDensityDistribution()
        {
        }

        private void initialize(double[] observations, double[] weights, int[] repeats, double? smoothing)
        {
            if (smoothing == null)
            {
                smoothing = SmoothingRule(observations, weights, repeats);
            }

            this.samples = observations;
            this.weights = weights;
            this.repeats = repeats;
            this.smoothing = smoothing.Value;


            if (weights != null)
            {
                this.type = WeightType.Fraction;
                this.numberOfSamples = samples.Length;
                this.sumOfWeights = weights.Sum();
                this.constant = 1.0 / (Constants.Sqrt2PI * this.smoothing);
            }
            else if (repeats != null)
            {
                this.type = WeightType.Repetition;
                this.numberOfSamples = repeats.Sum();
                this.sumOfWeights = 1.0;
                this.constant = 1.0 / (Constants.Sqrt2PI * this.smoothing * numberOfSamples);
            }
            else
            {
                this.type = WeightType.None;
                this.numberOfSamples = samples.Length;
                this.sumOfWeights = 1.0;
                this.constant = 1.0 / (Constants.Sqrt2PI * this.smoothing * numberOfSamples);
            }


            this.mean = null;
            this.variance = null;
        }

       
        public override string  ToString(string format, IFormatProvider formatProvider)
        {
            return String.Format(formatProvider, "Fn(x; S)");
        }


      
        public static double SmoothingRule(double[] observations)
        {
            double sigma = Measures.StandardDeviation(observations);
            return sigma * Math.Pow(4.0 / (3.0 * observations.Length), 1.0 / 5.0);
        }

       
        public static double SmoothingRule(double[] observations, double[] weights)
        {
            double N = weights.Sum();
            double sigma = Measures.WeightedStandardDeviation(observations, weights);
            return sigma * Math.Pow(4.0 / (3.0 * N), 1.0 / 5.0);
        }

       
        public static double SmoothingRule(double[] observations, int[] repeats)
        {
            double N = repeats.Sum();
            double sigma = Measures.WeightedStandardDeviation(observations, repeats);
            return sigma * Math.Pow(4.0 / (3.0 * N), 1.0 / 5.0);
        }

       
        public static double SmoothingRule(double[] observations, double[] weights, int[] repeats)
        {
            if (weights != null)
            {
                if (repeats != null)
                    throw new ArgumentException("Either weights or repeats can be different from null.");

                return SmoothingRule(observations, weights);
            }

            if (repeats != null)
            {
                if (weights != null)
                    throw new ArgumentException("Either weights or repeats can be different from null.");

                return SmoothingRule(observations, repeats);
            }

            return SmoothingRule(observations);
        }

        
        public double[] Generate1(int samples, double[] result)
        {
            var generator = Accord.Math.Random.Generator.Random;

            if (weights == null)
            {
                for (int i = 0; i < samples; i++)
                    result[i] = this.samples[generator.Next(this.samples.Length)];
                return result;
            }

            double u = generator.NextDouble();
            double uniform = u * sumOfWeights;

            for (int i = 0; i < samples; i++)
            {
                double cumulativeSum = 0;
                for (int j = 0; j < weights.Length; j++)
                {
                    cumulativeSum += weights[j];

                    if (uniform < cumulativeSum)
                    {
                        result[i] = this.samples[j];
                        break;
                    }
                }
            }

            return result;
        }

      
        public double Generate1()
        {
            var generator = Accord.Math.Random.Generator.Random;

            if (weights == null)
                return this.samples[generator.Next(this.samples.Length)];


            double u = generator.NextDouble();
            double uniform = u * sumOfWeights;

            double cumulativeSum = 0;
            for (int i = 0; i < weights.Length; i++)
            {
                cumulativeSum += weights[i];

                if (uniform < cumulativeSum)
                    return this.samples[i];
            }

            throw new InvalidOperationException("Execution should never reach here.");
        }
    }
}
