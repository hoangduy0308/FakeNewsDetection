

namespace Accord.Statistics.Models.Regression
{
    using System;
    using Accord.Math;
    using Accord.Statistics.Testing;
    using Accord.MachineLearning;
    using System.Runtime.Serialization;
    using Accord.Statistics.Models.Regression.Fitting;
    using Accord.Math.Optimization;
    using Accord.Compat;

    
    [Serializable]
    public class LogisticRegression1 : MulticlassLikelihoodClassifierBase<double[]>,
        ICloneable
    {

#pragma warning disable 0649
        [Obsolete]
        private int inputs;
        [Obsolete]
        private int categories;
#pragma warning restore 0649

        private double[][] coefficients;
        private double[][] standardErrors;



      
        public LogisticRegression1(int inputs, int categories)
        {
            if (inputs < 1)
                throw new ArgumentOutOfRangeException("inputs");
            if (categories < 1)
                throw new ArgumentOutOfRangeException("categories");

            this.NumberOfOutputs = categories;
            this.NumberOfClasses = categories;
            this.NumberOfInputs = inputs;
            this.coefficients = new double[categories - 1][];
            this.standardErrors = new double[categories - 1][];

            for (int i = 0; i < coefficients.Length; i++)
            {
                coefficients[i] = new double[inputs + 1];
                standardErrors[i] = new double[inputs + 1];
            }
        }

       
        public LogisticRegression1(int inputs, int categories, double[] intercepts)
            : this(inputs, categories)
        {
            for (int i = 0; i < coefficients.Length; i++)
                coefficients[i][0] = intercepts[i];
        }


        public double[][] Coefficients
        {
            get { return coefficients; }
            set { coefficients = value; }
        }

        
        public int NumberOfParameters
        {
            get { return coefficients.GetTotalLength(); }
        }

        
        public double[][] StandardErrors
        {
            get { return standardErrors; }
        }

       
        [Obsolete("Please use NumberOfOutputs instead.")]
        public int Categories
        {
            get { return NumberOfOutputs; }
        }

        [Obsolete("Please use NumberOfInputs instead.")]
        public int Inputs
        {
            get { return NumberOfInputs; }
        }



        
        [Obsolete("Please use Probabilities() instead.")]
        public double[] Compute(double[] input)
        {
            return Probabilities(input);
        }

        
        [Obsolete("Please use Probabilities() instead.")]
        public double[][] Compute(double[][] input)
        {
            return Probabilities(input);
        }

       
        public override double LogLikelihood(double[] input, int classIndex)
        {
            if (classIndex == 0)
                return 0;

            // Get category coefficients
            double[] c = coefficients[classIndex - 1];

            double logit = c[0]; // intercept
            for (int i = 0; i < input.Length; i++)
                logit += c[i + 1] * input[i];

            return logit;
        }

      
        public override double[] LogLikelihoods(double[] input, out int decision, double[] result)
        {
            for (int j = 1; j < coefficients.Length; j++)
            {
                // Get category coefficients
                double[] c = coefficients[j - 1];

                double logit = c[0]; // intercept
                for (int i = 0; i < input.Length; i++)
                    logit += c[i + 1] * input[i];

                result[j] = logit;
            }

            decision = result.ArgMax();

            return result;
        }

       
        public ChiSquareTest ChiSquare(double[][] input, double[][] output)
        {
            double[] sums = output.Sum(0);

            double[] intercept = new double[NumberOfOutputs - 1];
            for (int i = 0; i < intercept.Length; i++)
                intercept[i] = Math.Log(sums[i + 1] / sums[0]);

            var regression = new LogisticRegression1(NumberOfInputs, NumberOfOutputs, intercept);

            double ratio = GetLogLikelihoodRatio(input, output, regression);

            return new ChiSquareTest(ratio, (NumberOfInputs) * (NumberOfOutputs - 1));
        }

       
        public ChiSquareTest ChiSquare(double[][] input, int[] classes)
        {
            return ChiSquare(input, Jagged.OneHot(classes));
        }

       
        public DoubleRange GetConfidenceInterval(int category, int coefficient)
        {
            double coeff = coefficients[category][coefficient];
            double error = standardErrors[category][coefficient];

            double upper = coeff + 1.9599 * error;
            double lower = coeff - 1.9599 * error;

            DoubleRange ci = new DoubleRange(Math.Exp(lower), Math.Exp(upper));

            return ci;
        }

       
        public DoubleRange[] GetConfidenceInterval(int category)
        {
            var ranges = new DoubleRange[NumberOfInputs + 1];
            for (int i = 0; i < ranges.Length; i++)
                ranges[i] = GetConfidenceInterval(category, i);
            return ranges;
        }

        
       
        public double GetOddsRatio(int category, int coefficient)
        {
            return Math.Exp(coefficients[category][coefficient]);
        }

       
        public double[] GetOddsRatio(int category)
        {
            var odds = new double[NumberOfInputs + 1];
            for (int i = 0; i < odds.Length; i++)
                odds[i] = GetOddsRatio(category, i);
            return odds;
        }

        
        public WaldTest GetWaldTest(int category, int coefficient)
        {
            return new WaldTest(coefficients[category][coefficient], 0.0, standardErrors[category][coefficient]);
        }

        
        public WaldTest[] GetWaldTest(int category)
        {
            var tests = new WaldTest[NumberOfInputs + 1];
            for (int i = 0; i < tests.Length; i++)
                tests[i] = GetWaldTest(category, i);
            return tests;
        }

        
        public double GetDeviance(double[][] input, double[][] output)
        {
            return -2.0 * GetLogLikelihood(input, output);
        }

        
        public double GetDeviance(double[][] inputs, int[] classes)
        {
            return -2.0 * GetLogLikelihood(inputs, classes);
        }

        
        public double GetLogLikelihood(double[][] inputs, int[] classes)
        {
            return GetLogLikelihood(inputs, Jagged.OneHot(classes));
        }

        public void getdata()
        {

        }
        public double GetLogLikelihood(double[][] input, double[][] output)
        {
            double sum = 0;

            for (int i = 0; i < input.Length; i++)
            {
                double[] y = Probabilities(input[i]);
                double[] o = output[i];
                y = y.Multiply(o.Sum());

                for (int j = 0; j < y.Length; j++)
                {
                    if (o[j] > 0)
                        sum += o[j] * (Math.Log(y[j] / o[j]));

                    Accord.Diagnostics.Debug.Assert(!Double.IsNaN(sum));
                }
            }

            return sum;
        }

       
        public double GetLogLikelihoodRatio(double[][] input, double[][] output, LogisticRegression1 regression)
        {
            return 2.0 * (this.GetLogLikelihood(input, output) - regression.GetLogLikelihood(input, output));
        }


        [OnDeserialized]
        private void onDeserialized(StreamingContext context)
        {
            if (NumberOfInputs == 0 && NumberOfOutputs == 0)
            {
#pragma warning disable 0618, 0612
                NumberOfOutputs = inputs;
                NumberOfOutputs = categories;
#pragma warning restore 0618, 0612
            }
        }

        #region ICloneable Members


        public object Clone()
        {
            var mlr = new LogisticRegression1(NumberOfInputs, NumberOfOutputs);
            for (int i = 0; i < coefficients.Length; i++)
            {
                for (int j = 0; j < coefficients[i].Length; j++)
                {
                    mlr.coefficients[i][j] = coefficients[i][j];
                    mlr.standardErrors[i][j] = standardErrors[i][j];
                }
            }

            return mlr;
        }

        #endregion

    }
}
