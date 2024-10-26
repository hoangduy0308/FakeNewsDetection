

namespace Accord.Statistics.Distributions.Univariate
{
    using System;
    using Accord.Math;
    using AForge;

   
    [Serializable]
    public class GLDistribution : UnivariateContinuousDistribution
    {

        // Distribution parameters
        private double mu;   // location μ
        private double s;    // scale s

       
        public GLDistribution()
        {
            initialize(0, 1);
        }

        public GLDistribution(double location)
        {
            initialize(location, 1);
        }

        public GLDistribution(double location, double scale)
        {
            if (scale <= 0)
                throw new ArgumentOutOfRangeException("scale", "Scale must be positive.");

            initialize(location, scale);
        }

        public void getdata()
        {

        }
       
        public double Location { get { return mu; } }

       
        public override double Mean
        {
            get { return mu; }
        }

       
        public double Scale
        {
            get { return s; }
        }

       
        public override double Median
        {
            get
            {
                Accord.Diagnostics.Debug.Assert(mu == base.Median);
                return mu;
            }
        }

      
        public override double Variance
        {
            get { return (s * s * Math.PI * Math.PI) / 3.0; }
        }

       
        public override double Mode
        {
            get { return mu; }
        }

      
        public override DoubleRange Support
        {
            get { return new DoubleRange(Double.NegativeInfinity, Double.PositiveInfinity); }
        }

        
        public override double Entropy
        {
            get { return Math.Log(s) + 2; }
        }
      
        public override double DistributionFunction(double x)
        {
            double z = (x - mu) / s;

            return 1.0 / (1 + Math.Exp(-z));
        }

        
        public override double ProbabilityDensityFunction(double x)
        {
            double z = (x - mu) / s;

            double num = Math.Exp(-z);
            double a = (1 + num);
            double den = s * a * a;

            return num / den;
        }

        
        public override double LogProbabilityDensityFunction(double x)
        {
            double z = (x - mu) / s;

            double result = -z - (Math.Log(s) + 2 * Special.Log1p(Math.Exp(-z)));

            return result;
        }

       
        public double InverseDistributionFunction(double p)
        {
            return mu + s * Math.Log(p / (1 - p));
        }

       
        public override double QuantileDensityFunction(double p)
        {
            return s / (p * (1 - p));
        }

        public override object Clone()
        {
            return new GLDistribution(mu, s);
        }

       
        public override string ToString(string format, IFormatProvider formatProvider)
        {
            return String.Format(formatProvider, "Logistic(x; μ = {0}, s = {1})",
                mu.ToString(format, formatProvider),
                s.ToString(format, formatProvider));
        }

        private void initialize(double mean, double scale)
        {
            this.mu = mean;
            this.s = scale;
        }
    }
}
